using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib;
using RobotLib.FitnessProblem;
using Microsoft.Xna.Framework.Graphics;
using RobotDemo.Display;
using Microsoft.Xna.Framework;
using RobotLib.Obstacles;
using Microsoft.Xna.Framework.Input;

namespace RobotDemo.RoboticScreens
{
    /// <summary>
    /// “演示窗口”的最远派生类：DemoScreen->SRScreen->MinimalScreen
    /// 屏幕控件的更新绘制取决于游戏组件ScreenManager，其Draw()方法会“循环绘制”被激活的窗口（绘制窗口包含的各个控件）；
    /// 游戏组件的Update()方法（优先于Draw）则会“循环解析”激活窗口的输入事件信息，而实验状态的更新调用就在“解析方法”中；
    /// </summary>
	class MinimalScreen : SRScreen
	{
        //纹理、问题、地图数据、状态
		Texture2D fitTexture;
        PMinimal problem;
		FitnessMapProvider2D fitMap;
		SMinimal state;
        
        //颜色数组（实际数据与采样值）、移动矩阵
		Color[] data, sample;
        //地图尺寸：已收集的目标数（真目标、可收集的假目标）
		int sizex, sizey, tars;
		Matrix move;
		IDrawModel targetModel;
		bool HideInterference, hasMap;

		public MinimalScreen(ControlScreen screen)
			: base(screen)
		{
            //向“颜色字典”中添加二元组
			Title = "Minimal Target Problem";
			ObsColorMap.Add("Obstacle", Color.Black);
			ObsColorMap.Add("Target", Color.Red);//OrangeRed
			ObsColorMap.Add("Decoy", Color.Purple);//MediumPurple
			ObsColorMap.Add("Interference", Color.Pink);//DarkSalmon
			RoboticColorMap.Add("Run", Color.Green);
			RoboticColorMap.Add("Collecting", Color.Blue);
			RoboticColorMap.Add("Decoy", Color.White);//Purple
			RoboticColorMap.Add("Leaving", Color.Black);//White
            RoboticColorMap.Add("Diffusion", Color.Red);

            //原来的适应度范围
            ////存储各适应度值对应的颜色（对应于各个圆环）
            //sample = new Color[21];
            //sample[0] = Color.White;
            ////Lerp(a,b,t)在颜色a与颜色b间插值，t为0时返回颜色a，为1时返回颜色b；
            ////目标的“远郊”为“蓝色”到“青绿色”过渡
            //for (int i = 1; i <= 15; i++) 
            //    sample[i] = Color.Lerp(Color.Blue, Color.FromNonPremultiplied(5, 255, 255, 255), (i - 1) / 15f);
            ////目标的“近围”为“青绿色”到“橘黄色”过渡
            //for (int i = 16; i <= 20; i++) 
            //    sample[i] = Color.Lerp(Color.FromNonPremultiplied(5, 255, 255, 255), Color.Orange, (i - 16) / 5f);

            //存储各适应度值对应的颜色（对应于各个圆环）
            sample = new Color[41];
            sample[0] = Color.White;
            //Lerp(a,b,t)在颜色a与颜色b间插值，t为0时返回颜色a，为1时返回颜色b；
            //目标的“远郊”为“蓝色”到“青绿色”过渡
            for (int i = 1; i <= 30; i++)
                sample[i] = Color.Lerp(Color.Blue, Color.FromNonPremultiplied(5, 255, 255, 255), (i - 1) / 30f);
            //目标的“近围”为“青绿色”到“橘黄色”过渡
            for (int i = 31; i <= 40; i++)
                sample[i] = Color.Lerp(Color.FromNonPremultiplied(5, 255, 255, 255), Color.Orange, (i - 31) / 10f);



//			move = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            move = Matrix.CreateTranslation(0f, 0f, 0f);
			//scale = Matrix.CreateScale(PMinimal.FitnessRadius / 4);

            //创建与存储“目标模型”
			Primitive p;
			p = new Primitive(graphicsDevice);
			p.AddCircle(120, PMinimalMap.FitnessRadius*2, new Vector3(0, 0, -1f), new Vector3(0, 0, -1.5f));
//			p.AddCircle(120, PMinimalMap.FitnessRadius / 2, new Vector3(0, 0, 1f), new Vector3(0, 0, -1.0f));
			p.EndInitArray();
			targetModel = p;

			HideInterference = false;
		}

