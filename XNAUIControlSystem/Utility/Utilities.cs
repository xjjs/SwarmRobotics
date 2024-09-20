using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
    //定义控件库所用的有关Update和Draw功能的接口，GucControl开始是继承IGucDraw的（后来被注释掉了）
	interface IGucUpdate
	{
		void Update(GameTime timeElapsed);
	}

	interface IGucDraw
	{
		bool Visible { get; set; }

		int Width { get; set; }
		int Height { get; set; }
		int X { get; set; }
		int Y { get; set; }

		Rectangle Region { get; }
		int Right { get; }
		int Bottom { get; }
		Vector2 Position { get; }
		Vector2 Size { get; }

		//Color FrontColor { get; set; }
		Color BackColor { get; set; }

		event GucEventHandler SizeChanged, PositionChanged;

		void Draw(SpriteBatch spriteBatch);
	}
}
/*******************************
 * 主要功能与实现：将XNA中重要的Update与Draw函数实现为接口
 * 1.工程的Update函数的接口，IGucUpdate
 * 2.工程的Draw函数的接口，IGucDraw，定义了一些通用字段：
 * 位置、宽高、可视性、背景；
 * 区域、位置、尺寸，尺寸与位置更改的事件；
********************************/