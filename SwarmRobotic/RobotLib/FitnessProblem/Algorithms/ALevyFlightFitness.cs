using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem{

    public class ALevyFlightFitness : AFitness
    {
        public ALevyFlightFitness() { }

        protected internal override Vector3 FitnessSearch(RFitness robot) {
            //采用IGES策略的单独个体的策略，最优则保持、退步（非最优）则历史引导+大变化、进步（非最优）则历史导引+小变化
            robot.RandomSearch = false;
            Vector3 delta = Vector3.Zero;

            if (robot.Fitness.SensorData == 0)
            {
                if (robot.NumOfV > 1)
                {
                    robot.NumOfV--;
                    delta = robot.postionsystem.LastMove;
                    return NormalOrZero(delta) * maxspeed;
                }
                else if (robot.NumOfV == 1)
                {
                    robot.NumOfV--;
                    delta = robot.postionsystem.LastMove;
                    return NormalOrZero(delta) * robot.RemainingOfV;
                }
                else
                {
                    double len;
                    if (!robot.RandomSearch)  //选择幂律分布
                    {
                        len = rand.NextPowerLaw(PMinimal.FitnessRadius, ExponentialU);
                        //因为目标会被收集走，故lambda是变化的，不宜用来作为固定的边界
  //                      if (len > lambda) len = lambda;
                        if (len > problem.SizeX) len = problem.SizeX;
                    }
                    else  //选择指数分布
                    {
                        a = 1 / (AC * problem.SizeX);
                        len = rand.NextExponential(a);
                        if(len > problem.SizeX) len = problem.SizeX;
                    }
                
                    robot.NumOfV = (int)Math.Floor(len / maxspeed); //这里默认用掉一次最大速度移动，故不必加1
                    robot.RemainingOfV = (float)(len - robot.NumOfV * maxspeed);
                    return RandPosition() * maxspeed;                                    
                }
            }
            else
            {
                robot.NumOfV = 0;

                Vector3 ca, cb;
                int max, min, mid;

                if (robot.History.Count == 0 || robot.Fitness.SensorData == 0)
                {
                    delta = robot.postionsystem.LastMove;
                    return NormalOrRandom(delta) * maxspeed;
                }

                ca = cb = robot.History[0].Position;
                max = min = mid = robot.History[0].Fitness;
                foreach (var his in robot.History)
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
                if (robot.Fitness.SensorData > max)
                {
                    mid = max;
                    max = robot.Fitness.SensorData;
                    ca -= robot.postionsystem.GlobalSensorData;
                    cb -= robot.postionsystem.GlobalSensorData;
                }
                else if (robot.Fitness.SensorData < min)
                {
                    mid = min;
                    min = robot.Fitness.SensorData;
                    cb = cb - ca;
                    ca = robot.postionsystem.GlobalSensorData - ca;
                    delta = cb;
                    cb = ca;
                    ca = delta;
                }
                else
                {
                    mid = robot.Fitness.SensorData;
                    cb = cb - ca;
                    ca = robot.postionsystem.GlobalSensorData - ca;
                }

                GradientDelta(robot, max, mid, min, ref delta, ref ca, ref cb);

                delta = Vector3.Normalize(delta) * (1 - C3) + C3 * RandPosition();
                if (inertiaMove > 0.05) delta = inertiaMove * NormalOrRandom(robot.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);

                return NormalOrRandom(delta) * maxspeed;
            }

        }



        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
            u = 1.001f;
            lambda = (float)(1.07457 * Math.Sqrt(1000 * 1000 / 10)); //两个目标间的平均距离
            aC = 2.2f;
            inertiaMove = 0.6f;
            c3 = 0f;
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

        //幂律分布的指数
        float u;
        [Parameter(ParameterType.Float, Description = "u")]
        public float ExponentialU
        {
            get { return u; }
            set
            {
                if (value < 1.00001 || value > 4.0) throw new Exception("Must be at least 1.00001 and no more than 4");
                u = value;
            }
        }

        //目标之间的间距
        float lambda;
        [Parameter(ParameterType.Float, Description = "lambda")]
        public float Lambda {
            get { return lambda; }
            set {
                if (value < 20 || value > 5000) throw new Exception("Must be at least 20 and no more than 5000");
                lambda = value;
            }
        }

        protected void GradientDelta(RFitness r, int max, int mid, int min, ref Vector3 delta, ref Vector3 bPos, ref Vector3 cPos) {
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

        //指数分布参数与空因子
        float a;
        float aC;
        [Parameter(ParameterType.Float, Description = "aC")]
        public float AC
        {
            get { return aC; }
            set {
                if (value < 0.1f) aC = 0.1f;
                else if (value > 4.0f) aC = 4.0f;
                else aC = value;
            }
        }

    }
}

