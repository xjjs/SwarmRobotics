using System;
using System.Collections.Generic;

namespace GucUISystem
{
    /// <summary>
    /// 控件集合，实现接口ICollection;
    /// 主要数据：控件列表controls、父控件parent
    /// 主要方法：添加控件、移除控件、清空列表
    /// </summary>
	public class XNAControlCollection : ICollection<GucControl>
	{
        //控件列表
		List<GucControl> controls;   
        //控件列表的父控件
		GucControl parent;           

        /// <summary>
        /// 利用父控件创建List
        /// </summary>
        /// <param name="parent"></param>
		public XNAControlCollection(GucControl parent)
		{
			controls = new List<GucControl>();
			if (parent == null) throw new ArgumentNullException("parent");
			this.parent = parent;
		}

        //用List的方法实现ICollection接口中的方法

        /// <summary>
        /// 添加控件并绑定到父控件的显卡对象，更新控件的父控件
        /// </summary>
        /// <param name="item"></param>
		public void Add(GucControl item)
		{
			if (!controls.Contains(item))
			{
				controls.Add(item);  
				if (parent.graphicsDevice != null) item.BindGraphic(parent.graphicsDevice);
			}
			if (item.Parent != parent) item.Parent = parent;
		}

        /// <summary>
        /// 清空列表：将列表的首元素的父控件置空
        /// </summary>
		public void Clear()
		{
			while (controls.Count > 0)
				controls[0].Parent = null; 
		}

		public bool Contains(GucControl item) { return controls.Contains(item); }

		public void CopyTo(GucControl[] array, int arrayIndex) { controls.CopyTo(array, arrayIndex); }

		public int Count { get { return controls.Count; } }

		public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// 移除控件并清除父子关系
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
		public bool Remove(GucControl item)
		{
			if (controls.Contains(item))
			{
				controls.Remove(item);  
				if (parent != null) item.Parent = null;
				return true;
			}
			return false;
		}

        /// <summary>
        /// 声明索引器，可直接通过索引访问控件列表
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public GucControl this[int index] { get { return controls[index]; } }



        //实现IEnumerator<T>接口和IEnumerable接口，以便使用由迭代器支持的foreach语句

        //隐式实现IEnumerable<T>接口方法，可直接用.引用成员
		public IEnumerator<GucControl> GetEnumerator() { return controls.GetEnumerator(); }
        //显示实现IEnumerable接口方法，调用时先将对象转型为接口，再用.引用成员
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
/***********************************
 * 主要功能与实现
 * 1.定义一个控件集合：一个父控件、一个子控件列表
 * 2.利用List的方法实现集合接口的方法，只是对于Clear函数的实现有疑问？？
 * 3.隐式实现IEnumerable<T>接口方法，可直接用.引用成员
 * 4.显示实现IEnumerable接口方法，调用时先将对象转型为接口，再用.引用成员
************************************/