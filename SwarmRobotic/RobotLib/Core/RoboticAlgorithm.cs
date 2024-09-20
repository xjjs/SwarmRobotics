using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotLib
{
    /// <summary>
    /// 算法抽象类：设置参数、问题绑定
    /// </summary>
    /// <remarks></remarks>
    public abstract class RoboticAlgorithm : IParameter
    {
        //public string[] statelist;
		//protected List<Robotic> Robotics;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboticAlgorithm"/> class.
        /// </summary>
        /// <param name="ParaList">The para list.</param>
        /// <remarks></remarks>
        public RoboticAlgorithm()
			:base()
        {
            //statelist = null;
			CreateDefaultParameter();
        }

        /// <summary>
		/// Initializes the specified environment. Must be called after <see cref="InitializeParameter"/> method.
		/// When overriding, call <see cref="RoboticEnvironment.AddRobotics"/> before base.Bind.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <remarks></remarks>
        //public virtual void Bind(RoboticEnvironment environment)
        //{
        //    environment.algorithm = this;
        //    //Robotics = environment.robotics;
        //    ArrangeRobotic(environment.robotics);
        //}

        //public virtual void ArrangeRobotic(List<RobotBase> robotics)
        //{
        //    foreach (RobotBase r in robotics)
        //    {
        //        r.postionsystem.Move();
        //        r.ApplyChanges();
        //    }
        //}

		/// <summary>
		/// Must be called before <see cref="InitializeParameter"/> method.
		/// </summary>
		/// <param name="problem"></param>
		/// <returns></returns>
		public abstract bool Bind(RoboticProblem problem, bool changePara = true);

		public abstract void Reset();

		public abstract void Update(RobotBase robot, RunState state);

		//public abstract bool EndIteration(List<RobotBase> robots);

		//public abstract int RoboticCollision(RobotBase r1, RobotBase r2);

		//public abstract int ObstacleCollision(RobotBase r, Obstacles.Obstacle o);

		public abstract void CreateDefaultParameter();

		public abstract void InitializeParameter();

		public virtual object CreateCustomData() { return null; }

		public virtual void ClearCustomData(object data) { }

		public virtual void UpdateCustomData(RobotBase robot) { }

		public virtual string GetName { get { return this.GetType().Name; } }
	}
}
//算法抽象类：参数设置、问题绑定