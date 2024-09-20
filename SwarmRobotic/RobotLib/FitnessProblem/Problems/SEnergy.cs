using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Obstacles;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承SFitness（StateFitness），添加目标总能量、机器人的总能量、收集的能量
    /// </summary>
    [Serializable]
	public class SEnergy : SFitness
    {
        public float TargetEnergy, RobotEnergy, CollectEnergy;

		public bool EnergyMode { get; set; }

        public SEnergy() { }

		//public override bool Finished
		//{
		//    get { return RunRobot == 0 || RemainTarget == 0; }
		//    set { }
		//}
        
        //根据收集的能量、机器人剩余能量、机器人存活个体、迭代次数，比较两个群体状态（RunState）的优劣
        //返回正数则优，负数则差
		public override int CompareTo(RunState other)
		{
			var so = other as SEnergy;
			int result = CollectEnergy.CompareTo(so.CollectEnergy);
			if (result != 0) return result;

			result = RobotEnergy.CompareTo(so.RobotEnergy);
			if (result != 0) return EnergyMode ? result : -result;

			result = AliveRobots.CompareTo(so.AliveRobots);
			if (result != 0) return result;

			return -Iterations.CompareTo(so.Iterations);
		}
    }
}
