using System;
using Microsoft.Xna.Framework;

namespace RobotLib.FitnessProblem
{
    //原始的RPSO的效率较差，于是参考PSOE做了两处改进：
    //改进1：只有历史更优时引入随机分量以防止局部循环振荡
    //改进2：保证个体的速度，在没有指引信息的情况下保证“惯性速度”接近最大值
    //若关闭HasRandomState（并行仿真时常用的设置），则算法会在无适应度值区域随机漂浮，虽然算法一开始有考虑惯性分量last，
    //但只有当其非常小（小于0.1f）时才会重置，从而允许大量低速粒子的存在(如1大于0.1f)，不能保证“惯性速度”
	public class ARPSOFitness : AFitness
	{
		public ARPSOFitness() { }

		protected internal override Vector3 FitnessSearch(RFitness robot) {

            robot.RandomSearch = false;
            if (robot.RandomSearch)
            {
                bool hasN = false;
                var pos = robot.postionsystem;
                var Fitness = robot.Fitness.SensorData;
                int cmp;
                Vector3 delta, vec, last = pos.LastMove;

                //规范化惯性方向
                if (last.Length() < 0.1f)
                    last = RandPosition() * maxspeed;
                //delta = w * pos.LastMove + c1 * RandPosition() + c2 * RandPosition();
                delta = w * last;

                //寻找历史最优
                ////////////self
                int max = -1;
                Vector3 maxpos = Vector3.Zero;
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
                        delta += c1 * cmp * vec * (float)rand.NextDouble();

                        //改进1：仅有更优历史不行，必须没有更优邻域时才引入随机分量
                        //                    hasN = true;
                    }
                }
                ///////////neighbour
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
                        delta += c2 * cmp * vec * (float)rand.NextDouble();
                        hasN = true;
                    }
                }


                //若没有指导性信息（历史更优or邻域更优）则附加随机分量
                if (!hasN)
                {
                    //改进2，保证惯性移动的速度
                    if (delta.Length() > 0.1f)
                        delta = Vector3.Normalize(delta) * 0.8f + RandPosition() * (1 - 0.8f);
                    else
                        delta = RandPosition();
                    delta *= maxspeed;

                    //不能避免低速情形（即原来的惯性速度可能是1或2，不能保证惯性速度）
                    //delta += (1 - w) * RandPosition() * maxspeed;
                }
                //断点下面代码易检测其速度并不符合要求（虽然避障阶段会再次分析）

                //if (delta.Length() < 0.1f)
                //{
                //    delta = NormalOrZero(delta) * maxspeed;
                //}

                return delta;
            }
            else
            {
                bool hasN = false;
                Vector3 delta;
                delta = w * robot.postionsystem.LastMove;


                //寻找历史最优
                ////////////self
                int max = robot.Fitness.SensorData;
                Vector3 maxpos = Vector3.Zero;
                foreach (var item in robot.History)
                {
                    if (item.Fitness > max)
                    {
                        max = item.Fitness;
                        maxpos = item.Position;
                    }
                }
                if (max > robot.Fitness.SensorData)
                {
                    delta += C1 * (float)rand.NextDouble() * (maxpos - robot.postionsystem.GlobalSensorData);
                }

                ///////////neighbour
                max = robot.Fitness.SensorData;
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
                if (max > robot.Fitness.SensorData)
                {
                    delta += C2 * (float)rand.NextDouble() * (maxpos - robot.postionsystem.GlobalSensorData);
                    hasN = true;
                }


                //在没有邻居信息指导的情况下，要添加随机分量
                if (!hasN)
                {
                    if(delta.Length() != 0)
                        delta = Vector3.Normalize(delta) * (1-C3) + C3 * RandPosition();
                }

                while (delta.Length() < 0.1f) 
                    delta = RandPosition();
                return Vector3.Normalize(delta) * maxspeed;
            }

        }

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();

			c1 = 1.0f;
			c2 = 2.0f;
            c3 = 0.1f;
            w = 3.0f;
		}

		float w, c1, c2,c3;

		[Parameter(ParameterType.Float, Description = "w")]
		public float W
		{
			get { return w; }
			set
			{
				if (value < 0 || value >= 5) throw new Exception("Must be in [0,5)");
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

        [Parameter(ParameterType.Float, Description = "c3")]
        public float C3 {
            get { return c3; }
            set {
                if (value < 0) throw new Exception("Must be in positive");
                c3 = value;
            }
        }
	}
}
