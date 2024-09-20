using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem
{
    public class ARandomFitness : AFitness 
    {
        public ARandomFitness() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) 
        {
            //Ctrl+K+C, Ctrl+K+U

            //Random1（随机直线运动）：关闭HasRandomStage选项
            var last = robot.postionsystem.LastMove / maxspeed;
            if (last.Length() < 0.5) last = RandPosition();
            last *= maxspeed;
            return last;


            //Random2（单步随机移动）：关闭HasRandomStage选项
            //var last = RandPosition();
            //last *= maxspeed;
            //return last;


            //Random3（随机直线+梯度估计）：开启HasRandomStage选项
            //若初始化不在空白区域（无适应值区域），则有初始适应度值，直接进入该搜索算法（此时尚未有历史记录），则随机直线移动
            //为防止陷入无限循环，速度向量倍乘后（系数取值1-10）添加随机偏移
            //if (robot.History.Count < 3)
            //{
            //    var last = robot.postionsystem.LastMove / maxspeed;
            //    if (last.Length() < 0.5) last = RandPosition();
            //    last *= maxspeed;
            //    return last;
            //}
            //robot.RandomSearch = false;
            //Vector3 delta, maxpos, minpos;
            //int max, min, maxcount, mincount;
            //maxpos = minpos = robot.postionsystem.GlobalSensorData;
            //max = min = robot.Fitness.SensorData; maxcount = mincount = 1;
            //foreach (var his in robot.History)
            //{
            //    if (his.Fitness > max)
            //    {
            //        max = his.Fitness;
            //        maxpos = his.Position;
            //        maxcount = 1;
            //    }
            //    else if (his.Fitness == max)
            //    {
            //        maxpos += his.Position;
            //        maxcount++;
            //    }
            //    else if (his.Fitness < min)
            //    {
            //        min = his.Fitness;
            //        minpos = his.Position;
            //        mincount = 1;
            //    }
            //    else if (his.Fitness == min)
            //    {
            //        minpos += his.Position;
            //        mincount++;
            //    }
            //}
            //delta = 2 * NormalOrZero(maxpos / maxcount - minpos / mincount) + RandPosition();
            //return NormalOrZero(delta) * maxspeed;


            //Random4（随机直线+梯度估计）：开启HasRandomStage选项
            //单纯的直线运动无法确定极值，引入转向
            //直线运动时，多个最值点的均值可能会相互抵消，所以设置随机偏转

            //if (robot.History.Count < 3)
            //{
            //    var last = robot.postionsystem.LastMove / maxspeed;
            //    if (last.Length() < 0.5) last = RandPosition();
            //    last *= maxspeed;
            //    return last;
            //}
            //robot.RandomSearch = false;
            //Vector3 delta, maxpos, minpos;
            //int max, min, maxcount, mincount;
            //float alpha;
            //maxpos = minpos = robot.postionsystem.GlobalSensorData;
            //max = min = robot.Fitness.SensorData; maxcount = mincount = 1;

            //if (robot.Fitness.SensorData < robot.History[0].Fitness)
            //{

            //    alpha = MathHelper.Pi / 3 * (2 * rand.NextInt(2) - 1);
            //    delta = Vector3.Transform(NormalOrZero(robot.postionsystem.LastMove), Matrix.CreateRotationZ(alpha));
            //}
            //else
            //{
            //    foreach (var his in robot.History)
            //    {
            //        if (his.Fitness > max)
            //        {
            //            max = his.Fitness;
            //            maxpos = his.Position;
            //            maxcount = 1;
            //        }
            //        else if (his.Fitness == max)
            //        {
            //            maxpos += his.Position;
            //            maxcount++;
            //        }
            //        else if (his.Fitness < min)
            //        {
            //            min = his.Fitness;
            //            minpos = his.Position;
            //            mincount = 1;
            //        }
            //        else if (his.Fitness == min)
            //        {
            //            minpos += his.Position;
            //            mincount++;
            //        }

            //    }
            //    //                delta = maxpos / maxcount - minpos / mincount;
            //    delta = 2 * NormalOrZero(maxpos / maxcount - minpos / mincount) + RandPosition();
            //}
            //return NormalOrZero(delta) * maxspeed;

            //Random5：采用IGES策略的单独个体的策略，最优则保持、退步（非最优）则历史引导+大变化、进步（非最优）则历史导引+小变化
            //robot.RandomSearch = false;
            //Vector3 delta, history, maxpos = Vector3.Zero;
            //int max = robot.Fitness.SensorData, maxcount = 1;
            //maxpos = robot.postionsystem.GlobalSensorData;
            //max = robot.Fitness.SensorData;
            //maxcount = 1;
            //foreach (var his in robot.History)
            //{
            //    if (max < his.Fitness)
            //    {
            //        max = his.Fitness;
            //        maxpos = his.Position;
            //        maxcount = 1;
            //    }
            //    else if (max == his.Fitness)
            //    {
            //        maxpos += his.Position;
            //        maxcount++;
            //    }
            //}
            ////若当前适应度历史最优则忽略历史影响，否则计算历史最优位置的重心偏移
            //if (robot.Fitness.SensorData == max)
            //    history = Vector3.Zero;
            //else
            //    history = NormalOrZero(maxpos / maxcount - robot.postionsystem.GlobalSensorData);
            ////当前位置退步（差于第0条、首先要有历史）or上次位移太小，则历史导引（策略4） + 大的随机向量
            //if ((robot.History.Count > 0 && robot.Fitness.SensorData < robot.History[0].Fitness)
            //    || robot.postionsystem.LastMove.Length() < 0.1f)
            //    delta = RandPosition() + history;
            ////否则若存在历史更优，则历史导引（策略4） + 小的历史向量
            //else if (history.Length() > 0.1f)
            //    delta = history + RandPosition() / 10;
            ////否则若本身历史最优，则保持原方向移动（策略3）
            //else
            //    delta = NormalOrZero(robot.postionsystem.LastMove);
            //return NormalOrZero(delta) * maxspeed;

            //Random6：爬坡策略+步长自适应，最优或进步则扩大步幅（static stepSize），退步则缩小步幅+历史导引
            //RobotBase添加速度属性：MaxSpeed/Speed/MinSpeed，速度用Speed控制，限制其大小[1,10]，初值于RoboticProblem中设定
            //最优进步or保持：增大速度，原始速度向量；
            //非最优进步：减小速度，原始分量、历史分量、小随机分量（随机系数c1）；
            //退步：减小速度，历史分量、大随机分量（随机系数c2）；
            //引入加速因子apha与减速因子beta，有利于提高机器人运动的稳定性，减少不必要的能量耗费（移动距离）；

            //robot.RandomSearch = false;
            //Vector3 delta, history, maxpos = Vector3.Zero;
            //int max = robot.Fitness.SensorData, maxcount = 1;
            //maxpos = robot.postionsystem.GlobalSensorData;
            //max = robot.Fitness.SensorData;
            //maxcount = 1;
            //foreach (var his in robot.History)
            //{
            //    if (max < his.Fitness)
            //    {
            //        max = his.Fitness;
            //        maxpos = his.Position;
            //        maxcount = 1;
            //    }
            //    else if (max == his.Fitness)
            //    {
            //        maxpos += his.Position;
            //        maxcount++;
            //    }
            //}
            ////若当前适应度历史最优则忽略历史影响，否则计算历史最优位置的重心偏移
            //if (robot.Fitness.SensorData == max)
            //    history = Vector3.Zero;
            //else
            //    history = NormalOrZero(maxpos / maxcount - robot.postionsystem.GlobalSensorData);
            ////退步（差于第0条、首先要有历史）or上次位移太小
            //if ((robot.History.Count > 0 && robot.Fitness.SensorData < robot.History[0].Fitness)
            //    || robot.postionsystem.LastMove.Length() < 0.1f)
            //{
            //    if (robot.MaxSpeed != robot.MinSpeed)
            //        robot.Speed *= beta;
            //    delta = RandPosition() + history;
            //}
            ////非最优进步，则历史导引（策略4） + 小的历史向量
            //else if (history.Length() > 0.1f)
            //{
            //    if (robot.MaxSpeed != robot.MinSpeed) robot.Speed *= beta;
            //    //robot.postionsystem.LastMove/10 + 
            //    delta = history + RandPosition() / 10;
            //}
            ////最优，则保持原方向移动（策略3）
            //else
            //{
            //    if (robot.MaxSpeed != robot.MinSpeed) robot.Speed *= alpha;
            //    delta = robot.postionsystem.LastMove;
            //}
            //return NormalOrZero(delta) * robot.Speed;
        }

        public override void Update(RobotBase robot, RunState state)
        {
            var r = robot as RFitness;
            Vector3 delta = Vector3.Zero;

            //目标非空表示机器人已经发现目标，而且可以立即处理目标
            if (null != r.Target)
            {
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
                    if (inputLayer[26, 0] < r.inertiaState
                        && (r.state.SensorData == problem.statelist[0] || r.state.SensorData == problem.statelist[4]))
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
                //Generate records 8
                //数据采集条件与实验结束条件
                //if(state.Iterations == state.IterationNum && state.RobotID == r.id && false)
                //{
                //    state.Finished = true;

                //    GenerateOutput(r, ref delta);
                //    StreamWriter sw = new StreamWriter("records.csv", true);
                //    for (int i = 0; i < 30; i++) sw.Write("{0},", inputLayer[i, 0]);
                //    for (int i = 0; i < 2; i++) sw.Write("{0},", outputLayer[i]);
                //    sw.WriteLine();
                //    sw.Close();
                //}
                //Generate records 8
                LastProcess(r, ref delta);
            }
        }

        void StartDiffusionOrSearch(RFitness r, RunState state, ref Vector3 delta)
        {
            r.inertiaState = InitialInertiaState;
            //Generate records 5, inputLayer[27,0] replace rand.NextDouble()
            if (r.state.SensorData == problem.statelist[0])
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

        void ContinueDiffusionOrSearch(RFitness r, RunState state, ref Vector3 delta)
        {
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

        protected Vector3 RandPos(int direction)
        {
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
        protected void SelectDirection(RFitness r, ref Vector3 delta)
        {
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
            else if (inputLayer[28, 0] < 0.5f) delta = RandPos(Min1);
            else delta = RandPos(Min2);
            if (direction[1] == direction[2] && direction[2] == direction[3] && direction[3] == direction[4])
            {
                delta = RandPos(0);
            }
        }
        protected void TriangleSearch(RFitness r, ref Vector3 delta)
        {

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
                if (r.Neighbours.Count < 1) delta = Vector3.Normalize(delta) * 0.9f + 0.1f * RandPosition();
                delta = inertiaMove * NormalOrRandom(r.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);
                delta = NormalOrRandom(delta);
            }

        }
        protected void GradientDelta(RFitness r, int max, int mid, int min, ref Vector3 delta, ref Vector3 bPos, ref Vector3 cPos)
        {
            //三者相同
            if (max == min)
            {
                delta = NormalOrRandom(r.postionsystem.LastMove);
            }//两优一差
            else if (max == mid)
            {

                delta = NormalOrRandom((Vector3.Zero + bPos) / 2 - cPos);
            }//两差一优
            else if (min == mid)
            {
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

        protected void LastProcess(RFitness r, ref Vector3 delta)
        {

            if (r.state.NewData == r.state.SensorData) r.NumOfState++;
            else r.NumOfState = 0;

            if (r.NumOfState > 50) r.NumOfState = 50;

            //添加避障分量
            PostDelta(r, ref delta);

            //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
            Bounding(r.postionsystem.GlobalSensorData, ref delta);

            //存储现在的位置与适应度，用速度向量保存到NewData中
            AddHistory(r);
            r.postionsystem.NewData = delta;
        }


        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
            inertiaMove = 0.5f;
            initialInertiaState = 0.9997f;

        }
        float inertiaMove;
        float initialInertiaState;

        //Generate records 1
        double[,] inputLayer = new double[30, 1];
        double[] outputLayer = new double[2];
        protected void GenerateInput(RFitness r)
        {
            //internal state
            double stateValue = 0;
            switch (r.state.SensorData)
            {
                case "Run": stateValue = 0; break;
                case "Diffusion": stateValue = 1; break;
                default: stateValue = 0.5; break;
            }
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
            if ("Run" == r.state.NewData) outputLayer[0] = 0;
            else if ("Diffusion" == r.state.NewData) outputLayer[0] = 1;
            else outputLayer[0] = 0.5;
            outputLayer[1] = Math.Acos(delta.X / Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y));
            if (delta.Y < 0) outputLayer[1] = -outputLayer[1];
            outputLayer[1] = (outputLayer[1] + Math.PI) * 0.5 / Math.PI;
        }
        //Generatet records 1


        [Parameter(ParameterType.Float, Description = "Move Inertia")]
        public float InertiaMove
        {
            get { return inertiaMove; }
            set
            {
                if (value > 0.99f) inertiaMove = 0.99f;
                else if (value < 0.0f) inertiaMove = 0.0f;
                else inertiaMove = value;
            }
        }
        [Parameter(ParameterType.Float, Description = "State Inertia")]
        public float InitialInertiaState
        {
            get { return initialInertiaState; }
            set
            {
                if (value > 1.0f) initialInertiaState = 1.0f;
                else if (value < 0.1f) initialInertiaState = 0.1f;
                else initialInertiaState = value;
            }
        }
    }
}
