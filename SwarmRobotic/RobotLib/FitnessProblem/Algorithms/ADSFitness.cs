using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UtilityProject;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 求解能量限制条件下目标搜索的算法
    /// </summary>
	public class ADSFitness : AFitness
	{
		int step, numOfSteps, angle, _alpha, group, _group, maxtimeR, maxtimeF;
		int dirs, totaldirs, quad1, quad2, quad3, quad2m1;
		Matrix m1, m2;
		float xMax, yMax, balance, br;
		const float PiOver3 = MathHelper.Pi / 3, TwoPiOver3 = 2 * PiOver3, sigma = PiOver3 / 2.58f;
		Func<RFitness, Vector3> FitnessFunc;

		public ADSFitness() : base() { }

		public override void InitializeParameter()
		{
			base.InitializeParameter();
			m1 = Matrix.CreateRotationZ(MathHelper.ToRadians(angle));
			m2 = Matrix.CreateRotationZ(-MathHelper.ToRadians(angle));
			group = _group - 1;
			xMax = problem.MapSize.X - maxspeed;
			yMax = problem.MapSize.Y - maxspeed;
			balance = br * problem.RoboticSenseRange;
			numOfSteps = (int)(problem.RoboticSenseRange / step + 0.5f);
			FitnessFunc = RandomFit ? (Func<RFitness, Vector3>)RandomFitnessSearch : NormalFitnessSearch;
			quad1 = dirs;
			quad2 = dirs + dirs;
			quad2m1 = quad2 - 1;
			quad3 = quad1 + quad2;
			totaldirs = quad2 + quad2;
		}

		public override object CreateCustomData() { return new TagDS(totaldirs, problem.Population - 1); }

		public override void ClearCustomData(object data)
		{
			var Tag = data as TagDS;
			Tag.Angle = -1;
			Tag.Time = 0;
			Tag.lastDirection = Vector3.Zero;
		}

		protected internal override bool SearchRandomly(RFitness robot) { return (robot.AlgorithmData as TagDS).Angle >= -1 && base.SearchRandomly(robot); }

		void SetAngle(Vector3 vec, float[] Lengths)
		{
			var length = vec.Length();
			//if (length < step) length = step;
			int angle = (int)MathHelper.Clamp(totaldirs * (float)Math.Atan2(vec.Y, vec.X) / MathHelper.TwoPi + 0.5f, 0f, totaldirs);
			if (Lengths[angle] == 0 || Lengths[angle] > length)
				Lengths[angle] = length;
		}

		void SetAngle(TagDS tag)
		{
			float length;
			int width;
			for (int a = 0; a < totaldirs; a++)
			{
				length = tag.Lengths[a];
				if (length == 0) continue;
				width = (int)((problem.RoboticSenseRange - length) / step);
				if (width < 1) width = 1;
				else if (width > quad2m1) width = quad2m1;
				int angle = a - width + 1;
				if (angle < 0) angle += totaldirs;
				for (int i = 1; i < width; i++)
				{
					tag.Possibilities[angle] -= i;
					if (++angle == totaldirs) angle = 0;
				}
				for (int i = width; i > 0; i--)
				{
					tag.Possibilities[angle] -= i;
					if (++angle == totaldirs) angle = 0;
				}
			}
		}

		protected internal override Vector3 RandomSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as TagDS;
			if (!robotic.RandomSearch)
			{
				Tag.Time = 0;
				robotic.RandomSearch = true;
			}
			var pos = robotic.postionsystem.GlobalSensorData;
			Array.Clear(Tag.Possibilities, 0, totaldirs);

			//Bounding
			//TODO Bounce at border
			if (pos.X < maxspeed)
			{
				for (int i = quad1; i <= quad3; i++)
					Tag.Possibilities[i] = -100;
			}
			else if (pos.X > xMax)
			{
				for (int i = 0; i <= quad1; i++)
					Tag.Possibilities[i] = -100;
				for (int i = quad3; i < totaldirs; i++)
					Tag.Possibilities[i] = -100;
			}
			if (pos.Y < maxspeed)
			{
				for (int i = quad2; i < totaldirs; i++)
					Tag.Possibilities[i] = -100;
				Tag.Possibilities[0] = -100;
			}
			else if (pos.Y > yMax)
			{
				for (int i = 0; i <= quad2; i++)
					Tag.Possibilities[i] = -100;
			}

			Vector3 delta = Vector3.Zero, tmp;
			Tag.sortList.Clear();
			foreach (var item in robotic.Neighbours)
				Tag.sortList.Add(item.distance, item);
			foreach (var item in Tag.sortList.Values.Take(group))
			{
				if (item.distance > 0.1f)
					tmp = (item.distance - balance) * item.offset / item.distance;
				else
					tmp = -item.offset;
				if (tmp.Length() < 1)
					delta += tmp;
				else
					delta += Vector3.Normalize(tmp);
			}
			if (Tag.Angle >= 0 && Tag.Time > 0 && Tag.Possibilities[Tag.Angle] == 0)
				Tag.Time--;
			else
			{
				Array.Clear(Tag.Lengths, 0, totaldirs);
				foreach (var his in robotic.History)
				{
					if (his.Fitness == 0) SetAngle(his.Position - pos, Tag.Lengths);
				}
				int count = 0;
				foreach (var item in Tag.sortList.Values)
				{
					SetAngle(item.offset, Tag.Lengths);
					if (count++ < group) SetAngle(-item.offset, Tag.Lengths);
				}
				SetAngle(Tag);

				if (Tag.Angle >= 0 && Tag.Possibilities[Tag.Angle] >= _alpha)
					Tag.Time = maxtimeR;
				else
				{
					int sum = 0, cur, rnd, max = Tag.Possibilities[0], maxangle = 0;
					for (int i = 0; i < totaldirs; i++)
					{
						cur = Tag.Possibilities[i];
						if (cur > max)
						{
							max = cur;
							maxangle = i;
						}
						cur += numOfSteps;
						if (cur < 0) cur = 0;
						sum += cur;
						Tag.Possibilities[i] = sum;
					}

					if (sum == 0)
						cur = maxangle;
					else
					{
						rnd = rand.NextInt(sum);
						cur = Array.BinarySearch(Tag.Possibilities, rnd);
						if (cur < 0)
							cur = ~cur;
						else if (rnd == 0)
							while (Tag.Possibilities[cur] == rnd)
								cur++;
					}
					Tag.Angle = cur;
					Tag.Time = maxtimeR;
					float angle = MathHelper.TwoPi * cur / totaldirs;
					Tag.lastDirection = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * maxspeed;
				}
			}
			delta += Tag.lastDirection;
			if (delta.Length() < 0.1f)
				delta = RandPosition() * maxspeed;
			else
				delta = Vector3.Normalize(delta) * maxspeed;
			//if (group > 0 && sortList.Size > 0)
			//{
			//    Bounding(pos, ref delta);
			//    Tag.Angle = (int)MathHelper.Clamp(32 * (float)Math.Atan2(delta.Y, delta.X) / MathHelper.TwoPi + 0.5f, 0f, 32f);
			//}
			return delta;
		}

		protected internal override Vector3 FitnessSearch(RFitness robotic) { return FitnessFunc(robotic); }

		Vector3 RandomFitnessSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as TagDS;
			if (robotic.RandomSearch)
			{
				Tag.Time = 0;
				robotic.RandomSearch = false;
			}

			var pos = robotic.postionsystem;
			var Fitness = robotic.Fitness.SensorData;
			var lastFitness = robotic.History.Count == 0 ? 0 : robotic.History[0].Fitness;
			int p1, p2;
			Vector3 delta, vec, last = pos.LastMove;
			RFitness r;

			if (last.Length() < 0.1)
				last = RandPosition();
			else
				last /= maxspeed;
			if (Fitness >= lastFitness || lastFitness == 5)
			{
				Tag.Angle = -1;
				Tag.Time = 0;
				delta = last;

				p1 = p2 = 0;
				foreach (var his in robotic.History)
					if (his.Fitness > p1) p1 = his.Fitness;
				foreach (var nei in robotic.Neighbours)
				{
					r = nei.Target as RFitness;
					if (r.Fitness.SensorData > p2) p2 = r.Fitness.SensorData;
				}
				//p1 = robotic.History.Count == 0 ? 0 : robotic.History.Max(h => h.Fitness);
				//p2 = robotic.Neighbours.Max(n => (n.Robotic as REnergy).Fitness.SensorData);
				if (p1 >= p2 && p1 > Fitness)
				{
					foreach (var his in robotic.History)
					{
						if (his.Fitness != p1) continue;
						vec = his.Position - pos.GlobalSensorData;
						if (vec.Length() < 0.1f) continue;
						delta += Vector3.Normalize(vec);
					}
				}
				if (p2 >= p1 && p2 > Fitness)
				{
					foreach (var nei in robotic.Neighbours)
					{
						r = nei.Target as RFitness;
						if (r.Fitness.SensorData != p2) continue;
						vec = r.postionsystem.GlobalSensorData - pos.GlobalSensorData;
						if (vec.Length() < 0.1f) continue;
						delta += Vector3.Normalize(vec);
					}
				}
			}
			else if (lastFitness - Fitness > 1)
			{
				robotic.History.Clear(1);
				return FitnessSearch(robotic);
			}
			else if (Tag.Angle < -1)
			{
				Tag.Time--;
				if (Tag.Time > 0)
					delta = last;
				else
				{
					if (Tag.Angle == -2)
						delta = Vector3.Transform(Vector3.Transform(last, m2), m2);
					else	//-3
						delta = Vector3.Transform(Vector3.Transform(last, m1), m1);
					Tag.Angle = -1;
				}
			}
			else
			{
				delta = -last;
				p1 = rand.NextInt(2) * 2 - 1;
				Turn(ref delta, p1 > 0, Tag);
				Tag.Time = maxtimeF;
			}

			if (delta.Length() > 0.1f) delta.Normalize();
			delta *= maxspeed;
			return delta;
		}

		Vector3 NormalFitnessSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as TagDS;
			if (robotic.RandomSearch)
			{
				Tag.Time = 0;
				robotic.RandomSearch = false;
			}

			float tmp;
			var pos = robotic.postionsystem;
			var Fitness = robotic.Fitness.SensorData;
			var lastFitness = robotic.History.Count == 0 ? 0 : robotic.History[0].Fitness;
			int p1, p2;
			Vector3 delta, vec, last = pos.LastMove;
			RFitness r;

			if (last.Length() < 0.1)
				last = RandPosition();
			else
				last /= maxspeed;
			if (Fitness >= lastFitness || lastFitness == 5)
			{
				Tag.Angle = -1;
				Tag.Time = 0;
				delta = last;

				p1 = p2 = 0;
				foreach (var his in robotic.History)
					if (his.Fitness > p1) p1 = his.Fitness;
				foreach (var nei in robotic.Neighbours)
				{
					r = nei.Target as RFitness;
					if (r.Fitness.SensorData > p2) p2 = r.Fitness.SensorData;
				}
				//p1 = robotic.History.Count == 0 ? 0 : robotic.History.Max(h => h.Fitness);
				//p2 = robotic.Neighbours.Max(n => (n.Robotic as REnergy).Fitness.SensorData);
				if (p1 >= p2 && p1 > Fitness)
				{
					foreach (var his in robotic.History)
					{
						if (his.Fitness != p1) continue;
						vec = his.Position - pos.GlobalSensorData;
						if (vec.Length() < 0.1f) continue;
						delta += Vector3.Normalize(vec);
					}
				}
				if (p2 >= p1 && p2 > Fitness)
				{
					foreach (var nei in robotic.Neighbours)
					{
						r = nei.Target as RFitness;
						if (r.Fitness.SensorData != p2) continue;
						vec = r.postionsystem.GlobalSensorData - pos.GlobalSensorData;
						if (vec.Length() < 0.1f) continue;
						delta += Vector3.Normalize(vec);
					}
				}
			}
			else if (lastFitness - Fitness > 1)
			{
				robotic.History.Clear(1);
				return FitnessSearch(robotic);
			}
			else if (Tag.Angle < -1)
			{
				Tag.Time--;
				if (Tag.Time > 0)
					delta = last;
				else
				{
					if (Tag.Angle == -2)
						delta = Vector3.Transform(Vector3.Transform(last, m2), m2);
					else	//-3
						delta = Vector3.Transform(Vector3.Transform(last, m1), m1);
					Tag.Angle = -1;
				}
			}
			else
			{
				delta = -last;
				p1 = p2 = 0;
				foreach (var his in robotic.History)
				{
					vec = his.Position - pos.GlobalSensorData;
					tmp = Vector3.Cross(last, vec).Z;
					if (Math.Abs(tmp) < 0.1f) continue;
					if ((tmp > 0) == (his.Fitness >= Fitness))
						p1++;
					else
						p2++;
				}
				foreach (var nei in robotic.Neighbours)
				{
					r = nei.Target as RFitness;
					vec = r.postionsystem.GlobalSensorData - pos.GlobalSensorData;
					tmp = Vector3.Cross(last, vec).Z;
					if (Math.Abs(tmp) < 0.1f) continue;
					if ((tmp > 0) == (r.Fitness.SensorData >= Fitness))
						p1++;
					else
						p2++;
				}
				if (p1 == p2) p1 += rand.NextInt(2) * 2 - 1;
				Turn(ref delta, p1 > p2, Tag);
				Tag.Time = maxtimeF;
			}

			if (delta.Length() > 0.1f) delta.Normalize();
			delta *= maxspeed;
			return delta;
		}

		void Turn(ref Vector3 vec, bool direction, TagDS tag)
		{
			Matrix tran;
			if (angle == 0)
			{
				var degree = (direction ? TwoPiOver3 : -TwoPiOver3) + MathHelper.Clamp(rand.NextGaussianFloat() * sigma, -PiOver3, PiOver3);
				tran = Matrix.CreateRotationZ(degree);
				tag.backTurn = Matrix.CreateRotationZ(-degree);
				tag.Angle = direction ? -2 : -3;
			}
			else if (direction)
			{
				tran = m1;
				tag.backTurn = m2;
				tag.Angle = -2;
			}
			else
			{
				tran = m2;
				tag.backTurn = m1;
				tag.Angle = -3;
			}
			vec = Vector3.Transform(vec, tran);
		}

		protected internal override void AddHistory(RFitness r)
		{
			if (r.Fitness.SensorData == 5)
			{
				//r.History.Clear(1);
				return;
			}
			if (r.RandomSearch && r.History.Count > 0 && r.History[0].Fitness == r.Fitness.SensorData)
			{
				//for (int i = 0; i < r.History.Count; i++)
				//{
				//    var his = r.History[i];
				foreach (var his in r.History)
				{
					if (his.Fitness == r.Fitness.SensorData || Vector3.Distance(his.Position, r.postionsystem.GlobalSensorData) < step)
					{
						//history.RemoveAt(i);
						//break;
						//i--;
						return;
					}
				}
			}
			base.AddHistory(r);
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			angle = 60;// 60;//53 //60
			step = 10;	// 5;// 3;	//10 //4	//5
			_alpha = -7;	//-5;// 1; //0 //0	//-7
			_group = 1;
			br = 0.9f;
			maxtimeR = 4;
			maxtimeF = 2;
			RandomFit = false;
			dirs = 50;
		}

		[Parameter(ParameterType.Int, Description = "Angle")]
		public int Angle
		{
			get { return angle; }
			set
			{
				if (value != 0 && (value < 30 || value > 90)) throw new Exception("Must be 0(Random) or within [30,90]");
				angle = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Alpha")]
		public int Alpha
		{
			get { return _alpha; }
			set
			{
				if (value < -20 || value > 10) throw new Exception("Must be in [-20,10]");
				_alpha = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Step")]
		public int Step
		{
			get { return step; }
			set
			{
				if (value < 1 || value > 10) throw new Exception("Must be in [1,10]");
				step = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Group Size")]
		public int Group
		{
			get { return _group; }
			set
			{
				if (value < 1 || value > 10) throw new Exception("Must be in [1,10]");
				_group = value;
			}
		}

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

		[Parameter(ParameterType.Int, Description = "Iteration Random")]
		public int TimeR
		{
			get { return maxtimeR; }
			set
			{
				if (value < 0 || value > 10) throw new Exception("Must be within [0,10]");
				maxtimeR = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Iteration Fitness")]
		public int TimeF
		{
			get { return maxtimeF; }
			set
			{
				if (value < 0 || value > 5) throw new Exception("Must be within [0,5]");
				maxtimeF = value;
			}
		}

		[Parameter(ParameterType.Boolean, Description = "Random Fitness")]
		public bool RandomFit { get; set; }

		[Parameter(ParameterType.Int, Description = "Directions (1/4)")]
		public int Directions
		{
			get { return dirs; }
			set
			{
				if (value < 1 || value > 100) throw new Exception("Must be within [1,100]");
				dirs = value;
			}
		}

		class TagDS
		{
			public TagDS(int size, int capacity)
			{
				lastDirection = Vector3.Zero;
				Possibilities = new int[size];
				Lengths = new float[size];
				sortList = new SortedDistanceList<NeighbourData<RobotBase>>(capacity);
			}

			public int[] Possibilities;
			public float[] Lengths;
			public SortedDistanceList<NeighbourData<RobotBase>> sortList;
			public int Time = 0, Angle = -1;
			public Matrix backTurn;
			public Vector3 lastDirection;
		}
	}
}
