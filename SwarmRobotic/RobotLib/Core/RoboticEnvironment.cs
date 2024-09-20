using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using ParallelTest;
using RobotLib.Environment;
using RobotLib.Obstacles;

//批处理
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using RobotLib.FitnessProblem;

namespace RobotLib
{
    /// <summary>
    /// 类1：RoboticEnviroment
    /// 公共方法：构造器（环境生成）、用问题与算法初始化环境、创建簇（障碍物、障碍物阵列）
    /// 公共属性：噪声生成器、噪声百分比、可并行线程数
    /// Class represents the environment of the robotic simulation.
    /// <see cref="RoboticEnvironment"/> stores the robotics and the map information and provides a global control of
    /// the update process. External program can call <see cref="RoboticEnvironment.Update"/> method to update the whole swarm.
    /// </summary>
    /// <remarks></remarks>
	public abstract class RoboticEnvironment : IParameter
	{
        //机器人簇、障碍物簇列表、多障碍物簇列表、运行状态、问题
		public RobotCluster RobotCluster;
		public List<ObstacleCluster> ObstacleClusters;
		public List<MultiObstacleCluster> MultiObstacleClusters;
		public RunState runstate;
        bool lastFin;
        protected RoboticProblem problem;
		Stopwatch watch;

		public event Action<RobotBase, Obstacle, RunState> ObstacleCollison;
		public event Action<RobotBase, MultiObstacle, RunState> MultiObstacleCollison;
		public event Action<RobotBase, RobotBase, RunState> RobotCollison;
        public event Action<RoboticEnvironment> PreUpdate, PostUpdate, OnFinish, OnReset, OnInitialize, OnUpdateNeighbour;
        //public event Action<RoboticEnvironment> Reset;  //Initialize

		public NoiseGenerator NoiseGenerator { get; private set; }

        //public MapProvider mapProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboticEnvironment"/> class.
		/// </summary>
		/// <param name="neighbourhoodGenerator">The neighbourhood generator.</param>
		/// <remarks></remarks>
		public RoboticEnvironment()
			: base()
		{
			watch = new Stopwatch();
            RobotCluster = null;
            ObstacleClusters = new List<ObstacleCluster>();
			MultiObstacleClusters = new List<MultiObstacleCluster>();
			CreateDefaultParameter();

		}

		/// <summary>
		/// Initializes this instance. //Allocate memory space for <see cref="neighbourhoodMap"/> and <see cref="isNeighbour"/>
		/// and initializes <see cref="neighbourhoodGenerator"/>.
		/// Called after RoboticAlgorithm.Intialize(RoboticEnvironment)
        /// 设置问题与机器人簇的算法，将三种簇信息绑定到机器人个体
		/// </summary>
		/// <remarks></remarks>
        public void Initialize(RoboticProblem problem, RoboticAlgorithm algorithm)
		{
            
			this.problem = problem;
            //先创建问题、算法对象，然后创建环境对象（会调用下方的CreateClusters（）方法）
			problem.CreateEnvironment(this);
			RobotCluster.algorithm = algorithm;
            runstate.AliveRobots = problem.Population;
            lastFin = false;
            //RobotCluster = new RobotCluster(problem.RoboticSenseRange, problem.Population, problem.CreateRobot(runstate), NoiseGenerator);
			foreach (var r in RobotCluster.robots)
			{
				r.Bind(RobotCluster.isNeighbour[r.id], ObstacleClusters, MultiObstacleClusters);
				r.AlgorithmData = algorithm.CreateCustomData();
			}
            problem.ArrangeRobotic(RobotCluster.robots);

            runstate.SingleNum = problem.Population % 3;

            if (OnInitialize != null) OnInitialize(this);
            UpdateNeighbour();

            //设置数据采集条件
            Random rand = new Random();
            runstate.IterationNum = (int)Math.Floor(200 * rand.NextDouble());
            runstate.RobotID = (int)Math.Floor(50 * rand.NextDouble());
            runstate.Finished = false;

            //设置batch初始状态
            runstate.BatchFlag = false;
		}

