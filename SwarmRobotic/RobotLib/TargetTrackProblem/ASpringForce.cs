using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RobotLib.TargetTrackProblem
{
    public class ASpringForce : AForceTrack
    {
		float k, wk, br, nd, pd, delta;

		public ASpringForce() { }

        public override void InitializeParameter()
        {
            base.InitializeParameter();
			wk = k * WallC;
			delta = problem.RoboticSenseRange * br;
			nd = distance - delta;
			pd = distance + delta;
        }

		protected override Vector3 RoboForce(Vector3 direction, float len)
		{
			if (len < nd) len += delta;
			else if (len > pd) len -= delta;
			else return Vector3.Zero;
			return k * (len - distance) * direction;
		}

		protected override Vector3 WallForce(Vector3 direction, float len) { return wk * (len - walldis) * direction; }

        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
			if (Inertia)
			{
				WallC = 4f;
				k = 2.8f;
				br = 0.05f;
			}
			else
			{
				WallC = 3f;
				k = 1.6f;
				br = 0.1f;
			}
        }

		[Parameter(ParameterType.Float, Description = "Repulse K of Spring")]
		public float K
		{
			get { return k; }
			set
			{
				if (value <= 0) throw new Exception("Must be in positive");
				k = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Buffer Rate")]
		public float BR
		{
			get { return br; }
			set
			{
				if (value < 0 || value > 0.5f) throw new Exception("Must be in [0, 0.5]");
				br = value;
			}
		}
	}
}
