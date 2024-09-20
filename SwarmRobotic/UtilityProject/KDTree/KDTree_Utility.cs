namespace UtilityProject.KDTree
{
    //索引结点
	struct KDTreeNode
	{
		public int dimension, left, right, start, count;

		public override string ToString() { return string.Format("(d{0})<{1}>{2}={4}+{3}", dimension, left, right, count, start); }
	}

    //数据结点
	public interface IKDTreeData
	{
		int ID { get; }
		int Dimension { get; }
		float this[int index] { get; }
		bool Skip { get; }
	}
}