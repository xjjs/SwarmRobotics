using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.FitnessProblem;


//Search 干扰级别别


namespace RobotLib.FitnessProblem
{
    //public class MapProvider
    //{
    //    public MapProvider(int ObsNum) { ObstacleMap = new Obstacle[ObsNum]; }

    //    public Obstacle[] ObstacleMap { get; protected set; }

    //    public Vector3 MapSize;

    //    public bool LoadFromFile(string filename) { return false; }
    //}

	/// <summary>
	/// 为多目标搜索问题生成2D的适应度值地图（int[,]矩阵）：目标数组（包括真假目标）、干扰源数组、最终地图、环号地图（movemap）
    /// 私有字段（干扰地图、假目标地图、横纵尺寸、真目标数量）
	/// </summary>
    public class FitnessMapProvider2D //: MapProvider
	{
        public FitnessMapProvider2D(int sizex, int sizey, FitnessTarget[] targets, int realnumber, Interference[] inters = null)
        {
            this.sizex = sizex;
            this.sizey = sizey;
            this.targets = targets;
			this.realnumber = realnumber;
			interferences = inters;
			finalmap = new int[sizex, sizey];
			if (interferences != null)
				intermap = new int[sizex, sizey];
			else
				intermap = null;
			if (realnumber != targets.Length)
				decoymap = new int[sizex, sizey];
			else
				decoymap = null;
		}
        //当传入尺寸为单精度实数时的构造函数
		public FitnessMapProvider2D(float sizex, float sizey, FitnessTarget[] targets, int realnumber, 
            Interference[] inters = null) : this((int)Math.Floor(sizex) + 1, (int)Math.Floor(sizey) + 1, targets, realnumber, inters) { }
        //前realnumber个元素为真目标
		public FitnessTarget[] targets;
		public Interference[] interferences;
		//int[][,] maps;
		public int[,] finalmap;
		int[,] intermap, decoymap;
		int sizex, sizey, realnumber;

        //叠加中间地图，生成最终地图
		public void Update()
		{
            //复制假目标的适应度map到最终map
			if (decoymap == null)
				Array.Clear(finalmap, 0, finalmap.Length);
			else
				Array.Copy(decoymap, finalmap, finalmap.Length);
            
            //叠加可视真目标（不可视的为已收集完成的目标）的适应度到最终map
			foreach (var tar in targets.Take(realnumber))
			{
				if (tar.Visible)
				{
					int dx = (int)tar.Position.X - center, dy = (int)tar.Position.Y - center;
				    int minx = Math.Max(0, dx), maxx = Math.Min(sizex, dx + arraysize - 1), miny = Math.Max(0, dy), maxy = Math.Min(sizey, dy + arraysize - 1);
				    for (int x = minx; x < maxx; x++)
				        for (int y = miny; y < maxy; y++)
							finalmap[x, y] = Math.Max(finalmap[x, y], movemap[x - dx, y - dy] + tar.Energy);
				}
			}
            //叠加干扰源的适应度到最终map
			if (intermap != null)
			{
				for (int i = 0; i < sizex; i++)
					for (int j = 0; j < sizey; j++)
                        //Search 干扰级别别
						finalmap[i, j] = Math.Max(0, finalmap[i, j] + intermap[i, j]);
                        //finalmap[i, j] = intermap[i, j] < 0 ? 1 : finalmap[i, j];
			}
		}
        //生成包络网各结点的干扰值
		public void ResetInterference()
		{
			if (intermap != null)
			{
				Array.Clear(intermap, 0, intermap.Length);
				foreach (var inter in interferences)
				{
					int dx = (int)inter.Position.X - center, dy = (int)inter.Position.Y - center;
					int minx = Math.Max(0, dx), maxx = Math.Min(sizex, dx + arraysize - 1), miny = Math.Max(0, dy), maxy = Math.Min(sizey, dy + arraysize - 1);
					for (int x = minx; x < maxx; x++)
						for (int y = miny; y < maxy; y++)
                            //干扰负能量值加上环号，而且不大于0，有不同的负能量时取小者
							intermap[x, y] = Math.Min(intermap[x, y], -inter.Level - movemap[x - dx, y - dy]);

				}
			}
		}
        //生成包络网各结点的假目标适应度
		public void ResetDecoy()
		{
			if (decoymap != null)
			{
				Array.Clear(decoymap, 0, decoymap.Length);
				foreach (var tar in targets.Skip(realnumber))
				{
                    //对每个假目标，计算其包络框的左上顶点
					int dx = (int)tar.Position.X - center, dy = (int)tar.Position.Y - center;
                    //对四角边界进行bounding处理
					int minx = Math.Max(0, dx), maxx = Math.Min(sizex, dx + arraysize - 1), miny = Math.Max(0, dy), maxy = Math.Min(sizey, dy + arraysize - 1);
					for (int x = minx; x < maxx; x++)
						for (int y = miny; y < maxy; y++)
                            //目标正能量值减去环号，而且不小于0，有不同的正能量值时取大者，假目标与真目标的Energy是略有差异的
							decoymap[x, y] = Math.Max(decoymap[x, y], movemap[x - dx, y - dy] + tar.Energy);
				}
			}
		}

