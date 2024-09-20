using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;


namespace RobotLib.FitnessProblem{
    public class tristatePFSMFitness : AFitness
    {
        public tristatePFSMFitness() { }
        protected internal override Vector3 FitnessSearch(RFitness robot) { return Vector3.Zero; }

        public override void Update(RobotBase robot, RunState state) {
            var r = robot as RFitness;
            Vector3 delta = Vector3.Zero;

            //目标非空表示机器人已经发现目标，而且可以立即处理目标
            if (null != r.Target)  
            {
                problem.CollectTarget(r, state as SFitness);
                r.History.Clear();
            }
            else
            {
                foreach (var nbr in r.Neighbours)
                {
                    if (null != (nbr.Target as RFitness).Target)
                    {
                        r.Target = (nbr.Target as RFitness).Target;
                        break;
                    }
                }
                if (null != r.Target)
                {
                    delta = r.Target.Position - r.postionsystem.GlobalSensorData;
                    r.Target = null;
                    if (delta.Length() > maxspeed) delta = Vector3.Normalize(delta) * maxspeed;
                }
                else
                {
                    if (problem.statelist[4] == r.state.SensorData && rand.NextDouble() < r.inertiaDiffusion)
                    {
                        r.inertiaDiffusion *= InitialInertiaDiffusion;
                        delta = NormalOrRandom(r.postionsystem.LastMove) * maxspeed;
                        r.state.NewData = problem.statelist[4];
                    }
                    else if (problem.statelist[0] == r.state.SensorData && rand.NextDouble() < r.inertiaRun)
                    {
                        r.inertiaRun *= InitialInertiaRun;
                        TriangleSearch(r, ref delta);
                        delta = inertiaMove * NormalOrRandom(r.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);
                        delta = NormalOrRandom(delta) * maxspeed;
                        r.state.NewData = problem.statelist[0];
                    }
                    else
                    {
                        StartDiffusionOrSearch(r, state, ref delta);
                    }
                }
                LastProcess(r, ref delta);
            }
        }

        void StartDiffusionOrSearch(RFitness r, RunState state, ref Vector3 delta) {
            r.inertiaRun = InitialInertiaRun;
            r.inertiaDiffusion = InitialInertiaDiffusion;
            bool white = (0 == r.Fitness.SensorData);
            if (rand.NextDouble() < ProbabilityOfDiffusion(r.Neighbours.Count, white) * ProbabilityOfIterations(state.Iterations))
            {
                SelectDirection(r, ref delta);
                delta = NormalOrRandom(delta) * maxspeed;
                r.state.NewData = problem.statelist[4];
            }
            else
            {
                TriangleSearch(r, ref delta);
                r.state.NewData = problem.statelist[0];
            }
        }

        protected Vector3 RandPos(int direction) {
            double ang;
            switch (direction)
            {
                case 1: ang = (rand.NextDouble() * 2 + 1) * MathHelper.PiOver4; break;
                case 2: ang = (rand.NextDouble() * 2 + 5) * MathHelper.PiOver4; break;
                case 3: ang = (rand.NextDouble() * 2 + 3) * MathHelper.PiOver4; break;
                case 4: ang = (rand.NextDouble() * 2 - 1) * MathHelper.PiOver4; break;
                default: ang = rand.NextDouble() * MathHelper.TwoPi; break;
            }
            return new Vector3((float)Math.Cos(ang), (float)Math.Sin(ang), 0);
        }
        protected void SelectDirection(RFitness r, ref Vector3 delta) {
            int[] direction = new int[5] { 0, 0, 0, 0, 0 };
            int Min1 = 1, Min2 = 2, temp;
            foreach (var nbr in r.Neighbours)
            {
                if (nbr.offset.Y > 0) direction[1]++;
                else if (nbr.offset.Y < 0) direction[2]++;
                if (nbr.offset.X < 0) direction[3]++;
                else if (nbr.offset.X > 0) direction[4]++;
            }
            if (direction[Min1] > direction[Min2])
            {
                Min1 = 2;
                Min2 = 1;
            }
            if (direction[Min1] > direction[3])
            {
                temp = Min1;
                Min1 = 3;
                Min2 = temp;
            }
            if (direction[Min1] > direction[4])
            {
                temp = Min1;
                Min1 = 4;
                Min2 = temp;
            }
            if (direction[Min1] < direction[Min2]) delta = RandPos(Min1);
            else if (rand.NextDouble() < 0.5f) delta = RandPos(Min1);
            else delta = RandPos(Min2);
            if (direction[1] == direction[2] && direction[2] == direction[3] && direction[3] == direction[4])
            {
                delta = RandPos(0);
            }
        }
        protected void TriangleSearch(RFitness r, ref Vector3 delta) {

            Vector3 ca, cb;
            int max, min, mid;

            if (r.Neighbours.Count + r.History.Count < 2 || r.Fitness.SensorData == 0)
            {
                delta = NormalOrRandom(r.postionsystem.LastMove) * maxspeed;
            }
            else
            {
                max = -1;
                min = 100;
                mid = r.Fitness.SensorData;
                ca = cb = Vector3.Zero;

                if (r.Neighbours.Count > 1)
                {
                    foreach (var nbr in r.Neighbours)
                    {
                        if (max < (nbr.Target as RFitness).Fitness.SensorData)
                        {
                            if (max == -1)
                            {
                                max = (nbr.Target as RFitness).Fitness.SensorData;
                                ca = nbr.offset;
                            }
                            else
                            {
                                min = max;
                                cb = ca;
                                max = (nbr.Target as RFitness).Fitness.SensorData;
                                ca = nbr.offset;
                            }
                        }
                        else if (min > (nbr.Target as RFitness).Fitness.SensorData)
                        {
                            min = (nbr.Target as RFitness).Fitness.SensorData;
                            cb = nbr.offset;
                        }
                    }
                }
                else if (r.Neighbours.Count == 1)
                {
                    foreach (var his in r.History)
                    {
                        if (max < his.Fitness)
                        {
                            max = his.Fitness;
                            ca = his.Position - r.postionsystem.GlobalSensorData;
                        }
                    }

                    if (max > (r.Neighbours[0].Target as RFitness).Fitness.SensorData)
                    {
                        min = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                        cb = r.Neighbours[0].offset;
                    }
                    else
                    {
                        min = max;
                        cb = ca;
                        max = (r.Neighbours[0].Target as RFitness).Fitness.SensorData;
                        ca = r.Neighbours[0].offset;
                    }
                }
                else
                {

                    foreach (var his in r.History)
                    {
                        if (max < his.Fitness)
                        {
                            if (max == -1)
                            {
                                max = his.Fitness;
                                ca = his.Position - r.postionsystem.GlobalSensorData;
                            }
                            else
                            {
                                min = max;
                                cb = ca;
                                max = his.Fitness;
                                ca = his.Position - r.postionsystem.GlobalSensorData;
                            }
                        }
                        else if (min > his.Fitness)
                        {
                            min = his.Fitness;
                            cb = his.Position - r.postionsystem.GlobalSensorData;
                        }
                    }
                }

                if (r.Fitness.SensorData > max)
                {
                    mid = max;
                    max = r.Fitness.SensorData;
                }
                else if (r.Fitness.SensorData < min)
                {
                    mid = min;
                    min = r.Fitness.SensorData;
                    delta = -ca;
                    ca = delta + cb;
                    cb = delta;
                }
                else
                {
                    mid = r.Fitness.SensorData;
                    cb = cb - ca;
                    ca = -ca;
                }
                GradientDelta(r, max, mid, min, ref delta, ref ca, ref cb);
                delta += RandPosition() * 0.1f;
                delta = NormalOrRandom(delta);
            }

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
                delta = NormalOrRandom(delta);
            }
        }

