namespace UtilityProject.KDTree
{
	public class MultiDimensionRange
	{
		public float Distance { get; private set; }
		public float Distance2 { get; private set; }
		public float DistanceN { get; private set; }
		public MultiDimensionRange(int Dimension)
		{
			this.Dimension = Dimension;
			Center = new float[Dimension];
		}

		public MultiDimensionRange(IKDTreeData data, float Distance)
			: this(data.Dimension)
		{
			SetDistance(Distance);
			SetCenter(data);
		}

		public float[] Center { get; private set; }

		public int Dimension { get; private set; }

		public void SetCenter(IKDTreeData data)
		{
			for (int i = 0; i < Dimension; i++)
				Center[i] = data[i];
		}

		public void SetDistance(float value)
		{
			Distance = value;
			Distance2 = value * value;
			DistanceN = -value;
		}
        //某点在某维上是否为邻居
		public int CompareTo(IKDTreeData value, int dimension)
		{
			float cur = value[dimension] - Center[dimension];
			//if (cur < DistanceN) return -1;
			//if (cur > Distance) return 1;
			//return 0;
			return (cur < DistanceN) ? -1 : ((cur > Distance) ? 1 : 0);
		}
        //与某点的距离（各维累加、过界不再累加）
		public float Contain(IKDTreeData data)
		{
			float sum = 0, cur;
			for (int i = 0; i < Dimension; i++)
			{
				cur = data[i] - Center[i];
				//if (cur > Distance || cur < DistanceN) return float.PositiveInfinity;
				sum += cur * cur;
				if (sum > Distance2) return sum;
			}
			return sum;
		}
	}
}
