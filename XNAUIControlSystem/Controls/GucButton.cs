using Microsoft.Xna.Framework;

namespace GucUISystem
{
	public class GucButton : GucBorderBox
	{
		bool isMouseDown;
		GucLabel label;
		ControlDrawRegionFloat DrawRegionTexture;

		public GucButton(int border = 2)
			: base(0, 0, border)
		{
			DrawRegionTexture = new ControlDrawRegionFloat(Skin.Texture, new Rectangle(), DrawingDepth.Background);
			CustomDrawRegions.Add(DrawRegionTexture);

			TextureSource = Skin.Button;
			DisplayType = DisplaySkinType.Normal;
			isMouseDown = false;
			Rotation = 0;
			DrawCenter = false;

			label = new GucLabel();
			InnerControls.Add(label);
			label.X = label.Y = 4;
			label.CanHaveFocus = false;
			Text = "";

			Size = new Vector2(100, 25);
			minCenterSize = new Point(1, 1);
		}

		public void AutoFit()
		{
			if (label.Visible)
			{
				label.MaxSize = new Point(int.MaxValue, int.MaxValue);
				label.FitToSize();
				Size = new Vector2(label.Width + 8, label.Height + 8);
			}
		}

		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			DrawRegionTexture.DrawPos = new Vector2(CenterPosition.X + CenterSize.X / 2, CenterPosition.Y + CenterSize.Y / 2);
			DrawRegionTexture.Scale = CenterSize / (DrawRegionTexture.Origin * 2);
			label.MaxSize = new Point(Width - 8, Height - 8);
			if (label.Visible)
			{
				label.X = (Width - label.Width) / 2;
				label.Y = (Height - label.Height) / 2;
			}
		}

		public override Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				base.BackColor = value;
				if (DrawRegionTexture != null) DrawRegionTexture.Color = value;
			}
		}

		public string Text
		{
			get { return label.Text; }
			set
			{
				label.Text = value;
				label.Visible = (value != "");
				if (label.Visible)
				{
					label.X = (Width - label.Width) / 2;
					label.Y = (Height - label.Height) / 2;
				}
			}
		}

		DisplayTexture texture;
		public DisplayTexture TextureSource
		{
			get { return texture; }
			set
			{
				texture = value;
				//textureSize = new Vector2(texture[displayType].Width, texture[displayType].Height);
				Initialize(texture[displayType], 1);
				DrawRegionTexture.Source = regions[8].Source;
				DrawRegionTexture.Origin = new Vector2(regions[8].Source.Width / 2f, regions[8].Source.Height / 2f);
				DrawRegionTexture.Scale = CenterSize / (DrawRegionTexture.Origin * 2);
				RequireRedraw = true;
			}
		}

		float rot;
		public float Rotation {
			get { return rot; }
			set
			{
				rot = value;
				DrawRegionTexture.Rotation = rot;
			}
		}

		DisplaySkinType displayType;
		public DisplaySkinType DisplayType
		{
			get { return displayType; }
			set
			{
				if (displayType != value)
				{
					for (int i = 0; i < 9; i++)
					{
						regions[i].Source.X += texture[value].X - texture[displayType].X;
						regions[i].Source.Y += texture[value].Y - texture[displayType].Y;
					}
					//DrawRegionTexture.Source = new Rectangle(DrawRegionTexture.Source.X + texture[value].X - texture[displayType].X,
					//    DrawRegionTexture.Source.Y + texture[value].Y - texture[displayType].Y, DrawRegionTexture.Source.Width, DrawRegionTexture.Source.Height);
					DrawRegionTexture.Source = regions[8].Source;
					displayType = value;
					RequireRedraw = true;
				}
			}
		}

		protected override void OnMouseEnter() { if (!isMouseDown) DisplayType = DisplaySkinType.Hover; }

		protected override void OnMouseLeave() { if (!isMouseDown) DisplayType = DisplaySkinType.Normal; }

		protected override void OnParseInput(InputEventArgs input)
		{
			isMouseDown = input[MouseButtons.LeftButton].isPressing;
			DisplayType = isMouseDown ? DisplaySkinType.Pressed : (MouseInside ? DisplaySkinType.Hover : DisplaySkinType.Normal);
		}

		protected override void OnDeActivated()
		{
			isMouseDown = false;
			DisplayType = MouseInside ? DisplaySkinType.Hover : DisplaySkinType.Normal;
		}

		protected override void OnMouseDown(MouseButtons MouseButton, Point MousePosition)
		{
			if (MouseButton == MouseButtons.LeftButton)
			{
				isMouseDown = true;
				DisplayType = DisplaySkinType.Pressed;
			}
		}

		protected override void OnMouseUp(MouseButtons MouseButton, Point MousePosition)
		{
			if (MouseButton == MouseButtons.LeftButton)
			{
				isMouseDown = false;
				DisplayType = MouseInside ? DisplaySkinType.Hover : DisplaySkinType.Normal;
			}
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucBorderBox,GucLabel
 * 2.根据相关事件设置皮肤的显示模式（选择不同的纹理区域）
 * 3.在一个新的矩形绘制区域上绘制GubBorderBox
***********************************/