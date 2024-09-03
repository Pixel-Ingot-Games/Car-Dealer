using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Gley.TrafficSystem.Editor
{
    public class TrafficPathFindingCreator
    {
        public void GenerateWaypoints(WaypointSettings[] allEditorWaypoints)
        {
            int ChangeLanePenalty = 10;
            var allPathFindingWaypoints = new List<PathFindingWaypoint>();
            for (int i = 0; i < allEditorWaypoints.Length; i++)
            {
                List<int> penalties = new List<int>();
                List<WaypointSettingsBase> neighbors = new List<WaypointSettingsBase>();

                for (int j = 0; j < allEditorWaypoints[i].neighbors.Count; j++)
                {
                    neighbors.Add(allEditorWaypoints[i].neighbors[j]);
                    penalties.Add(allEditorWaypoints[i].neighbors[j].penalty);
                }
                for (int j = 0; j < allEditorWaypoints[i].otherLanes.Count; j++)
                {
                    neighbors.Add(allEditorWaypoints[i].otherLanes[j]);
                    penalties.Add(allEditorWaypoints[i].otherLanes[j].penalty + ChangeLanePenalty);
                }

                allPathFindingWaypoints.Add(new PathFindingWaypoint(i, allEditorWaypoints[i].transform.position, 0, 0, -1, neighbors.ToListIndex(allEditorWaypoints), penalties.ToArray(), allEditorWaypoints[i].allowedCars.Cast<int>().ToArray()));
            }

            PathFindingData data = MonoBehaviourUtilities.GetOrCreateObjectScript<PathFindingData>(TrafficSystemConstants.PlayHolder, false);
            data.SetPathFindingWaypoints(allPathFindingWaypoints.ToArray());
        }
    }
}