using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自AFitness的IGES算法：
    /// 算法属性：分组大小
    /// 四种策略：1径向分散、2移向群体最优位置重心、3惯性移动、4移向历史最优位置重心
    /// 显式地考虑了：NormalOrZero(delta) * maxspeed ==> 始终以最大速度移动
    /// </summary>
	public class AIGESFitness : AFitness
	{
		public AIGESFitness() { /*Radius60 = MathHelper.PiOver2 * 2 / 3;*/ }

		//protected internal override bool SearchRandomly(RFitness r) { return false; }

		protected internal override Vector3 FitnessSearch(RFitness robot)
		{
			robot.RandomSearch = false;
			Vector3 delta, history, maxpos = Vector3.Zero, center = Vector3.Zero, pos;
			int max = robot.Fitness.SensorData, maxcount = 1, count = 1, fit;

            //不必再次赋值
			//max = robot.Fitness.SensorData;

            //累加所有位置偏移与最优位置偏移，计算群重心
			foreach (var r in robot.Neighbours)
			{
                //若有邻居处于收集状态，则直接返回邻居偏移（即利用局部相对坐标移向该地点）
				if (r.Target.state.SensorData == problem.statelist[1]) return r.offset;
                //累加所有位置坐标（偏移）与最优位置坐标（偏移）
				count++;
				pos = r.offset;
				fit = (r.Target as RFitness).Fitness.SensorData;
				center += pos;
				if (fit > max)
				{
					max = fit;
					maxpos = pos;
					maxcount = 1;
				}
				else if (fit == max)
				{
					maxpos += pos;
					maxcount++;
				}
			}
			center /= count;
            
            //若组内个体适应度不全相同
			if (count > maxcount)
			{
                //策略2
				delta = NormalOrZero(maxpos / maxcount - center) + RandPosition() / 10;// +history;
                //若超过尺寸，再附加策略1
				if (count >= size) delta -= NormalOrZero(center);
			}
            //若组内个体适应度相同，则策略1
			else if (count > 1)
			{
				delta = RandPosition() / 10 - NormalOrZero(center);
			}
            //只有一个个体则考察历史记录
			else
			{
				maxpos = robot.postionsystem.GlobalSensorData;
				max = robot.Fitness.SensorData;
				maxcount = 1;
				foreach (var his in robot.History)
				{
					if (max < his.Fitness)
					{
						max = his.Fitness;
						maxpos = his.Position;
						maxcount = 1;
					}
					else if (max == his.Fitness)
					{
						maxpos += his.Position;
						maxcount++;
					}
				}
                //若当前适应度历史最优则忽略历史影响，否则计算历史最优位置的重心偏移
				if (robot.Fitness.SensorData == max)
					history = Vector3.Zero;
				else
					history = NormalOrZero(maxpos / maxcount - robot.postionsystem.GlobalSensorData);

                //若历史记录有多条且当前位置差于第0条（上一位置）or上次进步太小，则策略4+大的随机向量
				if ((robot.History.Count > 0 && robot.Fitness.SensorData < robot.History[0].Fitness) || robot.postionsystem.LastMove.Length() < 0.1f)
					delta = RandPosition() + history;
                    //若存在历史更优，则策略4
				else if (history.Length() > 0.1f)
					delta = history + RandPosition() / 10;
                    //若本身历史最优，则策略3（保持原方向移动）
				else
					delta = NormalOrZero(robot.postionsystem.LastMove);
			}

            //始终以最大速度运动
			return NormalOrZero(delta) * maxspeed;
		}


        //小组尺寸默认为5
		int size;
		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			size = 5;
		}
		
		[Parameter(ParameterType.Int, Description = "Split Sub-Swarm Size")]
		public int SSize
		{
			get { return size; }
			set
			{
				if (value < 2) throw new Exception("Must be at least 2");
				size = value;
			}
		}
	}
}
