using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficRoadData : RoadEditorData<Road>
    {
        internal override Road[] GetAllRoads()
        {
            return _allRoads;
        }
    }
}
