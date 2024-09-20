using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自AFitness的PSO算法
    /// 惯性分量、社会分量、历史分量都生成的delta最后乘以maxspeed
    /// 不能保证delta属于[0,1]，即不能保证最终速度小于maxspeed
    /// </summary>
	public class APSOEFitness : AFitness
	{
		float w, c1, c2;

		public APSOEFitness() : base() { }

        //虽是重写，但函数体代码没变
		protected internal override Vector3 RandomSearch(RFitness robotic)
		{
			robotic.RandomSearch = true;
			//Vector3 delta = new Vector3();
			//var pos = robotic.postionsystem;
			//delta = w * pos.LastMove + c1 * RandPosition() + c2 * RandPosition();
			var last = robotic.postionsystem.LastMove / maxspeed;
			if (last.Length() < 0.5) last = RandPosition();
			last *= maxspeed;
			Bounding(robotic.postionsystem.GlobalSensorData, ref last);
			return last;
		}

		protected internal override Vector3 FitnessSearch(RFitness robotic)
		{
			robotic.RandomSearch = false;
			var pos = robotic.postionsystem;
			var Fitness = robotic.Fitness.SensorData;
			int cmp;
			Vector3 delta, vec, last = pos.LastMove;

            //规范化惯性方向
			if (last.Length() < 0.1)
				last = RandPosition();
			else
				last /= maxspeed;
			//delta = w * pos.LastMove + c1 * RandPosition() + c2 * RandPosition();

			////////////self
            //寻找历史最优			
            int max = -1;
			Vector3 maxpos = Vector3.Zero;
			foreach (var item in robotic.History)
			{
				if (item.Fitness > max)
				{
					max = item.Fitness;
					maxpos = item.Position;
				}
			}
			cmp = (max == -1) ? 0 : Fitness.CompareTo(max);
			if (cmp == 0)
				delta = last;
			else
			{               
				vec = pos.GlobalSensorData - maxpos;
				vec.Normalize();
				if (float.IsNaN(vec.X))
					delta = last;
				else
					delta = w * last + c1 * cmp * vec * (float)rand.NextDouble();
			}
			///////////neighbour
			max = -1;
			RFitness r;
			foreach (var item in robotic.Neighbours)
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
                //邻域最优
				vec = pos.GlobalSensorData - maxpos;
				vec.Normalize();
				delta += c2 * cmp * vec * (float)rand.NextDouble();
			}
			else
			{
                //没有邻域更优则只考虑随机因素（历史因素也丢弃了。。）
				if (delta.Length() > 0.1f)
					delta = Vector3.Normalize(delta) * 0.8f + RandPosition() * 0.2f;
				else
					delta = RandPosition();
			}
            //注释掉原来的，新的返回对长度进行了限制
//			return delta * maxspeed;
            return NormalOrZero(delta) * maxspeed;
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			w = 0.5f;
			c1 = 3.3f;// 3.5f;
			c2 = 0.1f;// 0.1f;
		}

		[Parameter(ParameterType.Float, Description="w")]
		public float W
		{
			get { return w; }
			set
			{
				if (value < 0 || value > 1) throw new Exception("Must be in [0,1]");
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
