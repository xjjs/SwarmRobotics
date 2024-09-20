using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RobotLib.TargetTrackProblem
{
    public class ATetrahedron : AForceTrack
    {
        float k, wk;
        bool use3d;
        List<Tuple<Vector3, float, float>> nList;

		public ATetrahedron() { }

        public override void InitializeParameter()
        {
            base.InitializeParameter();
			wk = k * WallC;
			nList = new List<Tuple<Vector3, float, float>>(problem.Population);
            use3d = problem.SizeZ > 1;
        }

		protected override void CalculateRobotForce(RobotBase robot, ref Vector3 force, ref int count)
		{
			Vector3 p1, p2 = Vector3.Zero, p3;
			float mlen = 0;
			nList.Clear();
			foreach (var r in robot.Neighbours)
			{
				nList.Add(new Tuple<Vector3, float, float>(r.offset, r.distance, r.distance));
			}

			if (nList.Count > 0)
			{
				////////////////////////p1=min(dis(p,p1))/////////////////////////////////////
				nList.Sort((t1, t2) => t1.Item3.CompareTo(t2.Item3));
				p1 = nList[0].Item1;
				mlen = nList[0].Item2;
				nList.RemoveAt(0);
				force += RoboForce(p1 / mlen, mlen);
				count++;

				if (nList.Count > 0)
				{
					////////////////////////p2=min(dis(p,p2)+dis(p1,p2))/////////////////////////////////////
					for (int i = 0; i < nList.Count; i++)
					{
						nList[i] = new Tuple<Vector3, float, float>(nList[i].Item1, nList[i].Item2, (nList[i].Item1 - p1).Length() + nList[i].Item2);
					}
					nList.Sort((t1, t2) => t1.Item3.CompareTo(t2.Item3));
					while (nList.Count > 0)
					{
						p2 = nList[0].Item1;
						mlen = nList[0].Item2;
						nList.RemoveAt(0);
						if (Vector3.Cross(p1, p2) == Vector3.Zero) continue;
						force += RoboForce(p2 / mlen, mlen);
						count++;
						break;
					}

					if (use3d && nList.Count > 0)
					{
						////////////////////////p3=min(area(p,p1,p2,p3))/////////////////////////////////////
						p3 = Vector3.Cross(p1, p2);
						for (int i = 0; i < nList.Count; i++)
						{
							mlen = Vector3.Dot(nList[i].Item1, p3);
							if (mlen == 0)
							{
								nList.RemoveAt(i);
								i--;
							}
							else
								nList[i] = new Tuple<Vector3, float, float>(nList[i].Item1, nList[i].Item2, Math.Abs(mlen));
						}
						if (nList.Count > 0)
						{
							nList.Sort((t1, t2) => t1.Item3.CompareTo(t2.Item3));
							p3 = nList[0].Item1;
							mlen = nList[0].Item2;
							force += RoboForce(p3 / mlen, mlen);
							count++;
						}
					}
				}
			}
		}

		protected override Vector3 RoboForce(Vector3 direction, float len) { return k * (len - distance) * direction; }

		protected override Vector3 WallForce(Vector3 direction, float len) { return wk * (len - walldis) * direction; }

        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
			if (Inertia)
			{
				WallC = 6.5f;
				k = 1f;
			}
			else
			{
				WallC = 5.5f;	//6f
				k = 1f;
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
	}
}
