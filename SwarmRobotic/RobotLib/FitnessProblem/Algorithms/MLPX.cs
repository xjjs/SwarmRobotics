using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;
using Emgu.CV;
using System.IO;


namespace RobotLib.FitnessProblem
{
    public class MLPX : AFitness
    {
        static int co = 2;
        static int L = 64 * co;
        static int M = 128 * co;
        static int N = 128 * co;
        static int O = 64 * co;
        public MLPX() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) { return Vector3.Zero; }

        public override void Update(RobotBase robot, RunState state)
        {
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


                    //step1
                    //文件中存储的矩阵是转置形式
                    StreamReader sr = new StreamReader("parameters-snn-temp7m.txt");
                    //StreamReader sr = new StreamReader("parameters-snn-temp2m.txt");
                    //StreamReader sr = new StreamReader("parameters-snn-temp8m.txt");

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

                    //step2
                    //w5Array = new double[2, O];
                    //off5Array = new double[2, 1];
                    //for (int j = 0; j < O; j++)
                    //{
                    //    string line = sr.ReadLine();
                    //    string[] es = line.Split(' ');
                    //    for (int i = 0; i < 2; i++) w5Array[i, j] = double.Parse(es[i]);
                    //}
                    //for (int i = 0; i < 1; i++)
                    //{
                    //    string line = sr.ReadLine();
                    //    string[] es = line.Split(' ');
                    //    for (int j = 0; j < 2; j++) off5Array[j, i] = double.Parse(es[j]);
                    //}

                    sr.Close();
                }

                //accelerate3
                zeroLayer1.SetZero();
                zeroLayer2.SetZero();
                zeroLayer3.SetZero();
                zeroLayer4.SetZero();

