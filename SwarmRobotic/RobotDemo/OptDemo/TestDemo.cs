using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;
using UtilityProject.Funcs;
using RobotDemo.Display;
using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RobotLib.FitnessProblem;

namespace RobotDemo
{
	class TestDemo:OptDemo
	{
		public TestDemo(ControlScreen ctrlScreen)
			:base(ctrlScreen)
		{

		}

		public override void InitializeParameter()
		{
			int points = FitnessMapProvider3D.movemap.GetLength(0), mid = (points - 1) / 2;
			camera = new Camera();
			camera.SetSize(new Vector3(points, points, 1));
			fitMap = new RenderTarget2D(graphicsDevice, points, points);

			int[,] value = new int[points + 1, points + 1];
			for (int i = 0; i < points; i++)
				for (int j = 0; j < points; j++)
				{
					value[i, j] = Math.Max(0, 20 + FitnessMapProvider3D.movemap[i, j, mid]);
				}
			Color[] data = new Color[points *points];
			fitMap.GetData(data);
			for (int i = 0; i < points; i++)
			{
				for (int j = 0; j < points; j++)
				{
					//if (value[i, j] == 20)
					//    data[i * points + j] = Color.Red;
					if (value[i, j] > 10)
						data[i * points + j] = Color.Lerp(Color.FromNonPremultiplied(5, 255, 255, 255), Color.FromNonPremultiplied(200, 5, 200, 255), (value[i, j] - 10) / 10f);
					else if (value[i, j] > 0)
						data[i * points + j] = Color.Lerp(Color.Blue, Color.FromNonPremultiplied(5, 255, 255, 255), value[i, j] / 10f);
					else
						data[i * points + j] = Color.White;
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
		}

		protected override void ResetDemo()
		{
			
		}

		protected override void StepDemo()
		{
			
		}
	}
}