        //创建三个Cluster的函数，若Initialize能够完成环境构建，则problem.CreateEnvironment必定要调用该函数
		public void CreateClusters(RoboticProblem problem, int ObstacleCluesterNumber, params string[] @class)
		{
			RobotCluster = new RobotCluster(problem.RoboticSenseRange, problem.Population, problem.CreateRobot(this), NoiseGenerator);
			int i = 0;
            //在列表对象中添加簇对象，仍未创建最底层的“一般障碍物”对象
			for (; i < ObstacleCluesterNumber; i++)
				ObstacleClusters.Add(new ObstacleCluster(@class[i], problem.Population, NoiseGenerator));
            //若字符串数组尺寸大于障碍物簇的个数，则创建障碍物阵列
			for (; i < @class.Length; i++)
				MultiObstacleClusters.Add(new MultiObstacleCluster(@class[i], problem.Population, NoiseGenerator));
		}

		public void TestNeighbour()
		{
            int Population = problem.Population;
            int ObsNum = 0;
            foreach (var oc in ObstacleClusters)
                ObsNum += oc.Size;
            bool[] rN = new bool[Population], rM = new bool[ObsNum];
			//Console.WriteLine(algorithm.iterations + "Neighbour:");
			for (int i = 0; i < Population; i++)
			{
				Array.Clear(rN, 0, Population);
				foreach (var item in RobotCluster.robots[i].Neighbours)
					rN[item.Target.id] = true;
				for (int j = 0; j < Population; j++)
				{
					//Console.Write(isNeighbour[i][j] ? 1 : 0);
					//Console.Write("/");
					//Console.Write((isNeighbour[i][j] == rN[j]) ? 1 : 0);
					//Console.Write(" ");
                    if (RobotCluster.isNeighbour[i][j].isNeighbour != rN[j])
                        Console.WriteLine("Robo({2})({3}): {0}/{1}", RobotCluster.isNeighbour[i][j].isNeighbour, rN[j], i, j);
				}
				//Console.WriteLine();
			}
			//Console.WriteLine(algorithm.iterations + "Obstacle:");
			for (int i = 0; i < Population; i++)
			{
                Array.Clear(rM, 0, ObsNum);
                foreach (var item in RobotCluster.robots[i].mapsensor.SelectMany(ino => ino))
                    rM[item.Target.id] = true;
				foreach (var item in ObstacleClusters)
				{
					//Console.Write(de.neighbourhoodMap[i][item.id] ? 1 : 0);
					//Console.Write("/");
					//Console.Write((de.neighbourhoodMap[i][item.id] == rM[item.id]) ? 1 : 0);
					//Console.Write(" ");
                    foreach (var o in item.obstacles)
                    {
                        if (item.isNeighbour[i][o.id].isNeighbour != rM[o.id])
                            Console.WriteLine("Robo({2})-Obs({3}): {0}/{1}", item.isNeighbour[i][o.id].isNeighbour, rM[o.id], i, o.id);
                    }
				}
				//for (int j = 0; j < ObstacleNum; j++)
				//{
				//    Console.Write(neighbourhoodMap[i][j] ? 1 : 0);
				//    Console.Write("/");
				//    Console.Write((neighbourhoodMap[i][j] == rM[j]) ? 1 : 0);
				//    Console.Write(" ");
				//}
				//Console.WriteLine();
			}
		}

