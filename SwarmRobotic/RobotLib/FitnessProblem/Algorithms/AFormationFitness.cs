using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RobotLib.Environment;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自AFitness的AFormationFitness算法：
    /// </summary>
	public class AFormationFitness : AFitness
	{

		public AFormationFitness() {}

        /// <summary>
        /// 计算机器人的速度向量并保存到NewData
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="state"></param>
        public override void Update(RobotBase robot, RunState state)
        {
            //不能再CreateDefaParameter()中赋值
            transmissionThrehold = (int)(problem.RoboticSenseRange * TTC);
            sinLen = problem.RoboticSenseRange * 0.5 * LenC;
            cosLen = problem.RoboticSenseRange * 0.8660254 * LenC;

            var r = robot as RFitness;
            Vector3 delta = r.postionsystem.LastMove;



            //若机器人是独行者（不属于任何一个分组）
            if (r.singleFlag)
            {
                //若未发现目标、发现的目标不可见、发现了不可收集的假目标，则RandomSearch或FitnessSearch
                //实际第一个条件可省略，因为PMinimal和PEnergy都在CollectTarget都已考虑该情况
                if (r.Target == null || !problem.CollectTarget(r, state as SFitness))
                {
                    //邻居计算时会考虑障碍物、真假目标的可视性，故若目标不可视（上一次自己收集后还没收集完）则此次的Target字段自然为null
                    //若在Collecting状态则将NewData设为Run状态：目标最后由其他机器人收集完成，本机器人仍处在收集状态
                    if (r.state.SensorData == problem.statelist[1])
                        r.state.NewData = problem.statelist[0];

                    //若满足随机搜索条件，则按角度随机生成位移增量；
                    //否则考虑“假目标处理”or“真目标搜索”，计算速度向量（位移增量）

                    if (SearchRandomly(r))
                        delta = RandomSearch(r);
                    else
                    {
                        //avoid fake target
                        var d = AvoidDecoy(r);
                        //若未发现不可收集的假目标，而且没能进入Leaving状态（穿越+离开），则返回SearchAlone()向量
                        delta = d.HasValue ? d.Value : FitnessSearch(r);
                    }
                    LastProcess(r, ref delta);
                }
                //若正在收集目标或目标已收集完则清除历史记录
                else
                    r.History.Clear();
            }
            else
            {
                //若处于扩散阶段
                if (r.diffusionFlag != 0)
                {
                    //第一次进入扩散阶段
                    if (r.diffusionFlag > 1)
                    {
                        r.diffusionFlag--;
                        //若是领队，则计算运动方向（不进行运动、即NewData为0）
                        if (r.virID == r.groupID)
                        {
                            if (r.nextFlag)
                            {
                            }
                            else
                            {
                                r.nextFlag = true;
                                SelectDirection(r, ref delta);
                                LastProcess(r, ref delta);
                            }
                        }//若是队员，则确定自身角色，以领队目前位置为目标，计算更新向量
                        else
                        {
                            Vector3 cp = new Vector3(0, 0, 0);
                            Vector3 ca = new Vector3(0, 0, 0);
                            Vector3 cb = new Vector3(0, 0, 0);
                            foreach (var nbr in r.Neighbours)
                            {
                                if (nbr.Target.virID == r.groupID)
                                {
                                    r.teammate1 = nbr.Target;
                                    ca = -nbr.offset;
                                }
                                else if (nbr.Target.groupID == r.groupID)
                                {
                                    r.teammate2 = nbr.Target;
                                    cb = ca + nbr.offset;
                                }
                            }

                            if (!r.teammate1.nextFlag)
                            {
                                //替领队更新
                                r.teammate1.nextFlag = true;
                                SelectDirection((RFitness)(r.teammate1), ref delta);
                                LastProcess((RFitness)(r.teammate1), ref delta);
                            }

                            LeaderFollow((RFitness)(r.teammate1), ref delta, ref cp, ref ca, ref cb);
                            LastProcess(r, ref delta);
                        }
                    }//在后续的扩散阶段
                    else
                    {
                        //领队保持扩散运动
                        if (r.virID == r.groupID)
                        {
                            if (r.nextFlag)
                            {
                            }
                            else
                            {
                                r.nextFlag = true;
                                LeaderDiffusion(r, ref delta, state);
                                LastProcess(r, ref delta);
                            }
                        }//队员跟随领队，保持队形
                        else
                        {
                            Vector3 cp = new Vector3(0, 0, 0);
                            Vector3 ca = new Vector3(0, 0, 0);
                            Vector3 cb = new Vector3(0, 0, 0);
                            r.teammate1 = null;
                            r.teammate2 = null;
                            foreach (var nbr in r.Neighbours)
                            {
                                if (nbr.Target.virID == r.groupID)
                                {
                                    r.teammate1 = nbr.Target;
                                    ca = -nbr.offset;
                                }
                                else if (nbr.Target.groupID == r.groupID)
                                {
                                    r.teammate2 = nbr.Target;
                                    cb = ca + nbr.offset;
                                }
                            }
                            //允许对队友的独行状态延迟判断，即遍历的第一个队员可能到下一次迭代才发现队友独行
                            if (null == r.teammate1 || r.teammate1.singleFlag || null == r.teammate2 || r.teammate2.singleFlag)
                            {
                                r.diffusionFlag = 0;
                                ChangeToSingle(r, ref delta, state);
                            }
                            else
                            {
                                if (!r.teammate1.nextFlag)
                                {
                                    r.teammate1.nextFlag = true;
                                    LeaderDiffusion((RFitness)(r.teammate1), ref delta, state);
                                    LastProcess((RFitness)(r.teammate1), ref delta);
                                }
                                //将diffusionFlag与领队保持一致
                                r.diffusionFlag = r.teammate1.diffusionFlag;
                                LeaderFollow((RFitness)(r.teammate1), ref delta, ref cp, ref ca, ref cb);
                            }
                            LastProcess(r, ref delta);

                        }
                    }
                }//若处于搜索阶段或目标收集阶段
                else
                {
                    //若未发现目标（发现了不可收集的假目标），则为搜索阶段
                    if (!problem.CollectTarget(r, state as SFitness))
                    {
                        //若在Collecting状态则将NewData设为Run状态：目标最后由其他机器人收集完成，本机器人仍处在收集状态
                        if (r.state.SensorData == problem.statelist[1]) r.state.NewData = problem.statelist[0];

                        //若需要计算：当自身是领队且队员替自己更新时才不需计算（即r.nextFlag被置为true）
                        if (!r.nextFlag)
                        {
                            Vector3 cp = new Vector3(0, 0, 0);
                            Vector3 ca = new Vector3(0, 0, 0);
                            Vector3 cb = new Vector3(0, 0, 0);
                            r.teammate1 = null;
                            r.teammate2 = null;
                            foreach (var nbr in r.Neighbours)
                            {
                                if (nbr.Target.groupID == r.groupID)
                                {
                                    if (r.teammate1 == null)
                                    {
                                        r.teammate1 = nbr.Target;
                                        ca = nbr.offset;
                                    }
                                    else
                                    {
                                        r.teammate2 = nbr.Target;
                                        cb = nbr.offset;
                                    }
                                }
                            }

                            //检测不到队友或队友已转为独行状态，可延迟动作
                            if (null == r.teammate1 || r.teammate1.singleFlag || null == r.teammate2 || r.teammate2.singleFlag)
                            {
                                ChangeToSingle(r, ref delta, state);
                            }
                            else
                            {
                                //若队友1检测到了目标
                                if (null != (r.teammate1 as RFitness).Target)
                                {
                                    delta = (r.teammate1 as RFitness).Target.Position - r.postionsystem.GlobalSensorData;
                                    if (delta.Length() > maxspeed) delta = Vector3.Normalize(delta) * maxspeed;                                  
                                }//队友2检测到了目标
                                else if (null != (r.teammate2 as RFitness).Target)
                                {
                                    delta = (r.teammate2 as RFitness).Target.Position - r.postionsystem.GlobalSensorData;
                                    if (delta.Length() > maxspeed) delta = Vector3.Normalize(delta) * maxspeed;
                                }
                                else
                                {
                                    //按照适应度值与虚拟编号排序
                                    RFitness  first, second, third;
                                    first = r.teammate1 as RFitness;
                                    third = r.teammate2 as RFitness;
                                    second = r;
                                    Vector3 bPos = ca;
                                    Vector3 cPos = cb;

                                    if (first.Fitness.SensorData < third.Fitness.SensorData
                                        || first.Fitness.SensorData == third.Fitness.SensorData && first.virID > third.virID)
                                    {
                                        first = r.teammate2 as RFitness;
                                        third = r.teammate1 as RFitness;
                                        bPos = cb;
                                        cPos = ca;
                                    }
                                    if (r.Fitness.SensorData > first.Fitness.SensorData
                                        || r.Fitness.SensorData == first.Fitness.SensorData && r.virID < first.virID)
                                    {
                                        second = first;
                                        first = r;
                                    }
                                    else if (r.Fitness.SensorData < third.Fitness.SensorData
                                        || r.Fitness.SensorData == third.Fitness.SensorData && r.virID > third.virID)
                                    {
                                        second = third;
                                        third = r;
                                    }

                                    //若自身是领队，则计算增量：梯度计算、队形保持
                                    if (first.virID == r.virID)
                                    {
                                        first.nextFlag = true;
                                        LeaderSearch(first, second, third, ref delta, ref bPos, ref cPos);                                     

                                    }//若领队已完成计算，则确定角色，跟随领队
                                    else if (first.nextFlag)
                                    {
                                        //计算相对于自身领队的位置，另一队员相对于领队位置
                                        bPos = -bPos;
                                        cPos += bPos;
                                        LeaderFollow(first, ref delta, ref cp, ref bPos, ref cPos);
                                    }//若领队尚未完成计算，则完成领队计算后更新自身
                                    else
                                    {
                                        first.nextFlag = true;
                                        //计算自身相对于领队位置，另一队员相对于领队位置
                                        bPos = -bPos;
                                        cPos += bPos;
                                        ca = bPos;
                                        cb = cPos;

                                        //若本个体最差，则与另一队员交换，以保证bPos为第二优的个体
                                        if (r.virID == third.virID)
                                        {
                                            Vector3 temp = bPos;
                                            bPos = cPos;
                                            cPos = temp;                                         
                                        }
                                        LeaderSearch(first, second, third, ref delta, ref bPos, ref cPos);
                                        LastProcess(first, ref delta);
                                        LeaderFollow(first, ref delta, ref cp, ref ca, ref cb);
                                    }

                                }

                            }
                            LastProcess(r, ref delta);
                        }

                    }
                    //若为Collecting状态（或刚收集完目标），则清除历史记录
                    else
                    {
                        r.History.Clear();
                    }
                }//扩散状态判断括号
            }//独行状态判断括号
        }//Update方法括号


        //独行状态的搜索函数，可共享邻居的目标信息
        protected internal override Vector3 FitnessSearch(RFitness robot) {
            Vector3 delta = new Vector3(0, 0, 0);
            foreach (var nbr in robot.Neighbours)
            {
                if (null != (nbr.Target as RFitness).Target)
                {
                    robot.Target = (nbr.Target as RFitness).Target;
                    delta = robot.Target.Position-robot.postionsystem.GlobalSensorData;
                    break;
                }
            }
            if (null != robot.Target)
            {
                if (delta.Length() > maxspeed) return Vector3.Normalize(delta) * maxspeed;
                else return delta;
            }
            else
            {
                Vector3 ca, cb;
                int max, min, mid;

                if (robot.History.Count == 0 || robot.Fitness.SensorData == 0)
                {
                    delta = robot.postionsystem.LastMove;
                    return NormalOrRandom(delta) * maxspeed;
                }

                ca = cb = robot.History[0].Position;
                max = min = mid = robot.History[0].Fitness;
                foreach (var his in robot.History)
                {
                    if (max < his.Fitness)
                    {
                        max = his.Fitness;
                        ca = his.Position;
                    }
                    else if (his.Fitness > 0 && his.Fitness < min)
                    {
                        min = his.Fitness;
                        cb = his.Position;
                    }
                }
                if (robot.Fitness.SensorData > max)
                {
                    mid = max;
                    max = robot.Fitness.SensorData;
                    ca -= robot.postionsystem.GlobalSensorData;
                    cb -= robot.postionsystem.GlobalSensorData;
                }
                else if (robot.Fitness.SensorData < min)
                {
                    mid = min;
                    min = robot.Fitness.SensorData;
                    cb = cb - ca;
                    ca = robot.postionsystem.GlobalSensorData - ca;
                    delta = cb;
                    cb = ca;
                    ca = delta;
                }
                else
                {
                    mid = robot.Fitness.SensorData;
                    cb = cb - ca;
                    ca = robot.postionsystem.GlobalSensorData - ca;
                }

                GradientDelta(robot, max, mid, min, ref delta, ref ca, ref cb);
                //delta = inertiaMove * NormalOrRandom(robot.postionsystem.LastMove) + (1 - inertiaMove) * NormalOrRandom(delta);
                delta += RandPosition() * 0.1f;

                return NormalOrRandom(delta) * maxspeed;
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
                //delta = cPos + Vector3.Normalize((Vector3.Zero + bPos) / 2 - cPos)*2*10;

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
                //delta = bPos + tempPos + Vector3.Normalize(delta) * 2 * 10;
                delta = NormalOrRandom(delta);
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

        protected void SelectDirection(RFitness r, ref Vector3 delta) 
        {
            int[] direction = new int[5] { 0,0,0,0,0 };
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
            if (direction[Min1] < direction[Min2])
            {
                delta = RandPos(Min1);
            }else if (rand.NextDouble() < 0.5f) delta = RandPos(Min1);
            else delta = RandPos(Min2);
            if (direction[1] == direction[2] && direction[2] == direction[3] && direction[3] == direction[4])
            {
                delta = RandPos(0);
            }

            //int up, down, left, right;
            //up = down = left = right = 0;
            ////up/down/left/right依照一般坐标系而定
            //foreach (var nbr in r.Neighbours)
            //{
            //    if (nbr.offset.Y > 0) up++;
            //    if (nbr.offset.Y < 0) down++;
            //    if (nbr.offset.X < 0) left++;
            //    if (nbr.offset.X > 0) right++;
            //}

            //if (up <= down && up <= left && up <= right)
            //{
            //    delta = RandPos(1);
            //}
            //else if (down <= up && down <= left && down <= right)
            //{
            //    delta = RandPos(2);
            //}
            //else if (left <= up && left <= down && left <= right)
            //{
            //    delta = RandPos(3);
            //}
            //else
            //{
            //    delta = RandPos(4);
            //}

            //if (up == down && up == left && up == right)
            //{
            //    delta = RandPos(0);
            //}
        }

        protected void LastProcess(RFitness r, ref Vector3 delta) 
        {

            //添加避障分量
            //obstacle avoidance & maxspeed trim
            PostDelta(r, ref delta);
            // bounce at boundary

            //进行边界处理，Vector3是值传递的（不是引用传递），所以不会更改位置
            Bounding(r.postionsystem.GlobalSensorData, ref delta);

            //存储现在的位置与适应度，用速度向量保存到NewData中
            AddHistory(r);
            r.postionsystem.NewData = delta;
        }

        protected void ChangeToSingle(RFitness r, ref Vector3 delta, RunState state) 
        {
            r.singleFlag = true;
            state.SingleNum++;
            delta = NormalOrRandom(r.postionsystem.LastMove) * maxspeed;
        }

        /// <summary>
        /// ca为自身相对于领队位置，cb为另一队员相对于领队位置
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="delta"></param>
        /// <param name="cp"></param>
        /// <param name="ca"></param>
        /// <param name="cb"></param>
        protected void LeaderFollow(RFitness leader, ref Vector3 delta, ref Vector3 cp, ref Vector3 ca, ref Vector3 cb) 
        {
            //更新方向的右正交向量
            cp.X = leader.postionsystem.NewData.Y;
            cp.Y = -leader.postionsystem.NewData.X;
            //跟随领队：确定角色，计算增量，若两者相等则随机选择一个
            if (Vector3.Dot(ca, cp) < Vector3.Dot(cb, cp)
                || Vector3.Dot(ca,cp)==Vector3.Dot(cb,cp)&&rand.NextDouble()<0.5f)
            {
                cp = leader.postionsystem.NewData;
                cb.X = (float)(-sinLen * cp.Y + (cp.Length() - cosLen) * cp.X) / cp.Length();
                cb.Y = (float)(sinLen * cp.X + (cp.Length() - cosLen) * cp.Y) / cp.Length();
            }
            else
            {
                cp = leader.postionsystem.NewData;
                cb.X = (float)(sinLen * cp.Y + (cp.Length() - cosLen) * cp.X) / cp.Length();
                cb.Y = (float)(-sinLen * cp.X + (cp.Length() - cosLen) * cp.Y) / cp.Length();
            }
            delta = -ca + cb;
        }

        protected void LeaderDiffusion(RFitness leader, ref Vector3 delta, RunState state) 
        {
            Vector3 cp = new Vector3(0, 0, 0);
            Vector3 ca = new Vector3(0, 0, 0);
            Vector3 cb = new Vector3(0, 0, 0);
            leader.teammate1 = null;
            leader.teammate2 = null;
            foreach (var nbr in leader.Neighbours)
            {
                if (nbr.Target.groupID == leader.virID)
                {
                    if (null == leader.teammate1)
                    {
                        leader.teammate1 = nbr.Target;
                        ca = nbr.offset;
                    }
                    else
                    {
                        leader.teammate2 = nbr.Target;
                        cb = nbr.offset;
                    }
                }
            }
            //此种扩散标识的判断方式延迟了一步，影响不大
            if (leader.Neighbours.Count < DThreshold) leader.diffusionFlag = 0;

            if (null == leader.teammate1 || leader.teammate1.singleFlag
                || null == leader.teammate2 || leader.teammate2.singleFlag)
            {
                leader.diffusionFlag = 0;
                ChangeToSingle(leader, ref delta, state);
            }
            else
            {
                if (ca.Length() > TThreshold || cb.Length() > TThreshold)
                {
                    leader.Speed *= Beta;
                }
                else
                {
                    leader.Speed *= Alpha;
                }
                delta = NormalOrRandom(leader.postionsystem.LastMove) * leader.Speed;
            }
        }

        protected void LeaderSearch(RFitness first, RFitness second, RFitness third, ref Vector3 delta, ref Vector3 bPos, ref Vector3 cPos)
        {
            if (0 == first.Fitness.SensorData)
            {
                if (first.NumOfV > 0)
                {
                    first.NumOfV--;
                    delta = NormalOrRandom(first.postionsystem.LastMove);
                }
                else
                {                        
                    a = 1 / (AC * problem.SizeX);
                    double len = rand.NextExponential(a);
                    if (len > problem.SizeX) len = problem.SizeX;
                    first.NumOfV = (int)Math.Ceiling(len / maxspeed); //这里默认用掉一次最大速度移动，故不必加1
                    delta = RandPosition();
                }

                if (bPos.Length() > TThreshold || cPos.Length() > TThreshold)
                {
                    first.Speed *= Beta;
                }
                else
                {
                    first.Speed *= Alpha;
                }
                delta = Vector3.Normalize(delta) * first.Speed;
            }
            else
            {
                first.NumOfV = 0;
                second.NumOfV = 0;
                third.NumOfV = 0;
                //三者相同
                if (first.Fitness.SensorData == third.Fitness.SensorData)
                {
                    delta = NormalOrRandom(first.postionsystem.LastMove);
                }//两优一差
                else if (first.Fitness.SensorData == second.Fitness.SensorData)
                {
                    //delta = cPos + Vector3.Normalize((Vector3.Zero + bPos) / 2 - cPos)*2*10;

                    delta = NormalOrRandom((Vector3.Zero + bPos) / 2 - cPos);
                }//两差一优
                else if (third.Fitness.SensorData == second.Fitness.SensorData)
                {
                    delta = NormalOrRandom(Vector3.Zero - (bPos + cPos) / 2);
                }//各不相同
                else
                {
                    Vector3 tempPos = Vector3.Zero * (second.Fitness.SensorData - third.Fitness.SensorData)
                        + cPos * (first.Fitness.SensorData - second.Fitness.SensorData);
                    tempPos /= first.Fitness.SensorData - third.Fitness.SensorData;
                    tempPos -= bPos;
                    delta.X = -tempPos.Y;
                    delta.Y = tempPos.X;
                    if (Vector3.Dot(bPos - cPos, delta) < 0)
                    {
                        delta.X = tempPos.Y;
                        delta.Y = -tempPos.X;
                    }
                    //delta = bPos + tempPos + Vector3.Normalize(delta) * 2 * 10;
                    delta = NormalOrRandom(delta);
                }

                if (bPos.Length() > TThreshold || cPos.Length() > TThreshold)
                {
                    first.Speed *= Beta;
                }
                else
                {
                    first.Speed *= Alpha;
                }
                delta = Vector3.Normalize(delta) * first.Speed;
            }

        }

        public override void CreateDefaultParameter() 
        {
            base.CreateDefaultParameter();
            diffusionThreshold = 5;

            beta = 0.75f;
            alpha = 1 / beta;
            ttC = 0.8f;
            lenC = 0.5f;
            aC = 8.5f;
            //inertiaMove = 0.5f;

            //transmissionThrehold = (int)(problem.RoboticSenseRange * 3 / 4);
            //sinLen = problem.RoboticSenseRange / 4;
            //cosLen = problem.RoboticSenseRange * 0.4330127;
        }

        int diffusionThreshold;
        int transmissionThrehold;
        float alpha;
        float beta;
        double sinLen;
        double cosLen;
        float ttC;
        float lenC;

        //float inertiaMove;

        //[Parameter(ParameterType.Float, Description = "Move Inertia")]
        //public float InertiaMove
        //{
        //    get { return inertiaMove; }
        //    set
        //    {
        //        if (value > 0.99f) inertiaMove = 0.99f;
        //        else if (value < 0.1f) inertiaMove = 0.1f;
        //        else inertiaMove = value;
        //    }
        //}

        [Parameter(ParameterType.Int, Description = "Diffusion-Stop Size")]
        public int DThreshold {
            get { return diffusionThreshold; }
            set {
                if (value < 3) diffusionThreshold = 3;
                else diffusionThreshold = value;
            }
        }

        [Parameter(ParameterType.Int, Description = "Transmission Threshold")]
        public int TThreshold {
            get { return transmissionThrehold; }
            set {
                if (value < 5) transmissionThrehold = 5;
                else transmissionThrehold = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Alpha for Acceleration")]
        public float Alpha {
            get { return alpha; }
            set {
                if (value < 1) alpha = 1.0f;
                else alpha = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Beta for Deceleration")]
        public float Beta {
            get { return beta; }
            set {
                if (value < 0.5f) beta = 0.5f;
                else beta = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Scale factor for TT")]
        public float TTC
        {
            get { return ttC; }
            set {
                if (value > 0.9f) ttC = 0.9f;
                else if (value < 0.6f) ttC = 0.6f;
                else ttC = value;
            }
        }

        [Parameter(ParameterType.Float, Description = "Scale factor for Lengh")]
        public float LenC
        {
            get { return lenC;}
            set {
                if (value > ttC) lenC = ttC * 0.95f;
                else if (value < 0.5f) lenC  = 0.5f;
                else lenC = value;
            }
        }

        //指数分布参数与空因子
        float a;
        float aC;
        [Parameter(ParameterType.Float, Description = "aC")]
        public float AC {
            get { return aC; }
            set {
                if (aC < 0.1f) aC = 0.1f;
                else if (aC > 200.0f) aC = 200.0f;
                else aC = value;
            }
        }
	}
}
