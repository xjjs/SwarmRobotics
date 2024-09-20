using System;

namespace UtilityProject.KDTree
{
    public abstract class KDTree
    {
        protected int Count;
        protected float Distance, NDistance, Distance2;
        public Action<IKDTreeData, IKDTreeData, float> RoboticCallBack, ObstacleCallback;
        protected MultiDimensionRange range;
        protected FixSizeQueue<int> queue;
        public SortedDistanceList<IKDTreeData>[] disList;

        public int Dimension { get; private set; }

		public KDTree(int Dimension, float Distance)
		{
			this.Dimension = Dimension;
			range = new MultiDimensionRange(Dimension);
			range.SetDistance(Distance);
			this.Distance = Distance;
			NDistance = -Distance;
			Distance2 = Distance * Distance;
		}

        public void BindData(IKDTreeData[] Items, Action<IKDTreeData, IKDTreeData, float> RoboticCallBack = null,
            Action<IKDTreeData, IKDTreeData, float> ObstacleCallback = null, int kNN = 0)
        {
            this.RoboticCallBack = RoboticCallBack;
            this.ObstacleCallback = ObstacleCallback;
            Count = Items.Length;
            queue = new FixSizeQueue<int>(Count);
            if (kNN > 0)
            {
                //有几个机器人，就有几个有序列表
                disList = new SortedDistanceList<IKDTreeData>[Count];
                for (int i = 0; i < Count; i++)
                    disList[i] = new SortedDistanceList<IKDTreeData>(kNN);
            }
            else
                disList = null;
            BindData(Items);
        }

        protected abstract void BindData(IKDTreeData[] Items);

        public abstract void BuildTree();

        public abstract void FindAllInRange();

        public abstract void FindInRange(float Distance, IKDTreeData obstacle);

        public abstract void FindAllKNN();
    }

    //看SR的KD树前先看后面的“基本KD树”，基本实现逻辑类似
    /// <summary>
    /// 索引KD树
    /// </summary>
    public sealed class KDTree_SR : KDTree
    {       
        IKDTreeData[] values;

        //结点内的字段成员基本KD树单独定义了
        KDTreeNode[] nodes;
        int[] split;

        int Count1;
        int root;

        public KDTree_SR(int Dimension, float Distance) : base(Dimension, Distance) { }

        protected override void BindData(IKDTreeData[] Items)
        {
            values = Items;
            Count1 = Count - 1;
            nodes = new KDTreeNode[Count];
            //for (int i = 0; i < Count; i++)
            //    nodes[i] = new KDTreeNode();

            //元素的左右子树拆分方式同基本KD树，区别在于索引值
            split = new int[Count + 1];
            int temp;
            for (int i = 2; i <= Count; i++)
            {
                temp = (int)Math.Pow(2, (int)Math.Log(i, 2) - 1);
                if (i < temp * 3)
                    split[i] = i - temp;
                else
                    split[i] = (temp << 1) - 1;
            }
            root = InitTree(0, Count, 0);
        }

        //初始化树结点的索引（左右孩子的索引、要分割的维度、要分割的结点数），索引原则有2
        //规则1，若结点是每层的最左结点，则左子树的末结点、本节点、右孩子，“三者索引从小到大连续”，左<本<右
        //规则2，若结点不是最左结点，则父节点，本节点，左孩子，“三者索引从小到大连续”，左子树末结点+1=右孩子，本<左<右
        //0索引是最高层且最左方的结点
        int InitTree(int start, int count, int dimension)
        {
            //每次递归都重新创建一个新的结点吗？（已创建数组的空间自动删除？）
            KDTreeNode node = new KDTreeNode();
            node.dimension = dimension;
            node.count = count;

            //是否在每一层的最左端
            bool OnLeft = (start == 0);
            if (count == 1)
            {
                node.left = node.right = Count;
                node.start = start;
                //用新建对象替换已有对象，并返回自身索引
                nodes[start] = node;
                return start;
            }

            //分割维更新一样
            dimension++;
            if (dimension >= Dimension) dimension = 0;

            //index为自身索引值，对于最左结点（左子树的大小就是自身索引值）
            //最左结点的start为0，其余结点的
            int index = OnLeft ? split[count] : start;
            node.start = start;

            //一直递归到split[count]=1为止start都为0
            node.left = InitTree(OnLeft ? start : index + 1, split[count], dimension);
            //递归返回时，0索引的左孩子仍为0，

            if (count > 2)
            {
                //设置右孩子的索引
                node.right = split[count] + 1 + start;
                InitTree(node.right, count - node.right + start, dimension);
            }
            else
                node.right = Count;
            
            nodes[index] = node;
            return index;
        }