		void TranverseParallel(Action<RobotBase> action)
		{
            runstate.Command = "";
            string script = "C:/myInstall/Anaconda/python.exe C:/Users/Jie/gbdtProcess.py ";

			if (parallel == 1)
				foreach (var r in RobotCluster.robots)
				{
					if (r.Broken) continue;
					action(r);
				}
			else
			{
				var test = new ParallelTest<RobotBase>(UpdateRobot);
				test.Items = RobotCluster.robots.Where(r => !r.Broken); ;
				test.Start(parallel);
				test.GoTask.Wait();
			}

            //batch
            if (runstate.BatchFlag)
            {
                StreamWriter sw = new StreamWriter("C:/Users/Jie/inputdata.txt");
                sw.Write(runstate.Command);
                sw.Close();

                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
               
                p.StandardInput.WriteLine(script + "&exit");
                p.StandardInput.AutoFlush = true;

                string outputInfo = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();

                int head = outputInfo.IndexOf("]");
                string tail = outputInfo.Substring(head + 1);

                int start0 = tail.IndexOf("[");
                int end0 = tail.IndexOf("]");
                string result0 = tail.Substring(start0 + 1, end0 - start0 - 1);
                string[] states = result0.Trim().Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ');

                int start = tail.LastIndexOf("[");
                int end = tail.LastIndexOf("]");
                string result = tail.Substring(start + 1, end - start - 1);
                string[] angles = result.Trim().Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ');

                int i = 0;
                foreach (var r in RobotCluster.robots)
                {
                    if (!r.batchObject)
                    {
                        var robot = r as RFitness;

                        double state = double.Parse(states[i]);
                        if (state < 0.5) robot.state.NewData = "Run";
                        else robot.state.NewData = "Diffusion";

                        double angle = double.Parse(angles[i]);
                        float maxspeed = 5;
                        Vector3 delta = Vector3.Zero;
                        angle = angle * 2 * Math.PI - Math.PI;
                        delta.X = (float)(maxspeed * Math.Cos(angle));
                        delta.Y = (float)(maxspeed * Math.Sin(angle));
                        LastProcess(robot, ref delta);

                        i++;

                        //double moveX = robot.postionsystem.LastMove.X;
                        //double moveY = robot.postionsystem.LastMove.Y;
                        //double originalAngle = Math.Acos(moveX / Math.Sqrt(moveX * moveX + moveY * moveY));
                        //if (moveY < 0) originalAngle = -originalAngle;
                        ////角度归一化到0到1
                        //originalAngle = (originalAngle + Math.PI) * 0.5 / Math.PI;
                        ////runstate.Iterations < 1 || 
                        //if (runstate.Iterations < 1 || Math.Abs(angle - originalAngle) < 0.1 && (robot.state.SensorData == "Diffusion" || robot.Fitness.SensorData > 0)) robot.state.NewData = "Diffusion";
                        //else robot.state.NewData = "Run";
                    }
                }

            }
		}

        protected void LastProcess(RFitness r, ref Vector3 delta)
        {

            if (r.state.SensorData == r.state.NewData) r.NumOfState++;
            else r.NumOfState = 0;

            //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
            //Bounding(r.postionsystem.GlobalSensorData, ref delta);
            r.postionsystem.GlobalSensorData += delta;
            if (r.postionsystem.GlobalSensorData.X < 0) delta.X -= r.postionsystem.GlobalSensorData.X * 2;
            if (r.postionsystem.GlobalSensorData.X > problem.MapSize.X) delta.X += (problem.MapSize.X - r.postionsystem.GlobalSensorData.X) * 2;
            if (r.postionsystem.GlobalSensorData.Y < 0) delta.Y -= r.postionsystem.GlobalSensorData.Y * 2;
            if (r.postionsystem.GlobalSensorData.Y > problem.MapSize.Y) delta.Y += (problem.MapSize.Y - r.postionsystem.GlobalSensorData.Y) * 2;
            if (r.postionsystem.GlobalSensorData.Z < 0) delta.Z -= r.postionsystem.GlobalSensorData.Z * 2;
            if (r.postionsystem.GlobalSensorData.Z > problem.MapSize.Z) delta.Z += (problem.MapSize.Z - r.postionsystem.GlobalSensorData.Z) * 2;

            //存储现在的位置与适应度，用速度向量保存到NewData中
            //AddHistory(r);
            r.History.Add(r.postionsystem.GlobalSensorData, r.Fitness.SensorData); 

            r.postionsystem.NewData = delta;
        }



		void UpdateRobot(RobotBase r) { RobotCluster.algorithm.Update(r, runstate); }

