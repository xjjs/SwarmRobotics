using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Sensors;
using RobotLib.Environment;
using RobotLib.Obstacles;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承RFitness（RobotFitness），添加了公有字段Energy
    /// </summary>
	public class REnergy : FitnessProblem.RFitness
	{
		public REnergy(int hisSize) : base(hisSize) { }

		public override RobotBase Clone() { return new REnergy(History.Capacity); }

		public override string ToString() { return string.Format(Broken ? "({0}):{1} Broken {2}" : "({0}):{1} {4}/F{3} {2}", id, state.SensorData, postionsystem.GlobalSensorData, Fitness.SensorData, Energy); }

		public float Energy;
		//public bool RandomSearch;
	}
}
