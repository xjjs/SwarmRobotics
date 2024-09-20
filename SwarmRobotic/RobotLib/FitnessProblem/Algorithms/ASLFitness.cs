using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;
using Emgu.CV;
using System.IO;

using System.Threading.Tasks;
using System.Diagnostics;


namespace RobotLib.FitnessProblem {
    public class ASLFitness : AFitness {
        public ASLFitness() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) { return Vector3.Zero; }

        public override void Update(RobotBase robot, RunState state) {
            var r = robot as RFitness;
            Vector3 delta = Vector3.Zero;
            state.BatchFlag = true;

            //目标非空表示机器人已经发现目标，而且可以立即处理目标
            if (null != r.Target)
            {
                problem.CollectTarget(r, state as SFitness);
                r.History.Clear();
                r.batchObject = true;
            }
            else
            {
                r.batchObject = false;
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

                //mean-std归一化

//                double[,] md = new double[30, 2]{

//{0.44826,0.497095368852},
//{43.8743433333,33.3659667486},
//{0.0364754940383,3.47382224349},
//{-0.0370999427351,3.53251819418},
//{0.00535333333333,0.072970371765},
//{-0.00138179573926,0.931687153198},
//{-0.00298466254617,0.914837038338},
//{1.53858,3.49895006876},
//{0.753981666667,1.83939572858},
//{0.751445,1.83711424395},
//{0.747965,1.84606609456},
//{0.7539,1.85922872629},
//{4.85161666667,0.722859405226},
//{-0.0485259790677,5.78683568179},
//{0.0630256403419,5.91314855714},
//{8.16142833333,11.085498751},
//{-0.0986995274014,9.09813713493},
//{0.0945502757473,9.16773116878},
//{6.40435333333,9.69743067593},
//{7.52455666667,10.7632151471},
//{-0.307662014078,6.48700908748},
//{-0.10063309054,6.45979827698},
//{4.762445,9.51435385559},
//{-0.329840675321,4.9223299677},
//{-0.0943126839236,4.8882993154},
//{2.55942166667,7.04317701034},
//{0.499810984162,0.288757316221},
//{0.500394781317,0.288894320028},
//{0.499847181384,0.288706715709},
//{0.50053214842,0.288680589913},

//                };

//                for (int i = 0; i < 30; i++) inputLayer[i, 0] = (inputLayer[i, 0] - md[i, 0]) / md[i, 1];


//                string command = "C:/myInstall/Anaconda/python.exe C:/Users/Jie/gbdtProcess.py ";
//                for (int i = 0; i < 30; i++) command = command + inputLayer[i, 0] + ",";

                for (int i = 0; i < 30; i++) state.Command = state.Command + inputLayer[i, 0] + ",";

//                Process p = new Process();
//                p.StartInfo.FileName = "cmd.exe";
//                p.StartInfo.UseShellExecute = false;
//                p.StartInfo.RedirectStandardInput = true;
//                p.StartInfo.RedirectStandardOutput = true;
//                p.StartInfo.RedirectStandardError = true;
//                p.StartInfo.CreateNoWindow = true;
//                p.Start();
//                //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
//                p.StandardInput.WriteLine(command + "&exit");
//                p.StandardInput.AutoFlush = true;

//                string outputInfo = p.StandardOutput.ReadToEnd();
//                int start = outputInfo.LastIndexOf("[");
//                int end = outputInfo.LastIndexOf("]");
//                string result = outputInfo.Substring(start+1, end-start-1);
//                p.WaitForExit();
//                p.Close();

//                double angle = double.Parse(result);

//                double moveX = inputLayer[2, 0];
//                double moveY = inputLayer[3, 0];
//                double originalAngle = Math.Acos(moveX / Math.Sqrt(moveX * moveX + moveY * moveY));
//                if (moveY < 0) originalAngle = -originalAngle;
//                //角度归一化到0到1
//                originalAngle = (originalAngle + Math.PI) * 0.5 / Math.PI;

//                //state.Iterations < 1 || 

//                if (state.Iterations < 1 || Math.Abs(angle - originalAngle) < 0.1 && inputLayer[0, 0] > 0.5) r.state.NewData = "Diffusion";
//                else r.state.NewData = "Run";

//                angle = angle * 2 * Math.PI - Math.PI;
//                delta.X = (float)(maxspeed * Math.Cos(angle));
//                delta.Y = (float)(maxspeed * Math.Sin(angle));
//                LastProcess(r, ref delta);
            }
        }

        protected void LastProcess(RFitness r, ref Vector3 delta) {

            if (r.state.SensorData == r.state.NewData) r.NumOfState++;
            else r.NumOfState = 0;

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
        }

        Matrix<double> inputLayer = new Matrix<double>(30, 1);

    }
}

/*********** Note *******************
 * 1.仿真时要注释掉属性
*************************************/