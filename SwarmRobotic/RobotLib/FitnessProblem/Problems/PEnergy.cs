using System;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;
using System.Collections.Generic;
using System.Linq;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承PFitness（ProblemFitness），用于能量限制问题，机器人状态列表：运行、充电、能量耗尽
    /// 问题属性：目标能量（总能量、最小能量、最大能量），机器人能量（初始能量、最大能量、能量模式）
    /// </summary>
	public class PEnergy : PFitness
	{
        
		//public PEnergy() { statelist = new string[] { "Run", "Charging", "OutofPower" }; }
        public PEnergy() { statelist = new string[] { "Run", "Charging", "OutofPower","Decoy" ,"Diffusion" }; }

		public override RobotBase CreateRobot(RoboticEnvironment env) { return new REnergy(HistorySize); }

        //安排去群体的初始能量与位置
        public override void ArrangeRobotic(List<RobotBase> robotics)
        {
			float v = EnergyMode ? iREnergy : 0;
			foreach (REnergy r in robotics)
				r.Energy = v;
            base.ArrangeRobotic(robotics);
        }

        //创建环境
        public override void CreateEnvironment(RoboticEnvironment env)
        {
            //创建整体状态与三种簇
            var state = new SEnergy();
            env.runstate = state;
			env.CreateClusters(this, 2, "Obstacle", "Target");
            //创建目标并加入障碍物簇列表（类型编号为1的障碍物）
            var targets = new EnergyTarget[TargetNum];
			for (int i = 0; i < TargetNum; i++)
                targets[i] = new EnergyTarget(GenerateObstaclePos());
			env.ObstacleClusters[1].AddObstacle(targets);
            //注册障碍物碰撞处理函数，为每个目标分配初始能量
            env.ObstacleCollison += ObstacleCollison;
            GenerateEnergy(env.ObstacleClusters[1].obstacles, state);
            //注册更新函数与解析函数，根据能量模式（有能量限制or无能量限制）来设置更新函数（消耗的总能量or剩余的总能量）
			state.EnergyMode = EnergyMode;
			env.PostUpdate += EnergyMode ? (Action<RoboticEnvironment>)env_PostUpdateEnergy : env_PostUpdate;
			OnFinalize += FinishTest;

			base.CreateEnvironment(env);
		}
        //重新生成第1类型的障碍物并设置状态值（能量限制问题中第1类型的障碍物为能量目标）
        public override void ResetEnvironment(RoboticEnvironment env)
        {
            base.ResetEnvironment(env);
			var list = env.ObstacleClusters[1].obstacles;
			foreach (var item in list)
            {
                item.Position = GenerateObstaclePos();
                item.Visible = true;
            }
			GenerateEnergy(list, env.runstate as SEnergy);
        }

        //为每个目标分配能量（分配能量自动分配其影响范围）
		void GenerateEnergy(List<Obstacle> targets, SEnergy state)
		{
            //free：可拥有最大能量的目标的数量（可以是小数）
			float band = maxTEnergy - minTEnergy, free = (sumEnergy - TargetNum * minTEnergy) / band, energy, sum = 0;
			int remain = TargetNum;
			for (int i = TargetNum - 1; i > 0; i--)
			{
				if (free < 1)
					energy = (float)Random.NextDouble() * free;
				else if (free > i)
				{
                    //不能满足的数量
					energy = free - i;
					energy = (float)Random.NextDouble() * (1 - energy) + energy;
				}
				else
					energy = (float)Random.NextDouble();
                //energy表示为每个目标分配的额外能量（单位为band），分配方式如下：
                //若剩余能量free不足1则剩余的每个目标分配的能量为free*rand(0,1)
                //若剩余能量不能满足每个为1，则为该目标分配rand(0,1)
                //若剩余能量能满足每个为1，则为该目标分配rand(0,1)+(1-rand(0,1))*(free-i)
                //当free<1后，每次分出的总是属于(0,free)，所以不必担心没有能量剩余
				free -= energy;
                //为每个目标分配能量并统计所有目标的总能量
				(targets[i] as EnergyTarget).Energy = energy * band + minTEnergy;
				sum += (targets[i] as EnergyTarget).Energy;
			}
            //将最终剩余的能量赋给0目标（必然>=minTEnergy）
			(targets[0] as EnergyTarget).Energy = sumEnergy - sum;

			state.RobotEnergy = EnergyMode ? Population * iREnergy : 0;
			//state.AliveRobots = Population;
            //state.CollectedTargets = 0;
            state.TargetEnergy = sumEnergy;
			//state.CollectEnergy = 0;
        }

        //更新机器人的适应度值传感器
		public override void UpdateSensor(RobotBase robot, RunState state)
		{
			var r = robot as RFitness;
            //若未发现目标或目标的能量收集完成
			if (r.Target == null || (r.Target as EnergyTarget).Energy == 0)
			{
				int value = 0, fit;
				float lastdis = float.MaxValue;
				r.Target = null;
                //考察所有第1类型障碍物（目标），查找最大适应度值并用以更新NewData，若在多个目标内则记录距离最近的目标
				foreach (var target in r.mapsensor[1])
				{
					fit = 5 - (int)(target.distance * 5 / target.Target.SenseRange);
					if (fit > value)
					{
						value = fit;
						if (value == 5)
						{
							lastdis = target.distance;
							r.Target = target.Target;
						}
					}//若多个目标可以近似在同一位置（求fit时用了int的强制类型转换）
					else if (fit == 5 && lastdis > target.distance)
					{
						lastdis = target.distance;
						r.Target = target.Target;
					}
				}
				//if (value > 0 && value != 5 && Noise.NoisePercent != 0)
				//{
				//    lastdis = Noise.Random.NextGaussianNoise();
				//    if (lastdis < 0 && lastdis > -Noise.NoisePercent)
				//        value--;
				//    else if (lastdis > 0 && lastdis < Noise.NoisePercent && value < 4)
				//        value++;
				//}
				r.Fitness.NewData = value;
			}
			else
				r.Fitness.NewData = 0;
            //用NewData更新SensorData
			r.Fitness.ApplyChange();

			//History
			//foreach (var item in History)
			//    item.time++;
		}

        //目标收集：仅仅是向目标移动，并没有能量减弱
		public override bool CollectTarget(RFitness robot, SFitness state)
		{
			if (robot.Target == null) return false;
			if (robot.Target.Visible)
			{
				var delta = robot.Target.Position - robot.postionsystem.GlobalSensorData;
				if (delta.Length() > MaxSpeed)
				{
					delta.Normalize();
					delta = delta * MaxSpeed;
				}
				//Move
				robot.postionsystem.NewData = delta;
				return true;
			}
			robot.Target = null;
			return false;
		}

        //非能量限制模式下用来更新消耗的总能量（正比于移动的距离）
        void env_PostUpdate(RoboticEnvironment env)
        {
            var state = env.runstate as SEnergy;
            foreach (REnergy r in env.RobotCluster.robots)
            {
                //Energy Cost
                var cost = 0.05f + r.postionsystem.LastMove.Length() / 10;
				r.Energy += cost;
				state.RobotEnergy += cost;
            }
			state.Finished = state.CollectedTargets == TargetNum;
        }
        //能量限制模式下用来更新剩余的总能量，移除耗尽能量的个体
		void env_PostUpdateEnergy(RoboticEnvironment env)
		{
			var state = env.runstate as SEnergy;
			foreach (REnergy r in env.RobotCluster.robots)
			{
				if (r.Energy == 0) continue;
				//Energy Cost
				var cost = 0.05f + r.postionsystem.LastMove.Length() / 10;
				if (r.Energy - cost < 0.05f)
				{
					state.RobotEnergy -= r.Energy;
					r.Energy = 0;
					r.state.NewData = statelist[2];
					state.AliveRobots--;
					r.Broken = true;
				}
				else
				{
					state.RobotEnergy -= cost;
					r.Energy -= cost;
				}
			}
			state.Finished = state.AliveRobots == 0 || state.CollectedTargets == TargetNum;
		}

        //求取目标的总能量
		void FinishTest(RunState state, RoboticEnvironment env)
		{
			var s = state as SEnergy;
			s.TargetEnergy = env.ObstacleClusters[1].obstacles.OfType<EnergyTarget>().Sum(e => e.Visible ? e.Energy : 0);
		}

        //void RoboticCollision(RobotBase r1, RobotBase r2)
        //{
            //REnergy re1 = r1 as REnergy, re2 = r2 as REnergy;
            //if (re1.state.SensorData != "OutofPower" || re2.state.SensorData != "OutofPower")
            //{
            //    re1.Energy = (re1.Energy + re2.Energy) / 2;
            //    re2.Energy = re1.Energy;
            //    re1.state.NewData = "Run";
            //    re1.state.ApplyChange();
            //    re2.state.NewData = "Run";
            //    re2.state.ApplyChange();
            //}
        //}


        //处理碰撞事件（机器人与目标or障碍物）
        void ObstacleCollison(RobotBase robot, Obstacle o, RunState runstate)
        {
            var r = robot as REnergy;
            var target = o as EnergyTarget;
            var state = runstate as SEnergy;
            //若碰撞的不是目标，则根据能量模式选择“减少剩余能量”or“增加损耗能量”，碰撞次数加1
            if (target == null)
            {
				if (EnergyMode)
				{
					if (r.Energy < 10.01f)
					{
						state.RobotEnergy -= r.Energy;
						r.Energy = 0;
						r.state.NewData = statelist[2];
						r.Broken = true;
						return;
					}
					else
					{
						r.Energy -= 10;
						state.RobotEnergy -= 10;
					}
				}
				else
				{
					r.Energy+=10;
					state.RobotEnergy += 10;
				}
				state.ObsCollison++;
			}//若正在收集目标
			else if (r.postionsystem.LastMove.Length() < 0.01f)
			{
				//float delta = maxREnergy - robot.Energy;
				//if (delta > 10) delta = 10;
                //若能量收集完毕则进入搜索状态
				if (target.Energy <= 0 || target.Visible == false)
				{
					r.state.NewData = statelist[0];
					return;
				}

                //计算能量增量
				float delta;
				if (EnergyMode)
				{
					delta = Math.Min(Math.Min(maxREnergy - r.Energy, 10f), target.Energy);
					r.Energy += delta;
					state.RobotEnergy += delta;
				}
				else
					delta = Math.Min(10f, target.Energy);
                //更新能量，进入充电状态
				target.Energy -= delta;
				state.TargetEnergy -= delta;
				state.CollectEnergy += delta;
				r.state.NewData = statelist[1];
                //若剩余能量低于0.01则进入搜索状态，收集目标数递增，记录收集完成的迭代次数
				if (target.Energy < 0.01f)
				{
					target.Visible = false;
					state.TargetEnergy -= target.Energy;
					target.Energy = 0;
					r.state.NewData = statelist[0];
					state.CollectedTargets++;
					state.LastCollect = state.Iterations;
					//state.CollectEnergy += target.Energy;
					//if (EnergyMode)
					//{
					//    r.Energy += target.Energy;
					//    state.RobotEnergy += target.Energy;
					//}
					//target.Energy = 0;
				}
			}
        }



		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			RoboticSenseRange = 50;	//20
			Population = 100;
			MaxSpeed = 1;
			SizeX = SizeY = 1000;// 800;
			TargetNum = 60;
			sumEnergy = 30000;
			minTEnergy = 200;
			maxTEnergy = 1000;
			iREnergy = 200;
			maxREnergy = 100000;
			EnergyMode = false;
		}

		float sumEnergy, minTEnergy, maxTEnergy, maxREnergy, iREnergy;

		[Parameter(ParameterType.Float, Description = "Target Total Energy")]
		public float TotalEnergy
		{
			get { return sumEnergy; }
			set
			{
				var v = value / TargetNum;
				if (v < minTEnergy) throw new Exception("Must be at least " + minTEnergy * TargetNum + " energy");
				if (v > maxTEnergy) throw new Exception("Must be no more than " + maxTEnergy * TargetNum + " energy");
				sumEnergy = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Min Target Energy")]
		public float MinTargetEnergy
		{
			get { return minTEnergy; }
			set
			{
				if (value < 0) throw new Exception("Must be positive");
				if (value > maxTEnergy) throw new Exception("Must be less than Max Target Energy");
				minTEnergy = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Max Target Energy")]
		public float MaxTargetEnergy
		{
			get { return maxTEnergy; }
			set
			{
				if (value < minTEnergy) throw new Exception("Must be more than Min Target Energy");
				maxTEnergy = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Initial Robotic Energy")]
		public float StartRobotEnergy
		{
			get { return iREnergy; }
			set
			{
				if (value < 0) throw new Exception("Must be positive");
				if (value > maxREnergy) throw new Exception("Must be no more than Max Robotic Energy");
				iREnergy = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Max Robotic Energy")]
		public float MaxRobotEnergy
		{
			get { return maxREnergy; }
			set
			{
				if (value < iREnergy) throw new Exception("Must be more than Initial Robotic Energy");
				maxREnergy = value;
			}
		}

		[Parameter(ParameterType.Boolean, Description = "Energy Restrict Mode")]
		public bool EnergyMode { get; set; }
    }
}
