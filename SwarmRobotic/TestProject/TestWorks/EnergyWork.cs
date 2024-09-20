using System;
using System.Collections.Generic;
using System.IO;
using RobotLib.FitnessProblem;
using System.Linq;

namespace TestProject
{
	class EnergyWork
	{
		public static void Work()
		{
			//var funcs = new Action<ExperimentTest>[] { CompareNormal, CompareObstacle, CompareNoise };
			//foreach (var f in funcs)
			//{
			//    Compare(false, f);
			//    Compare(true, f);
			//}

			//OptimizeParamPSO();
			//OptimizeParam();

			CompareParamMuTheta();
		}

		static void OptimizeParam()
		{
			EnergyTest test = new EnergyTest(20, 10000, true, 1f);
			TestOptions option = new TestOptions(20);

			List<ExperimentTest> list = new List<ExperimentTest>();
			ExperimentTest para;

			para = new ExperimentTest(typeof(PEnergy), typeof(ADSFitness));
			list.Add(para);
			//para.SetPara(false, "Angle", new IntTestRange(60, 60));
			//para.SetPara(false, "Angle", new CombineRange(0, new IntTestRange(0, 0), new IntTestRange(50, 70)));
			//para.SetPara(false, "Step", new IntTestRange(1, 10));
			para.SetPara(false, "Step", new ArrayRange((object)2, 4, 5, 10));
			para.SetPara(false, "Alpha", new IntTestRange(-20, 0));
			//para.SetPara(false, "Group", new IntTestRange(1, 3));
			//para.SetPara(false, "BR", new FloatTestRange(7, 9, 1, 10));
			//para.SetPara(false, "TimeR", new IntTestRange(0, 5));
			//para.SetPara(false, "TimeF", new IntTestRange(0, 3));
			para.SetPara(false, "Directions", new IntTestRange(40, 55));
			//para.SetPara(false, "RandomFit", new ArrayRange(false, true));

			//test.TestParam("grid_", option, para);

			para = new ExperimentTest(typeof(PEnergy), typeof(APSOEFitness));
			list.Add(para);
			para.SetPara(false, "W", new FloatTestRange(5, 5, 1, 10));
			//para.SetPara(false, "C1", new FloatTestRange(25, 40, 1, 10));
			para.SetPara(false, "C2", new FloatTestRange(0, 1, 1, 10));
			//test.Repeat = 10;
			//para.SetPara(false, "W", new FloatTestRange(0, 10, 1, 10));
			//para.SetPara(false, "C1", new FloatTestRange(1, 40, 1, 10));
			//para.SetPara(false, "C2", new FloatTestRange(1, 40, 1, 10));
			//test.TestParam(para, "pso-grid", maps: maps, seeds: seeds);
			//test.TestParam("grid_", option, para);

			para = new ExperimentTest(typeof(PEnergy), typeof(ARPSOFitness));
			list.Add(para);
			para.SetPara(false, "W", new FloatTestRange(9, 9, 1, 10));
			//para.SetPara(false, "W", new FloatTestRange(5, 9, 1, 10));
			//para.SetPara(false, "C1", new FloatTestRange(10, 40, 2, 10));
			//para.SetPara(false, "C2", new FloatTestRange(10, 40, 2, 10));
			//test.Repeat = 10;
			//para.SetPara(false, "W", new FloatTestRange(0, 10, 1, 10));
			//para.SetPara(false, "C1", new FloatTestRange(1, 40, 1, 10));
			//para.SetPara(false, "C2", new FloatTestRange(1, 40, 1, 10));
			//test.TestParam(para, "pso-grid", maps: maps, seeds: seeds);

			para = new ExperimentTest(typeof(PEnergy), typeof(AUAVFitness));
			list.Add(para);
			para.SetPara(false, "BR", new FloatTestRange(6, 9, 1, 10));
			para.SetPara(false, "AveStep", new IntTestRange(1, 10));
			para.SetPara(false, "D", new IntTestRange(1, 10));

			test.TestParam("grid_", option, list.ToArray());
		}

