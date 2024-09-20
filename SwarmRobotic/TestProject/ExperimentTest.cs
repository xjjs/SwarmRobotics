using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;
using RobotLib.Environment;

namespace TestProject
{
    /// <summary>
    /// 实验测试对象：用于设置实验对象的各项属性参数（环境、问题、与算法），实现“参数范围列表”接口；
    /// 主数据：范围参数列表、值参数字典；
    /// 主功能：由参数范围列表生成“实验对象”；
    /// </summary>
	class ExperimentTest : ITestParamList<Experiment>
	{
        //声明对象：实验、环境、问题、算法
		Experiment exp;
		RoboticAlgorithm ra;
		RoboticEnvironment re;
		RoboticProblem rp;
   
        //类型对象：描述的是类型信息
		Type tp = null, ta = null;

        //主数据：范围参数列表（用以描述环境、问题or算法）、值参数字典（一种更为简洁的描述方式）
		List<Para> pararanges;
		Dictionary<string, object> values;

        //设置问题类型的构造函数：范围参数列表、值参数字典
		public ExperimentTest(Type ProblemType)
		{
			pararanges = new List<Para>();
			values = new Dictionary<string, object>();
			this.ProblemType = ProblemType;
		}

        //设置问题类型与算法类型的构造函数
		public ExperimentTest(Type ProblemType, Type AlgorithmType) : this(ProblemType) 
        { this.AlgorithmType = AlgorithmType; }

        //问题类型属性：确保是RoboticProblem的派生类
		public Type ProblemType
		{
			get { return tp; }
			set
			{
				if (value.IsSubclassOf(typeof(RoboticProblem)))
				{
					tp = value;
					pararanges.Clear();
				}
				else
					throw new Exception();
			}
		}

        //算法类型属性：确保是RoboticAlgorithm的派生类
		public Type AlgorithmType
		{
			get { return ta; }
			set
			{
				if (value.IsSubclassOf(typeof(RoboticAlgorithm)))
				{
					ta = value;
					pararanges.RemoveAll(p => p.isProblem == false);
					Name = ta.Name;
					return;
				}
				throw new Exception();
			}
		}

        //算法名称（参数集对象的名称为算法）
		public string Name { get; set; }

        //添加范围参数（名与范围）
		public bool SetPara(bool? isProblem, string name, ITestParamRange range)
		{
			if (isProblem != null)
			{
				if (isProblem.Value)
				{
					if (tp == null) return false;
					//if (!rp.CurrentParameterList.ContainsParameter(name)) return false;
				}
				else
				{
					if (ta == null) return false;
					//if (!ra.CurrentParameterList.ContainsParameter(name)) return false;
				}
			}
			Para p = new Para(name, range, isProblem);
			pararanges.Add(p);
			return true;
		}

        //添加范围参数（参数范围矩阵与名称列表）
		public bool SetPara(bool? isProblem, ITestParamMultiRange range, params string[] names)
		{
			if (isProblem != null)
			{
				if (isProblem.Value)
				{
					if (tp == null) return false;
					//if (!rp.CurrentParameterList.ContainsParameter(name)) return false;
				}
				else
				{
					if (ta == null) return false;
					//if (!ra.CurrentParameterList.ContainsParameter(name)) return false;
				}
			}
			Para p = new Para(range, isProblem, names);
			pararanges.Add(p);
			return true;
		}

        //移除参数描述对象
		public bool RemovePara(string name) { return pararanges.RemoveAll(p => p.Name.Contains(name)) > 0; }

        //字典中值参数
		public bool SetValue(bool? isProblem, string name, object value)
		{
			if (isProblem == null)
			{
				name = "e" + name;
			}
			else
			{
				if (isProblem.Value)
				{
					if (tp == null) return false;
					//if (!rp.CurrentParameterList.ContainsParameter(name)) return false;
					name = "p" + name;
				}
				else
				{
					if (ta == null) return false;
					//if (!ra.CurrentParameterList.ContainsParameter(name)) return false;
					name = "a" + name;
				}
			}

			values.Add(name, value);
			return true;
		}

        //字典中移除某参数
		public bool RemoveValue(bool? isProblem, string name)
		{
			if (isProblem == null)
				name = "e" + name;
			else if (isProblem.Value)
				name = "p" + name;
			else
				name = "a" + name;
			return values.Remove(name);
		}

        //由范围参数对象列表，生成范围列表（ToArray虽然是数据缓存，但存储的仍是对象的引用，可操纵原始数据）
		public ITestParamRange[] GenerateParas() { return pararanges.Select(p => p.Range).ToArray(); }

