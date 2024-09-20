using System;
using System.Collections.Generic;
using System.IO;
using UtilityProject.Funcs.CEC05;

namespace UtilityProject.Funcs
{
	public enum CECDimension { D2 = 2, D10 = 10, D30 = 30, D50 = 50 }

	public abstract class CECFunction : FixDimensionEvaluateFunction<double>
	{
		static string folder = @"CEC_data\";

		static Dictionary<CECDimension, EvaluateFunction<double>[]> funcs;

		static CECFunction()
		{
			funcs = new Dictionary<CECDimension, EvaluateFunction<double>[]>();
			foreach (CECDimension dim in Enum.GetValues(typeof(CECDimension)))
				funcs.Add(dim, new EvaluateFunction<double>[] { new F1(dim), new F2(dim), new F3(dim), new F4(dim), new F5(dim), new F6(dim), new F7(dim), 
					new F8(dim), new F9(dim), new F10(dim), new F11(dim), new F12(dim), new F13(dim), new F14(dim), new F15(dim), new F16(dim),
					new F17(dim), new F18(dim), new F19(dim), new F20(dim), new F21(dim), new F22(dim), new F23(dim), new F24(dim), new F25(dim) });
		}

		public static EvaluateFunction<double>[] Functions(CECDimension dimension) { return funcs[dimension]; }

		protected CECFunction(double Lower, double Upper, CECDimension Dim, double FBias)
			: base((int)Dim)
		{
			Range = new ComponentRange(Lower, Upper);
			tran_x = new double[Dimension];
			Bias = FBias;
		}

		public ComponentRange Range { get; private set; }
		public override ComponentRange GetRange(int Index) { return Range; }

		protected void LoadArray(string filename, double[] array)
		{
			filename = folder + filename + ".txt";
			var nums = File.ReadAllLines(filename)[0].Split(split, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
				array[i] = double.Parse(nums[i]);
		}

		protected void LoadArray(string filename, double[,] array)
		{
			filename = folder + filename + ".txt";
			var lines = File.ReadAllLines(filename);
			for (int i = 0; i < Dimension; i++)
			{
				var nums = lines[i].Split(split, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < Dimension; j++)
					array[i, j] = double.Parse(nums[j]);
			}
		}

		protected void LoadArray(string filename, out double[,] array, int startline)
		{
			array = new double[Dimension, Dimension];
			filename = folder + filename + ".txt";
			var lines = File.ReadAllLines(filename);
			for (int i = 0; i < Dimension; i++)
			{
				var nums = lines[i + startline].Split(split, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < Dimension; j++)
					array[i, j] = double.Parse(nums[j]);
			}
		}

		protected double[] tran_x;
		public double Bias { get; private set; }
		protected static readonly char[] split = new char[] { ' ' };

		public override string ToString() { return GetType().Name; }
	}
}

namespace UtilityProject.Funcs.CEC05
{
	static class CECBaseFunction
	{
		public static double Ackley(double[] x)
		{
			double r = 0, xi, tmp = 0;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i];
				r += xi * xi;
				tmp += Math.Cos(twoPI * xi);
			}
			return 20 + Math.E - 20 * Math.Exp(-0.2 * Math.Sqrt(r / x.Length)) - Math.Exp(tmp / x.Length);
		}