                //step5
                //int column = 0;
                //for (int i = 0; i < L; i++) w1Array[i, column] = twArray[i,0];

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
                //数据采集3-1
                switch (r.state.SensorData)
                {
                    case "Run": stateValue = -1; break;
                    case "Diffusion": stateValue = 1; break;
                    default: stateValue = 0; break;
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

                double[,] md = new double[30, 2]{

//{-0.104192222222,0.994146525499},
//{44.0185444444,33.4369964842},
//{0.0477848433274,3.47419949119},
//{-0.027889864168,3.53325089325},
//{0.00539444444444,0.0732485113405},
//{-0.00086033239021,0.932853087768},
//{-0.00242958146011,0.921716849998},
//{1.52871333333,3.47368296227},
//{0.747661111111,1.8205150604},
//{0.749191111111,1.83190168682},
//{0.746851111111,1.83921664365},
//{0.746388888889,1.84393337547},
//{4.85442,0.715061626905},
//{-0.0678725368426,5.79372588495},
//{0.0474517771128,5.92506542429},
//{8.14177111111,11.0712262122},
//{-0.110838563382,9.09567126172},
//{0.0628174293603,9.16243969367},
//{6.38992666667,9.68384791951},
//{7.50507888889,10.749444574},
//{-0.270740901705,6.47887129657},
//{-0.108475918724,6.45235871817},
//{4.74271,9.49688455757},
//{-0.311836165521,4.89781582837},
//{-0.0946947152763,4.87584291908},
//{2.54142666667,7.01613754524},
//{0.500007661415,0.288937676608},
//{0.500216831356,0.288873758636},
//{0.500074952373,0.288671558556},
//{0.50034552553,0.288843806496},
////{-0.103214444444,0.994658568234},
////{-0.00818173613501,1.79895624948},

{-0.453183333333,0.891142450125},
{40.2700977778,32.3225616503},
{0.043176897943,3.45704044288},
{-0.0221535383185,3.49318546079},
{0.0129266666667,0.11295825758},
{-0.00367374618618,1.45285431374},
{-0.00444602702583,1.43236850659},
{1.59712222222,4.04810937304},
{0.76348,2.01400994718},
{0.764874444444,2.02900779687},
{0.76046,2.03643002704},
{0.760122222222,2.03879724526},
{4.82344333333,0.823835183838},
{-0.0579419014597,5.08972437711},
{0.0356206395174,5.16951425606},
{12.27925,12.0780959911},
{-0.124114927375,11.356789606},
{0.0660638210379,11.3619364852},
{9.96232777778,10.8858347163},
{12.1520222222,12.0621324962},
{-0.203394082576,6.58491059007},
{-0.0896314545738,6.58787218956},
{6.89515666667,11.3669042051},
{-0.235235250522,4.90387697323},
{-0.0771799804908,4.92527642773},
{3.50753888889,8.47497091759},
{0.500110233742,0.288556346424},
{0.499667009901,0.288811995016},
{0.500131113176,0.288670219825},
{0.500567575629,0.288588835087},

//{-0.432271111111,0.901743692242},
//{-0.00594214965396,1.80222032942},
                };

                for (int i = 0; i < 30; i++)
                {
                    inputLayer[i, 0] = (inputLayer[i, 0] - md[i, 0]) / md[i, 1];
                }
                //mean-std归一化


                //accelerate4
                hiddenLayer1 = w1 * inputLayer;
                hiddenLayer1 += off1;
                if (activationFlag)
                {
                    hiddenLayer1.SetValue(0.0, hiddenLayer1.Cmp(zeroLayer1, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                }
                else
                {
                    tempLayer1 = hiddenLayer1 + 0.0;
                    hiddenLayer1.SetValue(0.0, hiddenLayer1.Cmp(zeroLayer1, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                    tempLayer1.SetValue(0.0, tempLayer1.Cmp(zeroLayer1, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GT));
                    CvInvoke.cvExp(tempLayer1, tempLayer1);
                    hiddenLayer1 = scale * (hiddenLayer1 + alpha * (tempLayer1-1.0));
                }
                //for (int i = 0; i < L; i++)
                //{
                //    hiddenLayer1[i, 0] += off1[i, 0];
                //    hiddenLayer1[i, 0] = relu(hiddenLayer1[i, 0]);
                //}

                hiddenLayer2 = w2 * hiddenLayer1;
                hiddenLayer2 += off2;
                if (activationFlag)
                {
                    hiddenLayer2.SetValue(0.0, hiddenLayer2.Cmp(zeroLayer2, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                }
                else
                {
                    tempLayer2 = hiddenLayer2 + 0.0;
                    hiddenLayer2.SetValue(0.0, hiddenLayer2.Cmp(zeroLayer2, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                    tempLayer2.SetValue(0.0, tempLayer2.Cmp(zeroLayer2, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GT));
                    CvInvoke.cvExp(tempLayer2, tempLayer2);
                    hiddenLayer2= scale * (hiddenLayer2 + alpha * (tempLayer2 - 1.0));
                }
                //for (int i = 0; i < M; i++)
                //{
                //    hiddenLayer2[i, 0] += off2[i, 0];
                //    hiddenLayer2[i, 0] = relu(hiddenLayer2[i, 0]);
                //}

                hiddenLayer3 = w3 * hiddenLayer2;
                hiddenLayer3 += off3;
                if (activationFlag)
                {
                    hiddenLayer3.SetValue(0.0, hiddenLayer3.Cmp(zeroLayer3, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                }
                else
                {
                    tempLayer3 = hiddenLayer3 + 0.0;
                    hiddenLayer3.SetValue(0.0, hiddenLayer3.Cmp(zeroLayer3, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                    tempLayer3.SetValue(0.0, tempLayer3.Cmp(zeroLayer3, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GT));
                    CvInvoke.cvExp(tempLayer3, tempLayer3);
                    hiddenLayer3 = scale * (hiddenLayer3 + alpha * (tempLayer3 - 1.0));
                }
                //for (int i = 0; i < N; i++)
                //{
                //    hiddenLayer3[i, 0] += off3[i, 0];
                //    hiddenLayer3[i, 0] = relu(hiddenLayer3[i, 0]);
                //}

                hiddenLayer4 = w4 * hiddenLayer3;
                hiddenLayer4 += off4;
                if (activationFlag)
                {
                    hiddenLayer4.SetValue(0.0, hiddenLayer4.Cmp(zeroLayer4, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                }
                else
                {
                    tempLayer4 = hiddenLayer4 + 0.0;
                    hiddenLayer4.SetValue(0.0, hiddenLayer4.Cmp(zeroLayer4, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_LT));
                    tempLayer4.SetValue(0.0, tempLayer4.Cmp(zeroLayer4, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GT));
                    CvInvoke.cvExp(tempLayer4, tempLayer4);
                    hiddenLayer4 = scale * (hiddenLayer4 + alpha * (tempLayer4 - 1.0));
                }
                //for (int i = 0; i < O; i++)
                //{
                //    hiddenLayer4[i, 0] += off4[i, 0];
                //    hiddenLayer4[i, 0] = relu(hiddenLayer4[i, 0]);
                //}

                outputLayer = w5 * hiddenLayer4;
                outputLayer += off5;


                //输出中心化

                //outputLayer[0, 0] += -0.103214444444;
                //outputLayer[1, 0] += -0.00818173613501;
                //outputLayer[0, 0] += -0.432271111111;
                //outputLayer[1, 0] += -0.00594214965396;

                //数据采集3-2：小于0.5改为< 0
                if (outputLayer[0, 0] < 0)
                {
                    r.state.NewData = "Run";
                }
                else r.state.NewData = "Diffusion";

                //数据采集3-3
                //outputLayer[1] = outputLayer[1] * 2 * Math.PI - Math.PI;
                delta.X = (float)(maxspeed * Math.Cos(outputLayer[1, 0]));
                delta.Y = (float)(maxspeed * Math.Sin(outputLayer[1, 0]));

                LastProcess(r, ref delta);
            }
        }

        protected void LastProcess(RFitness r, ref Vector3 delta)
        {

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



        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
            assignFlag = false;
            //offsetArray = new double[,] { { 0 } };
        }

        //accelerate1
        bool activationFlag = false;
        double alpha = 1.6732632423543772848170429916717;
        double scale = 1.0507009873554804934193349852946;

        bool assignFlag = false;
        bool fileFlag = false;

    
        [Parameter(ParameterType.Boolean, Description = "FF")]
        public bool FF
        {
            get { return fileFlag; }
            set { fileFlag = value; }
        }

        //public double relu(double data)
        //{
        //    return data > 0 ? data : 0;
        //}

        //public double relu(double data)
        //{
        //    double alpha = 1.6732632423543772848170429916717;
        //    double scale = 1.0507009873554804934193349852946;
        //    return data > 0.0 ? scale * data : scale * (alpha * Math.Exp(data) - alpha);
        //}


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

        //step3
        //Matrix<double> tW = new Matrix<double>(L, 1);
        //double[,] twArray;

        //accelerate2
        Matrix<double> inputLayer = new Matrix<double>(30, 1);
        Matrix<double> hiddenLayer1 = new Matrix<double>(L, 1);
        Matrix<double> hiddenLayer2 = new Matrix<double>(M, 1);
        Matrix<double> hiddenLayer3 = new Matrix<double>(N, 1);
        Matrix<double> hiddenLayer4 = new Matrix<double>(O, 1);
        Matrix<double> outputLayer = new Matrix<double>(2, 1);

        Matrix<double> zeroLayer1 = new Matrix<double>(L, 1);
        Matrix<double> zeroLayer2 = new Matrix<double>(M, 1);
        Matrix<double> zeroLayer3 = new Matrix<double>(N, 1);
        Matrix<double> zeroLayer4 = new Matrix<double>(O, 1);

        Matrix<double> tempLayer1 = new Matrix<double>(L, 1);
        Matrix<double> tempLayer2 = new Matrix<double>(M, 1);
        Matrix<double> tempLayer3 = new Matrix<double>(N, 1);
        Matrix<double> tempLayer4 = new Matrix<double>(O, 1);

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


        //[Parameter(ParameterType.Array, Description = "W5")]
        public double[,] W5
        {
            get { return w5Array; }
            set { w5Array = value; }
        }

        //[Parameter(ParameterType.Array, Description = "OFF5")]
        public double[,] OFF5
        {
            get { return off5Array; }
            set { off5Array = value; }
        }

        //step4
        //[Parameter(ParameterType.Array, Description = "TW")]
        //public double[,] TW
        //{
        //    get { return twArray; }
        //    set { twArray = value; }
        //}


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