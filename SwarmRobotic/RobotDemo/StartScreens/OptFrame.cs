using System;
using System.Linq;
using GucUISystem;
using Microsoft.Xna.Framework;
using RobotLib;

namespace RobotDemo
{
	class OptFrame : GucFrameBox
	{
		TypeParaFrame f;
		GucButton buttonNext;

		public OptFrame(ControlScreen parent)
		{
			this.Parent = parent;
			AutoInnerSize = true;

			buttonNext = new GucButton();
			Controls.Add(buttonNext);
			buttonNext.Width = 100;
			buttonNext.Text = "Next";
			buttonNext.Click += buttonNext_Click;

			f = new TypeParaFrame(typeof(OptDemo), "Demo", this, ctorParameter: new object[] { parent });
			f.HeightChanged += new Action<TypeParaFrame>(f_HeightChanged);
			f.Filter();
			InnerWidth = f.Width;
			buttonNext.X = f.Width - buttonNext.Width - 50;
		}

		void f_HeightChanged(TypeParaFrame obj)
		{
			buttonNext.Y = f.Height + 10;
			InnerHeight = buttonNext.Bottom + 10;
		}

		private void buttonNext_Click(GucControl sender)
		{
			OptDemo game = f.GetTypeInstance() as OptDemo;
			game.CreateUI();
			game.Reset();
			game.Show();
		}
	}
}
