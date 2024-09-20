using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;

namespace TestProject
{
    /// <summary>
    /// “测试主体”的基类，泛型参数类型必须是RunState的派生类
    /// 调用实验的运行函数Run()，属性：最大迭代次数、最大迭代次数数组、属性Repeat的处理代码已注释掉（TestThread还有应用）
    /// </summary>
    /// <typeparam name="T"></typeparam>
	class TestBase<T> //new()作为构造器约束指定类型参数必须有一个默认构造器
        where T : RunState, new()
	{
		public TestBase(int repeat = 50, int iterations = 0) : this(repeat, 0, iterations) { }

        //最大迭代次数列表默认是只有一个元素的列表，而且该元素（最大迭代次数）默认为0（不过会在派生类中重置）
		public TestBase(int repeat, int Default, params int[] iterations)
		{
			Repeat = repeat;
            //“实验状态”的所有字段名组成的逗号分隔串
			Title = RunState.GetTitle(typeof(T));
			MaxIterations = iterations;
			MaxIteration = iterations[Default];
		}

		private TestBase(TestBase<T> other)
		{
			Repeat = other.Repeat;
			MaxIteration = other.MaxIteration;
			MaxIterations = other.MaxIterations;
			Title = other.Title;
		}

		//public void TestRepeat(Experiment param, ref T[] results)
		//{
		//    for (int i = 0; i < Repeat; i++)
		//    {
		//        results[i] = TestOnce(param);
		//        param.Reset();
		//    }
		//}

		//public T TestRepeat(Experiment param, out int success)
		//{
		//    T sumresult = new T(), tmp;
		//    success = 0;
		//    for (int i = 0; i < Repeat; i++)
		//    {
		//        tmp = TestOnce(param);
		//        if (tmp.Success)
		//        {
		//            success++;
		//            sumresult.Add(tmp);
		//        }
		//        param.Reset();
		//    }
		//    return sumresult;
		//}

		public virtual int Repeat { get; set; }

        //最大迭代次数元素
        public int MaxIteration { get; set; }
        //最大迭代次数集合
		public int[] MaxIterations { get; private set; }

        //运行一次实验，并返回实验状态
		public virtual T TestOnce(Experiment param) { return param.Run(MaxIteration) as T; }

        //定义迭代器，返回各次实验状态（默认只有一个）
		public virtual IEnumerable<T> TestStep(Experiment param) 
        { foreach (var iter in MaxIterations) yield return param.Run(iter) as T; }

        //“实验状态”的所有字段名组成的逗号分隔串
		public string Title { get; protected set; }

        //获取结果的字段值串
		public virtual string ToString(T result) { return result.ToString(); }
        
        public virtual TestBase<T> Clone() { return new TestBase<T>(this); }
	}
}