        public override void BuildTree() { BuildTree(root); }

        //传入的index为树的根结点索引
        //数据结点的分割关系与Basic相同，只是索引不同了
        void BuildTree(int index)
        {
            //索引结点与数据结点是不同的数据结构，这里取的作为根的索引结点
            KDTreeNode node = nodes[index];

            //mid为数据点集合的中央结点的编号
            //最左结点的mid为左子树大小
            //其他结点的mid为左子树的末结点索引
            int s = node.start, c = node.count, dimension = node.dimension, mid = split[c] + s, pos;
            IKDTreeData midvalue, swap;
            //若是最左结点，则交换其与首结点的数据，即s为待考察结点的数据
            if (index != s)
            {
                swap = values[index];
                values[index] = values[s];
                values[s] = swap;
            }
            while (true)
            {
                pos = s;
                midvalue = values[s];

                //j从左孩子的索引开始
                for (int i = 1, j = s + 1; i < c; i++, j++)
                {
                    swap = values[j];
                    if (midvalue[dimension] > swap[dimension])
                    {
                        pos++;
                        if (j > pos)
                        {
                            values[j] = values[pos];
                            values[pos] = swap;
                        }
                    }
                }
                //若小于根节点的超过了左子树大小交换结点数据后，分割小结点集合
                if (pos > mid)
                {
                    c = pos - s;
                    swap = values[pos];
                    values[pos] = values[s];
                    values[s] = swap;
                }
                //若小于根节点的结点树小于左子树大小，则更改初始位置与结点数后重新分割
                else if (pos < mid)
                {
                    pos = pos - s + 1;
                    c -= pos;
                    s += pos;
                }
                else
                    break;
            }
            //将目标值调整到目标索引处
            if (s != index)
            {
                swap = values[index];
                values[index] = values[s];
                values[s] = swap;
            }

            //Build
            if (node.left < Count) BuildTree(node.left);
            if (node.right < Count) BuildTree(node.right);
        }

        public override void FindAllInRange()
        {
            int root = this.root;
            range.SetDistance(Distance);

            //依次考察每个机器人的邻居个体
            for (int i = Count1; i > 0; i--)
            {
                //考虑到邻域的对称性，考察完右子树的结点后，再考察根与左子树结点时，不在考虑右子树的结点了
                if (i == root) root = nodes[i].left;
                if (values[i].Skip) continue;
                FindInRange(i, root);
            }
        }

        void FindInRange(int index, int root)
        {
            int ind;
            float val;
            KDTreeNode node;
            IKDTreeData pos, value;

            //获取考察点并设为中心
            pos = values[index];
            range.SetCenter(pos);
            //根结点入队
            queue.Init();
            queue.Enqueue(root);
            while (queue.Contains)
            {
                ind = queue.Dequeue();
                node = nodes[ind];
                value = values[ind];
                val = value[node.dimension] - pos[node.dimension];
                if (val > Distance)
                {
                    if (node.left < index) queue.Enqueue(node.left);
                }
                else if (val < NDistance)
                {
                    if (node.right < index) queue.Enqueue(node.right);
                }
                else
                {
                    if (node.left < index) queue.Enqueue(node.left);
                    if (node.right < index) queue.Enqueue(node.right);
                    if (value.Skip) continue;
                    val = range.Contain(value);
                    if (val <= Distance2)
                        RoboticCallBack(value, pos, (float)Math.Sqrt(val));
                }
            }
        }

