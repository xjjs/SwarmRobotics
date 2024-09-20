using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using RobotLib;
using RobotLib.Environment;
using RobotLib.TargetTrackProblem;

namespace TestProject
{
	class TrackWork
	{
		public static void Work()
		{
			ParallelTest.ParallelTests.Threads = 3;
			//CompareParamNoise(true);
			CompareParamNoise(false);
			CompareParamLarge(true);
			CompareParamLarge(false);
		}

		static void OptimizeParam(bool inertia)
		{
            var test = new TestBase<STrack>(1, 5000);
			TestOptions option = new TestOptions(5, "Track", 3);
			string prefix = inertia ? "i-" : "ni-";

			List<ExperimentTest> list = new List<ExperimentTest>();
			ExperimentTest para;

			para = new ExperimentTest(typeof(PTargetTracking), typeof(ASpringForce));
			list.Add(para);
			para.SetValue(true, "HasInertia", inertia);
			para.SetPara(false, "Rate", new FloatTestRange(9, 9, 1, 10));
			//if (inertia)
			//{
			para.SetPara(false, "WallC", new FloatTestRange(30, 100, 5, 10));
			//}
			//else
			//{
			//    para.SetPara(false, "WallC", new FloatTestRange(50, 100, 1, 10));
			//}
			para.SetPara(false, "K", new FloatTestRange(10, 30, 1, 10));
			para.SetPara(false, "BR", new FloatTestRange(0, 10, 1, 20));

			para = new ExperimentTest(typeof(PTargetTracking), typeof(ALJForce));
			list.Add(para);
			para.SetValue(true, "HasInertia", inertia);
			if (inertia)
				para.SetPara(false, "Rate", new FloatTestRange(8, 8, 1, 10));
			else
				para.SetPara(false, "Rate", new FloatTestRange(9, 9, 1, 10));
			//para.SetPara(false, "Rate", new FloatTestRange(6, 9, 1, 10));
			//para.SetPara(false, "WallC", new FloatTestRange(10, 100, 5, 10));
			//para.SetPara(false, "C", new FloatTestRange(1, 50, 5, 10));
			//para.SetPara(false, "D", new FloatTestRange(1, 50, 5, 10));

			para = new ExperimentTest(typeof(PTargetTracking), typeof(ANewtonForce));
			list.Add(para);
			para.SetValue(true, "HasInertia", inertia);
			para.SetPara(false, "Rate", new FloatTestRange(9, 9, 1, 10));
			//para.SetPara(false, "Rate", new FloatTestRange(6, 9, 1, 10));
			//para.SetPara(false, "WallC", new FloatTestRange(10, 100, 5, 10));
			//para.SetPara(false, "GC", new FloatTestRange(1, 100, 5, 10));
			//para.SetPara(false, "P", new IntTestRange(1, 5));

			para = new ExperimentTest(typeof(PTargetTracking), typeof(ATetrahedron));
			list.Add(para);
			para.SetValue(true, "HasInertia", inertia);
			para.SetPara(false, "Rate", new FloatTestRange(9, 9, 1, 10));
			//para.SetPara(false, "Rate", new FloatTestRange(6, 9, 1, 10));
			//para.SetPara(false, "WallC", new FloatTestRange(10, 100, 5, 10));
			//para.SetPara(false, "K", new FloatTestRange(10, 50, 1, 10));
			test.TestParam(prefix, option, list.ToArray());
		}

		static void CompareParam(bool inertia)
		{
			string postfix = inertia ? "-i" : "-ni";
			ExperimentTest[] paras = new ExperimentTest[4];
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i] = new ExperimentTest(typeof(PTargetTracking));
				paras[i].SetValue(true, "HasInertia", inertia);

