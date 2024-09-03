using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Get path to a destination waypoint.
    /// </summary>
    internal class PathFindingManager 
    {
        private readonly GridDataHandler _gridDataHandler;
        private readonly PathFindingDataHandler _trafficPathFindingDataHandler;
        private readonly AStar _aStar;


        internal PathFindingManager (GridDataHandler gridDataHandler, PathFindingDataHandler trafficPathFindingDataHandler)
        {
            _gridDataHandler = gridDataHandler;
            _trafficPathFindingDataHandler = trafficPathFindingDataHandler;
            _aStar = new AStar ();
        }


        internal List<int> GetPathToDestination(int vehicleIndex, int currentWaypointIndex, Vector3 position, VehicleTypes vehicleType)
        {
            if (currentWaypointIndex < 0)
            {
                Debug.LogWarning($"Cannot find route to destination. Vehicle at index {vehicleIndex} is disabled or has an invalid target waypoint");
                return null;
            }

            int closestWaypointIndex = GetClosestPathFindingWaypoint(position, (int)vehicleType);
            if (closestWaypointIndex < 0)
            {
                Debug.LogWarning("No waypoint found closer to destination");
                return null;
            }

            List<int> path = _aStar.FindPath(currentWaypointIndex, closestWaypointIndex, (int)vehicleType, _trafficPathFindingDataHandler.GetPathFindingWaypoints());

            if (path != null)
            {
                return path;
            }

            Debug.LogWarning($"No path found for vehicle {vehicleIndex} to {position}");
            return null;
        }


        internal List<int> GetPath(Vector3 startPosition, Vector3 endPosition, VehicleTypes vehicleType)
        {
            var startIndex = GetClosestPathFindingWaypoint(startPosition, (int)vehicleType);
            if(startIndex== TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
            {
                Debug.LogWarning($"No traffic waypoint found close to {startPosition}");
                return null;
            }

            var endIndex = GetClosestPathFindingWaypoint(endPosition, (int)vehicleType);
            if(endIndex == TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
            {
                Debug.LogWarning($"No traffic waypoint found closed to {endPosition}");
                return null;
            }

            var path = _aStar.FindPath(startIndex, endIndex, (int)vehicleType, _trafficPathFindingDataHandler.GetPathFindingWaypoints());
            if (path == null)
            {
                Debug.LogWarning($"No path found from {startPosition} to {endPosition}");
            }
            return path;
        }


        private int GetClosestPathFindingWaypoint(Vector3 position, int type)
        {
            List<int> possibleWaypoints = _gridDataHandler.GetTrafficWaypointsAroundPosition(position);
            if (possibleWaypoints.Count == 0)
            {
                return TrafficSystemConstants.INVALID_WAYPOINT_INDEX;
            }


            float distance = float.MaxValue;
            int waypointIndex = TrafficSystemConstants.INVALID_WAYPOINT_INDEX;
            foreach (int waypoint in possibleWaypoints)
            {
                if (_trafficPathFindingDataHandler.GetAllowedAgents(waypoint).Contains(type))
                {
                    float newDistance = Vector3.SqrMagnitude(_trafficPathFindingDataHandler.GetWaypointPosition(waypoint) - position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        waypointIndex = waypoint;
                    }
                }
            }
            return waypointIndex;
        }
    }
}
