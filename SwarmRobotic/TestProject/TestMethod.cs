using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using RobotLib;
using RobotLib.FitnessProblem;
using System.Runtime.Serialization.Formatters.Binary;
using ParallelTest;

namespace TestProject
{
    /// <summary>
    /// 通过不同的静态方法成员提供不同的测试手段：
    /// </summary>
	static class TestMethod
	{
        //参数测试：执行测试函数并将结果写入文件
		public static void TestParam<T>
            (this TestBase<T> test, string prefix, TestOptions option, params ExperimentTest[] paras)
			where T : RunState, new()
		{
            //创建“实验测试”列表的名称列表（算法名列表）
			var names = CreateNames(paras);

			for (int i = 0; i < paras.Length; i++)
			{
				var results = option.pso >= 0 ? TestPSO(test, paras[i], prefix + names[i], option) 
                    : TestOnce(test, paras[i], prefix + names[i], option);
				StreamWriter sw = new StreamWriter(string.Format("{4}{0}-{1}-{2}-{3}.csv", names[i], results.Count, option.seeds.Length, test.Repeat, prefix), true);
				sw.Write(paras[i].Title.Replace("RandSeed,", ""));
				sw.Write(test.Title);
				for (int j = 1; j < option.steps; j++) sw.Write(test.Title);
				sw.WriteLine();
				foreach (var item in results)
					sw.WriteLine(item.ToString());
				sw.Close();
			}
		} 

        //对比测试：执行测试函数并将结果写入文件
		public static void TestCompare<T>
            (this TestBase<T> test, string name, TestOptions option, params ExperimentTest[] paras)
			where T : RunState, new()
		{
            //创建“实验测试”列表的名称列表（算法名列表），创建文件名并以流对象打开
			var names = CreateNames(paras);
			string filename = string.Format("{0}.csv", name);
			StreamWriter sw = new StreamWriter(filename, true);

            //写入各列标题：算法、障碍物个数（嵌套对象标题-名称列表）、各步目标收集率的状态信息
            //逗号分隔串在csv中自动为不同单元格
			sw.Write("Algorithm," + paras[0].Title.Replace("RandSeed,", ""));
            //“实验状态”所有字段名的逗号分隔串
			sw.Write(test.Title);

            //重复列写状态的字段：不同目标收集比率下的“实验状态”，换行后关闭文件
			for (int j = 1; j < option.steps; j++) sw.Write(test.Title);
			sw.WriteLine();
			sw.Close();

            
			int parallel = option.parallel;

            //利用“测试主体”在指定“测试选项”下测试各个“测试参数集”对象（paras中的每个对象都是一个测试参数集）
			for (int i = 0; i < paras.Length; i++)
			{
                //生成计算结果（并行计算）：测试主体、“测试参数集”（对应多个实验对象）、实验名-算法名、测试选项
				var results = TestOnce(test, paras[i], name + "-" + names[i], option);

                //每个测试参数集（paras[i]）可生成多个参数组合，对应多个实验对象（实验结果为results中的元素r）
                //每行写一个实验对象的结果：算法名 + 实验结果.ToSting()，结果的前几列是参数设置，后几列为各目标收集率下的RunState
				File.AppendAllLines(filename, results.Select(r => names[i] + "," + r.ToString()));
                foreach (var result in results)
                {
                    string ItersInfo = "";
                    for (int j = 0; j < result.IterPoint; j++) ItersInfo = ItersInfo + "," + result.IterList[j].ToString();
                    ItersInfo += '\n';
                    File.AppendAllText(filename, ItersInfo);
                }

              
                //执行完第一次实验后，将线程数设为-1，即下一次进入循环时option不再考虑并行（并行第一次启动就可以了）
				if (i == 0) option.parallel = -1;
			}
			option.parallel = parallel;
		}

        //数据生成
        public static void TestData1<T>
            (this TestBase<T> test, TestOptions option, params ExperimentTest[] paras)
            where T : RunState, new() {
            //创建“实验测试”列表的名称列表（算法名列表），创建文件名并以流对象打开
            var names = CreateNames(paras);
            string name = "data_generation";

            int parallel = option.parallel;

            //利用“测试主体”在指定“测试选项”下测试各个“测试参数集”对象（paras中的每个对象都是一个测试参数集）
            for (int i = 0; i < paras.Length; i++)
            {
                //生成计算结果（并行计算）：测试主体、“测试参数集”（对应多个实验对象）、实验名-算法名、测试选项
                TestData2(test, paras[i], name + "-" + names[i], option);

                //执行完第一次实验后，将线程数设为-1，即下一次进入循环时option不再考虑并行（并行第一次启动就可以了）
                if (i == 0) option.parallel = -1;
            }
            option.parallel = parallel;
        }

