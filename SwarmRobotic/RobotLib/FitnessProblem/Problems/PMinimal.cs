using System;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;
using System.Linq;
using System.Collections.Generic;

//Search 干扰级别别

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自SFitness（StateFitness），增加了假目标收集数（检测次数）CollectedDecoys
    /// </summary>
	[Serializable]
	public class SMinimal : SFitness
	{
		public int CollectedDecoys;

		public bool RequireUpdate { get; set; }

		public SMinimal() { }
	}
    /// <summary>
    /// 抽象类，继承自PFitness（ProblemFitness），用于多目标搜索问题
    /// 问题属性：假目标数量、假目标的可收集性、干扰源数量、目标的最大适应度值
    /// 多目标搜索的问题定义中，机器人的整体状态机包含四个状态：搜索、目标收集、目标离开（可看做假目标处理）、假目标处理
    /// “具体的”搜索算法负责“搜索状态”，特殊情况处理（如假目标处理）可进一步划分为子状态机
    /// 搜索算法“框架”见AFitness.cs
    /// </summary>
	public abstract class PMinimal : PFitness
	{
        //目标的最大适应度值、适应度环的宽度
		public const int FitnessLevel = 40, FitnessRadius = 5; //FitnessLevel 原为20

		public PMinimal() : base(FitnessProblemType.DecoyProblem) 
        { 
            //在不同的算法中，机器人可建模为不同的有限状态机，新的状态添加入列表即可
            statelist = new string[] { "Run", "Collecting", "Leaving", "Decoy" ,"Diffusion"};
        }

		public sealed override RobotBase CreateRobot(RoboticEnvironment env) { return new RFitness(HistorySize); }

        //整体状态添加了假目标的收集数目
		protected virtual RunState CreateState() { return new SMinimal(); }

        //创建环境（sealed密封以防止重写）：整体状态、三种簇
		public sealed override void CreateEnvironment(RoboticEnvironment env)
		{
            //创建整体状态
			int i;
			env.runstate = CreateState();

            //创建目标数组（真目标+假目标），尚不生成目标位置（初始位置都是Vector3.Zero）与适应度
			var targets = new FitnessTarget[TargetNum + decoyNum];
			for (i = 0; i < TargetNum; i++)
				targets[i] = new FitnessTarget(TargetSize);
			for (; i < targets.Length; i++)
				targets[i] = new FitnessTarget(real: false);

            //根据是否有干扰源来创建三种簇（机器人、一般障碍物、一般障碍物阵列），一般障碍物包括障碍物、真目标、假目标、干扰源
			if (interNum == 0)
			{
                //环境根据问题创建簇列表对象
				env.CreateClusters(this, 3, "Obstacle", "Target", "Decoy");
                //在簇列表对象的相应组对象（此时组对象GroupingList内仍无成员、但仍是List的一个派生类）中添加“一般障碍物”成员
				env.ObstacleClusters[1].AddObstacle(targets.Take(TargetNum));
				env.ObstacleClusters[2].AddObstacle(targets.Skip(TargetNum));
                //生成适应度地图对象（尚不构建地图）
				CreateEnvironment(env.runstate, targets, null);
			}
			else
			{
				var inters = new Interference[interNum];
				for (i = 0; i < interNum; i++)
					inters[i] = new Interference();
                //环境根据问题创建簇列表对象
				env.CreateClusters(this, 4, "Obstacle", "Target", "Decoy", "Interference");
				env.ObstacleClusters[1].AddObstacle(targets.Take(TargetNum));
				env.ObstacleClusters[2].AddObstacle(targets.Skip(TargetNum));
				env.ObstacleClusters[3].AddObstacle(inters);
				CreateEnvironment(env.runstate, targets, inters);
			}

            //生成真假目标（干扰源）的位置与适应度（干扰级别）
			SetTargets(env);

            //注册碰撞事件与更新事件的处理函数
			env.ObstacleCollison += ObstacleCollison;
			env.PostUpdate += new Action<RoboticEnvironment>(env_PostUpdate);
            //在簇列表对象的相应组对象添加“实际障碍物”成员
			base.CreateEnvironment(env);
		}

        //函数的具体是实现应该会调用适应度生成文件FitnessMap
		protected abstract void CreateEnvironment(RunState state, FitnessTarget[] targets, Interference[] interfereces);

        //重置环境：重置真目标的随机种子
		public sealed override void ResetEnvironment(RoboticEnvironment env)
		{
			base.ResetEnvironment(env);
			foreach (var tar in env.ObstacleClusters[1].obstacles)
				tar.Reset();
			SetTargets(env);
		}

        //生成真、假目标与干扰源的位置、适应度or干扰级别别
		protected virtual void SetTargets(RoboticEnvironment env)
		{
			foreach (FitnessTarget tar in env.ObstacleClusters[1].obstacles.Concat(env.ObstacleClusters[2].obstacles))
			{
				tar.Position = GenerateObstaclePos();
				tar.Energy = tar.Real ? Random.NextInt(maxTarFit - 2, maxTarFit + 1) : Random.NextInt(maxTarFit - 3, maxTarFit);
			}
			if (interNum > 0)
			{
				foreach (Interference inter in env.ObstacleClusters[3].obstacles)
				{
                    //干扰级别别
					inter.Position = GenerateObstaclePos();
					//inter.Level = Random.NextInt(5, 11);
                    //inter.Level = Random.NextInt(maxTarFit/2, maxTarFit);
                    inter.Level = maxTarFit;
				}
			}
		}

		//public override bool CollectTarget(RFitness robot, SFitness state)
		//{
		//    var s = state as SMinimal;
		//    if (robot.Target == null) return false;
		//    if (robot.Target.Visible)
		//    {
		//        var Tar = robot.Target as FitnessTarget;
		//        robot.state.NewData = statelist[1];
		//        Tar.Collect--;
		//        if (Tar.Collect <= 0)
		//        {
		//            if (Tar.Real || CollectDecoy)
		//            {
		//                Tar.Visible = false;
		//                if (Tar.Real)
		//                    s.CollectedTargets++;
		//                else
		//                    s.CollectedDecoys++;
		//                s.RequireUpdate = true;
		//                robot.Target = null;
		//                state.LastCollect = state.Iterations;
		//                robot.state.NewData = statelist[0];
		//                return true;
		//            }
		//            else if (robot.state.SensorData != statelist[3] && robot.state.SensorData != statelist[2])
		//            {
		//                s.CollectedDecoys++;
		//                robot.state.NewData = statelist[3];
		//            }
		//        }
		//    }
		//    else
		//    {
		//        robot.Target = null;
		//        robot.state.NewData = statelist[0];
		//    }
		//    return false;
		//}

        /// <summary>
        ///目标收集状态：通过UpdateSensor确定目标检测信息
        ///返回false：未发现目标、发现的目标不可见、发现了假目标且不可收集
        /// </summary>
		public sealed override bool CollectTarget(RFitness robot, SFitness state)
		{
			var s = state as SMinimal;
            //未发现目标则直接返回
			if (robot.Target == null) return false;
            //若目标可见
			if (robot.Target.Visible)
			{
				var Tar = robot.Target as FitnessTarget;

                //若是真目标or可收集的假目标则进入“Collecting”状态
				if (Tar.Real || CollectDecoy)
				{
					robot.state.NewData = statelist[1];
                    //由于被收集，目标资源数递减
					Tar.Collect--;
                    //收集完后目标收集数递增，并进入“Run”状态
					if (Tar.Collect == 0)
					{
                        //目标被收集完，则可视性置为false
						Tar.Visible = false;
						if (Tar.Real)
							s.CollectedTargets++;
						else
							s.CollectedDecoys++;

                        //目标被收集完才打开“更新需求”，即“收集过程不改变目标影响范围”？
						s.RequireUpdate = true;
						robot.Target = null;

                        //标记迭代次数，并转入搜索状态
						state.LastCollect = state.Iterations;
						robot.state.NewData = statelist[0];
					}
					return true;
				}
                //若是不可收集的假目标，则进入假目标处理状态（Leaving也可看作假目标处理事件的子状态）
                //而且尚未处在Decoy状态或Leaving状态，则递增假目标数检测数目，并将机器人转入Decoy状态
				else if (robot.state.SensorData != statelist[3] && robot.state.SensorData != statelist[2])
				{
					s.CollectedDecoys++;
					robot.state.NewData = statelist[3];
				}
			}//否则移除目标标记，并进入搜索状态：用于多个机器人同时处理的情况，即某一个机器人收集完后会将目标设为不可见
			else
			{
				robot.Target = null;
				robot.state.NewData = statelist[0];
			}
			return false;
		}

        //确认是是否迭代终止
		protected virtual void env_PostUpdate(RoboticEnvironment env)
		{
			//stop criteria
			var state = env.runstate as SMinimal;
			state.Success = state.CollectedTargets >= TargetNum || state.Finished;
			state.Finished = state.Success || state.AliveRobots == 0;
		}

        //若与障碍物碰撞，则机器人损毁
		void ObstacleCollison(RobotBase r, Obstacle o, RunState state) { if (o.GetType() == typeof(Obstacle)) r.Broken = true; }



		public override void InitializeParameter()
		{
			base.InitializeParameter();
			if (decoyNum == -1) decoyNum = TargetNum * 2;
			ProblemType = CollectDecoy ? FitnessProblemType.CollectProblem : FitnessProblemType.DecoyProblem;
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
            //RoboticProblem基类中已有下列属性赋值，所以可删掉该语句
//			Population = 10;
//			RoboticSenseRange = 20;
//			MaxSpeed = 10;
//			SizeX = SizeY = 400;

            //PFitness基类中已有该属性赋值，所以可删掉该语句
//			TargetNum = 10;
			decoyNum = 0;
			CollectDecoy = false;
			maxTarFit = FitnessLevel;
		}

		int decoyNum, interNum, maxTarFit;

		[Parameter(ParameterType.Int, Description = "Decoy Number")]
		public int DecoyNum
		{
			get { return decoyNum; }
			set
			{
				if (value < -1) throw new Exception("Must be more than -1");
				decoyNum = value;
			}
		}

        //打开可收集的开关则就没有假目标了
		[Parameter(ParameterType.Boolean, Description = "Decoy Collectable")]
		public bool CollectDecoy { get; set; }

		[Parameter(ParameterType.Int, Description = "Interference Number")]
		public int InterferenceNum
		{
			get { return interNum; }
			set
			{
				if (value < 0) throw new Exception("Must be non-negative");
				interNum = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Maximum Target Fitness")]
		public int MaximumTargetFitness
		{
			get { return maxTarFit; }
			set
			{
				if (value <= 0 || value > FitnessLevel) throw new Exception(string.Format("Must be with in (0,{0}]", FitnessLevel));
				maxTarFit = value;
			}
		}
	}
}
