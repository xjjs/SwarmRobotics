using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;
using RobotLib.EnergyProblem;

namespace TestProject
{
	class EnergyTest : TestBase<EnergyResult>
	{
		public EnergyTest(int repeat = 50)
			: base(repeat)
		{
		}

		public override EnergyResult TestOnce(Experiment param)
		{
			if (!(param.problem is PEnergy)) throw new Exception();
			AEnergy algo = param.algorithm as AEnergy;
			//float energy = algo.TargetEnergy;
			int lastCharge = 0;
			while (!param.environment.Finished)
			{
				param.Update();
				//if (algo.TargetEnergy != energy)
				//{
				//    energy = algo.TargetEnergy;
				//    lastCharge = param.environment.iterations;
				//}
			}
			//if (float.IsNaN(algo.RoboticEnergy)) energy = 0;
			return new EnergyResult(algo, lastCharge, param.environment.iterations);
		}

		public override IEnumerable<EnergyResult> TestStep(Experiment param)
		{
			throw new NotImplementedException();
		}

		public override TestBase<EnergyResult> Clone() { return new EnergyTest(Repeat); }
	}

	[Serializable]
	class EnergyResult : TestResult
	{
		public float TargetEnergy, RoboticEnergy;
		public int RunRobotic, RemainTarget, LastCharge;

		public EnergyResult(AEnergy algo, int lastCharge, int iteration)
			: base(iteration, 0)
		{
			TargetEnergy = algo.TargetEnergy;
			RoboticEnergy = algo.RoboticEnergy;
			RunRobotic = algo.RunningRobotic;
			RemainTarget = algo.RemainTargets;
			LastCharge = lastCharge;
		}

		public EnergyResult()
		{
			TargetEnergy = RoboticEnergy = 0;
			RunRobotic = RemainTarget = LastCharge =  0;
		}

		public static string Title { get { return "TargetEnergy,RoboticEnergy,RunRobotic,RemainTarget,LastCharge,Iterations,"; } }

		//public override string ToString() { return string.Format("{0},{1},{2},{3},{4},{5},", TargetEnergy, RoboticEnergy, RunRobotic, RemainTarget, LastCharge, Iterations); }

		public override string ToString(float divide)
		{
			if (divide == 0) divide = 1;
			return string.Format("{0},{1},{2},{3},{4},{5},", TargetEnergy / divide, RoboticEnergy / divide, RunRobotic / divide, RemainTarget / divide, LastCharge / divide, Iterations / divide);
		}

		public override void Add(TestResult other)
		{
			EnergyResult er = other as EnergyResult;
			if (er == null) throw new Exception();
			TargetEnergy += er.TargetEnergy;
			RoboticEnergy += er.RoboticEnergy;
			RunRobotic += er.RunRobotic;
			RemainTarget += er.RemainTarget;
			LastCharge += er.LastCharge;
			base.Add(other);
		}

		//public static EnergyResult operator +(EnergyResult p, EnergyResult q)
		//{
		//    var result = new EnergyResult();
		//    result.TargetEnergy = p.TargetEnergy + q.TargetEnergy;
		//    result.RoboticEnergy = p.RoboticEnergy + q.RoboticEnergy;
		//    result.RunRobotic = p.RunRobotic + q.RunRobotic;
		//    result.RemainTarget = p.RemainTarget + q.RemainTarget;
		//    result.LastCharge = p.LastCharge + q.LastCharge;
		//    result.Iterations = p.Iterations + q.Iterations;
		//    return result;
		//}


		public override bool Success
		{
			get { return RemainTarget == 0; }
		}
	}
}
