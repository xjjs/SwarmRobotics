using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GucUISystem
{
    //定义三种委托（实际为“函数指针类型”），无类型参数、单类型参数、双类型参数
	public delegate void GucEventHandler(GucControl sender);
	public delegate void GucEventHandler<in T>(GucControl sender, T args);
	public delegate void GucEventHandler<in T1, in T2>(GucControl sender, T1 arg1, T2 arg2);
    //依赖关系：一个类使用了另一个类（偶然性、临时性）
    //关联关系：强依赖关系，无偶然性和临时性
    //聚合关系：特殊的关联，has-a，整体与部分可分，可有各自的生命周期
    //组合关系：特殊的关联，has-a，整体与部分不可分，整体生命周期结束意味着部分的周期结束
    //委托：函数指针类型，事件：函数指针变量

    /// <summary>
    /// 控件的基类：
    /// 主要属性：可视性、重绘开关
    /// 主要事件：尺寸改变、位置改变、输入事件
    /// 关联对象：父控件、鼠标控件、激活控件，控件列表接口、控件列表对象、内部控件列表对象
    /// 内部属性：区域、位置与尺寸、尺寸限制、背景颜色
    /// </summary>
	public abstract class GucControl// : GucComponent//, IGucDraw
    {
        //控件的皮肤、标签，本类中未用到
		public static Skin Skin = null;   
        public object Tag { get; set; }  
//        public string AA{get;set;}

        // 可视性、重绘开关
        public bool Visible { get; set; }         
        public bool RequireRedraw { get; set; }  

        //事件（实际为“委托变量”-“函数指针变量”），规范委托名单属性的订阅与发布，订阅者的同名方法要在事件上登记
        //控件的事件（尺寸改变、位置改变）与输入设备的事件
        public event GucEventHandler SizeChanged, PositionChanged;
        public event GucEventHandler<InputEventArgs> InputEvent;  

        //双击的时间间隔
		const int DoubleClickSpan = 100;  //in milliseconds
        //父控件、鼠标所在控件
		GucControl parent,  mouseOn;      
        //本控件的矩形区域，注意是Rectangle类型而非自定义的区域类型
        //C#的类型一般都是引用类型，而XNA中的类型则多可以看做值类型（不需要new就可创建空间、作形参时是值传递），如Vector2/3、Rectangle等
        private Rectangle region; 
        //本控件的位置与尺寸、尺寸限制、背景颜色
        private int x, y, width, height;  
        private Point minsize, maxsize;   
        private Color backColor;

        //泛型接口的继承顺序为IEnumerable<T>->ICollection<T>->IDictionary<TKey,TValue>/IList<T>；
        //定义了两个属性：两个下级控件列表，一个是内部控件列表、一个是普通控件列表（公共）；
        protected XNAControlCollection InnerControls { get; private set; }
        public XNAControlCollection Controls { get; private set; }
        //定义了接口属性，以访问两个列表中的下级控件
        protected IEnumerable<GucControl> AllControls { get; private set; }



        //本控件的尺寸、位置
        private Vector2 size, position;  

        //本控件上有父控件，下有下级控件列表，控件列表成员可能还有下级控件列表，即本控件只是控件链中的一员
        //parentPos为到父控件为止所有控件位置的累加和，TotalPos为到本控件为止所有位置的累加和，即X/Y本身就是控件的绝对位置
		private Vector2 TotalPos, parentPos; 
         
        //更新到本控件的累加位置
        private void UpdateXY() {               
            TotalPos.X = parentPos.X + x;
            TotalPos.Y = parentPos.Y + y;
        }
	
        //利用尺寸限制截断过大或过小的尺寸
        private int ClampWidth(int w) {
            if (w < minsize.X) return minsize.X;
            if (w > maxsize.X) return maxsize.X;
            return w;
        }
        private int ClampHeight(int h) {
            if (h < minsize.Y) return minsize.Y;
            if (h > maxsize.Y) return maxsize.Y;
            return h;
        }

        //事件处理函数（不必与事件同名）
        //在事件PosChange上注册，以实现发布事件时的处理工作（更新父控件位置）
        private void ParentPosChange(GucControl parent) { parentPos = parent.TotalPos; }


        //渲染目标（render target）是一个缓冲，显卡通过该缓冲使用一个Effect类绘制场景的像素
        //默认渲染目标叫后备缓冲，物理上就是含下一帧要绘制的信息的一块显存
        //可用RenderTarget2D类创建另一个渲染目标，在显存中保留一块新区域用于绘制
        //使用方法：创建一个指定高、宽或其他选项的RenderTarget2D对象，然后调用GraphicsDevice.SetRenderTarget将其设为当前渲染目标

        //深度缓冲（Depth Buffer）是与渲染目标相同大小的缓冲，该缓冲记录每个像素的深度；
        //如果当前深度函数是CompareFunction.LessEqual时，只有小于等于当前深度值的值才会被保留，而大于当前深度值的值会被抛弃,这叫做深度测试
        //每次绘制像素时都会进行深度测试，当对一个像素进行深度测试时，它的颜色会被写入渲染目标，而深度被写入深度缓冲。

        //在XNA Framework中，Model类表示整个模型。而Model中每个独立的mesh都对应一个ModelMesh。
        //每个ModelMesh包含一个ParentBone，这个ParentBone控制相对于模型的位置和朝向。Model有一个Root bone，它决定了模型的位置和朝向。
        //每个ModelBone都有一个parent和许多children。模型对象的root bone是最终的parent。
        //它的children是ModelMesh对象的bone，而这个ModelMesh对象可能还有其他ModelMesh bones作为它的children。
        //对于给定的一族bone，旋转parent bone也会导致旋转它的children和这个children的children。
        //每个bone都有一个变换矩阵(叫做Transform)定义相对于parent bone的位置和旋转。当绘制ModelMesh时，世界矩阵就是基于这个bone变换的。

        //在绘制过程中加入变换的bone的最简单方法是使用CopyAbsoluteBoneTransformsTo方法。
        //这个方法提取相对的bone变换，并将这个变换最终变为相对于模型Root bone的变换（译者注：即绝对bone变换）。然后返回这些变换的拷贝。
        //当你绘制每个ModelMesh时，你可以使用绝对bone变换作为世界矩阵的第一个部分。通过这种方式，你无需考虑parent bone和它们之间的联系。

        //“尺寸改变”的事件处理函数：新建渲染目标、开启重绘开关、调用自身事件处理函数、通知其他事件处理函数
        protected void SizeChange() {
            //try
            //{
            if (graphicsDevice != null) renderBuffer = new RenderTarget2D(graphicsDevice, width, height);
            //}
            //catch
            //{
            //    renderBuffer = null;
            //}
            RequireRedraw = true;
            //自己的事件处理函数
            OnSizeChange();
            //发布事件到其他事件处理函数
            if (SizeChanged != null) SizeChanged(this);   
        }
        //自身事件处理函数
        protected virtual void OnSizeChange() { }


        //父控件属性，“父控件位置改变”事件处理程序、父控件的控件列表、到本控件累加位置与重绘开关
        public GucControl Parent       
        {
            get { return parent; }
            set {
                if (parent != value)
                {
                    //移除注册到父控件的“父控件位置改变”事件处理程序
                    //从父控件的“控件列表”or内部控件列表中移除自身（只在其一）
                    if (parent != null)          
                    {
                        parent.PositionChanged -= ParentPosChange;  
                        if (!parent.Controls.Remove(this))
                            parent.InnerControls.Remove(this);
                        //本句可以删除
                        parent = null;
                    }

                    parent = value;
                    //向新的父控件注册“父控件位置改变”事件处理程序，更新到父控件的累加位置
                    //优先添加到父控件的“控件列表”
                    if (value != null)
                    {
                        parent.PositionChanged += ParentPosChange;   
                        ParentPosChange(parent);
                        if (!parent.InnerControls.Contains(this))     
                            parent.Controls.Add(this);
                    }
                    else
                        parentPos = Vector2.Zero;

                    //更新累加位置，开启重绘开关
                    UpdateXY();                
                    RequireRedraw = true;      
                }
            }
        }

        //重置本控件尺寸，触发尺寸改变“事件”（虽然只是朴素的函数调用）
		public virtual int Width
		{
			get { return width; }
			set
			{
				width = ClampWidth(value);
				size.X = width;
				region.Width = width;
				SizeChange();
			}
		}
		public virtual int Height
		{
			get { return height; }
			set
			{
				height = ClampHeight(value);
				size.Y = height;
				region.Height = height;
				SizeChange();
			}
		}

        //重置本控件的位置（区域左上角坐标），更新累加位置，发布“位置改变”事件事件
		public virtual int X
		{
			get { return x; }
			set
			{
				x = value;
				position.X = x;
				region.X = x;
				//DrawRegion.DrawPos = position;
				UpdateXY();
				if (PositionChanged != null) PositionChanged(this);  
			}

		}
		public virtual int Y
		{
			get { return y; }
			set
			{
				y = value;
				position.Y = y;
				region.Y = y;
				UpdateXY();
				if (PositionChanged != null) PositionChanged(this);  
			}
		}


        //边界属性，重置区域尺寸限制，必然随之重置尺寸，但是缺少了相应的“尺寸改变事件”处理函数？？？
        //只读属性：区域、边界位置
        //读写属性：位置（取整处理、位置改变事件-没有开启重绘开关？？？）、尺寸（取整处理、尺寸改变事件）、颜色（开启重绘开关）
		public Point MinSize { get { return minsize; } 
            set { minsize = value; Width = ClampWidth(Width); Height = ClampHeight(Height); } }
		public Point MaxSize { get { return maxsize; } 
            set { maxsize = value; Width = ClampWidth(Width); Height = ClampHeight(Height); } }
		public Rectangle Region { get { return region; } }
		public int Right { get { return region.Right; } }
		public int Bottom { get { return region.Bottom; } }
		public Vector2 Position {
			get { return position; }
			set
			{
                //取整处理
				position.X = region.X = x = (int)Math.Ceiling(value.X);
				position.Y = region.Y = y = (int)Math.Ceiling(value.Y);
				UpdateXY();
				if (PositionChanged != null) PositionChanged(this);
			}
		}
		public virtual Vector2 Size
		{
			get { return size; }
			set
			{
				width = ClampWidth((int)Math.Ceiling(value.X));
				size.X = width;
				region.Width = width;
				height = ClampHeight((int)Math.Ceiling(value.Y));
				size.Y = height;
				region.Height = height;
				SizeChange();
			}
		}
		public virtual Color BackColor {
			get { return backColor; }
			set
			{
				backColor = value;
				RequireRedraw = true;
			}
		}

        //声明or定义属性：显卡、缓冲目标、DrawRegion列表、精灵批处理对象、深度、拖拽标志、区域的偏移
        public GraphicsDevice graphicsDevice { get; private set; }
        public RenderTarget2D renderBuffer { get; protected set; }
        protected List<ControlDrawRegion> CustomDrawRegions { get; private set; }
        protected SpriteBatch spriteBatch { get; private set; }
        public float DisplayDepth { get; set; }
        public bool isNotDragging { get; private set; }
        protected int customXOffset, customYOffset;

        /// <summary>
        /// set: RequireRedraw = RequireRedraw | value
        /// </summary>
        //{
        //get { return requireRedraw; }
        //set { if (value && !requireRedraw) requireRedraw = true; }
        //    get { return requireRedraw; }
        //    set
        //    {
        //        if (value && !requireRedraw)
        //        {
        //            requireRedraw = true;
        //            if (this.parent != null) this.parent.requireRedraw = true;
        //        }
        //    }
        //}

        /// <summary>
        ///递归地设置各级控件：设置图形设备、精灵批处理对象、新建渲染目标，前两者共有、渲染目标则是各自的
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public virtual void BindGraphic(GraphicsDevice graphicsDevice) {
            this.graphicsDevice = graphicsDevice;
            if (parent == null)
                spriteBatch = new SpriteBatch(graphicsDevice);
            else
                spriteBatch = parent.spriteBatch;
            //try
            //{
            renderBuffer = new RenderTarget2D(graphicsDevice, width, height);//, true, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            //}
            //catch
            //{
            //    renderBuffer = null;
            //}

            //以递归的方式，将“下级控制项列表”中的所有控制项绑定到相应的图形设备，foreach自带终止判断（AllControls是否包含元素）
            foreach (var ctrl in AllControls)
                ctrl.BindGraphic(graphicsDevice);
        }


        public GucControl()
        {
            //Contact是在Enurable类上实现的IEnumerable的扩展方法，ICollection接口继承了IEnumerable接口
            //创建“内部控件列表”对象、“控件列表”对象
			InnerControls = new XNAControlCollection(this);
			Controls = new XNAControlCollection(this);
			AllControls = InnerControls.Concat(Controls);

			//FrontColor = Color.White;
            //设置背景颜色（透明）、可视性、尺寸边界、区域尺寸（注意初始是单位正方形、以后会涉及相关运算）
			BackColor = Color.Transparent;
			Visible = true;
			minsize = new Point(1, 1);
			maxsize = new Point(int.MaxValue, int.MaxValue);
			size.X = region.Width = width = 1;
			size.Y = region.Height = height = 1;
			//PreviewInput = false;

            //置空指针：父控件、鼠标控件、激活控件、显卡对象
            parent = null;
			mouseOn = active = null;
			graphicsDevice = null;

            //关闭：鼠标在内、激活
			MouseInside = false;
			IsActive = false;
            //开启：重绘、非拖拽、可捕获焦点、使能
			RequireRedraw = true;
			isNotDragging = true;
			canHaveFocus = true;
			enable = true;

            //控件对应的drawRegion列表，只用于描述本控件区域region，设置显示层次
			CustomDrawRegions = new List<ControlDrawRegion>();
			DisplayDepth = DrawingDepth.ChildrenControl3;
        }


		/// <summary>
		/// Checks if coordinate (<paramref name="X"/>,<paramref name="Y"/>) is with in control
		/// </summary>
		/// <param name="X">X coordinate relative to container X coordinate</param>
		/// <param name="Y">Y coordinate relative to container Y coordinate</param>
        /// 
        //某点是否在控件的region内（前提是控件可视）
		public virtual bool IsInControl(Point pos) { return Visible && region.Contains(pos); }

        //region是本控件的总区域，CustomDrawRegions是需要控件自己绘制的区域，其他区域可以调用子控件绘制
        //控件与子控件不一定是范围包含，子控件有一部分可以在外边，只是在外边的部分不会显示
        //InnerControls与Controls都是子控件的集合，只不过InnerControls是为了画图或者重用的
        
        /// <summary>
        /// 以递归方式绘制各级控件：每个控件绘制自身区域列表DrawRegions与子控件的总区域region
        /// </summary>
        /// <returns></returns>
		public virtual bool Draw() 
		{
            //若AllControl内没有元素，则不会进入下级，相当于一个递归终止条件
            //有一个返回true，最终位运算的结构就是true，即若有一个子控件需重绘（且已重绘）则本控件就要重绘
			foreach (var ctrl in AllControls)
				RequireRedraw |= ctrl.Draw();  

            //绘制自己的DrawRegion列表与子控件的总区域region
			if (RequireRedraw)
			{
				//var render = graphicsDevice.GetRenderTargets();
                //设置本控件的缓冲目标为当前缓冲目标，清除资源缓冲并设置所有缓冲区的默认颜色
				graphicsDevice.SetRenderTarget(renderBuffer);
				graphicsDevice.Clear(BackColor);

                //BackToFront是从后(靠近1)往前（靠近0）绘制，深度值越小，越靠近上层
                //AlphaBlending模式的颜色混合，源颜色与目标颜色以因子alpha叠加混合
				this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                //自己绘制本控件的区域列表
				foreach (var r in CustomDrawRegions)
				{
					if (r.Show)
                        r.Draw(spriteBatch);
				}

                //绘制下级控件的region总区域
				foreach (var ctrl in AllControls)
				{
					//if (ctrl.Visible) ctrl.DrawRegion.Draw(spriteBatch);
                    //RenderTarget2D继承自Texture2D，故可以作为参数传入
					if (ctrl.Visible)
						spriteBatch.Draw(ctrl.renderBuffer, ctrl.position, null, ctrl.enable ? Color.White : Color.Gray, 
                            0, Vector2.Zero, 1,
							SpriteEffects.None, ctrl == active ? DrawingDepth.ActiveControl : ctrl.DisplayDepth);
				}
				this.spriteBatch.End();

				graphicsDevice.SetRenderTargets(null);
				//var stream = System.IO.File.Open(string.Format("{0}-{1}.jpg", width, height), System.IO.FileMode.Create);
				//renderTexture.SaveAsJpeg(stream, width, height);
				//stream.Close();

                //关闭重绘开关
				RequireRedraw = false;
				//if (OnRedraw != null) OnRedraw(this);
				return true;
			}
			return false;
		}

        /// <summary>
        /// 检测某点（相对于本控件的坐标）是否在region列表内（不是DrawRegion列表），是则返回region索引
        /// </summary>
        /// <param name="point"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
		protected int CheckPosition(Point point, params Rectangle[] regions)
		{
			point.X -= x;
			point.Y -= y;
			for (int i = 0; i < regions.Length; i++)
				if (regions[i].Contains(point)) return i + 1;
			return 0;
		}





        //控件激活：使能+获取焦点
        //解析输入设备的数据，鼠标移动、鼠标激活
		public void ParseInput(InputEventArgs input)
		{
			ParseMouseMove(input);
			ParseActivation(input);
		}

        /// <summary>
        /// 根据鼠标位置，查找最高层的“子控件”or“自绘制区域”
        /// </summary>
		protected virtual GucControl FindMouseOnControl(Point pos)
		{
            //目标控件，ch为绘图深度
			GucControl curMOn = null;
			float h = 1, ch;

            //遍历所有子控件，查找包含指定位置的、可视的、最上层的控件
			foreach (var item in AllControls)
			{
                //激活的控件设置单独的激活层次，实际上IsInControl本身已有对Visible的要求
				ch = (active == item) ? DrawingDepth.ActiveControl : item.DisplayDepth;
				if (item.Visible && ch < h && item.IsInControl(pos))
				{
					curMOn = item;
					h = ch; 
				}
			}

            //若找到了包含pos的子控件，则遍历所有自绘制区域
            //若包含该位置的可视区域居于子控件上层，则清除子控件标记
			if (curMOn != null) 
			{
                
				foreach (var item in CustomDrawRegions)  
				{
					if (item.Show && item.Contains(pos) && item.Depth < h)
					{
						curMOn = null;
						break;
					}
				}
			}

            //清零鼠标偏移
			customXOffset = customYOffset = 0;
			return curMOn;
		}

        /// <summary>
        /// 由“输入事件”对象解析鼠标移动情况（并递归解析）：离开控件、进入控件、移动事件
        /// </summary>
		void ParseMouseMove(InputEventArgs input)
		{
			Point pos0 = input.MousePosition;

            //若鼠标不在拖拽状态，则查找鼠标所在控件（鼠标控件）
            //根据“原鼠标控件”和“新鼠标控件”发布鼠标进入与离开的事件
			if (isNotDragging)
			{
				//Move events
				input.MousePosition = new Point(pos0.X - X, pos0.Y - Y);

                //这里传入的位置是相对位置？？？
				var curMOn = FindMouseOnControl(input.MousePosition);
				if (curMOn != null && !curMOn.enable) curMOn = null;
				if (mouseOn != curMOn)
				{
                    if (mouseOn != null) mouseOn.invokeMouseLeave();
					mouseOn = curMOn;
					if (mouseOn != null) mouseOn.invokeMouseEnter();
				}
			}

            //非拖拽状态下对鼠标控件的查找会清零偏移，因为只有拖拽状态下才会关注偏移量
			input.MousePosition = new Point(pos0.X - X + customXOffset, pos0.Y - Y + customYOffset);

            //若鼠标控件非空，则发布鼠标移动事件，并递归解析子控件的鼠标移动
			if (mouseOn != null)
			{
				mouseOn.invokeMouseMove(input.MousePosition);
				mouseOn.ParseMouseMove(input);
			}
            //恢复鼠标位置
			input.MousePosition = pos0;
		}

        /// <summary>
        /// 由“输入事件”对象解析鼠标激活情况
        /// </summary>
		void ParseActivation(InputEventArgs input)
		{
			Point pos0 = input.MousePosition, pos;

			//if (customYOffset != 0) customYOffset = customYOffset;

            //计算鼠标带偏移的相对位置
			pos = new Point(pos0.X - X + customXOffset, pos0.Y - Y + customYOffset);
			input.MousePosition = pos;
			//OnPreviewInput(input);
			//if (PreviewInput && PreviewInputEvent != null) PreviewInputEvent(this, input);
			//Activate control

            //若单击了鼠标控件则激活该控件
			if (input[MouseButtons.LeftButton].ButtonState == InputEventTypes.Press)
			{
				Activate(mouseOn);
				//if (mouseOn != null) mouseOn.ParseActivation(input);
			}

            //若单击激活了鼠标控件
			if (active != null)
			{

                //若只有左键处于Press事件，则激活Dragging条件；为什么要遍历其余的按键？
                //处于Dragging的条件：有按键处于按下状态且高索引的按键不释放
				foreach (var mb in input.MouseButtons)
				{
					if (mb.ButtonState == InputEventTypes.Press)
						isNotDragging = false;
					else if (mb.ButtonState == InputEventTypes.Release)
						isNotDragging = true;
				}
                //实现事件，发布（输入）事件
				OnParseInput(input);
				if (InputEvent != null) InputEvent(this, input);
                //递归地解析后继激活节点的激活情况
				active.ParseActivation(input);
			}
			else
			{
				//若未激活鼠标控件，则依次遍历各按键，针对其状态发布事件
                //处于Dragging的条件：有按键处于按下状态且高索引的按键不释放
				foreach (var mb in input.MouseButtons)
				{
					if (mb.ButtonState == InputEventTypes.Press)
					{
						isNotDragging = false;
						OnMouseDown(mb.Button, pos);
						if (MouseDown != null) MouseDown(this, mb.Button, pos);
						OnMousePress(mb.Button, pos);
						if (MousePress != null) MousePress(this, mb.Button, pos);
					}
					else if (mb.ButtonState == InputEventTypes.Release)
					{
						isNotDragging = true;
						OnMouseUp(mb.Button, pos);
						if (MouseUp != null) MouseUp(this, mb.Button, pos);
					}
					else if (mb.ButtonState == InputEventTypes.Repress)
					{
						OnMousePress(mb.Button, pos);
						if (MousePress != null) MousePress(this, mb.Button, pos);
					}
				}
				//Click & Double Click
                //若鼠标进入本控件且发生释放事件
				if (input[MouseButtons.LeftButton].ButtonState == InputEventTypes.Release && MouseInside)
				{
                    //发布单击事件
					OnClick();
					if (Click != null) Click(this);

                    //LastUpdateTime是一个固定值（每帧所用时间），为什么可用来确认双击事件？
                    //这里lastClickTime最终又被赋予相同的值，其差为0，肯定小于双击间隔；即所有单击会被误判为双击？
					if (input.LastUpdateTime - lastClickTime < DoubleClickSpan)
					{
						OnDoubleClick();
						if (DoubleClick != null) DoubleClick(this);
					}
					lastClickTime = input.LastUpdateTime;
				}
                //发布输入的事件
				OnParseInput(input);
				if (InputEvent != null) InputEvent(this, input);
			}
            //恢复鼠标位置
			input.MousePosition = pos0;
		}


		protected virtual void OnParseInput(InputEventArgs input) { }
		//public bool PreviewInput { get; set; }
		//public event GucEventHandler<InputEventArgs> PreviewInputEvent;
		//protected virtual void OnPreviewInput(InputEventArgs input) { }



        //鼠标的移入与移出事件
		public event GucEventHandler MouseLeave, MouseEnter;
		public event GucEventHandler<Point> MouseMove;
		public bool MouseInside { get; private set; }

        //若检测到鼠标移出本控件，则递归地清除鼠标所在控件mouseOn的记录，关闭鼠标在本控件内的开关
		internal void invokeMouseLeave()
		{
			if (mouseOn != null)
			{
                //递归地清除
				mouseOn.invokeMouseLeave();
				mouseOn = null;
			}
			MouseInside = false;
            //实现事件，发布事件
			OnMouseLeave();
			if (MouseLeave != null) MouseLeave(this);
		}
		protected virtual void OnMouseLeave() {  }

        //若检测到鼠标移入本控件，则开启鼠标在本控件内的开关
		internal void invokeMouseEnter()
		{
			MouseInside = true;
            //实现事件，发布事件
			OnMouseEnter();
			if (MouseEnter != null) MouseEnter(this);
		}
		protected virtual void OnMouseEnter() {  }
       
        //若检测到鼠标移动，则实现事件，发布事件
		internal void invokeMouseMove(Point MousePosition)
		{
			OnMouseMove(MousePosition);
			if (MouseMove != null) MouseMove(this, MousePosition);
		}
		protected virtual void OnMouseMove(Point MousePosition) { }



        //鼠标的按键事件
		int lastClickTime;
		public event GucEventHandler Click, DoubleClick;
		/// <summary>
		/// Invoked when mouse button is pressed for the first time
		/// </summary>
		public event GucEventHandler<MouseButtons, Point> MouseDown;
		/// <summary>
		/// Inovked when mouse button is released
		/// </summary>
		public event GucEventHandler<MouseButtons, Point> MouseUp;
		/// <summary>
		/// Invoked when mouse button is being pressed, repeated every 100 ms
		/// </summary>
		public event GucEventHandler<MouseButtons, Point> MousePress;
		protected virtual void OnClick() { }
		protected virtual void OnDoubleClick() { }
        //发布单击事件
		public void InvokeClick() { if (Click != null) Click(this); }
        //定义事件处理函数，缺少第一个参数？？？
		protected virtual void OnMouseDown(MouseButtons MouseButton, Point MousePosition) { }
		protected virtual void OnMouseUp(MouseButtons MouseButton, Point MousePosition) { }
		protected virtual void OnMousePress(MouseButtons MouseButton, Point MousePosition) { }

        //激活状态下，若失去焦点或失去使能，都将关闭激活；即激活为两者同时满足
        //是否捕获焦点
		private bool canHaveFocus;
		public bool CanHaveFocus
		{
			get { return canHaveFocus; }
			set
			{
				if (canHaveFocus != value)
				{
					canHaveFocus = value;
                    //激活状态下放弃焦点，则关闭激活
					if (!value && IsActive) DeActivate();
				}
			}
		}
        //是否被使能
		private bool enable;
		public virtual bool Enable
		{
			get { return enable; }
			set
			{
				if (enable != value)
				{
					enable = value;
                    //激活状态下放弃使能，则关闭激活
					if (!value && IsActive) DeActivate();
				}
			}
		}

        //active构成了一条激活链
        //传入非null后继结点到Activate，本控件先递归清除后继结点激活记录并关闭（除尾结点）激活状态
           //然后更新自身激活记录并激活传入的单个后继(作为新的尾结点)，最后依次激活前驱
        //传入null到Activate，本控件会递归清除后继结点激活记录并关闭（除尾结点）激活状态，然后清除自身激活记录；

		GucControl active;  
		public GucControl ActiveControl
		{
			get { return active; }
			set { Activate(value); }
		}
        //本控件的属性与方法，即是否激活是描述本控件的
		public bool IsActive { get; protected set; }


        //发布事件的情形1：本控件作为先驱而激活、或关闭激活后继结点
        //发布事件的情形2：本控件作为active链的首结点而激活、或关闭激活（无先驱时，自己操作自己）
		public bool Activate(GucControl control)
		{
            //控件非空的前提下，若不是本控件的子控件、已是激活控件、未激活、无焦点，则返回false
			if (control != null && (control.Parent != this || active == control || !control.CanHaveFocus || !control.Enable)) 
                return false;

            //active的初始值为null
			if (active != null)
			{
                //若传入值为null则以递归的方式依次清除各层的激活记录
				active.Activate(null);
				//active.isNotDragging = true;
                //若active非空，则关闭其激活状态（因为要重新激活control）并发布事件
				active.IsActive = false;
                //事件本身为空函数？发布事件
				active.OnDeActivated();
				if (active.DeActivated != null) active.DeActivated(active);
			}
            //更新激活记录
			active = control;
			if (control != null)
			{
                //为什么要在此处激活控件？因为控件只能被别的控件激活？
				control.IsActive = true;
                //事件本身为空函数？发布事件
				control.OnActivated();
				if (control.Activated != null) control.Activated(control);
                //激活本控件
				this.Activate();
			}
			return true;
		}
   
		public void Activate()
		{
            //若父控件非空则依次记录并激活本结点及前驱结点
            //若父控件为空则激活本控件，并发布事件
			if (parent != null)
				parent.Activate(this);
			else
			{
				IsActive = true;
				OnActivated();
				if (Activated != null) Activated(this);
			}
		}
		public void DeActivate()
		{
            //若父控件非空则清除父控件的激活记录，并关闭本控件的激活状态
            //若父控件为空则要清除本控件的激活记录和关闭激活状态，并发布事件
			if (parent != null)
				parent.Activate(null);
			else
			{
				if (active != null) Activate(null);
				IsActive = false;
				OnDeActivated();
				if (DeActivated != null) DeActivated(this);
			}
		}

        //事件与处理函数
		public event GucEventHandler Activated, DeActivated;
		protected virtual void OnActivated() { }
		protected virtual void OnDeActivated() { }

	}
}

