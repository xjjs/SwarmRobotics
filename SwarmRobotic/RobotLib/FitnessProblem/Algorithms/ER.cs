using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;
using Emgu.CV;
using System.IO;


namespace RobotLib.FitnessProblem {
    public class ER : AFitness {
        static int sizeM = 15;
        public ER() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) { return Vector3.Zero; }

        public override void Update(RobotBase robot, RunState state) {
            var r = robot as RFitness;
            Vector3 delta = Vector3.Zero;

            if (!assignFlag)
            {
                assignFlag = true;
                if (!fileFlag)
                {
                    wiArray = new double[sizeM, 30];
                    wmArray = new double[5, sizeM];
                    woArray = new double[2, 5];
                    offsetArray = new double[7 + sizeM, 1];

                    StreamReader sr = new StreamReader("w1552.txt");
                    for (int i = 0; i < sizeM; i++)
                    {
                        string line = sr.ReadLine();
                        string[] abc = line.Split('\t');
                        for (int j = 0; j < 30; j++) wiArray[i, j] = double.Parse(abc[j]);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        string line = sr.ReadLine();
                        string[] abc = line.Split('\t');
                        for (int j = 0; j < sizeM; j++) wmArray[i, j] = double.Parse(abc[j]);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        string line = sr.ReadLine();
                        string[] abc = line.Split('\t');
                        for (int j = 0; j < 5; j++) woArray[i, j] = double.Parse(abc[j]);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        string line = sr.ReadLine();
                        string[] abc = line.Split('\t');
                        for (int j = 0; j < 7 + sizeM; j++) offsetArray[j, i] = double.Parse(abc[j]);

                    }
                    sr.Close();
                }

                //int scale = 1;
                //for (int i = 0; i < 10; i++)
                //{
                //    for (int j = 0; j < 30; j++)
                //    {
                //        //前馈ANN有专门的初始化方式，根据一定的概率分布（这写在CMA中，此处不必考虑）
                //        wiArray[i, j] = (rand.NextDouble() * 2 - 1) * scale;
                //    }
                //}

                //for (int i = 0; i < 2; i++)
                //{
                //    for (int j = 0; j < 10; j++)
                //    {
                //        woArray[i, j] = (rand.NextDouble() * 2 - 1) * scale;
                //    }
                //}

                //for (int i = 0; i < 12; i++)
                //{
                //    offsetArray[i, 0] = (rand.NextDouble() * 2 - 1) * scale;
                //}
                wi.Data = wiArray;
                wm.Data = wmArray;
                wo.Data = woArray;
                offset.Data = offsetArray;
            }


            //目标非空表示机器人已经发现目标，而且可以立即处理目标
            if (null != r.Target)
            {
                problem.CollectTarget(r, state as SFitness);
                r.History.Clear();
            }
            else
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
                if (inputLayer[1, 0] > 50)
                {
                    inputLayer[1, 0] = 50;
                }
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
                inputLayer[26, 0] = rand.NextDouble();
                inputLayer[27, 0] = rand.NextDouble();
                inputLayer[28, 0] = rand.NextDouble();
                inputLayer[29, 0] = rand.NextDouble();

                //minmax归一化
                //double[,] minmax = new double[30,2];
                //StreamReader sr = new StreamReader("mstd.txt");
                //for (int i = 0; i < 30; i++)
                //{
                //    string line = sr.ReadLine();
                //    string[] abc = line.Split('\t');
                //    for (int j = 0; j < 2; j++) minmax[i,j] = double.Parse(abc[j]);
                //}
                //sr.Close();

                double[,] minmax = new double[30,2]{
{0,1
},{35.9190375302663,16.8315206845476
},{0.265752116445215,3.29673762391646
},{0.176452401074453,3.70145104183577
},{0.00696125907990315,0.0831495414068030
},{0.0173324455205811,0.947870853900989
},{0.0248133398910412,1.22486730649665
},{1.26483050847458,2.81363376785121
},{0.631961259079903,1.52139923598237
},{0.616828087167070,1.52243743994463
},{0.627118644067797,1.53172496687426
},{0.620157384987893,1.55911684009618
},{4.90223970944310,0.580032886378920
},{-0.0977327253049329,4.35390195879607
},{-0.100596400287530,4.79029686064847
},{7.25665859564165,10.5882720032210
},{-1.06215389440830,10.7242286454983
},{0.854828546292375,9.01920232634819
},{5.71655569007264,9.21825654842420
},{6.99894067796610,10.4258427840462
},{-0.269803488392857,6.20273959935175
},{-0.599058533141647,6.11179734830175
},{3.35381355932203,8.05431072657359
},{-0.264350052103511,4.67076123714381
},{0.0537456611682808,4.59154200167889
},{1.68901331719128,5.97570787963692
},{0,1
},{0,1
},{0,1
},{0,1}
                };

                for (int i = 0; i < 30; i++)
                {
                    inputLayer[i, 0] = (inputLayer[i, 0] - minmax[i, 0]) / minmax[i, 1];
                }
                //minmax归一化

                hiddenLayer = wi * inputLayer;
                for (int i = 0; i < sizeM; i++)
                {
                    hiddenLayer[i, 0] += offset[i, 0];
                    hiddenLayer[i, 0] = 1.0 / (1 + Math.Exp(-hiddenLayer[i, 0]));
                }
                hiddenLayer2 = wm * hiddenLayer;
                for (int i = 0; i < 5; i++)
                {
                    hiddenLayer2[i, 0] += offset[sizeM + i, 0];
                    hiddenLayer2[i, 0] = 1.0 / (1 + Math.Exp(-hiddenLayer2[i, 0]));
                }
                outputLayer = wo * hiddenLayer2;
                for (int i = 0; i < 2; i++)
                {
                    outputLayer[i, 0] += offset[sizeM+5 + i, 0];
                }
                if (outputLayer[0, 0] < 0.5) r.state.NewData = "Run";
                else r.state.NewData = "Diffusion";

                outputLayer[1, 0] = outputLayer[1, 0] * 2 * Math.PI - Math.PI;

                delta.X = (float)(maxspeed * Math.Cos(outputLayer[1, 0]));
                delta.Y = (float)(maxspeed * Math.Sin(outputLayer[1, 0]));
                LastProcess(r, ref delta);
            }
        }

        protected void LastProcess(RFitness r, ref Vector3 delta) {

            if (r.state.SensorData == r.state.NewData) r.NumOfState++;
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

        public override void CreateDefaultParameter() {
            base.CreateDefaultParameter();
            assignFlag = false;
            //offsetArray = new double[,] { { 0 } };
        }


        bool assignFlag = false;
        bool fileFlag = false;
        [Parameter(ParameterType.Boolean, Description="FF")]
        public bool FF {
            get { return fileFlag; }
            set { fileFlag = value; }
        }

        Matrix<double> wi = new Matrix<double>(sizeM, 30);
        double[,] wiArray;

        Matrix<double> wm = new Matrix<double>(5, sizeM);
        double[,] wmArray;

        Matrix<double> wo = new Matrix<double>(2, 5);
        double[,] woArray;

        Matrix<double> offset = new Matrix<double>(7 + sizeM, 1);
        double[,] offsetArray;

        Matrix<double> inputLayer = new Matrix<double>(30, 1);
        Matrix<double> hiddenLayer = new Matrix<double>(sizeM, 1);
        Matrix<double> hiddenLayer2 = new Matrix<double>(5, 1);
        Matrix<double> outputLayer = new Matrix<double>(2, 1);

        //Comment the following attributes when visual simulation
//       [Parameter(ParameterType.Array, Description = "WI")]
        public double[,] WI {
            get { return wiArray; }
            set { wiArray = value; }
        }
//        [Parameter(ParameterType.Array, Description = "WM")]
        public double[,] WM {
            get { return wmArray; }
            set { wmArray = value; }
        }
//       [Parameter(ParameterType.Array, Description = "WO")]
        public double[,] WO {
            get { return woArray; }
            set { woArray = value; }
        }
//        [Parameter(ParameterType.Array, Description = "OFFSET")]
        public double[,] OFFSET {
            get { return offsetArray; }
            set { offsetArray = value; }
        }

    }
}

/*********** Note *******************
 * 1.仿真时要注释掉属性
*************************************/