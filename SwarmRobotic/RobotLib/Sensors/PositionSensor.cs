using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Sensors
{
	//NewData is delta
	public class PositionSensor : ValueSensor<Vector3>
	{
		Vector3 up, velocity;
		public bool local, inertia;

		public PositionSensor(bool local = true, bool inertia = false)
            //: base((local ? "Local" : "Global") + " Position Sensor", Vector3.Zero)
            : base("Position Sensor", Vector3.Zero)
        {
            //全局的位置信息
			GlobalSensorData = Vector3.Zero;
            NewData = Vector3.Zero;
            TranformMatrix = Matrix.Identity;

            //世界的水平方向为x，竖直方向为y，垂直xOy平面向内为z，若将将这里的z轴负向看为摄像机的上方，则摄像机俯视世界地图
            up = Vector3.Forward;
            //transform = Matrix.CreateWorld(Vector3.Zero, Vector3.Right, Vector3.Forward);
            this.local = local;
			velocity = Vector3.Zero;
			this.inertia = inertia;
		}

		//public void Update(Vector3 delta) { this.delta = delta; }
        /// <summary>
        /// 用速度（位移增量）NewData更新全局数据GlobalSensorData，并用LastMove记录该速度
        /// </summary>
        public override void ApplyChange()
        {
			Vector3 delta = NewData;
            //利用速度增量（NewData)更新位置，并分是否考虑惯性的情况；增量与速度都为0则直接返回
			if (inertia)
			{
				velocity = velocity * 0.8f + NewData;
				GlobalSensorData += velocity;
				LastMove = velocity;

                //增量若为0则重设为速度
				if (delta == Vector3.Zero)
                { 
                    if (velocity == Vector3.Zero) return;
					delta = velocity;
				}
			}
			else
			{
				GlobalSensorData += NewData;
				LastMove = NewData;
                //增量若为0则直接返回
				if (delta == Vector3.Zero) return;
			}

            //单位化速度增量
			delta.Normalize();
			//newup = accelaration x left where left = up x accelaration
            //下面操作？？？原有的up必非0，若newup为0则delta必为0，与上面代码矛盾，故up不可能为0，判断与处理代码可忽略
            //新的up方向没变，只是乘上了一个比例因子（速度增量的平方）

            //up = Vector3.Cross(delta, Vector3.Cross(up, delta));
            //if (up.Length() == 0)
            //{ 
            //    up = Vector3.Cross(velocity, Vector3.Cross(velocity, delta));
            //    up.Normalize();
            //}

            //3D中有两个基本组件用于绘制一个场景：
            //将物体放到世界中，再将摄像机放到世界中，并指定摄像机对准哪个方向，只有摄像机看到的物体才会在屏幕上可见；
            //本工程中摄像机的摆放：Up为Y轴负向，前方为Z轴正向，即坐标空间为：从左至右为X轴正向，从上至下为Y轴正向，从外向内为Z轴正向；
            //创建机器人的世界矩阵，用于平移或旋转，参数1为模型的位置，参数2为模型的前方，参数3为模型的上方
            //模型的上方为XOY平面上位移向量的左方，模型前方为垂直屏幕向外，new Vector3(0,1,0)
            
            
            TranformMatrix = Matrix.CreateWorld(GlobalSensorData, up, Vector3.Cross(delta, up));
//            TranformMatrix = Matrix.CreateWorld(GlobalSensorData, Vector3.Forward, Vector3.Up);
			NewData = Vector3.Zero;
		}

    
        //该函数仅仅在群体初始化或者实验重置时调用，所有机器人的初始Z坐标都是0.5
        //移动前清空GlobalSensorData，表示自身位置为全局坐标系的原点(0,0)
        //ApplyChange()的NewData将机器人个体由全局坐标系原点移动到“初始的群体位置”
		public virtual void Move() 
        {
            GlobalSensorData = Vector3.Zero; 
            ApplyChange(); 
            velocity = Vector3.Zero;
        }

		public override Vector3 CalculateNeighbourData(IValueSensor<Vector3> data) 
        { return (data as PositionSensor).GlobalSensorData - this.GlobalSensorData; }

		//public Vector3 GlobalSensorData { get; protected set; }
		public Vector3 GlobalSensorData { get; set; }

		public override Vector3 SensorData { get { return local ? Vector3.Zero : GlobalSensorData; } }

		public Matrix TranformMatrix { get; protected set; }

        //上一次的移动向量
		public Vector3 LastMove { get; protected set; }
	}
}
///XNA游戏简介
///2D游戏主要对象有两个：GraphicsDeviceManager对象用来访问图形设备（GraphicsDevice属性），
///SpriteBatch对象用于绘制精灵（字体、图形），音频由Cue对象处理，Cue由一个或多个Sound构成，Sound由一个或多个Wave构成；
///
///3D游戏开发引入了一个特殊的对象：Camera，3D绘图类似用一台摄像机拍摄视频，Camera对象本身实现为一个游戏组件，并添加到Game1的组件列表中；
///3D游戏中的右手坐标系统：右为X轴正向，上为Y轴正向，垂直平面向外为Z轴正向
///在3D图形领域中，矩阵几乎是做任何事情的核心；XNA的摄像机由两个Matrix对象构成：
///视图矩阵view（位置、上方、前方）：实现世界坐标到摄像机坐标的转换
///投影矩阵projection（视角、宽高比、远近视距界——定义三维视锥或视野）：实现摄像机坐标到屏幕坐标的转换
///Camera实际看向的“点”是位置与前方的叠加（创建view矩阵时所需），定位目标坐标后单位化“前方”向量可用于移动摄像机
///进一步可设置摄像机的前后移动，左右上下移动，左右转动yaw，上下转动pitch，侧转动roll
///
///3D绘图最基本的“基元”就是三角形，VertexBuffer类型的对象存储顶点信息（参数为图形设备、顶点数组的类型与长度），用于设置图形设备的缓冲（2D没有）
///3D绘图工具是BasicEffect对象（效果对象、一般为高级着色器），effect的World/View/Projection属性用于设置物体位置与已有的摄像机信息
///绘制过程是应用效果的当前技术CurrentTechniqe的所有Pass，每个效果是一个或多个技术，每个技术是一个或多个Pass
///XNA的3D的一切都是通过Effect用HLSL（高级着色语言）来绘制的，BasicEffect即为Effect的派生类，可直接使用默认HLSL代码
///物体平移是用World矩阵乘以Matrix.CreateTranslation(x,y,z)来实现，绕Y轴旋转是乘以Matrix.CreateRotationY(angle)来实现，还可进行缩放
///
///所载入资源除了字体、图片与音频外，还有3D模型（一个点的集合、用于绘制复杂物体、也可应用颜色与纹理）
///对于一个模型，要定义一个Model对象（用于载入模型资源）与一个World对象（设置模型绘制位置）
///Update方法中什么也没有，2D中至少要更新精灵的帧；2D动画类似手翻动画书，3D则更像拍摄视频，Camera在每一帧自动获取视锥的一个快照，并在屏幕绘制
///每个Model对象的Meshes属性包含一个ModelMesh对象列表，每个ModelMesh的MeshParts属性包含一个ModelMeshPart对象列表，每个ModelMeshPart对象都包含
///绘制该部件所需的“材质”material和一个Effect对象（默认为BasicEffect类型、开发人员也可自己编写HLSL效果文件）；
///每个ModelMesh都有一个“变换”transformation，能将mesh移到模型中适当的位置；
///同样对每个Effect也要设置位置（由变换矩阵确定）与摄像机信息，此外还要设置默认的光照信息
///为管理多个模型，类似管理多个精灵，可用组件实现一个模型管理器；
///模型的平移、旋转等只需乘以相应操作矩阵；

//本程序尚有疑问，见标记？？？