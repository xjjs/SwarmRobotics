using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GucUISystem;
using Microsoft.Xna.Framework;
using RobotLib;

namespace RobotDemo
{
    /// <summary>
    /// 参数设置页：组合框、标签、参数项类列表、instance、类型项列表；
    /// 一个“类型项”列表被绑定到一个“组合框”，被组合框选中的“类型项”转化为instance对象；
    /// 由instance的类型信息（<属性,特性>数组）可进一步生成“参数项列表”，所有“参数项”的控件和“其他控件”都绑定到Page控件中；
    /// 可视化显示：第一行为“Label+组合框”，Label名由构造函数指定；第二行为“Label”，初值由构造函数指定"name+Parameters:"；
    /// 一个解决方案有多个工程项目，就有多个程序集（在Properties下有描述文件AssemblyInfo.cs）
    /// </summary>
	class TypeParaFrame
	{
        //添加字段：组合框、标签、参数项类列表、instance、类型项列表
		GucComboBox mainCombo;
		GucLabel lblPara;
		List<ParameterOption> ControlList;
		IParameter instance;
		TypeItem[] items;
		bool hasPage;

        //容器控件
        public GucContainerControl Page { get; private set; }
        public int Height {
            get { return height; }
            private set {
                height = value;
                if (hasPage) Page.InnerHeight = value;
                if (HeightChanged != null) HeightChanged(this);
            }
        }
        public int Width { get; private set; }
        int height;

		public event Action<TypeParaFrame> TypeChanged;
		public event Action<TypeParaFrame> HeightChanged;

		public TypeParaFrame(Type type, string name, GucContainerControl parent, bool page = true, object[] ctorParameter=null)
		{
			hasPage = page;

            //根据标识选择是否创建新的页面帧
			if (page)
			{
				this.Page = new GucFrameBox();
				Page.Parent = parent;
				Page.AutoInnerSize = true;
			}
			else
				Page = parent;

            //添加组合框到Page，注册选择值变化的事件处理函数
			mainCombo = new GucComboBox();
			Page.Controls.Add(mainCombo);
			mainCombo.Position = new Vector2(250, 0);
			mainCombo.Width = 180;
			mainCombo.SelectedChanged += new GucEventHandler(mainCombo_SelectedValueChanged);
          

            //添加标签到Page，保存name
			lblPara = new GucLabel();
			Page.Controls.Add(lblPara);
			lblPara.Text = name;
			lblPara.Position = new Vector2(5, 4);

            //添加标签到Page，保存name+" Parameters:"
			lblPara = new GucLabel();
			Page.Controls.Add(lblPara);
			lblPara.Text = name + " Parameters:";
			lblPara.Position = new Vector2(5, 30);
			lblPara.Visible = false;

            //设置“参数设置帧”的总宽度
			Width = mainCombo.Right + 10;
			if (page) Page.Width = Width;

            //由类型所在程序集，获取派生类集合
			var DerivedTypes = FindDerivedTypesFromAssembly(type);
            //所有非抽象类生成“类型项”并转化为“按名称排序”的数组
            //创建“类型项”时就已经创建了各项（环境、问题与算法的所有“非抽象派生类”）的实例并创建了默认参数，当选中该项后再初始化其他参数
			items = DerivedTypes.Where(t => !t.IsAbstract).
                Select(t => new TypeItem(t, ctorParameter)).OrderBy(t => t.Type.Name).ToArray();
            //创建参数项列表
			ControlList = new List<ParameterOption>();
		}

        //将“选中项”化为instance
		public void RecreateInstances()
		{
            //重建“类型项”列表的Item成员
			foreach (var item in items)
				item.CreateInstance();
            //组合框的选择项
			instance = (mainCombo.SelectedItem as TypeItem).Item as IParameter;
		}

        //根据传入的选择函数决定是否对元素进行投射变换，将“类型项”添加到组合框的内部列表，根据“筛选串”重置选中项的索引
		public void Filter(string startType = "", Predicate<TypeItem> select = null)
		{
            //读取组合框的被选中项的Tag对象、清空组合框的内部列表
			object selected = mainCombo.SelectedItem;
			mainCombo.Items.Clear();
			int index = 0;

            //根据传入的选择函数决定是否对元素进行投射变换，将“类型项”添加到组合框的内部列表
            //若select为null，只是将相关“类型项”添加到组合框的内部列表
            //若select非null，则要选择使得select(t)为true的“类型项”
			foreach (var t in (select == null ? items : items.Where(t => select(t))))
			{
                //若“类型项”中包含筛选串，则置index为组合框内部列表的项数（即含筛选串的索引，因为随后该项加入组合框列表）
				if (startType != "" && t.Type.Name.Contains(startType)) index = mainCombo.Items.Count;
                //添加“类型项”到组合框的“内部列表”
				mainCombo.Items.Add(t);
			}

            //更新“选中索引”or“选中项”
			var i = mainCombo.SelectedIndex;
			if (startType == "" && selected != null && mainCombo.Items.Contains(selected))
				mainCombo.SelectedItem = selected;
			else
				mainCombo.SelectedIndex = index;
			if (i == mainCombo.SelectedIndex)
			{
				foreach (var po in ControlList)
					po.Refresh();
			}
		}

