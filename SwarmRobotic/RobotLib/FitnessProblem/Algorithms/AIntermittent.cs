using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem
{

    public class AIntermittent: AFitness
    {
        public AIntermittent() { }

        protected internal override Vector3 FitnessSearch(RFitness robot)
        {
            //采用IGES策略的单独个体的策略，最优则保持、退步（非最优）则历史引导+大变化、进步（非最优）则历史导引+小变化
            robot.RandomSearch = false;
            Vector3 delta = Vector3.Zero;

            if (robot.Fitness.SensorData == 0 || robot.cnt > 0)
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

                    if (robot.cnt == 2)
                    {
                        robot.cnt = 1;
                        len = rand.NextExponential(1 / (AC * problem.SizeX));
                        if (len > problem.SizeX) len = problem.SizeX;
                    }
                    else if (robot.cnt == 1)
                    {
                        robot.cnt = 0;
                        len = rand.NextExponential(1 / (BC * problem.SizeX));
                        if (len > problem.SizeX) len = problem.SizeX;
                    }
                    else
                    {
                        robot.cnt = 1;
                        len = rand.NextExponential(1 / (AC * problem.SizeX));
                        if (len > problem.SizeX) len = problem.SizeX;
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
                //delta += RandPosition() * 0.1f;

                delta = Vector3.Normalize(delta) * (1 - C3) + C3 * RandPosition();
                delta = inertiaMove * NormalOrRandom(robot.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);

                return NormalOrRandom(delta) * maxspeed;
            }

        }



        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
            aC = 0.3f;
            bC = 3.0f;
            inertiaMove = 0.6f;
            c3 = 0f;
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
        //指数分布参数
        float aC;
        [Parameter(ParameterType.Float, Description = "aC")]
        public float AC
        {
            get { return aC; }
            set
            {
                if (value < 0.1f) aC = 0.1f;
                else if (value > 20.0f) aC = 20.0f;
                else aC = value;
            }
        }
        float bC;
        [Parameter(ParameterType.Float, Description = "bC")]
        public float BC
        {
            get { return bC; }
            set
            {
                if (value < 0.1f) bC = 0.1f;
                else if (value > 20.0f) bC = 20.0f;
                else bC = value;
            }
        }

    }
}