		static void OptimizeParamPSO()
		{
			EnergyTest test = new EnergyTest(20, 100000, true, 1f);
			TestOptions option = new TestOptions(20, "Energy", 3);
			option.pso = 0;

			List<ExperimentTest> list = new List<ExperimentTest>();
			ExperimentTest para;

			para = new ExperimentTest(typeof(PEnergy), typeof(ADSFitness));
			list.Add(para);
			para.SetPara(false, "Angle", new CombineRange(0, new IntTestRange(0, 0), new IntTestRange(30, 90)));
			para.SetPara(false, "Step", new IntTestRange(1, 10));
			para.SetPara(false, "Alpha", new IntTestRange(-10, 20));

			para = new ExperimentTest(typeof(PEnergy), typeof(APSOEFitness));
			list.Add(para);
			para.SetPara(false, "W", new FloatTestRange(0, 10, 1, 10));
			para.SetPara(false, "C1", new FloatTestRange(1, 100, 1, 10));
			para.SetPara(false, "C2", new FloatTestRange(1, 100, 1, 10));

			para = new ExperimentTest(typeof(PEnergy), typeof(ARPSOFitness));
			list.Add(para);
			para.SetPara(false, "W", new FloatTestRange(0, 10, 1, 10));
			para.SetPara(false, "C1", new FloatTestRange(1, 100, 1, 10));
			para.SetPara(false, "C2", new FloatTestRange(1, 100, 1, 10));

			para = new ExperimentTest(typeof(PEnergy), typeof(AUAVFitness));
			list.Add(para);
			para.SetPara(false, "BR", new FloatTestRange(0, 10, 1, 10));
			para.SetPara(false, "AveStep", new IntTestRange(1, 10));
			para.SetPara(false, "D", new IntTestRange(1, 10));

			test.TestParam("pso_", option, list.ToArray());
		}

