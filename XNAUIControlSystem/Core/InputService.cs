using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GucUISystem
{
    //键盘or鼠标的按键事件的枚举类型：释放、按下、再次按下（双击）
    public enum InputEventTypes
    {
        None = 0, Release = 1, Press = 2, Repress = 3
    }
    //鼠标按键的枚举类型：左、中、右
    public enum MouseButtons
    {
        LeftButton = 0, MiddleButton = 1, RightButton = 2
    }

    /// <summary>
    /// 键盘事件类（虽然命名是状态）：由按键的前后状态（是否按下）设置其事件类型（无、释放、按下、双击）
    /// </summary>
    public struct KeyState
    {
        //按键、事件类型、按下标识、计数器（两次按下的时间间隔）
        public Keys Key { get; private set; }
        public InputEventTypes PressState { get; private set; }
        public bool isPressing { get; private set; }
        internal float count;


        //this()调用默认构造器，这里没有
        public KeyState(Keys key)
            : this()
        {
            count = InputService.KeyResetTime;
            this.Key = key;
            isPressing = false;
            PressState = InputEventTypes.None;
        }

        //更新按键的事件，isPressing是原有按键标识，pressed是当前按键标识
        //根据标识变化设置状态：Press->NoPress为Release，NoPress->NoPress为None；
        //NoPress->Press为Press，并重置计数器；Press->Press大于时间间隔为RePress，否则为None（相当于什么都没发生）；
        internal void Set(bool pressed, int timeElapsed)
        {
            if (pressed)
            {
                if (isPressing)
                {
                    count -= timeElapsed;
                    if (count < 0)
                    {
                        count += InputService.KeyResetTime;
                        PressState = InputEventTypes.Repress;
                    }
                    else
                        PressState = InputEventTypes.None;
                }
                else
                {
                    PressState = InputEventTypes.Press;
                    count = InputService.KeyResetTime;
                }
            }
            else
                PressState = isPressing ? InputEventTypes.Release : InputEventTypes.None;
            isPressing = pressed;
        }

        //输出按键、按键标识、按键事件
		public override string ToString()
		{
			return string.Format("{0}({1})--{2}", Key, isPressing ? "Pressing" : "Releasing", PressState);
		}
    }

    /// <summary>
    /// 鼠标事件类（虽然命名是状态）：由按键的前后状态（是否按下）设置其事件类型（无、释放、按下、双击）
    /// </summary>
    public struct MouseButtonState
    {
		public MouseButtonState(MouseButtons Button) : this() { this.Button = Button; }

		//internal void Set(bool pressed)
		//{
		//    //if (pressed != isPressing)
		//    //{
		//    //    if (pressed)
		//    //        ButtonState = InputEventTypes.Down;
		//    //    else
		//    //        ButtonState = InputEventTypes.Up;
		//    //}
		//    //else
		//    //    ButtonState = InputEventTypes.None;
		//    ButtonState = pressed != isPressing ? InputEventTypes.None : (pressed ? InputEventTypes.Press : InputEventTypes.Release);
		//    isPressing = pressed;
		//}

        //按键、按键标识、鼠标事件、计数器
        public MouseButtons Button { get; private set; }
        public bool isPressing { get; private set; }
        public InputEventTypes ButtonState { get; private set; }
        internal float count;

        //更新按键的事件（同键盘），isPressing是原有按键标识，pressed是当前按键标识
		internal void Set(bool pressed, int timeElapsed)
		{
			if (pressed)
			{
				if (isPressing)
				{
					count -= timeElapsed;
					if (count < 0)
					{
						count += InputService.KeyResetTime;
						ButtonState = InputEventTypes.Repress;
					}
					else
						ButtonState = InputEventTypes.None;
				}
				else
				{
					ButtonState = InputEventTypes.Press;
					count = InputService.KeyResetTime;
				}
			}
			else
				ButtonState = isPressing ? InputEventTypes.Release : InputEventTypes.None;
			isPressing = pressed;
		}
	}


    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// 将输入服务实现为一个组件，记录所有键盘键与鼠标键的事件并定时更新
    /// </summary>
    public class InputService : GameComponent {

        //键盘事件与索引，鼠标事件
        internal KeyState[] keyList;
        int[] keyIndex;
        internal MouseButtonState[] mouseList;

        //创建列表对象，设置更新优先级（屏幕管理器）
        public InputService(ScreenManager manager)
            : base(manager.Game) {

            //typeof在编译时绑定到一个特定的Type对象，可访问名称、接口、特性、属性、可访问性等成员
            //GetValues传入枚举类型的Type对象，返回对应的枚举值数组；

            //创建列表对象，设置“组件的更新优先级”（越小越先更新）
            keyList = new KeyState[Enum.GetValues(typeof(Keys)).Length];
            keyIndex = new int[256];
            mouseList = new MouseButtonState[Enum.GetValues(typeof(MouseButtons)).Length];
            //upList = new List<InputKey>();
            //downList = new List<InputKey>();
            UpdateOrder = manager.UpdateOrder - 1;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// 游戏组件在开始运行前的初始化工作，可查询服务or载入资源，此处用于生成各“按键值”对应的“索引”与“事件”
        /// </summary>
        public override void Initialize() {
            int count = 0;
            foreach (Keys item in Enum.GetValues(typeof(Keys)))
            {
                //记录各个按键枚举值的编号
                keyIndex[(int)item] = count;
                //记录各个按键的状态
                keyList[count] = new KeyState(item);
                count++;
            }
            base.Initialize();
        }


        //读写属性：鼠标移动标识、鼠标移动距离、鼠标位置、控制键标识（Alt/Control/Shifr）、上次更新时间、鼠标滚轮初始值
        public bool MouseMove { get; private set; }
        public int MouseDelta { get; private set; }
        public Point MousePosition { get; private set; }
        public bool Alt { get; private set; }
        public bool Control { get; private set; }
        public bool Shift { get; private set; }
        public int LastUpdateTime { get; private set; }
        int lastScroll = 0;

        /// <summary>
        /// Allows the game component to update itself.
        /// 游戏组件的更新函数，用以更新键盘与鼠标事件
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime) {

            //ElapsedGameTime为从上次Update到现在的时间（若每次都一样则可看作中断间隔）
            LastUpdateTime = gameTime.ElapsedGameTime.Milliseconds;

            //读取键盘状态
            KeyboardState kstate = Keyboard.GetState();
            //遍历并更新所有键盘按键的事件，设置控制键标识
            for (int i = 0; i < keyList.Length; i++)
            {
                keyList[i].Set(kstate.IsKeyDown(keyList[i].Key), LastUpdateTime);
            }
            Alt = this[Keys.LeftAlt].isPressing || this[Keys.RightAlt].isPressing;
            Control = this[Keys.LeftControl].isPressing || this[Keys.RightControl].isPressing;
            Shift = this[Keys.LeftShift].isPressing || this[Keys.RightShift].isPressing;


            //读取鼠标状态
            MouseState ms = Mouse.GetState();
            //更新位置与滚动值、标识是否移动
            if (ms.X != MousePosition.X || ms.Y != MousePosition.Y)
            {
                MouseMove = true;
                MousePosition = new Point(ms.X, ms.Y);      //更新鼠标位置
            }
            MouseDelta = ms.ScrollWheelValue - lastScroll;  //记录滚轮变动
            lastScroll = ms.ScrollWheelValue;               //更新滚轮值

            //遍历并更新所有鼠标按键的事件
            mouseList[0].Set(ms.LeftButton == ButtonState.Pressed, LastUpdateTime);
            mouseList[1].Set(ms.MiddleButton == ButtonState.Pressed, LastUpdateTime);
            mouseList[2].Set(ms.RightButton == ButtonState.Pressed, LastUpdateTime);

            base.Update(gameTime);
        }

        //双击的最小间隔（分辨率）
        public const int KeyResetTime = 200;   //in milliseconds

        //创建输入参数对象
        public InputEventArgs CreateInputArgs() { return new InputEventArgs(this); }

        //由按键值直接索引按键事件
        public KeyState this[Keys key] { get { return keyList[keyIndex[(int)key]]; } }
        public MouseButtonState this[MouseButtons button] { get { return mouseList[(int)button]; } }

        //提供接口属性，为什么不直接返回KeyState[]和MouseButtonState[]，为了编码的统一性？
        public IEnumerable<KeyState> GetAllKeys { get { return keyList; } }
        public IEnumerable<MouseButtonState> GetAllMouseButtons { get { return mouseList; } }

        //<No Shift, Shift>
        //定义字典<键盘按键,字符串对>，Shift则存储不同串，No-Shift存储相同的串
        public static Dictionary<Keys, Tuple<string, string>> DisplayChar;
        static void SetChar(Keys key, string str) { DisplayChar[key] = Tuple.Create(str, str); }
        static void SetChar(Keys key, string str, string str_shift) { DisplayChar[key] = Tuple.Create(str, str_shift); }

        //创建按键字典并设置其值（“字符串对”）
        static InputService() {
            DisplayChar = new Dictionary<Keys, Tuple<string, string>>();
            SetChar(Keys.Space, " ", "");
            //number
            SetChar(Keys.D0, "0", ")");
            SetChar(Keys.D1, "1", "!");
            SetChar(Keys.D2, "2", "@");
            SetChar(Keys.D3, "3", "#");
            SetChar(Keys.D4, "4", "$");
            SetChar(Keys.D5, "5", "%");
            SetChar(Keys.D6, "6", "^");
            SetChar(Keys.D7, "7", "&");
            SetChar(Keys.D8, "8", "*");
            SetChar(Keys.D9, "9", "(");
            //alphabet
            for (int i = 65; i <= 90; i++)
                SetChar((Keys)i, ((char)(i + 32)).ToString(), ((char)i).ToString());
            //NumPad，小键盘
            for (int i = 96; i <= 105; i++)
                SetChar((Keys)i, ((char)(i - 48)).ToString());
            SetChar(Keys.Multiply, "*");
            SetChar(Keys.Add, "+");
            //SetChar(Keys.Separator, "|");
            SetChar(Keys.Subtract, "-");
            SetChar(Keys.Decimal, ".");
            SetChar(Keys.Divide, "/");
            //Signs，各种符号
            SetChar(Keys.OemSemicolon, ";", ":");
            SetChar(Keys.OemPlus, "=", "+");
            SetChar(Keys.OemComma, ",", "<");
            SetChar(Keys.OemMinus, "-", "_");
            SetChar(Keys.OemPeriod, ".", ">");
            SetChar(Keys.OemQuestion, "/", "?");
            SetChar(Keys.OemTilde, "`", "~");
            SetChar(Keys.OemOpenBrackets, "[", "{");
            SetChar(Keys.OemPipe, "\\", "|");
            SetChar(Keys.OemCloseBrackets, "]", "}");
            SetChar(Keys.OemQuotes, "'", "\"");
            //other
            //将未设置的按键设为空
            foreach (Keys item in Enum.GetValues(typeof(Keys)))
            {
                if (!DisplayChar.ContainsKey(item)) SetChar(item, "");
            }
        }

    }



    /// <summary>
    /// 输入事件类：获取输入组件的事件列表
    /// </summary>
    public class InputEventArgs
    {
        //蓝色的Keys为按键枚举类型，黑色的为按键列表，索引器由“类变量”使用
        public KeyState[] Keys { get; private set; }
        public bool Alt, Control, Shift;
        public int GetIndex(Keys key) { return Array.FindIndex(Keys, k => k.Key == key); }
        public KeyState this[Keys key] {
            get { return Keys[GetIndex(key)]; }
            set { Keys[GetIndex(key)] = value; }
        }

		/// <summary>
		/// Get all mouse buttons that trigger an event. Remove the button from the list if the event is parsed.
        /// 索引器由类变量使用
		/// </summary>
		public MouseButtonState[] MouseButtons { get; private set; }
		public MouseButtonState this[MouseButtons index]
		{
			get { return MouseButtons[(int)index]; }
			set { MouseButtons[(int)index] = value; }
		}
        public bool MouseMove; 
        public int MouseDelta;
        public Point MousePosition;
        public int LastUpdateTime;



        internal InputEventArgs(InputService input)
        {
			Keys = (KeyState[])input.keyList.Clone();  //按键状态的浅表副本
			Alt = input.Alt;
			Control = input.Control;
			Shift = input.Shift;

			MouseButtons = (MouseButtonState[])input.mouseList.Clone();  //鼠标按键状态的浅表副本
			MouseMove = input.MouseMove;
			MouseDelta = input.MouseDelta;
			MousePosition = input.MousePosition;
			LastUpdateTime = input.LastUpdateTime;
		}

        //判断键盘按键事件：单击、释放、按下（包括单击and双击）
		public bool isKeyDown(Keys key) { return Keys[GetIndex(key)].PressState == InputEventTypes.Press; }
		public bool isKeyPress(Keys key) { return (Keys[GetIndex(key)].PressState & InputEventTypes.Press) == InputEventTypes.Press; }
		public bool isKeyUp(Keys key) { return Keys[GetIndex(key)].PressState == InputEventTypes.Release; }
	}
   
}

/*************************************
 * 主要功能与实现
 * 1.将输入服务（键盘与鼠标）实现为一个组件，通过Update函数定时更新鼠标与键盘的状态
 * 2.定义按键（键盘或鼠标）的事件类型：无事件、释放、按下、重按（以一定时间间隔连续按两次）
 * 3.定义键盘按键状态类：按键枚举值、开关变量（是否按下）、设置事件
 * 4.定义鼠标按键状态类：按键枚举值、开关变量（是否按下）、设置事件
 * 5.定义输入服务组件：
 * 定义键盘按键状态列表
 * 定义鼠标按键状态列表
 * 定义键盘按键词典
 * 每隔一段时间用Update函数更新列表内的键盘按键状态和鼠标按键状态（开关变量与事件）
 * ***********************************/
