using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ParallelTest;
using RobotLib;
using RobotLib.Environment;
using RobotLib.FitnessProblem;
using RobotLib.TestProblem;

namespace TestProject
{
	class KDTreeTest
	{
		static void Test(string[] args)
		{
			for (int i = 0; i < 10; i++) Validate();
			//CompareParallel();
			//Compare();

			Console.WriteLine("Done");
			Console.Title = "Done";
			Console.ReadKey();
		}

		static void Validate()
		{
			RoboticEnvironment standard = new ESimple();//, tree1 = new EKDTree(), tree2 = new EKDTree2(), compare = new ECompare();
			//RoboticEnvironment[] environments = new RoboticEnvironment[] { compare, tree1, tree2 };
			RoboticEnvironment[] environments = new RoboticEnvironment[] { new ECompare(), new EKDTree(false), new EKDTree(true) };
			RoboticAlgorithm algorithm = new ADSFitness();
			RoboticProblem problem = new PEnergy();

			problem.InitializeParameter();
			algorithm.Bind(problem);
			algorithm.InitializeParameter();
			standard.InitializeParameter();

			Experiment experiment = new Experiment(standard, algorithm, problem);

			foreach (var env in environments)
			{
				env.InitializeParameter();
				env.Initialize(problem, algorithm);
                env.RobotCluster.robots = standard.RobotCluster.robots;
                for (int i = 0; i < env.ObstacleClusters.Count; i++)
                    env.ObstacleClusters[i].obstacles = standard.ObstacleClusters[i].obstacles;
                env.GenerateNeighbours();
            }

			for (int i = 0; i < 5000; i++)
			{
				foreach (var env in environments)
				{
					for (int j = 0; j < problem.Population; j++)
					{
                        for (int k = j + 1; k < problem.Population; k++)
						{
                            if (env.RobotCluster.isNeighbour[j][k].isNeighbour != standard.RobotCluster.isNeighbour[j][k].isNeighbour ||
                                Math.Abs(env.RobotCluster.isNeighbour[j][k].distance - standard.RobotCluster.isNeighbour[j][k].distance) > 1e-4)
                                Console.WriteLine("({0},{1}) {4}:{2}  standard:{3}", j, k, env.RobotCluster.isNeighbour[j][k], standard.RobotCluster.isNeighbour[j][k], env.GetType().Name);
						}
                        for (int o = 0; o < env.ObstacleClusters.Count; o++)
                        {
						    for (int k = 0; k < env.ObstacleClusters[o].obstacles.Count; k++)
						    {
                                if (env.ObstacleClusters[o].isNeighbour[j][k].isNeighbour != standard.ObstacleClusters[o].isNeighbour[j][k].isNeighbour ||
                                    Math.Abs(env.ObstacleClusters[o].isNeighbour[j][k].distance - standard.ObstacleClusters[o].isNeighbour[j][k].distance) > 1e-4)
                                    Console.WriteLine("Robo({0}) Obs({1}) {4}:{2} standard:{3}", j, k, env.ObstacleClusters[o].isNeighbour[j][k], standard.ObstacleClusters[o].isNeighbour[j][k], env.GetType().Name);
						    }
                        }
					}
				}

				experiment.Update();
                foreach (var env in environments)
                    env.GenerateNeighbours();
			}

		}

		static void Compare(int repeat = 10, int iteration = 10000)
		{
			EnvTestItem testItem = new EnvTestItem(repeat, iteration);
			//int[] population = new int[] { 2, 3, 5, 10, 20, 30, 40, 50, 100, 200, 300 };
			int[] obstacle = new int[] { 0, 100/*, 200, 300, 400, 500*/ };
			int[] population = Enumerable.Range(2, 30).ToArray();
			StringBuilder sb = new StringBuilder("Population,Obstacle,");
			sb.AppendLine(testItem.title);

			foreach (var pop in population)
			{
				foreach (var obs in obstacle)
				{
					CompareOnce(testItem, pop, obs);
					sb.AppendFormat("{0},{1},", pop, obs);
					foreach (var time in testItem.times)
						sb.AppendFormat("{0},", time.TotalMilliseconds);
					sb.AppendLine();
				}
			}
			File.WriteAllText(string.Format("result-{0}-{1}.csv", repeat, iteration), sb.ToString());
		}

