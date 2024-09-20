using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
	/// <summary>
	/// ToolTip > Border > TopLayer > ActiveControl > ChildrenControl > BottomLayer > Background
    /// 绘制层次，用浮点数表示深度（越小越靠前）
	/// </summary>
	public static class DrawingDepth
	{
		public const float ToolTip = 0.1f;
		public const float Border = 0.2f;
		public const float TopLayer = 0.3f;
		public const float ActiveControl = 0.4f;
		public const float ChildrenControl1 = 0.45f;
		public const float ChildrenControl2 = 0.5f;
		public const float ChildrenControl3 = 0.55f;
		public const float ChildrenControl4 = 0.6f;
		public const float BottomLayer = 0.7f;
		public const float Background = 0.8f;
	}
    
    /// <summary>
    /// 控件的region区域
    /// 主要属性：颜色、深度、显示开关，精灵的原点（图片的内部坐标系的位置、旋转中心）、旋转因子
    /// 主要方法：精灵绘制、点包含判断
    /// </summary>
	public abstract class ControlDrawRegion
	{
		public ControlDrawRegion(float depth = DrawingDepth.ChildrenControl3)
		{
			Color = Color.White;
			Show = true;
			Depth = depth;
		}

		public abstract void Draw(SpriteBatch spriteBatch);

		public abstract bool Contains(Point pos);

        //所绘制的精灵的原点，用于指示旋转与缩放等，该点是图片的内部坐标系，而非相对于屏幕的坐标系
		public Vector2 Origin;
		public Color Color;
		public float Rotation, Depth;
		public bool Show;
	}

    /// <summary>
    /// 浮点region区域（控制放缩）
    /// 派生属性：纹理、纹理的“选择矩形”、屏幕上的绘制起点与缩放因子
    /// 派生方法：无
    /// </summary>
	public class ControlDrawRegionFloat : ControlDrawRegion
	{
		public ControlDrawRegionFloat(Texture2D texture, Rectangle source, float depth = DrawingDepth.ChildrenControl3)
			: base(depth)
		{
			Texture = texture;
			Source = source;
		}
        //Draw的参数：纹理、绘制起点、纹理的选择矩形（null则选整个纹理）、着色（White为原色）
        //纹理的旋转弧度（绕其center）、精灵的原点（默认为(0,0)）、缩放因子、使用的Effect、绘制层次
		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, DrawPos, texSource, Color, Rotation, Origin, Scale, SpriteEffects.None, Depth);
		}

        //判断点是否则缩放后的矩形内，被缩放的是选择矩形
		public override bool Contains(Point pos)
		{
			float dx = pos.X - DrawPos.X, dy = pos.Y - DrawPos.Y;
			return dx >= 0 && dy >= 0 && dx <= Scale.X * texSize.X && dy <= Scale.Y * texSize.Y;
		}

        //纹理、绘制起点位置与缩放因子
		public Texture2D Texture;
		public Vector2 DrawPos;
		public Vector2 Scale;

		//Vector2 displaySize;
		//public Vector2 DrawSize
		//{
		//    get { return displaySize; }
		//    set
		//    {
		//        displaySize = value;
		//        Scale = displaySize / texSize;
		//    }
		//}
		Rectangle texSource;
		Vector2 texSize;
		public Rectangle Source
		{
			get { return texSource; }
			set
			{
				texSource = value;
				texSize = new Vector2(Source.Width, Source.Height);
			}
		}
		//public Vector2 Scale { get; private set; }
	}

    /// <summary>
    /// 整型region区域（自适应放缩）
    /// 派生属性：纹理、纹理的“选择矩形”、屏幕上的绘制矩形（选择的纹理会自动缩放匹配）
    /// 派生方法：无
    /// </summary>
	public class ControlDrawRegionInt : ControlDrawRegion
	{
		public ControlDrawRegionInt(Texture2D texture, Rectangle source, float depth = DrawingDepth.ChildrenControl3)
			: base(depth)
		{
			Texture = texture;
			Source = source;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Destination, Source, Color, Rotation, Origin, SpriteEffects.None, Depth);
		}

		public override bool Contains(Point pos) { return Destination.Contains(pos); }

		public Texture2D Texture;
		public Rectangle Destination;
		public Rectangle Source;
	}


    
    /// <summary>
    /// 字符串型region区域
    /// 派生属性：字体、文本、文本尺寸、屏幕上的绘制起点
    /// 派生方法：无
    /// </summary>
	public class ControlDrawRegionString : ControlDrawRegion
	{
		public ControlDrawRegionString(SpriteFont font)
		{
			Font = font;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawString(Font, text, DrawPos, Color, Rotation, Origin, 1, SpriteEffects.None, Depth);
		}

		public override bool Contains(Point pos)
		{
			float dx = pos.X - DrawPos.X, dy = pos.Y - DrawPos.Y;
			return dx >= 0 && dy >= 0 && dx <= TextSize.X && dy <= TextSize.Y;
		}

		public SpriteFont Font { get; private set; }
		string text;
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				TextSize = Font.MeasureString(text);
			}
		}
		public Vector2 DrawPos;
		Vector2 TextSize;
	}
}

/*************************
 * 主要功能与实现
 * 1.定义DrawingDepth类，以确定绘图层次，0表示最上层、1表示最下层；
 * 2.定义ControlDrawRegion类，原点、颜色、显示性、角度、层次，
 * 绘制函数、点检测函数；
 * 3.纹理、位置、比例、尺寸，
 * 4.字体、位置、文本、尺寸；
**************************/