		public override bool Bind(Experiment experiment)
		{
            //绑定到问题
			problem = experiment.problem as PMinimal;
			if (problem != null)
			{
				base.Bind(experiment);
                state = environment.runstate as SMinimal;
				sizex = problem.SizeX;
				sizey = problem.SizeY;
				fitTexture = new Texture2D(graphicsDevice, sizex, sizey);
				(mapModel as Primitive).Texture = fitTexture;

                //生成地图矩阵（网点适应度），1类型障碍物为真目标，2类型障碍物为假目标，3类型障碍物为干扰源
				var s = state as SMinimalMap;
				if (s == null)
				{
					var targets = environment.ObstacleClusters[1].obstacles.Concat(environment.ObstacleClusters[2].obstacles).OfType<FitnessTarget>().ToArray();
					if (problem.InterferenceNum == 0)
						fitMap = new FitnessMapProvider2D(sizex, sizey, targets, problem.CollectDecoy ? (problem.TargetNum + problem.DecoyNum) : problem.TargetNum);
					else
					{
						var inters = environment.ObstacleClusters[3].obstacles.OfType<Interference>().ToArray();
						fitMap = new FitnessMapProvider2D(sizex, sizey, targets, problem.CollectDecoy ? (problem.TargetNum + problem.DecoyNum) : problem.TargetNum, inters);
					}
					hasMap = true;
				}
				else
				{
					fitMap = s.fitnessMap;
					hasMap = false;
				}
                //创建地图网格（地图按单元格进行染色）
				data = new Color[sizex * sizey];
				UpdateMap();
				return true;
			}
			return false;
		}

        //绑定网格的颜色数据到纹理
		void UpdateMap()
		{
            //若是新建的地图生成对象，则生成地图
			if(hasMap) RefreshMap();
            //存储颜色采样值到相应的单元格
			for (int i = 0; i < sizex; i++)
				for (int j = 0; j < sizey; j++)
					data[i + j * sizex] = sample[fitMap.finalmap[i, j]];
            //将颜色的数据数组绑定到纹理
			fitTexture.SetData(data);
		}

		//for PMinimalDis，更新假目标与干扰值，生成最终地图
		void RefreshMap()
		{
			fitMap.ResetDecoy();
			if (problem.InterferenceNum > 0) fitMap.ResetInterference();
			fitMap.Update();
		}

        /// <summary>
        /// 绘制地图适应度值、绘制各种类型的物体（目标、假目标、障碍物等）、绘制机器人
        /// </summary>
        /// <param name="sender"></param>
		protected override void Display3D_Draw3DGraphic(GucUISystem.GucControl sender)
		{
            //当地图尺寸的长宽不相等时背景颜色就显示出来了
			graphicsDevice.Clear(Color.LightPink);

            //draw map，绘制地图的适应度信息，白色为背景颜色
            mapModel.Draw(move, camera.ViewMatrix, camera.ProjectionMatrix, Color.White);

			//draw obstacles，绘制地图上的各种物体（真目标、假目标、干扰源、障碍物）
            foreach (var cluster in environment.ObstacleClusters)
            {
				if (cluster.obstacles.Key == "Interference" && HideInterference) continue;
                var color = ObsColorMap[cluster.obstacles.Key];
				var model = (cluster.obstacles.Key == "Obstacle") ? obstacleModel : targetModel;
                foreach (var ob in cluster.obstacles)
                {
                    if (ob.Visible == false) continue;
                    model.Draw(Matrix.CreateTranslation(ob.Position), camera.ViewMatrix, camera.ProjectionMatrix, color);
                }
            }



			//draw robotics，绘制机器人群体
			if (ShowRobotics)
			{
                foreach (RobotBase robot in environment.RobotCluster.robots)
					roboticModel.Draw(robot.postionsystem.TranformMatrix, 
                        camera.ViewMatrix, camera.ProjectionMatrix, robot.Broken ? Color.Gray : RoboticColorMap[robot.state.SensorData]);
			}
 
		}

        /// <summary>
        /// 更新模块（按键事件处理方法）：更新地图，添加目标搜索的状态显示
        /// </summary>
        /// <param name="input"></param>
		protected override void CustomUpdate(GucUISystem.InputEventArgs input)
		{
			base.CustomUpdate(input);
			if (state != null)
			{
				int newtars = state.CollectedTargets + state.CollectedDecoys;
				if (newtars != tars)
				{
					UpdateMap();
					tars = newtars;
				}
			}

            if (problem.Population <= 160)
            {
                //添加目标搜索的状态显示
                if (problem.CollectDecoy)
                    InfoText += string.Format("\nReal Targets={0}/{2} Decoys={1}/{3}\n", problem.TargetNum - state.CollectedTargets, problem.DecoyNum - state.CollectedDecoys, state.CollectedTargets, state.CollectedDecoys);
                else
                    InfoText += string.Format("\nReal Targets={0}/{2} Decoy Visits={3}/{1}\n", problem.TargetNum - state.CollectedTargets, problem.DecoyNum, state.CollectedTargets, state.CollectedDecoys);
                //依次输出各机器人的信息
                foreach (var r in environment.RobotCluster.robots)
                    InfoText += r.ToString() + "\n";
                //选择是否隐藏“干扰源”
            }

			if (input.isKeyDown(Keys.N)) HideInterference = !HideInterference;
		}

        //重置演示：重置实验、生成地图数据
		protected override void ResetDemo()
		{
			base.ResetDemo();
			UpdateMap();
			tars = problem.TargetNum + problem.DecoyNum;
		}

		//protected override void StepDemo()
		//{
		//    base.StepDemo();
		//}
	}
}
