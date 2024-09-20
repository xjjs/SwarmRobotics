using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Obstacles
{
    /// <summary>
    /// 继承自Obstacle，增加了增加了能量（总体适应度值）、资源数属性，被感知范围由问题设定（环宽度的一半）
    /// 目标定义中：目标的适应度Energy与收集耗时Collect是两个独立的量，即“收集过程”中目标“Energy”保持不变
    /// </summary>
	public class FitnessTarget : Obstacle
	{
		public FitnessTarget(int size = 10, int energy = 0, bool real = true)
			: base(Vector3.Zero, RobotLib.FitnessProblem.PMinimalMap.FitnessRadius * 2)
		{
			Energy = energy;
            IniSize = size;
            Collect = IniSize;
			//Collect = CollectBase;
			Real = real;
		}

		public override void Reset(CustomRandom rand = null)
		{
			base.Reset(rand);
            Collect = IniSize;
			//Collect = CollectBase;
            Visible = true;
		}

		public int Energy { get; set; }

		public override string ToString() { return base.ToString() + ":" + Energy; }

		public int Collect { get; set; }

        public bool Real { get; protected set; }

		static int CollectBase = 10;
        private int IniSize;
	}
}
