using System;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;
using System.Linq;
using System.Collections.Generic;


namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自SMinimal，添加了用于生成适应度地图的类（以重写）
    /// </summary>
	[Serializable]
	public class SMinimalMap : SMinimal
	{
		public FitnessMapProvider2D fitnessMap { get; set; }

		public SMinimalMap() { }
	}
	
    /// <summary>
    /// 网格适应度问题，继承自Pminimal：影响范围固定（实际只是存储空间固定、具体大于0的区域还是有差别的），范围适应度与距离有关
    /// 适应度与干扰值是离散的，对应于一般障碍物的包络网结点
    /// </summary>
	public class PMinimalMap : PMinimal
	{
		public PMinimalMap() { }

		protected override RunState CreateState() { return new SMinimalMap(); }

        //根据问题属性生成适应度地图（真假目标、干扰源）
		protected override void CreateEnvironment(RunState state, FitnessTarget[] targets, Interference[] interfereces)
		{
            //as转换成功的前提是state本身就是SminimalMap（所以要在PMinimalMap中要重写函数CreateState）
			var s = state as SMinimalMap;

            //关闭CollectDecoy后，真目标的数目变为目标数组的长度，即数组中不再有“假目标”
			if (InterferenceNum == 0)
				s.fitnessMap = new FitnessMapProvider2D(SizeX, SizeY, targets, CollectDecoy ? targets.Length : TargetNum);
			else
				s.fitnessMap = new FitnessMapProvider2D(SizeX, SizeY, targets, CollectDecoy ? targets.Length : TargetNum, interfereces);
		}

        //设置一般障碍物、生成包络网结点值、生成最终地图
		protected override void SetTargets(RoboticEnvironment env)
		{
            //生成真假目标与干扰源的位置与值
			base.SetTargets(env);
            //更新包络网各结点的适应度与干扰值
			var fitMap = (env.runstate as SMinimalMap).fitnessMap;
			fitMap.ResetDecoy();
			if (InterferenceNum > 0) fitMap.ResetInterference();
            //生成最终地图
			fitMap.Update();

		}

        //重写顶层基类RoboticProblem的方法，更新适应度与目标检测信息
		public override void UpdateSensor(RobotBase robot, RunState state)
		{
			var r = robot as RFitness;

            //利用地图生成类直接读取机器人位置处的信息，并更新到SensorData中
			r.Fitness.NewData = (state as SMinimalMap).fitnessMap.GetFitness(r.postionsystem.GlobalSensorData);
			r.Fitness.ApplyChange();

            //此处dis不是目标感知范围半径（即圆环的一半），这样一来，最近的目标（真或假）自动成为机器人的感知目标
            //为什么注释掉了Where(on=>on.isNeighbor)的要求？
            //机器人的SenseRange可看作通信范围，也相当于机器人对其他机器人的识别范围
            //一般障碍物的SenseRange相当于机器人对特定障碍物的识别范围，在此范围内机器人可以识别出障碍物
            //在环境的GenerateNeighbours()方法中会计算机器人之间以及机器人与障碍物之间的距离

            //师兄说在计算距离前会对所有距离进行清空，清空的操作是将所有的距离设为 float.MaxValue，这样一来就说得通了
            //实际情况并非如此，而是在mapsensor的生成过程中（RobotBase的Bind函数）已经考虑了isNeighbour信息
			float dis = float.MaxValue;
			r.Target = null;
			foreach (var tar in r.mapsensor[1].Concat(r.mapsensor[2]))//.Where(on => on.isNeighbour))
			{
				if (dis > tar.distance)
				{
					dis = tar.distance;
					r.Target = tar.Target;
				}

                //验证进入循环体的tar的isNeighbor必为true
                //if (tar.isNeighbour == false)
                //{
                //    dis = tar.distance;
                //}

			}
		}

        //base（确认是否终止），根据需要（目标被收集完时激活需要）更新地图
		protected override void env_PostUpdate(RoboticEnvironment env)
        {
			base.env_PostUpdate(env);
            var state = env.runstate as SMinimalMap;
            if (state.RequireUpdate)
            {
                state.fitnessMap.Update();
                state.RequireUpdate = false;
            }
        }
	}
}
