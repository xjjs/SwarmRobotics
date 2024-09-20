using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace RobotLib.FitnessProblem
{
    /// <summary>
    /// 继承自AFitness，
    /// </summary>
	public class AHybridFitness : AFitness
	{
		AFitness randAlgo, fitAlgo;

		public AHybridFitness() : base() { }

		public override object CreateCustomData() { return new HybridTag(randAlgo.CreateCustomData(), fitAlgo.CreateCustomData()); }

		public override void ClearCustomData(object data)
		{
			var Tag = data as HybridTag;
			randAlgo.ClearCustomData(Tag.RandTag);
			fitAlgo.ClearCustomData(Tag.FitTag);
		}

		protected internal override bool SearchRandomly(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as HybridTag;
			robotic.AlgorithmData = Tag.FitTag;
			var result = fitAlgo.SearchRandomly(robotic);
			robotic.AlgorithmData = Tag;
			return result;
		}

		protected internal override Vector3 RandomSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as HybridTag;
			robotic.AlgorithmData = Tag.RandTag;
			var result = randAlgo.RandomSearch(robotic);
			robotic.AlgorithmData = Tag;
			return result;
		}

		protected internal override Vector3 FitnessSearch(RFitness robotic)
		{
			var Tag = robotic.AlgorithmData as HybridTag;
			robotic.AlgorithmData = Tag.FitTag;
			var result = fitAlgo.FitnessSearch(robotic);
			robotic.AlgorithmData = Tag;
			return result;
		}

		protected internal override void AddHistory(RFitness r)
		{
			if (r.RandomSearch)
				randAlgo.AddHistory(r);
			else
				fitAlgo.AddHistory(r);
		}

		public override void InitializeParameter()
		{
			base.InitializeParameter();
			randAlgo = RandType.GetConstructor(Type.EmptyTypes).Invoke(null) as AFitness;
			randAlgo.Bind(problem, false);
			randAlgo.InitializeParameter();
			fitAlgo = FitType.GetConstructor(Type.EmptyTypes).Invoke(null) as AFitness;
			fitAlgo.Bind(problem, false);
			fitAlgo.InitializeParameter();
		}

		public override void CreateDefaultParameter()
		{
			base.CreateDefaultParameter();
			RandType = typeof(ARPSOFitness);
			FitType = typeof(ADSFitness);
		}

		Type RandType, FitType;

		[Parameter("AFitnessTypes", StringFuncName = "Type2String", Description = "Random Search Type")]
		public Type RandomAlgorithm
		{
			get { return RandType; }
			set
			{
				if (value.IsSubclassOf(baseType))
					RandType = value;
				else
					throw new Exception("must be type of AFitness");
			}
		}

		[Parameter("AFitnessTypes", StringFuncName = "Type2String", Description = "Fitness Search Type")]
		public Type FitnessAlgorithm
		{
			get { return FitType; }
			set
			{
				if (value.IsSubclassOf(baseType))
					FitType = value;
				else
					throw new Exception("must be type of AFitness");
			}
		}

		public static Type[] AFitnessTypes() { return Assembly.GetAssembly(baseType).GetTypes().Where(t => t.IsClass && t.IsSubclassOf(baseType) && t != typeof(AHybridFitness)).ToArray(); }
		public static string Type2String(Type type) { return type.Name; }

		private static Type baseType = typeof(AFitness);

		public override string GetName { get { return string.Format("{0}+{1}", randAlgo.GetName, fitAlgo.GetName); } }

		internal class HybridTag
		{
			public HybridTag(object RandTag, object FitTag)
			{
				this.RandTag = RandTag;
				this.FitTag = FitTag;
			}

			public object RandTag, FitTag;
		}
	}
}
