using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;
using System.Threading;

//Generate records 2
using System.IO;
//Generate records 2


namespace RobotLib.FitnessProblem{
    public class APGESFitness : AFitness
    {
        public APGESFitness() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) { return Vector3.Zero; }
        public override void Update(RobotBase robot, RunState state) {
            var r = robot as RFitness;
            Vector3 delta = Vector3.Zero;

            //目标非空表示机器人已经发现目标，而且可以立即处理目标
            if (null != r.Target)  
            {
                r.inertiaState = 0.0f;   //Ensure restart the diffusion or search after collection
                problem.CollectTarget(r, state as SFitness);
                r.History.Clear();
            }
            else
            {
                //Generate records 3
                GenerateInput(r);
                //Generate records 3

                foreach (var nbr in r.Neighbours)
                {
                    if (null != (nbr.Target as RFitness).Target)
                    {
                        r.Target = (nbr.Target as RFitness).Target;
                        break;
                    }
                }
                if (null != r.Target)
                {
                    delta = r.Target.Position - r.postionsystem.GlobalSensorData;
                    r.Target = null;
                    if (delta.Length() > maxspeed) delta = Vector3.Normalize(delta) * maxspeed;

                }
                else
                {
                    //Generate records 4, inputLayer[26,0] replace rand.NextDouble()
                    if (inputLayer[26,0] < r.inertiaState
                        && (r.state.SensorData == problem.statelist[0]||r.state.SensorData == problem.statelist[4]))
                    {
                        ContinueDiffusionOrSearch(r, state, ref delta);
                    }
                    else
                    {
                        StartDiffusionOrSearch(r, state, ref delta);
                        //检测扩散策略的执行次数，实际上一开始大多都选为扩散状态，但之后几乎不再继续选择扩散了（群体密度小）
                        if (problem.statelist[4] == r.state.NewData) state.SingleNum++;
                    }
                }
                ////Generate records 8
                ////数据采集条件与实验结束条件
                //if (state.Iterations == state.IterationNum && state.RobotID == r.id)
                //{
                //    //state.Finished = true;
                //    GenerateOutput(r, ref delta);
                //    string record = inputLayer[0, 0].ToString();
                //    for (int i = 1; i < 30; i++) record = record + "," + inputLayer[i, 0].ToString();// sw.Write("{0},", inputLayer[i, 0]);
                //    record += "," + outputLayer[0].ToString() + "," + outputLayer[1].ToString() + System.Environment.NewLine;
                //    //for (int i = 0; i < 2; i++) record = record + "," + outputLayer[i].ToString(); //sw.Write("{0},", outputLayer[i]);
                //    //ReaderWriterLockSlim writeLock = new ReaderWriterLockSlim();
                //    //writeLock.EnterWriteLock();
                //    try
                //    {
                //        File.AppendAllText("records-m2.csv", record);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e.Message);
                //    }
                //}
                ////Generate records 8
                LastProcess(r, ref delta);
            }
        }

        void StartDiffusionOrSearch(RFitness r, RunState state, ref Vector3 delta) {
            r.inertiaState = InitialInertiaState;
            //Generate records 5, inputLayer[27,0] replace rand.NextDouble()
            if (inputLayer[27,0] < ProbabilityOfDiffusion(r.Neighbours.Count))
            {
                SelectDirection(r, ref delta);
                delta = NormalOrRandom(delta) * maxspeed;
                r.state.NewData = problem.statelist[4];
            }
            else
            {
                TriangleSearch(r, ref delta);
                r.state.NewData = problem.statelist[0];
            }
        }

        void ContinueDiffusionOrSearch(RFitness r, RunState state, ref Vector3 delta){
            r.inertiaState *= InitialInertiaState;
            if (r.state.SensorData == problem.statelist[4])
            {
                delta = NormalOrRandom(r.postionsystem.LastMove) * maxspeed;
                r.state.NewData = problem.statelist[4];
            }
            else
            {
                TriangleSearch(r, ref delta);
                //delta = inertiaMove * NormalOrRandom(r.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);
                delta = NormalOrRandom(delta) * maxspeed;
                r.state.NewData = problem.statelist[0];
            }
        }

