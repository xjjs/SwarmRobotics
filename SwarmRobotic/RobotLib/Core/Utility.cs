using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib
{
    public enum PositionType
    {
        Center, Min, Max
    }

    public static class Utility
    {
        //定义枚举器（个体的位置），返回个体位置
        public static IEnumerable<Vector3> UnionInitialize(int population, Vector3 position, int blank, PositionType xtype, PositionType ytype, PositionType ztype, bool useZ = true)
        {
            //计算群体的分布区域尺寸
            int sizex = (int)Math.Ceiling(Math.Pow(population, 1 / (useZ ? 3.0 : 2.0))), 
                sizey = sizex, 
                sizez = useZ ? sizex : 1, 
                index = 0;
            //设置初始位置，position.Z值为0.5，可将其设为0
            //float delta = blank * (sizex - 1), delta2 = delta / 2;
            position.X = SetPostion(position.X, xtype, blank * (sizex - 1));
            position.Y = SetPostion(position.Y, ytype, blank * (sizey - 1));
            position.Z = SetPostion(position.Z, ztype, blank * (sizez - 1));
 //           position.Z = 0;


            //遍历返回群体中每个个体的位置，注释掉返回Z的代码
            //yield return，接着上次的循环位置执行，一旦执行yield break则退出循环；
            for (int i = 0; i < sizex; i++)
            {
                for (int j = 0; j < sizey; j++)
                {
                    for (int k = 0; k < sizez && index < population; k++, index++)
                    {
                        yield return position;
                        position.Z += blank;
                    }
                    if (index >= population) yield break;
                    position.Z -= sizez * blank;
                    position.Y += blank;
                }
                if (index >= population) yield break;
                position.Y -= sizey * blank;
                position.X += blank;
            }

        }
        //设置起始遍历位置
        private static float SetPostion(float value, PositionType type, float delta)
        {
            if (type == PositionType.Min)
                return value;
            else if (type == PositionType.Max)
                return value - delta;
            else
                return value - delta / 2;
        }
        //用静态类的静态方法定义扩展方法
        //IGroup：表示具有公共键的对象的集合，继承自Enumerable
        /// <summary>
        ///将接口集合转为组内List
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
		public static GroupingList<TKey, TElement> ToGroupingList<TKey, TElement>(this IEnumerable<TElement> items, TKey key) { return new GroupingList<TKey, TElement>(key, items); }

		public static GroupingList<TKey, TElement> ToGroupingList<TKey, TElement>(this IGrouping<TKey, TElement> items) { return new GroupingList<TKey, TElement>(items.Key, items); }

		public static IEnumerable<IGrouping<TKey, TElement>> 
            Where<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> items, Func<TElement, bool> predicate)
		{
			return items.SelectMany(group => group.Where(predicate).Select(e => Tuple.Create(group.Key, e)).GroupBy(t => t.Item1, t => t.Item2));
		}
	}

    /// <summary>
    /// 组列表，本身是个列表（因为继承）但增加了键值字段，泛型类型为“键类型”与“组内元素”类型，形参为“键值”与“元素集合”
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
	public class GroupingList<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
	{
		public GroupingList(TKey key) { Key = key; }

        //入参使用接口，而非List对象；
		public GroupingList(TKey key, IEnumerable<TElement> items) : base(items) { Key = key; }

		public TKey Key { get; set; }
	}
    
    //随机数生成，生成的噪声要乘以sigma（噪声强度）
	public class CustomRandom
	{
		bool iset;
		double gset;
		Random r1, r2, r3;

		public CustomRandom()
		{
            //unchecked 指定不检查是否溢出
			r1 = new Random(unchecked((int)DateTime.Now.Ticks));
			r2 = new Random(~unchecked((int)DateTime.Now.Ticks));
			r3 = new Random();
			iset = true;
		}

		public CustomRandom(int seed)
		{
			r1 = new Random(unchecked(seed));
			r2 = new Random(~unchecked(seed));
			r3 = new Random(seed);
			iset = true;
		}

		// Gaussian Random Number Generator class
		// ref. ``Numerical Recipes in C++ 2/e'', p.293 ~ p.294
		public double NextGaussian()
		{
			if (iset)
			{
				double rsq, v1, v2, fac;
				do
				{
					v1 = 2 * r1.NextDouble() - 1;
					v2 = 2 * r2.NextDouble() - 1;
					rsq = v1 * v1 + v2 * v2;
				} while (rsq >= 1.0 || rsq == 0.0);

				fac = Math.Sqrt(-2.0 * Math.Log(rsq) / rsq);
				gset = v1 * fac;
				iset = false;
				return v2 * fac;
			}
			else
			{
				iset = true;
				return gset;
			}
		}

        // Power-Law Random Number Generator
        public double NextPowerLaw(double r, double u)
        {
            if (r < 0.00001 || u < 1.00001) 
                throw new Exception("Illegal Parameters for Power-Law Distribution");
            return r / Math.Pow(r1.NextDouble(), 1 / (u-1));
        }

        // Exponential Random Number Generator, f(x)=ae^(-ax),mean is 1/a
        public double NextExponential(double a)
        {
            double u = 0.0;
            do
            {
                u = r1.NextDouble();
            } while (u < 0.000000001);
            return -Math.Log(u, Math.E) / a;
        }

		public float NextGaussianFloat() { return (float)NextGaussian(); }

		public float NextGaussianNoise() { return (float)NextGaussian() * NoiseSigma; }

		public Vector3 NextGaussianNoise2D() { return new Vector3((float)NextGaussian(), (float)NextGaussian(), 0) * NoiseSigma; }

		public Vector3 NextGaussianNoise3D() { return new Vector3((float)NextGaussian(), (float)NextGaussian(), (float)NextGaussian()) * NoiseSigma; }

		public int NextInt() { return r3.Next(); }

		public int NextInt(int max) { return r3.Next(max); }

		public int NextInt(int min, int max) { return r3.Next(min, max); }

		/// <summary>
		/// [0,1)
		/// </summary>
		/// <returns></returns>
		public float NextFloat() { return (float)r3.NextDouble(); }

		public double NextDouble() { return r3.NextDouble(); }

		public const float NoiseSigma = 0.4f;
	}

	//public class WarpEnumerable<T>
	//{
	//    IEnumerable<T> _list;
	//    List<T> list;

	//    public WarpEnumerable(IEnumerable<T> expression = null)
	//    {
	//        _list = expression;
	//        list = null;
	//    }

	//    public WarpEnumerable(WarpEnumerable<T> e)
	//    {
	//        _list = e._list;
	//        list = e.list;
	//    }

	//    public bool Reset()
	//    {
	//        if (list == null) return false;
	//        list = null;
	//        return true;
	//    }

	//    public static implicit operator IEnumerable<T>(WarpEnumerable<T> e)
	//    {
	//        if (e.list == null) e.list = e._list.ToList();
	//        return e.list;
	//    }

	//    public static implicit operator WarpEnumerable<T>(IEnumerable<T> e) { return new WarpEnumerable<T>(e); }
	//}

	public class WrapEnumerable<T> : IEnumerable<T>
	{
        //接口对象、列表对象
        //实际的功能相当于一个列表，多数操作也是基于List的方法实现的；
		IEnumerable<T> _list;
		List<T> list;

		public WrapEnumerable(IEnumerable<T> expression = null) { Set(expression); }

		public void Set(IEnumerable<T> expression)
		{
			_list = expression;
			list = null;
		}

		public bool Flush()
		{
			if (list == null) return false;
			list = null;
			return true;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (list == null) list = _list.ToList();
			return list.GetEnumerator();
		}

        //非泛型的显式实现
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public int Count
		{
			get
			{
				if (list == null) list = _list.ToList();
				return list.Count;
			}
		}

        //定义索引运算符
		public T this[int index] { get { return list[index]; } }
	}
}
//实用工具
//位置类型：中心、最小、最大
//Utility：设置群体控件，遍历群体位置
//GroupList：一个带有公共键的List
//CustomRandom：随机数生成算法
//WrapEnumerable：带有Enumerable接口的List；