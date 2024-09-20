using System;

namespace UtilityProject
{
	public class FixSizeQueue<T>
	{
		internal int start, end;
		internal T[] values;

		public FixSizeQueue(int capacity)
		{
			values = new T[capacity];
			Init();
		}

		public void Init()
		{
			start = 0;
			end = 0;
		}

		public T Dequeue() { return values[start++]; }

		public bool Contains { get { return start < end; } }

		public void Enqueue(T item) { values[end++] = item; }
	}

	public class FixMaxSizeQueue<T>
	{
		internal int start;
		internal T[] values;
		public T Last { get; private set; }
		public int Size { get; private set; }
		public int Capacity { get; private set; }

		public FixMaxSizeQueue(int capacity)
		{
			values = new T[capacity];
			Capacity = capacity;
			Init();
		}

		public void Init()
		{
			start = 0;
			Size = 0;
		}

		public T Dequeue()
		{
			if (Size > 0)
			{
				Last = values[start];
				if (++start >= Capacity) start = 0;
				Size--;
				return Last;
			}
			throw new InvalidOperationException();
		}

		public bool Contains { get { return Size > 0; } }

		public bool Enqueue(T item)
		{
			if (Size == Capacity)
			{
				Last = values[start];
				values[start] = item;
				if (++start >= Capacity) start = 0;
				return true;
			}
			else
			{
				values[(start + ++Size) % Capacity] = item;
				return false;
			}
		}
	}

}
