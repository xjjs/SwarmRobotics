using System;
using System.Collections.Generic;
using RobotLib.FitnessProblem;
using RobotLib;
using Emgu.CV;
using System.IO;


namespace TestProject
{
    /// <summary>
    /// 用作多目标搜索问题的“测试入口”，静态成员Work()为主程序入口
    /// </summary>
	class MinimalWork
	{
		static int Threads = Environment.ProcessorCount > 16 ? 16 : 8;

        //作为主程序入口的Work调用其他函数以进行专项测试，最后用来进行博士论文相关的实验测试（函数调用移到PhdThesis中了）
		public static void Work()
		{
			//OptimizeParam();
			//CompareParam(500, true, false, 0.5f);
			//CompareParam(500, false, false);
			//CompareParam(1000, true, true, 0.5f);
			//CompareObstacle(500, 0.25f, 0.5f, 0.75f);

			//CompareParam(CollectDecoy: null);	//CompareTar();
			//CompareParam(Real1: true);	//CompareDecoy1();
			//CompareParam(CollectDecoy: true, Real1: true);	//CompareDecoy1(CollectDecoy: true);
			//CompareParam(CollectDecoy: true);	//CompareDecoy(CollectDecoy: true);
			//=CompareParam();	//CompareDecoy();
			//CompareDecoy(500, true);

			//CompareCooperate(true);
			//OptimizeParam();

			//CompareParam(CollectDecoy: null);
			//CompareInterference();
			//CompareCooperateLarge(true);
			//CompareCooperate(true);

			//CompareCooperateQuick();
			//OptimizeParam();
			//CompareCooperateLarge(true);
			//CompareCooperate(true);
			//CompareObstacle();
			//CompareInterference();

			//PhDThesis();
            //for(int i = 0; i < 3; i++)
            //OptimizeParam();
            //CompareSearch();
            //CompareSearch1();
            //CompareSearch2();
            //CompareSearch3();

            //for(int k = 0; k < 3; ++k)
            //    SingleTest();
            //ObstacleAvoide();
            DataGenerate();

            //EvolveFrame(false);
            //EvolveFrame(true);
		}

        //用于参数优化：优化某个算法则注释掉其余的算法优化代码，开启“随机搜索阶段”
		static void OptimizeParam(bool CollectDecoy = false)
		{
            //粗网格遍历：为减少时间，采用8幅地图20次运行
            //细网格遍历：为提高精度，采用16幅地图50次运行

            //创建一个测试对象：最大迭代次数5000，目标收集率为1
			//var test = new MinimalTest(30, 5000, 1f);	//10
            var test = new MinimalTest(25, 5000, 1f);	//10
            //创建拥有20个种子地图的测试选项
			//TestOptions option = new TestOptions(102);	//5
            TestOptions option = new TestOptions(40, "singleTest1", Threads);
            //创建实验测试的列表对象、单一的实验对象
			List<ExperimentTest> list = new List<ExperimentTest>();
			ExperimentTest para;

            //GES算法的参数优化
			//para = new ExperimentTest(typeof(PMinimal), typeof(AGESFitness));	//48 hours
			//list.Add(para);
			//para.SetValue(true, "CollectDecoy", CollectDecoy);
			//para.SetPara(false, "SSize", new IntTestRange(4, 8));
			//para.SetPara(false, "RWeight", new FloatTestRange(50, 95, 1, 100));
			//para.SetPara(false, "SRange", new FloatTestRange(5, 45, 1, 100));


            ////IGES算法的参数优化，创建用IGES解决多目标搜索问题的实验对象
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));	//1 hour
            //list.Add(para);
            ////这里并非是实际设置问题的各项属性，而是将参数设置保存到描述列表（SetPara）和字典(SetValue)中
            //para.SetValue(true, "CollectDecoy", CollectDecoy);
            //para.SetValue(true, "DecoyNum", 0);
            //para.SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100));
            ////IGES算法唯一要优化的参数：小组尺寸
            //para.SetPara(false, "SSize", new IntTestRange(2, 30));
            //para.SetValue(true, "Population", 30);
            //para.SetValue(true, "TargetNum", 30);

			//para.SetPara(false, "SSize", new ArrayRange((object)4, 5, 6, 7, 8, 9, 10, 100));
			//para.SetPara(true, "Population", new IntTestRange(10, 50, 10));



