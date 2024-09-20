using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GucUISystem;

namespace TestGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
			this.IsMouseVisible = true;
			this.IsFixedTimeStep = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
			//GucUISystem.Input.Win32Input.Initialize(this.Window);
            //掌握各种控件的使用方法，首先创建“屏幕管理器”（游戏组件）
            //“绘制子控件region”与“调用子控件的Draw方法”是不同的：ScreenManager会调用激活的Screen的Draw方法，然后绘制激活的Screen；
            //激活窗口的Draw方法来自于父类Container（Draw方法开始会调用所有子控件的Draw），若需重绘则绘制所有可见的Inner子控件
			ScreenManager manager = new ScreenManager(this);
			this.Components.Add(manager);
			base.Initialize();


            //创建新屏幕
			s1 = new Screen(manager);
			//s.InnerHeight = s.Height - 25;
			s1.InnerHeight = s1.InnerWidth = 400;
            s1.Text = "FirstScreen";
			s1.Show();

            //创建滚动条并添加到屏幕
			GucScrollBar bar = new GucScrollBar();
			s1.Controls.Add(bar);
			bar.Height = 500;
			bar.Width = 50;
			bar.isVertical = false;
			bar.Value = 50;
			bar.MaxValue = 100;
			bar.X = 50;
			bar.Y = 100;
			bar.ValueChanged += new GucEventHandler(bar_ValueChanged);

            //创建文本框并添加到屏幕
			GucTextBox txtBox = new GucTextBox();
			s1.Controls.Add(txtBox);
			txtBox.X = 50;
			txtBox.Y = 50;
			txtBox.Width = 200;
			txtBox.Text = "";

			//GucWin32TextBox txt2 = new GucWin32TextBox();
			//s.Controls.Add(txt2);
			//txt2.X = 100;
			//txt2.Y = 240;
			//txt2.Width = 300;
			//txt2.Text = "";

            //创建标签并添加到屏幕
			l = new GucLabel();
			s1.Controls.Add(l);
			l.X = 50;
			l.Y = 200;
			l.Text = "Press Key";

            //创建按钮并添加到屏幕，注册事件处理程序
			GucButton b = new GucButton();
			s1.Controls.Add(b);
			b.X = 275;
			b.Y = 50;
			b.Width = 125;
			b.Text = "Test Screen";
			b.Click += new GucEventHandler(b_Click1);

            //创建按钮并添加到屏幕，注册事件处理程序
			b = new GucButton();
			s1.Controls.Add(b);
			b.X = 400;
			b.Y = 50;
			b.Width = 100;
			b.Text = "Add";
			b.Click += new GucEventHandler(b_Click3);

            //创建按钮并添加的屏幕，注册事件处理程序
			b = new GucButton();
			s1.Controls.Add(b);
			b.X = 525;
			b.Y = 50;
			b.Width = 100;
			b.Text = "Delete";
			b.Click += new GucEventHandler(b_Click3);

            //创建选择框并添加到屏幕
			c = new GucCheckBox();
			s1.Controls.Add(c);
			c.X = 575;
			c.Y = 100;
			c.AutoSize = true;
			c.Width = 170;
			c.Height = 50;
			c.Text = "CheckBox Test";
			c.BackColor = Color.White;

            //创建状态列表并添加到屏幕
			sl = new GucStateList();
			s1.Controls.Add(sl);
			sl.X = 250;
			sl.Y = 200;
			sl.Items.Add(null, "Item 1");

            //创建组合框并添加到屏幕
			cb = new GucComboBox();
			s1.Controls.Add(cb);
			cb.X = 425;
			cb.Y = 200;
			cb.Items.Add(null, "Item 1");

            //创建3D显示区域并添加到屏幕，注册事件处理程序
			Guc3DGraphDisplay g = new Guc3DGraphDisplay();
			s1.Controls.Add(g);
			g.Draw3DGraphic += new GucEventHandler(g_Draw3DGraphic);
			g.BackColor = Color.Blue;
			g.X = 20;
			g.Y = 250;
			g.Width = g.Height = 220;
 //           g.AA = "display3D";

            //创建新的屏幕并添加按钮
			s2 = new Screen(manager);
            s2.Text = "SecondScreen";

			b = new GucButton();
			s2.Controls.Add(b);
			b.X = 275;
			b.Y = 50;
			b.Width = 125;
			b.Text = "Test Screen";
			b.Click += new GucEventHandler(b_Click2);
		}

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

			// TODO: use this.Content to load your game content here
            //载入3D模型，并获取绝对bone变换矩阵列表
			model = Content.Load<Model>("mothership");
			absoluteBoneTransforms = new Matrix[model.Bones.Count];
            //备份所有bone到parenbone的相对变换
			model.CopyAbsoluteBoneTransformsTo(absoluteBoneTransforms);

            //创建摄像机矩阵
            //视图矩阵：位置、朝向、上方
            //投影矩阵：视角、纵横比、近视界、远视界
            //纵宽比：Window.ClientBounds.Height / (float)Window.ClientBounds.Width
            view = Matrix.CreateLookAt(new Vector3(0, 0, 2), new Vector3(0, 0, 0), Vector3.Up);
			projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                Window.ClientBounds.Height / (float)Window.ClientBounds.Width, 1, 100);

            //角度与旋转矩阵
			angle = 0;
			rotation = Matrix.Identity;
		}

		Matrix view, projection, rotation;
		Matrix[] absoluteBoneTransforms;
		float angle;
		Model model;

		void g_Draw3DGraphic(GucControl sender)
		{
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
                    //对model的所有mesh，对mesh的所有effect执行如下操作：光照、视图与投影矩阵、世界矩阵
					effect.EnableDefaultLighting();
					effect.View = view;
					effect.Projection = projection;
                    //从bone集合中获取parentbone的索引
					effect.World = rotation * absoluteBoneTransforms[mesh.ParentBone.Index];
				}
				mesh.Draw();
			}
		}

		void b_Click1(GucControl sender) { s2.Show(); }

		void b_Click2(GucControl sender) { s1.Show(); }

        //Add或Delete事件
		void b_Click3(GucControl sender)
		{
			GucButton b = sender as GucButton;
			int ind;
			if (b.Text == "Add")
			{
				ind = new Random().Next(0, sl.Count + 1);
				sl.Items.Insert(ind, new GucStateCollectionItem(null, string.Format("Item {0}", sl.Count + 1)));
				cb.Items.Insert(ind, new GucStateCollectionItem(null, string.Format("Item {0}", cb.Count + 1)));
				l.Text = string.Format("Inserted item at {0}", ind);
			}
			else
			{
				if (sl.Count == 0) return;
				ind = new Random().Next(0, sl.Count);
				sl.Items.RemoveAt(ind);
				cb.Items.RemoveAt(ind);
				l.Text = string.Format("Deleted item at {0}", ind);
			}
		}

        //滑动条，滑动值
		void bar_ValueChanged(GucControl sender)
		{
			if ((sender as GucScrollBar).Value == 0)
				c.Text = "CheckBox Test ";
			else
				c.Text = "CheckBox Test " + (sender as GucScrollBar).Value;
		}
        
		GucLabel l;
		GucCheckBox c;
		GucStateList sl;
		GucComboBox cb;
		Screen s1, s2;

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
			KeyboardState ks = Keyboard.GetState();
			string text = "";
			foreach (Keys item in Enum.GetValues(typeof(Keys)))
			{
				if (ks.IsKeyDown(item))
					text += item.ToString() + "\n";
			}
			if (text != "") l.Text = text;
            //每次旋转一度并化为弧度，标准化到-pi/pi间，创建旋转矩阵
			angle -= MathHelper.ToRadians(1);
			angle = MathHelper.WrapAngle(angle);
			rotation = Matrix.CreateRotationZ(angle);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
