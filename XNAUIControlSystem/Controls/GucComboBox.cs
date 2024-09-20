using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
    /// <summary>
    /// 继承自GucControl：组合框；
    /// 可视化显示：一个“文本框”加一个“带箭头的按钮”，单击俺就可显示包含“Label列表”的“FrameBox”框；
    /// 私有字段控件：GucTextBox与GucButton、GucFrameBox与Label列表；
    /// 事件处理程序：Item的增、删、改、清除；
    /// </summary>
	public class GucComboBox : GucControl
	{
		GucTextBox text;
		GucButton button;
		GucFrameBox list;
		List<GucLabel> itemControls;
		public GucStateCollection Items { get; private set; }

		int select;
		public int SelectedIndex
		{
			get { return select; }
			set
			{
				if (select != value)
				{
					if (select >= 0 && select < itemControls.Count) itemControls[select].BackColor = Color.Transparent;
					select = value;
					if (select == -1 && Items.Count > 0) select = 0;
					if (select >= 0)
					{
						itemControls[select].BackColor = Color.LightSkyBlue;
						text.Text = itemControls[select].Text;
					}
					else
						text.Text = "";
					if (SelectedChanged != null) SelectedChanged(this);
				}
				showList = false;
				ToggleList();
			}
		}
        //读取的是Tag对象，设置的是索引，组合框由索引获取对象
		public object SelectedItem
		{
			get { return SelectedIndex == -1 ? null : Items[SelectedIndex].Tag; }
			set { SelectedIndex = Items.Find(value); }
		}
		public event GucEventHandler SelectedChanged;

		int margin, spacing;
		public int ItemMargin
		{
			get { return margin; }
			set
			{
				margin = value;
				spacing = Skin.TextFont.LineSpacing + margin;
			}
		}

		bool showList;
		Rectangle disRegion;

		public GucComboBox()
		{
            //创建控件并加入InnerControls列表
			text = new GucTextBox();
			button = new GucButton();
			list = new GucFrameBox(2);
			list.Y = button.Height = button.Width = text.Height;
			InnerControls.Add(text);
			InnerControls.Add(button);
			InnerControls.Add(list);

			list.InnerHeight = 1;
			list.EnableHorizontalBar = false;
			list.BarSize = 15;
			list.BackColor = Color.White;
			text.EnableCursor = false;
			text.Click += new GucEventHandler(Top_Click);
			button.TextureSource = Skin.DownArrow;
			button.Click += new GucEventHandler(Top_Click);

            
			itemControls = new List<GucLabel>();
			Items = new GucStateCollection();
            //选中项的初始索引为-1
			select = -1;
			Items.ItemAdded += new Action<GucStateCollection, int>(Items_ItemAdded);
			Items.ItemCleared += new Action<GucStateCollection>(Items_ItemCleared);
			Items.ItemRemoved += new Action<GucStateCollection, int>(Items_ItemRemoved);
			Items.ItemChanged += new Action<GucStateCollection, int>(Items_ItemChanged);

			ItemMargin = 2;
			showList = false;

			Size = new Vector2(100, text.Height);
			MinSize = new Point(text.Width + text.Height, text.Height);
		}

		void Top_Click(GucControl sender)
		{
			showList = !showList;
			ToggleList();
		}

		void Items_ItemChanged(GucStateCollection arg1, int arg2)
		{
			itemControls[arg2].Text = Items[arg2].Description;
			list.RequireRedraw = true;
		}

		void Items_ItemAdded(GucStateCollection arg1, int arg2)
		{
			GucLabel l = new GucLabel();
			list.Controls.Add(l);
			itemControls.Insert(arg2, l);
			l.Click += new GucEventHandler(label_Click);

			l.AutoSize = false;
			l.Width = list.InnerWidth;
			l.Y = arg2 * spacing;
			l.Text = Items[arg2].Description;
			l.Offset = -2;
			l.Height = spacing;

			if (itemControls.Count == 1)
				SelectedIndex = 0;
			else if (select >= arg2)
				select++;
			for (int i = arg2 + 1; i < itemControls.Count; i++)
				itemControls[i].Y += spacing;
			list.InnerHeight += spacing;
		}

		void Items_ItemRemoved(GucStateCollection arg1, int arg2)
		{
			GucLabel l = itemControls[arg2];
			l.Parent = null;
			itemControls.RemoveAt(arg2);
			l.Click -= label_Click;

			for (int i = arg2; i < itemControls.Count; i++)
				itemControls[i].Y -= spacing;

			if (select == arg2)
			{
				SelectedIndex = -1;
			}
			else if (select > arg2)
				select--;
			list.InnerHeight -= spacing;
		}

		void Items_ItemCleared(GucStateCollection obj)
		{
			select = -1;
			foreach (var item in itemControls)
			{
				item.Parent = null;
				item.Click -= label_Click;
			}
			itemControls.Clear();
			list.InnerHeight = 1;
		}

		void label_Click(GucControl sender)
		{
			SelectedIndex = itemControls.IndexOf(sender as GucLabel);
		}

		protected override void OnSizeChange()
		{
			text.Width = Width - button.Width;
			button.X = text.Right;
			list.Size = new Vector2(Width, 3 * text.Height);
			list.InnerWidth = Width - 4;
			showList = false;
			ToggleList();
		}

		public int Count { get { return Items.Count; } }

		void ToggleList()
		{
			if (showList)
			{
				if (graphicsDevice != null) renderBuffer = new RenderTarget2D(graphicsDevice, Width, Height + list.Height);
				disRegion = new Rectangle(X, Y, Width, renderBuffer.Height);
			}
			else
			{
				if (graphicsDevice != null) renderBuffer = new RenderTarget2D(graphicsDevice, Width, Height);
				disRegion = Region;
			}
			RequireRedraw = true;
		}

		public override bool IsInControl(Point pos)
		{
			if (showList)
				return Visible && disRegion.Contains(pos);
			else
				return base.IsInControl(pos);
		}

		protected override void OnDeActivated()
		{
			showList = false;
			ToggleList();
		}
	}
}
