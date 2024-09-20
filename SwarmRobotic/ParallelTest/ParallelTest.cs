using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTest
{
	public static class ParallelTests
	{
        //静态成员在第一次调用之前进行初始化
		static frmTest form = new frmTest();


        //需要三种匿名方法：创建“线程测试”对象、生成“测试结果”、添加or累加“测试结果”
		static public void ParallelTest<ItemType, TestType, ResultType>
			(this IEnumerable<ItemType> List, 
            Func<TestType> Init, //创建“线程测试”对象的方法
            Func<ItemType, TestType, ResultType> Body, //利用元组与“线程测试”对象生成结果的方法
            Action<ResultType> AddResult, //添加（未有）or累加（已有）结果的方法
            string Name, //文件名称
            int parallel = -1,  //线程个数
            int total = 0)  //参数空间的尺寸
		{
            //设置线程数
			if (parallel != -1) Threads = parallel;
            //“生成结果”与“创建线程测试”的匿名方法分别赋给“并行测试”对象的委托变量TaskBodyDelegate与CreateTestDelegate
			var test = new ParallelTest<ItemType, ResultType, TestType>(Body, Init);
            //设置“添加结果”的委托
			test.AddResult = AddResult;

            //获取元组集合的引用
			test.Items = List;

            //执行并行测试，将程序控制权限交给“并行测试”窗口
			ParallelTest(test, Name, total);
		}

        //需要三种匿名方法：创建“线程测试”对象、生成“测试结果”、添加or累加“测试结果”
        static public void TestData3<ItemType, TestType, ResultType>
            (this IEnumerable<ItemType> List,
            Func<TestType> Init, //创建“线程测试”对象的方法
            Func<ItemType, TestType, ResultType> Body, //利用元组与“线程测试”对象生成结果的方法
            Action<ResultType> AddResult, //添加（未有）or累加（已有）结果的方法
            string Name, //文件名称
            int parallel = -1,  //线程个数
            int total = 0)  //参数空间的尺寸
        {
            //设置线程数
            if (parallel != -1) Threads = parallel;
            //“生成结果”与“创建线程测试”的匿名方法分别赋给“并行测试”对象的委托变量TaskBodyDelegate与CreateTestDelegate
            var test = new ParallelTest<ItemType, ResultType, TestType>(Body, Init);
            //设置“添加结果”的委托
            test.AddResult = AddResult;

            //获取元组集合的引用
            test.Items = List;

            //执行并行测试，将程序控制权限交给“并行测试”窗口
            ParallelTest(test, Name, total);
        }

        //无需创建“线程测试”与添加“结果”的匿名方法
		static public void ParallelTest<ItemType>(this IEnumerable<ItemType> List, Action<ItemType> Body, string Name, int parallel = -1, int total = 0)
		{
			if (parallel != -1) Threads = parallel;
			var test = new ParallelTest<ItemType>(Body);
			test.Items = List;
			ParallelTest(test, Name, total);
		}
        //无需创建“线程测试”的匿名方法
        static public void ParallelTest<ItemType, ResultType>
            (this IEnumerable<ItemType> List, Func<ItemType, ResultType> Body, Action<ResultType> AddResult, string Name, int parallel = -1, int total = 0) {
            if (parallel != -1) Threads = parallel;
            var test = new ParallelTest<ItemType, ResultType>(Body);
            test.AddResult = AddResult;
            test.Items = List;
            ParallelTest(test, Name, total);
        }

        static public List<ResultType> ParallelTest<ItemType, TestType, ResultType>
            (this IEnumerable<ItemType> List, Func<TestType> Init, Func<ItemType, TestType, ResultType> Body, string Name, int parallel = -1, int total = 0) {
            var list = new List<ResultType>();
            ParallelTest(List, Init, Body, r => { list.Add(r); }, Name, parallel, total);
            return list;
        }
		static public List<ResultType> ParallelTest<ItemType, ResultType>
			(this IEnumerable<ItemType> List, Func<ItemType, ResultType> Body, string Name, int parallel = -1, int total = 0)
		{
			var list = new List<ResultType>();
			ParallelTest(List, Body, r => { list.Add(r); }, Name, parallel, total);
			return list;
		}


        //将“并行测试对象”与窗口绑定
		static void ParallelTest(IParallelTest test, string Name, int total)
		{
			form.Total = total;
			form.Bind(test, Name);
			form.AppendOutputLine(Name + " started at " + DateTime.Now);
			form.Start(Threads);


			form.ShowDialog();
			form.AppendOutputLine(Name + " finished at " + DateTime.Now);
			form.UnBind();
			Threads = test.RunningThread;
		}

		public static int Threads = 8;
	}

    /// <summary>
    /// 并行测试的接口：事件、属性与方法
    /// </summary>
    public interface IParallelTest
    {
        event Action<int> ThreadsChanged;
        event Action<int, string> TaskCreated;
		event Action<int, bool> TaskFinished;
		event Action<int, int> TaskSwaped;
        event Action<string> ErrorThrown;
        event Action GoLast;
        event Action Finished;

        int RunningThread { get; }
		int PauseThread { get; }
		int RequireThread { get; }
        bool isRunning { get; }
        int TimeStep { get; set; }

        void Start(int threads);
        void Pause();
        void Resume();
        void Stop();

        void AddThread();
        void RemoveThread();
    }

    /// <summary>
    /// 并行测试基类：泛型参数TItem一般为（参数集串,实验对象）二元组
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
	public abstract class ParallelTestBase<TItem> : IParallelTest
	{
        //构造函数
		public ParallelTestBase()
		{
			tasks = new Task[0];
			GoTask = null;
		}

        //Tuple集合
		public IEnumerable<TItem> Items;
        //任务列表：任务主体为“结果生成”函数，可看为“子线程列表”
        //但这些任务并未调用WaitOne方法、即主线程的ReSet并不能阻塞它们，只是各任务执行完当前的“结果生成”后会停止运行
		Task[] tasks;
        //Go任务，“主线程”
        public Task GoTask { get; private set; }
        //My任务，“子线程”
		MyTask waitTask;

        //Cancel用于说明
        //Last用于说明迭代器是否越界（元素已检索完毕）
		bool Cancel, Last;

        //主线程与子线程通过ManualResetEvent对象联系起来，主线程调用Set与Reset方法，子线程调用WaitOne方法
        //主线程调用Reset()会阻塞子线程，调用Set()则子线程继续执行
		ManualResetEvent pauseevent = new ManualResetEvent(false);

        //属性重写为public类型：运行的线程数、暂停的线程数、线程的需求数
		public int RunningThread { get; private set; }
		public int PauseThread { get; private set; }
		public int RequireThread { get; private set; }
		public bool isRunning { get; private set; }
		public int TimeStep { get; set; }

        //定义事件（委托变量）
		public event Action<int> ThreadsChanged;
		//public event Action<int, TItem> TaskCreatedDetail;
		public event Action<int, string> TaskCreated;
		public event Action<int, bool> TaskFinished;
		public event Action<int, int> TaskSwaped;
		public event Action<string> ErrorThrown;
		public event Action GoLast, Finished;

        //由元组与ID创建任务，任务主体为“结果生成”函数
        protected abstract Task CreateTask(TItem item, int threadID);

        //由元组迭代器与ID创建任务，推进迭代器并判断是否为尾元素
		void CreateTask(IEnumerator<TItem> el, int threadID)
		{
			tasks[threadID] = CreateTask(el.Current, threadID);

            //若成功创建了任务且事件非空，则发布事件（线程ID，Tuple的串形式），Tuple的串形式为调试时显示Current的串
			if (TaskCreated != null) TaskCreated(threadID, el.Current.ToString());

			//if (TaskCreatedDetail != null) TaskCreatedDetail(threadID, el.Current);
          
            //枚举数越过集合结尾则MoveNext()返回false（所有参数集已经遍历完毕），Last则为true
			Last = !el.MoveNext();

            //若元组枚举结束且事件非空，则发布事件关闭按钮以防止误操作？
			if (Last && GoLast != null) GoLast();

            //启动任务
			tasks[threadID].Start();
		}

        //析构任务：调用“结果累加”函数
		protected abstract void FinalizeTask(Task task);

        //调整tasks列表尺寸，并发布“线程数调整”事件
		protected virtual void ChangeThreads(int Size)
		{
			Array.Resize(ref tasks, Size);
			if (ThreadsChanged != null) ThreadsChanged(Size);
		}

        //异常处理函数，发布异常事件
		bool HandleError(Exception exception)
		{
			var str = string.Format("Error at {3}:{0}\r\nSource: {1}\r\nStack:\r\n{2}", exception.Message, exception.Source, exception.StackTrace, DateTime.Now);
			Console.WriteLine(str);
			if (ErrorThrown != null) ErrorThrown(str);
			return true;
		}

        //设置需求线程数，关闭Cancel选项，创建并启动Go任务
		public void Start(int threads)
		{
			RequireThread = threads;
			Cancel = false;
			//if (GoTask != null)
			//{
			//    if (GoTask.Exception != null)
			//    {
			//        ;
			//    }
			//}
			GoTask = Task.Factory.StartNew(Go);
		}

        //主线程Reset()以阻塞子线程，关闭主线程运行状态，创建并启动子线程My任务（实际My任务一直处于阻塞状态）
		public void Pause()
		{
			pauseevent.Reset();
			isRunning = false;

            //此处创建的my任务启动就意味着被阻塞
            //每次Pause都创建一个My任务，该任务只有一条语句——调用WaitOne()方法（执行完后线程就自动关闭并释放资源）
			waitTask = new MyTask(WaitTask);
			waitTask.Start();
		}

        //恢复主线程运行状态，主线程Set()以继续运行子线程，主线程一直是Go任务
		public void Resume()
		{
			isRunning = true;
			pauseevent.Set();
		}

        //开启Cancel选项
        //可停止主线程的“暂停状态”
        //也可停止主线程的“运行状态”
		public void Stop()
		{
			Cancel = true;
			if (!isRunning) Resume();
		}

        //依次取元组，创建任务
		void Go()
		{
            //获取当前元组对应迭代器
			var el = Items.GetEnumerator();
			int fin;
            //标记结束的任务不是My任务
			bool foo = false;
            
            //若迭代器指示的不是“尾元组”，则考虑利用“新元组”创建任务
			if (el.MoveNext())
			{
                //开启运行状态，重置尾元素指示、需求线程数、暂停线程数、调整tasks与tests尺寸为需求线程数
				isRunning = true;
				Last = false;

                //运行的线程数设为“需求的线程数”，虽然实际的任务尚未创建
				RunningThread = RequireThread;
				PauseThread = 0;
				ChangeThreads(RequireThread);


                //依次创建并启动子线程，直到线程需求数or取到尾元组
				for (int i = 0; i < RequireThread; i++)
				{
					CreateTask(el, i);  
                
                    //若刚创建的任务使用的是最后一个元组，则重置线程需求数（减去多余的），并调整tasks与tests尺寸
					if (Last)
					{
						RunningThread = RequireThread = tasks.Length;
						ChangeThreads(i + 1);
						break;
					}
				}

                //若上述子线程未能取完元组数，则“主线程”进入循环，调用Stop()会终止循环
				while (!Last)
				{
					//fin = Task.WaitAny(tasks, TimeStep);
                    
                    //主线程在死循环内（Last为false时）的主要任务：等待任务列表中的某一任务完成，返回已完成的任务索引
                    //WaitAny方法只会阻塞本线程，其他的Task仍会继续执行（实验测试而得）；
					fin = Task.WaitAny(tasks);
					if (Cancel) break;
					//if (fin >= 0)
					//{

                    //若结束的是My任务，则暂停线程数递减，否则析构结束的任务（累加结果），并用foo标记
					if (tasks[fin] is MyTask)
						PauseThread--;
					else
					{
						if (tasks[fin].Exception == null)
							FinalizeTask(tasks[fin]);
						else
							tasks[fin].Exception.Handle(HandleError);
						//if (TaskFinished != null) TaskFinished(fin, tasks[fin].Exception == null);
						foo = true;
					}

                    //若线程需求数小于正在运行的线程数（由于用户窗口操作会改变“线程需求数”）
                    //则发布任务结束事件，调整任务列表
					if (RequireThread < RunningThread)
					{
                        //发布任务结束事件
						if (foo && TaskFinished != null) TaskFinished(fin, tasks[fin].Exception == null);

                        //若结束的任务不是尾任务，则进行交换处理（因为“线程需求数”递减移除的是尾任务）
						int l1 = tasks.Length - 1;
						if (fin != l1)
						{
							Swap(tasks, fin, l1);

                            //发布交换事件，进行显示控件的交换处理
							if (TaskSwaped != null) TaskSwaped(fin, l1);
						}

                        //每个任务的完成时间差异较大，时间差内可完成“任务完成后的处理工作”，故此处仅需递减一次即可（表示挂起的任务）
						RunningThread--;
						ChangeThreads(l1);
					}
                    //否则若处在运行状态，则发布结束事件后重新创建任务，若已取到尾元素，则跳出循环
					else if (isRunning)
					{
						if (foo && TaskFinished != null) TaskFinished(fin, tasks[fin].Exception == null);

                        //利用结束的任务编号fin创建新的任务
						CreateTask(el, fin);
						if (Last) break;
                        //若线程需求书仍大于运行的线程数，则继续创建线程直至等于线程需求数or取到元组尾元素
						if (RequireThread > RunningThread)
						{
							int i = RunningThread;
							RunningThread = RequireThread;
							ChangeThreads(RequireThread);
							for (; i < RequireThread; i++)
							{
								CreateTask(el, i);
								if (Last)
								{
									RequireThread = RunningThread = tasks.Length;
									ChangeThreads(i + 1);
									break;
								}
							}
						}
					}
                    //若处于Pause状态，则执行完的任务ID不再用来创建新任务，而是替换为My任务
                    //若任务结束，发布结束事件，并将结束的任务设为等待任务
					else
					{
						PauseThread++;
						if (foo && TaskFinished != null) TaskFinished(fin, tasks[fin].Exception == null);
						tasks[fin] = waitTask;
					}
                    //置foo为false，表示结束的任务已得到处理
					foo = false;
					//}
				}

				//while (!Task.WaitAll(tasks, TimeStep)) ;
                //最后一个Tuple进入测试后（or进入Cancel状态后），等待任务列表中的所有任务完成
				Task.WaitAll(tasks);
				fin = 0;
				foreach (var item in tasks)
				{
                    //My类型的任务则暂停线程数递减，否则析构该任务（累加结果）
					if (item is MyTask)
						PauseThread--;
					else if (item.Exception == null)
						FinalizeTask(item);
					else
						item.Exception.Handle(HandleError);
					if (TaskFinished != null) TaskFinished(fin, item.Exception == null);
					fin++;
				}
			}
            //Go任务结束，关闭运行标记
			isRunning = false;
			//tasks = null;
			if (Finished != null) Finished();
		}

        //请求线程数递增
		public void AddThread() { RequireThread++; }
        //请求线程数递减
		public void RemoveThread() { if (RequireThread > 1) RequireThread--; }

        //交换数组元素，泛型参数T
		static void Swap<T>(T[] array, int i1, int i2)
		{
			var swarp = array[i1];
			array[i1] = array[i2];
			array[i2] = array[i1];
		}

        //My任务，直到System.Threading.WaitHandle收到信号
		void WaitTask() { pauseevent.WaitOne(); }

        //析构函数（波浪线前缀），释放由当前System.Threading.WaitHandle占用的资源
		~ParallelTestBase() { pauseevent.Close(); }
	}

    /// <summary>
    /// 添加了委托AddResult，析构时调用
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public abstract class ParallelTestBase<TItem, TResult> : ParallelTestBase<TItem> {
        public ParallelTestBase() { }

        //析构时累加任务结果
        protected override void FinalizeTask(Task task) { AddResult((task as Task<TResult>).Result); }

        public Action<TResult> AddResult;
    }

    /// <summary>
    /// 添加了委托TaskBodyDelegate与CreateTestDelegate，创建“线程测试”的列表对象tests
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TTest"></typeparam>
    public class ParallelTest<TItem, TResult, TTest> : ParallelTestBase<TItem, TResult>
    {
        public ParallelTest(Func<TItem, TTest, TResult> TaskBody, Func<TTest> CreateTest)
        {
            TaskBodyDelegate = TaskBody;
            CreateTestDelegate = CreateTest;
            tests = new List<TTest>();
        }
        //“结果生成”与“创建线程测试”的委托变量
        public Func<TItem, TTest, TResult> TaskBodyDelegate;
        public Func<TTest> CreateTestDelegate;

        //“线程测试”对象列表
        List<TTest> tests;

        //调整“任务列表”尺寸，保证tasks与tests长度相同
        protected override void ChangeThreads(int Size)
        {
            base.ChangeThreads(Size);
            if (tests.Count < Size)
            {
                for (int i = tests.Count; i < Size; i++)
                    tests.Add(CreateTestDelegate());
            }
            //else if (tests.Count > Size)
                //tests.RemoveRange(Size, tests.Count - Size);
        }

        //利用委托TaskBodyDelegat构造匿名方法以创建任务
        //任务对象可看作委托变量（函数指针），这里的委托变量为利用元组与“线程测试”对象生成“结果”

        //关于new Task<TResult>(function)使用指定的函数初始化新的 System.Threading.Tasks.Task<TResult>。
        //function表示要在任务中执行的代码的委托。在完成此函数后，该任务的 System.Threading.Tasks.Task<TResult>.Result
        //属性将设置为返回此函数的结果值。
        protected override Task CreateTask(TItem item, int threadID) 
        { return new Task<TResult>(() => TaskBodyDelegate(item, tests[threadID])); }
    }
    //My类型的任务
	class MyTask : Task { public MyTask(Action action) : base(action) { } }





    public class ParallelTest<TItem> : ParallelTestBase<TItem> {
        public ParallelTest(Action<TItem> TaskBody) { TaskBodyDelegate = TaskBody; }

        public Action<TItem> TaskBodyDelegate;

        protected override Task CreateTask(TItem item, int threadID) { return new Task(() => { TaskBodyDelegate(item); }); }

        protected override void FinalizeTask(Task task) { }
    }



    public class ParallelTest<TItem, TResult> : ParallelTestBase<TItem, TResult> {
        public ParallelTest(Func<TItem, TResult> TaskBody) { TaskBodyDelegate = TaskBody; }

        public Func<TItem, TResult> TaskBodyDelegate;

        protected override Task CreateTask(TItem item, int threadID) { return new Task<TResult>(() => TaskBodyDelegate(item)); }
    }
}