        protected Vector3 RandPos(int direction) {
            double ang;
            //Generate records 7, inputLayer[29,0] replace rand.NextDouble();
            switch (direction)
            {
                case 1: ang = (inputLayer[29, 0] * 2 + 1) * MathHelper.PiOver4; break;
                case 2: ang = (inputLayer[29, 0] * 2 + 5) * MathHelper.PiOver4; break;
                case 3: ang = (inputLayer[29, 0] * 2 + 3) * MathHelper.PiOver4; break;
                case 4: ang = (inputLayer[29, 0] * 2 - 1) * MathHelper.PiOver4; break;
                default: ang = inputLayer[29, 0] * MathHelper.TwoPi; break;
            }
            return new Vector3((float)Math.Cos(ang), (float)Math.Sin(ang), 0);
        }
        protected void SelectDirection(RFitness r, ref Vector3 delta) {
            int[] direction = new int[5] { 0, 0, 0, 0, 0 };
            int Min1 = 1, Min2 = 2, temp;
            foreach (var nbr in r.Neighbours)
            {
                if (nbr.offset.Y > 0) direction[1]++;
                else if (nbr.offset.Y < 0) direction[2]++;
                if (nbr.offset.X < 0) direction[3]++;
                else if (nbr.offset.X > 0) direction[4]++;
            }
            if (direction[Min1] > direction[Min2])
            {
                Min1 = 2;
                Min2 = 1;
            }
            if (direction[Min1] > direction[3])
            {
                temp = Min1;
                Min1 = 3;
                Min2 = temp;
            }
            if (direction[Min1] > direction[4])
            {
                temp = Min1;
                Min1 = 4;
                Min2 = temp;
            }
            //Generate records 6, inputLayer[28,0] replace rand.NextDouble()
            if (direction[Min1] < direction[Min2]) delta = RandPos(Min1);
            else if (inputLayer[28,0] < 0.5f) delta = RandPos(Min1);
            else delta = RandPos(Min2);
            if (direction[1] == direction[2] && direction[2] == direction[3] && direction[3] == direction[4])
            {
                delta = RandPos(0);
            }
        }
        protected void TriangleSearch(RFitness r, ref Vector3 delta) {

            Vector3 ca, cb;
            int max, min, mid;

            if (r.Neighbours.Count + r.History.Count < 2 || r.Fitness.SensorData == 0)
            {
                delta = NormalOrRandom(r.postionsystem.LastMove) * maxspeed;
            }
            else
            {
                max = -1;
                min = 100;
                mid = r.Fitness.SensorData;
                ca = cb = Vector3.Zero;

                if (r.Neighbours.Count > 1)
                {
                    max = min = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                    ca = cb = r.Neighbours[0].offset;
                    //max = min = r.Fitness.SensorData;
                    //ca = cb = Vector3.Zero;
                    foreach (var nbr in r.Neighbours)
                    {
                        //if (nbr.offset.Length() < 20)
                        {

                            if (max < (nbr.Target as RFitness).Fitness.SensorData)
                            {
                                max = (nbr.Target as RFitness).Fitness.SensorData;
                                ca = nbr.offset;

                            }
                            else if (min > (nbr.Target as RFitness).Fitness.SensorData)
                            {
                                min = (nbr.Target as RFitness).Fitness.SensorData;
                                cb = nbr.offset;
                            }
                        }
                    }
                    //foreach (var nbr in r.Neighbours)
                    //{
                    //    if (max < (nbr.Target as RFitness).Fitness.SensorData)
                    //    {
                    //        if (max == -1)
                    //        {
                    //            max = (nbr.Target as RFitness).Fitness.SensorData;
                    //            ca = nbr.offset;
                    //        }
                    //        else
                    //        {
                    //            min = max;
                    //            cb = ca;
                    //            max = (nbr.Target as RFitness).Fitness.SensorData;
                    //            ca = nbr.offset;
                    //        }
                    //    }
                    //    else if (min > (nbr.Target as RFitness).Fitness.SensorData)
                    //    {
                    //        min = (nbr.Target as RFitness).Fitness.SensorData;
                    //        cb = nbr.offset;
                    //    }
                    //}
                }
                else if (r.Neighbours.Count == 1)// && r.Neighbours[0].offset.Length() < 20)
                {
                    foreach (var his in r.History)
                    {
                        if (max < his.Fitness)
                        {
                            max = his.Fitness;
                            ca = his.Position - r.postionsystem.GlobalSensorData;
                        }
                    }

                    if (max > (r.Neighbours[0].Target as RFitness).Fitness.SensorData)
                    {
                        min = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                        cb = r.Neighbours[0].offset;
                    }
                    else
                    {
                        min = max;
                        cb = ca;
                        max = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                        ca = r.Neighbours[0].offset;
                    }
                }
                else
                {
                    if (r.History.Count > 0)
                    {
                        max = min = r.History[0].Fitness;
                        ca = cb = r.History[0].Position - r.postionsystem.GlobalSensorData;
                        foreach (var his in r.History)
                        {
                            if (max < his.Fitness)
                            {
                                max = his.Fitness;
                                ca = his.Position - r.postionsystem.GlobalSensorData;
                            }
                            else if (min > his.Fitness)
                            {
                                min = his.Fitness;
                                cb = his.Position - r.postionsystem.GlobalSensorData;
                            }
                        }
                    }

                    //foreach (var his in r.History)
                    //{
                    //    if (max < his.Fitness)
                    //    {
                    //        if (max == -1)
                    //        {
                    //            max = his.Fitness;
                    //            ca = his.Position - r.postionsystem.GlobalSensorData;
                    //        }
                    //        else
                    //        {
                    //            min = max;
                    //            cb = ca;
                    //            max = his.Fitness;
                    //            ca = his.Position - r.postionsystem.GlobalSensorData;
                    //        }
                    //    }
                    //    else if (min > his.Fitness)
                    //    {
                    //        min = his.Fitness;
                    //        cb = his.Position - r.postionsystem.GlobalSensorData;
                    //    }
                    //}
                }

                if (r.Fitness.SensorData > max)
                {
                    mid = max;
                    max = r.Fitness.SensorData;
                }
                else if (r.Fitness.SensorData < min)
                {
                    mid = min;
                    min = r.Fitness.SensorData;
                    delta = -ca;
                    ca = delta + cb;
                    cb = delta;
                }
                else
                {
                    mid = r.Fitness.SensorData;
                    cb = cb - ca;
                    ca = -ca;
                }
                GradientDelta(r, max, mid, min, ref delta, ref ca, ref cb);
                //delta += RandPosition() * 0.1f;
                if (r.Neighbours.Count < 1)
                {
                    delta = Vector3.Normalize(delta) * 0.9f + 0.1f * RandPosition();

                }
                else
                {
                    delta = inertiaMove * NormalOrRandom(r.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);
                }
                delta = NormalOrRandom(delta);
            }

        }
        protected void GradientDelta(RFitness r, int max, int mid, int min, ref Vector3 delta, ref Vector3 bPos, ref Vector3 cPos)
        {
            //处理三者共线
            //delta.X = -bPos.Y;
            //delta.Y = bPos.X;

            //三者相同
            if (max == min)
            {
                 delta = NormalOrRandom(r.postionsystem.LastMove);

            }//两优一差
            else if (max == mid)
            {
                //if (Vector3.Dot(delta, cPos) < 0.01f) delta = RandPosition();
                //else 
                    delta = NormalOrRandom((Vector3.Zero + bPos) / 2 - cPos);
            }//两差一优
            else if (min == mid)
            {
                //if (Vector3.Dot(delta, cPos) < 0.01f) delta = RandPosition();
                //else 
                    delta = NormalOrRandom(Vector3.Zero - (bPos + cPos) / 2);
            }//各不相同
            else
            {
                Vector3 tempPos = (Vector3.Zero * (mid - min) + cPos * (max - mid)) / (max - min);
                tempPos -= bPos;
                delta.X = -tempPos.Y;
                delta.Y = tempPos.X;
                if (Vector3.Dot(bPos - cPos, delta) < 0)
                {
                    delta.X = tempPos.Y;
                    delta.Y = -tempPos.X;
                }
                delta = NormalOrRandom(delta);
            }
        }

