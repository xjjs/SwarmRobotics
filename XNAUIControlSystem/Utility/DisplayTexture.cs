using Microsoft.Xna.Framework;

namespace GucUISystem
{
	public enum DisplaySkinType
	{
		Normal, Hover, Pressed
	}

	public struct DisplayTexture
	{
		public Rectangle Normal, Hover, Pressed;

		public Rectangle this[DisplaySkinType type]
		{
			get
			{
				switch (type)
				{
					case DisplaySkinType.Normal: return Normal;
					case DisplaySkinType.Hover: return Hover;
					//case DisplaySkinType.Pressed:
					default: return Pressed;
				}
			}
		}
	}
}
/***************
 * 主要功能与实现
 * 1.定义皮肤显示模式的枚举；
 * 2.定义显示纹理的结构体，根据枚举值返回相应矩形；
 * 3.该结构体有三个矩形成员；
 * 4.this[]可用于声明索引器，相当于[]重载，displayTexture[0]指的就是Normal矩形；
****************/