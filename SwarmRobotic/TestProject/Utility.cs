using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//工程测试的相关数据结构
namespace TestProject
{
    /// <summary>
    /// 参数范围：最小、最大、计数，初始化、当前对象、单步移动
    /// </summary>
	interface ITestParamRange
	{
		object Current { get; }
		bool Step();
		void Init();
		void Default();
		double Min { get; }
		double Max { get; }
		void Set(double val);
		object Preview(double val);
		int Count { get; }
	}

    /// <summary>
    /// 参数范围列表：标题、计数，生成“参数范围”列表、获取“参数范围”列表的参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface ITestParamList<T>
    {
		int Count { get; }

        ITestParamRange[] GenerateParas();

        //object[] GetIndex(params ITestParamRange[] paras);

        //由范围列表获取泛型对象
        T GetParam(params ITestParamRange[] paras);

		string Title { get; }
    }

    /// <summary>
    /// 继承参数范围，采用Int类型；主要字段：当前整数、默认整数、步长；主要属性：最小、最大、区间步数
    /// </summary>
    class IntTestRange : ITestParamRange {
        protected int step, cur, @default;

        public IntTestRange(int Min, int Max, int step = 1) : this(Min, Max, Min, step) { }

        public IntTestRange(int Min, int Max, int @default, int step) {
            this.Min = Min;
            this.Max = Max + 1;
            this.step = step;
            this.@default = @default;
            this.Count = (Max - Min) / step + 1;
            Init();
        }

        public virtual object Current { get { return cur; } }

        //注意这里的设置：成功时返回false（与GirdTest处迭代器的定义是一致的）
        public virtual bool Step() {
            cur += step;
            if (cur >= Max) return true;
            return false;
        }

        public virtual void Init() { cur = (int)Min; }

        public virtual void Default() { cur = @default; }

        public double Min { get; protected set; }

        public double Max { get; protected set; }

        //没有考虑超出边界赋值的情形
        public virtual void Set(double val) {
            cur = (int)val;
            if (cur == Max) cur--;
        }

        //只是显示一下数值
        public virtual object Preview(double val) {
            var r = (int)val; if (r == Max) r--;
            return r;
        }

        public int Count { get; private set; }

        public override string ToString() { return string.Format("Cur={0}, Min={1}, Max={2}", Current, Min, Max); }
    }

    /// <summary>
    /// 继承Int型参数范围，采用Float类型的参数value=cur/@base表示（cur由int转为float型、@base为int型）
    /// </summary>
	sealed class FloatTestRange : IntTestRange
	{
		int @base;
		float value;

		public FloatTestRange(int Min, int Max, int step, int @base) : this(Min, Max, step, Min, @base) { }

		public FloatTestRange(int Min, int Max, int step, int @default, int @base) : base(Min, Max, @default, step) 
        { 
            this.@base = @base;
            this.value = (float)cur / @base;
        }

		public override object Current { get { return value; } }

		public override bool Step()
		{
			var next = base.Step();
			value = (float)cur / @base;
			return next;
		}

        //以下语句会使得value初值无穷大
        public override void Init() {
            base.Init();
            value = (float)cur / @base;
        }

		public override void Default()
		{
			base.Default();
			value = (float)cur / @base;
		}

		public override void Set(double val)
		{
			base.Set(val);
			value = (float)val / @base;
		}

		public override object Preview(double val) { return (float)base.Preview(val) / @base; }
	}


    /// <summary>
    /// 继承Int型参数范围，定义了object列表，cur字段充当了列表索引
    /// </summary>
	sealed class ArrayRange : IntTestRange
    {
        object[] list;

		public ArrayRange(params object[] list) : base(0, list.Length - 1, 0, 1) { this.list = list; }//: this(0, (object[])list) { }

		//public ArrayRange(int defaultind, params object[] list) : base(0, list.Length - 1, defaultind, 1) { this.list = list; }

		public override object Current { get { return list[(int)base.Current]; } }

		public override object Preview(double val) { return list[(int)base.Preview(val)]; }

		public override string ToString() { return string.Format("Cur={0}, Min={1}, Max={2}", Current, Min, Max); }
	}

    //继承Int型参数范围，定义了父结点以实现相关性参数范围的基类，Object型的Current由函数返回
    class RelevantSerialRange : IntTestRange {
        protected ITestParamRange parent;
        //具有两个入参，返回值类型为object
        Func<object, int, object> func;

        public RelevantSerialRange(ITestParamRange parent, int count, Func<object, int, object> calcfunc)
            : base(0, count - 1) {
            this.parent = parent;
            func = calcfunc;
        }

