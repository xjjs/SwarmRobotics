using System;
using System.Reflection;
using GucUISystem;
using Microsoft.Xna.Framework;
using RobotLib;

namespace RobotDemo
{
    /// <summary>
    /// 参数项类，用于UI界面的参数设置；
    /// 每个参数项有3个控件和3个主要成员（instance、instance的某个属性、描述该属性的特性）；
    /// </summary>
	abstract class ParameterOption
	{
        //属性、特性、接口对象
		public PropertyInfo pi;
		public ParameterAttribute att;
		public IParameter instance;

        //标签控件（名称or错误）、基类控件（存储新创建的控件）、错误标识
		public GucLabel name, error;
		public GucControl field;
		public bool hasError { get; private set; }

        //设置属性、特性与接口对象（多态性）
		protected ParameterOption(PropertyInfo pi, ParameterAttribute att, IParameter instance)
		{
			this.pi = pi;
			this.att = att;
			this.instance = instance;
			name = new GucLabel();
			name.Text = att.Description;
			name.AutoSize = true;
			error = new GucLabel();
			error.Text = "";
			error.AutoSize = true;
			error.FontColor = Color.Red;
			hasError = false;
		}

		public abstract void Dispose();

        //刷新“控件绑定值”
		public abstract void Refresh();

        /// <summary>
        /// 设置值到代码主体
        /// </summary>
		protected void SetValue(GucControl sender)
		{
			try
			{
                //设置某对象的属性
				pi.SetValue(instance, GetControlValue(), BindingFlags.InvokeMethod | BindingFlags.SetProperty, null, null, null);
				name.FontColor = Color.Black;
				error.Text = "";
				hasError = false;
			}
			catch (Exception e)
			{
				if (e is TargetInvocationException)
					error.Text = e.InnerException.Message;
				else if (e is FormatException || e is OverflowException)
					error.Text = "Input must be " + att.Type.ToString().ToLower() + " value";
				else
					throw;
				name.FontColor = Color.Red;
				hasError = true;
			}
            //发布重组织事件
			if (ReArrange != null) ReArrange();
		}

		protected abstract object GetControlValue();

        //根据“参数类型”创建不同类型的“参数选项”对象
		public static ParameterOption Create(PropertyInfo pi, ParameterAttribute att, IParameter instance)
		{
			switch (att.Type)
			{
				case ParameterType.Boolean:
					return new ParameterOptionBoolean(pi, att, instance);
				case ParameterType.String:
					return new ParameterOptionString(pi, att, instance);
				case ParameterType.Float:
					return new ParameterOptionFloat(pi, att, instance);
				case ParameterType.Int:
					return new ParameterOptionInt(pi, att, instance);
				case ParameterType.Array:
					return new ParameterOptionArray(pi, att, instance);
				//case ParameterType.Enum:
				default:
					return new ParameterOptionEnum(pi, att, instance);
			}
		}

        //重布置
		public event Action ReArrange;
	}

    /// <summary>
    /// 布尔型参数选项：复选框、绑定到instance、注册“设置事件”处理函数
    /// </summary>
	class ParameterOptionBoolean : ParameterOption
	{
		GucCheckBox cb;

		public ParameterOptionBoolean(PropertyInfo pi, ParameterAttribute att, IParameter instance)
			: base(pi, att, instance)
		{
			cb = new GucCheckBox();
			cb.Checked = (bool)pi.GetValue(instance, null);
			cb.CheckedChanged += new GucEventHandler(SetValue);
			cb.Text = "";
			field = cb;
		}
        //清除事件处理函数、刷新属性值、获取属性值
		public override void Dispose() { cb.CheckedChanged -= SetValue; }
		public override void Refresh() { cb.Checked = (bool)pi.GetValue(instance, null); }
		protected override object GetControlValue() { return cb.Checked; }
	}

    /// <summary>
    /// 枚举型参数选项：组合框（添加属性的所有列表项）、绑定到instance、注册“设置事件”处理函数
    /// </summary>
	class ParameterOptionEnum : ParameterOption
	{
		GucComboBox cmb;

		public ParameterOptionEnum(PropertyInfo pi, ParameterAttribute att, IParameter instance)
			: base(pi, att, instance)
		{
			cmb = new GucComboBox();
			foreach (var item in Enum.GetValues(pi.PropertyType))
				cmb.Items.Add(item);
			cmb.SelectedItem = pi.GetValue(instance, null);
			cmb.SelectedChanged += new GucEventHandler(SetValue);
			//cmb.DropDownStyle = ComboBoxStyle.DropDownList;
			//cmb.FlatStyle = FlatStyle.Popup;
			field = cmb;
		}
        //清除事件处理函数、刷新被选项、获取被选项
		public override void Dispose() { cmb.SelectedChanged -= SetValue; }
		public override void Refresh() { cmb.SelectedItem = pi.GetValue(instance, null); }
		protected override object GetControlValue() { return cmb.SelectedItem; }
	}

