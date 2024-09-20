using RobotLib;
using RobotLib.TestProblem;
using Microsoft.Xna.Framework;

namespace RobotDemo
{
	class TestScreen : SRScreen
	{
		public TestScreen(ControlScreen screen)
			: base(screen)
		{
			Title = "Test Problem";
			ObsColorMap.Add("Obstacle", Color.Black);
			RoboticColorMap.Add("Run", Color.Green);
		}

		public override bool Bind(Experiment experiment)
		{
			if (experiment.problem is PTest)
				return base.Bind(experiment);
			return false;
		}
	}
}