        //由范围列表构造实验对象
		public Experiment GetParam(params ITestParamRange[] paras)
		{
            //传入的范围参数列表paras未使用，因为其存储的引用只是pararanges的一部分，相应的Current可直接从pararanges中读取
			if (tp == null || ta == null || paras.Length != pararanges.Count) throw new Exception();
			try
			{
                //用问题类的“默认构造函数”创建问题对象，设置属性，初始化参数
				rp = (RoboticProblem)tp.GetConstructor(Type.EmptyTypes).Invoke(null);
                //遍历问题参数描述对象列表，以设置问题对象的参数
				foreach (var item in pararanges.Where(item => item.isProblem == true))
				{
                    //设置问题对象的相应属性的值
					if (item.isMulti)
						for (int i = 0; i < item.Name.Length; i++)
							rp.SetValue(item.Name[i], (item.Range as ITestParamMultiRange)[i]);
					else
						rp.SetValue(item.Name[0], item.Range.Current);
				}
                //遍历值参数字典中的问题参数，并设置问题对象相应属性的值
				foreach (var item in values.Where(item => item.Key.StartsWith("p")))
					rp.SetValue(item.Key.Substring(1), item.Value);
				rp.InitializeParameter();



                //由算法类型创建算法对象，设置属性，初始化参数
				ra = (RoboticAlgorithm)ta.GetConstructor(Type.EmptyTypes).Invoke(null);
                //绑定到问题后，遍历问题参数描述对象列表，以设置算法对象的参数
				ra.Bind(rp, true);
				foreach (var item in pararanges.Where(item => item.isProblem == false))
				{
					if (item.isMulti)
						for (int i = 0; i < item.Name.Length; i++)
							ra.SetValue(item.Name[i], (item.Range as ITestParamMultiRange)[i]);
					else
						ra.SetValue(item.Name[0], item.Range.Current);
				}
                //遍历字典中的算法参数，并设置算法对象相应属性的值
				foreach (var item in values.Where(item => item.Key.StartsWith("a")))
					ra.SetValue(item.Key.Substring(1), item.Value);
				ra.InitializeParameter();



                //创建KD树定义的环境对象，设置属性，初始化参数
				re = new EKDTree();
				foreach (var item in pararanges.Where(item => item.isProblem == null))
				{
					if (item.isMulti)
						for (int i = 0; i < item.Name.Length; i++)
							re.SetValue(item.Name[i], (item.Range as ITestParamMultiRange)[i]);
					else
						re.SetValue(item.Name[0], item.Range.Current);
				}
				foreach (var item in values.Where(item => item.Key.StartsWith("e")))
					re.SetValue(item.Key.Substring(1), item.Value);
				re.InitializeParameter();

                //创建并返回实验对象
				exp = new Experiment(re, ra, rp);
				return exp;
			}
			catch
			{
				return null;
			}
		}

        //描述对象列表的元素计数
		public int Count { get { return pararanges.Count; } }

        //返回“嵌套类”的名称串所合成的串
		public string Title { get { return pararanges.Aggregate("", (str, p) => str + p.Title); } }

        //范围参数嵌套类（单个参数or参数矩阵）：参数范围、名称列表
		class Para
		{
			public string[] Name;
			public ITestParamRange Range;
			public bool? isProblem;
			public bool isMulti;
            //标题为名称串（必以逗号“，”结尾）
            public string Title { get; private set; }
            //显示参数描述的对象：是环境、问题or算法
            public override string ToString() 
            { return (isProblem == null ? "E" : (isProblem.Value ? "P" : "A")) + "_" + Title; }

            //用参数名与参数范围构造对象
			public Para(string name, ITestParamRange range, bool? isProblem)
			{
				Name = new string[] { name };
				Range = range;
				this.isProblem = isProblem;
				isMulti = false;
				Title = name + ",";
			}

            //用参数名称列表与范围矩阵构造对象
			public Para(ITestParamMultiRange range, bool? isProblem, params string[] names)
			{
                //矩阵的列数不能大于名字列表的元素个数
				if (range.Dimension < names.Length) throw new ArgumentException();
				Name = names;
				Range = range;
				this.isProblem = isProblem;
				isMulti = true;
                //对序列应用累加器，参数1为种子，参数2为累加函数
				Title = names.Aggregate("", (str, n) => str + n + ",");
			}

		}
	}
}
