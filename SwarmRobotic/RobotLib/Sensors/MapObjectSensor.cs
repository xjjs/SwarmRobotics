using System.Collections.Generic;
using System.Linq;
using RobotLib.Environment;
using RobotLib.Obstacles;

namespace RobotLib.Sensors
{
    //public class MapObjectSensor : PrivateSensor<IEnumerable<NeighbourData<Obstacle>>>, IEnumerable<NeighbourData<Obstacle>>//, ILookup<string, Obstacle>
    //{
    //    public MapObjectSensor() : base("obstacle sensor", Enumerable.Empty<NeighbourData<Obstacle>>()) { }

    //    public MapObjectSensor(List<NeighbourData<Obstacle>> list) : base("obstacle sensor", list.Where(o => o.isNeighbour)) { }

    //    public IEnumerator<NeighbourData<Obstacle>> GetEnumerator() { return SensorData.GetEnumerator(); }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

    //    public IEnumerable<NeighbourData<Obstacle>> this[string key]
    //    {
    //        get { return SensorData.Where(o => o.Target.@class == key); }
    //    }
    //}
}
