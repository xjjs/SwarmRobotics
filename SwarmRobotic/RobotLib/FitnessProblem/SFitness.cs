using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotLib.FitnessProblem
{
	//SEnergy: RunRobot->AliveRobots, RemainTarget->CollectedTargets, LastCharge->LastCollect
	//SMinimal: new LastCollect
    /// <summary>
    ///可看为StateFitness，继承RunState，添加收集的目标数，移动总距离
    /// </summary>
	[Serializable]
	public class SFitness : RunState
	{
		public int CollectedTargets, LastCollect;
		public float TotalDistance;

		public SFitness() { }
	}
}
