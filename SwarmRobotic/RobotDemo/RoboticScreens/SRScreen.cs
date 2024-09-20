using System;
using System.IO;
using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RobotDemo.Display;
using RobotLib;
using System.Collections.Generic;

namespace RobotDemo
{
    /// <summary>
    /// 继承自DemoScreen（演示窗口），群体机器人窗口；
    /// </summary>
	abstract class SRScreen : DemoScreen
	{
		protected Experiment experiment;
        protected RoboticEnvironment environment;
		//protected Texture2D map;   
        //机器人模型、地图模型、障碍物模型
		protected IDrawModel roboticModel;
		//protected IDrawModel cubicModel, planeModel;
		protected IDrawModel[] roboModels;
		protected int modelIndex;
		protected IDrawModel mapModel;
		protected IDrawModel obstacleModel;
        //机器人与障碍物的颜色字典（不同的障碍物类型采用不同的颜色、机器人根据毁坏与否可选择不同的颜色）
		protected Dictionary<string, Color> RoboticColorMap, ObsColorMap;
		protected bool ShowRobotics;

        //字典与模型
		public SRScreen(ControlScreen ctrlScreen)
			: base(ctrlScreen)
		{
			RoboticColorMap = new Dictionary<string, Color>();
			ObsColorMap = new Dictionary<string, Color>();
			ShowRobotics = true;
                                                                                                                                                                                               
            //实现IDrawModel接口的类，包括两个array
            //第一个array为位置/颜色/纹理的顶点数组，第二个array为前一个数组的顶点索引
			Primitive p;
			var Content = Manager.Game.Content;
			var models = new List<IDrawModel>();

			//robot model，创建机器人模型并添加简易模型（模型1）
			p = new Primitive(graphicsDevice);//, Content.Load<Texture2D>("robottexture"));
			p.AddSimpleCraft(0.75f, new Vector3(0, 0, -2f));
			//p.AddSimpleCraft(0.375f, new Vector3(0, 0, -0.5f));
            //终止模型的初始化并准备待渲染的模型，添加模型
			p.EndInitArray();
			//planeModel = p;
			models.Add(p);

            //添加飞机模型（模型2）、敌机模型（模型3）
            //models.Add(new Model3D(graphicsDevice, Content.Load<Model>("jet"),
            //    Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0, MathHelper.PiOver2)
            //    * Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(0, 0, -1.5f)));
            //models.Add(new Model3D(graphicsDevice, Content.Load<Model>("enemy"),
            //    Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, -MathHelper.PiOver2)
            //    * Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(0.5f, 0, -1)));

			models.Add(new Model3D(graphicsDevice, Content.Load<Model>("jet"),
				Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0, MathHelper.PiOver2) 
                * Matrix.CreateScale(3.0f) * Matrix.CreateTranslation(0, 0, -1.5f)));

			models.Add(new Model3D(graphicsDevice, Content.Load<Model>("enemy"),
				Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, -MathHelper.PiOver2) 
                * Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(0, 0, 0)));

            //添加障碍物立方模型（模型3）
			p = new Primitive(graphicsDevice);
			p.AddCubic(5, new Vector3(0, 0, -1f));
			//p.AddCubic(1, new Vector3(0, 0, -0.5f));
			p.EndInitArray();
			//cubicModel = p;
			models.Add(p);
			obstacleModel = p;

			roboModels = models.ToArray();
			modelIndex = 3;
			roboticModel = roboModels[modelIndex];  //cubicModel
			//obstacle model

