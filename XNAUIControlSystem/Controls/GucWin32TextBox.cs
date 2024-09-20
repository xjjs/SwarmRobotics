using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GucUISystem
{
	public class GucWin32TextBox : GucBorderBox
	{
		const int CursorFreq = 500;
		GucLabel label;
		int charHeight, cursorTime, curPos;
		bool drawCursor;
		Vector2 CursorOffset, CursorSize;
		float DisplaySize;

		string text;
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				label.Text = text;
				curPos = text.Length;
			}
		}

		public Color FontColor
		{
			get { return label.FontColor; }
			set { label.FontColor = FontColor; }
		}

		public GucWin32TextBox()
			: base(2, 1, 3)
		{
			text = "";
			charHeight = Skin.TextFont.LineSpacing;
			label = new GucLabel();
			this.InnerControls.Add(label);
			label.AutoSize = false;

			Initialize(Skin.TextBox, Skin.TextBoxMargin);
			minCenterSize = new Point(1, charHeight);
			label.X = CenterPosition.X;
			label.Y = CenterPosition.Y;

			CursorSize = new Vector2(1, charHeight);
			CursorOffset = new Vector2(0, label.Y);
			Width = 50;
			cursorTime = 0;
			drawCursor = false;
			DisplaySize = 0;

			Win32Input.CharEntered += new GucEventHandler<char>(Win32Input_CharEntered);
			Win32Input.CommandEntered += new GucEventHandler<char>(Win32Input_CommandEntered);
			Win32Input.KeyDown += new GucEventHandler<Keys>(Win32Input_KeyDown);
		}

		void Win32Input_KeyDown(GucControl sender, Keys args)
		{
			Console.WriteLine("KeyDown: {0}", args);
		}

		void Win32Input_CommandEntered(GucControl sender, char args)
		{
			Console.WriteLine("Command: {0}", (int)args);
		}

		void Win32Input_CharEntered(GucControl sender, char args)
		{
			Console.WriteLine("Char: {0}", args);
		}

		protected override void OnParseInput(InputEventArgs input)
		{
			if (cursorTime + CursorFreq < input.LastUpdateTime)
			{
				cursorTime = input.LastUpdateTime;
				drawCursor = !drawCursor;
			}
			////input chars
			//if (!input.Alt && !input.Control)
			//{
			//    foreach (var key in input.Keys)
			//    {
			//        if ((key.PressState & InputEventTypes.Press) == InputEventTypes.Press)
			//        {
			//            if (input.Shift)
			//                text += InputService.DisplayChar[key.Key].Item2;
			//            else
			//                text += InputService.DisplayChar[key.Key].Item1;
			//        }
			//    }
			//}
			////control
			//if (input.isKeyPress(Keys.Back) && text.Length > 0)
			//    text = text.Substring(0, text.Length - 1);
			//Text = text;
		}

		void CalculateCursorPosition()
		{
			var txtSize = Skin.TextFont.MeasureString(text.Substring(0, curPos));
			if (txtSize.X < label.Width)
				label.Offset = 0;
			else
				label.Offset = ((int)(2f * txtSize.X / label.Width) - 1) * label.Width / 2f;
			CursorOffset.X = txtSize.X - label.Offset + label.X + Skin.TextFont.Spacing / 2;
			DisplaySize = Skin.TextFont.MeasureString(text).X - label.Offset;
		}

		protected override void OnMouseDown(MouseButtons MouseButton, Point MousePosition)
		{

		}

		protected override void OnMouseUp(MouseButtons MouseButton, Point MousePosition)
		{

		}

		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			label.Size = CenterSize;
			CalculateCursorPosition();
		}

		protected override void OnActivated()
		{
			cursorTime = 0;
			drawCursor = false;
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucBorderBox,GucLabel
 * 2.可进行区域选择
***********************************/