        //返回指定位置处的适应度值，单精度实数取整到整数，实际的适应度地图是这样吗？
		public int GetFitness(Vector3 position)
		{
			if (position.X < 0 || position.X >= sizex || position.Y < 0 || position.Y >= sizey) return 0;
			return finalmap[(int)position.X, (int)position.Y];
		}
        //生成适应度同心圆的包络网各结点的环号，用（-环号）在movemap中表示
        //静态构造函数：属于类，只会被执行一次，在创建第一个实例或引用任何静态成员之前，由.NET自动调用
		static FitnessMapProvider2D()
		{
            //rad为等适应度环的宽度，第一环（圆心）由中心分为两部分：中间的圆心为目标、周围的环（一半宽度）为可以感知到目标的区域
            //设置适应度环的宽度与整体半径（环宽度*环数）
            //Radius与Level是不变的常量，不可通过UI界面设置，计算时考虑20层，将负值忽略掉了而已
			int rad = PMinimal.FitnessRadius;
			center = rad * PMinimal.FitnessLevel;
            //直径上单位长度所标记的各点
			arraysize = center * 2 + 1;
            //同心圆的包络网
			movemap = new int[arraysize, arraysize];
			double val;
			for (int i = -center; i <= center; i++)
				for (int j = -center; j <= center; j++)
				{
                    //长度/每环宽度=环号（设圆心与第一环的环号为0）
					val = Math.Sqrt(i * i + j * j) / rad;
                    //在每个位置用（-环号）标记，相应的位置有center的偏移
					movemap[i + center, j + center] = (int)Math.Ceiling(-val);
				}
		}
		static public int[,] movemap;
		static public readonly int center, arraysize;
	}

    /// <summary>
    /// 为多目标搜索问题生成3D的适应度地图（int[,,]矩阵）：最终地图
    /// 私有字段：目标数组、障碍物地图列表、各维尺寸、环号地图
    /// </summary>
	public class FitnessMapProvider3D //: MapProvider
	{
        public FitnessMapProvider3D(int sizex, int sizey, int sizez, FitnessTarget[] targets)
        {
            this.sizex = sizex;
            this.sizey = sizey;
            this.sizez = sizez;
            this.targets = targets;
        }

        public FitnessMapProvider3D(float sizex, float sizey, float sizez, FitnessTarget[] targets)
            : this((int)Math.Floor(sizex) + 1, (int)Math.Floor(sizey) + 1, (int)Math.Floor(sizez) + 1, targets) { }

		FitnessTarget[] targets;
		int[][,,] maps;
		public int[,,] finalmap;
		int sizex, sizey, sizez;

		public void InitObstalces()
		{
            //有几个目标则有几个障碍物地图
			maps = new int[targets.Length][, ,];
			finalmap = new int[sizex, sizey, sizez];
            //对每张地图，设置各位置的能量值
			for (int i = 0; i < targets.Length; i++)
			{
				maps[i] = new int[sizex, sizey, sizez];
				int dx = (int)targets[i].Position.X - 200, dy = (int)targets[i].Position.Y - 200, dz = (int)targets[i].Position.Z - 200;
				int minx = Math.Max(0, dx), maxx = Math.Min(sizex, dx + 400), miny = Math.Max(0, dy), maxy = Math.Min(sizey, dy + 400), minz = Math.Max(0, dz), maxz = Math.Min(sizez, dz + 400);
				for (int x = minx; x < maxx; x++)
					for (int y = miny; y < maxy; y++)
						for (int z = minz; z < maxz; z++)
						{
							maps[i][x, y, z] = Math.Max(0, movemap[x - dx, y - dy, z - dz] + targets[i].Energy);
						}
			}
			UpdateObstalces();
		}

		public void UpdateObstalces()
		{
			Array.Clear(finalmap, 0, finalmap.Length);
            //归并地图列表，求取所有位置的最大的适应度值
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i].Visible)
				{
					for (int x = 0; x < sizex; x++)
						for (int y = 0; y < sizey; y++)
							for (int z = 0; z < sizez; z++)
							{
								finalmap[x, y, z] = Math.Max(finalmap[x, y, z], maps[i][x, y, z]);
							}
				}
			}
		}
        //设置各位置（横纵偏移皆为center）的环号（中心环号为0）
		static FitnessMapProvider3D()
		{
            //环宽为10，环数为20，半径为center，尺寸为arraysize
			int rad = 10, fit = 20, center = rad * fit, arraysize = center * 2 + 1;
			movemap = new int[arraysize, arraysize, arraysize];
			double val;
			for (int i = -center; i <= center; i++)
				for (int j = -center; j <= center; j++)
					for (int k = -center; k <= center; k++)
					{
						val = Math.Sqrt(i * i + j * j + k * k) / rad;
						movemap[i + center, j + center, k + center] = (int)Math.Ceiling(-val);
					}
		}
		static public int[, ,] movemap;
	}
}