            ////PSO算法的参数优化：原来的APSOMinimal算法已不见了，可用ARPSOFitness代替
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));	//9 hours
            //list.Add(para);
            //para.SetPara(false, "W", new FloatTestRange(25, 35, 2, 10));
            //para.SetPara(false, "C1", new FloatTestRange(5, 15, 2, 10));
            //para.SetPara(false, "C2", new FloatTestRange(15, 25, 2, 10));
            ////para.SetPara(false, "C3", new FloatTestRange(0, 3, 1, 10));

            ////AdaPSO
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(AdaPSOFitness));	//9 hours
            //list.Add(para);
            ////para.SetPara(false, "W", new FloatTestRange(10, 20, 2, 10));
            //para.SetPara(false, "C1", new FloatTestRange(5, 7, 1, 10));
            //para.SetPara(false, "C2", new FloatTestRange(3, 7, 1, 10));
            ////para.SetPara(false, "C3", new FloatTestRange(1, 3, 1, 10));
            //para.SetPara(false, "Alpha", new FloatTestRange(0, 6, 1, 10));
            ////para.SetPara(false, "Beta", new FloatTestRange(6, 8, 1, 10));
            


            ////LevyFllight算法的参数优化
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));
            //list.Add(para);
            ////由于程序问题，FloatRange的参数设置使得其初值Cur为正无穷大，但参数取值还是正确的？
            ////para.SetPara(false, "ExponentialU", new FloatTestRange(1001, 1050, 1, 1000));
            ////para.SetPara(false, "AC", new FloatTestRange(1, 40, 1, 10));
            ////para.SetPara(false, "Lambda", new IntTestRange(2100, 3000, 100));
            //para.SetPara(false, "InertiaMove", new FloatTestRange(0, 8, 1, 10));
            ////para.SetPara(false, "C3", new FloatTestRange(5, 15, 1, 100));

            ////AIntermittent算法的参数优化
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(AIntermittent));
            //list.Add(para);
            //para.SetPara(false, "AC", new FloatTestRange(1, 10, 1, 10));
            //para.SetPara(false, "BC", new FloatTestRange(25, 35, 1, 10));
            //para.SetPara(false, "InertiaMove", new FloatTestRange(4, 7, 1, 10));
            ////para.SetPara(false, "C3", new FloatTestRange(0, 2, 1, 10));

            ////SinglySearch算法的参数优化
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));
            //list.Add(para);
            //para.SetPara(false, "InertiaMove", new FloatTestRange(0, 9, 1, 10));
            ////para.SetPara(false, "C3", new FloatTestRange(0, 5, 1, 10));

            ////AFormation算法的参数优化
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(AFormationFitness));
            //list.Add(para);
            //para.SetPara(false, "DThreshold", new IntTestRange(3, 10, 1));
            //para.SetPara(false, "AC", new FloatTestRange(50, 150, 5, 10));

            //PGES算法
            para = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            list.Add(para);

            //para.SetPara(false, "InitialInertiaState", new FloatTestRange(1, 99, 1, 100));
            //para.SetPara(false, "InitialInertiaState", new FloatTestRange(9900, 9999, 1, 10000));
            //para.SetPara(false, "InitialInertiaState", new FloatTestRange(99900, 99999, 1, 100000));
            //para.SetPara(false, "InertiaMove", new FloatTestRange(0, 100, 1, 100));
            //para.SetPara(false, "InertiaMove", new FloatTestRange(35, 65, 1, 100));
            para.SetPara(false, "DiffusionThreshold", new FloatTestRange(1, 100, 1, 10));
            //para.SetPara(false, "DiffusionThreshold", new FloatTestRange(5, 45, 1, 10));
            //para.SetPara(false, "C3", new FloatTestRange(0, 5, 1, 10)); //稳定性测试

            ////Random算法
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(ARandomFitness));
            //list.Add(para);
            //para.SetPara(false, "InertiaMove", new FloatTestRange(0, 8, 1, 10));
            //para.SetPara(false, "InitialInertiaState", new FloatTestRange(99960, 99975, 1, 100000));

            ////tristatePFSM算法
            //para = new ExperimentTest(typeof(PMinimalMap), typeof(tristatePFSMFitness));
            //list.Add(para);
            ////para.SetPara(false, "InitialInertiaRun", new FloatTestRange(999900, 999940, 5, 1000000));
            ////para.SetPara(false, "InitialInertiaDiffusion", new FloatTestRange(999700, 999750, 5, 1000000));
            ////para.SetPara(false, "InertiaMove", new FloatTestRange(49, 51, 1, 100));
            //para.SetPara(false, "DiffusionThreshold", new FloatTestRange(5, 35, 1, 10));



            //参数测试：对要测试的实验列表进行并行测试
			test.TestParam("opt_dt_", option, list.ToArray());
		}

        //用于算法对比：关闭“随机搜索阶段”，不考虑“干扰源”，按目标收集率进行结果统计
        //“参数优化”阶段与“算法对比”阶段采用不同的设置合适么？
		static void CompareParam(int size = 1000, int steps = 3, bool? CollectDecoy = false, bool Real1 = false, bool hasRand = false)
		{
            //目标不可收集则采用6个实验测试，否则选用3个
			ExperimentTest[] paras = new ExperimentTest[CollectDecoy == false ? 6 : 3];


            //设置问题参数
			for (int i = 0; i < paras.Length; i++)
			{
                //创建实验测试对象、设置地图尺寸
				paras[i] = new ExperimentTest(typeof(PMinimalMap));
				paras[i].SetValue(true, "Size", size);

                //CollectDecoy为null时不考虑假目标（假目标数置0），否则将CollectDecoy开关设为实际值
                //根据条件选择是否只用“1个真目标”，不考虑干扰源
				if (CollectDecoy.HasValue)
				{
					paras[i].SetValue(true, "CollectDecoy", CollectDecoy.Value);
					if (Real1) paras[i].SetValue(true, "TargetNum", 1);
					paras[i].SetValue(true, "InterferenceNum", 0);
				}
				else
					paras[i].SetValue(true, "DecoyNum", 0);
			}

            //绑定算法与设置算法参数
            //若假目标可收集则只设置进行3组实验，否则进行6组实验（对比独立策略与合作策略）
			if (paras.Length == 3)
			{
				paras[0].AlgorithmType = typeof(AGESFitness);
				paras[1].AlgorithmType = typeof(AIGESFitness);
				paras[2].AlgorithmType = typeof(ARPSOFitness);
			}
			else
			{
				paras[0].AlgorithmType = typeof(AGESFitness);
				paras[1].AlgorithmType = typeof(AGESFitness);
				paras[1].SetValue(false, "Cooperate", false);

				paras[2].AlgorithmType = typeof(AIGESFitness);
				paras[3].AlgorithmType = typeof(AIGESFitness);
				paras[3].SetValue(false, "Cooperate", false);

				paras[4].AlgorithmType = typeof(ARPSOFitness);
				paras[5].AlgorithmType = typeof(ARPSOFitness);
				paras[5].SetValue(false, "Cooperate", false);
			}


			//Set Environment Params --> 实际设置的是算法参数（false）与问题参数（true）
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(false, "HasRandomStage", hasRand);
                
                //不考虑假目标时只对群体、真目标数进行范围测试；否则也考虑假目标的数目设置；
				if (CollectDecoy == null)	//Compare-Tar
				{
					paras[i].SetPara(true, "Population", new IntTestRange(10, 50, 10, 2));
					paras[i].SetPara(true, "TargetNum", new IntTestRange(10, 100, 2));
				}
				else
				{
					paras[i].SetPara(true, "Population", new IntTestRange(10, 30, 10, 10));
					if (!Real1)	//Compare-Decoy or Compare-Collect
					{
						var range = new IntTestRange(10, 40, 10);
						paras[i].SetPara(true, "TargetNum", range);
						paras[i].SetPara(true, "DecoyNum", new RelevantSerialRange(range, 2, (o, j) => (j + 2) * (int)o));
					}
					else	//Compare-Decoy1 or Compare-Collect1
						paras[i].SetPara(true, "DecoyNum", new IntTestRange(10, 40, 10));
				}
			}

			var test = new MinimalTest(20, size * 10, 0.5f, 0.75f, 1f);
			TestOptions option = new TestOptions(20, "Fitness", Threads, steps);
			test.TestCompare((CollectDecoy == null ? "Compare-Tar-" : (CollectDecoy.Value ? "Compare-Collect" : "Compare-Decoy") + (Real1 ? "1-" : "-")) + steps + "-" + size, option, paras);
		}

        //假目标躲避测试：可选单个算法测试or所有算法测试（3个）
		static void CompareCooperate(bool full = false)
		{
			ExperimentTest[] paras = new ExperimentTest[full ? 3 : 1];
			paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
			if (full)
			{
				paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
				paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));
			}

            //object类型可装箱float与bool类型
			object[,] @params = new object[12, 2];
			for (int i = 0; i < 11; i++)
			{
				@params[i, 0] = true;
				@params[i, 1] = i / 10f;
			}
			@params[11, 0] = false;
			@params[11, 1] = 1;

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(true, "InterferenceNum", 0);

                //矩阵的每行为(Cooperate,PLeave)的一次取值，11行对应11种不同的取值组合，仍支持相应的Step方法（只不过是按行移动）
				paras[i].SetPara(false, new MatrixMultiRange(@params), "Cooperate", "PLeave");
				//paras[i].SetPara(true, "Population", new IntTestRange(10, 50, 10, 10));
				//paras[i].SetPara(true, "Population", new ArrayRange((object)10, 50, 100));
				//var range = new IntTestRange(10, 40, 10);
				//paras[i].SetPara(true, "TargetNum", range);
				//paras[i].SetPara(true, "DecoyNum", new RelevantSerialRange(range, 1, (o, j) => (j + 2) * (int)o));
				paras[i].SetValue(true, "Population", 50);
				paras[i].SetValue(true, "TargetNum", 20);
				paras[i].SetValue(true, "DecoyNum", 80);
			}

			var test = new MinimalTest(full ? 20 : 10, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			TestOptions option = new TestOptions(full ? 20 : 10, "pavoid", Threads, 6);
			test.TestCompare("avoid-pleave", option, paras);
		}

        //假目标躲避策略的快速测试：2个算法、不测试Cooperate策略中的穿越概率PLeave
		static void CompareCooperateQuick()
		{
			ExperimentTest[] paras = new ExperimentTest[2];
			paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
                //数组的每个元素是属性"Cooperate"的一次取值，一般的参数是最大最小值的区间遍历，此处是数组遍历（支持相应的Step方法）
				paras[i].SetPara(false, "Cooperate", new ArrayRange(true, false));
				paras[i].SetValue(true, "Population", 50);
				paras[i].SetValue(true, "TargetNum", 20);
				paras[i].SetValue(true, "DecoyNum", 80);
				paras[i].SetValue(true, "InterferenceNum", 0);
			}

			var test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			TestOptions option = new TestOptions(20, "pavoid", Threads, 6);
			test.TestCompare("Compare-Decoy-Small", option, paras);
		}

        //假目标躲避策略的大规模测试：2个算法，参数矩阵，种群、真目标、假目标较大范围取值以进行测试
		static void CompareCooperateLarge(bool full = false, bool hasRand = false)
		{
			ExperimentTest[] paras = new ExperimentTest[2];
			paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
			//paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));
			//ExperimentTest[] paras = new ExperimentTest[1];
			//paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));

			object[,] @params = new object[12, 2];
			for (int i = 0; i < 11; i++)
			{
				@params[i, 0] = true;
				@params[i, 1] = i / 10f;
			}
			@params[11, 0] = false;
			@params[11, 1] = 1;

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(false, "HasRandomStage", hasRand);
                //根据full选择矩阵or数组（是否测试PLeave）
				if (full)
					paras[i].SetPara(false, new MatrixMultiRange(@params), "Cooperate", "PLeave");
				else
					paras[i].SetPara(false, "Cooperate", new ArrayRange(true, false));

                //这里的较大范围测试只是选取了3个特定值（不是类似IntTestRange的Step变化）
				paras[i].SetPara(true, "Population", new ArrayRange((object)10, 50, 100));
				//var range = new IntTestRange(10, 40, 10);
				//paras[i].SetPara(true, "TargetNum", range);
				//paras[i].SetPara(true, "DecoyNum", new RelevantSerialRange(range, 3, (o, j) => (j + 1) * (int)o));
				paras[i].SetPara(true, "TargetNum", new ArrayRange((object)1, 20, 50));
				paras[i].SetPara(true, "DecoyNum", new IntTestRange(0, 200, 20));
			}

			var test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			TestOptions option = new TestOptions(20, "pavoid", Threads, 6);
			test.TestCompare("Compare-Decoy-Large", option, paras);
		}

        //抗干扰效果测试：关闭“随机搜索阶段”，设置真目标与干扰源的范围参数，按目标收集率进行结果统计
		static void CompareInterference(bool hasRand = false)
		{
			ExperimentTest[] paras = new ExperimentTest[3];
			paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));
			paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(true, "DecoyNum", 0);
				paras[i].SetValue(false, "HasRandomStage", hasRand);
				paras[i].SetPara(true, "Population", new ArrayRange((object)10, 50, 100));
				//var range = new IntTestRange(10, 100, 10);
				//paras[i].SetPara(true, "TargetNum", range);
				//paras[i].SetPara(true, "InterferenceNum", new RelevantSerialRange(range, 3, (o, j) => (j + 3) * (int)o));
				paras[i].SetPara(true, "TargetNum", new IntTestRange(10, 100, 10));
				//paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100, 200, 300, 500, 800));
				paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 50, 100, 150));
			}

			var test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			TestOptions option = new TestOptions(20, "inter", Threads, 6);
			test.TestCompare("Compare-Inter", option, paras);
		}

		//static void CompareTar(int size = 1000, int steps = 6)
		//{
		//    ExperimentTest[] paras = new ExperimentTest[3];

		//    for (int i = 0; i < paras.Length; i++)
		//    {
		//        paras[i] = new ExperimentTest(typeof(PMinimal));
		//        paras[i].SetValue(true, "Size", size);
		//        paras[i].SetValue(true, "DecoyNum", 0);
		//        paras[i].SetValue(true, "CollectDecoy", true);
		//    }

		//    paras[0].AlgorithmType = typeof(AGESFitness);
		//    paras[1].AlgorithmType = typeof(AGES2Fitness);
		//    paras[2].AlgorithmType = typeof(ARPSOFitness);

		//    //Set Environment Params
		//    for (int i = 0; i < paras.Length; i++)
		//    {
		//        paras[i].SetPara(true, "Population", new IntTestRange(10, 50, 10, 5));
		//        paras[i].SetPara(true, "TargetNum", new IntTestRange(10, 100, 10));
		//    }

		//    var test = new MinimalTest(20, size * 10, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
		//    TestOptions option = new TestOptions(20, "Fitness", Environment.ProcessorCount > 16 ? 16 : 3, steps);
		//    test.TestCompare("Compare-Tar-" + steps + "-" + size, option, paras);
		//}

		//static void CompareDecoy1(int size = 1000, int steps = 6, bool CollectDecoy = false)
		//{
		//    ExperimentTest[] paras = new ExperimentTest[CollectDecoy ? 3 : 6];

		//    for (int i = 0; i < paras.Length; i++)
		//    {
		//        paras[i] = new ExperimentTest(typeof(PMinimal));
		//        paras[i].SetValue(true, "Size", size);
		//        paras[i].SetValue(true, "TargetNum", 1);
		//        paras[i].SetValue(true, "CollectDecoy", CollectDecoy);
		//    }

		//    if (CollectDecoy)
		//    {
		//        paras[0].AlgorithmType = typeof(AGESFitness);
		//        paras[1].AlgorithmType = typeof(AGES2Fitness);
		//        paras[2].AlgorithmType = typeof(ARPSOFitness);
		//    }
		//    else
		//    {
		//        paras[0].AlgorithmType = typeof(AGESFitness);
		//        paras[1].AlgorithmType = typeof(AGESFitness);
		//        paras[1].SetValue(false, "Cooperate", false);
		//        paras[2].AlgorithmType = typeof(AGES2Fitness);
		//        paras[3].AlgorithmType = typeof(AGES2Fitness);
		//        paras[3].SetValue(false, "Cooperate", false);
		//        paras[4].AlgorithmType = typeof(ARPSOFitness);
		//        paras[5].AlgorithmType = typeof(ARPSOFitness);
		//        paras[5].SetValue(false, "Cooperate", false);
		//    }

		//    //Set Environment Params
		//    for (int i = 0; i < paras.Length; i++)
		//    {
		//        paras[i].SetPara(true, "Population", new IntTestRange(10, 30, 10, 10));
		//        paras[i].SetPara(true, "DecoyNum", new IntTestRange(10, 40, 10));
		//    }

		//    var test = new MinimalTest(20, size * 10, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
		//    TestOptions option = new TestOptions(20, "Fitness", Environment.ProcessorCount > 16 ? 16 : 3, steps);
		//    test.TestCompare((CollectDecoy ? "Compare-Collect1-" : "Compare-Decoy1-") + steps + "-" + size, option, paras);
		//}

		//static void CompareDecoy(int size = 1000, int steps = 6, bool CollectDecoy = false)
		//{
		//    ExperimentTest[] paras = new ExperimentTest[CollectDecoy ? 3 : 6];

		//    for (int i = 0; i < paras.Length; i++)
		//    {
		//        paras[i] = new ExperimentTest(typeof(PMinimal));
		//        paras[i].SetValue(true, "Size", size);
		//        paras[i].SetValue(true, "CollectDecoy", CollectDecoy);
		//    }

		//    if (CollectDecoy)
		//    {
		//        paras[0].AlgorithmType = typeof(AGESFitness);
		//        paras[1].AlgorithmType = typeof(AGES2Fitness);
		//        paras[2].AlgorithmType = typeof(ARPSOFitness);
		//    }
		//    else
		//    {
		//        paras[0].AlgorithmType = typeof(AGESFitness);
		//        paras[1].AlgorithmType = typeof(AGESFitness);
		//        paras[1].SetValue(false, "Cooperate", false);
		//        paras[2].AlgorithmType = typeof(AGES2Fitness);
		//        paras[3].AlgorithmType = typeof(AGES2Fitness);
		//        paras[3].SetValue(false, "Cooperate", false);
		//        paras[4].AlgorithmType = typeof(ARPSOFitness);
		//        paras[5].AlgorithmType = typeof(ARPSOFitness);
		//        paras[5].SetValue(false, "Cooperate", false);
		//    }

		//    //Set Environment Params
		//    for (int i = 0; i < paras.Length; i++)
		//    {
		//        paras[i].SetPara(true, "Population", new IntTestRange(10, 30, 10, 10));
		//        var range = new IntTestRange(10, 40, 10);
		//        paras[i].SetPara(true, "TargetNum", range);
		//        //paras[i].SetPara(true, "DecoyNum", new RelevantRange(range, o => 2 * (int)o));
		//        paras[i].SetPara(true, "DecoyNum", new RelevantSerialRange(range, 2, (o, j) => (j + 2) * (int)o));
		//    }

		//    var test = new MinimalTest(20, size * 10, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
		//    TestOptions option = new TestOptions(20, "Fitness", Environment.ProcessorCount > 16 ? 16 : 3, steps);
		//    test.TestCompare((CollectDecoy ? "Compare-Collect-" : "Compare-Decoy-") + steps + "-" + size, option, paras);
		//}

        /// <summary>
        /// 单纯的避障效果对比（障碍物数目0-500按步变化）：无干扰源、无假目标，按目标收集率进行结果统计
        /// 关闭了“算法框架”的“随机搜索阶段”
        /// </summary>
		static void CompareObstacle(int size = 1000, int steps = 6, int interference = 0, int decoy = 0, bool full = true, bool hasRand = false)
		{
            //创建“实验测试”对象列表，问题为“网格适应度问题”，算法可选择IGES
			ExperimentTest[] paras = new ExperimentTest[full ? 3 : 2];
			paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
			if (full) paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));

            //将各测试对象的<属性,值>存入相应“实验测试”对象的字典
			for (int i = 0; i < paras.Length; i++)
			{
                //存储“问题”的属性值（true）到值参数字典
				paras[i].SetValue(true, "Size", size);
				paras[i].SetValue(true, "DecoyNum", decoy);
				paras[i].SetValue(true, "InterferenceNum", interference);
				//paras[i].SetPara(true, "Population", new IntTestRange(10, 20, 10, 2));
				//paras[i].SetPara(true, "TargetNum", new IntTestRange(10, 40, 2));
				paras[i].SetValue(true, "Population", decoy > 0 ? 50 : 30);
				paras[i].SetValue(true, "TargetNum", 30 - decoy);
				//paras[i].SetPara(true, "ObstacleNum", new IntTestRange(0, 100, 10));
                //存储“算法”的属性值（false）到值参数字典
                paras[i].SetValue(false, "HasRandomStage", hasRand);
                //存储“问题”的属性值（true）到范围参数列表
				paras[i].SetPara(true, "ObstacleNum", new IntTestRange(0, 500, 50));
			}
            //创建基于SMinimal状态的“测试主体”，设置重复次数、最大迭代次数、目标收集率
            //将SMinimal的类型信息存入RunState的静态字典，将SMinimal的字段名称的逗号分隔串赋给“测试主体”的Title
			var test = new MinimalTest(20, size * 10, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
            //创建“测试选项”，设置线程数、关键的测试步数steps（不同的目标收集率）
            //将地图种子读入“测试选项”的seeds数组or重新创建seeds数组与种子文件Fitness.seed
			TestOptions option = new TestOptions(20, "Fitness", Threads, steps);
            //对比测试：利用“实验测试”列表与“测试选项”执行实验，并将结果写入文件
			test.TestCompare(string.Format("Compare-Obs-{0}-{1}I{2}D{3}", steps, size, interference, decoy), option, paras);
		}

        //完全测试：4个算法（包括DS算法），
		static void CompareAll()
		{
			ExperimentTest[] paras = new ExperimentTest[4];
			paras[0] = new ExperimentTest(typeof(PMinimalDis), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalDis), typeof(ARPSOFitness));
			paras[2] = new ExperimentTest(typeof(PMinimalDis), typeof(AIGESFitness));
			paras[3] = new ExperimentTest(typeof(PMinimalDis), typeof(ADSFitness));

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(false, "Cooperate", true);
				paras[i].SetValue(true, "DecoyNum", 60);
				//paras[i].SetPara(true, "SizeZ", new ArrayRange(1, 400));
				paras[i].SetPara(true, "Population", new IntTestRange(10, 100, 30, 10));

                //CombineRange的目的是将真目标的个数取值1与[10,100]组合起来
				paras[i].SetPara(true, "TargetNum", new CombineRange(1, new ArrayRange(1), new IntTestRange(10, 100, 30, 10)));
				paras[i].SetPara(true, "ObstacleNum", new IntTestRange(0, 500, 300, 50));
				paras[i].SetPara(true, "InterferenceNum", new IntTestRange(0, 100, 50, 10));
				paras[i].SetPara(true, "MaximumTargetFitness", new IntTestRange(5, 20, 10, 1));

                //最后一个参数@base：生成的整数值除以@base可生成所需的float数据
				paras[i].SetPara(null, "NoisePercent", new FloatTestRange(0, 10, 1, 10));
			}

			var test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			TestOptions option = new TestOptions(20, "all", Threads, 6, false);
