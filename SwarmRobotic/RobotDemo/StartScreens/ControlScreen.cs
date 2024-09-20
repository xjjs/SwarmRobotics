using System;
using System.Linq;
using GucUISystem;
using Microsoft.Xna.Framework;
using RobotLib;

namespace RobotDemo
{
    /// <summary>
    /// 管理窗口：继承自Screen；
    /// 控件的基本依赖关系：“参数项”->“参数设置页”->“功能设置帧”->“主控窗口”
    /// </summary>
	class ControlScreen : Screen
	{
        //功能按钮：退出、载入群体机器人问题帧、载入优化问题帧
		GucButton buttonExit, buttonSR, buttonOpt;
        //功能设置帧（功能上是一个Container型组件）：群体机器人功能设置帧、优化功能设置帧
		SRFrame frameSR;
		OptFrame frameOpt;

		public ControlScreen(ScreenManager manager)
			:base(manager)
		{
            //添加SR问题设置帧
			frameSR = new SRFrame(this);
			Controls.Add(frameSR);
            frameSR.X = Math.Max(0, (this.Width - 270 - frameSR.Width) / 2);
			frameSR.Y = 150;
            frameSR.SizeChanged += new GucEventHandler(ChildSizeChanged);

            //添加Opt问题设置帧
			frameOpt = new OptFrame(this);
			Controls.Add(frameOpt);
            frameOpt.X = Math.Max(0, (this.Width - 270 - frameOpt.Width) / 2);
			frameOpt.Y = 150;
            frameOpt.SizeChanged += new GucEventHandler(ChildSizeChanged);

            //添加退出按钮
			buttonExit = new GucButton();
			Controls.Add(buttonExit);
			buttonExit.Width = 225;
			buttonExit.Text = "Exit";
			buttonExit.Click += buttonExit_Click;
			buttonExit.X = 600;

            //添加SR选项按钮
			buttonSR = new GucButton();
			Controls.Add(buttonSR);
			buttonSR.Width = 225;
			buttonSR.Text = "Swarm Robotic Demo";
			buttonSR.Click += buttonSR_Click;
			buttonSR.X = 50;

            //添加Opt选项按钮
			buttonOpt = new GucButton();
			Controls.Add(buttonOpt);
			buttonOpt.Width = 225;
			buttonOpt.Text = "Swarm Intelligence Demo";
			buttonOpt.Click += buttonOpt_Click;
			buttonOpt.X = 325;

            buttonExit.Y = buttonOpt.Y = buttonSR.Y = 50;

            //激活SR选项按钮，设置窗口名称
			buttonSR_Click(buttonSR);
			//buttonOpt_Click(buttonOpt);
			Text = "Experiment Setup";

            //调整内部尺寸以容纳子控件列表
            FitInnerSize();
		}

        void ChildSizeChanged(GucControl sender) { FitInnerSize(); }

		private void buttonExit_Click(GucControl sender) { Exit(); }

        //激活SR选项帧，关闭Opt选项帧，切换被单击按钮的背景颜色
		private void buttonSR_Click(GucControl sender)
		{
			frameOpt.Visible = false;
			frameSR.Visible = true;
			buttonOpt.BackColor = Color.White;
			buttonSR.BackColor = Color.LightBlue;
		}

        //激活Opt选项帧，关闭SR选项帧，切换被单击按钮的背景颜色
		private void buttonOpt_Click(GucControl sender)
		{
			frameOpt.Visible = true;
			frameSR.Visible = false;
			buttonOpt.BackColor = Color.LightBlue;
			buttonSR.BackColor = Color.White;
		}

		public void DefaultOperation() { frameSR.DefaultOperation(); }

		public void Reset()
		{
			frameSR.Reset();
		}
	}
}
