using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Obstacles
{
    /// <summary>
    /// 继承自Obstacle，仅增加了属性Level
    /// </summary>
	public class Interference : Obstacle
	{
		public Interference(int level = 5) : base(Vector3.Zero) { Level = level; }

		public int Level { get; set; }

		public override string ToString() { return base.ToString() + ": -" + Level; }
	}
}
