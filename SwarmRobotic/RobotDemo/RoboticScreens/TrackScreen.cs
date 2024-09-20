using GucUISystem;
using Microsoft.Xna.Framework;
using RobotLib;
using RobotLib.Obstacles;
using RobotLib.TargetTrackProblem;

namespace RobotDemo
{
	class TrackScreen : SRScreen
	{
		protected GucStateList stateDestination, stateFollow;
        STrack state;

		public TrackScreen(ControlScreen screen)
			: base(screen)
		{
			stateDestination = null;
			Title = "Target Tracking Problem";
			ObsColorMap.Add("Obstacle", Color.Black);
			ObsColorMap.Add("Target", Color.Red);
			RoboticColorMap.Add("run", Color.Green);
			RoboticColorMap.Add("near", Color.Blue);
		}

		public override bool Bind(Experiment experiment)
		{
			if (experiment.problem is PTargetTracking)
			{
				base.Bind(experiment);
				camera.FollowFunc = FollowSwarm;
				stateDestination.Visible = (experiment.problem as PTargetTracking).HasTarget;
                state = environment.runstate as STrack;
				return true;
			}
			return false;
		}

		protected override void CreateCustomUIStates()
		{
			stateFollow = new GucStateList();
			stateFollow.CheckedTexutre = Skin.CheckBoxChecked;
			stateFollow.NormalTexutre = Skin.CheckBoxNormal;
			stateFollow.Text = "Camara";
			stateFollow.Width = Panel.InnerWidth - 20;
			stateFollow.Items.Add(false, "Free Move");
			stateFollow.Items.Add(true, "Following");
			stateFollow.SelectedChanged += new GucEventHandler(stateFollow_SelectedChanged);
			Panel.Controls.Add(stateFollow);

			stateDestination = new GucStateList();
			stateDestination.CheckedTexutre = Skin.CheckBoxChecked;
			stateDestination.NormalTexutre = Skin.CheckBoxNormal;
			stateDestination.Text = "Destination Moving";
			stateDestination.Width = Panel.InnerWidth - 20;
			stateDestination.Items.Add(ObstacleMovingSate.None, "None");
			stateDestination.Items.Add(ObstacleMovingSate.RandomLine, "Random Moving");
			stateDestination.SelectedIndex = 1;
			stateDestination.SelectedChanged += new GucEventHandler(stateDestination_SelectedChanged);
			Panel.Controls.Add(stateDestination);
		}

		void stateFollow_SelectedChanged(GucControl sender)
		{
			camera.Follow = (bool)stateFollow.SelectedItem;
		}

		void stateDestination_SelectedChanged(GucControl sender)
		{
			(environment.ObstacleClusters[1].obstacles[0] as MovingObstacle).MovingState = (ObstacleMovingSate)stateDestination.SelectedItem;
		}

		protected override void CustomUpdate(InputEventArgs input)
		{
			base.CustomUpdate(input);
			stateFollow.SelectedIndex = camera.Follow ? 1 : 0;
		}

		protected override void Display3D_Draw3DGraphic(GucControl sender)
		{
			graphicsDevice.Clear(Color.White);

			//draw map
			//mapModel.Draw(Matrix.Identity, camera.ViewMatrix, projection, Color.LightSkyBlue);
			mapModel.Draw(Matrix.Identity, camera.ViewMatrix, camera.ProjectionMatrix, Color.White);

			//draw obstacles
            foreach (var cluster in environment.ObstacleClusters)
            {
				var color = ObsColorMap[cluster.obstacles.Key];
				var scale = cluster.obstacles.Key == "Target" ? 2f : 1f;
                foreach (var ob in cluster.obstacles)
                {
                    if (ob.Visible == false) continue;
                    obstacleModel.Draw(Matrix.CreateScale(scale) * Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, camera.ProjectionMatrix, color);
                }
            }

			foreach (var cluster in environment.MultiObstacleClusters)
			{
				var color = ObsColorMap[cluster.obstacles.Key];
				foreach (var mo in cluster.obstacles)
				{
					if (mo.Visible == false) continue;
					foreach (var ob in mo)
					{
						obstacleModel.Draw(Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, camera.ProjectionMatrix, color);
					}
				}
			}

			//draw robotics
			if (ShowRobotics)
			{
                foreach (RobotBase robot in environment.RobotCluster.robots)
					roboticModel.Draw(robot.postionsystem.TranformMatrix, camera.ViewMatrix, camera.ProjectionMatrix, robot.Broken ? Color.Gray : RoboticColorMap[robot.state.SensorData]);
			}
		}

		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			if (stateFollow != null)
				stateFollow.Width = Panel.InnerWidth - 20;
			if (stateDestination != null)
				stateDestination.Width = Panel.InnerWidth - 20;
		}

		Vector3 FollowSwarm()
		{
			if (state.AliveRobots == 0) return Vector3.Zero;
			Vector3 center = Vector3.Zero;
            foreach (var robot in environment.RobotCluster.robots)
			{
				if (robot.Broken) continue;
				center += robot.postionsystem.GlobalSensorData;
			}
            center = center / state.AliveRobots;
			//center.Z = camera.ViewCenter.Z;
			return center;
		}
	}
}
