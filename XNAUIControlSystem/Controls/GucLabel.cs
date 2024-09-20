using Microsoft.Xna.Framework;


namespace GucUISystem
{

	public class GucLabel : GucControl
	{
        //标签控件要绘制的区域列表：文本区域、光标区域、选择区域
        ControlDrawRegionString DrawRegionText;
        ControlDrawRegionFloat DrawRegionCursor, DrawRegionSelect;

        
        
        //文本：最重要的字段
		string text;
		public string Text
		{
			get { return text; }
			set
			{
				if (text == value) return;
				text = value;
                //调整到文本自身的尺寸
				if (autoSize) FitToSize();
				DrawRegionText.Text = value;
				RequireRedraw = true;
			}
		}
		//自动尺寸开关
		bool autoSize;
		public bool AutoSize
		{
			get { return autoSize; }
			set
			{
				autoSize = value;
				if (autoSize) FitToSize();
			}
		}
        //字体颜色、区域文本颜色
		Color fontColor;
		public Color FontColor
		{
			get { return fontColor; }
			set
			{
				fontColor = value;
				DrawRegionText.Color = value;
				RequireRedraw = true;
			}
		}

		//Vector2 txtPos;
        //文本区域的原点
		public float Offset
		{
			get { return DrawRegionText.Origin.X; }
			set { DrawRegionText.Origin.X = value; }
		}

        //光标：频率、时间、绘制开关、使能开关
		const int CursorFreq = 500;
		int cursorTime;
		bool drawCursor, enableCursor;
		public bool EnalbeCursor
		{
			get { return enableCursor; }
			set
			{
				enableCursor = value;
                //关闭使能，则关闭光标显示
				if (!value) DrawRegionCursor.Show = false;
			}
		}

		//Vector2 SelOffset, SelSize;
		readonly float HalfSpacing;
		//float DisplaySize;

        //光标位置、选择位置
		int curPos, selPos;
		public int CursorPosition
		{
			get { return curPos; }
			set
			{
				curPos = value;
				var txtSize = Skin.TextFont.MeasureString(text.Substring(0, curPos));
				if (txtSize.X < Width)
					Offset = 0;
				else
					Offset = ((int)(2f * txtSize.X / Width) - 1) * Width / 2f;
				DrawRegionCursor.DrawPos.X = txtSize.X - DrawRegionText.Origin.X + HalfSpacing;
				RequireRedraw = true;
				SetSelRegion();
			}
		}
		public int SelectPos
		{
			get { return selPos; }
			set
			{
				selPos = value;
				RequireRedraw = true;
				SetSelRegion();
			}
		}



		public GucLabel()
		{
            //创建并添加label的区域列表，由于初始尺寸为1*1，所以Scale就可表示实际尺寸了
			DrawRegionText = new ControlDrawRegionString(Skin.TextFont);
			DrawRegionCursor = new ControlDrawRegionFloat(Skin.Texture, Skin.White1x1, DrawingDepth.TopLayer);
			DrawRegionSelect = new ControlDrawRegionFloat(Skin.Texture, Skin.White1x1, DrawingDepth.BottomLayer);
			CustomDrawRegions.Add(DrawRegionText);
			CustomDrawRegions.Add(DrawRegionCursor);
			CustomDrawRegions.Add(DrawRegionSelect);

            //半个空格的宽度，单个空行的高度
            Text = "";
            autoSize = true;
            Height = Skin.TextFont.LineSpacing;
			HalfSpacing = Skin.TextFont.Spacing / 2;
			FontColor = Color.Black;


			//txtPos = Vector2.Zero;
            //空行高度用来设置比例因子？
			DrawRegionCursor.Scale = new Vector2(1, Height);
			DrawRegionCursor.Show = false;
			DrawRegionCursor.Color = Color.Black;
			DrawRegionSelect.Color = Color.LightSkyBlue;
			DrawRegionSelect.Scale.Y = Height;
			
			cursorTime = 0;
            //整型位置
			selPos = -1;
			curPos = 0;
			drawCursor = false;
			EnalbeCursor = false;
		}

		void SetSelRegion()
		{
			if (selPos > curPos)
			{
				DrawRegionSelect.DrawPos.X = DrawRegionCursor.DrawPos.X + (curPos > 0 ? HalfSpacing : -HalfSpacing);
				DrawRegionSelect.Scale.X = Skin.TextFont.MeasureString(text.Substring(curPos, selPos - curPos)).X + (selPos < text.Length ? HalfSpacing : 0);
				DrawRegionSelect.Show = true;
			}
			else if (selPos >= 0)
			{
				DrawRegionSelect.DrawPos.X = Skin.TextFont.MeasureString(text.Substring(0, selPos)).X + (selPos > 0 ? HalfSpacing : 0);
				DrawRegionSelect.Scale.X = Skin.TextFont.MeasureString(text.Substring(selPos, curPos - selPos)).X + HalfSpacing;
				DrawRegionSelect.Show = true;
			}
			else
				DrawRegionSelect.Show = false;
		}

		protected override void OnParseInput(InputEventArgs input)
		{
			if (enableCursor)
			{
				cursorTime += input.LastUpdateTime;
				if (cursorTime >= CursorFreq)
				{
					cursorTime -= CursorFreq;
					drawCursor = !drawCursor;
				}
				DrawRegionCursor.Show = drawCursor;
			}
		}

		protected override void OnActivated()
		{
			cursorTime = 0;
			drawCursor = false;
			//DrawRegionCursor.Show = false;
		}

		protected override void OnDeActivated()
		{
			SelectPos = -1;
			DrawRegionCursor.Show = false;
		}

		protected override void OnSizeChange()
		{
			CursorPosition = curPos;
		}

		public void FitToSize()
		{
			Size = Skin.TextFont.MeasureString(text);
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.不使用其他控件成员
 * 2.分为三个绘制区域实现：文本区域、光标区域、选择区域
***********************************/