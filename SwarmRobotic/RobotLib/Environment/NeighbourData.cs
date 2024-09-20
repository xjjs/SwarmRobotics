using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Obstacles;
using Microsoft.Xna.Framework;

namespace RobotLib.Environment
{


	public class NeighbourData
	{
		public NeighbourData(NoiseGenerator noise) { this.noise = noise; }

		public void Set() { isNeighbour = false; }

		public void Set(Vector3 offset) { Set(offset.Length(), offset); }

		public void Set(float distance, Vector3 offset)
		{
			isNeighbour = true;
			realdistance = this.distance = distance;
			realoffset = this.offset = offset;
			noise.ParseNoise(this);
		}
		public bool isNeighbour;
		public float distance, realdistance;
		public Vector3 offset, realoffset;
		NoiseGenerator noise;

		public override string ToString() { return isNeighbour ? string.Format("{0}({1})", offset, distance) : "false"; }
	}

    /// <summary>
    /// 邻居类：是否邻居，距离、相对位置（真实值与加噪声后的值），增加特征“泛型”字段
    /// </summary>
	public sealed class NeighbourData<T> : NeighbourData
	{
		public NeighbourData(T Target, NoiseGenerator noise) : base(noise) { this.Target = Target; }
		public T Target;
	}

    //噪声生成器，添加到邻居数据上
	public class NoiseGenerator
	{
		public NoiseGenerator(CustomRandom Random, float NoisePercent = 0)
		{
			this.Random = Random;
			this.NoisePercent = NoisePercent;
		}

		public CustomRandom Random { get; set; }

		public Action<NeighbourData> ParseNoise;

		void GetNoise(NeighbourData data)
		{
            //Clamp：将第一个参数value限制到第二与第三个参数之内；
			float rate = 1 + MathHelper.Clamp(Random.NextGaussianFloat() * noisefac, lb, ub);
            //添加噪声
			data.distance *= rate;
			data.offset *= rate;
		}

		void GetNoNoise(NeighbourData data) { }

		float noise, noisefac, ub, lb;
		public float NoisePercent
		{
			get { return noise; }
			set
			{
                //原始噪声，比例噪声，噪声上界，噪声下界；
				noise = value;
				noisefac = noise / 2.58f;
				ub = noise;
				lb = Math.Max(-noise, -0.9f);
                //将噪声添加到邻域数据上
				ParseNoise = noise == 0 ? (Action<NeighbourData>)GetNoise : GetNoise;
			}
		}
	}

    /// <summary>
    /// 机器人簇：算法、感知范围（邻域尺寸）、机器人列表、邻居组列表（组元素为邻居类）、种群个体数、噪声
    /// </summary>
    public class RobotCluster
    {
        public RoboticAlgorithm algorithm;
        /// <summary>
        /// A constant defining neighbourhood size.
        /// </summary>
        public float SenseRange { get; private set; }
        /// <summary>
        /// List of robotics.
        /// </summary>
        public List<RobotBase> robots;
        /// <summary>
        /// The neighbourhood matrix defining whether 2 robots are neighbours, that is within <see cref="SenseRange"/> distance.
        /// The matrix is symmetric.
        /// </summary>
        public List<GroupingList<int, NeighbourData<RobotBase>>> isNeighbour;
        public int Population { get; private set; }
		NoiseGenerator noise;

        /// <summary>
        ///生成所有个体的邻居列表（组元素列表）
        /// </summary>
        /// <param name="roboticrange"></param>
        /// <param name="population"></param>
        /// <param name="robot"></param>
        /// <param name="noise"></param>
        public RobotCluster(float roboticrange, int population, RobotBase robot, NoiseGenerator noise)
        {
            this.SenseRange = roboticrange;
			this.noise = noise;
            robots = new List<RobotBase>();
            robots.Add(robot);
            robot.id = 0;
            for (int i = 1; i < population; i++)
            {
                var nr = robot.Clone();
                nr.id = i;
                robots.Add(nr);
            }
            Population = population;
            //有多少个体就建立包含对少组对象
			isNeighbour = new List<GroupingList<int, NeighbourData<RobotBase>>>(population);
            //将个体列表投射为邻居列表（作为一个组元素），再设置相应的key值
            for (int i = 0; i < population; i++)
                isNeighbour.Add(robots.Select(r => new NeighbourData<RobotBase>(r, noise)).ToGroupingList(i));
        }

