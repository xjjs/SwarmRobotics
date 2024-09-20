using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using RobotLib;
using System.Runtime.Serialization.Formatters.Binary;
using ParallelTest;

namespace TestProject
{
    /// <summary>
    /// “线程测试主体”由测试主体与元组生成Repeat次的累加结果TestResults（RunState的状态数组）；
    /// 主数据：测试主体、结果数组、步数与尺寸；主方法TestParam：由(参数,实验)元组生成实验结果；
    /// </summary>
    /// <typeparam name="T"></typeparam>
	sealed class TestThread<T>
        where T : RunState, new()
	{
		public TestBase<T> test;
        //T为RunState类型的，故resultArray为状态数组
		public T[] resultArray;
		public int steps { get; private set; }
		public int size { get; private set; }
		int size1;

        //构造函数为主数据赋值
		public TestThread(TestBase<T> test, int steps = 0)
		{
			this.test = test.Clone();
			this.steps = steps;
			size = steps == 0 ? 1 : steps;
			size1 = size - 1;
			resultArray = new T[size];
		}

        //输入为元组（各参数形成的串，各参数生成的实验对象），由测试主体生成Repeat次的累加结果（RunState的状态数组）
		public TestResults<T> TestParam(Tuple<string, Experiment> t)
		{
			if (t.Item2 == null) return null;

            //利用元组创建结果对象：参数1为参数值的逗号分隔串，参数2为目标收集率的步数
            //若不移除最后一项（地图种子），则不能对采用不同地图的相同实验进行累加
			var item = new TestResults<T>(t.Item1.RemoveLastComponnet(), size);

            //重复Repeat次测试
			for (int i = 0; i < test.Repeat; i++)
			{
                //步数为0则运行一次实验
				if (steps == 0)
					resultArray[0] = test.TestOnce(t.Item2);
				else
				{
                    //返回迭代器
					var renum = test.TestStep(t.Item2).GetEnumerator();

                    //分别记录不同目标收集率下的结果（实际可返回MaxIterations次，不过这里主需要返回steps次？？？）
                    //生成状态数组
					for (int j = 0; j < size; j++)
					{
						renum.MoveNext();
						resultArray[j] = renum.Current;
					}
				}

                //累加Repeat次的结果（状态数组，不同目标收集率下的状态列表）
				item.Add(resultArray);
				t.Item2.Reset();
			}
			return item;
		}
	}
}
