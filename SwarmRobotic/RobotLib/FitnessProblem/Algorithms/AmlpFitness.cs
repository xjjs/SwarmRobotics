using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;
using Emgu.CV;
using System.IO;


namespace RobotLib.FitnessProblem {
    public class AmlpFitness : AFitness {
        static int L = 128;
        static int M = 64;
        static int N = 32;
        static int O = 16;
        public AmlpFitness() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) { return Vector3.Zero; }

        public override void Update(RobotBase robot, RunState state) {
            var r = robot as RFitness;
            Vector3 delta = Vector3.Zero;

            if (!assignFlag)
            {
                assignFlag = true;
                if (!fileFlag)
                {
                    w1Array = new double[L, 30];
                    off1Array = new double[L, 1];

                    w2Array = new double[M, L];
                    off2Array = new double[M, 1];

                    w3Array = new double[N, M];
                    off3Array = new double[N, 1];

                    w4Array = new double[O, N];
                    off4Array = new double[O, 1];

                    w5Array = new double[2, O];
                    off5Array = new double[2, 1];

                    //文件中存储的矩阵是转置形式
                    StreamReader sr = new StreamReader("parameters-snntrain-t3m.txt");  //"parametersSELU-0mz.txt" +输出中心化

                    for (int j = 0; j < 30; j++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int i = 0; i < L; i++) w1Array[i, j] = double.Parse(es[i]);
                    }              
                    for (int i = 0; i < 1; i++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int j = 0; j < L; j++) off1Array[j, i] = double.Parse(es[j]);
                    }


                    for (int j = 0; j < L; j++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int i = 0; i < M; i++) w2Array[i, j] = double.Parse(es[i]);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int j = 0; j < M; j++) off2Array[j, i] = double.Parse(es[j]);
                    }

                    for (int j = 0; j < M; j++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int i = 0; i < N; i++) w3Array[i, j] = double.Parse(es[i]);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int j = 0; j < N; j++) off3Array[j, i] = double.Parse(es[j]);
                    }

                    for (int j = 0; j < N; j++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int i = 0; i < O; i++) w4Array[i, j] = double.Parse(es[i]);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int j = 0; j < O; j++) off4Array[j, i] = double.Parse(es[j]);
                    }

                    for (int j = 0; j < O; j++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int i = 0; i < 2; i++) w5Array[i, j] = double.Parse(es[i]);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        string line = sr.ReadLine();
                        string[] es = line.Split(' ');
                        for (int j = 0; j < 2; j++) off5Array[j, i] = double.Parse(es[j]);
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
                w1.Data = w1Array;
                off1.Data = off1Array;

                w2.Data = w2Array;
                off2.Data = off2Array;

                w3.Data = w3Array;
                off3.Data = off3Array;

                w4.Data = w4Array;
                off4.Data = off4Array;

                w5.Data = w5Array;
                off5.Data = off5Array;
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
                //if (inputLayer[1, 0] > 50)
                //{
                //    inputLayer[1, 0] = 50;
                //}
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

                double[,] md = new double[30,2]{

{0.44826,0.497095368852},
{43.8743433333,33.3659667486},
{0.0364754940383,3.47382224349},
{-0.0370999427351,3.53251819418},
{0.00535333333333,0.072970371765},
{-0.00138179573926,0.931687153198},
{-0.00298466254617,0.914837038338},
{1.53858,3.49895006876},
{0.753981666667,1.83939572858},
{0.751445,1.83711424395},
{0.747965,1.84606609456},
{0.7539,1.85922872629},
{4.85161666667,0.722859405226},
{-0.0485259790677,5.78683568179},
{0.0630256403419,5.91314855714},
{8.16142833333,11.085498751},
{-0.0986995274014,9.09813713493},
{0.0945502757473,9.16773116878},
{6.40435333333,9.69743067593},
{7.52455666667,10.7632151471},
{-0.307662014078,6.48700908748},
{-0.10063309054,6.45979827698},
{4.762445,9.51435385559},
{-0.329840675321,4.9223299677},
{-0.0943126839236,4.8882993154},
{2.55942166667,7.04317701034},
{0.499810984162,0.288757316221},
{0.500394781317,0.288894320028},
{0.499847181384,0.288706715709},
{0.50053214842,0.288680589913},

/*
{0.582274548372,0.492968943542},
{34.9112069196,28.1183034108},
{0.049278684367,3.47189187661},
{-0.05810062466,3.5050117869},
{0.0122441666762,0.109973847158},
{-0.00316044906799,1.40903559915},
{-0.00682653281833,1.38354483513},
{2.26198399719,4.25555531834},
{1.10780737166,2.24092227272},
{1.10085427078,2.23408076722},
{1.09743871246,2.25302876678},
{1.10579463193,2.26371557643},
{4.79181485844,0.848771831121},
{-0.0745158928661,6.77755207743},
{0.1004309307,6.92239309365},
{17.3310918732,10.0050542094},
{-0.193107356494,13.1320571239},
{0.188333472241,13.2101034908},
{14.4164960794,9.7831831571},
{17.2101978813,9.91329866689},
{-0.158364523927,7.38036160178},
{-0.0584319176374,7.43039298977},
{10.8532339162,11.8759993408},
{-0.196334508836,6.01976008501},
{-0.0301470831055,6.0714263208},
{5.85205981801,9.70530313099},
{0.500317436529,0.288779301511},
{0.501131542729,0.288788916911},
{0.499844964137,0.288704175935},
{0.500903709337,0.288564212614},
               */  
/*
{0.55725,0.496459905229},
{38.7975,28.2325520233},
{-0.006614194414,3.60400979412},
{-0.0263725768438,3.39904615367},
{0.00275,0.0523682871593},
{0.028427800177,0.759094766459},
{-0.00309561538725,0.508121321449},
{1.86425,3.64099188924},
{0.91425,1.95074266306},
{0.917,1.97284337949},
{0.93375,1.98238768597},
{0.8935,1.99252547035},
{4.834,0.754615133694},
{-0.594311020049,6.41461654249},
{-0.0049133568405,6.18263693687},
{9.589,10.6242448673},
{-1.37940794769,10.4372501804},
{-0.19093871269,9.07895649744},
{7.828,9.46646269733},
{9.0545,10.4222132846},
{0.896912814086,7.14957441547},
{0.615322168839,6.4254162477},
{5.09525,8.57176629625},
{-1.1328105793,5.46050240598},
{0.0263698296668,5.21149567831},
{2.7035,6.38765119195},
{0.496211093692,0.290656513878},
{0.500216695635,0.288506216992},
{0.497714867531,0.289538324996},
{0.309415879028,0.179143935963},
*/


                };

                for (int i = 0; i < 30; i++)
                {
                    inputLayer[i, 0] = (inputLayer[i, 0] - md[i, 0]) / md[i, 1];
                }
                //mean-std归一化

                hiddenLayer1 = w1 * inputLayer;
                for (int i = 0; i < L; i++)
                {
                    hiddenLayer1[i, 0] += off1[i, 0];
                    hiddenLayer1[i, 0] = relu(hiddenLayer1[i, 0]);
                }

                hiddenLayer2 = w2 * hiddenLayer1;
                for (int i = 0; i < M; i++)
                {
                    hiddenLayer2[i, 0] += off2[i, 0];
                    hiddenLayer2[i, 0] = relu(hiddenLayer2[i, 0]);
                }

                hiddenLayer3 = w3 * hiddenLayer2;
                for (int i = 0; i < N; i++)
                {
                    hiddenLayer3[i, 0] += off3[i, 0];
                    hiddenLayer3[i, 0] = relu(hiddenLayer3[i, 0]);
                }

                hiddenLayer4 = w4 * hiddenLayer3;
                for (int i = 0; i < O; i++)
                {
                    hiddenLayer4[i, 0] += off4[i, 0];
                    hiddenLayer4[i, 0] = relu(hiddenLayer4[i, 0]);
                }

                outputLayer = w5 * hiddenLayer4;
                for (int i = 0; i < 2; i++)
                {
                    outputLayer[i, 0] += off5[i, 0];
                }

                //输出中心化
                //outputLayer[0, 0] += 0.44899;
                //outputLayer[1, 0] += 0.497866177023;



                if (outputLayer[0, 0] < 0.5)
                {
                    r.state.NewData = "Run";
                }
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

        //public double relu(double data)
        //{
        //    return data > 0 ? data : 0;
        //}

        public double relu(double data)
        {
            double alpha = 1.6732632423543772848170429916717;
            double scale = 1.0507009873554804934193349852946;
            return data > 0.0 ? scale * data : scale * (alpha * Math.Exp(data) - alpha);
        }


        Matrix<double> w1 = new Matrix<double>(L, 30);
        Matrix<double> off1 = new Matrix<double>(L, 1);
        double[,] w1Array, off1Array;

        Matrix<double> w2 = new Matrix<double>(M, L);
        Matrix<double> off2 = new Matrix<double>(M, 1);
        double[,] w2Array, off2Array;

        Matrix<double> w3 = new Matrix<double>(N, M);
        Matrix<double> off3 = new Matrix<double>(N, 1);
        double[,] w3Array, off3Array;

        Matrix<double> w4 = new Matrix<double>(O, N);
        Matrix<double> off4 = new Matrix<double>(O, 1);
        double[,] w4Array, off4Array;

        Matrix<double> w5 = new Matrix<double>(2, O);
        Matrix<double> off5 = new Matrix<double>(2, 1);
        double[,] w5Array, off5Array;

        Matrix<double> inputLayer = new Matrix<double>(30, 1);
        Matrix<double> hiddenLayer1 = new Matrix<double>(L, 1);
        Matrix<double> hiddenLayer2 = new Matrix<double>(M, 1);
        Matrix<double> hiddenLayer3 = new Matrix<double>(N, 1);
        Matrix<double> hiddenLayer4 = new Matrix<double>(O, 1);
        Matrix<double> outputLayer = new Matrix<double>(2, 1);

        public double[,] W1
        {
            get { return w1Array; }
            set { w1Array = value; }
        }
        public double[,] OFF1
        {
            get { return off1Array; }
            set { off1Array = value; }
        }

        public double[,] W2
        {
            get { return w2Array; }
            set { w2Array = value; }
        }
        public double[,] OFF2
        {
            get { return off2Array; }
            set { off2Array = value; }
        }

        public double[,] W3
        {
            get { return w3Array; }
            set { w3Array = value; }
        }
        public double[,] OFF3
        {
            get { return off3Array; }
            set { off3Array = value; }
        }

        public double[,] W4
        {
            get { return w4Array; }
            set { w4Array = value; }
        }
        public double[,] OFF4
        {
            get { return off4Array; }
            set { off4Array = value; }
        }

        public double[,] W5
        {
            get { return w5Array; }
            set { w5Array = value; }
        }
        public double[,] OFF5
        {
            get { return off5Array; }
            set { off5Array = value; }
        }

        //Comment the following attributes when visual simulation
////       [Parameter(ParameterType.Array, Description = "WI")]
//        public double[,] WI {
//            get { return wiArray; }
//            set { wiArray = value; }
//        }
////        [Parameter(ParameterType.Array, Description = "WM")]
//        public double[,] WM {
//            get { return wmArray; }
//            set { wmArray = value; }
//        }
////       [Parameter(ParameterType.Array, Description = "WO")]
//        public double[,] WO {
//            get { return woArray; }
//            set { woArray = value; }
//        }
////        [Parameter(ParameterType.Array, Description = "OFFSET")]
//        public double[,] OFFSET {
//            get { return offsetArray; }
//            set { offsetArray = value; }
//        }

    }
}

/*********** Note *******************
 * 1.仿真时要注释掉属性
*************************************/