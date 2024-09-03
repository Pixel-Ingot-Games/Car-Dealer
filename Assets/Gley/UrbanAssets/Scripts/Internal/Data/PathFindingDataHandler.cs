using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    /// <summary>
    /// Path finding implementation.
    /// </summary>
    public class PathFindingDataHandler
    {
        private readonly PathFindingData _pedestrianPathFindingData;


        public PathFindingDataHandler(PathFindingData data)
        {
            _pedestrianPathFindingData = data;

        }


        internal PathFindingWaypoint[] GetPathFindingWaypoints()
        {
            return _pedestrianPathFindingData.AllPathFindingWaypoints;
        }


        internal Vector3 GetWaypointPosition(int waypointIndex)
        {
            return GetPathFindingWaypoint(waypointIndex).WorldPosition;
        }


        internal int[] GetAllowedAgents(int waypointIndex)
        {
            return GetPathFindingWaypoint(waypointIndex).AllowedAgents;
        }


        private PathFindingWaypoint GetPathFindingWaypoint(int waypointIndex)
        {
            return GetPathFindingWaypoints()[waypointIndex];
        }
    }
}
