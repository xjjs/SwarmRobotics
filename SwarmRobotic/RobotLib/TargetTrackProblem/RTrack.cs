using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Sensors;
using RobotLib.Environment;
using RobotLib.Obstacles;

namespace RobotLib.TargetTrackProblem
{
	public class RTrack : RobotBase
	{
        public RTrack(bool inertia = false) : base(inertia) { }

		public override void Bind(List<NeighbourData<RobotBase>> RobotNeighbour, List<ObstacleCluster> Obstacles, List<MultiObstacleCluster> MultiObstacles)
		{
			base.Bind(RobotNeighbour, Obstacles, MultiObstacles);
			this.Obstacles = mapsensor[0];
			LargeObstacles = largemapsensor[0];
			Target = Obstacles.Count > 1 ? Obstacles[1].isNeighbour[id][0] : null;
		}

        public override RobotBase Clone() { return new RTrack(postionsystem.inertia); }

        public NeighbourData<Obstacle> Target;
        public IEnumerable<NeighbourData<Obstacle>> Obstacles;
		public IEnumerable<IGrouping<int, NeighbourData<Obstacle>>> LargeObstacles;
	}
}
