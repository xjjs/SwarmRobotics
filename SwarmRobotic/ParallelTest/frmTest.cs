using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ParallelTest
{
    public partial class frmTest : Form
    {
		//static void Main(string[] args)
		//{
		//    //Application.EnableVisualStyles();
		//    //Application.SetCompatibleTextRenderingDefault(false);
		//    //Application.Run(new frmTest());
		//    List<Tuple<int, int>> dic = new List<Tuple<int, int>>();
		//    for (int i = 0; i < 20; i++)
		//        dic.Add(Tuple.Create(i, i % 5 + 3));
		//    List<Tuple<int, int>> results = new List<Tuple<int, int>>();
		//    dic.ParallelTest(t => { System.Threading.Thread.Sleep(t.Item2 * 1000); return t; }, t => { results.Add(t); }, "Test", 1);
		//}

        //已经测试的参数集个数
		public int Count { get; private set; }
        //参数空间尺寸
		public int Total { get; set; }
        //各个线程（任务）的所用的时钟滴答
		List<long> Ticks;
        //int size;
        //所有线程（任务）完成所用的总滴答数
		long totaltime;
        //“并行测试”接口
        IParallelTest test;
		bool finished;

        //以非终止状态创建信号
        public frmTest()
        {
            //控件初始化（通过可视化设置而自动生成的代码）
            InitializeComponent();

            //表示前一个“并行测试”（一个测试对应一个算法）的完成情况
			finished = true;
            //表示测试结束后是否关闭窗口，调试可得Stop()后窗口确实关闭了，只是下一个算法的“并行测试”又立刻打开了该窗口
			CloseAfterFinish = true;

            //主线程的初始状态设为非终止状态（子线程处于阻塞状态）
			FinishEvent = new AutoResetEvent(false);
			Ticks = new List<long>();
        }

        //绑定“并行测试”对象与“文件名称”，向“并行测试”的事件注册事件处理函数
		public void Bind(IParallelTest test, string name)
		{
			if (!finished) throw new InvalidOperationException("Previous parallel test is not finished yet");
			Text = name;
			this.test = test;

			test.ThreadsChanged += test_ThreadsChanged;
			test.TaskCreated += test_TaskCreated;
			test.TaskFinished += test_TaskFinished;
			test.TaskSwaped += test_TaskSwaped;
			test.ErrorThrown += AppendOutputLine;
			test.GoLast += test_GoLast;
			test.Finished += test_Finished;
		}

        //解除注册的事件处理函数，然后清空“并行测试”对象
		public void UnBind()
		{
			if (!finished) throw new InvalidOperationException("Previous parallel test is not finished yet");
			test.ThreadsChanged -= test_ThreadsChanged;
			test.TaskCreated -= test_TaskCreated;
			test.TaskFinished -= test_TaskFinished;
			test.TaskSwaped -= test_TaskSwaped;
			test.ErrorThrown -= AppendOutputLine;
			test.GoLast -= test_GoLast;
			test.Finished -= test_Finished;
			test = null;
		}

        //启动Go任务
		public void Start(int parallel)
		{
            //C#禁止跨线程直接访问控件，为了解决该问题，当InvokeRequired为真时，说明有一个其他线程想访问它（控件不是其创建的）
            //其他线程访问控件必须使用控件的Invoke方法来将调用封送到适当的线程
			if (this.InvokeRequired)
                //封装本方法到控件的Invoke方法，参数1为方法，参数2为方法的参数
				this.Invoke(new Action<int>(Start), parallel);
			else
			{
				if (!finished) throw new InvalidOperationException("Previous parallel test is not finished yet");

                //不发送结束信号，阻塞子线程？？？貌似并未定义线程任务用以等待结束信号？
				FinishEvent.Reset();
				Count = 0;
				Ticks.Clear();
				totaltime = 0;

                //更新标签lblInfo的文本显示
				UpdateText();

                //设置需求线程数parallel（任务数），并启动Go线程（Go任务）
				test.Start(parallel);

                //设置“增加线程”、“减少线程”、“暂停线程”、“停止线程”按钮的使能性
				btnUp.Enabled = true;
				btnDown.Enabled = parallel > 1;
				btnPause.Enabled = btnStop.Enabled = true;

                //文本设置在窗口初始化中已经完成，无需以下语句
				btnPause.Text = "Pause";
				btnStop.Text = "Stop";
			}
		}



        //要向“并行测试”对象注册的事件处理函数

        //线程数改变，则更新listThreads控件显示和Ticks数组，更新Text标签
		void test_ThreadsChanged(int size)
        {
			if (this.InvokeRequired)
				this.Invoke(new Action<int>(test_ThreadsChanged), size);
			else
			{
				while (size > listThreads.Items.Count) listThreads.Items.Add("");
				while (size < listThreads.Items.Count) listThreads.Items.RemoveAt(listThreads.Items.Count - 1);
                //新增线程的时钟滴答初始化为0
				if (Ticks.Count < size) Ticks.AddRange(Enumerable.Repeat(0L, size - Count + 1));
				//this.size = size;
				UpdateText();
			}
        }

        //创建任务，赋值到listThreads控件的Items属性，设置其时钟滴答为当前时间，更新Text标签
        //listThreads控件显示的是各个Tuple的字符串形式（参数集合+Experiments），行号（从0开始）等于线程号
        void test_TaskCreated(int id, string item)
        {
			if (this.InvokeRequired)
				this.Invoke(new Action<int, string>(test_TaskCreated), id, item);
            else
            {
                listThreads.Items[id] = item;
				Ticks[id] = DateTime.Now.Ticks;
                UpdateText();
            }
        }

        //结束任务，累加所用滴答数到总时间，递增Count，更新Text标签
		void test_TaskFinished(int id, bool success)
		{
			if (this.InvokeRequired)
				this.Invoke(new Action<int, bool>(test_TaskFinished), id, success);
			else
			{
				totaltime += DateTime.Now.Ticks - Ticks[id];
				Count++;
				UpdateText();
				//AppendOutputLine(string.Format("Task No.{0} element={1} finished {2} totaltime={3}", id, listThreads.Items[id], success ? "successfully" : "with exception", new DateTime(totaltime)));
			}
		}

        //交换任务，交换listThread控件显示与Ticks数组
        void test_TaskSwaped(int arg1, int arg2)
        {
            if (listThreads.InvokeRequired)
                listThreads.Invoke(new Action<int, int>(test_TaskSwaped), arg1, arg2);
            else
            {
                var swap = listThreads.Items[arg1];
                listThreads.Items[arg1] = listThreads.Items[arg2];
                listThreads.Items[arg2] = swap;
				var swapi = Ticks[arg1];
				Ticks[arg1] = Ticks[arg2];
				Ticks[arg2] = swapi;
            }
        }

        //Tuple访问结束，则关闭线程数调节按钮和暂停按钮（实验结束时关闭操作以保护结果）
        void test_GoLast()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(test_GoLast));
            else
                btnDown.Enabled = btnPause.Enabled = btnUp.Enabled = false;
        }

        //测试结束则发布结束信号，根据选项确定是否关闭窗口
        void test_Finished()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(test_Finished));
            else
            {
                btnStop.Enabled = false;
				finished = true;

				FinishEvent.Set();
                if(CloseAfterFinish) this.Close();
            }
        }

        //错误信息显示（事件触发）or一般信息显示（主动调用），则在txtError控件中添加错误显示，并写入日志文件中
        public void AppendOutputLine(string text) 
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(AppendOutputLine), text);
            else
            {
                if (txtError.Text.Length > 930) txtError.Text = "";
                else txtError.Text += text + Environment.NewLine;
                System.IO.File.AppendAllText("parallel.log", text + Environment.NewLine);
            }
        }



        //其他窗口操作相关的事件处理函数

        //更新标签的文本显示
		void UpdateText()
		{
			if (this.InvokeRequired)
				this.Invoke(new Action(UpdateText));
			else
                //标签信息为：Current Threads：ABC
                //A，若“并行测试”对象在运行状态则为空串，否则输出“运行的线程数-暂停的线程数”
                //B，若“运行线程数”等于“需求线程数”则为运行线程数，否则输出“运行线程数”==>“需求线程数”
                //C，若参数空间尺寸为0，则输出“Progress:Count”，否则输出D
                //D，若总时间>0则输出“Progress:Count/Total Estimated Finish：估计的剩余时间值”，否则输出“Progress：Count/Total”；
                //剩余时间值的估计：计算Count个参数集的时间*[(Total-Count)/(Count*运行的线程数)]
				lblInfo.Text = "Current Threads: " +
					//(test.isRunning ? (test.RunningThread == test.RequireThread ? test.RunningThread.ToString()
					//: string.Format("{0}==>{1}", test.RunningThread, test.RequireThread)) : (test.RunningThread - test.PauseThread).ToString())
					(test.isRunning ? "" : (test.RunningThread - test.PauseThread).ToString() + "/") +
					string.Format(test.RunningThread == test.RequireThread ? "{0}" : "{0}==>{1}", test.RunningThread, test.RequireThread)
					+ (Total == 0 ? string.Format("    Progress: {0}", Count) :
					(totaltime > 0 ? string.Format("    Progress: {0}/{1}    Estimated Finish: {2}", Count, Total, DateTime.Now.AddTicks(totaltime * (Total - Count) / (Count * test.RunningThread))) : string.Format("    Progress: {0}/{1}", Count, Total)));
  
		}

        //“需求线程数”加1，更新Text标签
        private void btnUp_Click(object sender, EventArgs e)
        {
            test.AddThread();
            UpdateText();
            btnDown.Enabled = true;
        }

        //“需求线程数”减1，更新Text标签
        private void btnDown_Click(object sender, EventArgs e) 
        {
            test.RemoveThread();
            UpdateText();
            btnDown.Enabled = test.RequireThread > 1;
        }

        //暂停/恢复按钮，更新Text标签
        //若“并行测试”对象处于运行状态，则关闭运行状态、阻塞子线程——My任务（主线程仍在Go任务），进入挂起状态（按钮文本变为“恢复”）
        //若“并行测试”对象未处于运行状态（挂起状态），则开启运行状态、激活子进程（My任务）；
        private void btnPause_Click(object sender, EventArgs e)
        {
            if (test.isRunning)
            {
                test.Pause();
                btnPause.Text = "Resume";
            }
            else
            {
                test.Resume();
                btnPause.Text = "Pause";
            }
            UpdateText();
        }

        //停止按钮
        //Stop():激活Cancle选项（可使得Go任务跳出循环)，若原本非运行会恢复运行状态
        //若仍是非运行状态，则再次实现单击“恢复”事件（应该没有必要？？？）
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to stop?", Text, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                btnPause.Enabled = btnStop.Enabled = false;
                test.Stop();
                btnStop.Text = "Stopping";

                //无论运行状态还是暂停状态下，单击Stop按钮后都不会执行以下语句（isRunning都是true）
				if (!test.isRunning) btnPause_Click(sender, e);
            }
        }

        //关闭窗口，触发的“停止”事件，依然只是停止一个“并行测试”（一个算法的测试）
        private void frmTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnStop.Enabled)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    //requires confirm
                    btnStop_Click(sender, e);
                    e.Cancel = true;
                }
                else
                {
                    //do not confirm
					btnPause.Enabled = btnStop.Enabled = false;
                    test.Stop();
                    btnStop.Text = "Stopping";
					//if (!test.isRunning) btnPause_Click(sender, e);
				}
            }
        }




		public bool CloseAfterFinish { get; set; }
        //与ManualResetEvent相似，主线程的Set方法将信号设为发送状态，Reset将信号设为不发送状态，子线程的WaitOne为等待信号发送
        //从名字可看出，一个自动，一个手动，自动表示只唤醒一个线程后自动设为不发送状态，手动则不会重置发送状态因此可以唤醒多个线程
		public AutoResetEvent FinishEvent { get; set; }

        //设置txtError文本框（TextBox）的高与宽，高同左侧的listThreads列表框（ListBox），宽为窗口宽度减去列表框
		private void frmTest_Resize(object sender, EventArgs e)
		{
			txtError.Height = listThreads.Height = this.ClientSize.Height - listThreads.Top;
			txtError.Width = this.ClientSize.Width - txtError.Left;
		}

        private void frmTest_Load(object sender, EventArgs e) {

        }

        private void txtError_TextChanged(object sender, EventArgs e) {

        }

        private void listThreads_SelectedIndexChanged(object sender, EventArgs e) {

        }

    }
}