        //选择值变动的事件处理程序
		void mainCombo_SelectedValueChanged(GucControl sender)
		{
			if (mainCombo.SelectedIndex == -1) return;

            //清除当前“参数项”列表的设置
			foreach (var po in ControlList)
			{
				po.ReArrange -= ArrangeItems;
				po.Dispose();
				Page.Controls.Remove(po.name);
				Page.Controls.Remove(po.error);
				Page.Controls.Remove(po.field);
			}
			ControlList.Clear();

            //由“选中项”创建istance，获取“该类型”的（属性,特性）数组
			instance = (mainCombo.SelectedItem as TypeItem).Item as IParameter;
			var cur_props = (mainCombo.SelectedItem as TypeItem).Type.GetParameterAttributes();

            //根据“类型”的（属性,特性）数组创建参数项，并添加到参数项列表，包含的控件绑定到Page内的控件列表
			foreach (var para in cur_props)
			{
				var po = ParameterOption.Create(para.Item1, para.Item2, instance);
				Page.Controls.Add(po.name);
				po.name.X = lblPara.X;
				Page.Controls.Add(po.error);
				po.error.X = lblPara.X;
				Page.Controls.Add(po.field);
				po.field.X = mainCombo.X;
				po.field.Width = mainCombo.Width;
				ControlList.Add(po);
				po.ReArrange += ArrangeItems;
			}
            //上面控件的位置只是简单堆叠，此处将其重新排列
			ArrangeItems();
            //发布类型改变事件
			if (TypeChanged != null) TypeChanged(this);
		}


        /// <summary>
        /// 获取instance，对象的初始参数由InitializeParameter()方法设置
        /// </summary>
        /// <returns></returns>
		public IParameter GetTypeInstance()
		{
			if (mainCombo.SelectedIndex == -1) return null;
			//Check Parameter
			foreach (var ctrl in ControlList)
			{
				if (ctrl.hasError) return null;
			}
			//return instance
			instance.InitializeParameter();
			return instance;
		}

        //安排各项的位置
		public void ArrangeItems()
		{
			int top = lblPara.Bottom + 10;
			if (ControlList.Count == 0)
			{
				lblPara.Visible = false;
				top = lblPara.Y;
			}
			else
			{
				lblPara.Visible = true;
				foreach (var ctrl in ControlList)
				{
					GucLabel label = ctrl.name;
					GucControl field = ctrl.field;
					label.Y = top;
					field.Y = label.Y + (label.Height - field.Height) / 2;
					label = ctrl.error;
					if (label.Text == "")
					{
						label.Visible = false;
						top = field.Bottom + 5;
					}
					else
					{
						label.Visible = true;
						label.Y = field.Bottom;
						top = label.Bottom + 5;
					}
				}
			}
			Height = top;
		}

		public static IEnumerable<Type> FindDerivedTypesFromAssembly(Type baseType, bool classOnly = true) 
        { return FindDerivedTypesFromAssembly(Assembly.GetAssembly(baseType), baseType, classOnly); }

        //迭代器，获取“定义某类的程序集”所含“类型”（“实现指定接口的类”or“指定类的派生类”），可设置是否只返回class类型
		public static IEnumerable<Type> FindDerivedTypesFromAssembly(Assembly assembly, Type baseType, bool classOnly = true)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly", "Assembly must be defined");

			if (baseType == null)
				throw new ArgumentNullException("baseType", "Parent Type must be defined");

			// get all the types
			var types = assembly.GetTypes();

			// works out the derived types
			foreach (var type in types)
			{
				// if classOnly, it must be a class
				// useful when you want to create instance
				if (classOnly && !type.IsClass)
					continue;

                //返回包含该接口的类or该类的派生类
				if (baseType.IsInterface)
				{
					var it = type.GetInterface(baseType.FullName);

					if (it != null)
						// add it to result list
						yield return type;
				}
				else if (type.IsSubclassOf(baseType))
				{
					// add it to result list
					yield return type;
				}
			}
		}
	}

    /// <summary>
    /// 类型项：类型、项
    /// </summary>
	class TypeItem
	{
		public Type Type { get; private set; }

		public object Item { get; private set; }

		public TypeItem(Type type, object[] ctorParameter = null) { Type = type; CreateInstance(ctorParameter); }

		public override string ToString() { return this.Type.Name; }

		public void CreateInstance(object[] parameter = null) { Item = Type.GetConstructors()[0].Invoke(parameter); }
	}
}
