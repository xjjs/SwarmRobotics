using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Obstacles
{
    /// <summary>
    /// 障碍物：位置、ID、影响半径、可视性
    /// </summary>
    public class Obstacle
    {
		public Obstacle() : this(Vector3.Zero) { }

		public Obstacle(Vector3 pos, float SenseRange = 0, CustomRandom rand = null)
        {
            Position = pos;
            this.id = 0;
			this.rand = rand;
			this.SenseRange = SenseRange;
			Visible = true;
			//if (this.SenseRange.Length > 1) Array.Sort(this.SenseRange, ReverseFloatComparer.ReverseComparison);
			//SenseRangeSquare = SenseRange.Select(t => t * t).ToArray();
        }

		public virtual void Update() { }

		public virtual void Reset(CustomRandom rand = null) { if (rand != null) this.rand = rand; }

		public override string ToString()
		{
			return string.Format("({0}){1}", id, Position);
		}

        public Vector3 Position;
        public int id;
		protected CustomRandom rand;
		public float SenseRange;
		public bool Visible;
//		public float[] SenseRangeSquare;
	}
}
