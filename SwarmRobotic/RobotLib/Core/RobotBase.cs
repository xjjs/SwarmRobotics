using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using RobotLib.Sensors;
using RobotLib.Obstacles;
using RobotLib.Environment;

namespace RobotLib
{
    /// <summary>
    /// 基本信息：ID、是否损坏、位置传感器、状态传感器、传感器接口列表（添加进前两个传感器）、障碍物列表
    /// </summary>
    public class RobotBase
    {
		public RobotBase()
		{
			Broken = false;
			state = new StateSensor<string>("state sensor", "");
			postionsystem = new PositionSensor();
            mapsensor = null;
			AlgorithmData = null;
            Sensors = new List<ISensor>();

            Sensors.Add(postionsystem);
            Sensors.Add(state);

			//mapsensor = Enumerable.Empty<Obstacle>();
			//mapsensor = new MapObjectSensor();
			//for (int i = 0; i < objecttypes; i++)
			//    mapsensor[i] = new MapObjectSensor();
		}

        public RobotBase(bool inertia) : this() { postionsystem.inertia = inertia; }

        /// <summary>
        /// Call after update. Updates all sensors in collection <see cref="Sensors"/>.
        /// </summary>
		public virtual void ApplyChanges()
        {
			if (Broken) return;
            foreach (var us in Sensors)
                us.ApplyChange();
			Neighbours.Flush();
        }

        /// <summary>
        /// return a new robot with same parameters and sensors setups
        /// individual states can be different
        /// </summary>
        /// <returns></returns>
		public virtual RobotBase Clone() { return new RobotBase(postionsystem.inertia); }

        public virtual void Bind(List<NeighbourData<RobotBase>> RobotNeighbour, List<ObstacleCluster> Obstacles, List<MultiObstacleCluster> MultiObstacles)
		{
            Neighbours = new WrapEnumerable<NeighbourData<RobotBase>>(RobotNeighbour.Where(r => r.isNeighbour));
            //mapsensor = Obstacles.Select(oc => new MapObjectSensor(oc.isNeighbour[id])).ToArray();
            
            mapsensor = Obstacles.Select(oc => oc.isNeighbour[id].Where(o => o.isNeighbour)).ToArray();
            //每个簇->每个组->每个障碍物阵列
			largemapsensor = MultiObstacles.Select(moc => moc.isNeighbour[id].Where(mo => mo.isNeighbour)).ToArray();
			//postionsystem.NeighbourData = Neighbours.Select(r => postionsystem.CalculateNeighbourData(r.Target.postionsystem));
			postionsystem.NeighbourData = null;
            state.NeighbourData = Neighbours.Select(r => r.Target.state.SensorData);
		}

		public override string ToString() { return string.Format("({0}):{3} {1} {2}", id, state.SensorData, postionsystem.GlobalSensorData, Broken ? "Broken" : ""); }

        //public IEnumerable<Obstacle> mapsensor;
		//public IEnumerable<Obstacle>[] mapsensor;
		//public MapObjectSensor[] mapsensor;
        /// <summary>
        /// 障碍物集合的各类型列表、障碍物阵列集合的各类型列表、机器人邻居列表
        /// </summary>
        public IEnumerable<NeighbourData<Obstacle>>[] mapsensor;
		public IEnumerable<IGrouping<int, NeighbourData<Obstacle>>>[] largemapsensor;
		public WrapEnumerable<NeighbourData<RobotBase>> Neighbours;

		public bool Broken;
		public int id;
		public PositionSensor postionsystem;
        //状态传感器state中包含所有邻居的SensorData，SensorData为string类型的，初始为空串，NewData未赋值
        //ApplyChange是用NewData更新SensorData
        public StateSensor<string> state;
		public object AlgorithmData;

        protected List<ISensor> Sensors;


        //对速度大小进行控制
        float speed;
        [Parameter(ParameterType.Float, Description = "MaxSpeed for Robot")]
        public float MaxSpeed { get; set; }
        [Parameter(ParameterType.Float, Description = "MinSpeed for Robot")]
        public float MinSpeed { get; set; }
        [Parameter(ParameterType.Float, Description = "Speed for Robot")]
        public float Speed {
            get { return speed; }
            set {
                if (value > MaxSpeed) speed = MaxSpeed;
                else if (value < MinSpeed) speed = MinSpeed;
                else speed = value;
            }
        }

        //Levy Flight 所需变量，适应度标志、普通迭代次数、剩余移动长度
        public int NumOfV;
        public float RemainingOfV;

        //编队搜索所需变量：扩散标志、虚拟编号、角色编号、组编号、独行标识
        public int diffusionFlag;
        public int virID;
        public int groupID;
        public bool singleFlag;
        public bool nextFlag;
        public RobotBase teammate1;
        public RobotBase teammate2;

        //PGES所需要的变量：决策惯性因子(状态惯性因子)
        public float inertiaState;
        public float inertiaRun;
        public float inertiaDiffusion;

        //EvolveRobotic所需要的变量
        public int NumOfState;

        public int cnt { get; set; }

        //batch
        public bool batchObject;
    }
}

//机器人的基类
//Clone：返回一个惯性参数相同的机器人；
//Bind：构建邻居列表、障碍物列表、多障碍物列表？Select水平投射，Where垂直筛选，先投射为一个布尔值列表，再筛选
