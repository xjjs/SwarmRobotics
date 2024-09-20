using System;
using GucUISystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RobotDemo
{
    //3D中有两个基本组件用于绘制一个场景：
    //将物体放到世界中，再将摄像机放到世界中，并指定摄像机对准哪个方向，只有摄像机看到的物体才会在屏幕上可见；
    //本工程中摄像机的摆放：Up为Y轴负向，前方为Z轴正向，即坐标空间为：从左至右为X轴正向，从上至下为Y轴正向，从外向内为Z轴正向；
    //设地图尺寸为(sizex,sizey,sizez)，其中sizez的大小为1，则有：
    //机器人方阵的初始位置为(sizex/2,sizey/2,0.5)
    //地图模型的位置：(-0.5,-0.5,0)，为处在最底层我改为了(-0.5,-0.5,1)
    //摄像机的位置(sizex/2,sizey/2,-sizex/2+1)=目标位置+摄像机参考向量的旋转结果向量，故地图越大，摄像机越远
    //其中，初始目标(sizex/2,sizey/2,1)，初始参考(0,0,-sizex/2)
    
	class Camera
	{
        //摄像机的参考位置、初始参考位置，用于计算旋转向量
		public Vector3 CameraRef, CameraRefOrigin;

        //view视图中心（即摄像机朝向的目标位置）
		Vector3 CenterPos, CenterPosOrigin, CenterPosMin, CenterPosMax;

        //绕X轴与绕Z轴的旋转角度
		float AngleXOrigin, AngleZOrigin;

        //平移速度、旋转速度、摄像机的位置、摄像机的上方
		Vector3 movespeed, rotatespeed, camerapos, cameraup;


        //Z轴的尺寸
		Vector3 zSize;

        //检测摄像机投影矩阵的设置
//        Vector3 tSize;

        //用于旋转的四维向量，绕某三维向量旋转角度theta，第四维分量w=cos(theta/2)
		Quaternion rotate;

        //摄像机的位置更新向量（速度向量、初始值为0）
		Vector3 delta = Vector3.Zero;

        //投影矩阵采用“直角坐标系”or“透视坐标系”
		bool isOrthographic, isFollow, is3D;
		Func<Vector3> followfunc;

        //视图中心（即摄像头看向的目标位置）
		public Vector3 ViewCenter
		{
			get { return CenterPos; }
			set { CenterPos = value; }
		}

        //绕X轴、Z轴旋转的角度
		public float AngleX { get; set; }
		public float AngleZ { get; set; }


        //指示是否处于跟随状态
        public bool Follow {
            get { return isFollow; }
            set {
                if (FollowFunc == null) isFollow = false;
                isFollow = value;
            }
        }
        //跟随函数
		public Func<Vector3> FollowFunc { 
			get{return followfunc;}
			set
			{
				followfunc = value;
				if (value == null) Follow = false;
			}
		}


        //视图矩阵与投影矩阵
		public Matrix ViewMatrix { get; private set; }
		public Matrix ProjectionMatrix { get; private set; }


        //标识投影矩阵是否为“直角坐标系”并重置“投影矩阵”
		public bool Orthographic
		{
			get { return isOrthographic; }
			set
			{
				isOrthographic = value;
                //切换“直角投影空间”or“透视投影空间”
				if (value)	//perspective=>orthographic
					ProjectionMatrix = Matrix.CreateOrthographic(-2 * CameraRef.Z, -2 * CameraRef.Z, -zSize.Z, zSize.Z);
				else	//orthographic=>perspective
					ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, 1, zSize.Z);
			}
		}

        //目前工程用该方法创建Camera对象
		public Camera(bool orthographic = true, bool is3D = false, Func<Vector3> FollowFunc = null)
		{
            //若是3D显示，则初值设为摄像机（而非地图）“仰转”pi/3和“右滚”3pi/4
            this.is3D = is3D;
			if (is3D)
			{
				AngleXOrigin = MathHelper.Pi / 3;
				AngleZOrigin = MathHelper.PiOver4 * 3;
			}
			else
			{
				AngleXOrigin = AngleZOrigin = 0;
			}

            //目标位置的最小值设为原点、各维速度单位化、绕X轴与绕Z轴转动的速度都是1度/次
			CenterPosMin = Vector3.Zero;
			movespeed = new Vector3(1f, 1f, 1f);
			rotatespeed = new Vector3(MathHelper.Pi / 180, 0, MathHelper.Pi / 180);

            //默认不进入跟随状态，选用“直角投影坐标系”
			Follow = false;
			Orthographic = orthographic;
			this.FollowFunc = FollowFunc;
		}

		public Camera(float min, float max)
		{
            //关闭3D，清零绕轴旋转量，各维速度单位化，绕X轴与绕Z轴转动速度都是1度/次
            //默认不进入跟随状态，选用“直角投影坐标系”
			is3D = false;
			AngleXOrigin = AngleZOrigin = 0;
			movespeed = new Vector3(1f, 1f, 1f);
			rotatespeed = new Vector3(MathHelper.Pi / 180, 0, MathHelper.Pi / 180);
			Follow = false;
			Orthographic = true;
			FollowFunc = null;

            //摄像机的参考位置的初始值在“Z轴负半轴的(min-max)/2处”
			zSize = new Vector3(min - max, -5, 0);
			CameraRefOrigin = new Vector3(0, 0, zSize.X / 2);
			zSize.Z = 1 - zSize.X;

            //设置视图中心（目标位置）的初始位置和边界位置，重置并更新摄像机
			CenterPosOrigin = Vector3.Zero;
			CenterPosMax = new Vector3(max, max, 0);
			CenterPosMin = new Vector3(min, min, 0);

            //重置并更新摄像机
			Reset();
			UpdateCamera();
		}

        //size为地图尺寸，X/Y/Z坐标用于确定视图中心位置，此外X/Y用于设置摄像机的参考位置
        //zSize的X坐标用于确定“摄像机在Z轴上的参考位置”，Z坐标用于远近视距的设置，Y坐标未用到
		public void SetSize(Vector3 size)
		{
            //摄像机的参考位置的初始值与远近视距
			zSize = new Vector3(-MathHelper.Max(size.X, size.Y), -5, 0);
			CameraRefOrigin = new Vector3(0, 0, zSize.X / 2);
			zSize.Z = size.Z - zSize.X;

 //           tSize = size;
           
          
            //视图中心（目标位置）的初始值与边界值，重置（角度与位置设为初值）并更新摄像机
			CenterPosOrigin = new Vector3(size.X / 2, size.Y / 2, 0);  //size.Z	
            CenterPosMax = new Vector3(size.X, size.Y, 0);   //CenterPosMax = size;

			Reset();
			UpdateCamera();
		}

		public void ParseInput(KeyboardState newKeyState, KeyboardState oldKeyState)
		{
			if (newKeyState.IsKeyDown(Keys.F) && oldKeyState.IsKeyUp(Keys.F))
				this.Follow = !this.Follow;
			if (newKeyState.IsKeyDown(Keys.V) && oldKeyState.IsKeyUp(Keys.V))
				this.Orthographic = !this.Orthographic;
			if (newKeyState.IsKeyDown(Keys.Left)) MoveLeft();
			if (newKeyState.IsKeyDown(Keys.Right)) MoveRight();
			if (newKeyState.IsKeyDown(Keys.Up)) MoveUp();
			if (newKeyState.IsKeyDown(Keys.Down)) MoveDown();
			if (newKeyState.IsKeyDown(Keys.OemPlus)) ZoomIn();
			if (newKeyState.IsKeyDown(Keys.OemMinus)) ZoomOut();
			if (newKeyState.IsKeyDown(Keys.OemComma)) YawUp();
			if (newKeyState.IsKeyDown(Keys.OemPeriod)) YawDown();
			if (is3D)
			{
				if (newKeyState.IsKeyDown(Keys.PageUp)) MoveZUp();
				if (newKeyState.IsKeyDown(Keys.PageDown)) MoveZDown();
				if (newKeyState.IsKeyDown(Keys.Home)) PitchDown();
				if (newKeyState.IsKeyDown(Keys.End)) PitchUp();
			}
			if (newKeyState.IsKeyDown(Keys.R)) Reset();
		}

        /// <summary>
        /// 键盘事件处理函数，DemoScreen会调用该处理函数
        /// </summary>
        /// <param name="input"></param>
		public void ParseInput(InputEventArgs input)
		{
            int Num = 20;  //利用键盘加快操作速度
			if (input.isKeyDown(Keys.F)) this.Follow = !this.Follow;
			if (input.isKeyDown(Keys.V)) this.Orthographic = !this.Orthographic;
			if (input.isKeyPress(Keys.A)) for(int i = 0; i < Num; i++) MoveLeft();
            if (input.isKeyPress(Keys.D)) for (int i = 0; i < Num; i++) MoveRight();
            if (input.isKeyPress(Keys.W)) for (int i = 0; i < Num; i++) MoveUp();
            if (input.isKeyPress(Keys.S)) for (int i = 0; i < Num; i++) MoveDown();
            if (input.isKeyPress(Keys.OemPlus)) for (int i = 0; i < Num; i++) ZoomIn();
            if (input.isKeyPress(Keys.OemMinus)) for (int i = 0; i < Num; i++) ZoomOut();
            if (input.isKeyPress(Keys.Left)) for (int i = 0; i < Num; i++) YawUp();
            if (input.isKeyPress(Keys.Right)) for (int i = 0; i < Num; i++) YawDown();
//			if (is3D)
			{
                if (input.isKeyPress(Keys.PageUp)) for (int i = 0; i < Num; i++) MoveZUp();
                if (input.isKeyPress(Keys.PageDown)) for (int i = 0; i < Num; i++) MoveZDown();
                if (input.isKeyPress(Keys.Down)) for (int i = 0; i < Num; i++) PitchDown();
                if (input.isKeyPress(Keys.Up)) for (int i = 0; i < Num; i++) PitchUp();
			}
			if (input.isKeyPress(Keys.R)) Reset();
		}

        public void UpdateCamera() {
            //check angle
            //if (AngleX < 0) AngleX = 0;
            //else if (AngleX > MathHelper.PiOver2) AngleX = MathHelper.PiOver2;
            //对“绕轴角度”进行边界限制
            AngleX = MathHelper.Clamp(AngleX, 0, MathHelper.Pi);//PiOver2
            AngleZ = MathHelper.WrapAngle(AngleZ);

            //Update rotate and pos
            //创建绕Z轴旋转的四维向量
            rotate = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, AngleZ);
            if (Follow)
                CenterPos = FollowFunc();
            else
                //利用“四维分量”变换“速度向量”
                CenterPos += Vector3.Transform(delta, rotate);
            delta = Vector3.Zero;

            //check pos
            //if (CenterPos.X < CenterPosMin.X) CenterPos.X = CenterPosMin.X;
            //else if (CenterPos.X > CenterPosMax.X) CenterPos.X = CenterPosMax.X;
            //if (CenterPos.Y < CenterPosMin.Y) CenterPos.Y = CenterPosMin.Y;
            //else if (CenterPos.Y > CenterPosMax.Y) CenterPos.Y = CenterPosMax.Y;
            //if (CenterPos.Z < CenterPosMin.Z) CenterPos.Z = CenterPosMin.Z;
            //else if (CenterPos.Z > CenterPosMax.Z) CenterPos.Z = CenterPosMax.Z;
            //对“目标位置”进行边界限制，尤其注意对CentroPos.Z的边界限制
            CenterPos.X = MathHelper.Clamp(CenterPos.X, CenterPosMin.X, CenterPosMax.X);
            CenterPos.Y = MathHelper.Clamp(CenterPos.Y, CenterPosMin.Y, CenterPosMax.Y);
            CenterPos.Z = MathHelper.Clamp(CenterPos.Z, CenterPosMin.Z, CenterPosMax.Z);
            //if (CameraRef.Z > -10) CameraRef.Z = -10;
            //对摄像机“参考位置”进行边界限制，在（-zSize.X,-5）之间
            CameraRef.Z = MathHelper.Clamp(CameraRef.Z, zSize.X, zSize.Y);

            //乘以绕X轴俯仰与绕Z轴翻滚的“四维向量”
            rotate *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, AngleX);
            //将摄像机的旋转向量加到新的目标位置上，得到摄像机的位置
            //摄像机位置的参考向量的旋转是相对于“坐标系原点”的，累加到“目标位置”上可得相对于目标的“摄像机旋转”
            camerapos = CenterPos + Vector3.Transform(CameraRef, rotate);

            //摄像机的上方是Y轴的负方向，创建视图（位置、目标、上方）
            cameraup = Vector3.Transform(Vector3.Down, rotate);
            ViewMatrix = Matrix.CreateLookAt(camerapos, CenterPos, cameraup);
            //创建投影：宽高（摄像机参考位置）、近视距远视距（Z轴尺寸）
            if (Orthographic)
