using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RobotLib
{
    /// <summary>
    /// 用于实验设置，公有字段有environment/problem/algorithm，
    /// </summary>
    public class Experiment
    {
        public RoboticEnvironment environment;
        public RoboticAlgorithm algorithm;
		public RoboticProblem problem;

		//public Dictionary<string, Color> roboticcolormap, objectcolormap;

		//public static readonly Color[] roboticcolorlist = { Color.Green, Color.Blue, Color.Red, Color.Yellow, Color.Silver, Color.Purple };
		//public static readonly Color[] objectcolorlist = { Color.Black, Color.Red, Color.Silver, Color.Yellow, Color.Brown, Color.Purple };

        public Experiment(RoboticEnvironment environment, RoboticAlgorithm algorithm, RoboticProblem problem)
        {
			//problem.InitializeParameter();
			//algorithm.Bind(problem);
			//algorithm.InitializeParameter();
			//environment.InitializeParameter();

            this.environment = environment;
            this.algorithm = algorithm;
			this.problem = problem;
            //algorithm.Bind(environment);
			environment.Initialize(problem, algorithm);

			//roboticcolormap = new Dictionary<string, Color>();
			//for (int i = 0; i < algorithm.statelist.Length; i++)
			//    roboticcolormap.Add(algorithm.statelist[i], roboticcolorlist[i]);
			//objectcolormap = new Dictionary<string, Color>();
			//for (int i = 0; i < problem.statelist.Length; i++)
			//    objectcolormap.Add(problem.statelist[i], objectcolorlist[i]);
		}

        //运行实验直到结束状态（可设置最大迭代次数限制），并返回群体状态
        public RunState Run(int MaxIteration = 0)
        {
            if (MaxIteration == 0)
            {
                while (!environment.runstate.Finished) Update();
            }
            else
            {
                while (!environment.runstate.Finished && environment.runstate.Iterations < MaxIteration) Update();
            }
            //拷贝状态，并进行资源的回收处理工作
			var clone = environment.runstate.ResultClone();
			problem.FinalizeState(clone, environment);
			return clone;
        }

        //更新实验
        public void Update()
        {
			//problem.UpdateObstacles();
            environment.Update();
			//environment.TestNeighbour();
        }

        //重置环境与算法
        public void Reset()
        {
			//problem.Reset();
			environment.Reset();
			algorithm.Reset();
        }

        //环境测试：默认2000次迭代，每次迭代进行环境更新，获得机器人之间、机器人与障碍物间的邻接矩阵；
        //再次计算机器人间距离、机器人与障碍物距离，若计算结果与生成的邻接矩阵不一致则报错；
        public void TestEnvrionment(int iteration = 2000)
        {
            Vector3 v;
            float dis;
            bool isN;
            var robots = environment.RobotCluster.robots;
            for (int i = 0; i < iteration; i++)
            {
                environment.Update();

                for (int j = 0; j < problem.Population; j++)
                {
                    for (int k = j + 1; k < problem.Population; k++)
					{
                        v = robots[j].postionsystem.GlobalSensorData - robots[k].postionsystem.GlobalSensorData;
                        dis = v.Length();
                        isN = (dis <= problem.RoboticSenseRange) && (!robots[j].Broken) && (!robots[k].Broken);
                        if (environment.RobotCluster.isNeighbour[j][k].isNeighbour != isN)
                            Console.WriteLine("({0},{1}) real:{3}  this:{2}", j, k, environment.RobotCluster.isNeighbour[j][k], (isN ? dis.ToString() : "false"));
					}
                    foreach (var o in environment.ObstacleClusters)
                    {
					    for (int k = 0; k < o.Size; k++)
					    {
                            v = robots[j].postionsystem.GlobalSensorData - o.obstacles[k].Position;
                            dis = v.Length();
                            isN = (dis <= problem.RoboticSenseRange) && (!robots[j].Broken) && (o.obstacles[k].Visible);
                            if (o.isNeighbour[j][k].isNeighbour != isN)
                                Console.WriteLine("Robo({3}) Obs({0}) standard:{2} this:{1}", k, o.isNeighbour[j][k], (isN ? dis.ToString() : "false"), j);
					    }
                    }
                }
            }
        }
    }
}

