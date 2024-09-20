using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RobotDemo.Display;
using System.Collections.Generic;

namespace RobotDemo
{
	class SingleScreen : Screen
	{
		GucTextBox yaw, pitch, roll;//, index;
		GucStateList s;
		Camera c;
		IDrawModel model;

		public SingleScreen(ScreenManager manager)
			:base(manager)
		{
            //创建摄像机对象并设置位置
			c = new Camera();
			c.SetSize(new Vector3(10, 10, 1));

            //要载入的3D模型列表，并依次添加4个对象
			var modelList = new List<IDrawModel>();
			Primitive p;

			//robot model
			p = new Primitive(graphicsDevice);//, Content.Load<Texture2D>("robottexture"));
			p.AddSimpleCraft(0.75f, new Vector3(0, 0, -1f));
			//p.AddSimpleCraft(0.375f, new Vector3(0, 0, -0.5f));
			p.EndInitArray();
			//planeModel = p;
			modelList.Add(p);

			p = new Primitive(graphicsDevice);
			p.AddCubic(2, new Vector3(0, 0, -1f));
			//p.AddCubic(1, new Vector3(0, 0, -0.5f));
			p.EndInitArray();
			//cubicModel = p;
			modelList.Add(p);
            //yaw/pitch/roll->偏航、俯仰、翻滚
            //modelList.Add(new Model3D(graphicsDevice, manager.Game.Content.Load<Model>("jet"),
            //    Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0, MathHelper.PiOver2) * Matrix.CreateScale(1f) * Matrix.CreateTranslation(0, 0, -1.5f)));
            modelList.Add(new Model3D(graphicsDevice, manager.Game.Content.Load<Model>("jet"),Matrix.Identity));
            //modelList.Add(new Model3D(graphicsDevice, manager.Game.Content.Load<Model>("enemy"),
            //    Matrix.Identity*Matrix.CreateScale(0.1f)));

            modelList.Add(new Model3D(graphicsDevice, manager.Game.Content.Load<Model>("enemy"),
               Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(float.Parse("-90")),
                    MathHelper.ToRadians(float.Parse("0")), MathHelper.ToRadians(float.Parse("90")))
                    * Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(0.5f, 0, -1)
                ));

            //创建与添加3D显示区域、注册事件处理程序
			Guc3DGraphDisplay g3d = new Guc3DGraphDisplay();
			g3d.Size = new Vector2(900, 900);
			g3d.Draw3DGraphic += new GucEventHandler(g3d_Draw3DGraphic);
			Controls.Add(g3d);

			GucLabel l;
			GucTextBox t;

            //创建与添加标签、文本框
			l = new GucLabel();
			l.X = 910;
			l.Y = 20;
			l.Text = "Yaw";
			Controls.Add(l);
			t = new GucTextBox();
			t.X = 1000;
			t.Y = 20;
			t.Text = "90";
			Controls.Add(t);
			yaw = t;

            //创建与添加标签、文本框
			l = new GucLabel();
			l.X = 910;
			l.Y = 60;
			l.Text = "Pitch";
			Controls.Add(l);
			t = new GucTextBox();
			t.X = 1000;
			t.Y = 60;
			t.Text = "0";
			Controls.Add(t);
			pitch = t;

            //创建与添加标签、文本框
			l = new GucLabel();
			l.X = 910;
			l.Y = 100;
			l.Text = "Roll";
			Controls.Add(l);
			t = new GucTextBox();
			t.X = 1000;
			t.Y = 100;
			t.Text = "-90";
			Controls.Add(t);
			roll = t;

			//l = new GucLabel();
			//l.X = 910;
			//l.Y = 140;
			//l.Text = "Index";
			//Controls.Add(l);
			//t = new GucTextBox();
			//t.X = 1000;
			//t.Y = 140;
			//t.Text = "3";
			//Controls.Add(t);
			//index = t;

            //创建与添加按钮、注册事件处理程序
			GucButton b = new GucButton();
			Controls.Add(b);
			b.X = 910;
			b.Y = t.Bottom + 10;
			b.Text = "Change";
			b.AutoFit();
			b.Click += new GucEventHandler(b_Click);

            //创建与添加状态列表、注册事件处理程序
			s = new GucStateList();
			s.X = 910;
			s.Y = b.Bottom + 10;
			foreach (var item in modelList)
				s.Items.Add(item);
			Controls.Add(s);
			s.SelectedChanged += new GucEventHandler(s_SelectedChanged);
			s.SelectedIndex = 3;

            //触发单击事件
//			b_Click(b);
		}

        //更新要绘制的模型
		void s_SelectedChanged(GucControl sender)
		{
			model = s.SelectedItem as IDrawModel;
		}

		void b_Click(GucControl sender)
		{
			try
			{
                s.Items[3].Tag = new Model3D(graphicsDevice, Manager.Game.Content.Load<Model>("enemy"),
                    Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(float.Parse(yaw.Text)),
                    MathHelper.ToRadians(float.Parse(pitch.Text)), MathHelper.ToRadians(float.Parse(roll.Text)))
                    * Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(0.5f, 0, -1));
                if (s.SelectedIndex == 3)
                {
                    s.SelectedIndex = 0;
                    s.SelectedIndex = 3;
                }
			}
			catch
			{
			}
		}

		void g3d_Draw3DGraphic(GucControl sender)
		{
			c.AngleX = MathHelper.Pi;
			c.UpdateCamera();  //未将Camera实现为一个游戏组件，所以需要手动更新
			graphicsDevice.Clear(Color.White);
			model.Draw(Matrix.CreateTranslation(5, 5, 0.5f), c.ViewMatrix, c.ProjectionMatrix, Color.Blue);
		}
	}

	class TestGame : Game
	{
		public GraphicsDeviceManager graphics;

		public TestGame()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1440;
			graphics.PreferredBackBufferHeight = 900;
			Content.RootDirectory = "Content";
			this.IsFixedTimeStep = false;
			this.IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			ScreenManager manager = new ScreenManager(this);
			this.Components.Add(manager);
			base.Initialize();

			SingleScreen ss = new SingleScreen(manager);
			ss.Show();
		}
	}
}
