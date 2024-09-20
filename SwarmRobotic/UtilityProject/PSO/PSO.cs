using System;
using UtilityProject.Funcs;

namespace UtilityProject.PSO
{
	//standard PSO ref: Daniel Bratton and James Kennedy, Defining a Standard for Particle Swarm Optimization, 2007
    /// <summary>
    /// 标准PSO算法：
    /// 主数据：粒子数组、惯性权重、学习因子
    /// </summary>
    /// <typeparam name="ValueType"></typeparam>
	public sealed class PSO<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public Particle<ValueType>[] particles;
		double w = 0.722984, c = 2.05;
		Random rand;

        //随机种子、最大迭代次数、评估、拓扑、最大步数比率、最小化问题
		public PSO(int randSeed = -1)
		{
			if (randSeed == -1)
				rand = new Random();
			else
				rand = new Random(randSeed);
			maxIteration = 5000;
			Evaluate = null;
			nTopo = null;
			MaxStepRate = 0.1f;
			Maximize = false;
		}

        //分配内存，创建数组
		public void InitSize(int population, int dimension)
		{
			if (!Evaluate.CheckDimention(dimension)) throw new ArgumentException("dimension");
			this.population = population;
			this.dimension = dimension;
			particles = new Particle<ValueType>[population];
			maxStep = new double[dimension];
			Ranges = new ComponentRange[dimension];
			for (int i = 0; i < population; i++)
				particles[i] = new Particle<ValueType>(dimension, i);
			Compare = Maximize ? (Func<ValueType, ValueType, bool>)CompareMaximize : CompareMinimize;
			nTopo.Bind(this);

            //获取参数范围的方法尚未实现么？
			for (int i = 0; i < dimension; i++)
				Ranges[i] = Evaluate.GetRange(i);
		}

        //初始化粒子各维度的位置、速度、适应度
		public void Init()
		{
			double pos;
			foreach (var p in particles)
			{
				for (int j = 0; j < dimension; j++)
				{
					pos = rand.NextDouble() * Ranges[j].InitSize + Ranges[j].InitLBound;
					p.p_position[j] = p.position[j] = pos;
					p.velocity[j] = Math.Max(-maxStep[j], Math.Min(maxStep[j], (rand.NextDouble() * Ranges[j].InitSize + Ranges[j].InitLBound - pos) / 2)); //non uniform
				}
				p.p_fitness = p.fitness = Evaluate.Evaluate(p.position);
			}
			nTopo.CalculateGBest(true);
			iteration = 0;
			for (int j = 0; j < dimension; j++)
				maxStep[j] = (Ranges[j].UBound - Ranges[j].LBound) * MaxStepRate;
		}
        //更新粒子速度、位置、适应度，没有更新全局最优粒子？？？
		public void Iterate()
		{
			foreach (var p in particles)
			{
				for (int j = 0; j < dimension; j++)
				{
					p.velocity[j] = Math.Max(-maxStep[j], Math.Min(maxStep[j], w * (p.velocity[j] +
						c * (rand.NextDouble() * (p.p_position[j] - p.position[j]) +
						rand.NextDouble() * (p.g_position[j] - p.position[j])))));
					p.position[j] += p.velocity[j];
					if (p.position[j] < Ranges[j].LBound)
					{
						p.position[j] = Ranges[j].LBound;
						//particles[i].velocity[j] = 0;
					}
					else if (p.position[j] > Ranges[j].UBound)
					{
						p.position[j] = Ranges[j].UBound;
						//particles[i].velocity[j] = 0;
					}
				}

				p.fitness = Evaluate.Evaluate(p.position);
				if (Compare(p.p_fitness, p.fitness))
				{
					p.p_fitness = p.fitness;
					Array.Copy(p.position, p.p_position, dimension);
				}
			}
			nTopo.CalculateGBest();
			iteration++;
		}


