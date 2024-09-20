using System;
using System.Linq;
using GucUISystem;
using Microsoft.Xna.Framework;
using RobotLib;

namespace RobotDemo
{
    /// <summary>
    /// 继承自GucFrameBox，SR功能设置帧
    /// “参数项”->“参数设置页”->“功能设置帧”->“主控窗口”
    /// </summary>
	class SRFrame : GucFrameBox
	{
		Experiment experiment;
		RoboticEnvironment re;
		RoboticAlgorithm ra;
		RoboticProblem rp;

        //“仿真”屏幕
		SRScreen game;
		SRScreen[] games;

        //三个参数设置帧（页）
		TypeParaFrame algo, env, problem;

        //用于界面切换
		bool state, first = true;
		GucButton buttonPrev, buttonNext;

		public SRFrame(ControlScreen parent)
		{
			this.Parent = parent;
			AutoInnerSize = true;

            //添加“Previous”按钮
			buttonPrev = new GucButton();
			Controls.Add(buttonPrev);
			buttonPrev.Width = 100;
			buttonPrev.Text = "Previous";
			buttonPrev.Click += buttonPrev_Click;

            //添加“Next”按钮
			buttonNext = new GucButton();
			Controls.Add(buttonNext);
			buttonNext.Width = 100;
			buttonNext.Text = "Next";
			buttonNext.Click += buttonNext_Click;

            //添加“参数设置帧”：环境、问题、算法
			env = new TypeParaFrame(typeof(RoboticEnvironment), "Environment", this);
			env.TypeChanged += TypeChanged;
			problem = new TypeParaFrame(typeof(RoboticProblem), "Problem", this);
			problem.HeightChanged += HeightChanged;
            algo = new TypeParaFrame(typeof(RoboticAlgorithm), "Algorithm", this);
            algo.HeightChanged += HeightChanged;

			buttonPrev.X = env.Width + 40;
			buttonNext.X = buttonPrev.Right + 25;

            //设置各“参数设置帧”的背景颜色
			env.Page.BackColor = Color.LightGreen;
            problem.Page.BackColor = Color.LightYellow;
			algo.Page.BackColor = Color.LightPink;
            BackColor = Color.LightCyan;

			algo.Page.X = problem.Page.Right + 5;
			Width = algo.Page.Right;

            //利用“筛选串”指定默认的“环境”与“问题”的派生类型
			env.Filter("Compare");
			problem.Filter("Minimal");

            //设置state标识为true，first初始也为true，用以开启相关“参数设置帧（页）”
			state = true;
			SetState(false);

            //获取SRScreen的派生类型，创建对象后转为对象列表
            //Type[]后的中括号用于列举所需要的类型参数（若有多个则用','分隔）
			games = TypeParaFrame.FindDerivedTypesFromAssembly(typeof(SRScreen))
				.Select(t => t.GetConstructor(new Type[] { typeof(ControlScreen) }).Invoke(new object[] { parent }))
				.OfType<SRScreen>().ToArray();

            //创建所有“演示屏幕”的UI控件
			foreach (var g in games)
				g.CreateUI();
		}

        //调整“页面”的位置
		private void TypeChanged(TypeParaFrame page)
		{
            //“问题”页与“算法”页齐平
			problem.Page.Y = algo.Page.Y = env.Page.Bottom + 10;
			HeightChanged(page);
		}

		private void HeightChanged(TypeParaFrame page)
		{
            //获取低边界，设置“容器的内部高度”
			int h = Math.Max(algo.Page.Bottom, problem.Page.Bottom);
			InnerHeight = h + 10;
		}

        //处理“单击事件”
		private void buttonPrev_Click(GucControl sender) { SetState(false); }

		private void buttonNext_Click(GucControl sender)
		{
            //state为false时进入“演示界面”
			if (state)
			{
				if (GenerateGame())
				{
					//this.Hide();
					//this.DialogResult = DialogResult.OK;
					//this.Close();
                    //创建“仿真”界面的元素，并激活显示该窗口
					game.CreateUI();
					game.Show();
				}
			}
			else
				SetState(true);
		}

        //打开or关闭“参数设置页”
		void SetState() { SetState(!state); }

		bool SetState(bool isFinal)
		{
			if (state == isFinal) return false;
            //关闭“问题”页面，使能“算法”页面与“Previous”按钮
			if (isFinal)	//env & algo
			{
				rp = problem.GetTypeInstance() as RoboticProblem;
				if (rp == null) return false;
				problem.Page.Enable = false;
				buttonPrev.Enable = algo.Page.Enable = true;

                //使用两个参数or一个参数设置算法
				if (first)
				{
					algo.Filter("MLP", FilterAlgorithm);
					first = false;
				}
				else
					algo.Filter(select: FilterAlgorithm);
                //调整“算法”页的位置
				TypeChanged(algo);
			}
            //使能“问题”页面，关闭“算法”页面与“Previous”按钮
			else	//prob
			{
				problem.Page.Enable = true;
				buttonPrev.Enable = algo.Page.Enable = false;
				TypeChanged(problem);
			}
			state = isFinal;
			return true;
		}

        /// <summary>
        /// 创建游戏：创建算法与环境对象，进而创建实验对象，并绑定到某一个“演示屏幕”；
        /// </summary>
        /// <returns></returns>
		bool GenerateGame()
		{
			//Init Algorithm
			ra = algo.GetTypeInstance() as RoboticAlgorithm;
			if (ra == null) return false;

			//Init Environment
			re = env.GetTypeInstance() as RoboticEnvironment;
			if (re == null) return false;

			//Init Other
			experiment = new Experiment(re, ra, rp);
			//experiment.TestEnvrionment();
			//Init Game
			foreach (var g in games)
			{
				if (g.Bind(experiment))
				{
					game = g;
					return true;
				}
			}
			return false;
		}

        //将“类型项”转为“算法”类型并绑定“问题”数据，返回值是布尔类型
		bool FilterAlgorithm(TypeItem type) { return (type.Item as RoboticAlgorithm).Bind(rp); }

		public void DefaultOperation()
		{
			buttonNext_Click(this);
			buttonNext_Click(this);
		}

        //由“演示”界面切换到“设置”界面
		public void Reset()
		{
			//algo.RecreateInstances();
			//env.RecreateInstances();
			//problem.RecreateInstances();
			//experiment.Reset();
			state = !state;
			SetState(!state);
		}
	}
}