		/// <summary>
		/// This method updates the whole swarm. External programs need only to call this method to update the swarm.
		/// This method calls <see cref="GenerateNeighbours"/> method to calculate the neighbourhood and
		/// environment information of the robotic swarm and after updating robotics' sensors, swarm status are updated.
		/// </summary>
		/// <remarks></remarks>
		public void Update()
		{
            //判断是否结束，并处理结束事件
            if (runstate.Finished)
            {
                if (lastFin) return;
                lastFin = true;
                if (OnFinish != null) OnFinish(this);
            }
            //更新前的事务处理
            if (PreUpdate != null) PreUpdate(this);
			foreach (var r in RobotCluster.robots)
			{
				if (r.Broken) continue;
				RobotCluster.algorithm.UpdateCustomData(r);
			}

			watch.Restart();
            //并行计算机器人的位移增量(可视化仿真过程只用一个线程)
            //Environment对象的创建时调用的“无参构造函数”中thread的创建为1，所以该方法永远是串行执行
			TranverseParallel(UpdateRobot);
			watch.Stop();
			runstate.Time += watch.ElapsedTicks;

            //利用位移增量更新机器人的位置
            foreach (var r in RobotCluster.robots)
			{
				if (r.Broken) continue;
				r.ApplyChanges();

                //串行“模拟”同步
                r.nextFlag = false;
			}
            //更新后的事务处理
            if (PostUpdate != null) PostUpdate(this);
            //计算下一次迭代所需要的邻居信息、适应度值与目标检测信息
			UpdateNeighbour();
            //碰撞检测，貌似多目标搜索中并未应用碰撞事件
			CollisonDetect();
            //Finished = algorithm.EndIteration(robotics) || AliveNumbers == 0;
			runstate.Iterations++;
            runstate.Iter2 = (float)(runstate.Iterations) * runstate.Iterations;
		}

		void UpdateNeighbour()
		{
            //清除个体，生成邻居
			GenerateNeighbours();
            //更新剩余个体的适应度值与目标检测信息
            foreach (var r in RobotCluster.robots)
			{
				if (r.Broken) continue;
				problem.UpdateSensor(r, runstate);
			}
            //发布事件
			if (OnUpdateNeighbour != null)
                OnUpdateNeighbour(this);
		}

		public virtual void Reset()
		{
			foreach (var r in RobotCluster.robots)
			{
				r.Broken = false;
                //失效的个体记录在算法数据中
				RobotCluster.algorithm.ClearCustomData(r.AlgorithmData);
			}

            runstate.Clear();
            runstate.AliveRobots = problem.Population;
            lastFin = false;
            problem.ResetEnvironment(this);
            problem.ArrangeRobotic(RobotCluster.robots);

            if (OnReset != null) OnReset(this);
			UpdateNeighbour();
		}

		/// <summary>
		/// Generate Neighbours Imformation, including robotics and obstacles.
		/// Call RoboticEnvironment.GenerateNeighbours to clear neighbour infos.
        /// 作为基类的功能，完成第一步的清零
		/// </summary>
		public virtual void GenerateNeighbours()
		{
            foreach (var r in RobotCluster.robots)
            {
                if (r.Broken) continue;
                Clear(r.id);
            }
			//for (int i = 0; i < Population; i++)
			//{
			//    Array.Clear(neighbourhoodMap[i], 0, ObstacleNum);
			//    Array.Clear(isNeighbour[i], 0, Population);
			//}
		}

        void Clear(int id)
        {
            RobotCluster.Clear(id);
            foreach (var oc in ObstacleClusters)
                oc.ClearRobot(id);
			foreach (var moc in MultiObstacleClusters)
				moc.ClearRobot(id);
        }