        //设置种群大小
        public void SetPopulation(int population)
        {
            if (Population > population)
            {
                robots.RemoveRange(population, Population - population + 1);
            }
            else
            {
                var adds = new RobotBase[population - Population + 1];
                for (int i = 0; i < adds.Length; i++)
                {
                    adds[i] = robots[0].Clone();
                    adds[i].id = Population + i;
                }
                //添加新增邻居，并利用投射设置新个体的邻居列表
				for (int i = 0; i < Population; i++)
					isNeighbour[i].AddRange(adds.Select(r => new NeighbourData<RobotBase>(r, noise)));
                robots.AddRange(adds);
                for (int i = Population; i < population; i++)
					isNeighbour.Add(robots.Select(r => new NeighbourData<RobotBase>(r, noise)).ToGroupingList(i));
            }
            Population = population;
        }

        //清空第id个机器人的邻居列表
        public void Clear(int id)
        {
            foreach (var item in isNeighbour[id])
                item.isNeighbour = false;
        }
    }

    /// <summary>
    /// 障碍物簇：组对象、组对象的尺寸、噪声、组列表
    /// 构造函数：参数列表中提供or不提供（障碍物列表）
    /// </summary>
	public class ObstacleCluster
	{
		public GroupingList<string, Obstacle> obstacles;
		/// <summary>
		/// An array consisting neighbourhood environment information. Each element stands for the map
		/// of a robotic in <see cref="robotics"/> list.
		/// The map is a list of robotic and obstacle positions.
		/// </summary>
		/// <remarks>The maps are calculated by <see cref="neighbourhoodGenerator"/> in
		/// <see cref="NeighbourhoodGenerator.GenerateNeighbours"/> method.</remarks>
		public List<GroupingList<int, NeighbourData<Obstacle>>> isNeighbour;
		public int Size { get; private set; }
		NoiseGenerator noise;

        /// <summary>
        /// 创建了整体组对象GroupingList和每个机器人的组对象(isNeighbour)，组内成员（实际的障碍物）并未创建
        /// </summary>>
		public ObstacleCluster(string @class, int population, NoiseGenerator noise)
		{
			this.noise = noise;
			obstacles = new GroupingList<string, Obstacle>(@class);
			Size = 0;
			isNeighbour = new List<GroupingList<int, NeighbourData<Obstacle>>>();
			for (int i = 0; i < population; i++)
				isNeighbour.Add(new GroupingList<int, NeighbourData<Obstacle>>(i));

		}

		public ObstacleCluster(string @class, int population, NoiseGenerator noise, IEnumerable<Obstacle> list)
		{
			this.noise = noise;
			obstacles = new GroupingList<string, Obstacle>(@class, list);
			int id = 0;
			foreach (var o in obstacles)
				o.id = id++;
			Size = obstacles.Count;
			isNeighbour = new List<GroupingList<int, NeighbourData<Obstacle>>>(population);
			SetPopulation(population);
		}

		public ObstacleCluster(string @class, int population, NoiseGenerator noise, params Obstacle[] array) : this(@class, population, noise, list: array) { }

		public void SetPopulation(int population)
		{
            //障碍物列表投射为邻居列表，再添加键值；population有多大，就将组对象投射了多少次
			for (int i = isNeighbour.Count; i < population; i++)
				isNeighbour.Add(obstacles.Select(o => new NeighbourData<Obstacle>(o, noise)).ToGroupingList(i));
		}

		public void AddObstacle(Obstacle obstacle)
		{
            //新障碍物分配id，加入组对象
			obstacle.id = Size++;
            //不添加新组，只是在组内添加障碍物
			foreach (var list in isNeighbour)
				list.Add(new NeighbourData<Obstacle>(obstacle, noise));
			obstacles.Add(obstacle);
		}

		public void AddObstacle(IEnumerable<Obstacle> obstacles)
		{
            //为新障碍物分配id
			foreach (var o in obstacles)
				o.id = Size++;
            //在每个机器人的邻居列表内，添加相应的障碍物列表
			foreach (var list in isNeighbour)
				list.AddRange(obstacles.Select(o => new NeighbourData<Obstacle>(o, noise)));
            //将新障碍物添加到组对象中
			this.obstacles.AddRange(obstacles);
		}

		public void AddObstacle(params Obstacle[] obstacles)
		{
			foreach (var o in obstacles)
				o.id = Size++;
			foreach (var list in isNeighbour)
				list.AddRange(obstacles.Select(o => new NeighbourData<Obstacle>(o, noise)));
			this.obstacles.AddRange(obstacles);
		}

		public void ClearRobot(int id)
		{
			foreach (var item in isNeighbour[id])
				item.isNeighbour = false;
		}

