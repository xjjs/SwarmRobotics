using System;
using Microsoft.Xna.Framework;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自AFitness的PSO算法：注意对更新向量delta的长度考察，并设置hasN来标志是否有合适的历史分量与社会分量
    /// 只有惯性分量乘以了maxspeed，社会分量与历史分量都不变；返回的delta不一定小于maxspeed
    /// </summary>
	class APSOMFitness : AFitness
	{
		public APSOMFitness() { }

		protected internal override bool SearchRandomly(RFitness r) { return false; }

		protected internal override Vector3 RandomSearch(RFitness robot)
		{
			throw new NotImplementedException();
		}

		protected internal override Vector3 FitnessSearch(RFitness robot)
		{
			robot.RandomSearch = false;
			bool hasN = false;
			var pos = robot.postionsystem;
			var Fitness = robot.Fitness.SensorData;
			int cmp;
			Vector3 delta, vec, last = pos.LastMove;

            //规范化惯性速度
			if (last.Length() < 0.1f)
				last = RandPosition() * maxspeed;
			//delta = w * pos.LastMove + c1 * RandPosition() + c2 * RandPosition();
			delta = w * last;

            
			int max = -1;
			Vector3 maxpos = Vector3.Zero;
			////////////self，历史分量
			foreach (var item in robot.History)
			{
				if (item.Fitness > max)
				{
					max = item.Fitness;
					maxpos = item.Position;
				}
			}
			cmp = (max == -1) ? 0 : Fitness.CompareTo(max);
			if (cmp != 0)
			{
				vec = pos.GlobalSensorData - maxpos;
				if (vec.Length() > 0.1f)
				{
                    //历史分量没有乘以maxspeed
					delta += c1 * cmp * vec * (float)rand.NextDouble();
					hasN = true;
				}
			}
			///////////neighbour，邻域分量
			max = -1;
			RFitness r;
			foreach (var item in robot.Neighbours)
			{
				r = item.Target as RFitness;
				if (r.Fitness.SensorData > max)
				{
					max = r.Fitness.SensorData;
					maxpos = r.postionsystem.GlobalSensorData;
				}
			}
			cmp = (max == -1) ? 0 : Fitness.CompareTo(max);
			if (cmp != 0)
			{
				vec = pos.GlobalSensorData - maxpos;
				if (vec.Length() > 0.1f)
				{
                    //社会分量也没有乘以maxspeed
					delta += c2 * cmp * vec * (float)rand.NextDouble();
					hasN = true;
				}
			}

            //没有历史更优，而且没有邻域更优时，只需再加上一个随机向量即可
			if (!hasN) delta += (1 - w) * RandPosition() * maxspeed;
			return delta;
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			w = 0.8f;
			c1 = 3.8f;
			c2 = 2.2f;
		}

		float w, c1, c2;

		[Parameter(ParameterType.Float, Description = "w")]
		public float W
		{
			get { return w; }
			set
			{
				if (value < 0 || value >= 1) throw new Exception("Must be in [0,1)");
				w = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "c1")]
		public float C1
		{
			get { return c1; }
			set
			{
				if (value < 0) throw new Exception("Must be in positive");
				c1 = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "c2")]
		public float C2
		{
			get { return c2; }
			set
			{
				if (value < 0) throw new Exception("Must be in positive");
				c2 = value;
			}
		}
	}
}
