using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotLib.Sensors
{
    //public class PrivateSensor<T> : ISelfSensor<T>
    //{
    //    public PrivateSensor(string name) { Name = name; }

    //    public PrivateSensor(string name, T @default) : this(name) { SensorData = @default; }

    //    public string Name { get; private set; }

    //    public T SensorData { get; set; }

    //    public override string ToString() { return SensorData.ToString(); }
    //}

    public abstract class Sensor<T> : IStateSensor<T>
	{
		public Sensor(string name) { Name = name; NeighbourData = Enumerable.Empty<T>(); }

        public Sensor(string name, T @default) : this(name) { SensorData = @default; }

		public string Name { get; private set; }

		public virtual T SensorData { get; protected set; }

		public IEnumerable<T> NeighbourData { get; set; }

		public T NewData { get; set; }

		public abstract void ApplyChange();

		public override string ToString() { return SensorData.ToString(); }
	}

    public class StateSensor<T> : Sensor<T>
	{
		//public PublicStateSensor(string name) : base(name) { }

		public StateSensor(string name, T @default) : base(name, @default) { }

		public override void ApplyChange() { SensorData = NewData; }
	}

    public abstract class ValueSensor<T> : Sensor<T>, IValueSensor<T>
	{
		//public PublicValueSensor(string name) : base(name) { }

        public ValueSensor(string name, T @default) : base(name, @default) { }

        public abstract T CalculateNeighbourData(IValueSensor<T> data);
	}

	//public abstract class PublicValueSensor<T, T2> : PublicSensor<T>, IPublicValueSensor<T, T2>
	//{
	//    public PublicValueSensor(string name) : base(name) { NeighbourData = Enumerable.Empty<T2>(); }

	//    public PublicValueSensor(string name, T @default) : base(name, @default) { }

	//    public abstract T2 CalculateNeighbourData(IPublicValueSensor<T, T2> data);

	//    public IEnumerable<T2> NeighbourData { get; set; }
	//}
}
