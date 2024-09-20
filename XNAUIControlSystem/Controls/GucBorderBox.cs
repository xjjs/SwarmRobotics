using Microsoft.Xna.Framework;

namespace GucUISystem
{
    /// <summary>
    /// 继承自GucControl，分为9个绘制区域实现：设想一个有边缘的按钮，将边缘内部矩形的四条边延长则可将整个区域分成9部分
    /// </summary>
	public class GucBorderBox : GucControl
	{
       
		readonly int HMargin, VMargin, Border, Border2;
        //设想一个有边缘的按钮，将边缘内部矩形的四条边延长则可将整个区域分成9部分，由左上角开始顺时针各部分编码如下
		//0-NW 1-N 2-NE 3-E 4-SE 5-S 6-SW 7-W 8-C
		//protected Rectangle[] source, destination;
		public ControlDrawRegionInt[] regions { get; private set; }

		Point minCSize;
		public Point minCenterSize
		{
			get { return minCSize; }
			set
			{
				minCSize = value;
				MinSize = new Point(minCenterSize.X + Border2 + HMargin, minCenterSize.Y + Border2 + VMargin);
			}
		}
		public Vector2 CenterSize { get; private set; }
		public Point CenterPosition { get; private set; }

		Color centerColor;
		public Color CenterColor
		{
			get { return centerColor; }
			set
			{
				centerColor = value;
				regions[8].Color = value;
			}
		}

		bool drawCenter;
		public bool DrawCenter
		{
			get { return drawCenter; }
			set
			{
				drawCenter = value;
				regions[8].Show = value;
			}
		}

		public GucBorderBox(int HMargin = 0, int VMargin = 0, int Border = 5)
		{
			this.HMargin = HMargin * 2;
			this.VMargin = VMargin * 2;
			this.Border = Border;
			this.Border2 = Border * 2;
			centerColor = Color.White;
			CenterPosition = new Point(HMargin + Border, VMargin + Border);
			drawCenter = true;
            //9个控制区域
			regions = new ControlDrawRegionInt[9];
		}

		public void Initialize(Rectangle TextureWhole, int BorderSize = 0) 
        { Initialize(TextureWhole, new Rectangle(BorderSize, BorderSize, BorderSize, BorderSize)); }
		public void Initialize(Rectangle TextureWhole, Rectangle BorderSize)
		{
            //采用矩形数据结构只是用来方便存储信息
			Rectangle[] source, destination;
			source = new Rectangle[9];
            //记录各个方向的起点与长度
			//Rectangle TextureWhole = Skin.TextBox, BorderSize = Skin.TextBoxMargin;
			//source[7].X = source[6].X = source[2].Y = source[1].Y = source[0].X = source[0].Y = 0;	//--------------------------X of W, Y of E
			source[8].X = source[5].X = source[1].X = source[6].Width = source[7].Width = source[0].Width = BorderSize.X;	//----Width of W, X of N/S.C
			source[8].Y = source[7].Y = source[3].Y = source[2].Height = source[1].Height = source[0].Height = BorderSize.Y;	//Height of N, Y of E/W.C
			source[4].Width = source[3].Width = source[2].Width = BorderSize.Width;	//--------------------------------------------Width of E
			source[6].Height = source[5].Height = source[4].Height = BorderSize.Height;	//----------------------------------------Height of S
			source[4].X = source[3].X = source[2].X = TextureWhole.Width - BorderSize.X;	//------------------------------------X of E
			source[6].Y = source[5].Y = source[4].Y = TextureWhole.Height - BorderSize.Y;	//------------------------------------Y of S
			source[8].Width = source[5].Width = source[1].Width = TextureWhole.Width - BorderSize.X - BorderSize.Width;	//--------Width of C
			source[8].Height = source[7].Height = source[3].Height = TextureWhole.Height - BorderSize.Y - BorderSize.Height;	//Height of C

			destination = new Rectangle[9];
			destination[1].X = destination[5].X = destination[8].X = destination[0].Width = destination[2].Width =
				destination[3].Width = destination[4].Width = destination[6].Width = destination[7].Width = Border;
			destination[3].Y = destination[7].Y = destination[8].Y = destination[0].Height = destination[1].Height =
				destination[2].Height = destination[4].Height = destination[5].Height = destination[6].Height = Border;
			//destination[0].X = destination[0].Y = destination[1].Y = destination[2].Y = destination[6].X = destination[7].X = 0;

			for (int i = 0; i < 9; i++)
			{
                //相对位置变绝对位置
				source[i].X += TextureWhole.X;
				source[i].Y += TextureWhole.Y;
				if (regions[i] == null)
				{
					regions[i] = new ControlDrawRegionInt(Skin.Texture, source[i], i == 8 ? DrawingDepth.Background : DrawingDepth.Border);
					regions[i].Destination = destination[i];
				}
				else
					regions[i].Source = source[i];
			}
			regions[8].Color = centerColor;
			regions[8].Show = drawCenter;
			//CustomDrawRegions.Add(regions[5]);
			CustomDrawRegions.AddRange(regions);
		}

		protected override void OnSizeChange()
		{
			CenterSize = new Vector2(Width - Border2 - HMargin, Height - Border2 - VMargin);
			//destination[1].Width = destination[5].Width = destination[8].Width = Width - Border2;
			//destination[3].Height = destination[7].Height = destination[8].Height = Height - Border2;
			//destination[2].X = destination[3].X = destination[4].X = Width - Border;
			//destination[4].Y = destination[5].Y = destination[6].Y = Height - Border;
			regions[1].Destination.Width = regions[5].Destination.Width = regions[8].Destination.Width = Width - Border2;
			regions[3].Destination.Height = regions[7].Destination.Height = regions[8].Destination.Height = Height - Border2;
            //目标区域的终点坐标？起点坐标应该是Border和Border
			regions[2].Destination.X = regions[3].Destination.X = regions[4].Destination.X = Width - Border;
			regions[4].Destination.Y = regions[5].Destination.Y = regions[6].Destination.Y = Height - Border;
		}
	}
}

/**********************************
 * 主要功能与实现
 * 1.不使用其他控件成员
 * 2.分为9个绘制区域实现：设想一个有边缘的按钮，将边缘内部矩形的四条边延长则可将整个区域分成9部分
***********************************/