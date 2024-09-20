using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using RobotLib.Environment;

namespace RobotLib.TestProblem
{
    /// <summary>
    /// 继承自RoboticProblem，用于测试的问题，只有一个状态“Run”
    /// 问题属性：障碍物的数量、障碍物的感知范围
    /// </summary>
    public class PTest : RoboticProblem
    {
        public PTest() { statelist = new string[] { "Run" }; } //statelist = new string[] { "Obstacle" }; }

		public override RobotBase CreateRobot(RoboticEnvironment env) { return new RobotBase(); }

        //位移增量（速度）随机生成，状态都设为Run，然后更新机器人位置并清零NewData与LastMove
        public override void ArrangeRobotic(List<RobotBase> robots)
        {
            foreach (var cur in robots)
            {
                cur.postionsystem.NewData = GenerateRandomPos();
                cur.state.NewData = statelist[0];
            }
            base.ArrangeRobotic(robots);
        }

        Vector3 GenerateRandomPos() { return new Vector3((float)Random.NextDouble() * SizeX, (float)Random.NextDouble() * SizeY, (float)Random.NextDouble() * SizeZ); }

        //环境中只包含随机生成的障碍物，在簇列表Clusters中添加簇对象Cluster（组对象）
        public override void CreateEnvironment(RoboticEnvironment env)
        {          
			env.CreateClusters(this, 1, "Obstacle");
            Obstacle[] obstacles = new Obstacle[obsNum];
            for (int i = 0; i < obsNum; i++)
                obstacles[i] = new Obstacle(GenerateRandomPos(), oRange);
			env.ObstacleClusters[0].AddObstacle(obstacles);
            env.runstate = new RunState();
        }

        //重置环境为重置障碍物的位置
        public override void ResetEnvironment(RoboticEnvironment env)
        {
            base.ResetEnvironment(env);
            foreach (var o in env.ObstacleClusters[0].obstacles)
                o.Position = GenerateRandomPos();
            //for (int i = 0; i < obsNum; i++)
            //    clusters[0].obstacles[i].Position = GenerateObstaclePos();
        }

        //初始种群为50
        public override void CreateDefaultParameter()
        {
            base.CreateDefaultParameter();
            RoboticSenseRange = 40;
            SizeZ = 100;
            Population = 50;
            obsNum = 100;
            oRange = 10;
        }

        int obsNum;
        float oRange;

        [Parameter(ParameterType.Int, Description = "Obstacle Number")]
        public int ObstacleNum
        {
            get { return obsNum; }
            set
            {
                if (value < 0) throw new Exception("Must be at least 0");
                obsNum = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Obstacle Sensing Range")]
        public float ObstacleSenseRange
        {
            get { return oRange; }
            set
            {
                if (value < 3 || value > 100) throw new Exception("Must be in [3, 100]");
                oRange = value;
            }
        }
    }
}