		static void Compare(bool EnergyMode, Action<ExperimentTest> paramFunc)
		{
			Type[] Algorithms = new Type[] { typeof(ADSFitness), typeof(ARPSOFitness), typeof(AUAVFitness) };
			ExperimentTest[] paras = new ExperimentTest[10];
			for (int i = 0; i < paras.Length; i++)
				paras[i] = new ExperimentTest(typeof(PEnergy));
			for (int i = 0; i < Algorithms.Length; i++)
			{
				for (int j = 0; j < Algorithms.Length; j++)
				{
					var item = paras[i * Algorithms.Length + j];
					if (i == j)
						item.AlgorithmType = Algorithms[i];
					else
					{
						item.AlgorithmType = typeof(AHybridFitness);
						item.Name = string.Format("Hybrid_{0}+{1}", Algorithms[i].Name, Algorithms[j].Name);
						item.SetValue(false, "RandomAlgorithm", Algorithms[i]);
						item.SetValue(false, "FitnessAlgorithm", Algorithms[j]);
					}
				}
			}
			paras[9].AlgorithmType = typeof(ADSFitness);
			paras[9].Name = "DS_Random";
			paras[9].SetValue(false, "RandomFit", true);

			//paras = new ExperimentTest[] {paras[9], paras[0], paras[3] };

			//Set Environment Params
			foreach (var item in paras)
			{
				item.SetValue(true, "EnergyMode", EnergyMode);
				paramFunc(item);
			}
			string prefix = paramFunc.Method.Name + (EnergyMode ? "-EM" : "-TM");
			EnergyTest test = new EnergyTest(25, 100000, true, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			TestOptions option = new TestOptions(20, "Energy", -1, 6, false);
			test.TestCompare(prefix + "-SE", option, paras);

			test.EnergyRate = false;
			test.TestCompare(prefix + "-ST", option, paras);
		}

		static void CompareNormal(ExperimentTest item)
		{
			//paras[i].SetPara(true, "TargetNum", new ArrayRange(4, new object[] { 9, 12, 16, 20, 25, 32, 40 }));
			item.SetPara(true, "TargetNum", new IntTestRange(40, 100, 60, 10));
			item.SetPara(true, "Population", new IntTestRange(50, 150, 100, 10));
			item.SetPara(true, "TotalEnergy", new FloatTestRange(20000, 40000, 2000, 30000, 1));
			//item.SetPara(true, "TargetNum", new IntTestRange(10, 40, 20, 5));
			//item.SetPara(true, "Population", new IntTestRange(1, 16, 9, 1));
			//item.SetPara(true, "Population", new ArrayRange(3, new object[] { 1, 4, 6, 9, 12, 16 }));
			//item.SetPara(true, "TotalEnergy", new FloatTestRange(2000, 8000, 1000, 5000, 1));
			//paras[i].SetPara(true, "TargetNum", new ArrayRange(0, new object[] { 25 }));
			//paras[i].SetPara(true, "Population", new IntTestRange(9, 9, 9, 1));
			//paras[i].SetPara(true, "TotalEn", new FloatTestRange(10000, 10000, 500, 10000, 1));
		}

		static void CompareObstacle(ExperimentTest test) { test.SetPara(true, "ObstacleNum", new IntTestRange(0, 1000, 100)); }

		static void CompareNoise(ExperimentTest test) { test.SetPara(null, "NoisePercent", new FloatTestRange(0, 9, 1, 10)); }

		static void CompareParamMuTheta()
		{
			ExperimentTest para = new ExperimentTest(typeof(PEnergy), typeof(ADSFitness));
			//para.SetValue(false, "Angle", 60);
			para.SetPara(false, "Step", new IntTestRange(1, 10, 5, 1));
			para.SetPara(false, "Alpha", new IntTestRange(-15, 0, 0, 1));

			EnergyTest test = new EnergyTest(20, 10000, true, 0.8f);
			TestOptions option = new TestOptions(20);

			test.TestParam("test-mu-theta-", option, para);
		}

		static void CompareParamAngle()
		{
			ExperimentTest para = new ExperimentTest(typeof(PEnergy), typeof(ADSFitness));
			//para.SetValue(false, "Step", 5);
			//para.SetValue(false, "Alpha", 0);
			para.SetPara(false, "Angle", new IntTestRange(30, 90, 60, 1));

			TestBase<SEnergy> test = new TestBase<SEnergy>(50);
			TestOptions option = new TestOptions(50);

			test.TestParam("test-angle-", option, para);
		}

		internal static void CSV2TexTable(string inputfile, string outputfile)
		{
			StreamReader sr = new StreamReader(inputfile);
			StreamWriter sw = new StreamWriter(outputfile);
			List<string[]> list = new List<string[]>();
			string[] split;
			string line;
			int length = 0, nelen;
			while (!sr.EndOfStream)
			{
				line = sr.ReadLine();
				split = line.Split(',');
				list.Add(Parse(split));
				for (nelen = split.Length - 1; nelen >= 0; nelen--)
				{
					if (split[nelen].Trim() != "") break;
				}
				if (nelen > length) length = nelen;
			}
			sr.Close();

			sw.WriteLine(@"\documentclass[a4paper]{article}");
			sw.WriteLine(@"\usepackage[paperwidth=18.4 cm,paperheight=26 cm,top=2cm,bottom=2cm,left=2cm,right=2cm,includemp=false,marginparsep=0cm,marginparwidth=0cm]{geometry}");
			sw.WriteLine(@"\begin{document}");
			sw.WriteLine(@"\begin{table}[H]");
			sw.WriteLine(@"\begin{center}");
			sw.WriteLine(@"\caption{caption}");
			sw.WriteLine(@"\label{T:label}");
			sw.Write(@"\begin{tabular}{|");
			for (int i = 0; i <= length; i++)
				sw.Write("c|");
			sw.WriteLine("}");
			sw.WriteLine(@"\hline");
			foreach (var arr in list)
			{
				for (int i = 0; i <= length; i++)
				{
					if (i < arr.Length) sw.Write(arr[i]);
					if (i != length) sw.Write(" & ");
				}
				sw.WriteLine(@"\\ \hline");
			}
			sw.WriteLine(@"\end{tabular}");
			sw.WriteLine(@"\end{center}");
			sw.WriteLine(@"\end{table}");
			sw.WriteLine(@"\end{document}");
			sw.Close();
		}

		static string[] Parse(string[] input)
		{
			string item;
			//TimeSpan time;
			int index;
			double number;
			for (int i = 0; i < input.Length; i++)
			{
				item = input[i];
				index = item.IndexOf('-');
				if (index != -1)
					input[i] = item.Substring(0, index);
				else
				{
					index = item.IndexOf(':');
					if (index != -1)
					{
						try
						{
							input[i] = TimeSpan.Parse(item).TotalSeconds.ToString();
						}
						catch
						{
							number = int.Parse(item.Substring(0, index)) * 60;
							number += double.Parse(item.Substring(index + 1));
							input[i] = number.ToString();
						}
					}
				}

			}
			return input;
		}
	}
}
