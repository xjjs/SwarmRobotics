using System;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;
using System.Linq;
using System.Collections.Generic;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自PMinimal，影响范围与目标适应度有关，范围适应度与距离有关
    /// 适应度与干扰值是距离的连续函数，没有离散化处理，故不需要生成包络网结点的CreateEnvironment函数
    /// </summary>
	public class PMinimalDis : PMinimal
	{
        //设置圆心半径
		const int TargetRadius = FitnessRadius / 2;

		public PMinimalDis() { }

		protected override void CreateEnvironment(RunState state, FitnessTarget[] targets, Interference[] interfereces)
		{
		}

        //进一步设置一般障碍物的感知范围：真假目标（干扰源）的感知范围与适应度值（干扰级别）有关
		protected override void SetTargets(RoboticEnvironment env)
		{
			base.SetTargets(env);
			foreach (FitnessTarget tar in env.ObstacleClusters[1].obstacles.Concat(env.ObstacleClusters[2].obstacles))
				tar.SenseRange = tar.Energy * FitnessRadius;
			if (InterferenceNum > 0)
			{
				foreach (Interference inter in env.ObstacleClusters[3].obstacles)
					inter.SenseRange = inter.Level * FitnessRadius;
			}
		}

        //重写顶层基类RobotProblem的方法，更新传感器信息，更新适应度值信息或目标检测信息
		public override void UpdateSensor(RobotBase robot, RunState state)
		{
			var r = robot as RFitness;
            //求取真假目标的最大适应度值并设置为NewData
			r.Fitness.NewData = r.mapsensor[1].Concat(r.mapsensor[2]).Select(CalculateFitness).Aggregate(0, Math.Max);
			if (InterferenceNum > 0)
			{
                //语句Lambda若只有一个参数且传入已定义的函数中，则可以只用函数名
				r.Fitness.NewData -= r.mapsensor[3].Select(CalculateInterference).Aggregate(0, Math.Max);
				if (r.Fitness.NewData < 0) r.Fitness.NewData = 0;
			}
            //StateSensor中的ApplyChange()是用NewData更新SensorData
			r.Fitness.ApplyChange();

            //目标感知半径
			float dis = TargetRadius;
			r.Target = null;
            //机器人在目标感知半径内则设置目标，若在多个目标的感知范围内则选取最近的目标
            //注意：即使目标在机器人的感知范围内，机器人也不一定发现目标，机器人必须在目标的感知范围内才能感知到目标的位置
			foreach (var tar in r.mapsensor[1].Concat(r.mapsensor[2]))//.Where(on => on.isNeighbour))
			{
				if (dis > tar.distance)
				{
					dis = tar.distance;
					r.Target = tar.Target;
				}
			}
		}

        //计算适应度值：与距离成反比
		int CalculateFitness(NeighbourData<Obstacle> nd) 
        { return (nd.Target as FitnessTarget).Energy - (int)Math.Ceiling(nd.distance / FitnessRadius); }

        //计算干扰强度：与距离成反比
		int CalculateInterference(NeighbourData<Obstacle> nd) 
        { return (nd.Target as Interference).Level - (int)Math.Ceiling(nd.distance / FitnessRadius); }
	}
}