    /// <summary>
    /// 字符串型参数选项：文本框、绑定到instance、注册“设置事件”处理函数
    /// </summary>
	class ParameterOptionString : ParameterOption
	{
		GucTextBox tb;

		public ParameterOptionString(PropertyInfo pi, ParameterAttribute att, IParameter instance)
			: base(pi, att, instance)
		{
			tb = new GucTextBox();
			tb.Text = pi.GetValue(instance, null).ToString();
			tb.DeActivated += new GucEventHandler(SetValue);
			field = tb;
		}
        //清除事件处理函数、刷新被选项、获取被选项
		public override void Dispose() { tb.DeActivated -= SetValue; }
		public override void Refresh() { tb.Text = pi.GetValue(instance, null).ToString(); }
		protected override object GetControlValue() { return tb.Text; }
	}

    /// <summary>
    /// 整型参数选项：文本框、绑定到instance、注册“设置事件”处理函数
    /// </summary>
	class ParameterOptionInt : ParameterOption
	{
		GucTextBox tb;

		public ParameterOptionInt(PropertyInfo pi, ParameterAttribute att, IParameter instance)
			: base(pi, att, instance)
		{
			tb = new GucTextBox();
			tb.Text = pi.GetValue(instance, null).ToString();
			tb.DeActivated += new GucEventHandler(SetValue);
			field = tb;
		}

        //清除事件处理函数、刷新被选项、获取被选项
        public override void Dispose() { tb.DeActivated -= SetValue; }
		public override void Refresh() { tb.Text = pi.GetValue(instance, null).ToString(); }
		protected override object GetControlValue() { return int.Parse(tb.Text); }
	}

    /// <summary>
    /// 浮点型参数选项：文本框、绑定到instance、注册“设置事件”处理函数
    /// </summary>
	class ParameterOptionFloat : ParameterOption
	{
		GucTextBox tb;

		public ParameterOptionFloat(PropertyInfo pi, ParameterAttribute att, IParameter instance)
			: base(pi, att, instance)
		{
			tb = new GucTextBox();
			tb.Text = pi.GetValue(instance, null).ToString();
			tb.DeActivated += new GucEventHandler(SetValue);
			field = tb;
		}
        //清除事件处理函数、刷新被选项、获取被选项
		public override void Dispose() { tb.DeActivated -= SetValue; }
		public override void Refresh() { tb.Text = pi.GetValue(instance, null).ToString(); }
		protected override object GetControlValue() { return float.Parse(tb.Text); }
	}

    /// <summary>
    /// 列表型参数选项：组合框（所有列表项）、绑定到instance、注册“设置事件”处理函数
    /// </summary>
	class ParameterOptionArray : ParameterOption
	{
		GucComboBox cmb;

		public ParameterOptionArray(PropertyInfo pi, ParameterAttribute att, IParameter instance)
			: base(pi, att, instance)
		{
			cmb = new GucComboBox();
            //搜索pi所在的类（一个Type对象），获取与“列表名”同名的方法
			var mi = pi.DeclaringType.GetMethod(att.ValueArrayName);
            //若特性中的“方法名”为空则直接返回“同名方法”获取的列表项，否则对“同名方法”获取的列表项用指定方法处理
			if (att.StringFuncName == null)
			{
				foreach (var item in (Array)mi.Invoke(instance, null))
					cmb.Items.Add(item);
			}
			else
			{
				var sfunc = pi.DeclaringType.GetMethod(att.StringFuncName);
				foreach (var item in (Array)mi.Invoke(instance, null))
					cmb.Items.Add(item, (string)sfunc.Invoke(instance, new object[] { item }));
			}
			cmb.SelectedItem = pi.GetValue(instance, null);
			cmb.SelectedChanged += new GucEventHandler(SetValue);
			field = cmb;
		}
        //清除事件处理函数、刷新被选项、获取被选项
		public override void Dispose() { cmb.SelectedChanged -= SetValue; }
		public override void Refresh() { cmb.SelectedItem = pi.GetValue(instance, null); }
		protected override object GetControlValue() { return cmb.SelectedItem; }
	}
}