        //对比测试：执行测试函数并将结果写入文件
        public static void TestER<T>
            (this TestBase<T> test, TestOptions option,double[,] fitness, params ExperimentTest[] paras)
            where T : RunState, new() {
            //创建“实验测试”列表的名称列表（算法名列表），创建文件名并以流对象打开
            var names = CreateNames(paras);
            int parallel = option.parallel;

            //利用“测试主体”在指定“测试选项”下测试各个“测试参数集”对象（paras中的每个对象都是一个测试参数集）
            for (int i = 0; i < paras.Length; i++)
            {
                //生成计算结果（并行计算）：测试主体、“测试参数集”（对应多个实验对象）、实验名-算法名、测试选项
                var results = TestOnce(test, paras[i], i.ToString()+"-" + names[i], option);

                fitness[0, i] = results[0].ERValue();

                //执行完第一次实验后，将线程数设为-1，即下一次进入循环时option不再考虑并行（并行第一次启动就可以了）
                if (i == 0) option.parallel = -1;
            }
            option.parallel = parallel;
        }

        //转存“参数集对象”列表为名称列表，并考虑计数信息
		static string[] CreateNames(ExperimentTest[] paras)
		{
			string[] names = new string[paras.Length];
			string tmp;
			int k;
			for (int i = 0; i < names.Length; i++)
			{
				names[i] = paras[i].Name;
				if (i > 0)
				{
					k = 2;
					tmp = names[i];

                    //不仅存储名称，还有同前计数，如第2个abc串存储为abc2，第4个abc串存储为abc234
					while (names.Take(i).Contains(names[i]))
					{
						names[i] = tmp + k.ToString();
						k++;
					}
				}
			}
			return names;
		}

        //测试PSO
		static List<TestResults<T>> TestPSO<T>(this TestBase<T> test, ExperimentTest para, string name, TestOptions option)
			where T : RunState, new()
		{
            //添加属性RandSeed的取值信息
			para.SetPara(true, "RandSeed", new ArrayRange(option.seeds));
            //只初始化最后一个参数，部分网格测试
			var paralist = para.PartGridTest(para.Count - 1);
            //测试窗口与日志文件名
			frmTest form = new frmTest();
			form.CloseAfterFinish = false;
			string logfile = name + ".log", logline;

			var func = new TestEvaluateFunction<T>(para, test, option, form, 1);
			form.Bind(func.parallelTest, name);
			var pso = new UtilityProject.PSO.PSO<TestResults<T>>();
			pso.nTopo = new UtilityProject.PSO.IndexTopo<TestResults<T>>(1);
			pso.Evaluate = func;
			pso.maxIteration = 500;
			pso.MaxStepRate = 1f;
			pso.Maximize = true;
			pso.InitSize(10, para.Count - 1);

			var thread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
			{
				pso.InitParallel();
				logline = new StateInfo(typeof(T)).title;
				File.AppendAllText(logfile, logline + Environment.NewLine);
				form.AppendOutputLine(logline);
				logline = string.Format("0:\t| {0}", pso.nTopo.g_fitness, func.GetParas(pso.nTopo.g_position).GetString());
				File.AppendAllText(logfile, logline + Environment.NewLine);
				form.AppendOutputLine(logline);
				while (!pso.isStop)
				{
					pso.IterateParallel();
					logline = string.Format("{1}:\t| {0}", pso.nTopo.g_fitness, pso.iteration);
					File.AppendAllText(logfile, logline + Environment.NewLine);
					form.AppendOutputLine(logline);
					if (func.Cancelled)
					{
						File.AppendAllText(logfile, "Cancelled");
						break;
					}
				}
				form.Invoke(new Action(() => form.Close()));
			}));
			thread.Start();
			form.ShowDialog();