        protected void LastProcess(RFitness r, ref Vector3 delta) {

            if (r.state.NewData == r.state.SensorData) r.NumOfState++;
            else r.NumOfState = 0;

            //对状态迭代次数进行截断，有必要么？
            //if (r.NumOfState > 50) r.NumOfState = 50;

            //添加避障分量
            PostDelta(r, ref delta);

            //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
            Bounding(r.postionsystem.GlobalSensorData, ref delta);

            //存储现在的位置与适应度，用速度向量保存到NewData中
            AddHistory(r);
            r.postionsystem.NewData = delta;
        }

        float ProbabilityOfDiffusion(int N) {
            if (N > DiffusionThreshold) return (float)(N - DiffusionThreshold) / N;
            else return 0f;
        }

        public override void CreateDefaultParameter() {
            base.CreateDefaultParameter();

            //initialInertiaState = 0.99963f;
            initialInertiaState = 0.9997f;
            //inertiaMove = 0.5f;
            //inertiaMove = 0.0f;
            inertiaMove = 0.55f;
            //diffusionThreshold = 1.9f;
            //diffusionThreshold = 2.0f;
            diffusionThreshold = 2.3f;
        }
        float inertiaMove;
        float initialInertiaState;
        float diffusionThreshold;

        //Generate records 1
        double[,] inputLayer = new double[30, 1];
        double[] outputLayer = new double[2];
        protected void GenerateInput(RFitness r) 
        {
            //机器人个体内置罗盘（规定局部坐标系的方向），相对位置与全局坐标系统一，与机器人朝向无关
            //internal state
            double stateValue = 0;

            //数据采集更改2-1
            switch (r.state.SensorData)
            {
                case "Run": stateValue = 0; break;
                case "Diffusion": stateValue = 1; break;
                default: stateValue = 0.5; break;
            }
            //switch (r.state.SensorData)
            //{
            //    case "Run": stateValue = -1; break;
            //    case "Diffusion": stateValue = 1; break;
            //    default: stateValue = 0; break;
            //}

            inputLayer[0, 0] = stateValue;
            inputLayer[1, 0] = r.NumOfState;
            inputLayer[2, 0] = r.postionsystem.LastMove.X;
            inputLayer[3, 0] = r.postionsystem.LastMove.Y;

            //external info
            //1.Target
            inputLayer[4, 0] = 0;
            inputLayer[5, 0] = 0;
            inputLayer[6, 0] = 0;
            foreach (var nbr in r.Neighbours)
            {
                if (null != (nbr.Target as RFitness).Target)
                {
                    inputLayer[4, 0] = 1;
                    inputLayer[5, 0] = (nbr.Target as RFitness).Target.Position.X - r.postionsystem.GlobalSensorData.X;
                    inputLayer[6, 0] = (nbr.Target as RFitness).Target.Position.Y - r.postionsystem.GlobalSensorData.Y;
                    break;
                }
            }
            //屏幕虚拟坐标：Right为+X、Down为+Y、In为+Z，故下方的机器人数统计为下、上、右、左
            //2.Robots distribution
            inputLayer[7, 0] = r.Neighbours.Count;
            inputLayer[8, 0] = inputLayer[9, 0] = inputLayer[10, 0] = inputLayer[11, 0] = 0;
            foreach (var nbr in r.Neighbours)
            {
                if (nbr.offset.Y > 0) inputLayer[8, 0]++;
                else if (nbr.offset.Y < 0) inputLayer[9, 0]++;
                if (nbr.offset.X > 0) inputLayer[10, 0]++;
                else if (nbr.offset.X < 0) inputLayer[11, 0]++;
            }
            //3.Fitness
            double min = 100, max = -1;
            Vector3 minPos = Vector3.Zero, maxPos = Vector3.Zero;
            inputLayer[12, 0] = r.History.Count;
            if (0 == r.History.Count)
            {
                inputLayer[13, 0] = inputLayer[14, 0] = inputLayer[15, 0] = 0;
                inputLayer[16, 0] = inputLayer[17, 0] = inputLayer[18, 0] = 0;
            }
            else if (1 == r.History.Count)
            {
                inputLayer[13, 0] = r.History[0].Position.X - r.postionsystem.GlobalSensorData.X;
                inputLayer[14, 0] = r.History[0].Position.Y - r.postionsystem.GlobalSensorData.Y;
                inputLayer[15, 0] = r.History[0].Fitness;
                inputLayer[16, 0] = inputLayer[17, 0] = inputLayer[18, 0] = 0;
            }
            else
            {
                max = min = r.History[0].Fitness;
                minPos = maxPos = r.History[0].Position - r.postionsystem.GlobalSensorData;
                foreach (var his in r.History)
                {
                    if (max < his.Fitness)
                    {
                        max = his.Fitness;
                        maxPos = his.Position - r.postionsystem.GlobalSensorData;
                    }
                    else if (min > his.Fitness)
                    {
                        min = his.Fitness;
                        minPos = his.Position - r.postionsystem.GlobalSensorData;
                    }
                }
                inputLayer[13, 0] = maxPos.X; inputLayer[14, 0] = maxPos.Y; inputLayer[15, 0] = max;
                inputLayer[16, 0] = minPos.X; inputLayer[17, 0] = minPos.Y; inputLayer[18, 0] = min;

            }

            inputLayer[19, 0] = r.Fitness.SensorData;
            if (0 == r.Neighbours.Count)
            {
                inputLayer[20, 0] = inputLayer[21, 0] = inputLayer[22, 0] = 0;
                inputLayer[23, 0] = inputLayer[24, 0] = inputLayer[25, 0] = 0;
            }
            else if (1 == r.Neighbours.Count)
            {
                inputLayer[20, 0] = r.Neighbours[0].offset.X;
                inputLayer[21, 0] = r.Neighbours[0].offset.Y;
                inputLayer[22, 0] = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                inputLayer[23, 0] = inputLayer[24, 0] = inputLayer[25, 0] = 0;
            }
            else
            {
                max = min = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                minPos = maxPos = r.Neighbours[0].offset;
                foreach (var nbr in r.Neighbours)
                {
                    if (max < (nbr.Target as RFitness).Fitness.SensorData)
                    {
                        max = (nbr.Target as RFitness).Fitness.SensorData;
                        maxPos = nbr.offset;
                    }
                    else if (min > (nbr.Target as RFitness).Fitness.SensorData)
                    {
                        min = (nbr.Target as RFitness).Fitness.SensorData;
                        minPos = nbr.offset;
                    }
                }
                inputLayer[20, 0] = maxPos.X; inputLayer[21, 0] = maxPos.Y; inputLayer[22, 0] = max;
                inputLayer[23, 0] = minPos.X; inputLayer[24, 0] = minPos.Y; inputLayer[25, 0] = min;
            }

            //random variants
            inputLayer[26, 0] = rand.NextDouble();  //Maintain State 
            inputLayer[27, 0] = rand.NextDouble();  //Diffusion Control
            inputLayer[28, 0] = rand.NextDouble();  //Direction Selection
            inputLayer[29, 0] = rand.NextDouble();  //Direction Generation
        }
        protected void GenerateOutput(RFitness r, ref Vector3 delta) 
        {

            //数据采集更改2-2

            if ("Run" == r.state.NewData) outputLayer[0] = 0;
            else if ("Diffusion" == r.state.NewData) outputLayer[0] = 1;
            else outputLayer[0] = 0.5;
            outputLayer[1] = Math.Acos(delta.X / Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y));
            if (delta.Y < 0) outputLayer[1] = -outputLayer[1];
            //角度归一化到0到1
            outputLayer[1] = (outputLayer[1] + Math.PI) * 0.5 / Math.PI;

