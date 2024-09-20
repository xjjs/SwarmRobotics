using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GucUISystem;

namespace GucUISystem
{
	public sealed class GucScrollBar : GucControl
	{
		public const int minSquare = 5, minLength = minSquare * 2;
		int diffSize;
		int minVal, maxVal, Val, durVal, diffVal, change;
		GucButton upArrow, downArrow, slideBlock;
		ControlDrawRegionInt DrawRegion;

		public event GucEventHandler ValueChanged;

		bool isVert;
		public bool isVertical
		{
			get { return isVert; }
			set
			{
				if (isVert != value)
				{
					isVert = value;
					int w = Width, h = Height;
					if (isVert)
						MinSize = new Point(minSquare, minLength);
					else
						MinSize = new Point(minLength, minSquare);
					Width = h;
					Height = w;
					upArrow.Rotation = downArrow.Rotation = isVert ? 0 : -MathHelper.PiOver2;
				}
			}
		}

		public int ChangeSize
		{
			get { return change; }
			set { if (value > 0) change = Math.Min(value, isVert ? slideBlock.Height : slideBlock.Width); }
		}
		public int Duration
		{
			get { return durVal; }
			set
			{
				durVal = value;
				//if (durVal > diffVal) durVal = diffVal;
				ResizeBlock();
			}
		}

		public int MinValue
		{
			get { return minVal; }
			set
			{
				minVal = value;
				if (minVal > maxVal) minVal = maxVal;
				diffVal = maxVal - minVal;
				if (value < minVal) value = minVal;
				ResizeBlock();
			}
		}
		public int MaxValue
		{
			get { return maxVal; }
			set
			{
				maxVal = value;
				if (minVal > maxVal) maxVal = minVal;
				diffVal = maxVal - minVal;
				if (value > maxVal) value = maxVal;
				ResizeBlock();
			}
		}
		public int Value
		{
			get { return Val; }
			set
			{
				if (Val != value)
				{
					if (value > maxVal)
						value = maxVal;
					else if (value < minVal)
						value = minVal;
					RequireRedraw |= (value != Val) && (diffSize > 0);
					Val = value;
					if (isVert)
						slideBlock.Y = Width + (int)Math.Round((Val - minVal) * (diffSize - slideBlock.Height) / (double)diffVal, MidpointRounding.AwayFromZero);
					else
						slideBlock.X = Height + (int)Math.Round((Val - minVal) * (diffSize - slideBlock.Width) / (double)diffVal, MidpointRounding.AwayFromZero);
					if (ValueChanged != null) ValueChanged(this);
				}
			}
		}

		public GucScrollBar()
		{
			DrawRegion = new ControlDrawRegionInt(Skin.Texture, Skin.BorderBackground, DrawingDepth.Background);
			CustomDrawRegions.Add(DrawRegion);

			isVert = true;
			minVal = 0;
			maxVal = 100;
			durVal = 10;
			Val = 0;
			diffVal = maxVal - minVal;
			change = 1;

			upArrow = new GucButton();
			upArrow.DisplayDepth = DrawingDepth.ChildrenControl1;
			downArrow = new GucButton();
			downArrow.DisplayDepth = DrawingDepth.ChildrenControl1;
			slideBlock = new GucButton();
			this.InnerControls.Add(upArrow);
			this.InnerControls.Add(downArrow);
			this.InnerControls.Add(slideBlock);
			downArrow.MinSize = upArrow.MinSize = new Point(minSquare, minSquare);
			downArrow.X = downArrow.Y = upArrow.X = upArrow.Y = 0;
			upArrow.TextureSource = Skin.UpArrow;
			downArrow.TextureSource = Skin.DownArrow;
			upArrow.MousePress += new GucEventHandler<MouseButtons, Point>(upArrow_MousePress);
			downArrow.MousePress += new GucEventHandler<MouseButtons, Point>(downArrow_MousePress);
			slideBlock.MouseDown += new GucEventHandler<MouseButtons, Point>(slideBlock_MouseDown);
			//slideBlock.MouseUp += new GucEventHandler<MouseButtons>(slideBlock_MouseUp);
			slideBlock.MouseMove += new GucEventHandler<Point>(slideBlock_MouseMove);
			this.MousePress += new GucEventHandler<MouseButtons, Point>(GucScrollBar_MousePress);

			MinSize = new Point(minSquare, minLength);
			//SizeChange();
		}

