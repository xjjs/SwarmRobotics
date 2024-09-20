using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Obstacles
{
    /// <summary>
    ///继承自Obstacle，增加了能量Energy字段，影响范围为能量值的1/10
    /// </summary>
	public class EnergyTarget : Obstacle
	{
		public EnergyTarget(Vector3 pos, float energy = 0)
			: base(pos)
		{
			Energy = energy;
		}

		public float Energy
		{
			get { return energy; }
			set
			{
				energy = value;
				SenseRange = value / 10;
			}
		}

		float energy;

		public override string ToString()
		{
			return base.ToString() + ":" + energy;
		}
	}
}
