using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RobotLib.Obstacles
{
	public class MovingObstacle : Obstacle
	{
		public MovingObstacle(Vector3 pos, float speed, float SenseRange, CustomRandom rand = null, 
            ObstacleMovingSate state = ObstacleMovingSate.None, float radiusXY = 20, float radiusZ = 15)
			: base(pos, SenseRange, rand)
		{
			MovingState = state;
			MovingRadiusXY = radiusXY;
			MovingRadiusZ = radiusZ;
			times = 0;
			OriginPosition = pos;
			this.speed = speed;
			if (this.rand == null) this.rand = new CustomRandom();
		}

        public override void Update()
        {
            if (MovingState == ObstacleMovingSate.RandomLine)
            {
                if (times == 0)
                {
                    //随机更新移动的位置，并将其分为150次来逐步更新（防止移动过快）
                    destination = new Vector3(RandFloat() * MovingRadiusXY, RandFloat() * MovingRadiusXY,
                        RandFloat() * MovingRadiusZ) + OriginPosition;
					delta = (destination - Position) / 150;
					times = 150;
					//delta = Vector3.Normalize(destination - Position) * speed;
					//times = (int)((destination - Position).Length() / speed);
					//if (times == 0) times = 1;
                }
                Position += delta;
                times--;
            }
        }

		float RandFloat() { return rand.NextFloat() * 2 - 1; }

		public override void Reset(CustomRandom rand = null)
		{
			base.Reset(rand);
			OriginPosition = Position;
			times = 0;
		}
        //障碍物的移动状态、移动半径
        public ObstacleMovingSate MovingState;
        //速度字段没有用到
        public float MovingRadiusXY, MovingRadiusZ, speed;

        Vector3 delta, destination;
        int times;
		public Vector3 OriginPosition { get; private set; }
	}

    //默认的元素类型为int
	public enum ObstacleMovingSate
	{
		None, RandomLine
	}
}