/***********************************
 * 主要功能与实现
 * 1.实现基本的控件结点，对输入数据（键盘和鼠标）进行解析，并触发相应事件
 * 2.本控件结点，作为三个控件链中元素：
 * 总体控件列表：父控件parent、本区域region + 本区域列表CustomDrawRegions、下级控件列表（内部、普通、所有），共用图形设备；
 * 鼠标控件列表：鼠标控件mouseON、从下级控件中查找目标；在本控件触发鼠标移动事件，并递归触发后继控件的鼠标移动事件；
 * 激活控件列表：激活控件active、激活下级控件；查询鼠标的按键事件，并触发本控件的事件，双击事件触发有问题？
 * 3.激活（active非空）：左键Press，使能 + 捕获鼠标焦点（可视、最上层、包含鼠标位置的下级控件，前提为本区域列表不合要求）；
 * 失去焦点或关闭使能，则关闭激活；
 * 发布激活事件1：本控件作为先驱而激活、或关闭后继结点
 * 发布激活事件2：本控件作为active链的首结点而激活、或关闭（无先驱时，自己操作自己）
 * 4.重绘：总体控件列表的后继结点有一个需要重绘则重绘本控件，即绘制“本区域列表”+“所有可视的下级控件？”；
 * 5.位置：
 * position：本控件的位置坐标；
 * parentPos：到父控件为止，控件链上结点位置坐标的累加和；
 * TotolPos：到本控件为止，控件链上结点位置坐标的累加和；
************************************/