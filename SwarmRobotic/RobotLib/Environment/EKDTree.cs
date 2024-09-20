using System.Collections;
using System.Linq;
using UtilityProject.KDTree;
using RobotLib.Obstacles;

namespace RobotLib.Environment
{
	public class EKDTree : RoboticEnvironment
	{
		ArrayList result = new ArrayList();
		KDTree tree;
		KDTreeRoboticData[] rdatas;
		KDTreeObstacleData[] odatas;
		KDTreeMultiObstacleData[] modatas;

		public EKDTree() { }

		public EKDTree(bool UseFast) : this() { this.UseFast = UseFast; }

		void EKDTree_OnInitialize(RoboticEnvironment obj)
		{
			int dim = problem.MapSize.Z > 1 ? 3 : 2;
            //分别将三种簇列表生成三种数据结点数组，即生成“新的对象”
            //Select是返回各个元素构成的集合（若元素有子元素则是一个嵌套集合），SelectMany是提取各个元素的子元素以构成一个新的集合（有子元素也是单层集合）
			rdatas = RobotCluster.robots.Select(r => new KDTreeRoboticData(r, dim)).ToArray();
			odatas = ObstacleClusters.SelectMany(o => o.obstacles.Select(obs => new KDTreeObstacleData(obs, dim, o))).ToArray();
			modatas = MultiObstacleClusters.SelectMany(moc => moc.obstacles.SelectMany(mo => mo.Select(obs => new KDTreeMultiObstacleData(obs, dim, moc)))).ToArray();
			//根据选项确定是否使用索引KD树
            tree = UseFast ? (KDTree)new KDTree_SR(dim, RobotCluster.SenseRange) : (KDTree)new KDTree_Basic(dim, RobotCluster.SenseRange);
            tree.BindData(rdatas, RoboticCallBack, ObstacleCallBack);
		}

		public override void GenerateNeighbours()
		{
			base.GenerateNeighbours();
            //建树并按范围半径查找
			tree.BuildTree();
			//robotic
			//for (int i = Population - 1; i > 0; i--)
			//{
			//    if (rdatas[i].Robotic.Broken) continue;
			//    tree.FindInRange(i);
			//}

            //考察并标记机器人的邻居列表
			tree.FindAllInRange();

			//obstacle
			tree.ObstacleCallback = ObstacleCallBack;
			foreach (var obs in odatas)
				if (obs.Range > 0 && !obs.Skip)
					tree.FindInRange(obs.Range, obs);

			//multiobstacle
			tree.ObstacleCallback = MultiObstacleCallBack;
			foreach (var obs in modatas)
				if (obs.Range > 0 && !obs.Skip)
					tree.FindInRange(obs.Range, obs);
		}

        //查找到邻域内的机器人就设置两者邻居关系，参数为：邻居机器人位置、考察点位置、两者欧式距离
		void RoboticCallBack(IKDTreeData id1, IKDTreeData id2, float distance)
		{
            //相对位置用“本结点”-“邻居结点”计算
            //as运算符类似于强制转换操作。但是无法进行转换，则as返回null而非引发异常。
			var v = (id2 as KDTreeRoboticData).pos.GlobalSensorData - (id1 as KDTreeRoboticData).pos.GlobalSensorData;
			RobotCluster.isNeighbour[id1.ID][id2.ID].Set(distance, v);
			RobotCluster.isNeighbour[id2.ID][id1.ID].Set(distance, -v);
		}
       
        //查找到邻域内的障碍物就设置两者邻居关系，参数为：邻居机器人位置、考察障碍物位置、两者欧氏距离
		void ObstacleCallBack(IKDTreeData id1, IKDTreeData id2, float distance)
		{
            //相对坐标也是用“本结点”-“邻居结点”
			var obs = id2 as KDTreeObstacleData;
			var v = obs.o.Position - (id1 as KDTreeRoboticData).pos.GlobalSensorData;
			obs.cluster.isNeighbour[id1.ID][id2.ID].Set(distance, v);
		}
        //查找到邻域内的障碍物阵列
		void MultiObstacleCallBack(IKDTreeData id1, IKDTreeData id2, float distance)
		{
			var obs = id2 as KDTreeMultiObstacleData;
			var v = obs.o.Position - (id1 as KDTreeRoboticData).pos.GlobalSensorData;
			obs.cluster.GetData(id1.ID, id2.ID).Set(distance, v);
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			UseFast = true;
		}

		public override void InitializeParameter()
		{
			base.InitializeParameter();
			OnInitialize += new System.Action<RoboticEnvironment>(EKDTree_OnInitialize);
		}

		[Parameter(ParameterType.Boolean, Description = "Fast KDTree")]
		public bool UseFast { get; set; }
	}
	
    //定义机器人的数据结点
	class KDTreeRoboticData : IKDTreeData
    {
		public Sensors.PositionSensor pos;

		public KDTreeRoboticData(RobotBase r, int Dimension)
		{
			ID = r.id;
			pos = r.postionsystem;
			this.Dimension = Dimension;
			Robotic = r;
		}

		public int Dimension { get; set; }

		public int ID { get; set; }

		public RobotBase Robotic { get; set; }

		public float this[int index] { get { return (index == 0) ? pos.GlobalSensorData.X : (index == 1) ? pos.GlobalSensorData.Y : pos.GlobalSensorData.Z; } }

		public bool Skip { get { return Robotic.Broken; } }
	}
	//定义障碍物簇的数据结点
	class KDTreeObstacleData : IKDTreeData
	{
		public Obstacle o;
        public ObstacleCluster cluster;

		public KDTreeObstacleData(Obstacle o, int Dimension, ObstacleCluster cluster)
		{
			ID = o.id;
			this.o = o;
			this.Dimension = Dimension;
            this.cluster = cluster;
		}

		public int Dimension { get; set; }

		public int ID { get; set; }

		public float Range { get { return o.SenseRange; } }

		public float this[int index] { get { return (index == 0) ? o.Position.X : (index == 1) ? o.Position.Y : o.Position.Z; } }

		public bool Skip { get { return !o.Visible; } }
	}
    //定义障碍物阵列的数据结点
	class KDTreeMultiObstacleData : IKDTreeData
	{
		public Obstacle o;
		public MultiObstacleCluster cluster;

		public KDTreeMultiObstacleData(Obstacle o, int Dimension, MultiObstacleCluster cluster)
		{
			ID = o.id;
			this.o = o;
			this.Dimension = Dimension;
			this.cluster = cluster;
		}

		public int Dimension { get; set; }

		public int ID { get; set; }

		public float Range { get { return o.SenseRange; } }

		public float this[int index] { get { return (index == 0) ? o.Position.X : (index == 1) ? o.Position.Y : o.Position.Z; } }

		public bool Skip { get { return !o.Visible; } }
	}
}
