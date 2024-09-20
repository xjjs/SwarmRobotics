using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;
using RobotLib.Environment;
using RobotLib.FitnessProblem;
using Microsoft.Xna.Framework;

namespace TestProject
{
    /// <summary>
    /// 继承自TestBase，同样repeat参数所设置的Repeat属性没有用处
    /// </summary>
	class MinimalTest : TestBase<SMinimal>
	{
		public MinimalTest(int repeat, params int[] IterCoefs)
			: base(repeat)
		{
			StopByTar = false;
			var l = IterCoefs.Length / 2;
            if (l * 2 != IterCoefs.Length) throw new Exception();
			IterEqus = new Tuple<int, int>[l];
			for (int i = 0; i < l; i++)
				IterEqus[i] = Tuple.Create(IterCoefs[i * 2], IterCoefs[i * 2 + 1]);
			TarRates = null;
			MaxIteration = int.MaxValue;
		}

		public MinimalTest(int repeat = 50, int MaxIter = 10000, params float[] TarRates)
			: base(repeat)
		{
			StopByTar = true;
			var l = TarRates.Where(r => r <= 1).ToList();
            l.Sort();
            if (!l.Contains(1f)) l.Add(1f);
			//if (TarRates[TarRates.Length - 1] < 1) l = l.Concat(Enumerable.Repeat(1f, 1));
			this.TarRates = l.ToArray();
			IterEqus = null;

            //最大迭代次数base中先赋值为0，然后次数赋值为目标值
            MaxIteration = MaxIter;
		}

        //用于Clone函数，以方便对象复制
		private MinimalTest(int Repeat, bool StopByTar, int MaxIter, float[] TarRates, Tuple<int, int>[] IterEqus)
			:base(Repeat)
		{
			this.StopByTar = StopByTar;
			this.MaxIteration = MaxIter;
			this.TarRates = TarRates;
			this.IterEqus = IterEqus;
		}

        //返回指定索引编号的实验状态
        public override SMinimal TestOnce(Experiment param) { return TestStep(param).ElementAt(indexOnce); }

        //返回目标比率数组or迭代次数数组中的元素
        public override IEnumerable<SMinimal> TestStep(Experiment param)
		{
            var state = param.environment.runstate as SMinimal;
            SMinimal result;
			if (StopByTar)
			{
				foreach (var rate in TarRates)
				{
                    //计算要求的目标数目
					int collectTars = (int)(rate * (param.problem as PMinimal).TargetNum + 0.5);
                    //持续更新环境（运行实验）直至结束
                    while (!state.Finished && state.CollectedTargets < collectTars && state.Iterations < MaxIteration)
                        param.Update();
                    //拷贝状态并进行资源的回收处理工作
                    result = state.ResultClone() as SMinimal;
					param.problem.FinalizeState(result, param.environment);
                    //成功状态为收集到了指定数量的目标
					result.Success = state.CollectedTargets >= collectTars;
                    yield return result;
				}
			}
			else
			{
				foreach (var itco in IterEqus)
				{
                    //迭代次数：目标综合迭代次数+基础迭代次数？
                    int iteration = itco.Item1 * (param.problem as PMinimal).TargetNum + itco.Item2;
                    param.Run(iteration);
                    result = state.ResultClone() as SMinimal;
					param.problem.FinalizeState(result, param.environment);
					//result.Success = state.Iterations >= iteration || state.Finished;
                    //成功状态为有机器人个体幸存
                    result.Success = state.AliveRobots > 0;
                    yield return result;
                }
			}
		}

        public override TestBase<SMinimal> Clone() { return new MinimalTest(Repeat, StopByTar, MaxIteration, TarRates, IterEqus); }

		/// <summary>
		/// true for Targets, false for Iterations
        /// 终止条件是收集目标则为true，否则为false（即终止条件为迭代次数）
		/// </summary>
		bool StopByTar;
        //目标收集的比率数组
		float[] TarRates;
        //迭代信息，迭代比率与迭代**？？
		//IterRate, IterCons
		Tuple<int, int>[] IterEqus;
		public int indexOnce = 0;
	}
}