        public override void FindInRange(float Distance, IKDTreeData obstacle)
        {
            int ind;
            float val, NDistance = -Distance, Distance2 = Distance * Distance;
            KDTreeNode node;
            IKDTreeData value;

            //设置障碍物为范围中心
            range.SetCenter(obstacle);
            range.SetDistance(Distance);
            //根结点入队
            queue.Init();
            queue.Enqueue(root);
            while (queue.Contains)
            {
                ind = queue.Dequeue();
                node = nodes[ind];
                value = values[ind];
                val = value[node.dimension] - obstacle[node.dimension];
                if (val > Distance)
                {
                    if (node.left < Count) queue.Enqueue(node.left);
                }
                else if (val < NDistance)
                {
                    if (node.right < Count) queue.Enqueue(node.right);
                }
                else
                {
                    if (node.left < Count) queue.Enqueue(node.left);
                    if (node.right < Count) queue.Enqueue(node.right);
                    if (value.Skip) continue;
                    val = range.Contain(value);
                    if (val <= Distance2)
                        ObstacleCallback(value, obstacle, (float)Math.Sqrt(val));
                }
            }
        }

        public override void FindAllKNN()
        {
            //result是一个有序列表
            SortedDistanceList<IKDTreeData> result;
            for (int i = 0; i < Count; i++)
            {
                //disList是列表的列表，机器人的树索引i与其ID是不同的，ID指定其近邻列表
                result = disList[values[i].ID];
                //初始化列表并求取第i个机器人的邻居列表(KNN邻居列表是非对称的)
                result.Clear();
                FindKNN(values[i], result);
            }
        }

        void FindKNN(IKDTreeData pos, SortedDistanceList<IKDTreeData> result)
        {
            int count = 0;
            int ind;
            double val;
            KDTreeNode node;
            //设置范围中心
            range.SetCenter(pos);
            range.SetDistance((float)result.maxDis);
            //最左下结点入队？？难道不应该是根结点root入队？
            //最左下结点的left与right都为Count（结点数目），这样while循环执行一次就退出了
            //???
            queue.Init();
            queue.Enqueue(0);
            while (queue.Contains)
            {
                count++;
                ind = queue.Dequeue();
                node = nodes[ind];
                val = values[ind][node.dimension] - pos[node.dimension];
                if (val > range.Distance)
                {
                    //<Count结点数目是为了保证索引值有效
                    if (node.left < Count) queue.Enqueue(node.left);
                }
                else if (val < -range.Distance)
                {
                    if (node.right < Count) queue.Enqueue(node.right);
                }
                else
                {
                    if (node.left < Count) queue.Enqueue(node.left);
                    if (node.right < Count) queue.Enqueue(node.right);
                    val = range.Contain(values[ind]);
                    if (result.Add(val, values[ind]))
                        range.SetDistance((float)result.maxDis);
                }
            }
        }
    }

    /// <summary>
    /// 基本KD树
    /// </summary>
    public sealed class KDTree_Basic : KDTree
    {
        IKDTreeData[] values;
        int[] split, dims, leftInd, rightInd, countSub;
        //bool[] left;

        public KDTree_Basic(int Dimension, float Distance) : base(Dimension, Distance) { }

        //初始化要分割i个结点生成的左子树的大小split[i]
        protected override void BindData(IKDTreeData[] Items)
        {
            values = Items;
            split = new int[Count + 1];
            int temp;
            for (int i = 2; i <= Count; i++)
            {
                //在这里i可看作要分割的结点数，split[i]则是左子树要包含的节点数目
                //此处KD树的构建原则是，对于每一层要尽量填满左子树（一般的KD树实现则是尽量将结点平均分配）
                //对一棵二叉树，层数从0开始递增，结点编号则从1开始从左往右从上往下编号，第i层有2^i个结点，前i-1层共有2^i-1个结点
                //即(int)Math.Log(i,2)-1为父结点的层数，temp为该层的结点数
                //注意：此处编号从1开始考虑的是总结点个数，在下面具体的树的构建则是从0开始（表示的是索引）
                temp = (int)Math.Pow(2, (int)Math.Log(i, 2) - 1);
                if (i < temp * 3)
                    split[i] = i - temp;
                else
                    split[i] = (temp << 1) - 1;
            }
            dims = new int[Count];
            leftInd = new int[Count];
            rightInd = new int[Count];
            countSub = new int[Count];
            InitTree(0, Count, 0);
        }