		public void CollisonDetect()
		{
            //BoundingBox box1 = new BoundingBox(), box2 = new BoundingBox();
            //Vector3 dis = Vector3.One, halfdis = dis / 2;
            ////Vector3 dis = Vector3.One * 2, halfdis = Vector3.One;
            //RobotBase r2;
            if (RobotCollison != null)
            {
                foreach (var rc in RobotCluster.robots)
                {
                    if (rc.Broken) continue;
                    //考察机器人rc邻居列表中位于rc之后的个体（跳过id+1个体，并返回剩余的元素）
                    foreach (var r in RobotCluster.isNeighbour[rc.id].Skip(rc.id + 1))
                    {
                        //碰撞检测距离为1
                        if (r.isNeighbour && r.realdistance < 1)
                        {
                            //发布碰撞事件，r.Target即另一个机器人的数据RobotBase
                            RobotCollison(rc, r.Target, runstate);
                            if (rc.Broken)
                            {
                                Clear(rc.id);
                                runstate.RoboCollison++;
								runstate.AliveRobots--;
                            }
                            if (r.Target.Broken)
                            {
                                Clear(r.Target.id);
                                runstate.RoboCollison++;
								runstate.AliveRobots--;
							}
                        }
                    }
                }
            }
            if (ObstacleCollison != null)
            {
                foreach (var r in RobotCluster.robots)
                {
                    if (r.Broken) continue;
                    foreach (var cluster in ObstacleClusters)
                    {
                        foreach (var o in cluster.isNeighbour[r.id])
                        {
                            if (o.isNeighbour && o.realdistance < 1)
                            {
                                //发布碰撞事件
                                ObstacleCollison(r, o.Target, runstate);
                                if (r.Broken)
                                {
                                    Clear(r.id);
                                    runstate.ObsCollison++;
									runstate.AliveRobots--;
								}
                                //若障碍物不可见，则从簇中清除该障碍物对所有机器人的影响
                                if (!o.Target.Visible) cluster.ClearObstacle(o.Target.id);
                            }
                        }
                    }
                }
            }
			if (MultiObstacleCollison != null)
			{
				foreach (var r in RobotCluster.robots)
				{
					if (r.Broken) continue;
					foreach (var cluster in MultiObstacleClusters)
					{
						foreach (var mo in cluster.isNeighbour[r.id])
						{
							var target = cluster.obstacles[mo.Key];
							foreach (var o in mo)
							{
								if (o.isNeighbour && o.realdistance < 1)
								{
									MultiObstacleCollison(r, target, runstate);
									if (r.Broken)
									{
										Clear(r.id);
										runstate.ObsCollison++;
										runstate.AliveRobots--;
									}
									if (!target.Visible) cluster.ClearObstacle(mo.Key);
								}
							}
						}
					}
				}
			}
		}

		protected void CheckNeighbour(int index1, int index2, Vector3 v1, Vector3 v2)
		{
            var v = v2 - v1;
			float dis = v.Length();
			//float dis = Distance(v1, v2);
			if (dis <= RobotCluster.SenseRange)
			{
                RobotCluster.isNeighbour[index1][index2].Set(dis, v);
                RobotCluster.isNeighbour[index2][index1].Set(dis, -v);
			}
		}

		protected void CheckObstacle(int index, Vector3 v, int cluster, Obstacle o)
		{
            var v0 = o.Position - v;
            float dis = v0.Length();
            if (dis <= o.SenseRange) ObstacleClusters[cluster].isNeighbour[index][o.id].Set(dis, v0);
		}

        protected void CheckObstacle(int index, Vector3 v, ObstacleCluster cluster, Obstacle o)
        {
			var v0 = o.Position - v;
			float dis = v0.Length();
			if (dis <= o.SenseRange) cluster.isNeighbour[index][o.id].Set(dis, v0);
        }

		protected void CheckMultiObstacle(int index, Vector3 v, int cluster, Obstacle o)
		{
			var v0 = o.Position - v;
			float dis = v0.Length();
			if (dis <= o.SenseRange) MultiObstacleClusters[cluster].GetData(index, o.id).Set(dis, v0);
		}

		protected void CheckMultiObstacle(int index, Vector3 v, MultiObstacleCluster cluster, Obstacle o)
		{
			var v0 = o.Position - v;
			float dis = v0.Length();
			if (dis <= o.SenseRange) cluster.GetData(index, o.id).Set(dis, v0);
		}

		public virtual void CreateDefaultParameter()
		{
			noise = 0;
			parallel = 1;
		}

		public virtual void InitializeParameter()
		{
			NoiseGenerator = new NoiseGenerator(new CustomRandom(), noise);
			ObstacleClusters.Clear();
			MultiObstacleClusters.Clear();
			ObstacleCollison = null;
			MultiObstacleCollison = null;
			RobotCollison = null;
			PreUpdate = null;
			PostUpdate = null;
			OnFinish = null;
			OnReset = null;
			OnInitialize = null;
			OnUpdateNeighbour = null;
			ParallelTest.ParallelTests.Threads = parallel;
		}

		float noise;
		int parallel;

		[Parameter(ParameterType.Float, Description = "Sensing Noise (Percent)")]
		public float NoisePercent
		{
			get { return noise; }
			set
			{
				if (value < 0 || value > 1) throw new Exception("Must be in [0, 1]");
				noise = value;
			}
		}
		
		[Parameter(ParameterType.Int, Description = "Parallel Threads")]
		public int Parallel
		{
			get { return parallel; }
			set
			{
				if (value < 1) throw new Exception("Must be at least 1");
				parallel = value;
			}
		}
	}
}
