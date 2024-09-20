using System;
using System.Linq;
using UtilityProject.KDTree;

namespace UtilityProject.PSO
{
	public class GlobalTopo<ValueType> : NeighbourTopology<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public GlobalTopo() { }

		public override void CalculateGBest(bool first = false)
		{
			//find best present best position
			int maxId = -1;
			if (first)
			{
				g_fitness = particles[0].fitness;
				maxId = 0;
			}
			for (int i = 0; i < Population; i++)
			{
				if (Compare(g_fitness, particles[i].fitness))
				{
					g_fitness = particles[i].fitness;
					maxId = i;
				}
			}
			if (maxId != -1)
			{
				Array.Copy(particles[maxId].position, g_position, Dimension);
				for (int i = 0; i < Population; i++)
				{
					particles[i].g_fitness = g_fitness;
					Array.Copy(g_position, particles[i].g_position, Dimension);
				}
			}
		}

		public override object Clone() { return new GlobalTopo<ValueType>(); }
	}

	public class IndexTopo<ValueType> : NeighbourTopology<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public IndexTopo(int width = 1)
		{
			this.width = width; 
			total = width * 2 + 1;
		}

		public override void Bind(PSO<ValueType> pso)
		{
			base.Bind(pso);
			neighbour = new int[Population, total];
			int k, t;
			for (int i = 0; i < Population; i++)
			{
				neighbour[i, 0] = i;
				k = 1;
				for (int j = 1; j <= width; j++)
				{
					t = i - j;
					if (t < 0) t += Population;
					neighbour[i, k++] = t;
					t = i + j;
					if (t >= Population) t -= Population;
					neighbour[i, k++] = t;
				}
			}
		}

		public override void CalculateGBest(bool first = false)
		{
			int gmax = -1, maxId;
			if (first)
			{
				g_fitness = particles[0].fitness;
				gmax = 0;
			}
			for (int i = 0; i < Population; i++)
			{
				maxId = -1;
				if (first)
				{
					particles[i].g_fitness = particles[i].fitness;
					maxId = 0;
				}
				for (int j = 0; j < total; j++)
				{
					if (Compare(particles[i].g_fitness, particles[neighbour[i, j]].fitness))
					{
						particles[i].g_fitness = particles[neighbour[i, j]].fitness;
						maxId = j;
					}
				}
				if (maxId != -1)
				{
					Array.Copy(particles[neighbour[i, maxId]].position, particles[i].g_position, Dimension);
					if (Compare(g_fitness, particles[i].g_fitness))
					{
						g_fitness = particles[i].g_fitness;
						gmax = i;
					}
				}
			}
			if (gmax != -1) Array.Copy(particles[gmax].g_position, g_position, Dimension);
		}

		public override object Clone() { return new IndexTopo<ValueType>(width); }

		protected int width, total;
		int[,] neighbour;
	}

	public class NaiveKNNTopo<ValueType> : NeighbourTopology<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public NaiveKNNTopo(int k) { kNN = k + 1; }

		public override void Bind(PSO<ValueType> pso)
		{
			base.Bind(pso);
			distances = new double[Population][];
			indecis = new int[Population][];
			for (int i = 0; i < Population; i++)
			{
				distances[i] = new double[Population];
				indecis[i] = new int[Population];
			}
		}

		public override void CalculateGBest(bool first = false)
		{
			double sum, t;
			int mId = -1, maxId;
			if (first)
			{
				g_fitness = particles[0].fitness;
				mId = 0;
			}
			for (int i = 0; i < Population; i++)
			{
				indecis[i][i] = i;
				distances[i][i] = 0;
				for (int j = i + 1; j < Population; j++)
				{
					indecis[i][j] = j;
					indecis[j][i] = i;
					sum = 0;
					for (int k = 0; k < Dimension; k++)
					{
						t = particles[i].position[k] - particles[j].position[k];
						sum += t * t;
					}
					distances[i][j] = distances[j][i] = sum;
				}
				Array.Sort(distances[i], indecis[i]);
				if (first)
				{
					particles[i].g_fitness = particles[i].fitness;
					maxId = 0;
				}
				else
					maxId = -1;
				foreach (var item in indecis[i].Take(kNN))
				{
					if (Compare(particles[i].g_fitness, particles[item].fitness))
					{
						particles[i].g_fitness = particles[item].fitness;
						maxId = item;
					}
				}
				if (maxId != -1)
				{
					Array.Copy(particles[maxId].position, particles[i].g_position, Dimension);
					if (Compare(g_fitness, particles[i].g_fitness))
					{
						g_fitness = particles[i].g_fitness;
						mId = i;
					}
				}
			}
			if (mId != -1) Array.Copy(particles[mId].g_position, g_position, Dimension);
		}

		public override object Clone() { return new NaiveKNNTopo<ValueType>(kNN); }

		protected int kNN;
		internal double[][] distances;
		internal int[][] indecis;
	}

	//TODO: -1 and first
	public class KNNTopo<ValueType> : NeighbourTopology<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public KNNTopo(int k) { kNN = k; }

		public override void Bind(PSO<ValueType> pso)
		{
			base.Bind(pso);
			nLists = new SortedDistanceList<int>[Population];
			for (int i = 0; i < Population; i++)
				nLists[i] = new SortedDistanceList<int>(kNN);
		}

		public override void CalculateGBest(bool first = false)
		{
			int mId = -1, maxId;
			double dis, t;
			if (first) g_fitness = particles[0].fitness;
			for (int i = 0; i < Population; i++)
			{
				if (first) particles[i].g_fitness = particles[i].fitness;
				nLists[i].Clear();
			}
			for (int i = 0; i < Population; i++)
			{
				for (int j = i + 1; j < Population; j++)
				{
					dis = 0;
					for (int k = 0; k < Dimension; k++)
					{
						t = particles[i].position[k] - particles[j].position[k];
						dis += t * t;
					}
					nLists[i].Add(dis, j);
					nLists[j].Add(dis, i);
				}
				maxId = i;
				foreach (var item in nLists[i].Values)
				{
					if (Compare(particles[i].g_fitness, particles[item].fitness))
					{
						particles[i].g_fitness = particles[item].fitness;
						maxId = item;
					}
				}
				Array.Copy(particles[maxId].position, particles[i].g_position, Dimension);
				if (Compare(g_fitness, particles[i].g_fitness))
				{
					g_fitness = particles[i].g_fitness;
					mId = i;
				}
			}
			Array.Copy(particles[mId].g_position, g_position, Dimension);
		}

		public override object Clone() { return new KNNTopo<ValueType>(kNN); }

		protected int kNN;
		internal SortedDistanceList<int>[] nLists;
	}

	public class KDTreeTopo<ValueType> : NeighbourTopology<ValueType>
		where ValueType : IComparable<ValueType>
	{
		public KDTreeTopo(int k) { kNN = k; }

		public override void Bind(PSO<ValueType> pso)
		{
			base.Bind(pso);
			tree = new KDTree_Basic(Dimension, 0);
			items = new IKDTreeData[Population];
			for (int i = 0; i < Population; i++)
				items[i] = new ParticleKDTreeData<ValueType>(particles[i], i, Dimension);
			tree.BindData(items, kNN: kNN);
			nLists = tree.disList;
		}

		public override void CalculateGBest(bool first = false)
		{
			int mId = -1, maxId, item;
			if (first) g_fitness = particles[0].fitness;
			tree.BuildTree();
			tree.FindAllKNN();
			for (int i = 0; i < Population; i++)
			{
				if (first) particles[i].g_fitness = particles[i].fitness;
				maxId = i;
				foreach (var kdata in nLists[i].Values)
				{
					item = kdata.ID;
					if (Compare(particles[i].g_fitness, particles[item].fitness))
					{
						particles[i].g_fitness = particles[item].fitness;
						maxId = item;
					}
				}
				Array.Copy(particles[maxId].position, particles[i].g_position, Dimension);
				if (Compare(g_fitness, particles[i].g_fitness))
				{
					g_fitness = particles[i].g_fitness;
					mId = i;
				}
			}
			Array.Copy(particles[mId].g_position, g_position, Dimension);
		}

		public override object Clone() { return new KDTreeTopo<ValueType>(kNN); }

		protected int kNN;
		KDTree_Basic tree;
		IKDTreeData[] items;
		internal SortedDistanceList<IKDTreeData>[] nLists;
	}

	class ParticleKDTreeData<ValueType> : IKDTreeData
		where ValueType : IComparable<ValueType>
	{
		public ParticleKDTreeData(Particle<ValueType> p, int index, int dimension)
		{
			data = p.position;
			ID = index;
			Dimension = dimension;
		}

		public int ID { get; private set; }

		public int Dimension { get; private set; }

		public float this[int index] { get { return (float)data[index]; } }

		public bool Skip { get { return false; } }

		double[] data;
	}
}
