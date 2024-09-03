using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficRoadDrawer : RoadDrawer<TrafficRoadData, Road>
    {
        internal TrafficRoadDrawer (TrafficRoadData data):base(data) 
        {
        }
    }
}