			form.Close();
			form.UnBind();
			para.RemovePara("RandSeed");
			return func.results;
		}

		static List<TestResults<T>> TestOnce<T>(this TestBase<T> test, ExperimentTest para, 
            string name, TestOptions option, bool watch = false)
			where T : RunState, new()
		{
			//Console.Title = name;
			//Console.WriteLine(name + " start " + DateTime.Now);

            //向“测试参数集”的“范围参数”列表中添加地图种子列表RandSeed
			para.SetPara(true, "RandSeed", new ArrayRange(option.seeds));

            //生成测试Tuple集合（foreach括号内在in后的集合）与测试空间尺寸、创建结果列表——状态列表（不同目标收集率）的列表
            //在Tuple中：item1为各范围参数的Current组成列表的字符串形式，item2为参数集所生成的对象
			var paralist = option.grid ? para.GridTest() : para.HybridTest(para.Count - 1);
			var count = option.grid ? para.GridCount() : para.HybridCount(para.Count - 1); 
			var results = new List<TestResults<T>>();

            //此处仅仅是设置工作，接下来启动窗口frmTest后才开始真正的“实验更新”操作
            //利用已有方法构造匿名方法作为形参用于并行测试（元组集合的扩展方法）
            //形参：创建线程测试对象t（匿名方法）、生成测试结果r（匿名方法）、同名（相同参数设置）累加or异名添加（匿名方法）、文件名、线程数、参数空间尺寸
            //此处前后的Lambda表达式似乎建立的联系，这是由ParallelTest方法定义时确定的，而非后面语句利用前面语句的结果（当然函数体中可能是这样）
            //测试类型为由“测试主体”生成的“线程测试”对象
			paralist.ParallelTest(
                () => new TestThread<T>(test, option.steps), 
                (item, t) => t.TestParam(item), 
                r =>{
				    if (r == null) return;
				    var ritem = results.Find(tr => tr.Name == r.Name);
				    if (ritem == null)
					results.Add(r.Clone());
				    else
					    ritem.Add(r);
                },
                name,
                option.parallel,
                count);

            //根据名称将结果排序
			results.Sort(CompareResults);
			//var finalresult = results.GroupBy(t => t.Name,
			//    (key, group) =>
			//    {
			//        TestResults<T> sum = null;
			//        foreach (var item in group)
			//        {
			//            if (sum == null)
			//                sum = item.Clone();
			//            else
			//                sum.Add(item);
			//        }
			//        return sum;
			//    }).OrderBy(t => t.Name);

            //移除范围参数RandSeed，然后返回结果
			para.RemovePara("RandSeed");
			//Console.WriteLine(name + " end " + DateTime.Now);
			return results;
		}

        static void TestData2<T>(this TestBase<T> test, ExperimentTest para,
            string name, TestOptions option, bool watch = false)
            where T : RunState, new() {

            //向“测试参数集”的“范围参数”列表中添加地图种子列表RandSeed
            para.SetPara(true, "RandSeed", new ArrayRange(option.seeds));

            //生成测试Tuple集合（foreach括号内在in后的集合）与测试空间尺寸、创建结果列表——状态列表（不同目标收集率）的列表
            //在Tuple中：item1为各范围参数的Current组成列表的字符串形式，item2为参数集所生成的对象
            var paralist = option.grid ? para.GridTest() : para.HybridTest(para.Count - 1);
            var count = option.grid ? para.GridCount() : para.HybridCount(para.Count - 1);
            var results = new List<TestResults<T>>();

            //此处仅仅是设置工作，接下来启动窗口frmTest后才开始真正的“实验更新”操作
            //利用已有方法构造匿名方法作为形参用于并行测试（元组集合的扩展方法）
            //形参：创建线程测试对象t（匿名方法）、生成测试结果r（匿名方法）、同名（相同参数设置）累加or异名添加（匿名方法）、文件名、线程数、参数空间尺寸
            //此处前后的Lambda表达式似乎建立的联系，这是由ParallelTest方法定义时确定的，而非后面语句利用前面语句的结果（当然函数体中可能是这样）
            //测试类型为由“测试主体”生成的“线程测试”对象
            paralist.TestData3(
                () => new TestThread<T>(test, option.steps),
                (item, t) => t.TestParam(item),
                r =>
                {
                    if (r == null) return;
                },
                name,
                option.parallel,
                count);

            //根据名称将结果排序
            results.Sort(CompareResults);
            para.RemovePara("RandSeed");
        }

        //用名称来比较两个测试结果（可能用以排序）
		static int CompareResults<T>(TestResults<T> t1, TestResults<T> t2) where T : RunState, new()
        { return t1.Name.CompareTo(t2.Name); }
	}

    /// <summary>
    /// 测试结果（状态列表与计数列表）：可序列化、可比较
    /// 主数据（所有过程状态的累加和与累加数）：结果状态数组、计数数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
	[Serializable]
	sealed class TestResults<T> : IComparable<TestResults<T>>
		where T : RunState, new()
	{
		public string Name;
		public T[] Result;
		public int[] Count;

        //统计检验1，统计所有迭代次数
        public int[] IterList;
        public int IterPoint;

        //通过名称与步数创建测试结果对象
		public TestResults(string name, int steps)
		{
            //Result对象的内存重复分配了，这里应该会自动调用默认构造函数吧？？？
			Result = new T[steps];
			for (int i = 0; i < steps; i++)
				Result[i] = new T();
			Name = name;
			Count = new int[steps];
			Count.Initialize();

            //统计检验2
            IterList = new int[5000];
            IterPoint = 0;
		}

		public TestResults(string name, int count, T result)
		{
			Name = name;
			Result = new T[] { result };
			Count = new int[] { count };
		}

		private TestResults() { Result = null; }

		public override string ToString()
		{
			string result = Name;
            for (int i = 0; i < Result.Length; i++)
            {
                Result[i].SD = (float)(Count[i] * Math.Sqrt((Result[i].Iter2 - (double)(Result[i].Iterations) * Result[i].Iterations/Count[i])/(Count[i]-1)));
                //在泛型约束中指出T为RunState，追加的串为各个值(字段值/计数)连接而成的字符串
                result += Result[i].ToString(Count[i]);
            }
			return string.Format("{0}", result);
		}

        public double ERValue() {
            return Result[0].Iterations / ((Result[0] as SFitness).CollectedTargets+0.00001);
        }

        //累加一次结果（一个状态数组），并递增计数，注意为了ER进化这里屏蔽了Success的要求
		public void Add(T[] results)
		{
			if (Result.Length != results.Length) throw new Exception();
			for (int i = 0; i < Result.Length; i++)
				if (results[i].Success || true)
				{
					Result[i].Add(results[i]);
					Count[i]++;
				}

            //统计检验3，添加迭代次数（存储单一地图不同重复次数的结果）
            IterList[IterPoint] = results[Result.Length - 1].Iterations;
            IterPoint++;
		}

        //累加另一对象，并更新计数
		public void Add(TestResults<T> result)
		{
			if (Result.Length != result.Result.Length) throw new Exception();
			for (int i = 0; i < Result.Length; i++)
				if (result.Count[i] > 0)
				{
					Result[i].Add(result.Result[i]);
					Count[i] += result.Count[i];
				}

            //统计检验5，累加不同地图的结果
            for (int i = 0; i < result.IterPoint; i++, IterPoint++) IterList[IterPoint] = result.IterList[i];
		}

        public TestResults<T> Clone()
		{
			var r = new TestResults<T>();
			r.Name = Name;
            //Clone()虽是浅表副本，但对于值元素而言就是完全副本
			r.Count = (int[])Count.Clone();
			r.Result = new T[Result.Length];
			Array.Copy(Result, r.Result, Result.Length);

            //统计检验4，拷贝迭代次数信息
            r.IterPoint = IterPoint;
            r.IterList = new int[5000];
            Array.Copy(IterList, r.IterList, IterList.Length);

			return r;
		}

        //测试结果比较：优先考虑次数、其次考虑状态信息
		public int CompareTo(TestResults<T> other)
		{
			int result = Count[0].CompareTo(other.Count[0]);
			if (result != 0) return result;
			return Result[0].CompareTo(other.Result[0]);
		}
	}

    /// <summary>
    /// 测试选项，主数据：地图种子列表、地图文件
    /// </summary>
	class TestOptions
	{
        //种子列表
		public object[] seeds;
		public bool grid = true;
		public int parallel = 3, pso = -1;
		public int steps = 0;
		Random rand = new Random();
        //文件名为“地图名.seed”
		string _mapname, _filename;

        
		public TestOptions(int maps, string mapname = "map", int parallel = -1, int steps = 0, bool grid = true, int pso = -1)
		{
			MapName = mapname;

            //载入“相应测试”的地图文件，若没有则重新创建
			if (!LoadMap(mapname) || seeds.Length != maps) 
                SetMapSeeds(maps);
			this.parallel = parallel;
			this.steps = steps;
			this.grid = grid;
			this.pso = pso;
		}

        //创建地图的随机种子并存入文件
		public void SetMapSeeds(int maps)
		{
			seeds = new object[maps];
			for (int i = 0; i < maps; i++)
				seeds[i] = rand.Next();
			SaveMap(MapName);
		}

        /// <summary>
        /// 读取地图文件，将字符串集合转化为整数数组，存入seeds
        /// </summary>
		public bool LoadMap(string mapname)
		{
			//if (!File.Exists(_filename)) return false;
			try
			{
				seeds = File.ReadAllLines(_filename).Select(line => (object)int.Parse(line)).ToArray();
				return true;
			}
			catch
			{
				return false;
			}
		}

        /// <summary>
        /// 将seeds的对象数组转化为字符串集合，写入地图文件
        /// </summary>
		public void SaveMap(string mapname) { File.AppendAllLines(_filename, seeds.Select(s => s.ToString())); }

		public string MapName
		{
			get { return _mapname; }
			set
			{
				_mapname = value;
				_filename = _mapname + ".seed";
			}
		}
	}

    //继承自“固定维度评估函数”
	class TestEvaluateFunction<T> : UtilityProject.Funcs.FixDimensionEvaluateFunction<TestResults<T>>
		where T : RunState, new()
	{
        //定义参数范围的接口列表
		ITestParamRange[] ranges;
        //定于元素范围列表
		UtilityProject.Funcs.ComponentRange[] comRanges;
        //声明对象：测试主体、线程、参数设置、测试结果列表
		TestBase<T> test;
		TestThread<T> thread;
		ExperimentTest para;
		int resultStep, Steps;
		public List<TestResults<T>> results;
        //测试窗口与线程数
		frmTest form;
		int parallel = 3;

        //并行测试属性
		public ParallelTest<Tuple<string, Experiment>, TestResults<T>, TestThread<T>> parallelTest { get; private set; }

		public TestEvaluateFunction(ExperimentTest para, TestBase<T> test, TestOptions option, frmTest form, int skipLast = 0)
			: base(para.Count - skipLast)
		{
			this.ranges = para.GenerateParas();
			comRanges = ranges.Take(Dimension).Select(r => new UtilityProject.Funcs.ComponentRange(r.Min, r.Max)).ToArray();
			this.test = test;
			this.para = para;
			resultStep = option.pso;
			Steps = option.steps;
			if (option.parallel != -1) parallel = option.parallel;
			this.form = form;
			results = new List<TestResults<T>>();
			parallelTest = new ParallelTest<Tuple<string, Experiment>, TestResults<T>, TestThread<T>>((item, t) => t.TestParam(item), CreateTestThread);
			parallelTest.AddResult = AddResult;
			thread = CreateTestThread();
		}

		public override TestResults<T> Evaluate(double[] x)
		{
			for (int i = 0; i < Dimension; i++)
				ranges[i].Set(x[i]);
			TestResults<T> result = results.Find(t => t.Name == ranges.GetIndex().GetString().RemoveLastComponnet());
			if (result == null)
			{
				foreach (var item in para.PartGridTest(Dimension))
				{
					if (result == null)
						result = thread.TestParam(item).Clone();
					else
						result.Add(thread.TestParam(item));
				}
				result = new TestResults<T>(result.Name, result.Count[resultStep], result.Result[resultStep]);
				results.Add(result);
			}
			return result;
		}

		public override void Evaluate(UtilityProject.PSO.Particle<TestResults<T>>[] particles)
		{
			parallelTest.Items = EvaluateAll(particles);
			form.Start(parallel);
			form.FinishEvent.WaitOne();
			parallel = parallelTest.RunningThread;
		}

		IEnumerable<Tuple<string, Experiment>> EvaluateAll(UtilityProject.PSO.Particle<TestResults<T>>[] particles)
		{
			string name;
			TestResults<T> result;
			foreach (var p in particles)
			{
				for (int i = 0; i < Dimension; i++)
					ranges[i].Set(p.position[i]);
				name = ranges.GetIndex().GetString().RemoveLastComponnet();
				result = results.Find(t => t.Name == name);
				if (result == null)
				{
					result = new TestResults<T>(name, thread.size);
					results.Add(result);
					p.fitness = result;
					foreach (var item in para.PartGridTest(Dimension))
						yield return item;
				}
				else
					p.fitness = result;
			}
		}

		public override UtilityProject.Funcs.ComponentRange GetRange(int Index) { return comRanges[Index]; }

		public object[] GetParas(double[] best) { return ranges.Take(Dimension).Select((r, i) => r.Preview(best[i])).ToArray(); }

		public bool Cancelled { get; private set; }

		TestThread<T> CreateTestThread() { return new TestThread<T>(test, steps: Steps); }

		void AddResult(TestResults<T> result) { results.Find(t => t.Name == result.Name).Add(result); }
	}
}
//Result对象的内存重复分配了，这里应该会自动调用默认构造函数吧？？？