		static void CompareParallel(int repeat = 10, int iteration = 10000)
		{
			int[] obstacle = new int[] { 10, 20, 30, 40, 50 };
			int[] population = Enumerable.Range(2, 29).ToArray();//.Concat(Enumerable.Range(4, 7).Select(t => t * 10)).Concat(Enumerable.Range(2, 4).Select(t => t * 100)).ToArray();
			int[] obstacle0 = new int[] { 0 };
			int[] population2 = new int[] { 100, 1000 };
			//StringBuilder sb = new StringBuilder("Population,Obstacle," + (new EnvTestItem(repeat, iteration)).title);
			//sb.AppendLine();
			string filename = string.Format("result-{0}-{1}.csv", repeat, iteration);
			File.AppendAllText(filename, "Population,Obstacle," + (new EnvTestItem(repeat, iteration)).title + Environment.NewLine);

			ParallelTests.ParallelTest(population2.MergeList(obstacle0),//.Concat(ParallelTest.MergeList(population, obstacle)),
				() => new EnvTestItem(repeat, iteration),
				(tuple, item) =>
				{
					CompareOnce(item, tuple.Item1, tuple.Item2);
					return Tuple.Create(tuple.Item1, tuple.Item2, item.times);
				}, (tuple) =>
				{
					var sb = new StringBuilder();
					sb.AppendFormat("{0},{1},", tuple.Item1, tuple.Item2);
					foreach (var time in tuple.Item3)
						sb.AppendFormat("{0},", time.TotalMilliseconds);
					sb.AppendLine();
					File.AppendAllText(filename, sb.ToString());
				}, "Test Environment");

			//File.WriteAllText(string.Format("result-{0}-{1}.csv", repeat, iteration), sb.ToString());
		}

		static void CompareOnce(EnvTestItem test, int population, int obstacle)
		{
			Stopwatch watch = new Stopwatch();
			ATest algorithm = new ATest();
			PTest problem = new PTest();
			problem.Population = population;
			problem.ObstacleNum = obstacle;
			//problem.CurrentParameterList["Population"].Value = population;
			//problem.CurrentParameterList["ObsNum"].Value = obstacle;
			problem.InitializeParameter();
			algorithm.Bind(problem);
			algorithm.RandSeed = test.rand.Next();
			//algorithm.CurrentParameterList["rand"].Value = test.rand.Next();
			algorithm.InitializeParameter();
			Experiment experiment;

			for (int ind = 0; ind < test.environments.Length; ind++)
			{
				watch.Reset();
				var env = test.environments[ind];
				//env.robotics.Clear();
				experiment = new Experiment(env, algorithm, problem);
				for (int repeat = 0; repeat < test.repeats; repeat++)
				{
					for (int iter = 0; iter < test.iterations; iter++)
					{
						watch.Start();
						env.GenerateNeighbours();
						watch.Stop();
						env.Update();
					}
					experiment.Reset();
				}
				test.times[ind] = watch.Elapsed;
			}
		}
	}

	class EnvTestItem
	{
		public EnvTestItem(int repeats, int iterations)
		{
			rand = new Random();
            environments = new RoboticEnvironment[] { new ESimple(), new EKDTree(false), new EKDTree(true) };
            times = new TimeSpan[environments.Length];
			this.repeats = repeats;
			this.iterations = iterations;
			title = string.Empty;
			for (int i = 0; i < environments.Length; i++)
			{
				environments[i].InitializeParameter();
				title += string.Format("{0},", environments[i].GetType().Name);
			}
		}

		public RoboticEnvironment[] environments;
		public TimeSpan[] times;
		public int repeats, iterations;
		public Random rand;
		public string title;
	}
}
