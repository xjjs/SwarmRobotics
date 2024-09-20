using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RobotLib
{
    //通知编译器，该类可以序列化为容易传输的格式（将对象的类名、字段等转换为字节流）
    //序列化的作用：将对象的状态保存在存储媒体中以便以后可创建完全相同的副本；按值将对象从一个应用程序域发送至另一个应用程序域；
	/// <summary>
	/// 迭代次数、存活个体数、机器人碰撞次数、障碍物碰撞次数、运行时间
    /// 属性：是否成功、是否完成、是否解析终结、状态信息(元数据列表)、标题
	/// </summary>
    [Serializable]
	public class RunState : IComparable<RunState>
	{
        //公共字段，添加的Iter2的是迭代次数的平方
		public int Iterations, AliveRobots, RoboCollison, ObsCollison, SingleNum;
        public float Iter2, SD;
		public long Time;
        //采集数据使用的字段
        public int IterationNum;
        public int RobotID;

        //批处理用的字段
        public bool BatchFlag { get; set;}
        public string Command {get; set;}

        //公共属性：成功、结束、终结状态
		public virtual bool Success { get; set; }
		public virtual bool Finished { get; set; }
		public bool Finalized { get; set; }
        public StateInfo Info { get; private set; }
        public string Title { get; private set; }

        //获取信息类型、字段名称串、清零所有字段值与状态属性
        public RunState() {
            Info = GetInfo();
            Title = Info.title;
            this.Clear();
        }

        //加上另一个状态变量，以更新自身所有字段
		public void Add(RunState other)
		{
			for (int i = 0; i < Info.FieldCount; i++)
			{
				var f = Info.fields[i];
				f.SetValue(this, Info.AddFuncs[i](f.GetValue(this), f.GetValue(other)));
			}
		}
        //清零所有字段的值与状态属性
		public void Clear()
		{
			for (int i = 0; i < Info.FieldCount; i++)
				Info.fields[i].SetValue(this, Info.zeros[i]);
			Finalized = Success = Finished = false;
		}

		/// <summary>
		/// Create a copied clone with all result fields as well as properties.
		/// TODO: Mark properties with NotCopyAttribute if do not want it to copy.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public RunState ResultClone()
		{
            //调用构造器以创建新对象，并拷贝字段值与属性值
			var result = Info.constructor.Invoke(null) as RunState;
			foreach (var f in Info.fields)
				f.SetValue(result, f.GetValue(this));
			foreach (var p in Info.properties)
				p.SetValue(result, p.GetValue(this, null), null);

            //这里应该没有必要再次进行属性设置了吧？
			result.Finished = Finished;
			result.Success = Success;
			result.Finalized = Finalized;
			return result;
		}

        //用字符串result，记录类型中所有字段的值
        public override string ToString() { return ToString(1); }

        //依然返回所有字段的值，只是字段要除以传入参数值；divide就是csv文件中最后一个字段Count的值；
		public string ToString(int divide)
		{
			int div = divide == 0 ? 1 : divide;
			string result = "";
			for (int i = 0; i < Info.FieldCount; i++)
				result += Info.DivideFuncs[i](Info.fields[i].GetValue(this), div) + ",";
			result += divide + ",";
			return result;
		}

        //获取当前类型的类型信息，字典中有则从中获取，没有则添加入字典
		StateInfo GetInfo()
		{
			var type = GetType();
			if (dictionary.ContainsKey(type))
				return dictionary[type];
			else
			{
				var info = new StateInfo(type);
				dictionary.Add(type, info);
				return info;
			}
		}
        //类的静态方法，获取当前类型（RunState及其派生类）的字段名称串，字典中有则从中获取，没有则添加入字典
		public static string GetTitle(Type type)
		{
			if (dictionary.ContainsKey(type))
				return dictionary[type].title;
			else
			{
				var info = new StateInfo(type);
				dictionary.Add(type, info);
				return info.title;
			}
		}

        //静态字段，用以存储<类型,类型信息>的字典对象
		static Dictionary<Type, StateInfo> dictionary = new Dictionary<Type, StateInfo>();

        //作为基类中的一个虚方法，状态好坏用剩余个体数判定
		public virtual int CompareTo(RunState other) { return AliveRobots.CompareTo(other.AliveRobots); }
	}
    
    //状态信息类：构造器信息、字段信息列表、属性信息列表、字段个数（FieldCount）、字段名称串（title）
    //其他：根据字段的数据类型实现相应的加法与除法
	[Serializable]
    public class StateInfo
    {
        //标题存储所有字段的名称
        public string title;
        public FieldInfo[] fields;
		public PropertyInfo[] properties;
		public Func<object, object, object>[] AddFuncs;
		public Func<object, int, object>[] DivideFuncs;
		public object[] zeros;
        public int FieldCount;
        public ConstructorInfo constructor;

        public StateInfo(Type type)
        {
            //获取属性信息列表（去除属性Info与Title）
			properties = type.GetProperties().Where(p => p.Name != "Info" && p.Name != "Title").ToArray();
            //获取字段信息列表
            fields = type.GetFields().ToArray();
            //获取字段个数
            FieldCount = fields.Length;

            AddFuncs = new Func<object, object, object>[FieldCount];
			DivideFuncs = new Func<object, int, object>[FieldCount];
            zeros = new object[FieldCount];
            title = "";
            //对每个字段，登记名字，由字段类型在函数列表中登记加法与除法
            for (int i = 0; i < FieldCount; i++)
            {
                var f = fields[i];
                title += f.Name + ",";
                if (f.FieldType == typeof(int))
                {
                    AddFuncs[i] = AddInt;
					DivideFuncs[i] = DivideInt;
                    zeros[i] = 0;
                }
                else if (f.FieldType == typeof(float))
                {
                    AddFuncs[i] = AddFloat;
					DivideFuncs[i] = DivideFloat;
					zeros[i] = 0f;
                }
				else if (f.FieldType == typeof(long))
				{
					AddFuncs[i] = AddLong;
					DivideFuncs[i] = DivideLong;
					zeros[i] = 0L;
				}
                else
                    throw new TypeAccessException("StateInfo Class Constructor");
            }
			title += "Count,";

            //获取构造器信息
            constructor = type.GetConstructor(Type.EmptyTypes);
        }

		static object AddInt(object v1, object v2) { return (int)v1 + (int)v2; }
		static object AddFloat(object v1, object v2) { return (float)v1 + (float)v2; }
		static object AddLong(object v1, object v2) { return (long)v1 + (long)v2; }

		static object DivideInt(object v, int divide) { return (int)v / (float)divide; }
		static object DivideFloat(object v, int divide) { return (float)v / divide; }
		static object DivideLong(object v, int divide) { return (float)(TimeSpan.FromTicks((long)v).TotalMilliseconds / divide); }

	}
}
