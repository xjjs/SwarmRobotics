using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib
{
    /// <summary>
    /// 历史条目：位置、适应度、时间，可比较性
    /// </summary>
	public class HistoryItem : IComparable<HistoryItem>
	{
		public Vector3 Position;
		public int Fitness;
		public int time;

		public HistoryItem(Vector3 Position, int Fitness)
		{
			this.Position = Position;
			this.Fitness = Fitness;
			time = 0;
		}

		public int CompareTo(HistoryItem other)
		{
			return Fitness.CompareTo(other.Fitness);
		}
	}

    /// <summary>
    /// 历史条目列表：条目列表字段，末条目索引字段；
    /// 历史条目信息：位置、适应度、时间
    /// </summary>
	public class HistoryList : IList<HistoryItem>
	{
		List<HistoryItem> queue;
		int lastInd;

		public HistoryList(int capacity)
		{
			queue = new List<HistoryItem>(capacity);
			Capacity = capacity;
			lastInd = capacity - 1;
		}

		public void Add(Vector3 Position, int Fitness)
		{
			Add(new HistoryItem(Position, Fitness));
		}

		public bool AddDistinct(Vector3 Position, int Fitness)
		{
			if (queue.All(hi => hi.Position != Position))
			{
				Add(new HistoryItem(Position, Fitness));
				return true;
			}
			return false;
		}

        //总是将新条目插入0索引处，若列表已满，则移除末条目
		public void Add(HistoryItem item)
		{
			if (queue.Count == Capacity)
				queue.RemoveAt(lastInd);
			queue.Insert(0, item);
		}

		public void Clear() { queue.Clear(); }

		public void Clear(int Low) { queue.RemoveAll(i => i.Fitness >= Low); }

		public bool Contains(HistoryItem item) { return queue.Contains(item); }

		public void CopyTo(HistoryItem[] array, int arrayIndex) { queue.CopyTo(array, arrayIndex); }

		public int Count { get { return queue.Count; } }

		public bool IsReadOnly { get { return false; } }

		public bool Remove(HistoryItem item) { return queue.Remove(item); }

		public IEnumerator<HistoryItem> GetEnumerator() { return queue.GetEnumerator(); }

		public HistoryItem Last() { return queue.Count == 0 ? null : queue[0]; }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public int IndexOf(HistoryItem item) { return queue.IndexOf(item); }

		public void Insert(int index, HistoryItem item) { throw new NotImplementedException(); }

		public void RemoveAt(int index) { queue.RemoveAt(index); }

		public int Capacity { get; private set; }
        //定义了索引[]运算符
		public HistoryItem this[int index]
		{
			get { return queue[index]; }
			set { queue[index] = value; }
		}
	}
}
