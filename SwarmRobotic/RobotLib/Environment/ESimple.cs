using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RobotLib.Environment
{
    public class ESimple : RoboticEnvironment
    {
		public ESimple() { }

        public override void GenerateNeighbours()
        {
			base.GenerateNeighbours();
            Vector3 pos;
			for (int i = 0; i < problem.Population; i++)
            {
                if (RobotCluster.robots[i].Broken) continue;
                pos = RobotCluster.robots[i].postionsystem.GlobalSensorData;
                for (int j = i + 1; j < problem.Population; j++)
                {
                    if (RobotCluster.robots[j].Broken) continue;
                    CheckNeighbour(i, j, pos, RobotCluster.robots[j].postionsystem.GlobalSensorData);
                }
                foreach (var oc in ObstacleClusters)
                {
                    foreach (var obs in oc.obstacles)
                    {
                        if (obs.Visible && obs.SenseRange > 0)
                            CheckObstacle(i, pos, oc, obs);
                    }
                }
				foreach (var moc in MultiObstacleClusters)
				{
					foreach (var mo in moc.obstacles)
					{
						foreach (var obs in mo)
						{
							if (obs.Visible && mo.SenseRange > 0)
								CheckMultiObstacle(i, pos, moc, obs);
						}
					}
				}
            }
        }
    }
}