            //if ("Run" == r.state.NewData) outputLayer[0] = -1;
            //else if ("Diffusion" == r.state.NewData) outputLayer[0] = 1;
            //else outputLayer[0] = 0;
            //outputLayer[1] = Math.Acos(delta.X / Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y));
            ////角度保持在-pi到pi
            //if (delta.Y < 0) outputLayer[1] = -outputLayer[1];
        }
        //Generatet records 1


        [Parameter(ParameterType.Float, Description = "Move Inertia")]
        public float InertiaMove {
            get { return inertiaMove; }
            set {
                if (value > 0.99f) inertiaMove = 0.99f;
                else if (value < 0.0f) inertiaMove = 0.0f;
                else inertiaMove = value;
            }
        }
        [Parameter(ParameterType.Float, Description = "State Inertia")]
        public float InitialInertiaState {
            get { return initialInertiaState; }
            set {
                if (value > 1.0f) initialInertiaState = 1.0f;
                else if (value < 0.1f) initialInertiaState = 0.1f;
                else initialInertiaState = value;
            }
        }
        [Parameter(ParameterType.Int, Description = "Diffusion Threshold")]
        public float DiffusionThreshold {
            get { return diffusionThreshold; }
            set {
                if (value > 10) diffusionThreshold = 10;
                else if (value < 0.1) diffusionThreshold = 0.1f;
                else diffusionThreshold = value;
            }
        }


        //稳定性测试
        float c3;
        [Parameter(ParameterType.Float, Description = "c3")]
        public float C3
        {
            get { return c3; }
            set
            {
                if (value < 0) throw new Exception("Must be in positive");
                c3 = value;
            }
        }
    }
}
