using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityProject
{
	public sealed class SortedDistanceList<TValue>
	{
		public SortedDistanceList(int capacity)
		{
			disList = new double[capacity];
			indList = new TValue[capacity];

            //最大的坐标
			last = capacity - 1;
			Clear();
		}

		public void Clear() { Size = 0; maxDis = double.PositiveInfinity; }

		public bool Add(double distance, TValue value)
		{
			int pos = Array.BinarySearch(disList, 0, Size, distance);
            //表示未找到，取反可直接得到要插入的位置

			if (pos < 0) pos = ~pos;

            //设置false与true的目的是指明是否有必要更新maxDis
            //若数组已满且新添加的元素无效，则返回false
			if (pos > last)
				return false;

			else if (pos < last)
			{
				Array.Copy(disList, pos, disList, pos + 1, last - pos);
				Array.Copy(indList, pos, indList, pos + 1, last - pos);
			}
			disList[pos] = distance;
			indList[pos] = value;
			if (Size <= last) Size++;

            //若数组已满且新添加的元素有效，则返回true
			if (Size > last)
			{
				maxDis = Math.Sqrt(disList[last]);
				return true;
			}

            //未满则返回false
			return false;
		}

		public IEnumerable<TValue> Values
		{
			get
			{
				//if (size > last)
				return indList.Take(Size);
				//else
				//    return indList.Take(size);
			}
		}

		public double maxDis { get; private set; }

		internal double[] disList;
		internal TValue[] indList;
		int last;

		public int Size { get; private set; }
	}

}