		void GucScrollBar_MousePress(GucControl sender, MouseButtons arg1, Point arg2)
		{
			if (isVert)
			{
				if (arg2.Y > slideBlock.Bottom)
					Value = Val + change;
				else if (arg2.Y < slideBlock.Y)
					Value = Val - change;
			}
			else
			{
				if (arg2.X > slideBlock.Right)
					Value = Val + change;
				else if (arg2.X < slideBlock.X)
					Value = Val - change;
			}
			//Value = isVert ? (arg2.Y > slideBlock.Y ? Val + change : Val - change) : (arg2.X > slideBlock.X ? Val + change : Val - change);
		}

		protected override void OnParseInput(InputEventArgs input)
		{
			if (input.isKeyDown(Keys.Home))
			{
				Value = minVal;
				return;
			}
			if (input.isKeyDown(Keys.End))
			{
				Value = maxVal;
				return;
			}
			int delta = -input.MouseDelta / 120;
			if (input.isKeyPress(Keys.PageDown) || input.isKeyPress(Keys.Down) || input.isKeyPress(Keys.Right))
				delta++;
			if (input.isKeyPress(Keys.PageUp) || input.isKeyPress(Keys.Up) || input.isKeyPress(Keys.Left))
				delta--;
			Value = Val + delta * change;
		}

		void upArrow_MousePress(GucControl sender, MouseButtons arg1, Point arg2) { Value = Val - change; }
		void downArrow_MousePress(GucControl sender, MouseButtons arg1, Point arg2) { Value = Val + change; }

		Point dragOrigin;
		void slideBlock_MouseDown(GucControl sender, MouseButtons arg1, Point arg2) { dragOrigin = arg2; }
		//void slideBlock_MouseUp(GucControl sender, MouseButtons args) { isDragging = false; }

		void slideBlock_MouseMove(GucControl sender, Point args)
		{
			if (!slideBlock.isNotDragging)
			{
				if (isVert)
					Value = (int)Math.Round((args.Y - Width - dragOrigin.Y) * diffVal / (double)(diffSize - slideBlock.Height), MidpointRounding.AwayFromZero) + minVal;
				else
					Value = (int)Math.Round((args.X - Height - dragOrigin.X) * diffVal / (double)(diffSize - slideBlock.Width), MidpointRounding.AwayFromZero) + minVal;
			}
		}

		protected override void OnSizeChange()
		{
			if (isVert)
			{
				downArrow.Width = downArrow.Height = upArrow.Width = upArrow.Height = Width;
				downArrow.X = 0;
				downArrow.Y = Height - Width;
				diffSize = downArrow.Y - Width;
				if (diffSize > 0)
				{
					slideBlock.Width = Width;
					slideBlock.X = 0;
					ResizeBlock();
					slideBlock.Visible = true;
				}
				else
					slideBlock.Visible = false;
			}
			else
			{
				downArrow.Width = downArrow.Height = upArrow.Width = upArrow.Height = Height;
				downArrow.X = Width - Height;
				downArrow.Y = 0;
				diffSize = downArrow.X - Height;
				if (diffSize > 0)
				{
					slideBlock.Height = Height;
					slideBlock.Y = 0;
					ResizeBlock();
					slideBlock.Visible = true;
				}
				else
					slideBlock.Visible = false;
			}
			DrawRegion.Destination.Width = Width;
			DrawRegion.Destination.Height = Height;
		}

		void ResizeBlock()
		{
			int size = diffSize;
			if (diffVal > 0)
			{
				size = (int)(durVal * diffSize / (diffVal + durVal));
				if (diffSize > 5 && size < 5) size = 5;
			}
			if (isVert)
			{
				slideBlock.Height = size;
				slideBlock.Y = Width + (int)((Val - minVal) * (diffSize - slideBlock.Height) / diffVal);
			}
			else
			{
				slideBlock.Width = size;
				slideBlock.X = Height + (int)((Val - minVal) * (diffSize - slideBlock.Width) / diffVal);
			}
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucButton（端点按钮与中间滑块）
***********************************/