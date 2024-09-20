using System;
using Microsoft.Xna.Framework;

namespace GucUISystem
{
    /// <summary>
    /// 继承自GucControl：实现为单选框（虽然一般CheckBox为复选框，OptionButton为单选框）；
    /// 字段控件：GucButton、GucLabel
    /// </summary>
	public class GucCheckBox : GucControl
	{
		GucButton checkButton;
		GucLabel label;
		//Vector2 blockSize, blockPos;
		ControlDrawRegionFloat DrawRegionBlock;

		public event GucEventHandler CheckedChanged;

		public GucCheckBox()
		{
			DrawRegionBlock = new ControlDrawRegionFloat(Skin.Texture, Skin.White1x1, DrawingDepth.TopLayer);
			CustomDrawRegions.Add(DrawRegionBlock);
			DrawRegionBlock.Scale.X = 5;
			DrawRegionBlock.Color = Color.Transparent;

            //注册“按钮”的“单击事件处理程序”
			checkButton = new GucButton(0);
			InnerControls.Add(checkButton);
			//checkButton.X = checkButton.Y = 0;
			checkButton.Click += new GucEventHandler(checkBox_Click);

			label = new GucLabel();
			InnerControls.Add(label);
			//label.Y = 0;
			//label.AutoSize = false;
			label.Click += new GucEventHandler(checkBox_Click);
			label.SizeChanged += new GucEventHandler(label_SizeChanged);

			autoSize = false;
			needChange = true;
			AutoCheck = true;
			textureCheck = Skin.CheckBoxChecked;
			textureNormal = Skin.CheckBoxNormal;
			Size = new Vector2(75, 25);
			Checked = false;
		}

		public override Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				base.BackColor = value;
				if (DrawRegionBlock != null) DrawRegionBlock.Color = value;
			}
		}

		protected override void OnClick() { checkBox_Click(this); }
        
        //单击事件处理程序会设置“Check属性”，进而触发“选中状态”改变事件
		void checkBox_Click(GucControl sender)
		{
			if (AutoCheck)
			{
				Checked = !checkValue;
				RequireRedraw = true;
			}
		}

		bool checkValue;
        //只在设置Checked属性时触发“选中状态改变”事件
		public bool Checked
		{
			get { return checkValue; }
			set
			{
				if (checkValue != value)
				{
					checkValue = value;
                    //根据是否选中而选择合适的“纹理”
					checkButton.TextureSource = checkValue ? textureCheck : textureNormal;
					if (CheckedChanged != null) CheckedChanged(this);
				}
			}
		}

		public bool AutoCheck { get; set; }

		DisplayTexture textureCheck, textureNormal;
		public DisplayTexture CheckedTexture
		{
			get { return textureCheck; }
			set
			{
				textureCheck = value;
				if (checkValue) checkButton.TextureSource = textureCheck;
			}
		}
		public DisplayTexture NormalTexture
		{
			get { return textureNormal; }
			set
			{
				textureNormal = value;
				if (!checkValue) checkButton.TextureSource = textureNormal;
			}
		}

		public string Text
		{
			get { return label.Text; }
			set { label.Text = value; }
		}

		bool autoSize;
		public bool AutoSize
		{
			get { return autoSize; }
			set
			{
				if (autoSize != value)
				{
					autoSize = value;
					//label.AutoSize = value;
					label_SizeChanged(label);
				}
			}
		}

		protected override void OnSizeChange()
		{
			DrawRegionBlock.Scale.Y = checkButton.Width = checkButton.Height = Height;
			label.X = checkButton.Right + 5;
			label.Y = (Height - label.Height) / 2;
			DrawRegionBlock.DrawPos.X = Width - 5;
			//label.Width = Width - label.X - 5;
		}

		bool needChange;
		void label_SizeChanged(GucControl sender)
		{
			if (autoSize && needChange)
			{
				needChange = false;
				var h = Math.Max(label.Height, Height);
				Size = new Vector2(label.Width + h + 10, h);
				needChange = true;
			}
		}
	}
}