//                ProjectionMatrix = Matrix.CreateOrthographic(tSize.X, tSize.Y, -zSize.Z, zSize.Z);
                ProjectionMatrix = Matrix.CreateOrthographic(-2 * CameraRef.Z, -2 * CameraRef.Z, -zSize.Z, zSize.Z);
        }

		//初始化：绕轴角度、摄像机的参考位置与目标位置
        public void Reset()
		{
			AngleX = AngleXOrigin;
			AngleZ = AngleZOrigin;
			CameraRef = CameraRefOrigin;
			CenterPos = CenterPosOrigin;
		}

		#region Opearations

        //平移向量movespeed各维度都是1

        //摄像机左移，从左到右为X轴正向
		public void MoveLeft() { delta.X += movespeed.X; this.Follow = false; }
		public void MoveRight() { delta.X -= movespeed.X; this.Follow = false; }
        //摄像机上移，从上到下为Y轴正向
		public void MoveUp() { delta.Y += movespeed.Y; this.Follow = false; }
		public void MoveDown() { delta.Y -= movespeed.Y; this.Follow = false; }

        //远离与接近（目标位置的移动）
		public void MoveZUp() { CenterPos.Z += movespeed.Z; this.Follow = false; }
		public void MoveZDown() { CenterPos.Z -= movespeed.Z; this.Follow = false; }
        //缩小与放大（摄像机参考位置的移动），从外向内为Z轴正向
		public void ZoomOut() { CameraRef.Z -= movespeed.Z; }
		public void ZoomIn() { CameraRef.Z += movespeed.Z; }
        //Pitch是俯仰
		public void PitchDown() { AngleX += rotatespeed.X; }
		public void PitchUp() { AngleX -= rotatespeed.X; }
        //Yaw为偏航
		public void YawUp() { AngleZ -= rotatespeed.Z; }
		public void YawDown() { AngleZ += rotatespeed.Z; }

		#endregion
	}
}
