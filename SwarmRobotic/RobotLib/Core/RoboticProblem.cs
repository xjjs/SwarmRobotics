using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;

namespace RobotLib
{
    /// <summary>
    /// 问题抽象类：抽象方法（创建机器人、创建环境），虚方法（重置环境、更新传感器），地图尺寸（三维向量）
    /// 特性修饰属性：种群大小、机器人感知范围、地图各维尺寸、最大速度
    /// </summary>
    public abstract class RoboticProblem : IParameter
    {
        public RoboticProblem()
            : base()
        {
            statelist = null;
            CreateDefaultParameter();
        }

        //public abstract void UpdateObstacles();

        public abstract RobotBase CreateRobot(RoboticEnvironment env);

        public abstract void CreateEnvironment(RoboticEnvironment environment);

        public virtual void ResetEnvironment(RoboticEnvironment env)
        {
            if (seed != -1) Random = new CustomRandom(seed);
        }

        public virtual void ArrangeRobotic(List<RobotBase> robotics)
         {
            int Len = (int)Math.Ceiling(Math.Pow(pop, 1 / 2.0));
            int LastColumnNum = pop / Len;
            int ColumnNum = 0;
            
            foreach (RobotBase r in robotics)
            {
                //EvolveRobotic
                r.NumOfState = 0;

                //Levy飞行的连续标志
                r.NumOfV = 0;
                r.cnt = 2;

                //编队飞行的初始分组
                r.diffusionFlag = 2;           
                r.virID = pop - 1 - r.id;
                ColumnNum = r.id / Len;
                if ((LastColumnNum - ColumnNum) % 2 == 1)
                {
                    r.virID = 2 * pop - 1 - (2 * ColumnNum + 1) * Len - r.virID;
                }

                r.groupID = r.virID - r.virID % 3;
                if (r.groupID + 2 > pop - 1)
                {
                    r.singleFlag = true;
                    r.diffusionFlag = 0;
                }
                else
                {
                    r.singleFlag = false;
                }
                r.nextFlag = false;

                //PGES的惯性因子赋值
                r.inertiaState = 0.0f;
                r.inertiaDiffusion = 0.0f;
                r.inertiaRun = 0.0f;
                

                //速度赋值
                r.MaxSpeed = MaxSpeed;
                r.MinSpeed = MaxSpeed/5;
                r.Speed = r.MaxSpeed;
                //Move()之后，位置（GlobalSensorData）已经更改，位移增量（NewData）存入LastMove后，变为Vector3.Zero
                r.postionsystem.Move();
                //再次调用ApplyChange()，唯一的作用是将LastMove清零(Arrange阶段不需要记录上次的速度)
                r.ApplyChanges();
            }
        }

		/// <summary>
		/// Call before update
		/// </summary>
		public virtual void UpdateSensor(RobotBase robot, RunState state) { }

		public void FinalizeState(RunState state, RoboticEnvironment environment)
		{
			if (state.Finalized) return;
            //资源回收事件所触发的处理函数
			if (OnFinalize != null) OnFinalize(state, environment);
			state.Finalized = true;
		}

		protected event Action<RunState, RoboticEnvironment> OnFinalize;

		public CustomRandom Random { get; private set; }
        public Vector3 MapSize { get; private set; }
        public string[] statelist 
        { 
            get; 
            protected set; 
        }

        public virtual void InitializeParameter()
        {
            if (seed == -1)
				Random = new CustomRandom();
            else
				Random = new CustomRandom(seed);
			if (Size != 0) SizeX = SizeY = Size;
			MapSize = new Vector3(sizeX, sizeY, sizeZ);
		}

        public virtual void CreateDefaultParameter()
        {
            TestMode = true;
            seed = -1;
            rRange = 20;
            pop = 50;
//            sizeX = sizeY = 2000;
            sizeX = sizeY = 1000;
            sizeZ = 0; //1
            maxSpeed = 5;
            Size = 0;
		}

        int seed, pop, sizeX, sizeY, sizeZ;
		float rRange, maxSpeed;

        [Parameter(ParameterType.Int, Description = "Random Seed")]
        public int RandSeed
        {
            get { return seed; }
            set
            {
                if (value < -1) throw new Exception("Must be at least -1");
                seed = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Robot Neighbour Range")]
        public float RoboticSenseRange
        {
            get { return rRange; }
            set
            {
                if (value < 3 || value > 100) throw new Exception("Must be in [3, 100]");
                rRange = value;
            }
        }

        [Parameter(ParameterType.Int, Description = "Population")]
        public int Population
        {
            get { return pop; }
            set
            {
                if (value <= 0) throw new Exception("Must be positive");
                pop = value;
            }
        }

        [Parameter(ParameterType.Int, Description = "Map X-axis Size")]
        public int SizeX
        {
            get { return sizeX; }
            set
            {
                if (value < 10) throw new Exception("Must be at least 10");
                sizeX = value;
            }
        }

        [Parameter(ParameterType.Int, Description = "Map Y-axis Size")]
        public int SizeY
        {
            get { return sizeY; }
            set
            {
                if (value < 10) throw new Exception("Must be at least 10");
                sizeY = value;
            }
        }

        [Parameter(ParameterType.Int, Description = "Map Z-axis Size")]
        public virtual int SizeZ
        {
            get { return sizeZ; }
            set
            {
                if (value < 1) throw new Exception("Must be at least 1");
                sizeZ = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Max Speed")]
        public float MaxSpeed
        {
            get { return maxSpeed; }
            set
            {
                if (value <= 0) throw new Exception("Must be positive");
                maxSpeed = value;
            }
        }

        public bool TestMode { get; set; }

        //非零则生成该尺寸的正方形地图
        public int Size { get; set; }
	}
}