		public static double Rastrigin(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i];
				r += xi * xi - 10 * Math.Cos(twoPI * xi) + 10;
			}
			return r;
		}

		public static double Elliptic(double[] x)
		{
			double r = 0;
			for (int i = 0; i < x.Length; i++)
				r += x[i] * x[i] * Math.Pow(1e6, i / (x.Length - 1.0));
			return r;
		}

		public static double Weierstrass(double[] x)
		{
			double r = 0, xi, sum = 0;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i];
				for (int j = 0; j <= 20; j++)
					r += Math.Pow(0.5, j) * Math.Cos(twoPI * Math.Pow(3, j) * (xi + 0.5));
			}
			for (int j = 0; j <= 20; j++)
				sum += Math.Pow(0.5, j) * Math.Cos(Math.PI * Math.Pow(3, j));
			return r - sum * x.Length;
		}

		public static double Griewank(double[] x)
		{
			double r = 0, xi, p = 1;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i];
				r += xi * xi;
				p *= Math.Cos(xi / Math.Sqrt(i + 1));
			}
			return r / 4000 - p + 1;
		}

		public static double Sphere(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = x[i];
				r += xi * xi;
			}
			return r;
		}

		public static double Schwefel(double[] x)
		{
			double r = 0, sum = 0;
			for (int i = 0; i < x.Length; i++)
			{
				sum += x[i];
				r += sum * sum;
			}
			return r;
		}

		public static double Rosenbrock(double[] x)
		{
			double r = 0, xi, tmp, last = x[0];
			for (int i = 1; i < x.Length; i++)
			{
				xi = x[i];
				tmp = last * last - xi;
				last--;
				r += 100 * tmp * tmp + last * last;
				last = xi;
			}
			return r;
		}

		public static double F8F2(double[] x)
		{
			double r = 0, tmp;
			int lastInd = x.Length - 1;
			for (int i = 0; i < lastInd; i++)
			{
				tmp = 100 * Math.Pow((x[i] * x[i] - x[i + 1]), 2) + Math.Pow((x[i] - 1), 2);
				r += (tmp * tmp) / 4000 - Math.Cos(tmp) + 1;
			}
			tmp = 100.0 * Math.Pow((x[lastInd] * x[lastInd] - x[0]), 2) + Math.Pow((x[lastInd] - 1), 2);
			r += (tmp * tmp) / 4000 - Math.Cos(tmp) + 1;
			return r;
		}

		public static double Schaffer(double[] x)
		{
			double res = 0;
			for (int i = 1; i < x.Length; i++)
				res += Schaffer(x[i], x[i - 1]);
			res += Schaffer(x[0], x[x.Length - 1]);
			return res;
		}

		public static double Schaffer_nc(double[] x)
		{
			double res = 0;
			for (int i = 1; i < x.Length; i++)
				res += Schaffer(RoundOff(x[i]), RoundOff(x[i - 1]));
			res += Schaffer(RoundOff(x[0]), RoundOff(x[x.Length - 1]));
			return res;
		}

		/// <summary>
		/// Round off x to Temp
		/// </summary>
		public static double RoundOff(double x, double origin = 0)
		{
			int a;
			double b, res;
			if (Math.Abs(x - origin) >= 0.5)
			{
				res = 2.0 * x;
				a = (int)res;
				b = Math.Abs(res - a);
				if (b < 0.5)
					return a / 2.0;
				else
				{
					if (res <= 0.0)
						return (a - 1.0) / 2.0;
					else
						return (a + 1.0) / 2.0;
				}
			}
			else
				return x;
		}

		public static double Schaffer(double x, double y)
		{
			//x=RoundOff(x);
			//y=RoundOff(y);
			double r1, r2;
			r2 = x * x + y * y;
			r1 = Math.Pow(Math.Sin(Math.Sqrt(r2)), 2) - 0.5;
			r2 = 1 + 0.001 * r2;
			return 0.5 + r1 / (r2 * r2);
		}

		public static double Rastrigin_nc(double[] x)
		{
			double r = 0, xi;
			for (int i = 0; i < x.Length; i++)
			{
				xi = RoundOff(x[i]);
				r += xi * xi - 10 * Math.Cos(twoPI * xi) + 10;
			}
			return r;
		}

		public static double RandomNormalDeviate()
		{
			//return 0;
			if (randflag)
			{
				rndx1 = Math.Sqrt(-2.0 * Math.Log(rand.NextDouble()));
				rndx2 = twoPI * rand.NextDouble();
				randflag = false;
				return (rndx1 * Math.Cos(rndx2));
			}
			else
			{
				randflag = true;
				return (rndx1 * Math.Sin(rndx2));
			}
		}

		private static double twoPI = Math.PI * 2;
		private static bool randflag = true;
		private static double rndx1, rndx2;
		private static Random rand = new Random();
	}

	public abstract class CECShiftFunction : CECFunction
	{
		protected CECShiftFunction(double Lower, double Upper, CECDimension Dim, double FBias)
			: base(Lower, Upper, Dim, FBias)
		{
			Origin = new double[Dimension];
		}

		protected virtual void Transform(double[] x)
		{
			for (int i = 0; i < Dimension; i++)
				tran_x[i] = x[i] - Origin[i];
		}

		protected double[] Origin;
	}

	public abstract class CECRotateFunction : CECShiftFunction
	{
		protected CECRotateFunction(double Lower, double Upper, CECDimension Dim, double FBias)
			: base(Lower, Upper, Dim, FBias)
		{
			temp = new double[Dimension];
			Rotation = new double[Dimension, Dimension];
			for (int i = 0; i < Dimension; i++)
				Rotation[i, i] = 1;
		}

		protected override void Transform(double[] x)
		{
			for (int i = 0; i < Dimension; i++)
				temp[i] = x[i] - Origin[i];
			for (int i = 0; i < Dimension; i++)
			{
				tran_x[i] = 0;
				for (int j = 0; j < Dimension; j++)
					tran_x[i] += Rotation[j, i] * temp[j];
			}
		}

		protected double[,] Rotation;
		double[] temp;
	}

	public abstract class CECHybridFunction : CECFunction
	{
		protected CECHybridFunction(double Lower, double Upper, CECDimension Dim, int nfunc, double FBias, params Func<double[], double>[] Funcs)
			: base(Lower, Upper, Dim, FBias)
		{
			FuncNum = nfunc;
			Lambda = new double[FuncNum];
			Origin = new double[FuncNum][];
			Rotation = new double[FuncNum][,];
			Weight = new double[FuncNum];
			Sigma = new double[FuncNum];
			FValue = new double[FuncNum];
			FNorm = new double[FuncNum];
			this.FBias = new double[FuncNum];
			for (int i = 0; i < FuncNum; i++)
			{
				Origin[i] = new double[Dimension];
				Rotation[i] = new double[Dimension, Dimension];
				for (int j = 0; j < Dimension; j++)
				{
					Rotation[i][j, j] = 1;
				}
				Weight[i] = 1.0 / FuncNum;
				Lambda[i] = Sigma[i] = 1;
				this.FBias[i] = 100 * i;
			}
			//Functions = new Func<double[], double>[FuncNum];
			//for (int i = 0; i < FuncNum; i++)
			//    Functions[i] = Funcs[i];
			hybridFunctions = Funcs;
			//if (Functions.Length != FuncNum) throw new Exception();

			temp = new double[Dimension];
		}

		protected void Transform(double[] x, int index)
		{
			for (int i = 0; i < Dimension; i++)
				temp[i] = (x[i] - Origin[index][i]) / Lambda[index];
			for (int i = 0; i < Dimension; i++)
			{
				tran_x[i] = 0;
				for (int j = 0; j < Dimension; j++)
					tran_x[i] += Rotation[index][j, i] * temp[j];
			}
		}

		protected void Transform(int index)
		{
			for (int i = 0; i < Dimension; i++)
				temp[i] = 5 / Lambda[index];
			for (int i = 0; i < Dimension; i++)
			{
				tran_x[i] = 0;
				for (int j = 0; j < Dimension; j++)
					tran_x[i] += Rotation[index][j, i] * temp[j];
			}
		}

		protected void CalculateNorm()
		{
			for (int i = 0; i < FuncNum; i++)
			{
				Transform(i);
				FNorm[i] = hybridFunctions[i](tran_x);
			}
		}

		public override double Evaluate(double[] x)
		{
			for (int i = 0; i < FuncNum; i++)
			{
				Transform(x, i);
				FValue[i] = hybridFunctions[i](tran_x);
			}
			//CalculateWeight(x);
			double sum, max = double.NegativeInfinity;
			for (int i = 0; i < FuncNum; i++)
			{
				sum = 0;
				for (int j = 0; j < Dimension; j++)
					sum += (x[j] - Origin[i][j]) * (x[j] - Origin[i][j]);
				Weight[i] = Math.Exp(-sum / (2 * Dimension * Sigma[i] * Sigma[i]));
				if (max < Weight[i]) max = Weight[i];
			}
			sum = 0;
			for (int i = 0; i < FuncNum; i++)
			{
				if (Weight[i] != max) Weight[i] *= (1 - Math.Pow(max, 10));
				sum += Weight[i];
			}
			if (sum == 0)
			{
				for (int i = 0; i < FuncNum; i++)
					Weight[i] = 1.0 / FuncNum;
			}
			else
			{
				for (int i = 0; i < FuncNum; i++)
					Weight[i] /= sum;
			}

			sum = Bias;
			for (int i = 0; i < FuncNum; i++)
				sum += Weight[i] * (FValue[i] * 2000 / FNorm[i] + FBias[i]);
			return sum;
		}

		protected void Set(double[] array, params double[] values)
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = values[i];
		}

		protected void LoadArray(string filename, double[][,] array)
		{
			filename = @"..\..\input_data\" + filename + ".txt";
			var lines = File.ReadAllLines(filename);
			int startline = 0;
			for (int i = 0; i < FuncNum; i++)
			{
				for (int j = 0; j < Dimension; j++)
				{
					var nums = lines[j + startline].Split(split, StringSplitOptions.RemoveEmptyEntries);
					for (int k = 0; k < Dimension; k++)
						array[i][j, k] = double.Parse(nums[k]);
				}
				startline += Dimension;
			}
		}

		protected void LoadArray(string filename, double[][] array)
		{
			filename = @"..\..\input_data\" + filename + ".txt";
			var lines = File.ReadAllLines(filename);
			for (int i = 0; i < FuncNum; i++)
			{
				var nums = lines[i].Split(split, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < Dimension; j++)
					array[i][j] = double.Parse(nums[j]);
			}
		}

		protected int FuncNum;
		protected double[] Lambda, FBias, Sigma, Weight, FValue, FNorm;
		protected double[][] Origin;
		protected double[][,] Rotation;
		protected Func<double[], double>[] hybridFunctions;

		double[] temp;
	}

}

