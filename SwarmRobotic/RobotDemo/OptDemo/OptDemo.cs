using System;
using System.Linq;
using RobotLib;
using UtilityProject.Funcs;
using RobotDemo.Display;
using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotDemo
{
	abstract class OptDemo : DemoScreen, IParameter
	{
		protected IDrawModel particleModel, mapModel;
		protected Matrix shift;
		protected ComponentRange range;

		protected RenderTarget2D fitMap;

		public OptDemo(ControlScreen ctrlScreen)
			: base(ctrlScreen)
		{
			Primitive p = new Primitive(graphicsDevice);
			p.AddSphere(3f, 16, Vector3.Zero);
			p.EndInitArray();
			particleModel = p;

			CreateDefaultParameter();
		}

		public virtual void CreateDefaultParameter()
		{
			FitnessFunc = CILFunction.Functions[0];
		}

		public virtual void InitializeParameter()
		{
			int points = 500;
			camera = new Camera();
			camera.SetSize(new Vector3(points, points, 1));
			range = FitnessFunc.GetRange(0);
			var size = (float)(range.UBound - range.LBound);
			fitMap = new RenderTarget2D(graphicsDevice, points + 1, points + 1);

			int[] select = new int[] { 100,1000, 10000, 50000, 100000, 200000 };
			float[] percent = new float[] { 0.8f, 0.6f,0.5f, 0.4f, 0.3f, 0.2f, 0.1f, 0 };
			double[,] value = new double[points + 1, points + 1];
			double max = double.MinValue, min = double.MaxValue;
			for (int i = 0; i <= points; i++)
				for (int j = 0; j <= points; j++)
				{
					value[i, j] = FitnessFunc.Evaluate(new double[] { i * size / points + range.LBound, j * size / points + range.LBound });
					if (value[i, j] > max) max = value[i, j];
					if (value[i, j] < min) min = value[i, j];
				}
			max -= min;
			var marks = value.OfType<double>().OrderBy(i => i).Where((val, ind) => select.Contains(ind)).ToArray();
			Color[] data = new Color[(points + 1) * (points + 1)];
			fitMap.GetData(data);
			for (int i = 0; i <= points; i++)
			{
				for (int j = 0; j <= points; j++)
				{
					//spriteBatch.Draw(Skin.Texture, new Rectangle(i, j, 1, 1), Skin.White1x1, Color.Lerp(Color.Red, Color.White, (float)((value[i, j] - min) / max)));
					int k = Array.BinarySearch(marks, value[i, j]);
					if (k < 0) k = ~k;
					data[i * (points + 1) + j] = Color.Lerp(Color.White, Color.Red, percent[k]);
				}
			}
			fitMap.SetData(data);
			//var stream = System.IO.File.Open(string.Format("fit.jpg", points + 1, points + 1), System.IO.FileMode.Create);
			//fitMap.SaveAsJpeg(stream, points + 1, points + 1);
			//stream.Close();

			Primitive p = new Primitive(graphicsDevice, fitMap);
			//Vector3 center = camera.ViewCenter;
			//center.Z = mapProvider.MapSize.Z;
			p.AddPanel(points, points, new Vector3(0, 0, -1f), camera.ViewCenter);
			//p.AddPanel(mapProvider.MapSize.X, mapProvider.MapSize.Y, new Vector3(0, 0, 1f), camera.ViewCenter);
			p.EndInitArray();
			mapModel = p;

			shift = Matrix.CreateTranslation(new Vector3(-(float)range.LBound, -(float)range.LBound, 0)) * Matrix.CreateScale(points / size);
		}

		protected override void CustomUpdate(InputEventArgs input) { }

		protected override void Display3D_Draw3DGraphic(GucControl sender)
		{
			graphicsDevice.Clear(Color.White);
			mapModel.Draw(Matrix.Identity, camera.ViewMatrix, camera.ProjectionMatrix, Color.White);
		}

		protected override bool Finished { get { return false; } }

		[Parameter("Functions", Description = "Fitness Function")]
		public EvaluateFunction<double> FitnessFunc { get; set; }

		public static EvaluateFunction<double>[] Functions() { return CILFunction.Functions; }
	}
}
