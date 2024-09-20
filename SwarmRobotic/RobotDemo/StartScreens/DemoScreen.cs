using System;
using System.IO;
using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RobotDemo.Display;
using RobotLib;
using System.Collections.Generic;

namespace RobotDemo
{
    /// <summary>
    /// 继承自Screen，演示窗口；
    /// </summary>
	abstract class DemoScreen : Screen
	{
        //操作面板
		protected GucFrameBox Panel;
        //标签控件
		protected GucLabel lblFin;
		GucLabel lblInfo;
        //状态列表
		protected GucStateList stateRun, stateView;
        //3D显示区域
		protected Guc3DGraphDisplay Display3D;

        //摄像机
		protected Camera camera;
		//protected Matrix projection;//rotate;

		int speed = -1, oldspeed = 10, step = 0, fig = 0;
		int tempfps = 0, lasttime = 0;
		protected int fps = 0;

        //截屏编号
		int screenshotId = 0;

        //game对象、UI创建标识、主控窗口
		RoboticGame game;
		bool CreatedUI;
		ControlScreen ctrlScreen;

		public string Title { get; set; }

		protected string InfoText { get; set; }

		public DemoScreen(ControlScreen ctrlScreen)
			: base(ctrlScreen.Manager)
		{
            //绑定“主控窗口”与游戏对象
			this.ctrlScreen = ctrlScreen;
			Title = "";

            //可通过组件获取游戏对象
			game = Manager.Game as RoboticGame;
			CreatedUI = false;

            //创建摄像机对象
			camera = new Camera();
		}

		public void CreateUI()
		{
            //保证只创建一次UI
			if (CreatedUI) return;
			CreatedUI = true;

            //创建与添加3D显示模块，与窗口同高的正方形（此处设置的分辨率而非纹理），注册事件处理函数
			Display3D = new Guc3DGraphDisplay();
			Controls.Add(Display3D);
			Display3D.Width = Display3D.Height = Height;
			Display3D.Draw3DGraphic += new GucEventHandler(Display3D_Draw3DGraphic);

            //创建与添加控制面板，与窗口同高
			Panel = new GucFrameBox();
			Controls.Add(Panel);
			Panel.AutoInnerSize = true;
			Panel.Height = Panel.X = Height;
			Panel.Width = Width - Height;
            Panel.BackColor = Color.Transparent; //Color.LightGreen;


			GucButton button;
			int bottom = 10;
			int left = 10;

            //创建与添加按钮，注册事件处理函数
			button = new GucButton(0);
			button.Text = "Return to Setup";
			button.AutoFit();
			button.Click += new GucEventHandler(button_Click);
			Panel.Controls.Add(button);

            //创建与添加“运行方式列表”
			stateRun = new GucStateList();
			stateRun.CheckedTexutre = Skin.CheckBoxChecked;
			stateRun.NormalTexutre = Skin.CheckBoxNormal;
			stateRun.Width = Panel.InnerWidth - 20;
            //设置标题、添加“CheckBox列表”，注册事件处理函数
            //单击CheckBox的按钮控件会触发本控件的“选中状态改变”事件
			stateRun.Text = "Running Speed";
            //第一个参数相当于speed，即当step到达speed时更新一次实验，故speed越小则仿真越快
			stateRun.Items.Add(-1, "Pause");
			stateRun.Items.Add(60, "Slow");
			stateRun.Items.Add(10, "Normal");
			stateRun.Items.Add(3, "Quick");
			stateRun.Items.Add(1, "Ultra");
			stateRun.SelectedChanged += new GucEventHandler(stateRun_SelectedChanged);
			Panel.Controls.Add(stateRun);

            //结束状态的指示标签
			lblFin = new GucLabel();
			lblFin.Text = "Finished!";
			lblFin.Visible = false;
			Panel.Controls.Add(lblFin);

            //创建与添加“视图状态列表”，直角坐标or透视坐标
			stateView = new GucStateList();
			stateView.CheckedTexutre = Skin.CheckBoxChecked;
			stateView.NormalTexutre = Skin.CheckBoxNormal;
			stateView.Width = Panel.InnerWidth - 20;
            //设置标题、添加“CheckBox列表”，注册事件处理函数
            //单击CheckBox的按钮会触发本控件的“选中状态改变”的事件
			stateView.Text = "View Type";
			stateView.Items.Add(false, "Perspective");
			stateView.Items.Add(true, "Orthographic");
			stateView.SelectedChanged += new GucEventHandler(stateView_SelectedChanged);
			if (camera.Orthographic) stateView.SelectedIndex = 1;
			Panel.Controls.Add(stateView);

            //创建UI的状态显示，只是本类中尚未实现该方法
			CreateCustomUIStates();
			//bottom = stateRun.Bottom;

            //设置面板上所有控件的位置
			foreach (var item in Panel.Controls)
			{
				if (item.Visible)
				{
					item.X = left;
					item.Y = bottom;
					bottom = item.Bottom;
				}
			}
			lblFin.Position = stateRun.Position;
			bottom += 8;

            //创建与添加“控制面板”，此处不是“状态列表”而是“按钮”，注册“按钮切换”事件
			button = new GucButton(0);
			button.Text = "- Control Panel";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.Click += this.ToggleControlPanel;
			Panel.Controls.Add(button);

            //Up/Down/Left/Right：分别是地图“上/下/左/右”移动
            //Zoom-in与Zoom-out：分别是放大与缩小功能
			left = stateRun.X;
			bottom = button.Bottom;
			button = new GucButton(0);
			button.Text = "Up";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.MoveUp);
			Panel.Controls.Add(button);

