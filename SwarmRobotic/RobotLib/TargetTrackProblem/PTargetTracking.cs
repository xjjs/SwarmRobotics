using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;

namespace RobotLib.TargetTrackProblem
{
	public class PTargetTracking : RoboticProblem
	{
        public PTargetTracking() : base() { statelist = new string[] { "run", "near" }; }

		public override RobotBase CreateRobot(RoboticEnvironment env) { return new RTrack(HasInertia); }

        public override void ArrangeRobotic(List<RobotBase> robotics)
        {
            Vector3 pos = MapSize / 4;
            pos.Z *= 2;
            IEnumerator<RobotBase> r = robotics.GetEnumerator();
            IEnumerator<Vector3> v = Utility.UnionInitialize(Population, pos, 5, PositionType.Center, PositionType.Center, PositionType.Center, MapSize.Z > 1).GetEnumerator();
            while (r.MoveNext() && v.MoveNext())
            {
                r.Current.postionsystem.NewData = v.Current;
                r.Current.state.NewData = statelist[0];
            }
            base.ArrangeRobotic(robotics);
        }

		Vector3 GenerateObstaclePos()
		{
			float x, y;
			x = (float)Random.NextDouble() * SizeX;
			y = (float)Random.NextDouble() * SizeY;
			if (x == 0 && y == 0)
			{
                x = SizeX;
                y = SizeY;
			}
            while (x < SizeX / 2 && y < SizeY / 2)
			{
				x *= 2;
				y *= 2;
			}
			if (SizeZ <= 1)
				return new Vector3(x, y, 0.5f);
			else
				return new Vector3(x, y, (float)Random.NextDouble() * SizeZ);
		}

		void SetLargeObstaclePos(MultiObstacle mo)
		{
			Vector3 pos = GenerateObstaclePos();
			if (SizeZ <= 1)
			{
				mo.XMin = (int)pos.X;
				mo.YMin = (int)pos.Y;
				if (Random.NextInt(2) == 0)
				{
					mo.XMax = mo.XMin + Random.NextInt(50);
					mo.YMax = mo.YMin + Random.NextInt(2);
				}
				else
				{
					mo.XMax = mo.XMin + Random.NextInt(2);
					mo.YMax = mo.YMin + Random.NextInt(50);
				}
				mo.ZMin = mo.ZMax = 0;
			}
			else
			{
				mo.XMin = (int)pos.X;
				mo.YMin = (int)pos.Y;
				mo.ZMin = (int)pos.Z;
				switch (Random.NextInt(3))
				{
					case 0:
						mo.XMax = mo.XMin + Random.NextInt(2);
						mo.YMax = mo.YMin + Random.NextInt(50);
						mo.ZMax = mo.ZMin + Random.NextInt(50);
						break;
					case 1:
						mo.XMax = mo.XMin + Random.NextInt(50);
						mo.YMax = mo.YMin + Random.NextInt(2);
						mo.ZMax = mo.ZMin + Random.NextInt(50);
						break;
					case 2:
						mo.XMax = mo.XMin + Random.NextInt(50);
						mo.YMax = mo.YMin + Random.NextInt(50);
						mo.ZMax = mo.ZMin + Random.NextInt(2);
						break;
					default:
						break;
				}
			}
			mo.SetObstacles();
		}

        public override void CreateEnvironment(RoboticEnvironment env)
        {
            var state = new STrack();
            env.runstate = state;
            state.Target = null;
			if (HasTarget)
				env.CreateClusters(this, 2, "Obstacle", "Target", "Obstacle");
			else
				env.CreateClusters(this, 1, "Obstacle", "Obstacle");

            var obstacles = new Obstacle[obsNum];
            for (int i = 0; i < obsNum; i++)
                obstacles[i] = new Obstacle(GenerateObstaclePos(), oRange);
			env.ObstacleClusters[0].AddObstacle(obstacles);
            if (HasTarget)
            {
                var Target = new MovingObstacle(GenerateObstaclePos(), MaxSpeed * 2, RoboticSenseRange, Random, ObstacleMovingSate.RandomLine, 30, SizeZ <= 1 ? 0 : 30);
				env.ObstacleClusters[1].AddObstacle(Target);
                env.PostUpdate += UpdateTarget;
                state.Target = Target.Position;
            }
			var mobstacles = new MultiObstacle[lobsNum];
			for (int i = 0; i < lobsNum; i++)
			{
				var mo = new MultiObstacle(oRange);
				SetLargeObstaclePos(mo);
				mobstacles[i] = mo;
			}
			env.MultiObstacleClusters[0].AddObstacle(mobstacles);

            env.ObstacleCollison += ObstacleCollison;
            env.RobotCollison += RobotCollison;
			env.MultiObstacleCollison += MultiObstacleCollison;
			if (TestMode) env.PostUpdate += UpdateTest;
			OnFinalize += FinishTest;
            state.First = -1;
        }