				//Set Environment Params
				//paras[i].SetPara(true, "ObstacleNum", new IntTestRange(100, 5000, 500, 100));
				paras[i].SetPara(true, "ObstacleNum", new ArrayRange(1, new object[] { 100, 500, 1000, 2000 }));
				//paras[i].SetPara(true, "Population", new IntTestRange(1, 64, 25, 1));
				paras[i].SetPara(true, "Population", new ArrayRange(3, new object[] { 9, 16, 25, 36, 49, 64, 81 }));
				//paras[i].SetPara(true, "sizeZ", new IntTestRange(1, 50, 1, 1));
				paras[i].SetPara(true, "SizeZ", new ArrayRange(0, new object[] { 1, 25, 50 }));
			}

			paras[0].AlgorithmType = typeof(ASpringForce);
			paras[1].AlgorithmType = typeof(ANewtonForce);
			paras[2].AlgorithmType = typeof(ALJForce);
			paras[3].AlgorithmType = typeof(ATetrahedron);

            var test = new TestBase<STrack>(1, 10000);
			var option = new TestOptions(50, "Track", -1);
			test.TestCompare("Compare-all" + postfix, option, paras);
		}

		static void CompareParamNoise(bool inertia)
		{
			string postfix = inertia ? "-i" : "-ni";
			ExperimentTest[] paras = new ExperimentTest[4];
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i] = new ExperimentTest(typeof(PTargetTracking));
				paras[i].SetValue(true, "HasInertia", inertia);
				paras[i].SetValue(true, "ObstacleNum", 500);
				paras[i].SetValue(true, "SizeZ", 1);

				//Set Environment Params
				//paras[i].SetPara(true, "ObstacleNum", new ArrayRange(1, new object[] { 500, 2000 }));
				//paras[i].SetPara(true, "Population", new ArrayRange(1, new object[] { 25, 36, 64 }));
				paras[i].SetPara(true, "Population", new ArrayRange(0, new object[] { 36 }));
				//paras[i].SetPara(true, "SizeZ", new ArrayRange(0, new object[] { 1, 50 }));
				paras[i].SetPara(null, "NoisePercent", new FloatTestRange(0, 10, 1, 10));
			}

			paras[0].AlgorithmType = typeof(ASpringForce);
			paras[1].AlgorithmType = typeof(ANewtonForce);
			paras[2].AlgorithmType = typeof(ALJForce);
			paras[3].AlgorithmType = typeof(ATetrahedron);

			var test = new TestBase<STrack>(10, 1, 5000, 10000);
			var option = new TestOptions(10, "Track", -1, 2);
			test.TestCompare("Compare-noise-36" + postfix, option, paras);
		}

		static void CompareParamLarge(bool inertia)
		{
			string postfix = inertia ? "-i" : "-ni";
			ExperimentTest[] paras = new ExperimentTest[4];
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i] = new ExperimentTest(typeof(PTargetTracking));
				paras[i].SetValue(true, "HasInertia", inertia);
				paras[i].SetValue(true, "ObstacleNum", 0);
				paras[i].SetValue(true, "SizeZ", 1);

				//Set Environment Params
				//paras[i].SetPara(true, "Population", new ArrayRange(1, new object[] { 25, 36, 64 }));
				paras[i].SetPara(true, "Population", new ArrayRange(0, new object[] { 36 }));
				paras[i].SetPara(true, "LargeObstacleNum", new ArrayRange(2, new object[] { 0, 10, 50, 100, 200 }));
				//paras[i].SetPara(true, "SizeZ", new ArrayRange(0, new object[] { 1, 50 }));
			}

			paras[0].AlgorithmType = typeof(ASpringForce);
			paras[1].AlgorithmType = typeof(ANewtonForce);
			paras[2].AlgorithmType = typeof(ALJForce);
			paras[3].AlgorithmType = typeof(ATetrahedron);

			var test = new TestBase<STrack>(1, 1, 5000, 10000);
			var option = new TestOptions(20, "Track", -1, 2);
			test.TestCompare("Compare-large-36" + postfix, option, paras);
		}
	}
}