        protected void LastProcess(RFitness r, ref Vector3 delta) {
            //添加避障分量
            PostDelta(r, ref delta);

            //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
            Bounding(r.postionsystem.GlobalSensorData, ref delta);

            //存储现在的位置与适应度，用速度向量保存到NewData中
            AddHistory(r);
            r.postionsystem.NewData = delta;
        }
        float ProbabilityOfIterations(int T) {
            if (T < MaxIterations) return 1.0f;
            else return 1.0f;
        }
        float ProbabilityOfDiffusion(int N, bool white) {
            if (white)
            {
                if (N > DiffusionThreshold) return RatioWhite * (N - DiffusionThreshold) / N;
                else return 0f;
            }
            else
            {
                if (N > DiffusionThreshold) return (float)(N - DiffusionThreshold) / N;
                else return 0f;
            }
        }

        public override void CreateDefaultParameter() {
            base.CreateDefaultParameter();
            inertiaMove = 0.5f;
            initialInertiaRun = 0.99992f;
            initialInertiaDiffusion = 0.99973f;
            maxIterations = 100;
            diffusionThreshold = 2.1f;
            ratioWhite = 1.0f;
        }
        float inertiaMove;
        float initialInertiaRun;
        float initialInertiaDiffusion;
        int maxIterations;
        float diffusionThreshold;
        float ratioWhite;


        [Parameter(ParameterType.Float, Description = "Move Inertia")]
        public float InertiaMove {
            get { return inertiaMove; }
            set {
                if (value > 0.99f) inertiaMove = 0.99f;
                else if (value < 0.1f) inertiaMove = 0.1f;
                else inertiaMove = value;
            }
        }
        [Parameter(ParameterType.Float, Description = "Run Inertia")]
        public float InitialInertiaRun {
            get { return initialInertiaRun; }
            set {
                if (value > 1.0f) initialInertiaRun = 1.0f;
                else if (value < 0.1f) initialInertiaRun = 0.1f;
                else initialInertiaRun = value;
            }
        }
        [Parameter(ParameterType.Float, Description = "Search Inertia")]
        public float InitialInertiaDiffusion {
            get { return initialInertiaDiffusion; }
            set {
                if (value > 1.0f) initialInertiaDiffusion = 1.0f;
                else if (value < 0.1f) initialInertiaDiffusion = 0.1f;
                else initialInertiaDiffusion = value;
            }
        }
        [Parameter(ParameterType.Int, Description = "Max Iterations")]
        public int MaxIterations {
            get { return maxIterations; }
            set {
                if (value > 300) maxIterations = 300;
                else if (value < 50) maxIterations = 50;
                else maxIterations = value;
            }
        }
        [Parameter(ParameterType.Float, Description = "Diffusion Threshold")]
        public float DiffusionThreshold
        {
            get { return diffusionThreshold; }
            set {
                if (value > 10) diffusionThreshold = 10.0f;
                else if (value < 1) diffusionThreshold = 1.0f;
                else diffusionThreshold = value;
            }
        }
        [Parameter(ParameterType.Float, Description = "White Ratio")]
        public float RatioWhite {
            get { return ratioWhite; }
            set {
                if (value > 4f) ratioWhite = 4f;
                else if (value < 0.25f) ratioWhite = 0.25f;
                else ratioWhite = value;
            }
        }

    }
}
