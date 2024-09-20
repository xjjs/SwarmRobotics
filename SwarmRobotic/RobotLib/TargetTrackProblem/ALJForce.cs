using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.TargetTrackProblem
{
    public class ALJForce : AForceTrack
    {
		float RC, AC, WRC, WAC, c, d;

		public ALJForce() { }

		public override void InitializeParameter()
		{
			base.InitializeParameter();
			AC = c;
			RC = d;
			WAC = WallC * AC;
			WRC = WallC * RC;

            float p = (float)Math.Pow(distance, 12);
            RC *= p;
            WRC *= p;
            p = (float)Math.Pow(distance, 6);
            AC *= p;
            WAC *= p;
		}

		protected override Vector3 RoboForce(Vector3 direction, float len) { return (float)(AC / Math.Pow(len, 7) - RC / Math.Pow(len, 13)) * direction; }

		protected override Vector3 WallForce(Vector3 direction, float len) { return (float)(WAC / Math.Pow(len, 7) - WRC / Math.Pow(len, 13)) * direction; }

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			if (Inertia)
			{
				Rate = 0.8f;	//0.9f
				WallC = 10f;	//7.5f
				c = 1.6f;	//0.6f
				d = 4.6f;	//1.1f
			}
			else
			{
				WallC = 9f;
				c = 0.1f;
				d = 3.1f;
			}
		}

		[Parameter(ParameterType.Float, Description = "Attractive Constrant")]
		public float C
		{
			get { return c; }
			set
			{
				if (value <= 0) throw new Exception("Must be in positive");
				c = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Repulsive Constrant")]
		public float D
		{
			get { return d; }
			set
			{
				if (value <= 0) throw new Exception("Must be in positive");
				d = value;
			}
		}
	}
}
