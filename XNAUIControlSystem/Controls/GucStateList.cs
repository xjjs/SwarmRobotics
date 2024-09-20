using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GucUISystem
{
    /// <summary>
    /// 继承自GucControl，状态列表；
    /// 主要数据：Label、GucCheckBox列表itemControls、状态集合Items（Tag对象、描述串）；
    /// 主要方法：对GucCheckBox的列表元素进行增、删、改、清除；
    /// 对GucCheckBox的按钮控件的单击事件会触发本控件的“选中状态改变”事件；
    /// </summary>
	public class GucStateList : GucControl
	{
		public GucStateCollection Items { get; private set; }
		List<GucCheckBox> itemControls;
		GucLabel label;

		int selection;

        /// <summary>
        /// 设置“选中项索引”时会触发“选项改变”事件
        /// </summary>
		public int SelectedIndex
		{
			get { return selection; }
			set
			{
				if (Items.Count > 0)
				{
					itemControls[value].Checked = true;
					CheckedChanged(itemControls[value]);
				}
			}
		}
        /// <summary>
        /// 设置选中项时会设置“索引”，从而触发“选项改变”事件
        /// </summary>
		public object SelectedItem
		{
			get { return SelectedIndex == -1 ? null : Items[SelectedIndex].Tag; }
			set { SelectedIndex = Items.Find(value); }
		}
		public event GucEventHandler SelectedChanged;

		int itemHeight, itemMargin, itemSpacing;
		public int ItemHeight
		{
			get { return itemHeight; }
			set
			{
				itemHeight = value;
				ArrangeItems();
			}
		}
		public int ItemMargin
		{
			get { return itemMargin; }
			set
			{
				itemMargin = value;
				ArrangeItems();
			}
		}
		
		public string Text
		{
			get { return label.Text; }
			set { label.Text = value; }
		}

		DisplayTexture normal, @checked;
		public DisplayTexture NormalTexutre
		{
			get { return normal; }
			set
			{
				normal = value;
				foreach (var item in itemControls)
					item.NormalTexture = normal;
			}
		}
		public DisplayTexture CheckedTexutre
		{
			get { return @checked; }
			set
			{
				@checked = value;
				foreach (var item in itemControls)
					item.CheckedTexture = @checked;
			}
		}

		public GucStateList()
		{
			label = new GucLabel();
			InnerControls.Add(label);

			Items = new GucStateCollection();
			itemControls = new List<GucCheckBox>();
			selection = 0;
			Items.ItemAdded += new Action<GucStateCollection, int>(Items_ItemAdded);
			Items.ItemCleared += new Action<GucStateCollection>(Items_ItemCleared);
			Items.ItemRemoved += new Action<GucStateCollection, int>(Items_ItemRemoved);
			Items.ItemChanged += new Action<GucStateCollection, int>(Items_ItemChanged);
			itemHeight = 25;
			itemMargin = 5;
			Width = 75;
			normal = Skin.RadioBoxNormal;
			@checked = Skin.RadioBoxChecked;
			ArrangeItems();
		}

        //只是更改“描述文本”
		void Items_ItemChanged(GucStateCollection arg1, int arg2)
		{
			itemControls[arg2].Text = Items[arg2].Description;
		}

		void Items_ItemAdded(GucStateCollection arg1, int arg2)
		{
			GucCheckBox b = new GucCheckBox();
			InnerControls.Add(b);
			itemControls.Insert(arg2, b);
			b.CheckedChanged += new GucEventHandler(CheckedChanged);

			b.Size = new Vector2(Width, itemHeight);
			b.Y = label.Bottom + arg2 * itemSpacing + itemMargin;
			b.CheckedTexture = @checked;
			b.NormalTexture = normal;
			b.Text = Items[arg2].Description;

			if (itemControls.Count == 1)
				b.Checked = true;
			else if (selection >= arg2)
				selection++;
			for (int i = arg2 + 1; i < itemControls.Count; i++)
				itemControls[i].Y += itemSpacing;
			ItemsChanged();
		}

		void Items_ItemRemoved(GucStateCollection arg1, int arg2)
		{
			GucCheckBox b = itemControls[arg2];
			b.Parent = null;
			itemControls.RemoveAt(arg2);
			b.CheckedChanged -= CheckedChanged;

			for (int i = arg2; i < itemControls.Count; i++)
				itemControls[i].Y -= itemSpacing;
			if (selection == arg2)
			{
				selection = 0;
				if (itemControls.Count > 0) itemControls[0].Checked = true;
			}
			else if (selection > arg2)
				selection--;
			ItemsChanged();
		}

		void Items_ItemCleared(GucStateCollection obj)
		{
			selection = 0;
			foreach (var item in itemControls)
			{
				item.Parent = null;
				item.CheckedChanged -= CheckedChanged;
			}
			itemControls.Clear();
			ItemsChanged();
		}

        
        /// <summary>
        /// 更新状态列表的高度
        /// </summary>
		void ItemsChanged()
		{
			Height = label.Bottom + Items.Count * itemSpacing;// -itemMargin;
			RequireRedraw = true;
		}

		void CheckedChanged(GucControl sender)
		{
			int index = itemControls.IndexOf(sender as GucCheckBox), tmp;
			if (selection != index)
			{
				if ((sender as GucCheckBox).Checked)
				{
					tmp = selection;
					selection = index;
					itemControls[tmp].Checked = false;
					if (SelectedChanged != null) SelectedChanged(this);
				}
			}
			else
				itemControls[selection].Checked = true;
		}

		void ArrangeItems()
		{
			itemSpacing = itemHeight + itemMargin;
			for (int i = 0, top = label.Bottom; i < itemControls.Count; i++, top += itemSpacing)
				itemControls[i].Y = top;
			ItemsChanged();
		}

		public int Count { get { return Items.Count; } }

		protected override void OnSizeChange()
		{
			foreach (var item in itemControls)
				item.Width = Width;
		}
	}
}
/**********************************
 * 主要功能与实现
 * 1.使用的控件成员：GucLabel，GucCheckBox列表
 * 2.要处理的事件：对item进行增、删、改、清除
***********************************/
