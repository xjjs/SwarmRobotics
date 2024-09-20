using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自AFitness的GES算法：
    /// 算法属性：分组大小、排斥权重、缩放因子
    /// 私有字段：组尺寸、缩放因子数组
    /// 未显式地进行：NormalOrZero(delta) * maxspeed，故可能有问题？？？
    /// </summary>
	public class AGESFitness : AFitness
	{
		int splitSize;
		//float Radius60;
		float[] shrinkRanges;

        //基类构造器默认调用CreateDefaultParameter方法
		public AGESFitness() { /*Radius60 = MathHelper.PiOver2 * 2 / 3;*/ }

		//protected internal override bool SearchRandomly(RFitness r) { return false; }

        //机器人i的适应度搜索算法
		protected internal override Vector3 FitnessSearch(RFitness robot)
		{
 //           HasRandomStage = true;
			robot.RandomSearch = false;
			int max;
			//float tag = robotic.Tag;
			Vector3 delta, maxpos, history;
			RFitness R;

            //暂存当前位置与适应度为最优位置，然后用历史记录更新
			maxpos = robot.postionsystem.GlobalSensorData;
			max = robot.Fitness.SensorData;
			foreach (var his in robot.History)
			{
				if (max < his.Fitness)
				{
					max = his.Fitness;
					maxpos = his.Position;
				}
			}
            //计算位移增量delta（速度）的历史部分history（过大则单位化、小则不变）
			history = NormalOrZero(maxpos - robot.postionsystem.GlobalSensorData);

			
            //var neighbours = robot.Neighbours.Select(n => n.Target as RFitness)/*.Where(r => r.state.SensorData == "Run")*/.ToList();
			//小组过大则拆分，
            if (robot.Neighbours.Count >= splitSize)
			{
				//split
				//neighbours.Add(robotic);
				//neighbours.Sort();	//descending
				RFitness L1, L2 = robot.Neighbours[0].Target as RFitness;

               
                //注意此处考虑群体中每个机器人的统一编号——要求机器人有编号识别能力？？？
                //此处要选出两个适应度最大的个体L1与L2，但CompareTo的作用是返回较小者，所以需要改正？？？组内适应度差别最大为1，影响应该不大
				if (robot.CompareTo(L2) > 0)
					L1 = robot;
				else
				{
					L1 = L2;
					L2 = robot;
				}
				foreach (var r in robot.Neighbours)
				{
					R = r.Target as RFitness;
					if (R.CompareTo(L1) > 0)
					{
						L2 = L1;
						L1 = R;
					}
					else if (R.CompareTo(L2) > 0)
						L2 = R;
				}
                //计算分裂向量，根据robot是否为leader而选择速度更新公式
				maxpos = NormalOrZero(L1.postionsystem.GlobalSensorData - L2.postionsystem.GlobalSensorData) * rw;
				if (L1 == robot) //if (neighbours[0] == robotic)
					delta = DownHill(robot) + maxpos;
					//delta = maxpos;
				else if (L2 == robot) //else if (neighbours[1] == robotic)
					delta = DownHill(robot) - maxpos;
					//delta = -maxpos;
				else
				{
					double rn = rand.NextDouble(), th = 1d / (L1.Fitness.SensorData - L2.Fitness.SensorData + 2);
                    //L1与L2的适应度值若相等（差值为0）则以1/2概率跟随L2，否则（差值为1）以1/3概率跟随L2
					if (rn < th)	//follow L2
						delta = NormalOrZero(L2.postionsystem.GlobalSensorData - robot.postionsystem.GlobalSensorData) * ShrinkRand() - maxpos;
					else
						delta = NormalOrZero(L1.postionsystem.GlobalSensorData - robot.postionsystem.GlobalSensorData) * ShrinkRand() + maxpos;
				}
                //累加上历史部分，比例因子取值空间为{0.4,0.6,0.8}，为什么总是定义些离散的参数空间呢？？？
				delta += history * (rand.NextInt(3) + 2) / 5f;
			}
			else
			{
				Vector3 center = robot.postionsystem.GlobalSensorData;
				maxpos = center;
				max = robot.Fitness.SensorData;
				foreach (var r in robot.Neighbours)
				{
					R = r.Target as RFitness;
					center += R.postionsystem.GlobalSensorData;

					if (R.Fitness.SensorData > max)
					{
						max = R.Fitness.SensorData;
						maxpos = R.postionsystem.GlobalSensorData;
					}
				}
				center /= robot.Neighbours.Count + 1;
				maxpos -= center;

                //若本身最优，且是被用来计算组分量的个体，则只利用自身信息进行更新
				if (max == robot.Fitness.SensorData && maxpos.Length() < 0.1f)
				{
					if (history.Length() <= 0.1f)
						delta = DownHill(robot);
					else
						delta = history + RandPosition() / 10;
				}
				else
					delta = history * (rand.NextInt(3) + 2) / 5f + NormalOrZero(maxpos) * ShrinkRand();
			}

			//if (tag != 0) robotic.Tag = 0;

            //未用下面的语句显式地进行：NormalOrZero(delta) * maxspeed，故可能有问题？？？（调试可得其可能超速）
            /*
            if (delta.Length() > 1f)
            {
                delta = NormalOrZero(delta);
            }
             * */
			return delta * maxspeed;
		}

        //惯性速度(适应度地图上看类似下山)：上次速度（位移增量）够大则单位化返回，否则重新生成单位速度（位移增量）并返回
		Vector3 DownHill(RFitness robotic)
		{
			Vector3 last = robotic.postionsystem.LastMove;
           
			if (last.Length() < 0.1f)
				last = RandPosition();
			else
				last = Vector3.Normalize(last);
			//if (robotic.Tag != 0)
			//{
			//    if (robotic.Fitness.SensorData > robotic.History[0].Fitness)
			//        return last;
			//    else
			//        return Vector3.Transform(last, Matrix.CreateRotationZ(-robotic.Tag * 2));
			//}
			//else if (robotic.History.Count > 0)
			//{
			//    if (robotic.Fitness.SensorData < robotic.History[0].Fitness)
			//    {
			//        float angle = (float)(rand.NextDouble() * Radius60 + MathHelper.PiOver2);
			//        angle *= (rand.Next(2) << 1) - 1;
			//        robotic.Tag = angle;
			//        return Vector3.Transform(last, Matrix.CreateRotationZ(angle));
			//    }
			//    else
			//        return last;
			//}
			//else
				return last;
		}
        //缩放因子由因子数组随机生成
		float ShrinkRand() { return shrinkRanges[rand.NextInt(3)]; }



		public override void InitializeParameter()
		{
			base.InitializeParameter();
			splitSize = size - 1;
			shrinkRanges = new float[] { 1 - sr, 1, 1 + sr };
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			size = 6;
			rw = 0.88f;
			sr = 0.27f;
		}

		int size;
		float rw, sr;

		[Parameter(ParameterType.Int, Description = "Split Sub-Swarm Size")]
		public int SSize
		{
			get { return size; }
			set
			{
				if (value < 4) throw new Exception("Must be at least 4");
				size = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Repluse Weight")]
		public float RWeight
		{
			get { return rw; }
			set
			{
				if (value <= 0 || value > 1) throw new Exception("Must be in (0,1]");
				rw = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Shrink Range")]
		public float SRange
		{
			get { return sr; }
			set
			{
				if (value <= 0 || value >= 1) throw new Exception("Must be in (0,1)");
				sr = value;
			}
		}
	}
}

//？？？分组时leader的选择有问题