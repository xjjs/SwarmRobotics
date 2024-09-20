using GucUISystem;
using Microsoft.Xna.Framework;
using RobotDemo.Display;
using RobotLib;
using RobotLib.FitnessProblem;
using RobotLib.Obstacles;

namespace RobotDemo
{
	class EnergyScreen : SRScreen
	{
		protected GucStateList stateFitness;
        SEnergy state;
		bool fitness;
		//RenderTarget2D fitMap;
		//Matrix View, Project;
		//bool changed = true;
		Primitive[] Circles;
		Color[] fitColors = new Color[] { Color.DeepSkyBlue, Color.SlateBlue, Color.SeaGreen, Color.YellowGreen, Color.GreenYellow };

		public EnergyScreen(ControlScreen screen)
			: base(screen)
		{
			//problem = experiment.problem as PEnergy;
			stateFitness = null;
			fitness = true;
			Title = "Engery Problem";
			ObsColorMap.Add("Obstacle", Color.Black);
			ObsColorMap.Add("Target", Color.Red);
			RoboticColorMap.Add("Run", Color.Green);
			RoboticColorMap.Add("OutofPower", Color.Gray);
			RoboticColorMap.Add("Charging", Color.Red);

            RoboticColorMap.Add("Decoy", Color.Red);
            RoboticColorMap.Add("Diffusion", Color.Black);

			//fitMap = new RenderTarget2D(GraphicsDevice, (int)mapProvider.MapSize.X, (int)mapProvider.MapSize.Y);
			//(mapModel as Primitive).Texture = fitMap;
			//View = Matrix.CreateLookAt(new Vector3(mapProvider.MapSize.X / 2, mapProvider.MapSize.Y / 2, -10),
			//    new Vector3(mapProvider.MapSize.X / 2, mapProvider.MapSize.Y / 2, 0), Vector3.Down);
			//Project = Matrix.CreateOrthographic(mapProvider.MapSize.X, mapProvider.MapSize.Y, 1, 1000);
			Circles = new Primitive[5];
			for (int i = 0; i < 5; i++)
			{
				Circles[i] = new Primitive(graphicsDevice);
				Circles[i].AddCircle(60, (5 - i) / 5f, new Vector3(0, 0, -1f), new Vector3(0, 0, 0.5f - (i + 1) / 10f));
				Circles[i].EndInitArray();
			}
		}

		public override bool Bind(Experiment experiment)
		{
			if (experiment.problem is PEnergy)
			{
				base.Bind(experiment);
				//stateView.SelectedIndex = 0;
                state = environment.runstate as SEnergy;
				return true;
			}
			return false;
		}

		protected override void CreateCustomUIStates()
		{
			stateView.Visible = false;

			stateFitness = new GucStateList();
			stateFitness.CheckedTexutre = Skin.CheckBoxChecked;
			stateFitness.NormalTexutre = Skin.CheckBoxNormal;
			stateFitness.Text = "Fitness Map";
			stateFitness.Width = Panel.InnerWidth - 20;
			stateFitness.Items.Add(true, "Show Fitness");
			stateFitness.Items.Add(false, "Hide Fitness");
			stateFitness.SelectedIndex = 0;
			stateFitness.SelectedChanged += new GucEventHandler(stateFitness_SelectedChanged);
			Panel.Controls.Add(stateFitness);
		}

		void stateFitness_SelectedChanged(GucControl sender)
		{
			fitness = (bool)stateFitness.SelectedItem;
		}

		protected override void CustomUpdate(InputEventArgs input)
		{
			base.CustomUpdate(input);
			InfoText += string.Format("\nTarget Energy={0}/{2}\nRobotic Energy={1}/{3}\n", state.TargetEnergy, state.RobotEnergy, (experiment.problem as PEnergy).TargetNum - state.CollectedTargets, state.AliveRobots);
			foreach (var r in environment.RobotCluster.robots)
				InfoText += r.ToString() + "\n";
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
                foreach (var ob in cluster.obstacles)
                {
					if (!ob.Visible) continue;
					if (ob is EnergyTarget)
                        obstacleModel.Draw(Matrix.CreateScale((ob as EnergyTarget).Energy / 200) * Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, camera.ProjectionMatrix, color);
                    else
                        obstacleModel.Draw(Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, camera.ProjectionMatrix, color);
                }
            }
			//if (experiment.algorithm.obstaclelist != null)
			//{
			//    foreach (var ob in experiment.algorithm.obstaclelist)
			//    {
			//        obstacleModel.Draw(Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, projection, Color.Red);
			//    }
			//}

			//draw robotics
            if (ShowRobotics)
            {
                foreach (RobotBase robot in environment.RobotCluster.robots)
                    roboticModel.Draw(robot.postionsystem.TranformMatrix, camera.ViewMatrix, camera.ProjectionMatrix, robot.Broken ? Color.Gray : RoboticColorMap[robot.state.SensorData]);
            }

			if (fitness)
			{
                foreach (EnergyTarget ob in environment.ObstacleClusters[1].obstacles)
				{
					if (!ob.Visible) continue;
					Matrix transform = Matrix.CreateScale(ob.SenseRange, ob.SenseRange, 1) * Matrix.CreateTranslation(ob.Position);
					for (int i = 0; i < 5; i++)
						Circles[i].Draw(transform, camera.ViewMatrix, camera.ProjectionMatrix, fitColors[i]);
				}
			}
		}

		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			if (stateFitness != null) stateFitness.Width = Panel.InnerWidth - 20;
		}
	}
}
