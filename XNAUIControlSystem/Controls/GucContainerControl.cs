using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
    //容器类控件
	public class GucContainerControl : GucControl
	{
		protected RenderTarget2D innerrenderBuffer { get; private set; }


        GucScrollBar hBar, vBar;
        //上下左右的边缘宽度
        int lMar, rMar, tMar, bMar, hOffset, vOffset;

        bool autoISize;
        public bool AutoInnerSize {
            get {
                return autoISize;
                //下面这句会陷入死循环吧？
                //return AutoInnerSize; 
            }
            set {
                if (autoISize != value)
                {
                    autoISize = value;
                    if (value) OnSizeChange();
                }
            }
        }
        //内部区域
        ControlDrawRegionFloat innerRegion;
        public int iHeight, iWidth;
        public int InnerHeight {
            get { return iHeight; }
            set {
                if (autoISize)
                    Height = value + tMar + bMar;
                else
                    iHeight = value;
                //尺寸更改
                InnerSizeChange();
            }
        }
        public int InnerWidth {
            get { return iWidth; }
            set {
                if (autoISize)
                    Width = value + lMar + rMar;
                else
                    iWidth = value;
                InnerSizeChange();
            }
        }

        //滑动条宽度、使能
        public bool EnableVeticalBar { get; set; }
        public bool EnableHorizontalBar { get; set; }
        int barSize;
        public int BarSize {
            get { return barSize; }
            set {
                hBar.Height = vBar.Width = barSize = value;
                //调整显示
                AdjustBar();
            }
        }

		public GucContainerControl(int lMargin, int rMargin, int tMargin, int bMargin)
		{
			autoISize = false;
			iHeight = Height;
			iWidth = Width;
			lMar = lMargin;
			rMar = rMargin;
			tMar = tMargin;
			bMar = bMargin;
			innerRegion = new ControlDrawRegionFloat(null, new Rectangle(0, 0, iWidth, iHeight), DrawingDepth.BottomLayer);
			CustomDrawRegions.Add(innerRegion);

			innerRegion.DrawPos = new Vector2(lMar, tMar);
			innerRegion.Scale = new Vector2(1);
			hBar = new GucScrollBar();
			vBar = new GucScrollBar();
			hBar.isVertical = false;
			InnerControls.Add(hBar);
			InnerControls.Add(vBar);
			vBar.Y = tMar;
			hBar.X = lMar;
			hBar.ValueChanged += new GucEventHandler(hBar_ValueChanged);
			vBar.ValueChanged += new GucEventHandler(vBar_ValueChanged);
			hBar.ChangeSize = vBar.ChangeSize = 15;
			hBar.Height = vBar.Width = barSize = 25;
			hBar.DisplayDepth = DrawingDepth.ChildrenControl1;
			vBar.DisplayDepth = DrawingDepth.ChildrenControl1;
			EnableVeticalBar = EnableHorizontalBar = true;
			hOffset = vOffset = 0;
			MinSize = new Point(lMar + rMar + 25, tMar + bMar + 25);
		}

		public override void BindGraphic(GraphicsDevice graphicsDevice)
		{
			base.BindGraphic(graphicsDevice);
			innerrenderBuffer = new RenderTarget2D(graphicsDevice, iWidth, iHeight);
			innerRegion.Texture = innerrenderBuffer;
		}

		public override bool Draw()
		{
            foreach (var ctrl in AllControls)
            {
                /*
                if(ctrl.AA == "display3D")
                {
                    RequireRedraw |= ctrl.Draw();
                }
                else{
                    RequireRedraw |= ctrl.Draw();
                }*/
                RequireRedraw |= ctrl.Draw();
            }
			if (RequireRedraw)
			{
				//var render = graphicsDevice.GetRenderTargets();
				graphicsDevice.SetRenderTarget(innerrenderBuffer);
				graphicsDevice.Clear(BackColor);

				this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
				foreach (var ctrl in Controls)
				{
					if (ctrl.Visible)
						spriteBatch.Draw(ctrl.renderBuffer, ctrl.Position, null, ctrl.Enable ? Color.White : Color.LightGray, 0,
							Vector2.Zero, 1, SpriteEffects.None, ctrl.IsActive ? DrawingDepth.ActiveControl : ctrl.DisplayDepth);
				}
				this.spriteBatch.End();

				graphicsDevice.SetRenderTarget(renderBuffer);
				graphicsDevice.Clear(BackColor);

				this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
				foreach (var r in CustomDrawRegions)
				{
					if (r.Show) r.Draw(spriteBatch);
				}
				foreach (var ctrl in InnerControls)
				{
					if (ctrl.Visible)
						spriteBatch.Draw(ctrl.renderBuffer, ctrl.Position, null, ctrl.Enable ? Color.White : Color.Gray, 0,
							Vector2.Zero, 1, SpriteEffects.None, ctrl.IsActive ? DrawingDepth.ActiveControl : ctrl.DisplayDepth);
				}
				this.spriteBatch.End();

				graphicsDevice.SetRenderTargets(null);

				//var stream = System.IO.File.Open(string.Format("{0}-{1}.jpg", innerrenderBuffer.Width, innerrenderBuffer.Height), System.IO.FileMode.Create);
				//innerrenderBuffer.SaveAsJpeg(stream, innerrenderBuffer.Width, innerrenderBuffer.Height);
				//stream.Close();

				RequireRedraw = false;
				return true;
			}
			return false;
		}

		protected override GucControl FindMouseOnControl(Point pos)
		{
			GucControl curMOn = null;
			float h = 1, ch;
			bool isInner = false;
			foreach (var item in InnerControls)
			{
				ch = item.IsActive ? DrawingDepth.ActiveControl : item.DisplayDepth;
				if (item.Visible && ch < h && item.IsInControl(pos))
				{
					curMOn = item;
					h = ch;
				}
			}
			var pos2 = new Point(pos.X + hOffset - lMar, pos.Y + vOffset - tMar);
			foreach (var item in Controls)
			{
				ch = (item.IsActive ? DrawingDepth.ActiveControl : item.DisplayDepth);
				if (item.Visible && ch < h && item.IsInControl(pos2))
				{
					curMOn = item;
					h = ch;
					isInner = true;
				}
			}
			if (curMOn != null)
			{
				foreach (var item in CustomDrawRegions)
				{
					if (item.Show && item.Contains(pos) && item.Depth < h)
					{
						curMOn = null;
						break;
					}
				}
			}
			if (curMOn != null && isInner)
			{
				customXOffset = hOffset - lMar;
				customYOffset = vOffset - tMar;
			}
			else
				customXOffset = customYOffset = 0;
			return curMOn;
		}

		void vBar_ValueChanged(GucControl sender)
		{
			vOffset = vBar.Value;
			innerRegion.Source = new Rectangle(hOffset, vOffset, Math.Min(iWidth, Width - rMar), Math.Min(iHeight, Height - bMar));
		}

		void hBar_ValueChanged(GucControl sender)
		{
			hOffset = hBar.Value;
			innerRegion.Source = new Rectangle(hOffset, vOffset, Math.Min(iWidth, Width - rMar), Math.Min(iHeight, Height - bMar));
		}

		protected override void OnSizeChange()
		{
			if (autoISize)
			{
				bool ci = false;
				if (Height - tMar - bMar != iHeight)
				{
					ci = true;
					iHeight = Height - tMar - bMar;
				}
				if (Width - lMar - rMar != iWidth)
				{
					ci = true;
					iWidth = Width - lMar - rMar;
				}
				if (ci)
					InnerSizeChange();
				else
					AdjustBar();
			}
			else
			{
				bool ci = false;
				if (Height - tMar - bMar > iHeight)
				{
					ci = true;
					iHeight = Height - tMar - bMar;
				}
				if (Width - lMar - rMar > iWidth)
				{
					ci = true;
					iWidth = Width - lMar - rMar;
				}
				if (ci)
					InnerSizeChange();
				else
					AdjustBar();
			}
		}

		void InnerSizeChange()
		{
			if (graphicsDevice != null)
			{
				innerrenderBuffer = new RenderTarget2D(graphicsDevice, iWidth, iHeight);
				innerRegion.Texture = innerrenderBuffer;
			}
			innerRegion.Source = new Rectangle(hOffset, vOffset, Math.Min(iWidth, Width - lMar - rMar), Math.Min(iHeight, Height - tMar - bMar));
			AdjustBar();
		}

		void AdjustBar()
		{
			int deltaH = iWidth - Width + lMar;
			int deltaV = iHeight - Height + tMar + (deltaH > 0 ? hBar.Height : 0);

			if (EnableVeticalBar && deltaV > 0)
			{
				deltaH = iWidth - Width + lMar + vBar.Width;
				vBar.Duration = vBar.Height = Height - tMar - bMar;
				vBar.MaxValue = deltaV;
				vBar.Visible = true;
				vBar.X = Width - vBar.Width - rMar;
			}
			else
				vBar.Visible = false;
			if (EnableHorizontalBar && deltaH > 0)
			{
				hBar.Duration = hBar.Width = Width - lMar - rMar - (deltaV > 0 ? vBar.Width : 0);
				hBar.MaxValue = deltaH;
				hBar.Visible = true;
				hBar.Y = Height - hBar.Height - bMar;
			}
			else
				hBar.Visible = false;
			RequireRedraw = true;
		}

		public void FitInnerSize()
		{
			int w = 0, h = 0;
			foreach (var item in Controls)
			{
				if (item.Right > w) w = item.Right;
				if (item.Bottom > h) h = item.Bottom;
			}
			iHeight = h + 1;
			iWidth = w + 1;
			if (iHeight > Height) iHeight += hBar.Height;
			if (iWidth > Width) iWidth += vBar.Width;
			InnerSizeChange();
		}

		public void FitInnerHeight()
		{
			int h = 0;
			foreach (var item in Controls)
			{
				if (item.Bottom > h) h = item.Bottom;
			}
			InnerHeight = h + 1;
		}

		public void FitInnerWidth()
		{
			int w = 0;
			foreach (var item in Controls)
			{
				if (item.Right > w) w = item.Right;
			}
			InnerWidth = w + 1;
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucScrollBar（水平与垂直）
 * 2.内部绘制区域，内部缓冲区，内部绘制与尺寸调整
***********************************/