		public void ClearObstacle(int id)
		{
			foreach (var item in isNeighbour)
				item[id].isNeighbour = false;
		}
	}
    /// <summary>
    /// 多障碍物簇：多障碍物组对象、组列表（机器人ID->障碍物阵列号->障碍物编号）
    /// </summary>
	public class MultiObstacleCluster
	{
		public GroupingList<string, MultiObstacle> obstacles;
		/// <summary>
		/// An array consisting neighbourhood environment information. Each element stands for the map
		/// of a robotic in <see cref="robotics"/> list.
		/// The map is a list of robotic and obstacle positions.
		/// </summary>
		/// <remarks>The maps are calculated by <see cref="neighbourhoodGenerator"/> in
		/// <see cref="NeighbourhoodGenerator.GenerateNeighbours"/> method.</remarks>
		public List<GroupingList<int, GroupingList<int, NeighbourData<Obstacle>>>> isNeighbour;
		public int Size { get; private set; }
		NoiseGenerator noise;

		public MultiObstacleCluster(string @class, int population, NoiseGenerator noise)
		{
			this.noise = noise;
			obstacles = new GroupingList<string, MultiObstacle>(@class);
			Size = 0;
			isNeighbour = new List<GroupingList<int, GroupingList<int, NeighbourData<Obstacle>>>>();
			for (int i = 0; i < population; i++)
				isNeighbour.Add(new GroupingList<int, GroupingList<int, NeighbourData<Obstacle>>>(i));

		}

		public MultiObstacleCluster(string @class, int population, NoiseGenerator noise, IEnumerable<MultiObstacle> list)
		{
			this.noise = noise;
			obstacles = new GroupingList<string, MultiObstacle>(@class, list);
			int id;
			Size = 0;
            //为每个障碍物阵列内的障碍物编号（高于8bit的为起区分作用的阵列码），尺寸为障碍物阵列的个数
			foreach (var mo in obstacles)
			{
				mo.id = Size;
				id = 0;
				foreach (var o in mo)
					o.id = (Size << 8) + (id++);
				Size++;
			}
			isNeighbour = new List<GroupingList<int, GroupingList<int, NeighbourData<Obstacle>>>>(population);
			SetPopulation(population);
		}

		public MultiObstacleCluster(string @class, int population, NoiseGenerator noise, params MultiObstacle[] array) : this(@class, population, noise, list: array) { }

		public void SetPopulation(int population)
		{
			for (int i = isNeighbour.Count; i < population; i++)
				isNeighbour.Add(obstacles.Select(mo => mo.Select(o => new NeighbourData<Obstacle>(o, noise)).ToGroupingList(mo.id)).ToGroupingList(i));
		}

		public void AddObstacle(MultiObstacle obstacle)
		{
			obstacle.id = Size;
			int id = 0;
			foreach (var o in obstacle)
				o.id = (Size << 8) + (id++);
			foreach (var list in isNeighbour)
				list.Add(obstacle.Select(o => new NeighbourData<Obstacle>(o, noise)).ToGroupingList(Size));
			obstacles.Add(obstacle);
			Size++;
		}

		public void AddObstacle(IEnumerable<MultiObstacle> obstacles)
		{
			int id;
			foreach (var mo in obstacles)
			{
				mo.id = Size;
				id = 0;
				foreach (var o in mo)
					o.id = (Size << 8) + (id++);
				Size++;
			}
			foreach (var list in isNeighbour)
				list.AddRange(obstacles.Select(mo => mo.Select(o => new NeighbourData<Obstacle>(o, noise)).ToGroupingList(mo.id)));
			this.obstacles.AddRange(obstacles);
		}

		public void AddObstacle(params MultiObstacle[] obstacles)
		{
			int id;
			foreach (var mo in obstacles)
			{
				mo.id = Size;
				id = 0;
				foreach (var o in mo)
					o.id = (Size << 8) + (id++);
				Size++;
			}
			foreach (var list in isNeighbour)
				list.AddRange(obstacles.Select(mo => mo.Select(o => new NeighbourData<Obstacle>(o, noise)).ToGroupingList(mo.id)));
			this.obstacles.AddRange(obstacles);
		}

		public void ClearRobot(int id)
		{
			foreach (var item in isNeighbour[id])
				foreach (var o in item)
					o.isNeighbour = false;
		}

		public void ClearObstacle(int id)
		{
			foreach (var item in isNeighbour)
				foreach (var o in item[id])
					o.isNeighbour = false;
		}

		public NeighbourData<Obstacle> GetData(int robotID, int obsID) { return isNeighbour[robotID][obsID >> 8][obsID & 0xFF]; }
	}
}
