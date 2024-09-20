using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotLib.Obstacles;
using Microsoft.Xna.Framework;

namespace RobotLib.TargetTrackProblem
{
    [Serializable]
    public class STrack : RunState
    {
        public int First, Miss, Follow, Lives;
        public float AveDis, TotalDis, Dis, AveNear, AveSwarms;

        public STrack()
        {
            //First = Miss = Lives = Follow = 0;
            //AveDis = TotalDis = Dis = AveNear = AveSwarms = 0;
        }

        //public override bool Success { get { return AveSwarms > 0 && First >= 0; } }

        public Vector3? Target { get; set; }
    }
}