//			test.TestCompare("Compare-All", option, paras);

			paras = new ExperimentTest[4];
			paras[0] = new ExperimentTest(typeof(PMinimalDis), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalDis), typeof(ARPSOFitness));
			paras[2] = new ExperimentTest(typeof(PMinimalDis), typeof(AIGESFitness));
			paras[3] = new ExperimentTest(typeof(PMinimalDis), typeof(ADSFitness));

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(true, "MaximumTargetFitness", 10);
				paras[i].SetValue(false, "Cooperate", true);
				paras[i].SetValue(true, "DecoyNum", 60);
				paras[i].SetValue(true, "Population", 30);
				paras[i].SetValue(true, "TargetNum", 30);
				paras[i].SetValue(true, "ObstacleNum", 300);
				paras[i].SetValue(true, "InterferenceNum", 50);
				paras[i].SetPara(null, "NoisePercent", new FloatTestRange(0, 10, 1, 5, 10));
			}

			test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			option = new TestOptions(20, "all", Threads, 6, false);
			test.TestCompare("Compare-All-Noise", option, paras);

			paras = new ExperimentTest[4];
			paras[0] = new ExperimentTest(typeof(PMinimalDis), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PMinimalDis), typeof(ARPSOFitness));
			paras[2] = new ExperimentTest(typeof(PMinimalDis), typeof(AIGESFitness));
			paras[3] = new ExperimentTest(typeof(PMinimalDis), typeof(ADSFitness));

			//Set Environment Params
			for (int i = 0; i < paras.Length; i++)
			{
				paras[i].SetValue(true, "MaximumTargetFitness", 10);
				paras[i].SetValue(true, "Population", 30);
				paras[i].SetValue(true, "TargetNum", 30);
				paras[i].SetValue(true, "ObstacleNum", 300);
				paras[i].SetValue(true, "InterferenceNum", 50);

				paras[i].SetPara(false, "Cooperate", new ArrayRange(true, false));
				paras[i].SetPara(true, "DecoyNum", new IntTestRange(0, 200, 60, 20));
			}

			test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
			option = new TestOptions(20, "all", Threads, 6);
			//test.TestCompare("Compare-All-decoy", option, paras);
		}

        //用于测试能量限制问题
		static void CompareAllECS()
		{
            //创建实验的参数对象
			ExperimentTest[] paras = new ExperimentTest[4];
			paras[0] = new ExperimentTest(typeof(PEnergy), typeof(AGESFitness));
			paras[1] = new ExperimentTest(typeof(PEnergy), typeof(ARPSOFitness));
			paras[2] = new ExperimentTest(typeof(PEnergy), typeof(AIGESFitness));
			paras[3] = new ExperimentTest(typeof(PEnergy), typeof(ADSFitness));

            //设置相应的属性值
			for (int i = 0; i < paras.Length; i++)
			{
				//paras[i].SetValue(false, "Cooperate", true);
				//paras[i].SetValue(true, "DecoyNum", 60);
				//paras[i].SetValue(true, "MaximumTargetFitness", 10);
				paras[i].SetValue(true, "Population", 30);
				paras[i].SetValue(true, "TargetNum", 30);
				paras[i].SetValue(true, "ObstacleNum", 300);
				//paras[i].SetValue(true, "InterferenceNum", 50);
				paras[i].SetValue(true, "EnergyMode", true);
			}
            //创建测试主体对象，读取测试状态的字段列表，参数分别为：重复次数、最大迭代次数、目标收集比率
			var test = new MinimalTest(20, 10000, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f);
            //创建测试选项，地图数、地图名、线程数、steps？grid？
			TestOptions option = new TestOptions(20, "all", Threads, 6, false);
            //执行实验并将结果写入文件"Compare-All-ESC.csv"
			test.TestCompare("Compare-All-ECS", option, paras);
		}
        static void ReRunP67() {
            //CompareParam(CollectDecoy: null);
            CompareInterference();
            CompareObstacle(interference: 50);
            CompareCooperateLarge();
            CompareCooperate(true);
        }

		static void PhDThesis()
		{
			//CompareObstacle();
            //OptimizeParam();
			//CompareInterference();
			//CompareAll();
			//CompareAllECS();
			//CompareCooperateLarge();
			//CompareObstacle(decoy: 20);
			//CompareObstacle(interference: 100);
			//ReRunP67();
			CompareCooperate(true);
		}

        static void SingleTest(bool hasRand = false)
        {
            ExperimentTest[] paras = new ExperimentTest[4];//11
            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP0));
            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP1));
            paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP2));
            //paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP3));
            //paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP4));
            //paras[5] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP5));
            //paras[6] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP6));
            //paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP7));
            //paras[8] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP8));
            //paras[9] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP9));
            //paras[10] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));

            for (int i = 0; i < paras.Length; i++)
            {

                paras[i].SetValue(true, "DecoyNum", 0);
                paras[i].SetValue(false, "HasRandomStage", hasRand);

                paras[i].SetValue(true, "Population", 50);
                //paras[i].SetPara(true, "Population", new ArrayRange((object)25,50,75,100,125,150,175,200));


                //var range = new IntTestRange(10, 100, 10);
                //paras[i].SetPara(true, "TargetNum", range);
                //paras[i].SetPara(true, "InterferenceNum", new RelevantSerialRange(range, 3, (o, j) => (j + 3) * (int)o));

                paras[i].SetValue(true, "TargetNum", 10);
                //paras[i].SetPara(true, "TargetNum", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));
                //paras[i].SetPara(true, "TargetNum", new IntTestRange(1, 15, 1));
                //paras[i].SetPara(true, "TargetSize", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));


                //paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100, 200, 300, 500, 800));
                paras[i].SetValue(true, "InterferenceNum", 0);
            }
            var test = new MinimalTest(25, 1000, 1f);
            //option.step与上面收集率的个数是一致的，此处取值为6
            TestOptions option = new TestOptions(40, "singleTest", Threads-3, 1);
            test.TestCompare("Single-Test", option, paras);
        }


        //简单环境测试：无障碍物、无假目标、无干扰源，地图1000*1000，机器人数量50
        static void CompareSearch(bool hasRand = false) 
        {
            ExperimentTest[] paras = new ExperimentTest[8];
            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
            //            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));

            paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));  
            paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));
            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(AIntermittent));
       
            paras[5] = new ExperimentTest(typeof(PMinimalMap), typeof(AFormationFitness));
            paras[6] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(AdaPSOFitness));
            //paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(tristatePFSMFitness));

            //paras[8] = new ExperimentTest(typeof(PMinimalMap), typeof(ARandomFitness));