            //创建与添加Down按钮，注册摄像机“下移”事件处理程序
			left = button.Right;
			button = new GucButton(0);
			button.Text = "Down";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.MoveDown);
			Panel.Controls.Add(button);

            //创建与添加Left按钮，注册摄像机“左移”事件处理程序
			left = button.Right;
			button = new GucButton(0);
			button.Text = "Left";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.MoveLeft);
			Panel.Controls.Add(button);
            
            //创建与添加Right按钮，注册摄像机“右移”事件处理程序
			left = button.Right;
			button = new GucButton(0);
			button.Text = "Right";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.MoveRight);
			Panel.Controls.Add(button);

 
			left = stateRun.X;
			bottom = button.Bottom;
			button = new GucButton(0);
			button.Text = "Z+";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.MoveZUp);
			Panel.Controls.Add(button);

			left = button.Right;
			button = new GucButton(0);
			button.Text = "Pitch+";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.PitchUp);
			Panel.Controls.Add(button);

			left = button.Right;
			button = new GucButton(0);
			button.Text = "Yaw+";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.YawUp);
			Panel.Controls.Add(button);

			left = stateRun.X;
			bottom = button.Bottom;
			button = new GucButton(0);
			button.Text = "Z-";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.MoveZDown);
			Panel.Controls.Add(button);

			left = button.Right;
			button = new GucButton(0);
			button.Text = "Pitch-";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.PitchDown);
			Panel.Controls.Add(button);

			left = button.Right;
			button = new GucButton(0);
			button.Text = "Yaw-";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.YawDown);
			Panel.Controls.Add(button);

			left = stateRun.X;
			bottom = button.Bottom;
			button = new GucButton(0);
			button.Text = "Zoom In";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.ZoomIn);
			Panel.Controls.Add(button);

			left = button.Right;
			button = new GucButton(0);
			button.Text = "Zoom Out";
			button.AutoFit();
			button.X = left;
			button.Y = bottom;
			button.MousePress += new DelegateWrapper(camera.ZoomOut);
			Panel.Controls.Add(button);

            //创建与添加标签按钮，用于显示状态信息
			lblInfo = new GucLabel();
            lblInfo.BackColor = Color.LightPink;
			lblInfo.X = stateRun.X;
			lblInfo.Y = button.Bottom + 8;
			Panel.Controls.Add(lblInfo);
		}

		protected virtual void CreateCustomUIStates() { }

        //切换控制面板
		void ToggleControlPanel(GucControl sender)
		{
			var button = sender as GucButton;
			if (button.Text.StartsWith("-"))
			{
                //收起列表，控件设为不可视
				button.Text = "+ Control Panel";
				foreach (var item in Panel.Controls)
				{
					if (item is GucButton && item.Y > button.Y && item.Y < lblInfo.Y)
						item.Visible = false;
				}
                //重设标签控件位置
				lblInfo.Tag = lblInfo.Y;
				lblInfo.Y = button.Bottom + 10;
			}
			else
			{
                //展开列表，控件设为可视
				button.Text = "- Control Panel";
				foreach (var item in Panel.Controls)
				{
					if (item is GucButton)
						item.Visible = true;
				}
                //重设标签控件位置
				lblInfo.Y = (int)lblInfo.Tag;
			}
		}

        //重置状态列表与摄像机
		public void Reset()
		{
			stateRun.SelectedIndex = 0;
			camera.Reset();
			lblFin.Visible = false;
			stateRun.Visible = true;
		}

        //重新设置3D显示区域、控制面板、状态列表的尺寸
		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			if (Display3D != null)
			{
				Display3D.Width = Display3D.Height = Height;
				Panel.Height = Panel.X = Height;
				Panel.Width = Width - Height;
				stateRun.Width = Panel.InnerWidth - 20;
				stateView.Width = Panel.InnerWidth - 20;
			}
		}

        //输入事件处理函数，内部会调用摄像机的“键盘事件处理函数”
		protected override void OnParseInput(InputEventArgs input)
		{
            //Escape按键由“演示界面”到“设置界面”
			if (input.isKeyDown(Keys.Escape))
			{
				button_Click(null);
				return;
			}

            //按键“P”用于“暂停仿真”
			if (input.isKeyDown(Keys.P))
			{
				if (speed > 0)
					stateRun.SelectedIndex = 0;
				else
					stateRun.SelectedItem = (oldspeed > 0 ? oldspeed : 10);
			}

            //按键“I”用于关闭“状态显示”
			if (input.isKeyDown(Keys.I)) lblInfo.Visible = !lblInfo.Visible;
			if (input.isKeyDown(Keys.OemTilde)) stateRun.SelectedIndex = 0;
			if (input.isKeyDown(Keys.D1)) stateRun.SelectedIndex = 1;
			if (input.isKeyDown(Keys.D2)) stateRun.SelectedIndex = 2;
			if (input.isKeyDown(Keys.D3)) stateRun.SelectedIndex = 3;
			if (input.isKeyDown(Keys.D4)) stateRun.SelectedIndex = 4;

            //按键“F5”用于重置演示界面与摄像机
			if (input.isKeyDown(Keys.F5))
			{
				stateRun.SelectedIndex = 0;
				//experiment = new Experiment(experiment.environment, experiment.algorithm, experiment.environment.mapProvider);
				ResetDemo();
				camera.Reset();
				lblFin.Visible = false;
				stateRun.Visible = true;
			}
            //按键“T”进入单步仿真状态
			if (input.isKeyPress(Keys.T))
			{
				stateRun.SelectedIndex = 0;
				StepDemo();
			}

            //按键“C”用于存储截屏信息
			if (input.isKeyDown(Keys.C))
			{
				TakeScreenShot(input.Shift);
			}

            //Alt&&Enter用于进入全屏，不过貌似有bug
			if ((input.Alt && input.isKeyDown(Keys.Enter)) || (game.isFullScreen && input.isKeyDown(Keys.Escape)))
			{
				game.ToggleFullScreen();
			}

            //触发摄像机的“输入参数解析事件”并更新摄像机
			camera.ParseInput(input);
			camera.UpdateCamera();

            //每次更新tempfps都累加，当累加到一定程度后保存显示，并清零
			lasttime += input.LastUpdateTime;
			if (lasttime >= 1000)
			{
				fps = tempfps;
				tempfps = 0;
				lasttime -= 1000;

			}
			tempfps++;
			Text = string.Format("{1} FPS:{0}", fps, Title);

            fig++;
            if (fig % 5 == 0)
            {
   //             TakeScreenShot(input.Shift);
            }


            //非pause状态则进入循环
			if (speed > 0)
			{
				step++;



                //当累计步数step到达speed则更新实验
				if (step == speed)
				{
					StepDemo();

					if (Finished)
					{
						stateRun.Visible = false;
						lblFin.Visible = true;
						stateRun.SelectedIndex = 0;
					}
					step = 0;
				}
			}

			CustomUpdate(input);
			lblInfo.Text = InfoText;
			Panel.FitInnerSize();
		}

		protected abstract void ResetDemo();
		protected abstract void StepDemo();
		protected abstract bool Finished { get; }

        //主控窗口重置与显示，“演示”界面切换到“设置”界面
		void button_Click(GucControl sender)
		{
			ctrlScreen.Reset();
			ctrlScreen.Show();
		}

        //视图“状态列表”改变
		void stateView_SelectedChanged(GucControl sender) { camera.Orthographic = (bool)stateView.SelectedItem; }

        //运行“状态列表”改变
		void stateRun_SelectedChanged(GucControl sender)
		{
			var cur = (int)stateRun.SelectedItem;
			if (speed == cur) return;
			if (cur == -1)
			{
				oldspeed = speed;
			}
			speed = cur;
			step = 0;
		}

        //更新与3D区域绘制
		protected abstract void CustomUpdate(InputEventArgs input);
        
		protected abstract void Display3D_Draw3DGraphic(GucControl sender);

		//protected virtual void CustomDraw() { }
        //存储截屏文件：可全屏存储、可只存储地图
		void TakeScreenShot(bool infobar)
		{
			string filename;
			do
			{
				filename = "screenshot" + screenshotId++ + ".jpg";
			} while (File.Exists(filename));
			Stream s = File.OpenWrite(filename);
			if(infobar)
				renderBuffer.SaveAsJpeg(s, Width, Height);
			else
				Display3D.renderBuffer.SaveAsJpeg(s, Display3D.Width, Display3D.Height);
			s.Close();
		}
	}

    /// <summary>
    /// 包装委托以匹配事件类型；
    /// 隐式类型转换：“包装类型”可被隐式转换为“事件处理程序”，转换的方式就是调用Call方法；
    /// </summary>
	class DelegateWrapper
	{
		Action act;
		public DelegateWrapper(Action action) { act = action; }

		public void Call(GucControl sender, MouseButtons button, Point pos) { act(); }

		public static implicit operator GucEventHandler<MouseButtons, Point>(DelegateWrapper obj) { return obj.Call; }
	}
}