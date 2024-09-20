using System;

namespace UtilityProject.Funcs.CEC05
{
	/// <summary>
	/// Shifted Sphere Function
	/// </summary>
	public sealed class F1 : CECShiftFunction
	{
		public F1(CECDimension Dim)
			: base(-100, 100, Dim, -450)
		{
			LoadArray("sphere_func_data", Origin);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Sphere(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Schwefel’s Problem 1.2
	/// </summary>
	public sealed class F2 : CECShiftFunction
	{
		public F2(CECDimension Dim)
			: base(-100, 100, Dim, -450)
		{
			LoadArray("schwefel_102_data", Origin);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Schwefel(tran_x) + Bias;
		}
	}
	
	/// <summary>
	/// Shifted Rotated High Conditioned Elliptic Function
	/// </summary>
	public sealed class F3 : CECRotateFunction
	{
		public F3(CECDimension Dim)
			: base(-100, 100, Dim, -450)
		{
			LoadArray("high_cond_elliptic_rot_data", Origin);
			LoadArray("elliptic_M_" + Dim.ToString(), Rotation);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Elliptic(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Schwefel’s Problem 1.2 with Noise in Fitness
	/// </summary>
	public sealed class F4 : CECShiftFunction
	{
		public F4(CECDimension Dim)
			: base(-100, 100, Dim, -450)
		{
			LoadArray("schwefel_102_data", Origin);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Schwefel(tran_x) * (1 + 0.4 * Math.Abs(CECBaseFunction.RandomNormalDeviate())) + Bias;
		}
	}

	/// <summary>
	/// Schwefel’s Problem 2.6 with Global Optimum on Bounds
	/// </summary>
	public sealed class F5 : CECShiftFunction
	{
		public F5(CECDimension Dim)
			: base(-100, 100, Dim, -310)
		{
			LoadArray("schwefel_206_data", Origin);
			LoadArray("schwefel_206_data", out A, 1);

			int index;
			if (Dimension % 4 == 0)
				index = Dimension / 4;
			else
				index = Dimension / 4 + 1;
			for (int i = 0; i < index; i++)
				Origin[i] = -100;
			index = Dimension * 3 / 4 - 1;
			for (int i = index; i < Dimension; i++)
				Origin[i] = 100;

			B = new double[Dimension];
			for (int i = 0; i < Dimension; i++)
			{
				B[i] = 0;
				for (int j = 0; j < Dimension; j++)
					B[i] += A[i, j] * Origin[j];
			}
		}

		public override double Evaluate(double[] x)
		{
			double r = double.NegativeInfinity, cur;
			for (int i = 0; i < Dimension; i++)
			{
				cur = -B[i];
				for (int j = 0; j < Dimension; j++)
					cur += A[i, j] * x[j];
				cur = Math.Abs(cur);
				if (r < cur) r = cur;
			}
			return r + Bias;
		}

		double[,] A;
		double[] B;
	}
	
	/// <summary>
	/// Shifted Rosenbrock’s Function
	/// </summary>
	public sealed class F6 : CECShiftFunction
	{
		public F6(CECDimension Dim)
			: base(-100, 100, Dim, 390)
		{
			LoadArray("rosenbrock_func_data", Origin);
			for (int i = 0; i < Dimension; i++)
				Origin[i]--;
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Rosenbrock(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Rotated Griewank’s Function without Bounds
	/// </summary>
	public sealed class F7 : CECRotateFunction
	{
		public F7(CECDimension Dim)
			: base(double.NegativeInfinity, double.PositiveInfinity, Dim, -180)
		{
			LoadArray("griewank_func_data", Origin);
			LoadArray("griewank_M_" + Dim.ToString(), Rotation);
			Range.InitLBound = 0;
			Range.InitSize = 600;
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Griewank(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Rotated Ackley’s Function with Global Optimum on Bounds
	/// </summary>
	public sealed class F8 : CECRotateFunction
	{
		public F8(CECDimension Dim)
			: base(-32, 32, Dim, -140)
		{
			LoadArray("ackley_func_data", Origin);
			LoadArray("ackley_M_" + Dim.ToString(), Rotation);
			for (int i = 0; i < Dimension; i += 2)
				Origin[i] = -32;
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Ackley(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Rastrigin’s Function
	/// </summary>
	public sealed class F9 : CECShiftFunction
	{
		public F9(CECDimension Dim)
			: base(-5, 5, Dim, -330)
		{
			LoadArray("rastrigin_func_data", Origin);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Rastrigin(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Rotated Rastrigin’s Function
	/// </summary>
	public sealed class F10 : CECRotateFunction
	{
		public F10(CECDimension Dim)
			: base(-5, 5, Dim, -330)
		{
			LoadArray("rastrigin_func_data", Origin);
			LoadArray("rastrigin_M_" + Dim.ToString(), Rotation);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Rastrigin(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Rotated Weierstrass Function
	/// </summary>
	public sealed class F11 : CECRotateFunction
	{
		public F11(CECDimension Dim)
			: base(-0.5, 0.5, Dim, 90)
		{
			LoadArray("weierstrass_data", Origin);
			LoadArray("weierstrass_M_" + Dim.ToString(), Rotation);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Weierstrass(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Schwefel’s Problem 2.13
	/// </summary>
	public sealed class F12 : CECShiftFunction
	{
		public F12(CECDimension Dim)
			: base(-Math.PI, Math.PI, Dim, -460)
		{
			A = new double[Dimension, Dimension];
			B = new double[Dimension, Dimension];
			alpha = new double[Dimension];

			var lines = System.IO.File.ReadAllLines(@"..\..\input_data\schwefel_213_data.txt");
			string[] nums;
			for (int i = 0; i < Dimension; i++)
			{
				nums = lines[i].Split(split, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < Dimension; j++)
					A[i, j] = double.Parse(nums[j]);
			}
			for (int i = 0; i < Dimension; i++)
			{
				nums = lines[i + 100].Split(split, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < Dimension; j++)
					B[i, j] = double.Parse(nums[j]);
			}
			nums = lines[200].Split(split, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < Dimension; i++)
				alpha[i] = double.Parse(nums[i]);
		}

		public override double Evaluate(double[] x)
		{
			double r = 0, sum;
			for (int i = 0; i < Dimension; i++)
			{
				sum = 0;
				for (int j = 0; j < Dimension; j++)
					sum += A[i, j] * (Math.Sin(alpha[j]) - Math.Sin(x[j])) + B[i, j] * (Math.Cos(alpha[j]) - Math.Cos(x[j]));
				r += sum * sum;
			}
			return r + Bias;
		}

		double[,] A, B;
		double[] alpha;
	}

	/// <summary>
	/// Expanded Extended Griewank’s plus Rosenbrock’s Function (F8F2)
	/// </summary>
	public sealed class F13 : CECShiftFunction
	{
		public F13(CECDimension Dim)
			: base(-5, 5, Dim, -130)
		{
			LoadArray("EF8F2_func_data", Origin);
			for (int i = 0; i < Dimension; i++)
				Origin[i]--;
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.F8F2(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Shifted Rotated Expanded Scaffer’s F6 Function
	/// </summary>
	public sealed class F14 : CECRotateFunction
	{
		public F14(CECDimension Dim)
			: base(-100, 100, Dim, -300)
		{
			LoadArray("E_ScafferF6_func_data", Origin);
			LoadArray("E_ScafferF6_M_" + Dim.ToString(), Rotation);
		}

		public override double Evaluate(double[] x)
		{
			Transform(x);
			return CECBaseFunction.Schaffer(tran_x) + Bias;
		}
	}

	/// <summary>
	/// Hybrid Composition Function
	/// </summary>
	public class F15 : CECHybridFunction
	{
		public F15(CECDimension Dim)
			: base(-5, 5, Dim, 10, 120,
			CECBaseFunction.Rastrigin, CECBaseFunction.Rastrigin,
			CECBaseFunction.Weierstrass, CECBaseFunction.Weierstrass,
			CECBaseFunction.Griewank, CECBaseFunction.Griewank,
			CECBaseFunction.Ackley, CECBaseFunction.Ackley,
			CECBaseFunction.Sphere, CECBaseFunction.Sphere)
		{
			LoadArray("hybrid_func1_data", Origin);
			Set(Lambda, 1, 1, 10, 10, 1.0 / 12, 1.0 / 12, 5.0 / 32, 5.0 / 32, 1.0 / 20, 1.0 / 20);
			CalculateNorm();
		}
	}

	/// <summary>
	/// Rotated Version of Hybrid Composition Function F15
	/// </summary>
	public class F16 : F15
	{
		public F16(CECDimension Dim)
			: base(Dim)
		{
			LoadArray("hybrid_func1_M_" + Dim.ToString(), Rotation);
			CalculateNorm();
		}
	}

	/// <summary>
	/// F16 with Noise in Fitness
	/// </summary>
	public sealed class F17 : F16
	{
		public F17(CECDimension Dim) : base(Dim) { }

		public override double Evaluate(double[] x) { return (base.Evaluate(x) - Bias) * (1 + 0.2 * Math.Abs(CECBaseFunction.RandomNormalDeviate())) + Bias; }
	}

	/// <summary>
	/// Rotated Hybrid Composition Function
	/// </summary>
	public class F18 : CECHybridFunction
	{
		public F18(CECDimension Dim)
			: base(-5, 5, Dim, 10, 10,
			CECBaseFunction.Ackley, CECBaseFunction.Ackley,
			CECBaseFunction.Rastrigin, CECBaseFunction.Rastrigin,
			CECBaseFunction.Sphere, CECBaseFunction.Sphere,
			CECBaseFunction.Weierstrass, CECBaseFunction.Weierstrass,
			CECBaseFunction.Griewank, CECBaseFunction.Griewank)
		{
			LoadArray("hybrid_func2_data", Origin);
			LoadArray("hybrid_func2_M_" + Dim.ToString(), Rotation);
			Set(Sigma, 1, 2, 1.5, 1.5, 1, 1, 1.5, 1.5, 2, 2);
			Set(Lambda, 5 / 16.0, 5 / 32.0, 2, 1, 0.1, 0.05, 20, 10, 1 / 6.0, 1 / 12.0);
			Array.Clear(Origin[9], 0, Dimension);
			CalculateNorm();
		}
	}

	/// <summary>
	/// Rotated Hybrid Composition Function with narrow basin global optimum
	/// </summary>
	public sealed class F19 : F18
	{
		public F19(CECDimension Dim)
			: base(Dim)
		{
			Set(Sigma, 0.1, 2, 1.5, 1.5, 1, 1, 1.5, 1.5, 2, 2);
			Set(Lambda, 0.5 / 32, 5 / 32.0, 2, 1, 0.1, 0.05, 20, 10, 1 / 6.0, 1 / 12.0);
			CalculateNorm();
		}
	}

	/// <summary>
	/// Rotated Hybrid Composition Function with Global Optimum on the Bounds
	/// </summary>
	public sealed class F20 : F18
	{
		public F20(CECDimension Dim)
			: base(Dim)
		{
			for (int i = 1; i < Dimension; i+=2)
				Origin[0][i] = 5;
			CalculateNorm();
		}
	}

	/// <summary>
	/// Rotated Hybrid Composition Function
	/// </summary>
	public class F21 : CECHybridFunction
	{
		public F21(CECDimension Dim)
			: base(-5, 5, Dim, 10, 360,
			CECBaseFunction.Schaffer, CECBaseFunction.Schaffer,
			CECBaseFunction.Rastrigin, CECBaseFunction.Rastrigin,
			CECBaseFunction.F8F2, CECBaseFunction.F8F2,
			CECBaseFunction.Weierstrass, CECBaseFunction.Weierstrass,
			CECBaseFunction.Griewank, CECBaseFunction.Griewank)
		{
			LoadArray("hybrid_func3_data", Origin);
			LoadArray("hybrid_func3_M_" + Dim.ToString(), Rotation);
			Set(Sigma, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2);
			Set(Lambda, 0.25, 0.05, 5, 1, 5, 1, 50, 10, 0.125, 0.025);
			CalculateNorm();
		}
	}

	/// <summary>
	/// Rotated Hybrid Composition Function with High Condition Number Matrix
	/// </summary>
	public sealed class F22 : F21
	{
		public F22(CECDimension Dim)
			: base(Dim)
		{
			LoadArray("hybrid_func3_HM_" + Dim.ToString(), Rotation);
			CalculateNorm();
		}
	}

	/// <summary>
	/// Non-Continuous Rotated Hybrid Composition Function
	/// </summary>
	public sealed class F23 : F21
	{
		public F23(CECDimension Dim) : base(Dim) { temp = new double[Dimension]; }

		public override double Evaluate(double[] x)
		{
			for (int i = 0; i < Dimension; i++)
				temp[i] = CECBaseFunction.RoundOff(x[i], Origin[0][i]);
			return base.Evaluate(temp);
		}

		double[] temp;
	}

	/// <summary>
	/// Rotated Hybrid Composition Function
	/// </summary>
	public class F24 : CECHybridFunction
	{
		public F24(CECDimension Dim)
			: base(-5, 5, Dim, 10, 260,
			CECBaseFunction.Weierstrass, CECBaseFunction.Schaffer,
			CECBaseFunction.F8F2, CECBaseFunction.Ackley,
			CECBaseFunction.Rastrigin, CECBaseFunction.Griewank,
			CECBaseFunction.Schaffer_nc, CECBaseFunction.Rastrigin_nc,
			CECBaseFunction.Elliptic, null)
		{
			LoadArray("hybrid_func4_data", Origin);
			LoadArray("hybrid_func4_M_" + Dim.ToString(), Rotation);
			for (int i = 0; i < FuncNum; i++)
				Sigma[i] = 2;
			Set(Lambda, 10, 0.25, 1, 5.0/32, 1, 0.05, 0.1, 1, 0.05, 0.05);
			hybridFunctions[9] = NoiseSphere;
			CalculateNorm();
		}

		double NoiseSphere(double[] x) { return CECBaseFunction.Sphere(x) * (1 + 0.1 * CECBaseFunction.RandomNormalDeviate()); }
	}

	/// <summary>
	/// Rotated Hybrid Composition Function without bounds
	/// </summary>
	public sealed class F25 : F24
	{
		public F25(CECDimension Dim)
			: base(Dim)
		{
			Range.LBound = double.PositiveInfinity;
			Range.UBound = double.NegativeInfinity;
			Range.InitLBound = 2;
			Range.InitSize = 3;
		}
	}
}