        //并行初始化
		public void InitParallel()
		{
			double pos;
			foreach (var p in particles)
			{
				for (int j = 0; j < dimension; j++)
				{
					pos = rand.NextDouble() * Ranges[j].InitSize + Ranges[j].InitLBound;
					p.p_position[j] = p.position[j] = pos;
					p.velocity[j] = Math.Max(-maxStep[j], Math.Min(maxStep[j], (rand.NextDouble() * Ranges[j].InitSize + Ranges[j].InitLBound - pos) / 2)); //non uniform
				}
			}
			Evaluate.Evaluate(particles);
			foreach (var p in particles)
				p.p_fitness = p.fitness;
			nTopo.CalculateGBest(true);
			iteration = 0;
			for (int j = 0; j < dimension; j++)
				maxStep[j] = (Ranges[j].UBound - Ranges[j].LBound) * MaxStepRate;
		}
        //并行更新
		public void IterateParallel()
		{
			foreach (var p in particles)
			{
				for (int j = 0; j < dimension; j++)
				{
					p.velocity[j] = Math.Max(-maxStep[j], Math.Min(maxStep[j], w * (p.velocity[j] +
						c * (rand.NextDouble() * (p.p_position[j] - p.position[j]) +
						rand.NextDouble() * (p.g_position[j] - p.position[j])))));
					p.position[j] += p.velocity[j];
					if (p.position[j] < Ranges[j].LBound)
					{
						p.position[j] = Ranges[j].LBound;
						//particles[i].velocity[j] = 0;
					}
					else if (p.position[j] > Ranges[j].UBound)
					{
						p.position[j] = Ranges[j].UBound;
						//particles[i].velocity[j] = 0;
					}
				}
			}
			Evaluate.Evaluate(particles);
			foreach (var p in particles)
			{
				if (Compare(p.p_fitness, p.fitness))
				{
					p.p_fitness = p.fitness;
					Array.Copy(p.position, p.p_position, dimension);
				}
			}
			nTopo.CalculateGBest();
			iteration++;
		}

        //判断是否并行执行
		public void Test(bool parallel = false)
		{
			if (parallel)
			{
				InitParallel();
				while (!isStop) IterateParallel();
			}
			else
			{
				Init();
				while (!isStop) Iterate();
			}
		}

        //属性：迭代是否终止、粒子数、维度、迭代次数、最大步数数组与范围数组（？？？）
		public bool isStop { get { return iteration >= maxIteration; } }
		public int population { get; private set; }
		public int dimension { get; private set; }
		public int iteration { get; private set; }
		public double[] maxStep { get; private set; }
		public ComponentRange[] Ranges { get; private set; }

        //最大步数比率、是否最大化、最大迭代次数、评估、邻域拓扑
		public float MaxStepRate;
		public bool Maximize;
		public int maxIteration;
		public EvaluateFunction<ValueType> Evaluate;
		public NeighbourTopology<ValueType> nTopo;

        //比较函数指针
		public Func<ValueType, ValueType, bool> Compare { get; private set; }
        //返回true，则v2优于v1
		private bool CompareMaximize(ValueType v1, ValueType v2) { return v1.CompareTo(v2) < 0; }
		private bool CompareMinimize(ValueType v1, ValueType v2) { return v1.CompareTo(v2) > 0; }
	}

    /// <summary>
    /// PSO粒子：可比较；
    /// 主数据：索引、速度、位置与适应度、局部位置与适应度、全局位置与适应度；
    /// </summary>
    /// <typeparam name="ValueType"></typeparam>
	public class Particle<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public Particle(int dimension, int index)
		{
			position = new double[dimension];
			velocity = new double[dimension];
			p_position = new double[dimension];
			g_position = new double[dimension];
			this.index = index;
		}

		public double[] position, velocity, p_position, g_position;
		public ValueType fitness, p_fitness, g_fitness;
		public int index;
	}

    /// <summary>
    /// 邻域拓扑抽象类
    /// 主数据：粒子数组、种群与维度大小、全局最优位置与适应度
    /// </summary>
    /// <typeparam name="ValueType"></typeparam>
	public abstract class NeighbourTopology<ValueType> : ICloneable
		where ValueType : IComparable<ValueType>
	{
		public NeighbourTopology() { }

		public virtual void Bind(PSO<ValueType> pso)
		{
			Population = pso.population;
			Dimension = pso.dimension;
			this.particles = pso.particles;
			g_position = new double[Dimension];
			Compare = pso.Compare;
		}

		public abstract void CalculateGBest(bool first = false);

		public Func<ValueType, ValueType, bool> Compare { get; private set; }

		protected Particle<ValueType>[] particles;
		protected int Population;
		protected int Dimension;
		public double[] g_position;
		public ValueType g_fitness;

		public abstract object Clone();
	}
}

