using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;
using RobotLib.FitnessProblem;

namespace TestProject
{
	class EnergyTest : TestBase<SEnergy>
	{
		public EnergyTest(int repeat = 50, int MaxIter = 100000, bool EnergyRate = true, params float[] Rates)
			: base(repeat)
		{
			this.Rates = Rates;
			this.EnergyRate = EnergyRate;
			MaxIteration = MaxIter;
		}

		public override SEnergy TestOnce(Experiment param) { return TestStep(param).ElementAt(indexOnce); }

		public override IEnumerable<SEnergy> TestStep(Experiment param)
		{
			SEnergy state = param.environment.runstate as SEnergy, result;
			if (EnergyRate)
			{
				float totalEng = (param.problem as PEnergy).TotalEnergy, energy;
				foreach (var rate in Rates)
				{
					energy = totalEng * rate;
					while (!state.Finished && state.CollectEnergy < energy && state.Iterations < MaxIteration)
						param.Update();
					result = state.ResultClone() as SEnergy;
					param.problem.FinalizeState(result, param.environment);
					result.Success = state.CollectEnergy >= energy;
					yield return result;
				}
			}
			else
			{
				float totalTar = (param.problem as PEnergy).TargetNum, target;
				foreach (var rate in Rates)
				{
					target = totalTar * rate;
					while (!state.Finished && state.CollectedTargets < target && state.Iterations < MaxIteration)
						param.Update();
					result = state.ResultClone() as SEnergy;
					param.problem.FinalizeState(result, param.environment);
					result.Success = state.CollectedTargets >= target;
					yield return result;
				}
			}
		}

		public override TestBase<SEnergy> Clone() { return new EnergyTest(Repeat, MaxIteration, EnergyRate, Rates); }

		float[] Rates;
		public bool EnergyRate;
		public int indexOnce = 0;
	}
}
