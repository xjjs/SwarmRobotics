using System;
using System.Collections.Generic;

namespace GucUISystem
{
    //状态项：标签（项本身）与描述
    public class GucStateCollectionItem {
        public object Tag { get; set; }

        public GucStateCollectionItem(object Tag, string description = null) {
            this.Tag = Tag;
            this.description = description;
        }

        string description;
        public string Description { get { return description == null ? Tag.ToString() : description; } }
    }

    //继承并实现IList接口
	public class GucStateCollection : IList<GucStateCollectionItem>
	{
        //状态列表
		List<GucStateCollectionItem> List;
        //定义事件：增、删、改、清
		public event Action<GucStateCollection, int> ItemAdded, ItemRemoved, ItemChanged;
		public event Action<GucStateCollection> ItemCleared;

		public GucStateCollection()
		{
			List = new List<GucStateCollectionItem>();
		}
        public void Add(object Tag, string description = null) { Add(new GucStateCollectionItem(Tag, description)); }


        //实现ICollection接口
		public void Add(GucStateCollectionItem item)
		{
			List.Add(item);
			if (ItemAdded != null) ItemAdded(this, List.Count - 1);
		}

		public void Clear()
		{
			List.Clear();
            //发布列表清空事件
			if (ItemCleared != null) ItemCleared(this);
		}

		public bool Contains(GucStateCollectionItem item) { return List.Contains(item); }

		public bool Contains(object item) { return List.Exists(t => t.Tag == item); }

		public void CopyTo(GucStateCollectionItem[] array, int arrayIndex) { List.CopyTo(array, arrayIndex); }

		public int Count { get { return List.Count; } }

		public bool IsReadOnly { get { return false; } }

		public bool Remove(GucStateCollectionItem item)
		{
			var index = List.IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

        //实现IEnumerator接口
		public IEnumerator<GucStateCollectionItem> GetEnumerator() { return List.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

        //实现IList接口的方法
		public int IndexOf(GucStateCollectionItem item) { return List.IndexOf(item); }

		public void Insert(int index, GucStateCollectionItem item)
		{
			List.Insert(index, item);
            //若事件列表非空，则触发事件
			if (ItemAdded != null) ItemAdded(this, index);
		}

		public void RemoveAt(int index)
		{
			List.RemoveAt(index);
			if (ItemRemoved != null) ItemRemoved(this, index);
		}

		public GucStateCollectionItem this[int index]
		{
			get { return List[index]; }
			set
			{
				List[index] = value;
				if (ItemChanged != null) ItemChanged(this, index);
			}
		}

        //根据标签返回索引
		public int Find(object Tag) { return List.FindIndex(i => Tag.Equals(i.Tag)); }
        //类型参数T实现了可比较的接口
		public int Find<T>(T Tag) where T : IEquatable<T> { return List.FindIndex(i => Tag.Equals((T)i.Tag)); }
	}

}

/***************************************
 * 主要功能与实现
 * 1.定义状态项：标签与描述；
 * 2.定义状态集合，继承并实现IList接口；
 * 3.分别实现ICollection/IEnumerable/IList接口的方法；
 * 4.事件使用：
 * 定义委托类型（函数指针），名称无所谓、主要是返回值与参数
 * 由委托定义事件，委托表示可以注册该事件的函数
 * 在相应位置将函数注册事件（函数一般与事件同名、只是属于不同类）
 * 调用以触发事件，向事件注册的函数会依次被调用，注册多次的函数相应也执行多次
****************************************/