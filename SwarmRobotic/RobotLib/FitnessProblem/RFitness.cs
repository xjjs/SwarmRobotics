using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Sensors;
using RobotLib.Environment;
using RobotLib.Obstacles;
using Microsoft.Xna.Framework;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 可看为RobotFitness，继承自RobotBase，添加：适应度传感器、目标、历史列表、障碍物集合字段
    /// 现在每个机器人有三种传感器：局部坐标系传感器positionsystem，状态机传感器state，适应度传感器Fitness
    /// </summary>
	public class RFitness : RobotBase, IComparable<RFitness>
	{
		public RFitness(int hisSize)
		{
			Fitness = new StateSensor<int>("Fitness Sensor", 0);
			Target = null;
			History = new HistoryList(hisSize);
			LeaveCheckPoint = null;
            cnt = 0;
		}

        //绑定3中簇列表的信息，主要是设置邻居适应度列表与单纯障碍物列表
		public override void Bind(List<NeighbourData<RobotBase>> RobotNeighbour, List<ObstacleCluster> Obstacles, List<MultiObstacleCluster> MultiObstacles)
		{
			base.Bind(RobotNeighbour, Obstacles, MultiObstacles);
            //邻居机器人的适应度信息
			Fitness.NeighbourData = Neighbours.Select(r => (r.Target as RFitness).Fitness.SensorData);
            //第一种类型的障碍物为单纯的“障碍物”
			this.Obstacles = mapsensor[0];
		}

		public override RobotBase Clone() { return new RFitness(History.Capacity); }

        //与其他个体的适应度值比较，小者为优
		public int CompareTo(RFitness other)
		{
			int val = Fitness.SensorData.CompareTo(other.Fitness.SensorData);
			if (val == 0) val = id.CompareTo(other.id);
			return -val;	//descending
		}

		public override string ToString() { return string.Format(Broken ? "({0}):{1} Broken {2}" : "({0}):{1} F{3} {2}", id, state.SensorData, postionsystem.GlobalSensorData, Fitness.SensorData); }

		public StateSensor<int> Fitness;
		public Obstacle Target;
		public HistoryList History;
        //单独定义单纯障碍物列表（见Bind），为了使用方便么？
		public IEnumerable<NeighbourData<Obstacle>> Obstacles;
        //穿越标记点，路径上离假目标最近的点
		public Vector3? LeaveCheckPoint;
		public bool RandomSearch;

        //间歇式搜索,AdaPSO到上一代为止的最优适应度
        public int cnt;
        public int interNum; //迭代次数

	}
}
