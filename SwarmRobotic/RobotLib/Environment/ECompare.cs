using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RobotLib.Environment
{
    public class ECompare : RoboticEnvironment
    {
		public ECompare() { }

        public override void GenerateNeighbours()
        {
			base.GenerateNeighbours();
			Vector3 pos, pos2;
            for (int i = 0; i < problem.Population; i++)
            {
                if (RobotCluster.robots[i].Broken) continue;
                pos = RobotCluster.robots[i].postionsystem.GlobalSensorData;
                for (int j = i + 1; j < problem.Population; j++)
                {
                    if (RobotCluster.robots[j].Broken) continue;
                    pos2 = RobotCluster.robots[j].postionsystem.GlobalSensorData;
					if (IsNeighbourPossible(pos, pos2, RobotCluster.SenseRange))
						CheckNeighbour(i, j, pos, pos2);
                }
				foreach (var oc in ObstacleClusters)
				{
                    foreach (var obs in oc.obstacles)
                    {
                        if (obs.Visible && obs.SenseRange > 0 && IsNeighbourPossible(pos, obs.Position, obs.SenseRange))
                            CheckObstacle(i, pos, oc, obs);
                    }
				}
				foreach (var moc in MultiObstacleClusters)
				{
					foreach (var mo in moc.obstacles)
					{
						foreach (var obs in mo)
						{
							if (obs.Visible && mo.SenseRange > 0 && IsNeighbourPossible(pos, obs.Position, mo.SenseRange))
								CheckMultiObstacle(i, pos, moc, obs);
						}
					}
				}
			}
        }

        bool IsNeighbourPossible(Vector3 v1, Vector3 v2, float distance)
        {
            float delta = v1.X - v2.X;
            if (delta < 0) delta = -delta;
            if (delta > distance) return false;
            delta = v1.Y - v2.Y;
            if (delta < 0) delta = -delta;
            if (delta > distance) return false;
            delta = v1.Z - v2.Z;
            if (delta < 0) delta = -delta;
            if (delta > distance) return false;
            return true;
        }
    }
}
