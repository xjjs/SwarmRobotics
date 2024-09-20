using Microsoft.Xna.Framework;

namespace GucUISystem
{
    /// <summary>
    /// This is a game component that implements IUpdateable and IDrawable.
    /// 继承自GucContainerControl，并未添加新的控件成员；
    /// </summary>
	public class Screen : GucContainerControl
	{
		public ScreenManager Manager { get; set; }

		public Screen(ScreenManager manager)
			: base(0, 0, 0, 0)
		{
			if (manager.GraphicsDevice != null) BindGraphic(manager.GraphicsDevice);
			this.AutoInnerSize = true;
			manager.Add(this);
			var size = manager.Game.Window.ClientBounds;
//            var size = manager.Game.GraphicsDevice.Viewport;

            //用于说明工程的游戏仿真窗口只有一个，所有的Screen都是控件
            //size.Width = size.Width / 2;
            //size.Height = size.Height / 2;

			base.Width = size.Width;
			base.Height = size.Height;
			InnerWidth = size.Width;
			InnerHeight = size.Height;
			text = "";
			BackColor = Color.Green; //LightBlue
		}

		public void Show() { Manager.Show(this); }

		public override int Height
		{
			get { return base.Height; }
			set { }
		}

		public override int Width
		{
			get { return base.Width; }
			set { }
		}

		public override int X
		{
			get { return base.X; }
			set { }
		}

		public override int Y
		{
			get { return base.Y; }
			set { }
		}

		public override Vector2 Size
		{
			get { return base.Size; }
			set { }
		}

		public event GucEventHandler TitleChanged;
		string text;
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				if (TitleChanged != null) TitleChanged(this);
			}
		}

		public void Exit() { Manager.Game.Exit(); }
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucContainerControl（继承）
***********************************/