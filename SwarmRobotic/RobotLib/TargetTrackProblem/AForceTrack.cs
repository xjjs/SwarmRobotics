using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;

namespace RobotLib.TargetTrackProblem
{
    public abstract class AForceTrack : RoboticAlgorithm
    {
        protected float distance, walldis, maxspeed;
		float rate, wc;
		protected bool Inertia = true;
		protected PTargetTracking problem;

        public AForceTrack() { }// statelist = new string[] { "run", "near" }; }

        public override void InitializeParameter()
        {
			maxspeed = problem.MaxSpeed;
			distance = problem.RoboticSenseRange * rate;
			walldis = problem.ObstacleSenseRange;
		}

		public override bool Bind(RoboticProblem problem, bool changePara = true)
		{
			//if (!base.Bind(problem)) return false;
			this.problem = problem as PTargetTracking;
			if (this.problem == null) return false;
			if (Inertia != this.problem.HasInertia)
			{
				Inertia = this.problem.HasInertia;
				if (changePara) CreateDefaultParameter();
			}
			return true;
		}

		protected virtual void CalculateRobotForce(RobotBase robot, ref Vector3 force, ref int count)
		{
			foreach (var r in robot.Neighbours)
			{
				force += RoboForce(r.offset / r.distance, r.distance);
				count++;
			}
		}

        public override void Update(RobotBase robot, RunState runstate)
        {
            Vector3 force = Vector3.Zero;
			var rtrack = robot as RTrack;
            float tlen;
            int count = 0;
			foreach (var point in rtrack.Obstacles)
            {
				tlen = point.distance;
				if (tlen <= walldis)
				{
					force += WallForce(point.offset / tlen, tlen);
					count++;
				}
            }
			foreach (var lo in rtrack.LargeObstacles)
			{
				foreach (var point in lo)
				{
					tlen = point.distance;
					if (tlen <= walldis)
					{
						force += WallForce(point.offset / tlen, tlen);
						count++;
					}
				}
			}
			CalculateRobotForce(robot, ref force, ref count);
			if (count > 0)
			{
				force /= count;
				tlen = force.Length();
				if (tlen > maxspeed) force = force / tlen * maxspeed;
			}
            var state = runstate as STrack;
            if (state.Target != null)
			{
				if (rtrack.Target.isNeighbour)
				{
					state.Target = robot.postionsystem.GlobalSensorData + rtrack.Target.offset;
					robot.state.NewData = "near";
				}
				else
				{
					robot.state.NewData = "run";
					Vector3 dest = robot.postionsystem.GlobalSensorData;
                    dest = state.Target.Value - dest;
					dest.Normalize();
					dest /= 5;
					force += dest;
				}
			}
			robot.postionsystem.NewData = force;
		}

        protected abstract Vector3 WallForce(Vector3 direction, float len);

        protected abstract Vector3 RoboForce(Vector3 direction, float len);

        public override void Reset() { }

        public override void CreateDefaultParameter()
        {
			rate = 0.9f;
			wc = 5;
        }

		[Parameter(ParameterType.Float, Description = "Balance Rate")]
		public float Rate
		{
			get { return rate; }
			set
			{
				if (value <= 0 || value > 1.5f) throw new Exception("Must be in (0, 1.5]");
				rate = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Wall Constrant")]
		public float WallC
		{
			get { return wc; }
			set
			{
				if (value <= 0) throw new Exception("Must be in positive");
				wc = value;
			}
		}
    }
}
