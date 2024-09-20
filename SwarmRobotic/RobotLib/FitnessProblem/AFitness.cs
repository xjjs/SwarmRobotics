using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Obstacles;
using Microsoft.Xna.Framework;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 可看作AlgorithmFitness，继承自RoboticAlgorithm，包含假目标的处理策略
    /// 属性：随机种子、合作性、是否有随机阶段、离开概率
    /// 字段：最大速度、随机种子
    /// 搜索算法的“框架”：核心方法为Update，整个群体状态更新时会调用的方法
    /// 1.Update方法用于计算某个机器人的速度向量delta并保存到NewData，考虑“假目标处理or真目标搜索”、“障碍物躲避”
    /// 机器人的状态（状态传感器为state）：Run, Collecting, Leaving, Decoy
    /// </summary>
	public abstract class AFitness : RoboticAlgorithm
	{
		protected PFitness problem;
        protected RunState instate;

        //问题类型默认为假目标处理类型
    	protected FitnessProblemType currentClassState = FitnessProblemType.DecoyProblem;
		const float PiOver6 = MathHelper.Pi / 6;

		public AFitness() { }

        //绑定到问题，根据问题属性设置最大速度、随机速度生成函数、问题类型
		public override bool Bind(RoboticProblem problem, bool changePara = true)
		{
			this.problem = problem as PFitness;
			maxspeed = problem.MaxSpeed;
			if (this.problem == null) return false;
			RandPosition = problem.SizeZ > 1 ? (Func<Vector3>)RandPosition3D : RandPosition2D;
			if (currentClassState != this.problem.ProblemType)
			{
				currentClassState = this.problem.ProblemType;
				if (changePara) CreateDefaultParameter();
			}
			return true;
		}

        //随机生成单位速度向量，在问题绑定方法Bind中确定是2D还是3D
        protected Func<Vector3> RandPosition { get; private set; }
        protected Vector3 RandPosition2D() {
            double ang = rand.NextDouble() * MathHelper.TwoPi;
            return new Vector3((float)Math.Cos(ang), (float)Math.Sin(ang), 0);
        }
        //ang1可看为与xz平面的夹角，ang2可看为与x轴的夹角
        protected Vector3 RandPosition3D() {
            double ang1 = rand.NextDouble() * MathHelper.TwoPi, ang2 = rand.NextDouble() * MathHelper.TwoPi;
            var cos = Math.Cos(ang2);
            return new Vector3((float)(Math.Cos(ang1) * cos), (float)(Math.Sin(ang1) * cos), (float)Math.Sin(ang2));
        }


        /// <summary>
        /// 计算机器人的速度向量并保存到NewData
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="state"></param>
		public override void Update(RobotBase robot, RunState state)
		{
			var r = robot as RFitness;
            r.interNum = state.Iterations;
            instate = state;
      

            //若未发现目标、发现的目标不可见、发现了不可收集的假目标，则RandomSearch或FitnessSearch
            //实际第一个条件可省略，因为PMinimal和PEnergy都在CollectTarget都已考虑该情况
			if (r.Target == null || !problem.CollectTarget(r, state as SFitness))
			{
                //邻居计算时会考虑障碍物、真假目标的可视性，故若目标不可视（上一次自己收集后还没收集完）则此次的Target字段自然为null
                //若在Collecting状态则将NewData设为Run状态：目标最后由其他机器人收集完成，本机器人仍处在收集状态
                if (r.state.SensorData == problem.statelist[1]) 
                    r.state.NewData = problem.statelist[0];

                //若满足随机搜索条件，则按角度随机生成位移增量；
                //否则考虑“假目标处理”or“真目标搜索”，计算速度向量（位移增量）
				Vector3 delta;
				if (SearchRandomly(r))
					delta = RandomSearch(r);
				else
				{
					//avoid fake target
					var d = AvoidDecoy(r);
                    //若未发现不可收集的假目标，而且没能进入Leaving状态（穿越+离开），则返回FitnessSearch向量
					delta = d.HasValue ? d.Value : FitnessSearch(r);
				}

                //添加避障分量
				//obstacle avoidance & maxspeed trim
				PostDelta(r, ref delta);
				// bounce at boundary
                
                //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
				Bounding(r.postionsystem.GlobalSensorData, ref delta);

                //存储现在的位置与适应度，用速度向量保存到NewData中
				AddHistory(r);
				r.postionsystem.NewData = delta;
			}
                //若正在收集目标或目标已收集完则清除历史记录
			else
				r.History.Clear();
		}


        //简单的避障策略：在位移增量delta方向上，所有法向偏移不超过1的障碍物生成的法向量累加（生成的避障斥力累加）
		protected void PostDelta(RFitness r, ref Vector3 delta)
		{
			var length = delta.Length();
			var ndelta = Vector3.Normalize(delta);
			var avoid = Vector3.Zero;

            //计算与障碍物的距离在某方向delta的投影，
			foreach (var ob in r.Obstacles)
			{
				var dis = ob.offset;
				var dcos = Vector3.Dot(dis, ndelta);
				var dsin = Math.Sqrt(dis.LengthSquared() - dcos * dcos);
                //若障碍物与目标方向（delta）的法向距离小于1，且在障碍物前方（切向距离<length），则累加避障向量（即法向向量）
                //斥力大小与距离成正比？
				if (dsin < 1 && dcos < length)
					avoid += ndelta * dcos - dis;
			}
            //在delta上累加所有的法向量（长度为dsin）
			delta += avoid;

            //当速度过大时才会限制（偏小则不会）
			if (delta.Length() > maxspeed)
				delta = Vector3.Normalize(delta) * maxspeed;
		}

        //位置更新后，若越界则调整位移增量，Vector3类型是值传递的
        //由不等式的推算可知，若初始值满足0<position.X<Xmax，则调整后的delta也满足0<posiion.X+delta<Xmax
		protected void Bounding(Vector3 position, ref Vector3 delta)
		{
			position += delta;
			if (position.X < 0) delta.X -= position.X * 2;
			if (position.X > problem.MapSize.X) delta.X += (problem.MapSize.X - position.X) * 2;
			if (position.Y < 0) delta.Y -= position.Y * 2;
			if (position.Y > problem.MapSize.Y) delta.Y += (problem.MapSize.Y - position.Y) * 2;

            if (position.Z < 0) delta.Z -= position.Z * 2;
            if (position.Z > problem.MapSize.Z) delta.Z += (problem.MapSize.Z - position.Z) * 2;
		}

        //广义随机搜索阶段：允许随机搜索、自身与邻居的适应度都为0，无历史记录或上一位置（第0条记录）的适应度为0
		protected internal virtual bool SearchRandomly(RFitness r)
        { return HasRandomStage && r.Fitness.SensorData == 0 && r.Fitness.NeighbourData.All(t => t == 0) 
            && (r.History.Count == 0 || r.History[0].Fitness == 0); }

		//protected internal abstract Vector3 RandomSearch(RFitness robot);
        //在随机搜索阶段生成位移增量
		protected internal virtual Vector3 RandomSearch(RFitness robot)
		{
			robot.RandomSearch = true;
            //上次的速度向量除以最大速率获取原始位移增量，若小于单位长度则重新随机生成后再乘以最大速率得到位移增量
			var last = robot.postionsystem.LastMove / maxspeed;
			if (last.Length() < 0.5) last = RandPosition();
			last *= maxspeed;
            //界定并返回位移增量（确保速度向量合法）
			Bounding(robot.postionsystem.GlobalSensorData, ref last);
			return last;
		}

        //在适应度搜索阶段生成位移增量
        protected internal abstract Vector3 FitnessSearch(RFitness robot);

        //添加历史记录，历史信息=全局位置信息+适应度信息
        //历史记录里保存的是全局坐标信息？？
		protected internal virtual void AddHistory(RFitness r) 
        { r.History.Add(r.postionsystem.GlobalSensorData, r.Fitness.SensorData); }



		protected Func<RFitness, Vector3?> AvoidDecoy;
        //在避障阶段生成位移增量
        //返回null：没有发现假目标、而且未能进入Leaving状态
		protected Vector3? AvoidDecoyCooperate(RFitness robot)
		{
            //关闭随机搜索状态
			robot.RandomSearch = false;
			//stay or leave at fake target
			bool stay = false;

            //若检测到了假目标（不可收集）
			if (robot.Target != null)
			{
				//fakelist.AddDistinct(robot.postionsystem.GlobalSensorData, robot.Fitness.SensorData);
				foreach (var nei in robot.Neighbours)
				{
                    //若已有小id的邻居处于Decoy状态则不再停留（这里又用到了机器人的统一编号信息，没有问题么？？？）
					if (nei.Target.state.SensorData == problem.statelist[3] && nei.Target.id < robot.id)
					{
						stay = false;
						break;
					}
                    //若有邻居处于Run状态或Collecting状态，则自己可能成为信标
                    //这里应该用不到Collecting状态，处在该状态的机器人根本不会进入Update中的条件；
                    //Collecting的设置是为了PMinimal中旧版本的CollectTarget（已经被注释掉了）
					stay |= nei.Target.state.SensorData == problem.statelist[0] 
                        || nei.Target.state.SensorData == problem.statelist[1];
				}
                //若自己是满足条件的信标，则不再移动
                //CollectTarget方法中在处于搜索状态的机器人发现假目标时设置，该robot即使这里不设置，当它再次进入CollectTarget时也会设置
				if (stay) return Vector3.Zero;

                //否则进入Leaving状态并随机选择一个方向最大速度离开（下次更新前若不能将robot.Target置为null，则会再次随机选择方向离开？？？）
				robot.state.NewData = problem.statelist[2];
				return RandPosition() * maxspeed;
			}


            //若未检测到假目标（不可收集）
			Vector3 temp;
			//leave nearby fake target
            //若处在Leaving状态
			if (robot.state.SensorData == problem.statelist[2])
			{
                //从这里可以看出，穿越与离开都用Leaving表示
                //若未到达穿越标记（距离假目标最近的点），则继续移动，否则清除标记后继续移动
				if (robot.LeaveCheckPoint.HasValue)
				{
					temp = robot.LeaveCheckPoint.Value - robot.postionsystem.GlobalSensorData;
					if (Vector3.Dot(temp, robot.postionsystem.LastMove) < 1e-6)
						robot.LeaveCheckPoint = null;
					return NormalOrZero(robot.postionsystem.LastMove) * maxspeed;
				}
                //若经过了穿越标记
                //若检测到更优适应度值，则清空历史并重新进入搜索状态，否则保持方向移动
				if (robot.History.Count > 0 && robot.Fitness.SensorData > robot.History[0].Fitness)
				{
					robot.state.NewData = problem.statelist[0];
					robot.History.Clear();
				}
				else
					return NormalOrZero(robot.postionsystem.LastMove) * maxspeed;
			}
            //若不在Leaving状态，则检测周围邻居是否处于信标状态（监测是否收到信标信号）
			else
			{
				Vector3 decoy = Vector3.Zero; 
              
                //检测处于Decoy状态的最近的邻居，并将下一状态设为Leaving
				foreach (var nei in robot.Neighbours)
				{
					if (nei.Target.state.SensorData == problem.statelist[3])
					{
                        //邻居的偏移向量
						temp = nei.Target.postionsystem.GlobalSensorData - robot.postionsystem.GlobalSensorData;
                        //将下一状态设为离开（收到信标信号），记录最近的信标的偏移为decoy
						if ((decoy.Length() == 0 && temp.Length() <= problem.RoboticSenseRange) || temp.Length() < decoy.Length())
						{
							robot.state.NewData = problem.statelist[2];
							decoy = temp;
						}
					}
				}

                //若下一状态为离开（收到了信标信号），则以一定概率进入穿越状态（并不是Collecting，因为没有显式地设置为1）
				if (robot.state.NewData == problem.statelist[2])
				{
                    //以一定概率进入穿越状态（稍微绕过假目标）
					if (rand.NextFloat() < pleave)
					{
                        //要求alpha为(-pi/3,-pi/6)U(pi/6,pi/3)，下面得以满足，(2 * rand.NextInt(2) - 1)要么为1要么为-1
						float alpha = (rand.NextFloat() + 1) * PiOver6 * (2 * rand.NextInt(2) - 1);
                        //将假目标向量（绕Z轴）旋转角度alpha
						temp = Vector3.Transform(decoy, Matrix.CreateRotationZ(alpha));
                        //计算穿越标记（距离假目标最近的点），返回速度向量
						robot.LeaveCheckPoint = temp + robot.postionsystem.GlobalSensorData;
						return NormalOrZero(temp) * maxspeed;
						//robot.History.Clear();
					}
					else //否则保持当前状态（Run状态、忽略假目标信息）
						robot.state.NewData = robot.state.SensorData;
				}
			}

            //若未发现不可收集的假目标，而且没能进入Leaving状态（收到信号+满足概率），则返回null
			return null;
		}
        //非合作型策略：随机选择方向离开后进入Leaving状态
		protected Vector3? AvoidDecoyNonCooperate(RFitness robot)
		{
			robot.RandomSearch = false;
			//HistoryList fakelist = robot.FakeList;
			//Vector3 temp, fake = Vector3.Zero;

            //找到假目标Target则随机离开，不对目标信息Target进行处理（UpdateSensor方法会更新其状态）
			//leave fake target
			if (robot.Target != null)
			{
				//fakelist.AddDistinct(robot.postionsystem.GlobalSensorData, robot.Fitness.SensorData);
				robot.state.NewData = problem.statelist[2];
				return RandPosition() * maxspeed;
			}
			////leave nearby fake target
			//foreach (var item in fakelist)
			//{
			//    temp = robot.postionsystem.GlobalSensorData - item.Position;
			//    if ((fake.Length() == 0 && temp.Length() <= problem.RoboticSenseRange) || temp.Length() < fake.Length())
			//    {
			//        robot.state.NewData = problem.statelist[2];
			//        fake = temp;
			//    }
			//}
            //若处于离开状态
			if (robot.state.SensorData == problem.statelist[2])
			{
                //重新找到适应度值升高的情况
				if (robot.History.Count > 0 && robot.Fitness.SensorData > robot.History[0].Fitness)
				{
					robot.state.NewData = problem.statelist[0];
					robot.History.Clear();
				}
				else
					return NormalOrZero(robot.postionsystem.LastMove) * maxspeed;
			}
			//fakelist.Clear();
			return null;
		}
        //可空修饰符？是对值类型而言的
		protected Vector3? NoAvoidDecoy(RFitness robot) { return null; }

		public override void Reset()
		{
			if (seed == -1)
				rand = new CustomRandom();
			else
				rand = new CustomRandom(seed);
		}

        //单位化（值太小则保持不变）
		protected static Vector3 NormalOrZero(Vector3 vec)
		{
			if (vec.Length() > 0.01f)
				return Vector3.Normalize(vec);
			else
				return vec;	//Vector3.Zero
		}

        //单位化（太小则重新选取后再单位化）
        protected Vector3 NormalOrRandom(Vector3 vec) 
        {
            while (vec.Length() < 0.01f)
            {
                vec = RandPosition();
            }
            return Vector3.Normalize(vec);
        }



		public override void InitializeParameter()
		{
			if (seed == -1)
				rand = new CustomRandom();
			else
				rand = new CustomRandom(seed);
			if (currentClassState == FitnessProblemType.DecoyProblem)
				AvoidDecoy = Cooperate ? (Func<RFitness, Vector3?>)AvoidDecoyCooperate : AvoidDecoyNonCooperate;
			else
				AvoidDecoy = NoAvoidDecoy;
		}

		public override void CreateDefaultParameter()
		{
			seed = -1;
			Cooperate = false;
			pleave = 0.9f;
			HasRandomStage = false;
		}

		protected float maxspeed;
		protected CustomRandom rand;
		int seed;
		float pleave;

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

		[Parameter(ParameterType.Boolean, Description = "Cooperate Avoid")]
		public bool Cooperate { get; set; }

		[Parameter(ParameterType.Float, Description = "Leave Possibility")]
		public float PLeave
		{
			get { return pleave; }
			set
			{
				if (value < 0 || value > 1) throw new Exception("Must be within [0,1]");
				pleave = value;
			}
		}

		[Parameter(ParameterType.Boolean, Description = "Has Random Stage")]
		public bool HasRandomStage { get; set; }

		public override string GetName { get { return base.GetName.Substring(1, base.GetName.Length - 8); } }

	}
}

//确定stay标志的地方，应该用不到Collecting状态，处在该状态的机器人根本不会进入Update中的条件，Collecting的设置是为了PMinimal中旧版本的CollectTarget（已经被注释掉了）