using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProject
{
    /// <summary>
    /// 测试列表类：主要用以实现“ITestParamList”一些的扩展方法，即与“实验参数集合”的相关操作
    /// </summary>
	static class TestList
	{
        //实现各种迭代器
        //两个集合的笛卡尔乘积的迭代器
		static public IEnumerable<Tuple<T1, T2>> MergeList<T1, T2>(this IEnumerable<T1> e1, IEnumerable<T2> e2)
		{
			foreach (var i1 in e1)
				foreach (var i2 in e2)
					yield return Tuple.Create(i1, i2);
		}
        
        //网格测试GridTest，返回Tuple集合迭代器，item1为各参数的Current组成列表的字符串形式，item2为参数集所生成的对象
		public static IEnumerable<Tuple<string, ParaType>> GridTest<ParaType>(this ITestParamList<ParaType> itp)
		{
            //生成范围参数列表并初始化
			ITestParamRange[] ranges = itp.GenerateParas();
			foreach (var item in ranges)
				item.Init();
			int index;

			while (true)
			{
                //item1为所有参数选中值（测试值）的列表的串形式，item2为由参数创建的实验对象
				yield return new Tuple<string, ParaType>(ranges.GetIndex().GetString(), itp.GetParam(ranges));

                //当参数Step到范围边界时返回true，否则返回false
                //变动方式类似整数的“进位变动”，即低索引的变动（Step）到边界则高索引变动（Step）
				index = 0;
				while (ranges[index].Step())
				{
					ranges[index].Init();
					index++;

                    //若所有参数已经取完则跳出本次的迭代器循环
					if (index == ranges.Length) yield break;
				}
			}
		}
        //计算总空间的大小：各个参数的变动范围的乘积
		public static int GridCount<ParaType>(this ITestParamList<ParaType> itp)
		{
			int Count = 1;
			foreach (var item in itp.GenerateParas())
				Count *= item.Count;
			return Count;
		}


        //部分网格测试，返回Tuple集合迭代器，item1为各参数的Current组成列表的字符串形式，item2为各参数所生成的对象
		public static IEnumerable<Tuple<string, ParaType>> 
            PartGridTest<ParaType>(this ITestParamList<ParaType> itp, params int[] gridIndecies)
		{
			ITestParamRange[] ranges = itp.GenerateParas();
            //根据索引数组gridIndecies，对部分元素进行初始化
			for (int i = 0; i < ranges.Length; i++)
			{
				if (gridIndecies.Contains(i))
					ranges[i].Init();
			}

			int index;
			while (true)
			{
                //item1为各参数的Current组成列表的字符串形式（逗号连接），item2为各参数所生成的对象
				yield return new Tuple<string, ParaType>(ranges.GetIndex().GetString(), itp.GetParam(ranges));

                //和前面一样，是移动部分参数后再初始化？初始化也会将移动后的变化清空吧？
				index = 0;
				while (ranges[gridIndecies[index]].Step())
				{
					ranges[gridIndecies[index]].Init();
					index++;
					if (index == gridIndecies.Length)
						yield break;
				}
			}
		}
        //计算选定的总空间大小：各个参数的变动范围的乘积
		public static int PartGridCount<ParaType>(this ITestParamList<ParaType> itp, params int[] gridIndecies)
		{
			int mul = 1;
			ITestParamRange[] ranges = itp.GenerateParas();
			for (int i = 0; i < ranges.Length; i++)
				if (gridIndecies.Contains(i))
					mul *= ranges[i].Count;
			return mul;
		}


        //单一参数测试，返回Tuple集合迭代器，item1为各参数的Current组成列表的字符串形式，item2为各参数所生成的对象
		public static IEnumerable<Tuple<string, ParaType>> SingleParaTest<ParaType>(this ITestParamList<ParaType> itp)
		{
			ITestParamRange[] ranges = itp.GenerateParas();
			foreach (var item in ranges)
				item.Default();
			var defaults = ranges.Select(r => r.Current).ToArray();

            //返回默认值生成的Tuple元组
			yield return new Tuple<string, ParaType>(ranges.GetIndex().GetString(), itp.GetParam(ranges));

            //依次将各参数由默认值变为初始值，若不同则返回相应的Tuple元组
			for (int i = 0; i < ranges.Length; i++)
			{
				ranges[i].Init();
				do
				{
					if (ranges[i].Current.Equals(defaults[i])) continue;
                    //返回初始值不是默认值的Tuple元组
					yield return new Tuple<string, ParaType>(ranges.GetIndex().GetString(), itp.GetParam(ranges));
                    //此处退出循环的条件是移动有效？？？
				} while (!ranges[i].Step());
                //重新变为默认值
				ranges[i].Default();
			}
		}
        //单一参数测试的空间：各个参数变动范围的求和
		public static int SingleParaCount<ParaType>(this ITestParamList<ParaType> itp)
		{
			int Count = 0;
			foreach (var item in itp.GenerateParas())
				Count += item.Count;
			return Count;
		}



        //混合参数测试，返回Tuple集合迭代器，item1为各参数的Current组成列表的字符串形式，item2为各参数所生成的对象
		public static IEnumerable<Tuple<string, ParaType>> HybridTest<ParaType>(this ITestParamList<ParaType> itp, params int[] gridIndecies)
		{
			ITestParamRange[] ranges = itp.GenerateParas();
			//foreach (var item in ranges)
			//    item.Init();

            //未选中参数的索引数组
			int[] singleIndecies = new int[ranges.Length - gridIndecies.Length];
			int index = 0;

            //选中的参数初始化，未选中的参数默认值，存储未选中参数的值
			for (int i = 0; i < ranges.Length; i++)
			{
				if (gridIndecies.Contains(i))
					ranges[i].Init();
				else
				{
					singleIndecies[index++] = i;
					ranges[i].Default();
				}
			}
			if (index != singleIndecies.Length) throw new Exception("Error in HybridTest");
            //生成未选中参数的列表
			var defaults = singleIndecies.Select(i => ranges[i].Current).ToArray();

			while (true)
			{
                //变动选中的参数，返回生成的元组
				yield return new Tuple<string, ParaType>(ranges.GetIndex().GetString(), itp.GetParam(ranges));

				index = 0;
                //当移动有效则初始化本身（不改变参数设置），然后考虑下一个？
				while (ranges[gridIndecies[index]].Step())
				{
					ranges[gridIndecies[index]].Init();
					index++;
					if (index == gridIndecies.Length)
						goto Breakout0;
				}
			}
                //变动未选中的参数（若初始值与默认值相同则不必变动），返回生成的元组
		Breakout0: ;
			for (int i = 0; i < singleIndecies.Length; i++)
			{
				ranges[singleIndecies[i]].Init();
				do
				{
					if (ranges[singleIndecies[i]].Current.Equals(defaults[i])) continue;
					while (true)
					{
						yield return new Tuple<string, ParaType>(ranges.GetIndex().GetString(), itp.GetParam(ranges));

						index = 0;
						while (ranges[gridIndecies[index]].Step())
						{
							ranges[gridIndecies[index]].Init();
							index++;
							if (index == gridIndecies.Length)
								goto Breakout;
						}
					}
				Breakout: ;
				} while (!ranges[singleIndecies[i]].Step());
				ranges[singleIndecies[i]].Default();
			}

		}
        //混合参数测试的空间：被选中的参数变动范围乘积、未选中的参数范围求和、最后两者乘积
		public static int HybridCount<ParaType>(this ITestParamList<ParaType> itp, params int[] gridIndecies)
		{
			ITestParamRange[] ranges = itp.GenerateParas();
			int mul = 1, add = 0;
			for (int i = 0; i < ranges.Length; i++)
				if (gridIndecies.Contains(i))
					mul *= ranges[i].Count;
				else
					add += ranges[i].Count;
			return add == 0 ? mul : mul * add;
		}


        //将对象列表转成用","分隔的字符串返回，对象本身可以是对象列表
		public static string GetString(this object[] objects)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var item in objects)
			{
				if (item is object[])
					foreach (var item2 in item as object[])
						sb.AppendFormat("{0},", item2);
				else
					sb.AppendFormat("{0},", item);
			}
			return sb.ToString();
		}
        //返回相应的Current列表，索引是object类型？？？
		public static object[] GetIndex(this ITestParamRange[] paras) { return paras.Select(r => r.Current).ToArray(); }
        //将Current转存到result数组中
		public static void GetIndex(this ITestParamRange[] paras, object[] result)
		{
			for (int i = 0; i < paras.Length; i++)
				result[i] = paras[i].Current;
		}
        //返回移除最后元素的字符串（分隔符为逗号，）
		public static string RemoveLastComponnet(this string str) 
        { return str.Substring(0, str.Substring(0, str.Length - 1).LastIndexOf(',') + 1); }
	}
}