        public override void ResetEnvironment(RoboticEnvironment env)
		{
            base.ResetEnvironment(env);
            foreach (var item in env.ObstacleClusters[0].obstacles)
				item.Position = GenerateObstaclePos();
            if (env.ObstacleClusters.Count > 1)
            {
                var Target = env.ObstacleClusters[1].obstacles[0];
                Target.Position = GenerateObstaclePos();
                Target.Reset(Random);
                (env.runstate as STrack).Target = Target.Position;
            }
			foreach (var item in env.MultiObstacleClusters[0].obstacles)
				SetLargeObstaclePos(item);
            (env.runstate as STrack).First = -1;
		}

        void UpdateTarget(RoboticEnvironment env) { env.ObstacleClusters[1].obstacles[0].Update(); }

        void UpdateTest(RoboticEnvironment env)
        {
            var result = env.runstate as STrack;
            int near = 0;

            foreach (var r in env.RobotCluster.robots)
            {
                if (r.Broken) continue;
                if (r.state.SensorData == "near") near++;
                result.TotalDis += r.postionsystem.LastMove.Length();
            }
            if (near > 0)
            {
                if (result.First == -1) result.First = result.Iterations;
                result.Follow++;
                result.AveNear += near;
            }
            result.Lives += result.AliveRobots;
            result.AveSwarms += CalculateSwarms(env.RobotCluster);
            if (result.AliveRobots <= Population / 2) result.Finished = true;
        }

        void FinishTest(RunState state, RoboticEnvironment env)
        {
			var result = state as STrack;
            if (result.AliveRobots > Population / 2)
            {
                foreach (var item in env.RobotCluster.robots)
                {
                    if (item.Broken) continue;
                    result.Dis += Vector3.Distance(item.postionsystem.GlobalSensorData, result.Target.Value);
                }
                result.Dis /= result.AliveRobots;
                result.AveDis = result.TotalDis / result.Lives;
                result.AveSwarms = result.AveSwarms / result.Iterations;
                result.Success = true;
            }
            else
                result.Success = false;
            if (result.First == -1)
                result.Miss = -1;
            else
                result.Miss = result.Iterations - result.First - result.Follow;
            if (result.AveNear > 0)
                result.AveNear /= result.Follow;
        }

        static int CalculateSwarms(RobotCluster cluster)
        {
            var broken = cluster.robots.Select(t => t.Broken).ToArray();
            var NeighbourhoodMatrx = cluster.isNeighbour;
            int size = cluster.Population, Swarms = 0;
            Queue<int> queue = new Queue<int>(size);
            int i;

            while (true)
            {
                for (i = 0; i < size; i++)
                {
                    if (!broken[i])
                    {
                        broken[i] = true;
                        queue.Enqueue(i);
                        break;
                    }
                }
                if (i == size) break;
                while (queue.Count > 0)
                {
                    i = queue.Dequeue();
                    for (int j = 0; j < size; j++)
                    {
                        if (NeighbourhoodMatrx[i][j].isNeighbour && !broken[j])
                        {
                            broken[j] = true;
                            queue.Enqueue(j);
                        }
                    }
                }
                Swarms++;
            }
            return Swarms;
        }

        void RobotCollison(RobotBase r1, RobotBase r2, RunState state) { r1.Broken = r2.Broken = true; }

        void ObstacleCollison(RobotBase r, Obstacle o, RunState state) { if (!(o is MovingObstacle)) r.Broken = true; }

		void MultiObstacleCollison(RobotBase r, MultiObstacle o, RunState state) { r.Broken = true; }

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			Population = 36;
			HasTarget = true;
			HasInertia = false;
			obsNum = 500;
			lobsNum = 0;
			oRange = 3;
		}

		int obsNum, lobsNum;
		float oRange;

		[Parameter(ParameterType.Boolean, Description = "Has Target")]
		public bool HasTarget { get; set; }

		[Parameter(ParameterType.Boolean, Description = "Has Inertia")]
		public bool HasInertia { get; set; }

		[Parameter(ParameterType.Int, Description = "Obstacle Number")]
		public int ObstacleNum
		{
			get { return obsNum; }
			set
			{
				if (value < 0) throw new Exception("Must be at least 0");
				obsNum = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Large Obstacle Number")]
		public int LargeObstacleNum
		{
			get { return lobsNum; }
			set
			{
				if (value < 0) throw new Exception("Must be at least 0");
				lobsNum = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Obstacle Sensing Range")]
		public float ObstacleSenseRange
		{
			get { return oRange; }
			set
			{
				if (value < 3 || value > 100) throw new Exception("Must be in [3, 100]");
				oRange = value;
			}
		}
	}
}
