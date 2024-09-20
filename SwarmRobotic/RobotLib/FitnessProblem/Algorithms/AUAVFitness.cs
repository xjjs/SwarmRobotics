using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UtilityProject;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 求解能量限制条件下目标搜索问题的算法：暂且先不看
    /// </summary>
	public class AUAVFitness : AFitness
	{
		public AUAVFitness() { }

		public override object CreateCustomData() { return new TagUAV(avestep); }

		public override void ClearCustomData(object data)
		{
			var Tag = data as TagUAV;
			Tag.LastSearch = Vector3.Zero;
			Tag.Fitness.Init();
			Tag.AveFitness = 0;
			Tag.Time = 0;
		}

		public override void UpdateCustomData(RobotBase robot)
		{
			var Tag = robot.AlgorithmData as TagUAV;
			var newv = (robot as RFitness).Fitness.SensorData / (float)avestep;
			if (Tag.Fitness.Enqueue(newv)) Tag.AveFitness -= Tag.Fitness.Last;
			Tag.AveFitness += newv;
		}

		Vector3 Spread(RFitness robot)
		{
			Vector3 delta = Vector3.Zero;
			int count = 0;
			float min = float.MaxValue;
			Vector3 minpos = Vector3.Zero;
			foreach (var item in robot.Neighbours)
			{
				delta -= item.offset;
				count++;
				if (item.distance < min)
				{
					min = item.distance;
					minpos = item.offset;
				}
			}
			if (count > 0)
			{
				if ((delta - minpos).Length() > balance)
				{
					float len = delta.Length();
					var tmp = balance * balance - Vector3.Cross(delta, minpos).LengthSquared() / (len * len);
					if (tmp < 0)
						delta = Vector3.Normalize(minpos) * (minpos.Length() - balance);
					else
						delta *= ((float)Math.Sqrt(tmp) + Vector3.Dot(delta, minpos) / len) / len;
				}
			}
			return delta;
		}

		protected internal override Vector3 RandomSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as TagUAV;
			if (!robotic.RandomSearch)
			{
				robotic.RandomSearch = true;
				Tag.Time = 0;
			}
			if (Tag.Time == 0)
			{
				Vector3 delta = robotic.postionsystem.LastMove;
				if (delta.Length() < 0.1f) delta = RandPosition() * maxspeed;
				Bounding(robotic.postionsystem.GlobalSensorData, ref delta);
				Tag.LastSearch = delta;
				Tag.Time = d;
				return delta + Spread(robotic);
				//var Tag = robotic.AlgorithmData as TagUAV;
				//if (Tag.LastSearch.Length() < 0.1f) Tag.LastSearch = RandPosition() * maxspeed;
				//Bounding(robotic.postionsystem.GlobalSensorData, ref Tag.LastSearch);
				//return delta + Tag.LastSearch;
			}
			else
			{
				Tag.Time--;
				return Spread(robotic) + Tag.LastSearch;
			}
		}

		protected internal override Vector3 FitnessSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as TagUAV;
			if (robotic.RandomSearch)
			{
				robotic.RandomSearch = false;
				Tag.Time = 0;
			}
			if (Tag.Time == 0)
			{
				float avef, maxavef = Tag.AveFitness;
				Vector3 maxdir = robotic.postionsystem.LastMove;
				foreach (var item in robotic.Neighbours)
				{
					avef = (((item.Target.AlgorithmData is TagUAV) ? item.Target.AlgorithmData : (item.Target.AlgorithmData as AHybridFitness.HybridTag).FitTag) as TagUAV).AveFitness;
					if (avef > maxavef)
					{
						maxavef = avef;
						maxdir = item.offset;
					}
				}
				Tag.Time = d;
				if (maxavef == Tag.AveFitness)
				{
					Tag.LastSearch = maxdir + RandPosition();
				}
				else
				{
					if (maxdir.Length() < 0.1f) maxdir = RandPosition() * maxspeed;
					Tag.LastSearch = maxdir;
				}
				return Spread(robotic) + maxdir;
			}
			else
			{
				Tag.Time--;
				return Spread(robotic) + Tag.LastSearch;
			}
		}

		public override void InitializeParameter()
		{
			base.InitializeParameter();
			balance = br * problem.RoboticSenseRange;
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			avestep = 5;
			d = 9;
			br = 0.7f;
		}

		int avestep, d;
		float balance, br;

		[Parameter(ParameterType.Float, Description = "Balance Rate")]
		public float BR
		{
			get { return br; }
			set
			{
				if (value <= 0 || value >= 1) throw new Exception("Must be within (0,1)");
				br = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Average Step")]
		public int AveStep
		{
			get { return avestep; }
			set
			{
				if (value < 1 || value > 10) throw new Exception("Must be within [1,10]");
				avestep = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "ReSelect Iteration")]
		public int D
		{
			get { return d; }
			set
			{
				if (value < 1 || value > 10) throw new Exception("Must be within [1,10]");
				d = value;
			}
		}

		class TagUAV
		{
			public TagUAV(int capacity)
			{
				LastSearch = Vector3.Zero;
				Fitness = new FixMaxSizeQueue<float>(capacity);
				Time = 0;
			}

			public Vector3 LastSearch;
			public FixMaxSizeQueue<float> Fitness;
			public float AveFitness;
			public int Time;
		}
	}
}
