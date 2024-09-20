using System;
using Microsoft.Xna.Framework;

//Generate records 1
using System.Threading;
using System.IO;
//Generate records 1

namespace RobotLib.FitnessProblem
{
    //原始的RPSO的效率较差，于是参考PSOE做了两处改进：
    //改进1：只有历史更优时引入随机分量以防止局部循环振荡
    //改进2：保证个体的速度，在没有指引信息的情况下保证“惯性速度”接近最大值
    //若关闭HasRandomState（并行仿真时常用的设置），则算法会在无适应度值区域随机漂浮，虽然算法一开始有考虑惯性分量last，
    //但只有当其非常小（小于0.1f）时才会重置，从而允许大量低速粒子的存在(如1大于0.1f)，不能保证“惯性速度”
	public class AdaPSOFitness : AFitness
	{
        public AdaPSOFitness() { }

        protected internal override Vector3 FitnessSearch(RFitness robot)
        {

            bool hasN = false;
            Vector3 delta = Vector3.Zero;
            float h, s, y;
            //delta = w * robot.postionsystem.LastMove;



            ////////////self-history
            int max = robot.Fitness.SensorData;
            Vector3 maxpos = Vector3.Zero;
            foreach (var item in robot.History)
            {
                if (item.Fitness > max)
                {
                    max = item.Fitness;
                    maxpos = item.Position;
                }
            }
            if (max > robot.Fitness.SensorData)
            {
                delta += C1 * (float)rand.NextDouble() * (maxpos - robot.postionsystem.GlobalSensorData);
            }

            if (0 == robot.cnt) h = 0;
            else if (max > robot.cnt) h = 1 - (float)robot.cnt / max;
            else h = 1 - (float)max / robot.cnt;
            robot.cnt = max;


            ///////////neighbour
            max = robot.Fitness.SensorData;
            float mean = max;
            RFitness r;
            foreach (var item in robot.Neighbours)
            {
                r = item.Target as RFitness;
                mean += r.Fitness.SensorData;
                if (r.Fitness.SensorData > max)
                {
                    max = r.Fitness.SensorData;
                    maxpos = r.postionsystem.GlobalSensorData;
                }
            }
            if (max > robot.Fitness.SensorData)
            {
                delta += C2 * (float)rand.NextDouble() * (maxpos - robot.postionsystem.GlobalSensorData);
                hasN = true;
            }

            mean = mean / (robot.Neighbours.Count + 1);
            if (0 == max) s = 1;
            else s = mean / max;
            if (max == robot.Fitness.SensorData) y = 1;
            else y = (float)(2 - (2 - 0.5) * robot.interNum / 500);

            w = (float)(y * (1 - alpha * h + beta * s));

            delta += w * robot.postionsystem.LastMove;

            //在没有邻居信息指导的情况下，要添加随机分量
            if (!hasN)
            {
                if (delta.Length() != 0)
                    delta = Vector3.Normalize(delta) * (1 - C3) + C3 * RandPosition();
            }

            while (delta.Length() < 0.1f)
                delta = RandPosition();

            delta = Vector3.Normalize(delta) * maxspeed;


            ////Generate records3 
            //r = robot as RFitness;
            //if (r.History.Count == 0 || r.Fitness.SensorData == 0)
            //{
            //    r.state.NewData = "Diffusion";
            //}
            //else
            //{
            //    r.state.NewData = "Run";
            //}
            ////Generate records3 
            ////Generate records 3
            //GenerateInput(r);
            ////数据采集条件与实验结束条件
            //if (instate.Iterations == instate.IterationNum && instate.RobotID == r.id)
            //{
            //    instate.Finished = true;
            //    GenerateOutput(r, ref delta);
            //    string record = inputLayer[0, 0].ToString();
            //    for (int i = 1; i < 30; i++) record = record + "," + inputLayer[i, 0].ToString();// sw.Write("{0},", inputLayer[i, 0]);
            //    record += "," + outputLayer[0].ToString() + "," + outputLayer[1].ToString() + System.Environment.NewLine;
            //    //for (int i = 0; i < 2; i++) record = record + "," + outputLayer[i].ToString(); //sw.Write("{0},", outputLayer[i]);
            //    //ReaderWriterLockSlim writeLock = new ReaderWriterLockSlim();
            //    //writeLock.EnterWriteLock();
            //    try
            //    {
            //        File.AppendAllText("records-arpso-natural.csv", record);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}
            ////Generate records 8
            //if (r.state.NewData == r.state.SensorData) r.NumOfState++;
            //else r.NumOfState = 0;

            return delta;

        }

        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();

            c1 = 0.6f;
            c2 = 0.4f;
            c3 = 0.1f;
            w = 1.0f;
            alpha = 0.3f;
            beta = 0.7f;
        }

        //Generate records 2
        double[,] inputLayer = new double[30, 1];
        double[] outputLayer = new double[2];
        protected void GenerateInput(RFitness r)
        {
            //机器人个体内置罗盘（规定局部坐标系的方向），相对位置与全局坐标系统一，与机器人朝向无关
            //internal state
            double stateValue = 0;

            //数据采集更改2-1
            //switch (r.state.SensorData)
            //{
            //    case "Run": stateValue = 0; break;
            //    case "Diffusion": stateValue = 1; break;
            //    default: stateValue = 0.5; break;
            //}
            switch (r.state.SensorData)
            {
                case "Run": stateValue = -1; break;
                case "Diffusion": stateValue = 1; break;
                default: stateValue = 0; break;
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

            //if ("Run" == r.state.NewData) outputLayer[0] = 0;
            //else if ("Diffusion" == r.state.NewData) outputLayer[0] = 1;
            //else outputLayer[0] = 0.5;
            //outputLayer[1] = Math.Acos(delta.X / Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y));
            //if (delta.Y < 0) outputLayer[1] = -outputLayer[1];
            ////角度归一化到0到1
            //outputLayer[1] = (outputLayer[1] + Math.PI) * 0.5 / Math.PI;

            if ("Run" == r.state.NewData) outputLayer[0] = -1;
            else if ("Diffusion" == r.state.NewData) outputLayer[0] = 1;
            else outputLayer[0] = 0;
            outputLayer[1] = Math.Acos(delta.X / Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y));
            //角度保持在-pi到pi
            if (delta.Y < 0) outputLayer[1] = -outputLayer[1];
        }
        //Generatet records 2


        float w, c1, c2, c3, alpha, beta;

        [Parameter(ParameterType.Float, Description = "w")]
        public float W
        {
            get { return w; }
            set
            {
                if (value < 0 || value >= 5) throw new Exception("Must be in [0,5)");
                w = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "c1")]
        public float C1
        {
            get { return c1; }
            set
            {
                if (value < 0) throw new Exception("Must be in positive");
                c1 = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "c2")]
        public float C2
        {
            get { return c2; }
            set
            {
                if (value < 0) throw new Exception("Must be in positive");
                c2 = value;
            }
        }

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

        [Parameter(ParameterType.Float, Description = "Alpha")]
        public float Alpha
        {
            get { return alpha; }
            set
            {
                if (value < 0 || value > 1) throw new Exception("Must be in [0,1)");
                alpha = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Beta")]
        public float Beta
        {
            get { return beta; }
            set
            {
                if (value < 0 || value > 1) throw new Exception("Must be in [0,1)");
                beta = value;
            }
        }
	}
}