        //用节点start的dimension维分割count个点
        //初始化每个结点的：左右孩子索引(leftInit/rightInit)、分割维度（dims）、分割结点数（countSub）
        //无左右孩子则设为1，无右孩子则设为2
        //索引0为最低层的根结点
        void InitTree(int start, int count, int dimension)
        {
            //本结点的维度，结点的索引从0开始
            dims[start] = dimension;
            //本结点要分割的点数
            countSub[start] = count;
            if (count == 1)
            {
                leftInd[start] = rightInd[start] = Count;
                return;
            }
            //下一结点要分割的维度（不考虑方差、按层循环指定）
            dimension++;
            if (dimension >= Dimension) dimension = 0;

            //左子树的起始结点：左孩子
            leftInd[start] = start + 1;
            //用左孩子的第dimension维分割左子树
            InitTree(leftInd[start], split[count], dimension);
            if (count > 2)
            {
                //右子树的起始结点：右孩子
                rightInd[start] = split[count] + 1 + start;
                //用右孩子的第dimension维分割剩余结点count-(rightInt[start]-start)
                InitTree(rightInd[start], count - rightInd[start] + start, dimension);
            }
            else
                //若带分割结点树为2，则将右孩子设为2
                rightInd[start] = Count;
        }

        public override void BuildTree() { BuildTree(0); }

        void BuildTree(int start = 0)
        {
            //读取所需数据以备分割，mid为左子树的末结点索引
            int s = start, c = countSub[start], dimension = dims[start], mid = split[c] + start, pos;
            IKDTreeData midvalue, swap;

            //如前所述，树的结构以及各结点索引都已经初始化完成，接下来的要做的是调整数组values中的元素，使其索引元素间的关系满足树的规定
            //本例实现的KD树规定：左子树的元素不大于根结点，而且根结点索引<左子树索引<右子树索引，遍历左子树只需根结点+左子树大小即可
            while (true)
            {
                //保存初始索引及其值
                pos = s;
                midvalue = values[s];
                //i控制循环次数，即所有的结点数-1（剩余结点数）
                //j表示只考虑索引值大于该结点的元素，若小于根节点则j可直接满足pos，否则j要增加到满足时赋给下一个pos（实际是交换）
                //这样c-1次循环下来，pos-s表示满足小于根节点的结点数
                for (int i = 1, j = s + 1; i < c; i++, j++)
                {
                    swap = values[j];
                    if (midvalue[dimension] > swap[dimension])
                    {
                        pos++;
                        if (j > pos)
                        {
                            values[j] = values[pos];
                            values[pos] = swap;
                        }
                    }
                }
                //若左子树容纳不了所有的小结点，则交换s与最终的小结点，并重置总结点数为c（其余的点肯定大于小结点不必再考虑了）
                if (pos > mid)
                {
                    c = pos - s;
                    swap = values[pos];
                    values[pos] = values[s];
                    values[s] = swap;
                }
                //若小结点不足以满足左子树，则重置总结点数为大结点数，重置起始结点为第一个大结点
                //即用第一个大结点划分剩余的大结点
                //迭代的最终目的是寻找一个足够大的点使pos恰为mid
                //这样的构造最终生成的左子树是不小于根结点
                else if (pos < mid)
                {
                    pos = pos - s + 1;
                    c -= pos;
                    s += pos;
                }
                else
                    break;
            }
            //若分割点不是初始结点则与其交换
            if (s != start)
            {
                swap = values[start];
                values[start] = values[s];
                values[s] = swap;
            }

            //若未考察到末结点则继续构建左右子树
            if (leftInd[start] < Count) BuildTree(leftInd[start]);
            if (rightInd[start] < Count) BuildTree(rightInd[start]);
        }

        //KD树的两种使用方式，一是范围近邻，二是k近邻

        //查找所有点的范围近邻，找到了就调用RoboticCallBack函数
        public override void FindAllInRange()
        {
            range.SetDistance(Distance);
            for (int i = values.Length - 1; i > 0; i--)
            {
                if (values[i].Skip) continue;
                FindInRange(i);
            }
        }
        //查找某点的范围近邻
        void FindInRange(int index)
        {
            int ind;
            IKDTreeData pos, value;
            float val;
            //将考察点设为范围中心
            pos = values[index];
            range.SetCenter(pos);

            //初始化整型队列并输入根结点索引
            //队列虽不是循环队列，但count的容量也足够了
            queue.Init();
            queue.Enqueue(0);

            //若队列非空则
            while (queue.Contains)
            {
                //计算队尾元素与中心元素的某维度的坐标差
                ind = queue.Dequeue();
                value = values[ind];
                val = value[dims[ind]] - pos[dims[ind]];

                //差值大于正边界则左孩子入队
                if (val > Distance)
                {
                    if (leftInd[ind] < Count) queue.Enqueue(leftInd[ind]);
                }
                //差值小于负边界则右孩子入队
                else if (val < NDistance)
                {
                    if (rightInd[ind] < Count) queue.Enqueue(rightInd[ind]);
                }
                //否则左右孩子都入队
                else
                {
                    if (leftInd[ind] < Count) queue.Enqueue(leftInd[ind]);
                    if (rightInd[ind] < Count) queue.Enqueue(rightInd[ind]);

                    //若没忽略则计算与中心元素的距离平方和
                    if (value.Skip) continue;
                    val = range.Contain(value);

                    //在距离内则传递两者坐标与距离
                    if (val <= Distance2)
                        RoboticCallBack(value, pos, (float)Math.Sqrt(val));
                }
            }
        }