        public override object Current { get { return func(parent.Current, (int)base.Current); } }

        public override object Preview(double val) { return func(parent.Current, (int)base.Preview(val)); }

        public override string ToString() { return string.Format("Cur={0}, RelVal={1}, Index={2}/{3}", Current, parent.Current, base.Current, Max - 1); }
    }
    /// <summary>
    /// 继承相关性参数范围的基类，
    /// </summary>
	sealed class RelevantRange : RelevantSerialRange
	{
        //具有一个入参，返回值类型为object
		Func<object, object> func;

		public RelevantRange(ITestParamRange parent, Func<object, object> calcfunc) : base(parent, 1, null) { func = calcfunc; }

		public override object Current { get { return func(parent.Current); } }

		//public override bool Step() { return true; }

		public override string ToString() { return string.Format("Cur={0}, RelVal={1}", Current, parent.Current); }
	}

    //继承参数范围基类，该范围的作用：将一个参数的不同的取值范围组合起来（相当于一个数组，Step方法则是依次取数组的元素）
    sealed class CombineRange : ITestParamRange {
        ITestParamRange[] ranges;
        double[] start, offset;
        int index, @default;

        public CombineRange(int @default, params ITestParamRange[] ranges) {
            this.ranges = ranges;
            this.@default = @default;
            start = new double[ranges.Length];
            offset = new double[ranges.Length];
            Max = 0;
            Count = 0;
            //将各范围的元素罗列为一个数组，则设置各范围在数组中的起始索引与缺省距离（该范围最小值与上范围最大值）
            for (int i = 0; i < ranges.Length; i++)
            {
                start[i] = Max;
                offset[i] = ranges[i].Min - Max;
                Max += ranges[i].Count;
                Count += ranges[i].Count;
            }
            Init();
        }

        public object Current { get { return ranges[index].Current; } }

        public bool Step() {
            if (ranges[index].Step())
            {
                index++;
                if (index == ranges.Length) return true;
                ranges[index].Init();
            }
            return false;
        }

        public void Init() {
            index = 0;
            ranges[0].Init();
        }

        public void Default() {
            index = @default;
            ranges[@default].Default();
        }

        public double Min { get { return 0; } }

        public double Max { get; private set; }

        public void Set(double val) {
            index = Array.BinarySearch(start, val);
            if (index < 0) index = (~index) - 1;
            ranges[index].Set(val + offset[index]);
        }

        public object Preview(double val) {
            int index = Array.BinarySearch(start, val);
            if (index < 0) index = (~index) - 1;
            return ranges[index].Preview(val + offset[index]);
        }

        public int Count { get; private set; }

        public override string ToString() { return string.Format("Cur={0}, Min={1}, Max={2}, Index={3}", Current, Min, Max, index); }
    }

    /// <summary>
    /// 继承参数范围基类的接口，成员有索引运算符、维度
    /// </summary>
	interface ITestParamMultiRange : ITestParamRange
	{
		object this[int index] { get; }
		int Dimension { get; }
	}
    /// <summary>
    /// 继承Int型参数范围，定义2维数组以实现矩阵参数范围，其中curArray为当前行；
    /// 单个Range描述“单个数”的范围变化
    /// MultiRange描述“多个数”的范围变化，多个数取值组成一行，每行都是各参数的一种取值组合
    /// </summary>
	sealed class MatrixMultiRange : IntTestRange, ITestParamMultiRange
	{
		object[,] list;
		object[] curArray;

		public MatrixMultiRange(object[,] list, int defaultind = 0)
			: base(0, list.GetLength(0) - 1, defaultind, 1)
		{
			this.list = list;

            //获取维度时，编号从0开始，故此处Dimension（取值为1）是获取列表的列数
			Dimension = list.GetLength(1);
			curArray = new object[Dimension];
			Init();
		}

		public override void Init()
		{
			if (curArray == null) return;
			base.Init();
			for (int i = 0; i < Dimension; i++)
				curArray[i] = list[(int)base.Current, i];
		}

		public override bool Step()
		{
			if (base.Step()) return true;
			for (int i = 0; i < Dimension; i++)
				curArray[i] = list[(int)base.Current, i];
			return false;
		}

        //Current返回的是列表引用（base.Current为整型的cur）
		public override object Current { get { return curArray; } }

		public override object Preview(double val) { return null; }

		public override string ToString() 
        { return string.Format("CurIndex={0}, Min={1}, Max={2}", base.Current, Min, Max); }

        //获取当前行的第index列（base.Current为整型）
		public object this[int index] { get { return list[(int)base.Current, index]; } }

		public int Dimension { get; private set; }
	}
}