////            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));       
            //Set Environment Params
            for (int i = 0; i < paras.Length; i++)
            {
                paras[i].SetValue(true, "DecoyNum", 0);
                paras[i].SetValue(false, "HasRandomStage", hasRand);

                //paras[i].SetValue(true, "Population", 50);
                paras[i].SetPara(true, "Population", new ArrayRange((object)25,50,75,100,125,150,175,200));


                //var range = new IntTestRange(10, 100, 10);
                //paras[i].SetPara(true, "TargetNum", range);
                //paras[i].SetPara(true, "InterferenceNum", new RelevantSerialRange(range, 3, (o, j) => (j + 3) * (int)o));

                paras[i].SetValue(true, "TargetNum", 10);
                //paras[i].SetPara(true, "TargetNum", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));
                //paras[i].SetPara(true, "TargetNum", new IntTestRange(1, 15, 1));
                //paras[i].SetPara(true, "TargetSize", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));


                //paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100, 200, 300, 500, 800));
                paras[i].SetValue(true, "InterferenceNum", 0);
            }
            //var test = new MinimalTest(30, 1000, 0.5f, 1f);
            ////option.step与上面收集率的个数是一致的，此处取值为6
            //TestOptions option = new TestOptions(120, "search", Threads, 2);

            var test = new MinimalTest(25, 1000, 0.5f, 1f);
            //option.step与上面收集率的个数是一致的，此处取值为6
            TestOptions option = new TestOptions(40, "singleTest", Threads, 2);
            test.TestCompare("Compare-Search-swarm", option, paras);
        }

        //简单环境测试：无障碍物、无假目标、无干扰源，地图1000*1000，机器人数量50
        static void CompareSearch1(bool hasRand = false)
        {
            ExperimentTest[] paras = new ExperimentTest[8];
            //paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
            ////            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
            //paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));

            //paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));
            ////paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));
            ////paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(AIntermittent));
            //paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP0));
            //paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP2));

            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
            paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));
            paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));
            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP0));
            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP2));

            paras[5] = new ExperimentTest(typeof(PMinimalMap), typeof(AFormationFitness));
            paras[6] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(AdaPSOFitness));
            //paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(tristatePFSMFitness));

            //paras[8] = new ExperimentTest(typeof(PMinimalMap), typeof(ARandomFitness));

            ////            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));       
            //Set Environment Params
            for (int i = 0; i < paras.Length; i++)
            {
                paras[i].SetValue(true, "DecoyNum", 0);
                paras[i].SetValue(false, "HasRandomStage", hasRand);

                //paras[i].SetValue(true, "Population", 50);
                paras[i].SetPara(true, "Population", new ArrayRange((object)25, 50, 75, 100, 125, 150, 175, 200));

                //障碍物
                paras[i].SetValue(true, "ObstacleNum", 500);

                //var range = new IntTestRange(10, 100, 10);
                //paras[i].SetPara(true, "TargetNum", range);
                //paras[i].SetPara(true, "InterferenceNum", new RelevantSerialRange(range, 3, (o, j) => (j + 3) * (int)o));

                paras[i].SetValue(true, "TargetNum", 10);
                //paras[i].SetPara(true, "TargetNum", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));
                //paras[i].SetPara(true, "TargetNum", new IntTestRange(1, 15, 1));
                //paras[i].SetPara(true, "TargetSize", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));


                //paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100, 200, 300, 500, 800));

                //干扰源
                //paras[i].SetValue(true, "InterferenceNum", 10);
                paras[i].SetValue(true, "InterferenceNum", 0);
            }
            //var test = new MinimalTest(30, 1000, 0.5f, 1f);
            ////option.step与上面收集率的个数是一致的，此处取值为6
            //TestOptions option = new TestOptions(120, "search", Threads, 2);

            var test = new MinimalTest(25, 1500, 0.5f, 1f);
            //option.step与上面收集率的个数是一致的，此处取值为6
            TestOptions option = new TestOptions(40, "singleTest", Threads-5, 2); //Threads-5
            test.TestCompare("Compare-swarm-obs", option, paras); //Compare-swarm-inter
        }

        //简单环境测试：无障碍物、无假目标、无干扰源，地图1000*1000，机器人数量50
        static void CompareSearch2(bool hasRand = false)
        {
            ExperimentTest[] paras = new ExperimentTest[8];
            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
            //            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));

            paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));
            //paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));
            //paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(AIntermittent));
            paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP0));


            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP2));

            paras[5] = new ExperimentTest(typeof(PMinimalMap), typeof(AFormationFitness));
            paras[6] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(AdaPSOFitness));
            //paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(tristatePFSMFitness));

            //paras[8] = new ExperimentTest(typeof(PMinimalMap), typeof(ARandomFitness));

            ////            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));       
            //Set Environment Params
            for (int i = 0; i < paras.Length; i++)
            {
                paras[i].SetValue(true, "DecoyNum", 0);
                paras[i].SetValue(false, "HasRandomStage", hasRand);

                paras[i].SetValue(true, "Population", 50);
                //paras[i].SetPara(true, "Population", new ArrayRange((object)25, 50, 75, 100, 125, 150, 175, 200));


                //var range = new IntTestRange(10, 100, 10);
                //paras[i].SetPara(true, "TargetNum", range);
                //paras[i].SetPara(true, "InterferenceNum", new RelevantSerialRange(range, 3, (o, j) => (j + 3) * (int)o));

                //paras[i].SetValue(true, "TargetNum", 10);
                paras[i].SetPara(true, "TargetNum", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));
                //paras[i].SetPara(true, "TargetNum", new IntTestRange(1, 15, 1));
                //paras[i].SetPara(true, "TargetSize", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));


                //paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100, 200, 300, 500, 800));
                paras[i].SetValue(true, "InterferenceNum", 0);
            }
            //var test = new MinimalTest(30, 1000, 0.5f, 1f);
            ////option.step与上面收集率的个数是一致的，此处取值为6
            //TestOptions option = new TestOptions(120, "search", Threads, 2);

            var test = new MinimalTest(25, 1000, 0.5f, 1f);
            //option.step与上面收集率的个数是一致的，此处取值为6
            TestOptions option = new TestOptions(40, "singleTest", Threads-5, 2);
            test.TestCompare("Compare-targets", option, paras);
        }

        //简单环境测试：无障碍物、无假目标、无干扰源，地图1000*1000，机器人数量50
        static void CompareSearch3(bool hasRand = false)
        {
            ExperimentTest[] paras = new ExperimentTest[8];
            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
            //            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AGESFitness));
            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));

            paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));
            //paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));
            //paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(AIntermittent));
            paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP0));
            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(MLP2));

            paras[5] = new ExperimentTest(typeof(PMinimalMap), typeof(AFormationFitness));
            paras[6] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(AdaPSOFitness));
            //paras[7] = new ExperimentTest(typeof(PMinimalMap), typeof(tristatePFSMFitness));

            //paras[8] = new ExperimentTest(typeof(PMinimalMap), typeof(ARandomFitness));

            ////            paras[4] = new ExperimentTest(typeof(PMinimalMap), typeof(ALevyFlightFitness));       
            //Set Environment Params
            for (int i = 0; i < paras.Length; i++)
            {
                paras[i].SetValue(true, "DecoyNum", 0);
                paras[i].SetValue(false, "HasRandomStage", hasRand);

                paras[i].SetValue(true, "Population", 50);
                //paras[i].SetPara(true, "Population", new ArrayRange((object)25, 50, 75, 100, 125, 150, 175, 200));


                //var range = new IntTestRange(10, 100, 10);
                //paras[i].SetPara(true, "TargetNum", range);
                //paras[i].SetPara(true, "InterferenceNum", new RelevantSerialRange(range, 3, (o, j) => (j + 3) * (int)o));

                paras[i].SetValue(true, "TargetNum", 10);
                //paras[i].SetPara(true, "TargetNum", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));
                //paras[i].SetPara(true, "TargetNum", new IntTestRange(1, 15, 1));
                paras[i].SetPara(true, "TargetSize", new ArrayRange((object)1, 5, 10, 15, 20, 30, 40, 50));


                //paras[i].SetPara(true, "InterferenceNum", new ArrayRange((object)0, 100, 200, 300, 500, 800));
                paras[i].SetValue(true, "InterferenceNum", 0);
            }
            //var test = new MinimalTest(30, 1000, 0.5f, 1f);
            ////option.step与上面收集率的个数是一致的，此处取值为6
            //TestOptions option = new TestOptions(120, "search", Threads, 2);

            var test = new MinimalTest(25, 1000, 0.5f, 1f);
            //option.step与上面收集率的个数是一致的，此处取值为6
            TestOptions option = new TestOptions(40, "singleTest", Threads-5, 2);
            test.TestCompare("Compare-time", option, paras);
        }


        static void ObstacleAvoide() 
        {
            int size = 1000;
            //创建“实验测试”对象列表，问题为“网格适应度问题”，算法可选择IGES
            ExperimentTest[] paras = new ExperimentTest[4];

            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ARPSOFitness));
            paras[1] = new ExperimentTest(typeof(PMinimalMap), typeof(AFormationFitness));
            paras[2] = new ExperimentTest(typeof(PMinimalMap), typeof(AIGESFitness));
            paras[3] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));

            //将各测试对象的<属性,值>存入相应“实验测试”对象的字典
            for (int i = 0; i < paras.Length; i++)
            {
                //存储“问题”的属性值（true）到值参数字典
                paras[i].SetValue(true, "Size", size);
                //存储“问题”的属性值（true）到范围参数列表
                paras[i].SetPara(true, "ObstacleNum", new IntTestRange(0, 500, 50));
            }
            //创建基于SMinimal状态的“测试主体”，设置重复次数、最大迭代次数、目标收集率
            //将SMinimal的类型信息存入RunState的静态字典，将SMinimal的字段名称的逗号分隔串赋给“测试主体”的Title
            var test = new MinimalTest(20, size * 10, 0.5f, 1f);
            //创建“测试选项”，设置线程数、关键的测试步数steps（不同的目标收集率）
            //将地图种子读入“测试选项”的seeds数组or重新创建seeds数组与种子文件Fitness.seed
            TestOptions option = new TestOptions(20, "Fitness", Threads, 2);
            //对比测试：利用“实验测试”列表与“测试选项”执行实验，并将结果写入文件
            test.TestCompare(string.Format("Obs-Avoide-{0}-{1}", 2, size), option, paras);
        }

        static void DataGenerate() {
            int size = 1000;
            ExperimentTest[] paras = new ExperimentTest[1];
            //paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(APGESFitness));
            //paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(ASinglySearch));
            paras[0] = new ExperimentTest(typeof(PMinimalMap), typeof(AdaPSOFitness));
            //参数：每幅地图重复实验次数（设为1）、最大迭代次数、收集率数组
            var test = new MinimalTest(1, size * 10, 1f);
            //创建“测试选项”，设置线程数、关键的测试步数steps（不同的目标收集率）
            //将地图种子读入“测试选项”的seeds数组or重新创建seeds数组与种子文件Fitness.seed
            TestOptions option = new TestOptions(1200000, "ANN", 8);  //16



            //对比测试：利用“实验测试”列表与“测试选项”执行实验，并将结果写入文件
            test.TestData1(option, paras);
        }

        //static void EvolveFrame(bool file) {
        //    int sizeM = 15;
        //    int N = 557;
        //    Random rnd = new Random();
        //    Matrix<double> xmean = new Matrix<double>(N, 1);
        //    Matrix<double> xbest = new Matrix<double>(N, 1);

        //    if (file)
        //    {
        //        StreamReader sr = new StreamReader("w1552.txt");
        //        int k = 0;
        //        for (int i = 0; i < sizeM; i++)
        //        {
        //            string line = sr.ReadLine();
        //            string[] abc = line.Split('\t');
        //            for (int j = 0; j < 30; j++, k++) xmean[k,0] = double.Parse(abc[j]);
        //        }
        //        for (int i = 0; i < 5; i++)
        //        {
        //            string line = sr.ReadLine();
        //            string[] abc = line.Split('\t');
        //            for (int j = 0; j < sizeM; j++, k++) xmean[k,0] = double.Parse(abc[j]);
        //        }
        //        for (int i = 0; i < 2; i++)
        //        {
        //            string line = sr.ReadLine();
        //            string[] abc = line.Split('\t');
        //            for (int j = 0; j < 5; j++, k++) xmean[k,0] = double.Parse(abc[j]);
        //        }
        //        for (int i = 0; i < 1; i++)
        //        {
        //            string line = sr.ReadLine();
        //            string[] abc = line.Split('\t');
        //            for (int j = 0; j < 7 + sizeM; j++, k++) xmean[k,0] = double.Parse(abc[j]);

        //        }
        //        sr.Close();              
        //    }
        //    else
        //    {
        //        for (int i = 0; i < N; i++) xmean[i, 0] = rnd.NextDouble();
        //    }
        //    //调用进化算法
        //    CMA_ES(ref xmean, ref xbest);

        //    StreamWriter sw = new StreamWriter("ww1552.txt");
        //    int m = 0;
        //    for (int i = 0; i < sizeM; i++)
        //    {
        //        for (int j = 0; j < 30; j++, m++) sw.Write("{0}\t", xmean[m, 0]);
        //        sw.WriteLine();
        //    }
        //    for (int i = 0; i < 5; i++)
        //    {
        //        for(int j = 0; j < sizeM; j++, m++) sw.Write("{0}\t", xmean[m,0]);
        //        sw.WriteLine();
        //    }
        //    for (int i = 0; i < 2; i++)
        //    {
        //        for (int j = 0; j < 5; j++, m++) sw.Write("{0}\t", xmean[m, 0]);
        //        sw.WriteLine();
        //    }
        //    for (int j = 0; j < 7 + sizeM; j++, m++) sw.Write("{0}\t", xmean[m, 0]);
        //    sw.WriteLine();
        //    sw.Close();

        //    sw = new StreamWriter("bw1552.txt");
        //    m = 0;
        //    for (int i = 0; i < sizeM; i++)
        //    {
        //        for (int j = 0; j < 30; j++, m++) sw.Write("{0}\t", xbest[m, 0]);
        //        sw.WriteLine();
        //    }
        //    for (int i = 0; i < 5; i++)
        //    {
        //        for (int j = 0; j < sizeM; j++, m++) sw.Write("{0}\t", xbest[m, 0]);
        //        sw.WriteLine();
        //    }
        //    for (int i = 0; i < 2; i++)
        //    {
        //        for (int j = 0; j < 5; j++, m++) sw.Write("{0}\t", xbest[m, 0]);
        //        sw.WriteLine();
        //    }
        //    for (int j = 0; j < 7 + sizeM; j++, m++) sw.Write("{0}\t", xbest[m, 0]);
        //    sw.WriteLine();
        //    sw.Close();

        //}

        static void EvolveFrame(bool file)
        {
            //step1
            //int N = 128;
            int sizeM = 129;
            int N = 258;

            Random rnd = new Random();
            Matrix<double> xmean = new Matrix<double>(N, 1);
            Matrix<double> xbest = new Matrix<double>(N, 1);

            if (file)
            {
                StreamReader sr = new StreamReader("MLPX.txt");

                //step2
                //string line = sr.ReadLine();
                //string[] es = line.Split(' ');
                //for (int i = 0; i < N; i++) xmean[i, 0] = double.Parse(es[i]);

                int k = 0;
                for (int i = 0; i < sizeM; i++)
                {
                    string line = sr.ReadLine();
                    string[] abc = line.Split(' ');
                    for (int j = 0; j < 2; j++, k++) xmean[k, 0] = double.Parse(abc[j]);
                }

                sr.Close();
            }
            else
            {
                //for (int i = 0; i < N; i++) xmean[i, 0] = rnd.NextDouble();
                CustomRandom initRand = new CustomRandom();
                for (int i = 0; i < N - 2; i++)
                {
                    xmean[i, 0] = initRand.NextGaussian();
                    xmean[i, 0] *= Math.Sqrt(1.0 / (sizeM - 1));
                    if (xmean[i, 0] > 1.0) xmean[i,0] = 1.0;
                    else if (xmean[i,0] < -1.0) xmean[i,0] = -1.0;
                }
                xmean[N - 2, 0] = 0.0;
                xmean[N - 1, 0] = 0.0;             
            }
            

            //调用进化算法
            CMA_ES(ref xmean, ref xbest);
            //GFWA(ref xmean, ref xbest);

            StreamWriter sw = new StreamWriter("MMLPX.txt");
            //step3
            //for (int i = 0; i < N; i++) sw.Write("{0} ", xmean[i, 0]);
            //sw.WriteLine();
            int m = 0;
            for (int i = 0; i < sizeM; i++)
            {
                for (int j = 0; j < 2; j++, m++) sw.Write("{0} ", xmean[m, 0]);
                sw.WriteLine();
            }
            sw.Close();

            sw = new StreamWriter("BMLPX.txt");
            //step4
            //for (int i = 0; i < N; i++) sw.Write("{0} ", xbest[i, 0]);
            //sw.WriteLine();
            m = 0;
            for (int i = 0; i < sizeM; i++)
            {
                for (int j = 0; j < 2; j++, m++) sw.Write("{0} ", xbest[m, 0]);
                sw.WriteLine();
            }
            sw.Close();

        }

        //static void Feval(ref Matrix<double> arx, double[,] fitness, int lambda) {
        //    int sizeM = 15;
        //    ExperimentTest[] paras = new ExperimentTest[lambda];
        //    for (int i = 0; i < lambda; i++) paras[i] = new ExperimentTest(typeof(PMinimalMap), typeof(ER));
        //    for (int i = 0; i < lambda; i++)
        //    {
        //        double[,] wiArray = new double[sizeM, 30];
        //        double[,] wmArray = new double[5, sizeM];
        //        double[,] woArray = new double[2, 5];
        //        double[,] offsetArray = new double[7 + sizeM, 1];
        //        int m = 0;
        //        for (int j = 0; j < sizeM; j++)
        //            for (int k = 0; k < 30; k++, m++) wiArray[j, k] = arx[m, i];
        //        for (int j = 0; j < 5; j++)
        //            for (int k = 0; k < sizeM; k++, m++) wmArray[j, k] = arx[m, i];
        //        for (int j = 0; j < 2; j++)
        //            for (int k = 0; k < 5; k++, m++) woArray[j, k] = arx[m, i];
        //        for (int k = 0; k < 7 + sizeM; k++, m++) offsetArray[k, 0] = arx[m, i];
        //        paras[i].SetValue(false, "WI", wiArray);
        //        paras[i].SetValue(false, "WM", wmArray);
        //        paras[i].SetValue(false, "WO", woArray);
        //        paras[i].SetValue(false, "OFFSET", offsetArray);
        //        paras[i].SetValue(false, "FF", true);
        //    }
        //    var test = new MinimalTest(1, 300, 1f);
        //    TestOptions option = new TestOptions(40, "ER", Threads, 0);
        //    test.TestER(option, fitness, paras);

        //}

        static void Feval(ref Matrix<double> arx, double[,] fitness, int lambda, int counteval)
        {
            int sizeM = 129;
            ExperimentTest[] paras = new ExperimentTest[lambda];
            for (int i = 0; i < lambda; i++) paras[i] = new ExperimentTest(typeof(PMinimalMap), typeof(MLPX));
            for (int i = 0; i < lambda; i++)
            {
                //step5
                //double[,] twArray = new double[128, 1];
                //for (int j = 0; j < 128; j++) twArray[j, 0] = arx[j, i];
                //paras[i].SetValue(false, "TW", twArray);

                double[,] w5Array = new double[2, sizeM - 1];
                double[,] offsetArray = new double[2, 1];
                int m = 0;

                for (int k = 0; k < sizeM - 1; ++k)
                    for (int j = 0; j < 2; j++, m++) w5Array[j, k] = arx[m, i];

                for (int j = 0; j < 2; j++, m++) offsetArray[j, 0] = arx[m, i];

                paras[i].SetValue(false, "W5", w5Array);
                paras[i].SetValue(false, "OFF5", offsetArray);

                paras[i].SetValue(false, "FF", false);
            }
            int repeatNum = 2;           
            if (counteval > 800) repeatNum = 8;
            if (counteval > 1200) repeatNum = 25;
            var test = new MinimalTest(repeatNum, 270, 1f);
            //var test = new MinimalTest(25, 270, 1f);
            TestOptions option = new TestOptions(40, "singleTest", Threads-5, 0);
            test.TestER(option, fitness, paras);

        }


        static double Feval(Matrix<double> point) {
            return 1;
        }

        static void CMA_ES(ref Matrix<double> xmean, ref Matrix<double> xbest) {
            CustomRandom rnd = new CustomRandom();
            //Input parameters
            int N = 258;

            double stopfitness = 1 * 10 ^ (-5);
            int stopeval = 100 * N;

            //Strategy parameters: Selection

            int lambda = 40;
            //double sigma = 0.0;
            double sigma = 0.03;
            //int lambda = 4 + (int)Math.Floor(3 * Math.Log(N));

            double sum = lambda / 2.0;
            int mu = (int)Math.Floor(sum);
            Matrix<double> weights = new Matrix<double>(lambda, 1);
            for (int i = 0; i < mu; i++) weights[i, 0] = Math.Log(sum + 0.5) - Math.Log(i + 1);
            weights = weights / weights.Sum;

            sum = 0;
            for (int i = 0; i < mu; i++)
            {
                sum += weights[i, 0] * weights[i, 0];
            }
            double mueff = 1 / sum;

            //Strategy parameters: Adaptation
            double cc = (4 + mueff / N) / (N + 4 + 2 * mueff / N);
            double c1 = 2 / ((N + 1.3) * (N + 1.3) + mueff);
            double cmu = 2 * (mueff - 2 + 1 / mueff) / ((N + 2) * (N + 2) + mueff);
            double cs = (mueff + 2) / (N + mueff + 5);
            sum = Math.Sqrt((mueff - 1) / (N + 1)) - 1;
            if (sum < 0) sum = 0;
            double damps = 1 + 2 * sum + cs;

            //Dynamic strategy parameters and constants
            Matrix<double> pc = new Matrix<double>(N, 1);
            Matrix<double> ps = new Matrix<double>(N, 1);
            for (int i = 0; i < N; i++) pc[i, 0] = ps[i, 0] = 0;
            Matrix<double> B = new Matrix<double>(N, N);
            Matrix<double> D = new Matrix<double>(N, N);
            Matrix<double> C = new Matrix<double>(N, N);

            //verify4-1
            Matrix<double> BD = new Matrix<double>(N, N);
            CvInvoke.cvSetIdentity(B, new Emgu.CV.Structure.MCvScalar(1.0));
            CvInvoke.cvSetIdentity(D, new Emgu.CV.Structure.MCvScalar(1.0));
            CvInvoke.cvSetIdentity(C, new Emgu.CV.Structure.MCvScalar(1.0));
            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < N; j++)
            //    {
            //        if (i == j) B[i, j] = D[i, j] = 1;
            //        else B[i, j] = D[i, j] = 0;
            //    }
            //}
            //Matrix<double> C = B * D * D * B;
            int eigeneval = 0;
            double chiN = Math.Sqrt(N) * (1 - 1.0 / (4 * N) + 1.0 / (21 * N * N));

            //Generation loop
            int counteval = 0;
            Matrix<double> arz = new Matrix<double>(N, lambda);
            Matrix<double> arx = new Matrix<double>(N, lambda);
            Matrix<double> zmean = new Matrix<double>(N, 1);
            Matrix<double> temp = new Matrix<double>(N, 1);
            Matrix<double> augmentedWeights = new Matrix<double>(lambda, 1);
            double[,] arfitness = new double[2, lambda];

            double min = 1000000;

            while (counteval < stopeval)
            {
                //verify4-2
                BD = B * D;
                //群体评估可改写为并行函数，即一次初始化Lambda个ExperimentTest对象
                for (int j = 0; j < lambda; j++)
                {
                    for (int i = 0; i < N; i++) arz[i, j] = temp[i, 0] = rnd.NextGaussian();

                    //if (counteval < 45) temp = xmean + 0 * (B * D * temp);
                    //else temp = xmean + sigma * (B * D * temp);

                    //if (counteval < 88) temp = xmean + 0.001 * (B * D * temp);
                    //else temp = xmean + sigma * (B * D * temp);

                    temp = xmean + sigma * (BD * temp);

                    for (int i = 0; i < N; i++) arx[i, j] = temp[i, 0];
                    //arfitness[0, j] = Feval(temp);
                    arfitness[1, j] = j;
                    //counteval++;
                }
                //swarm evaluation

                counteval += lambda;
                Feval(ref arx, arfitness, lambda, counteval);

                //Sort
                double value, index; int num;
                for (int j = 0; j < lambda - 1; j++)
                {
                    value = arfitness[0, j]; num = j;
                    for (int k = j + 1; k < lambda; k++)
                    {
                        if (value > arfitness[0, k])
                        {
                            value = arfitness[0, k];
                            num = k;
                        }
                    }
                    if (num != j)
                    {
                        value = arfitness[0, num];
                        index = arfitness[1, num];
                        arfitness[0, num] = arfitness[0, j];
                        arfitness[1, num] = arfitness[1, j];
                        arfitness[0, j] = value;
                        arfitness[1, j] = index;
                    }
                }
                //Calculate the augmented weights vector and xmean
                CvInvoke.cvSetZero(augmentedWeights);
                for (int j = 0; j < mu; j++) augmentedWeights[(int)arfitness[1, j], 0] = weights[j, 0];
                xmean = arx * augmentedWeights;
                zmean = arz * augmentedWeights;

                //Update evolution paths
                ps = (1 - cs) * ps + Math.Sqrt(cs * (2 - cs) * mueff) * (B * zmean);
                double hsig = 0, norm = 0;
                for (int i = 0; i < N; i++) norm += ps[i, 0] * ps[i, 0];
                norm = Math.Sqrt(norm);
                hsig = norm / Math.Sqrt(1 - Math.Pow(1 - cs, 2.0 * counteval / lambda));
                if (hsig / chiN < 1.4 + 2.0 / (N + 1)) hsig = 1;
                else hsig = 0;

                //verify4-3
                pc = (1 - cc) * pc + hsig * Math.Sqrt(cc * (2 - cc) * mueff) * (BD * zmean);

                //Adapt covariance matriax C
                Matrix<double> YY = new Matrix<double>(N, mu);
                Matrix<double> diagw = new Matrix<double>(mu, mu);
                CvInvoke.cvSetZero(YY);
                CvInvoke.cvSetZero(diagw);
                for (int j = 0; j < mu; j++)
                {
                    diagw[j, j] = weights[j, 0];
                    for (int i = 0; i < N; i++) YY[i, j] = arz[i, (int)arfitness[1, j]];
                }

                //verify4-4
                YY = BD * YY;

                C = (1 - c1 - cmu) * C + c1 * (pc * pc.Transpose() + (1 - hsig) * cc * (2 - cc) * C)
                    + cmu * YY * diagw * YY.Transpose();

                //Adapt step-size sigma
                sigma = sigma * Math.Exp((cs / damps) * (norm / chiN - 1));

                //Update B and D from C
                Matrix<double> W = new Matrix<double>(N, 1);
                Matrix<double> U = new Matrix<double>(N, N);
                if (counteval - eigeneval > lambda * 1.0 / (c1 + cmu) / N / 10)
                {
                    eigeneval = counteval;
                    for (int i = 1; i < N; i++)
                    {
                        for (int j = 0; j < i; j++) C[i, j] = C[j, i];
                    }
                    CvInvoke.cvSVD(C, W, U, U, Emgu.CV.CvEnum.SVD_TYPE.CV_SVD_U_T);
                    B = U.Transpose();
                    for (int i = 0; i < N; i++) D[i, i] = Math.Sqrt(W[i, 0]);
                }
                if (arfitness[0, 0] < min)
                {
                    min = arfitness[0, 0];
                    for (int i = 0; i < N; i++) xbest[i, 0] = arx[i, (int)arfitness[1, 0]];


                    StreamWriter sw = new StreamWriter("MMLPX.txt");
                    int sizeM = 129;
                    int m = 0;
                    for (int i = 0; i < sizeM; i++)
                    {
                        for (int j = 0; j < 2; j++, m++) sw.Write("{0} ", xmean[m, 0]);
                        sw.WriteLine();
                    }
                    sw.Close();

                    sw = new StreamWriter("BMLPX.txt");
                    m = 0;
                    for (int i = 0; i < sizeM; i++)
                    {
                        for (int j = 0; j < 2; j++, m++) sw.Write("{0} ", xbest[m, 0]);
                        sw.WriteLine();
                    }
                    sw.Close();
                }

                if (arfitness[0, 0] <= stopfitness) break;
                //flat fitness
                if (arfitness[0, 0] == arfitness[0, (int)Math.Ceiling(0.7 * lambda)])
                {
                    sigma = sigma * Math.Exp(0.2 + cs / damps);
                    Console.WriteLine("warning: flat fitness, consider reformulating the objective");
                }
                Console.WriteLine("{0}:{1}--{2}--{3}---{4}", counteval, arfitness[0, 0],arfitness[0,lambda-1],min, lambda);
            }
            Console.WriteLine("{0}:{1}", counteval, arfitness[0, 0]);
            //The best solution
            for (int i = 0; i < N; i++) xmean[i, 0] = arx[i, (int)arfitness[1, 0]];

            Console.ReadKey();

        }

        static void GFWA(ref Matrix<double> xmean, ref Matrix<double> xbest)
        {
            //step6
            //int N = 128;
            int N = 258;
            int sparkNum = 50;
            int maxEva = N * 10000;
            int countEva = 0;
            double UB = 1;
            double LB = -1;
            CustomRandom rnd = new CustomRandom();
            //Matrix<double> xmean = new Matrix<double>(N, 1);
            Matrix<double> temp = new Matrix<double>(N, 1);
            Matrix<double> gxmean = new Matrix<double>(N, 1);
            //for (int i = 0; i < N; i++) xmean[i, 0] = rnd.NextDouble() * (UB - LB) + LB;

            Matrix<double> sparks = new Matrix<double>(N, sparkNum);
            double[,] arfitness = new double[2, sparkNum];
            //single point
            double[,] pfitness = new double[2, 1];
            pfitness[1, 0] = 0;

            Feval(ref xmean, pfitness, 1, countEva);
            countEva++;
            double xftness = pfitness[0, 0];

            //double scope = UB - LB;
            double scope = 0.003;

            double gftness = double.MaxValue;
            double bftness = double.MaxValue;
            double cbftness = double.MaxValue;
            int selectNum = (int)Math.Ceiling(0.2 * sparkNum);

            while (countEva < maxEva)
            {
                //Generate sparks
                for (int j = 0; j < sparkNum; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        temp[i, 0] = (rnd.NextDouble() * 2 - 1) * scope + xmean[i, 0];
                        if (temp[i, 0] < LB || temp[i, 0] > UB) temp[i, 0] = rnd.NextDouble() * (UB - LB) + LB;
                        sparks[i, j] = temp[i, 0];
                    }
                    //arfitness[0, j] = Feval(temp);
                    arfitness[1, j] = j;
                    //countEva++;
                }


                countEva += sparkNum;
                Feval(ref sparks, arfitness, sparkNum, countEva);

                //for (int i = 0; i < N; i++)
                //{
                //    Console.Write("{0} ", xmean[i, 0]);
                //}
                //Console.WriteLine(":{0}", xftness);

                //for (int j = 0; j < sparkNum; j++)
                //{
                //    for (int i = 0; i < N; i++)
                //    {
                //        Console.Write("{0} ", sparks[i, j]);
                //    }
                //    Console.WriteLine(":{0}", arfitness[0, j]);
                //}

                //Sort

                double index, value; int num;
                for (int j = 0; j < sparkNum - 1; j++)
                {
                    num = j;
                    for (int k = j + 1; k < sparkNum; k++)
                    {
                        if (arfitness[0, num] > arfitness[0, k])
                        {
                            num = k;
                        }
                    }
                    if (num != j)
                    {
                        value = arfitness[0, num];
                        index = arfitness[1, num];
                        arfitness[0, num] = arfitness[0, j];
                        arfitness[1, num] = arfitness[1, j];
                        arfitness[0, j] = value;
                        arfitness[1, j] = index;
                    }
                }


                //Generate a guide spark
                for (int i = 0; i < N; i++)
                {
                    value = 0;
                    for (int j = 0; j < selectNum; j++)
                    {
                        value += sparks[i, (int)arfitness[1, j]];
                        value -= sparks[i, (int)arfitness[1, sparkNum - 1 - j]];
                    }
                    temp[i, 0] = xmean[i, 0] + value / selectNum;
                    if (temp[i, 0] < LB || temp[i, 0] > UB) temp[i, 0] = rnd.NextDouble() * (UB - LB) + LB;
                    gxmean[i, 0] = temp[i, 0];
                }

                countEva++;
                Feval(ref gxmean, pfitness, 1, countEva);

                gftness = pfitness[0, 0];


                if (xftness <= arfitness[0, 0] && xftness <= gftness)
                {
                    bftness = xftness;
                    temp = xmean * 1;
                }
                else if (gftness <= arfitness[0, 0] && gftness <= xftness)
                {
                    bftness = gftness;
                    temp = gxmean * 1;
                }
                else
                {
                    bftness = arfitness[0, 0];
                    num = (int)arfitness[1, 0];
                    for (int i = 0; i < N; i++) temp[i, 0] = sparks[i, num];
                }


                if (bftness < xftness) scope *= 1.2;
                else scope *= 0.9;

                if (bftness < cbftness)
                {
                    cbftness = bftness;
                    //min = arfitness[0, 0];
                    for (int i = 0; i < N; i++) xbest[i, 0] = temp[i,0];
                    //step8
                    //StreamWriter sw = new StreamWriter("MMLPX.txt");
                    //for (int i = 0; i < N; i++) sw.Write("{0} ", xmean[i, 0]);
                    //sw.WriteLine();
                    //sw.Close();

                    //sw = new StreamWriter("BMLPX.txt");
                    //for (int i = 0; i < N; i++) sw.Write("{0} ", xbest[i, 0]);
                    //sw.WriteLine();
                    //sw.Close();

                    StreamWriter sw = new StreamWriter("MMLPX.txt");
                    int sizeM = 129;
                    int m = 0;
                    for (int i = 0; i < sizeM; i++)
                    {
                        for (int j = 0; j < 2; j++, m++) sw.Write("{0} ", xmean[m, 0]);
                        sw.WriteLine();
                    }
                    sw.Close();

                    sw = new StreamWriter("BMLPX.txt");
                    m = 0;
                    for (int i = 0; i < sizeM; i++)
                    {
                        for (int j = 0; j < 2; j++, m++) sw.Write("{0} ", xbest[m, 0]);
                        sw.WriteLine();
                    }
                    sw.Close();
                }

                //if (bftness < cbftness) cbftness = bftness;
                Console.WriteLine("{0}---{1}---{2}---{3}---{4}", countEva, xftness, arfitness[0, 0], cbftness, scope);

                xmean = temp * 1;
                //for (int i = 0; i < N; i++) xmean[i, 0] = temp[i, 0];
                xftness = bftness;

            }

        }


	}
}