			//p = new Primitive(GraphicsDevice);
			////p.AddCubic(2, new Vector3(0, 0, -1f));
			//p.AddCubic(1, new Vector3(0, 0, -0.5f));
			//p.EndInitArray();
			//obstacleModel = p;
		}

        //绑定到实验，设置地图尺寸，创建地图的基底色彩
		public virtual bool Bind(Experiment experiment)
		{
            //绑定到实验与环境，利用地图尺寸对摄像机进行相关设置
			this.experiment = experiment;
            environment = experiment.environment;
            camera.SetSize(experiment.problem.MapSize);

			//map model,创建地图模型
			Primitive p = new Primitive(graphicsDevice);//, Manager.Game.Content.Load<Texture2D>("bluemap"));
			//Vector3 center = camera.ViewCenter;
			//center.Z = mapProvider.MapSize.Z;
            //原来的工程代码将地图的高宽与X/Y弄反了
//          p.AddPanel(experiment.problem.MapSize.X, experiment.problem.MapSize.Y, new Vector3(0, 0, -1f), camera.ViewCenter);
            p.AddPanel(experiment.problem.MapSize.Y, experiment.problem.MapSize.X, new Vector3(0, 0, -1f), camera.ViewCenter);
			//p.AddPanel(mapProvider.MapSize.X, mapProvider.MapSize.Y, new Vector3(0, 0, 1f), camera.ViewCenter);
			p.EndInitArray();
			mapModel = p;
			Reset();
			return true;
		}

        //更新模块（按键事件处理方法）：切换机器人的模型、切换是否显示机器人模型、运行状态更新
		protected override void CustomUpdate(InputEventArgs input)
		{
            //M键用于切换机器人的模型
			if (input.isKeyDown(Keys.M))
			{
				modelIndex++;
				if (modelIndex == roboModels.Length) modelIndex = 0;
				roboticModel = roboModels[modelIndex];
			}
            //H键用于切换是否显示机器人模型
			if (input.isKeyDown(Keys.H)) ShowRobotics = !ShowRobotics;

            //状态显示
            //每次“按键事件”处理函数都会设置DemoScreen的InfoText字段并调用该“更新模块”
            //显示的位置是视野中心的位置camera.ViewCenter，Z值为0或1，Camera Dis为参考Z值的相反数（等于距原点的距离）
			InfoText = string.Format("Parameters:\nx={2}\ny={3}\nz={4}\npitch={0}\nyaw={1}\nCamera Dis={5}\nAlive Robots={6}/{7}\nSingle Robots={11}\nIterations={8}\nComplete={9}\nCPU Time={10}ms\n",
                camera.AngleX, camera.AngleZ, camera.ViewCenter.X, camera.ViewCenter.Y, camera.ViewCenter.Z, -camera.CameraRef.Z, 
                environment.runstate.AliveRobots, experiment.problem.Population, environment.runstate.Iterations,
                environment.runstate.Finished, TimeSpan.FromTicks(environment.runstate.Time).TotalMilliseconds, environment.runstate.SingleNum);
		}

        //3D显示模块：绘制适应度地图、绘制障碍物、绘制机器人
		protected override void Display3D_Draw3DGraphic(GucControl sender)
		{
			graphicsDevice.Clear(Color.White);

			//draw map
			//mapModel.Draw(Matrix.Identity, camera.ViewMatrix, projection, Color.LightSkyBlue);
			mapModel.Draw(Matrix.Identity, camera.ViewMatrix, camera.ProjectionMatrix, Color.White);

			//draw obstacles，绘制不同类型的障碍物模型
            foreach (var cluster in environment.ObstacleClusters)
            {
				var color = ObsColorMap[cluster.obstacles.Key];
                foreach (var ob in cluster.obstacles)
                {
                    if (ob.Visible == false) continue;
                    obstacleModel.Draw(Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, camera.ProjectionMatrix, 
                        color);
                }
            }

            
			//draw robotics，绘制选择的机器人模型
			if (ShowRobotics)
			{
				foreach (RobotBase robot in environment.RobotCluster.robots)
					roboticModel.Draw(robot.postionsystem.TranformMatrix, camera.ViewMatrix, camera.ProjectionMatrix, 
                        robot.Broken ? Color.Gray : RoboticColorMap[robot.state.SensorData]);
			}
			//CustomDraw();
 
		}

		protected override void ResetDemo() { experiment.Reset(); }

		protected override void StepDemo() { experiment.Update(); }

		protected override bool Finished { get { return environment.runstate.Finished; } }
	}
}
