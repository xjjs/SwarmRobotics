using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Clipboard = System.Windows.Forms.Clipboard;

namespace GucUISystem
{
	//http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game
	public static class Win32Input
	{


        //定义单类型参数的窗口事件：输入字符、输入命令、键按下、键弹起
		/// <summary>
		/// Event raised when a character has been entered.
		/// </summary>
		public static event GucEventHandler<char> CharEntered;
		public static event GucEventHandler<char> CommandEntered;

		/// <summary>
		/// Event raised when a key has been pressed down. May fire multiple times due to keyboard repeat.
		/// </summary>
		public static event GucEventHandler<Keys> KeyDown;

		/// <summary>
		/// Event raised when a key has been released.
		/// </summary>
		public static event GucEventHandler<Keys> KeyUp;


        //定义委托类型WndProc（函数指针）
        //窗口过程WinProc，对应于某窗口的回调函数，四个参数分别是：窗口句柄、消息ID、两个消息参数（可附加数据）
		delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        //IntPtr为平台特定整数类型，用于本机资源，如窗口句柄，其大小足以包含系统指针
		static bool initialized;            //默认值为false
		static IntPtr prevWndProc;         //
		static WndProc hookProcDelegate;   //窗口过程委托变量（窗口函数）
		static IntPtr hIMC;                //输入上下文句柄

		//various Win32 constants that we need
        //不同的消息ID
		const int GWL_WNDPROC = -4;       //窗口函数地址，小于0表示访问窗口的数据结构
		const int WM_KEYDOWN = 0x100;
		const int WM_KEYUP = 0x101;
		const int WM_CHAR = 0x102;
		const int WM_IME_SETCONTEXT = 0x0281;
		const int WM_INPUTLANGCHANGE = 0x51;
		const int WM_GETDLGCODE = 0x87;
		const int WM_IME_COMPOSITION = 0x10f;
		const int DLGC_WANTALLKEYS = 4;   //控件处理所有的键盘输入




		//Win32 functions that we're using
        //调用非托管代码的API函数，需要导入函数所在的动态链接库
        //Windows的输入法管理器Input Method Manager，

        //获取正在输入窗口的输入法句柄（上下文）
		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr ImmGetContext(IntPtr hWnd);  
        //将上下文句柄与窗口关联
		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        //将消息传给指定窗口，并返回指定消息的处理结果
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        //在窗口额外存储空间的nIndex偏移处设置新的窗口属性dwNewLong，成功则返回前一个值，否则返回0
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);



		/// <summary>
		/// Initialize the TextInput with the given GameWindow.
        /// 设置游戏的“系统窗口”的回调函数，获取输入法句柄
		/// </summary>
		/// <param name="window">The XNA window to which text input should be linked.</param>
		public static void Initialize(GameWindow window)
		{
			if (initialized) 
				throw new InvalidOperationException("TextInput.Initialize can only be called once!");
            
            //将自定义的窗口过程赋给委托变量，并设置为“系统窗口”的窗口过程、返回原来的窗口过程，然后获取其输入法举句柄
			hookProcDelegate = new WndProc(HookProc);
			prevWndProc = (IntPtr)SetWindowLong(window.Handle, GWL_WNDPROC,(int)Marshal.GetFunctionPointerForDelegate(hookProcDelegate));
			hIMC = ImmGetContext(window.Handle);
			initialized = true;
		}

        //自定义的窗口过程：
        static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            //将消息传给窗口的前一个窗口过程，获取返回码
            IntPtr returnCode = CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);

            //根据消息类型，重构返回码
            switch (msg)
            {

                case WM_GETDLGCODE: //Windows将该消息发送给控件处理，在对话框中或在窗口位置的IsDialogMessage函数处理键盘输入
                                    //指定控件可处理的输入类型（ALLKEYS表所有类型按键），常用以防止Windows执行默认的键盘消息的响应
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;

                case WM_KEYDOWN:   //发布“键按下”事件
                    if (KeyDown != null)
                        KeyDown(null, (Keys)wParam); 
                    break;

                case WM_KEYUP:     //发布“键弹出”事件
                    if (KeyUp != null)
                        KeyUp(null, (Keys)wParam);
                    break;

                case WM_CHAR:     //输出消息数据，发布“命令输入”事件or普通的“字符输入”事件      
                    Console.WriteLine("WM_CHAR: {0} {1:X}", (int)wParam, (int)lParam);
                    char c = (char)wParam;
                    if (char.IsControl(c)) //判断是否是控制字符
                    {
                        if (CommandEntered != null) CommandEntered(null, c);
                    }
                    else
                    {
                        if (CharEntered != null) CharEntered(null, c);
                    }
                    break;
                case WM_IME_SETCONTEXT:   //设置窗口的输入法，不考虑返回码
                    if (wParam.ToInt32() == 1)
                        ImmAssociateContext(hWnd, hIMC);
                    break;
                case WM_INPUTLANGCHANGE:  //输入法发生了改变，返回码为1
                    ImmAssociateContext(hWnd, hIMC);
                    returnCode = (IntPtr)1;
                    break;
            }
            return returnCode;
        }

	}

    /// <summary>
    /// 剪切板类：通过设置线程的“单元状态”确保剪切板的单线程访问
    /// </summary>
	public static class ClipBorad
	{
		//Thread has to be in Single Thread Apartment state in order to receive clipboard
		static string _clipboardResult = "";

        //单线程套间特性，同一时间只有一个线程能够访问套间内的对象；
        //只能应用于入口点方法（C# 和 Visual Basic 中的 Main() 方法），对其他方法无效
        //此处的设置是无效的，真正起作用的是对线程的单元状态的设置SetApartmentState？？？
		[STAThread]
		static void PasteThread()
		{
			try
			{
				if (Clipboard.ContainsText())
				{
					_clipboardResult = Clipboard.GetText();
				}
				else
				{
					_clipboardResult = "";
				}
			}
			catch
			{
			}
		}

		[STAThread]
		static void CopyThread()
		{
			try
			{
				Clipboard.SetText(_clipboardResult);
			}
			catch
			{
			}
		}

		public static string Text
		{
           //创建线程读取与设置剪贴板信息
			get
			{
				//XNA runs in Multiple Thread Apartment state, which cannot recieve clipboard
                //XNA运行在多线程单元状态，不能访问剪切板
				Thread thread = new Thread(PasteThread);
                //设置线程进入“单线程”单元
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
				return _clipboardResult;
			}
			set
			{
				_clipboardResult = value;
				Thread thread = new Thread(CopyThread);
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
			}
		}
	}
}
/***********************
 * 主要功能与实现
 * 1.保证剪切板访问与设置的单线程性；
 * 2.设置游戏窗口的回调函数，用以处理传给游戏窗口的消息
 * 控件消息
 * 按键消息
 * 输入法消息
 * 3.导入动态链接库，使用非托管的API函数
 * *********************/
