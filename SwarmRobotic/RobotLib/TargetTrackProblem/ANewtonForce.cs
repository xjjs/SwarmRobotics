using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.TargetTrackProblem
{
    public class ANewtonForce : AForceTrack
    {
        float G, WG;
        int pow;

		public ANewtonForce() { }

        public override void InitializeParameter()
        {
            base.InitializeParameter();
			WG = G * WallC;
        }

		protected override Vector3 RoboForce(Vector3 direction, float len) { return (len > distance ? G : -G) / (float)Math.Pow(len, pow) * direction; }

		protected override Vector3 WallForce(Vector3 direction, float len) { return -WG / (float)Math.Pow(len, pow) * direction; }

        public override void CreateDefaultParameter()
        {
			base.CreateDefaultParameter();
			pow = 2;
			if (Inertia)
			{
				WallC = 2f;
				G = 7.6f;
			}
			else
			{
				WallC = 2f;	//4f
				G = 9.6f;
			}
        }

		[Parameter(ParameterType.Float, Description = "Gravitational")]
		public float GC
		{
			get { return G; }
			set
			{
				if (value <= 0) throw new Exception("Must be in positive");
				G = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Power")]
		public int P
		{
			get { return pow; }
			set
			{
				if (value <= 0) throw new Exception("Must be in positive");
				pow = value;
			}
		}
	}
}
