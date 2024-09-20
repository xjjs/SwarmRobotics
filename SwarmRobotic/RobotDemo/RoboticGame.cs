using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotDemo
{
	class RoboticGame : Game
	{
        //显卡管理器、缓冲宽高（窗口）、显卡默认的宽高（屏幕）
		public GraphicsDeviceManager graphics;
		int sizex, sizey, fullx, fully;

		public RoboticGame()
		{
            //根据显卡默认的宽高设置缓冲的宽高
			fullx = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; 
			fully = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			if (fullx > 1440 && fully > 900)
			{
				sizex = 1600;
				sizey = 1000;
			}
            //本机所用的分辨率
			else if (fullx > 1200 && fully > 800)
			{
				sizex = 1200;
				sizey = 800;
			}
			else
			{
				sizex = 1000;
				sizey = 750;
			}

            //创建显卡管理器、设置后备缓冲的宽高、资源管理器的根目录为“命名空间+Content”
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = sizex;
			graphics.PreferredBackBufferHeight = sizey;
			Content.RootDirectory = "Content";

            //固定时间步属性若为true，则游戏会按周期TargetElapsedTime(1/60s)来调用Update方法，未满周期则会继续调用Draw方法
            //若Update太长以至超过周期，则会将IsRunningSlowly设为true（可检测以设置合理参数），不调用Draw，而再次调用Update
            //将IsFixedTimeStep设为false，可使得XNA不按固定频率调用Update
			this.IsFixedTimeStep = false;
			this.IsMouseVisible = true;


		}

		protected override void Initialize()
		{
            ////关闭隐面消除功能
            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //GraphicsDevice.RasterizerState = rs;

            //创建“窗口管理器”，主数据是“窗口列表”，实现为一个游戏组件
			ScreenManager manager = new ScreenManager(this);
			this.Components.Add(manager);
			base.Initialize();

            //创建“主控窗口”并显示
			ControlScreen cs = new ControlScreen(manager);
			cs.Show();
		}

        //全屏与普通屏切换
		public void ToggleFullScreen()
		{
			if (graphics.IsFullScreen)
			{
				graphics.PreferredBackBufferWidth = sizex;
				graphics.PreferredBackBufferHeight = sizey;
			}
			else
			{
				graphics.PreferredBackBufferWidth = fullx;
				graphics.PreferredBackBufferHeight = fully;
			}
            //前面已经进行了尺寸转换，此处仅用来改变显卡mode
			graphics.ToggleFullScreen();
		}

		public bool isFullScreen { get { return graphics.IsFullScreen; } }
	}
}
