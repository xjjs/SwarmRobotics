using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Obstacles
{
    /// <summary>
    /// 多障碍物列表，根据个数与区间边界生成障碍物阵列
    /// </summary>
	public class MultiObstacle : IEnumerable<Obstacle>
	{
		public MultiObstacle(float SenseRange)
        {
            this.id = 0;
			this.SenseRange = SenseRange;
			Visible = true;
			XMin = XMax = YMin = YMax = ZMin = ZMax = 0;
			SubObstacles = new List<Obstacle>();
        }

		public void SetObstacles()
		{
			int count = 0;
			if (ZMin == ZMax && ZMin == 0)
			{
				for (int x = XMin; x <= XMax; x++)
					for (int y = YMin; y <= YMax; y++)
						AddObstacle(count++, x, y, 0.5f);
			}
			else
			{
				for (int x = XMin; x <= XMax; x++)
					for (int y = YMin; y <= YMax; y++)
						for (int z = ZMin; z <= ZMax; z++)
							AddObstacle(count++, x, y, z);
			}
            //Count为List中实际的元素个数，难道个数不就是count吗，该语句有什么意义吗？？？
			SubObstacles.RemoveRange(count, SubObstacles.Count - count);
		}

		void AddObstacle(int index, int x, int y, float z)
		{
			if (SubObstacles.Count > index)
				SubObstacles[index].Position = new Vector3(x, y, z);
			else
				SubObstacles.Add(new Obstacle(new Vector3(x, y, z), SenseRange));
		}

        public int id;
        //子障碍物的影响半径
		public float SenseRange { get; private set; }
		public bool Visible;
		public int XMin, XMax, YMin, YMax, ZMin, ZMax;

		List<Obstacle> SubObstacles;

		public IEnumerator<Obstacle> GetEnumerator() { return SubObstacles.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return SubObstacles.GetEnumerator(); }
	}
}
