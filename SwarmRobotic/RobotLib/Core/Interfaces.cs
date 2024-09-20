using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RobotLib
{
    /// <summary>
    /// 参数接口：创建默认参数、初始化参数
    /// </summary>
    public interface IParameter
    {
        void CreateDefaultParameter();
        void InitializeParameter();
    }

    public interface ISensor
    {
        string Name { get; }
        void ApplyChange();
    }

    public interface ISensor<T> : ISensor
    {
        T SensorData { get; }
    }

    //public interface ISelfSensor<T> : ISensor<T>
    //{
    //    new T SensorData { get; set; }
    //}

    public interface IStateSensor<T> : ISensor<T>
    {
		/// <summary>
		/// New Data or Delta Data due to overriding class
		/// </summary>
		T NewData { get; set; }
		/// <summary>
		/// Set SensorData according to NewData
		/// </summary>
		IEnumerable<T> NeighbourData { get; set; }
    }

    public interface IValueSensor<T> : IStateSensor<T>
	{
        T CalculateNeighbourData(IValueSensor<T> data);
	}

	//public interface IPublicValueSensor<T, T2> : IPublicSensor<T>
	//{
	//    T2 CalculateNeighbourData(IPublicValueSensor<T, T2> data);
	//    IEnumerable<T2> NeighbourData { get; set; }
	//}

	//public interface IPosition : IPublicValueSensor<Vector3>
	//{
	//    /// <summary>
	//    /// Used for displaying
	//    /// </summary>
	//    Vector3 GlobalSensorData { get; }
	//    Matrix TranformMatrix { get; }
	//    /// <summary>
	//    /// Moves the robotic and clears velocity
	//    /// </summary>
	//    void Move();

	//    //Vector3 AngleSensorData { get; }
	//}
}

/******************************/
//接口：参数、传感器（自身数据）、状态传感器（邻居数据）
//传感器接口方法：应用改变（更新数据？）
//值传感器接口方法：计算邻居数据