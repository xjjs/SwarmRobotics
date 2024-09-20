using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GucUISystem
{
	public class GucTextBox : GucBorderBox
	{
		GucLabel label;
		int charHeight, curPos, selPos;
		bool Selecting;

		string text;
		public string Text
		{
			get { return text; }
			set
			{
				text = value.Replace("\n", "");
				Selecting = false;
				curPos = text.Length;
				label.CursorPosition = 0;
				label.SelectPos = -1;
				label.Text = text;
				label.CursorPosition = curPos;
			}
		}

		public Color FontColor
		{
			get { return label.FontColor; }
			set { label.FontColor = FontColor; }
		}

		public bool EnableCursor
		{
			get { return label.EnalbeCursor; }
			set { label.EnalbeCursor = value; }
		}

		public GucTextBox()
			: base(2, 1, 3)
		{
			text = "";
			charHeight = Skin.TextFont.LineSpacing;
			label = new GucLabel();
			this.InnerControls.Add(label);
			label.AutoSize = false;
			label.Click += new GucEventHandler(label_Click);

			Initialize(Skin.TextBox, Skin.TextBoxMargin);
			minCenterSize = new Point(1, charHeight);
			label.X = CenterPosition.X;
			label.Y = CenterPosition.Y;
			Width = 50;
			label.EnalbeCursor = true;
		}

		void label_Click(GucControl sender) { InvokeClick(); }

		protected override void OnParseInput(InputEventArgs input)
		{
			if (!Enable) return;
			string newtext = "";
			if (input.Shift)
			{
				//selection
				if (input.isKeyPress(Keys.Left) && curPos > 0)
				{
					if (!Selecting)
					{
						Selecting = true;
						selPos = curPos;
					}
					curPos--;
				}
				if (input.isKeyPress(Keys.Right) && curPos < text.Length)
				{
					if (!Selecting)
					{
						Selecting = true;
						selPos = curPos;
					}
					curPos++;
				}
				if (input.isKeyPress(Keys.Home) && curPos > 0)
				{
					if (!Selecting)
					{
						Selecting = true;
						selPos = curPos;
					}
					curPos = 0;
				}
				if (input.isKeyPress(Keys.End) && curPos < text.Length)
				{
					if (!Selecting)
					{
						Selecting = true;
						selPos = curPos;
					}
					curPos = text.Length;
				}
				if (curPos == selPos) Selecting = false;
			}
			else
			{
				//control
				if (input.isKeyPress(Keys.Left))
				{
					if (Selecting)
					{
						if (curPos > selPos)
							curPos = selPos;
						else if (curPos > 0)
							curPos--;
						Selecting = false;
					}
					else if (curPos > 0)
						curPos--;
				}
				if (input.isKeyPress(Keys.Right))
				{
					if (Selecting)
					{
						if (curPos < selPos)
							curPos = selPos;
						else if (curPos < text.Length)
							curPos++;
						Selecting = false;
					}
					else if (curPos < text.Length)
						curPos++;
				}
				if (input.isKeyPress(Keys.Home))
				{
					if (Selecting) Selecting = false;
					curPos = 0;
				}
				if (input.isKeyPress(Keys.End))
				{
					if (Selecting) Selecting = false;
					curPos = text.Length;
				}
				if (input.isKeyPress(Keys.Back) &&  text.Length > 0)
				{
					if (Selecting)
					{
						Selecting = false;
						if (curPos < selPos)
							text = text.Remove(curPos, selPos - curPos);
						else
						{
							text = text.Remove(selPos, curPos - selPos);
							curPos = selPos;
						}
					}
					else if(curPos > 0)
					{
						curPos--;
						text = text.Remove(curPos, 1);
					}
				}
				if (input.isKeyPress(Keys.Delete))
				{
					if (Selecting)
					{
						Selecting = false;
						if (curPos < selPos)
							text = text.Remove(curPos, selPos - curPos);
						else
						{
							text = text.Remove(selPos, curPos - selPos);
							curPos = selPos;
						}
					}
					else if (curPos < text.Length)
						text = text.Remove(curPos, 1);
				}
				if (input.Control && input.isKeyPress(Keys.V))
					newtext = ClipBorad.Text;
				if (input.Control && input.isKeyPress(Keys.C) && Selecting)
				{
					if (curPos < selPos)
						ClipBorad.Text = text.Substring(curPos, selPos - curPos);
					else
						ClipBorad.Text = text.Substring(selPos, curPos - selPos);
				}
			}
			//input chars
			if (!input.Alt && !input.Control)
			{
				if (input.Shift)
					foreach (var key in input.Keys)
					{
						if ((key.PressState & InputEventTypes.Press) == InputEventTypes.Press)
							newtext += InputService.DisplayChar[key.Key].Item2;
					}
				else
					foreach (var key in input.Keys)
					{
						if ((key.PressState & InputEventTypes.Press) == InputEventTypes.Press)
							newtext += InputService.DisplayChar[key.Key].Item1;
					}
			}
			if (newtext != "")
			{
				if (Selecting)
				{
					if (curPos < selPos)
						text = text.Substring(0, curPos) + newtext + text.Substring(selPos);
					else
					{
						text = text.Substring(0, selPos) + newtext + text.Substring(curPos);
						curPos = selPos;
					}
					Selecting = false;
				}
				else
					text = text.Substring(0, curPos) + newtext + text.Substring(curPos);
				curPos += newtext.Length;
			}
			label.Text = text;
			//Cursor Position
			label.SelectPos = Selecting ? selPos : -1;
			label.CursorPosition = curPos;
		}

		protected override void OnSizeChange()
		{
			base.OnSizeChange();
			label.Size = CenterSize;
		}

		protected override void OnActivated()
		{
			if (!Selecting)
			{
				Selecting=true;
				label.CursorPosition = curPos = 0;
				label.SelectPos = selPos = text.Length;
			}
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucBorderBox,GucLabel
 * 2.可进行区域选择
***********************************/