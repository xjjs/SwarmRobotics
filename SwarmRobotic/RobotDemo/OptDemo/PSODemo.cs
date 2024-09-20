using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityProject.PSO;
using RobotLib;
using UtilityProject.Funcs;
using Microsoft.Xna.Framework;
using GucUISystem;

namespace RobotDemo
{
	class PSODemo : OptDemo
	{
		PSO<double> pso;
		Color[] colors;

		public PSODemo(ControlScreen ctrlScreen)
			: base(ctrlScreen)
		{
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			pop = 5;
			seed = -1;
			rate = 0.1f;
			epsilon = 0.0001f;
		}

		protected override void CreateCustomUIStates()
		{
			//base.CreateCustomUIStates();
			GucButton b = new GucButton();
			b.Click += new GucEventHandler(RefreshColor);
			b.Text = "Refresh Color";
			b.AutoFit();
			Panel.Controls.Add(b);
		}

		void RefreshColor(GucControl sender)
		{
			var allcolors = typeof(Color).GetProperties().Where(pi => pi.PropertyType == typeof(Color)).Select(pi => pi.GetValue(null, null)).Skip(1).OfType<Color>().Where(c => (c.R == 0 || c.B + c.G > 30) && Vector3.Distance(Vector3.One, c.ToVector3()) >= 0.3).ToList();
			Random rand = new Random();
			for (int i = 0; i < pop; i++)
			{
				int rv = rand.Next(allcolors.Count);
				colors[i] = allcolors[rv];
				allcolors.RemoveAt(rv);
				allcolors.RemoveAll(c => Vector3.Distance(colors[i].ToVector3(), c.ToVector3()) < 0.3);
			}
		}

		protected override void CustomUpdate(InputEventArgs input)
		{
			//base.CustomUpdate(input);
			if (input.isKeyUp(Microsoft.Xna.Framework.Input.Keys.R)) RefreshColor(null);
			InfoText = string.Format("{0}: {1}\r\n", pso.iteration, pso.nTopo.g_fitness);
		}

		public override void InitializeParameter()
		{
			base.InitializeParameter();
			reload = 1 + epsilon;
			pso = new PSO<double>();
			pso.nTopo = new IndexTopo<double>();
			pso.maxIteration = 1000;
			pso.Evaluate = FitnessFunc;
			pso.MaxStepRate = rate;
			pso.InitSize(pop, 2);
			pso.Init();

			colors = new Color[pop];
			RefreshColor(null);
		}

		protected override void Display3D_Draw3DGraphic(GucControl sender)
		{
			base.Display3D_Draw3DGraphic(sender);
			foreach (var p in pso.particles)
			{
				particleModel.Draw(Matrix.CreateTranslation(Vector3.Transform(new Vector3((float)p.position[0], (float)p.position[1], 0), shift)), camera.ViewMatrix, camera.ProjectionMatrix, colors[p.index]);
			}
		}

		int seed, pop;
		float rate, reload, epsilon;

		[Parameter(ParameterType.Int, Description = "Population")]
		public int Population
		{
			get { return pop; }
			set
			{
				if (value <= 0) throw new Exception("Must be positive");
				pop = value;
			}
		}

		[Parameter(ParameterType.Int, Description = "Random Seed")]
		public int RandSeed
		{
			get { return seed; }
			set
			{
				if (value < -1) throw new Exception("Must be at least -1");
				seed = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Max Speed Rate")]
		public float MSRate
		{
			get { return rate; }
			set
			{
				if (value < 0 || value > 1) throw new Exception("Must be in [0,1]");
				rate = value;
			}
		}

		[Parameter(ParameterType.Float, Description = "Reload Epsilon")]
		public float ReloadEpsilon
		{
			get { return epsilon; }
			set
			{
				if (value <= 0 || value > 1) throw new Exception("Must be in (0,1]");
				epsilon = value;
			}
		}

		protected override void ResetDemo()
		{
			pso.Init();
			InfoText = string.Format("{0}: {1}\r\n", pso.iteration, pso.nTopo.g_fitness);
		}

		protected override void StepDemo()
		{
			pso.Iterate();
			InfoText = string.Format("{0}: {1}\r\n", pso.iteration, pso.nTopo.g_fitness);
			if (pso.nTopo.g_fitness < reload) ResetDemo();
		}
	}
}
