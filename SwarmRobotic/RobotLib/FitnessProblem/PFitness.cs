using System;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;
using System.Linq;
using System.Collections.Generic;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 可看为ProblemFitness，继承自RoboticProblem，能量问题（实际为适应度值问题），更新环境（添加障碍物）与重置环境、更新总距离
    /// 特性修饰属性：目标数量、障碍物数量、障碍物感知范围、历史空间大小
    /// </summary>
	public abstract class PFitness : RoboticProblem
	{
        //问题类型默认为能量类型
		public PFitness(FitnessProblemType type = FitnessProblemType.EnergyProblem) { ProblemType = type; }

        //安排初始种群的区域，与地图同心的矩形区域
        public override void ArrangeRobotic(List<RobotBase> robotics)
        {
            //获取地图中心位置
            Vector3 pos = MapSize / 2;
            //筛选出robotics中RFitness成员，构成集合IEnumerate<RFitness>返回，进而返回集合的迭代器
			IEnumerator<RFitness> r = robotics.OfType<RFitness>().GetEnumerator();
            //生成每个个体的位置(地图中心位置处的群体的位置阵列)
            IEnumerator<Vector3> v = Utility.UnionInitialize(Population, 
                pos, 5, PositionType.Center, PositionType.Center, PositionType.Center, false).GetEnumerator();
            
            //将生成的位置阵列依次赋给每个机器人的NewData，第一次更新后可初始化机器人的初始位置
            while (r.MoveNext() && v.MoveNext())
            {
                //设置要更新的位置与状态
                r.Current.postionsystem.NewData = v.Current;
                r.Current.state.NewData = "Run";

				r.Current.Target = null;
                r.Current.History.Clear();
				r.Current.LeaveCheckPoint = null;
            }
            //应用更新的信息
            base.ArrangeRobotic(robotics);
        }

		protected Vector3 GenerateObstaclePos() 
        { return new Vector3((int)(Random.NextDouble() * SizeX), (int)(Random.NextDouble() * SizeY), SizeZ > 1 ? (int)(Random.NextDouble() * SizeZ) : 0f); }//0f本来是0.5

        //创建与添加障碍物
        public override void CreateEnvironment(RoboticEnvironment env)
        {
            var obstacles = new Obstacle[obsNum];
			for (int i = 0; i < obsNum; i++)
                obstacles[i] = new Obstacle(GenerateObstaclePos(), oRange);
            //添加之前只有簇列表对象与组对象（GroupingList），没有实际的“一般障碍物”成员
			env.ObstacleClusters[0].AddObstacle(obstacles);

            //注册更新事件的处理函数
			if (TestMode) env.PostUpdate += UpdateTest;
        }

        //重置随机种子，重新生成第0类型的障碍物
		public override void ResetEnvironment(RoboticEnvironment env)
		{
			base.ResetEnvironment(env);
			foreach (var obs in env.ObstacleClusters[0].obstacles)
				obs.Position = GenerateObstaclePos();
		}

		public abstract bool CollectTarget(RFitness robot, SFitness state);

        //叠加所有机器人上次的移动距离
        void UpdateTest(RoboticEnvironment env)
        {
			var state = env.runstate as SFitness;
            //叠加每个机器人的移动距离
            foreach (var r in env.RobotCluster.robots)
                if (!r.Broken) state.TotalDistance += r.postionsystem.LastMove.Length();
        }

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			tarNum = 10;
			obsNum = 0;
			oRange = 20;

			hisSize = 5;
            tarSize = 10;
		}

		int tarSize, tarNum, obsNum, hisSize;
		float oRange;

        [Parameter(ParameterType.Int, Description = "Target Size")]
        public int TargetSize
        {
            get { return tarSize; }
            set
            {
                if (value <= 0) throw new Exception("Must be positive");
                tarSize = value;
            }
        }

		[Parameter(ParameterType.Int, Description = "Target Number")]
		public int TargetNum
		{
			get { return tarNum; }
			set
			{
				if (value <= 0) throw new Exception("Must be positive");
				tarNum = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Obstacle Number")]
		public int ObstacleNum
		{
			get { return obsNum; }
			set
			{
				if (value < 0) throw new Exception("Must be at least 0");
				obsNum = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Obstacle Sensing Range")]
		public float ObstacleSenseRange
		{
			get { return oRange; }
			set
			{
				if (value < 3 || value > 100) throw new Exception("Must be in [3, 100]");
				oRange = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "History Size")]
		public int HistorySize
		{
			get { return hisSize; }
			set
			{
				if (value <= 0) throw new Exception("Must be at least 1");
				hisSize = value;
			}
		}

		public override int SizeZ
		{
			get { return base.SizeZ; }
			set { base.SizeZ = value; }
		}

		public FitnessProblemType ProblemType { get; set; }
	}

    /// <summary>
    /// 适应度问题的类型：能量、收集、诱饵
    /// </summary>
	public enum FitnessProblemType
	{
		EnergyProblem, CollectProblem, DecoyProblem
	}
}
