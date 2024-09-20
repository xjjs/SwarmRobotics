﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

//Generate records 1
using System.Threading;
using System.IO;
//Generate records 1
namespace RobotLib.FitnessProblem{

    public class ASinglySearch : AFitness
    {
        public ASinglySearch() { }

        protected internal override Vector3 FitnessSearch(RFitness robot)
        {
            Vector3 ca, cb, delta = Vector3.Zero;
            int max, min, mid;

            //Generate records3 
            var r = robot as RFitness;
           // r.state.NewData = "Diffusion";
           

            if (r.History.Count == 0 || r.Fitness.SensorData == 0)
            {
            //    r.state.NewData = "Run";
                delta = r.postionsystem.LastMove;
            }
            else
            {
                ca = cb = r.History[0].Position;
                max = min = mid = r.History[0].Fitness;
                foreach (var his in r.History)
                {
                    if (max < his.Fitness)
                    {
                        max = his.Fitness;
                        ca = his.Position;
                    }
                    else if (his.Fitness > 0 && his.Fitness < min)
                    {
                        min = his.Fitness;
                        cb = his.Position;
                    }
                }
                if (r.Fitness.SensorData > max)
                {
                    mid = max;
                    max = r.Fitness.SensorData;
                    ca -= r.postionsystem.GlobalSensorData;
                    cb -= r.postionsystem.GlobalSensorData;
                }
                else if (r.Fitness.SensorData < min)
                {
                    mid = min;
                    min = r.Fitness.SensorData;
                    cb = cb - ca;
                    ca = r.postionsystem.GlobalSensorData - ca;
                    delta = cb;
                    cb = ca;
                    ca = delta;
                }
                else
                {
                    mid = r.Fitness.SensorData;
                    cb = cb - ca;
                    ca = robot.postionsystem.GlobalSensorData - ca;
                }

                GradientDelta(r, max, mid, min, ref delta, ref ca, ref cb);

                //delta = delta * (1 - C3) + C3 * RandPosition();
                delta = inertiaMove * NormalOrRandom(robot.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);
            }
            
            delta = NormalOrRandom(delta) * maxspeed;

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
            //        File.AppendAllText("records-bms-natural.csv", record);
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


        public override void CreateDefaultParameter() {
            base.CreateDefaultParameter();
            inertiaMove = 0.7f;
            c3 = 0.1f;

        }

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

        float inertiaMove;

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


        protected void GradientDelta(RFitness r,int max, int mid, int min, ref Vector3 delta, ref Vector3 bPos, ref Vector3 cPos)
        {
            //三者相同
            if (max == min)
            {
                delta = NormalOrRandom(r.postionsystem.LastMove);
            }//两优一差
            else if (max == mid)
            {
                //delta = cPos + Vector3.Normalize((Vector3.Zero + bPos) / 2 - cPos)*2*10;

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
                //delta = bPos + tempPos + Vector3.Normalize(delta) * 2 * 10;
                delta = NormalOrRandom(delta);
            }
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
        //Generatet records 1
        //Generatet records 2

        protected void LastProcess(RFitness r, ref Vector3 delta)
        {

           // if (r.state.SensorData == r.state.NewData) r.NumOfState++;
            //else r.NumOfState = 0;
            if (r.NumOfState > 50) r.NumOfState = 50;

            //添加避障分量
            PostDelta(r, ref delta);

            //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
            Bounding(r.postionsystem.GlobalSensorData, ref delta);

            //存储现在的位置与适应度，用速度向量保存到NewData中
            AddHistory(r);
            r.postionsystem.NewData = delta;
        }
    }

}