        //利用KD树求障碍物附近的机器人，需要重置影响范围
        public override void FindInRange(float Distance, IKDTreeData obstacle)
        {
            int ind;
            float val, NDistance = -Distance, Distance2 = Distance * Distance;
            IKDTreeData value;
            //设置障碍物为中心
            range.SetCenter(obstacle);
            range.SetDistance(Distance);
            //初始化队列并加入根结点
            queue.Init();
            queue.Enqueue(0);
            while (queue.Contains)
            {
                //计算队尾元素与障碍物在某维的坐标差
                ind = queue.Dequeue();
                value = values[ind];
                val = value[dims[ind]] - obstacle[dims[ind]];
                //大于正界则左孩子入队
                if (val > Distance)
                {
                    if (leftInd[ind] < Count) queue.Enqueue(leftInd[ind]);
                }
                //小于负界则右孩子入队
                else if (val < NDistance)
                {
                    if (rightInd[ind] < Count) queue.Enqueue(rightInd[ind]);
                }
                //否则左右孩子都入队
                else
                {
                    if (leftInd[ind] < Count) queue.Enqueue(leftInd[ind]);
                    if (rightInd[ind] < Count) queue.Enqueue(rightInd[ind]);
                    //求实际距离的平方，若满足则调用ObstacleCallback函数
                    if (value.Skip) continue;
                    val = range.Contain(value);
                    if (val <= Distance2)
                        ObstacleCallback(value, obstacle, (float)Math.Sqrt(val));
                }
            }
        }

        //查找所有位置的K近邻
        public override void FindAllKNN()
        {
            //关于距离的一个有序列表
            SortedDistanceList<IKDTreeData> result;
            for (int i = 0; i < Count; i++)
            {
                //disList是列表的列表，机器人的树索引i与其ID是不同的，ID指定其近邻列表；
                result = disList[values[i].ID];
                //初始化列表并求取第i个机器人的邻居列表
                result.Clear();
                FindKNN(values[i], result);
            }
        }

        //查找某位置pos处的优先邻接表
        void FindKNN(IKDTreeData pos, SortedDistanceList<IKDTreeData> result)
        {
            int count = 0;
            int ind;
            double val;
            //Clear()后maxDis为最大正double
            range.SetCenter(pos);
            range.SetDistance((float)result.maxDis);
            //初始化队列并加入根结点
            queue.Init();
            queue.Enqueue(0);
            while (queue.Contains)
            {
                count++;
                //计算队尾元素与考察位置在某维的坐标差
                ind = queue.Dequeue();
                val = values[ind][dims[ind]] - pos[dims[ind]];
                //大于正界则左孩子入队
                if (val > range.Distance)
                {
                    if (leftInd[ind] < Count) queue.Enqueue(leftInd[ind]);
                }
                //小于负界则右孩子入队
                else if (val < -range.Distance)
                {
                    if (rightInd[ind] < Count) queue.Enqueue(rightInd[ind]);
                }
                //否则两孩子都入队
                else
                {
                    if (leftInd[ind] < Count) queue.Enqueue(leftInd[ind]);
                    if (rightInd[ind] < Count) queue.Enqueue(rightInd[ind]);

                    //计算累积的距离平方和
                    val = range.Contain(values[ind]);
                    //若数组已满且新添加的元素有效，则更新距离上界
                    if (result.Add(val, values[ind]))
                        range.SetDistance((float)result.maxDis);
                }
            }
        }
    }
}

//修改标识为三个?：???