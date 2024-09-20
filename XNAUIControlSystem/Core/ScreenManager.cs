using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        List<Screen> screens;
        InputService input;
		SpriteBatch spriteBatch;
		Screen active;

        public ScreenManager(Game game)
            : base(game)
        {
            UpdateOrder = 10;
            screens = new List<Screen>();
            input = new InputService(this);
			game.Components.Add(input);
			active = null;
			Game.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
			Game.Activated += new EventHandler<EventArgs>(Game_Activated);
			Game.Deactivated += new EventHandler<EventArgs>(Game_Deactivated);
        }

		void Game_Activated(object sender, EventArgs e) { if (active != null && !active.IsActive) active.Activate(); }

		void Game_Deactivated(object sender, EventArgs e) { if (active != null) active.DeActivate(); }

		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			if (active != null) 
				(active as GucControl).Size = new Vector2(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
		}

		public void Add(Screen screen)
		{
			screens.Add(screen);
			screen.Manager = this;
		}

        //显示某窗口对象
		public void Show(Screen screen)
		{
            //若该窗口已激活or不在管理列表内则直接返回
			if (active == screen || !screens.Contains(screen)) return;
            //若已有激活窗口则关闭激活
			if (active != null)
			{
				active.DeActivate();
				active.TitleChanged -= active_TitleChanged;
			}
            //将该窗口设为激活窗口并激活
			active = screen;
			if (active != null)
			{
				active.Activate();
				Game.Window.Title = active.Text;
				active.TitleChanged += active_TitleChanged;
			}
			else
				Game.Window.Title = "";
		}

		void active_TitleChanged(GucControl sender) { Game.Window.Title = (sender as Screen).Text; }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
			foreach (var screen in screens)
				screen.BindGraphic(GraphicsDevice);
        }

		protected override void LoadContent()
		{
			base.LoadContent();
			GucControl.Skin = Skin.LoadSkin(@"Skin", Game.Content);
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
			if (active != null) active.ParseInput(input.CreateInputArgs());
            base.Update(gameTime);
        }

		public override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			if (active != null)
			{
                //绘制激活的窗口，Screen没有自己的Draw，进一步回溯到Container
                //而Container会平等调用所有子控件的Draw函数（不考虑激活）
				active.Draw();
				spriteBatch.Begin();
				spriteBatch.Draw(active.renderBuffer, active.Region, Color.White);
				spriteBatch.End();
			}
			base.Draw(gameTime);
		}
    }
}
/**********************************
 * 主要功能与实现
 * 1.scree列表，被激活screen
 * 2.输入数据分析
 * 3.精灵批处理对象
***********************************/