using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficLaneData : LaneEditorData<Road, WaypointSettings>
    {
        internal TrafficLaneData(RoadEditorData<Road> roadData) : base(roadData)
        {
        }
    }
}