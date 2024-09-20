using System;
using UtilityProject.PSO;

namespace UtilityProject.Funcs
{
    /// <summary>
    /// 评估函数
    /// </summary>
    /// <typeparam name="ValueType"></typeparam>
	public abstract class EvaluateFunction<ValueType>
		where ValueType : IComparable<ValueType>
	{
		protected EvaluateFunction() { }

        //评估浮点向量
		public abstract ValueType Evaluate(double[] x);
        //评估PSO的粒子列表，尚未实现
		public virtual void Evaluate(Particle<ValueType>[] particles) { throw new NotImplementedException(); }
		public abstract ComponentRange GetRange(int Index);
		public abstract bool CheckDimention(int Dimension);
	}

    //着重对维度进行了限制与说明
	public abstract class FixDimensionEvaluateFunction<ValueType> : EvaluateFunction<ValueType>
		where ValueType : IComparable<ValueType>
	{
		protected FixDimensionEvaluateFunction(int Dim) { Dimension = Dim; }

		public int Dimension { get; private set; }

		public override bool CheckDimention(int Dimension) { return this.Dimension == Dimension; } 
	}

    //元素取值范围
	public class ComponentRange
	{
		public ComponentRange(double LBound, double UBound)
		{
			this.LBound = this.InitLBound = LBound;
			this.UBound = UBound;
			InitSize = UBound - LBound;
		}

		public ComponentRange(double LBound, double UBound, double InitLBound, double InitSize)
		{
			this.LBound = LBound;
			this.UBound = UBound;
			this.InitLBound = InitLBound;
			this.InitSize = InitSize;
		}

		public double LBound, UBound, InitLBound, InitSize;
	}
}