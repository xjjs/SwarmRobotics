using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.TestProblem
{
    /// <summary>
    /// 继承自RoboticAlgorithm
    /// 参数与属性只有随机数种子
    /// </summary>
	public class ATest : RoboticAlgorithm
	{
		Vector3 MapSize;
		float maxspeed;
		Random rand;
		Func<Vector3> RandVelocity;

        public ATest() { }//statelist = new string[] { "Run" }; }

        //设置地图尺寸、最大速度、随机速度生成函数
		public override bool Bind(RoboticProblem problem, bool changePara = true)
		{
            if (problem is PTest)
            {
                MapSize = problem.MapSize;
                maxspeed = problem.MaxSpeed;
                if (MapSize.Z > 1)
                    RandVelocity = RandVelocity3D;
                else
                    RandVelocity = RandVelocity2D;
                return true;
            }
            return false;
		}

		public override void Reset()
		{
			if (seed == -1)
				rand = new Random();
			else
				rand = new Random(seed);
		}

        //对delta进行越界检查，但此处的Bounding并没有位置更新，难道本身就允许坐标为负？？
		void Bounding(Vector3 position, ref Vector3 delta)
		{
			if ((position.X < 0 && delta.X < 0) || (position.X > MapSize.X && delta.X > 0))
				delta.X = -delta.X;
			if ((position.Y < 0 && delta.Y < 0) || (position.Y > MapSize.Y && delta.Y > 0))
				delta.Y = -delta.Y;
		}

		Vector3 RandVelocity2D()
		{
			double ang = rand.NextDouble() * MathHelper.TwoPi;
			return new Vector3((float)Math.Cos(ang), (float)Math.Sin(ang), 0);
		}

		Vector3 RandVelocity3D()
		{
			double ang1 = rand.NextDouble() * MathHelper.TwoPi, ang2 = rand.NextDouble() * MathHelper.TwoPi;
			return new Vector3((float)(Math.Cos(ang1) * Math.Cos(ang2)), (float)(Math.Sin(ang1) * Math.Cos(ang2)), (float)Math.Sin(ang2));
		}

        //保持以前的速度前进（若过小则重置速度）
        public override void Update(RobotBase robotic, RunState state)
		{
			Vector3 delta = robotic.postionsystem.LastMove;
			if (delta.Length() < 0.1 || rand.NextDouble() < 0.01)
				delta = RandVelocity();
			Bounding(robotic.postionsystem.GlobalSensorData, ref delta);
			robotic.postionsystem.NewData = delta;
		}

		public override void InitializeParameter()
		{
			if (seed == -1)
				rand = new Random();
			else
				rand = new Random(seed);
		}

        public override void CreateDefaultParameter() { seed = -1; }

		int seed;
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
	}
}
