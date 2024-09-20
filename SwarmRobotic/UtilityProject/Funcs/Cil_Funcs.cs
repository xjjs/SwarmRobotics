using System;

namespace UtilityProject.Funcs
{
	public abstract class CILFunction : EvaluateFunction<double>
	{
		protected CILFunction(double Lower, double Upper, double XOrigin = 0, double FBias = 1)
		{
			if (XOrigin == 0)
				Origin = (Upper - Lower) / 10;
			else
				Origin = XOrigin;
			Range = new ComponentRange(Lower + Origin, Upper + Origin);
			Bias = FBias;
		}

		public double Origin { get; private set; }
		public double Bias { get; private set; }
		public ComponentRange Range { get; private set; }
		public override ComponentRange GetRange(int Index) { return Range; }
		public override bool CheckDimention(int Dimension) { return true; }

		public static EvaluateFunction<double>[] Functions { get; private set; }
		public static EvaluateFunction<double> GetFunction(int index) { return Functions[index - 1]; }
		static CILFunction() { Functions = new EvaluateFunction<double>[] { new CIL.F1(), new CIL.F2(), new CIL.F3(), new CIL.F4(), new CIL.F5(), new CIL.F6(), new CIL.F7(), new CIL.F8(), new CIL.F9(), new CIL.F10() }; }

		public override string ToString() { return GetType().Name; }
	}
}

namespace UtilityProject.Funcs.CIL
{
	/// <summary>
	/// Sphere function
	/// </summary>
	public sealed class F1 : CILFunction
	{
		internal F1() : base(-5.12, 5.12) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += xi * xi;
			}
			return r + Bias;
		}
	}

	/// <summary>
	/// Axis parallel hyper-ellipsoid function
	/// </summary>
	public sealed class F2 : CILFunction
	{
		internal F2() : base(-5.12, 5.12) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += xi * xi * i;
			}
			return r + Bias;
		}
	}

	/// <summary>
	/// Schwefel function 1.2
	/// </summary>
	public sealed class F3 : CILFunction
	{
		internal F3() : base(-65.536, 65.536) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, sum = 0;
			for (int i = 0; i < x.Length; i++)
			{
				sum += x[i] - Origin;
				r += sum * sum;
			}
			return r + Bias;
		}
	}

	/// <summary>
	/// Rosenbrock function
	/// </summary>
	public sealed class F4 : CILFunction
	{
		internal F4() : base(-2.048, 2.048) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi, tmp, last = x[0] - Origin;
			for (int i = 1; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				tmp = last * last - xi;
				last--;
				r += 100 * tmp * tmp + last * last;
				last = xi;
			}
			return r + Bias;
		}
	}

	/// <summary>
	/// Rastrigin function
	/// </summary>
	public sealed class F5 : CILFunction
	{
		internal F5() : base(-5.12, 5.12) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += xi * xi - 10 * Math.Cos(twoPI * xi) + 10;
			}
			return r + Bias;
		}

		private static double twoPI = Math.PI * 2;
	}

	/// <summary>
	/// Schwefel function
	/// </summary>
	public sealed class F6 : CILFunction
	{
		internal F6() : base(-500, 500) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += -xi * Math.Sin(Math.Sqrt(Math.Abs(xi)));
			}
			return r + Bias;
		}
	}

	/// <summary>
	/// Griewangk function
	/// </summary>
	public sealed class F7 : CILFunction
	{
		internal F7() : base(-600, 600) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi, sum = 1;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += xi * xi;
				sum *= Math.Cos(xi / Math.Sqrt(i + 1));
			}
			return r / 4000 - sum + 1 + Bias;
		}
	}

	/// <summary>
	/// Sum of different power functions
	/// </summary>
	public sealed class F8 : CILFunction
	{
		internal F8() : base(-1, 1) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += Math.Pow(Math.Abs(xi), i + 1);
			}
			return r + Bias;
		}
	}

	/// <summary>
	/// Ackley function
	/// </summary>
	public sealed class F9 : CILFunction
	{
		internal F9() : base(-32.768, 32.768) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi, tmp = 0;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				r += xi * xi;
				tmp += Math.Cos(twoPI * xi);
			}
			return 20 + Math.E - 20 * Math.Exp(-0.2 * Math.Sqrt(r / x.Length)) - Math.Exp(tmp / x.Length) + Bias;
		}

		private static double twoPI = Math.PI * 2;
	}

	/// <summary>
	/// Weierstrass function
	/// </summary>
	public sealed class F10 : CILFunction
	{
		internal F10() : base(-0.5, 0.5) { }

		public override double Evaluate(double[] x)
		{
			double r = 0, xi, sum = 0;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i] - Origin;
				for (int j = 0; j < 20; j++)
					r += Math.Pow(0.5, j) * Math.Cos(twoPI * Math.Pow(3, j) * (xi + 0.5));
			}
			for (int j = 0; j < 20; j++)
				sum += Math.Pow(0.5, j) * Math.Cos(Math.PI * Math.Pow(3, j));
			return r + sum * x.Length + Bias;
		}

		private static double twoPI = Math.PI * 2;
	}
}

