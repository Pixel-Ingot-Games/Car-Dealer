using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Performs waypoint operations
    /// </summary>
    internal class WaypointManager : IDestroyable
    {
        private readonly Dictionary<int, int> _playerTarget;
        private readonly int[] _target; //contains at index the waypoint index of the target waypoint of that agent. Agent at position 2 has the target waypoint index target[2]
        private readonly bool[] _hasPath;
        private readonly Dictionary<int, Queue<int>> _pathToDestination;
        private readonly GridDataHandler _gridDataHandler;
        private readonly TrafficWaypointsDataHandler _trafficWaypointsDataHandler;

        private List<int> _disabledWaypoints;
        private SpawnWaypointSelector _spawnWaypointSelector;
        private bool _debugGiveWay;


        internal WaypointManager(TrafficWaypointsDataHandler trafficWaypointsDataHandler, GridDataHandler gridDataHandler, int nrOfVehicles, SpawnWaypointSelector spawnWaypointSelector, bool debugGIveWay)
        {
            _trafficWaypointsDataHandler = trafficWaypointsDataHandler;
            _gridDataHandler = gridDataHandler;
            Assign();
            WaypointEvents.onTrafficLightChanged += TrafficLightChanged;
            _playerTarget = new Dictionary<int, int>();
            _target = new int[nrOfVehicles];
            for (int i = 0; i < _target.Length; i++)
            {
                _target[i] = TrafficSystemConstants.INVALID_VEHICLE_INDEX;
            }
            _pathToDestination = new Dictionary<int, Queue<int>>();
            _hasPath = new bool[nrOfVehicles];
            _disabledWaypoints = new List<int>();
            _debugGiveWay = debugGIveWay;
            SetSpawnWaypointSelector(spawnWaypointSelector);
        }


        public void Assign()
        {
            DestroyableManager.Instance.Register(this);
        }


        internal int[] GetTargetWaypoints()
        {
            return _target;
        }


        /// <summary>
        /// Get Target waypoint index of agent
        /// </summary>
        /// <param name="agentIndex"></param>
        /// <returns></returns>
        internal int GetTargetWaypointIndex(int agentIndex)
        {
            return _target[agentIndex];
        }


        /// <summary>
        /// Get orientation of the waypoint
        /// </summary>
        /// <param name="waypointIndex"></param>
        /// <returns></returns>
        internal Quaternion GetNextOrientation(int waypointIndex)
        {
            if (!_trafficWaypointsDataHandler.HasNeighbors(waypointIndex))
            {
                return Quaternion.identity;
            }
            return Quaternion.LookRotation(
                _trafficWaypointsDataHandler.GetPosition(_trafficWaypointsDataHandler.GetNeighbors(waypointIndex)[0]) - _trafficWaypointsDataHandler.GetPosition(waypointIndex)
                );
        }


        /// <summary>
        /// Get orientation of the waypoint
        /// </summary>
        /// <param name="waypointIndex"></param>
        /// <returns></returns>
        internal Quaternion GetPrevOrientation(int waypointIndex)
        {
            if (!_trafficWaypointsDataHandler.HasPrevs(waypointIndex))
            {
                return Quaternion.identity;
            }
            return Quaternion.LookRotation(
                _trafficWaypointsDataHandler.GetPosition(waypointIndex) - _trafficWaypointsDataHandler.GetPosition(_trafficWaypointsDataHandler.GetPrevs(waypointIndex)[0])
                );
        }


        /// <summary>
        /// Enables unavailable waypoints
        /// </summary>
        internal void EnableAllWaypoints()
        {
            _trafficWaypointsDataHandler.SetTemperaryDisabledValue(_disabledWaypoints, false);
            _disabledWaypoints = new List<int>();
        }


        /// <summary>
        /// Mark a waypoint as disabled
        /// </summary>
        /// <param name="waypointIndex"></param>
        internal void AddDisabledWaypoint(int waypointIndex)
        {
            _disabledWaypoints.Add(waypointIndex);
            _trafficWaypointsDataHandler.SetTemperaryDisabledValue(waypointIndex, true);
        }


        internal List<int> GetDisabledWaypoints()
        {
            return _disabledWaypoints;
        }


        /// <summary>
        /// Get a free waypoint connected to the current one
        /// </summary>
        /// <param name="agentIndex">agent that requested the waypoint</param>
        /// <param name="vehicleType">type of the agent that requested the waypoint</param>
        /// <returns></returns>
        internal int GetCurrentLaneWaypointIndex(int agentIndex, VehicleTypes vehicleType)
        {
            int waypointIndex = PeekPoint(agentIndex);
            if (waypointIndex != -1)
            {
                return waypointIndex;
            }

            int oldWaypointIndex = GetTargetWaypointIndex(agentIndex);

            //check direct neighbors
            if (_trafficWaypointsDataHandler.HasNeighbors(oldWaypointIndex))
            {
                List<int> possibleWaypoints = _trafficWaypointsDataHandler.GetNeighborsWithConditions(oldWaypointIndex, vehicleType);
                if (possibleWaypoints.Count > 0)
                {
                    waypointIndex = possibleWaypoints[Random.Range(0, possibleWaypoints.Count)];
                }
            }

            //check other lanes
            if (waypointIndex == -1)
            {
                if (_trafficWaypointsDataHandler.HasOtherLanes(oldWaypointIndex))
                {
                    List<int> possibleWaypoints = _trafficWaypointsDataHandler.GetOtherLanesWithConditions(oldWaypointIndex, vehicleType);
                    if (possibleWaypoints.Count > 0)
                    {
                        waypointIndex = possibleWaypoints[Random.Range(0, possibleWaypoints.Count)];
                    }
                }
            }

            //check neighbors that are not allowed
            if (waypointIndex == -1)
            {
                if (_trafficWaypointsDataHandler.HasNeighbors(oldWaypointIndex))
                {
                    List<int> possibleWaypoints = _trafficWaypointsDataHandler.GetNeighborsWithConditions(oldWaypointIndex);
                    if (possibleWaypoints.Count > 0)
                    {
                        waypointIndex = possibleWaypoints[Random.Range(0, possibleWaypoints.Count)];
                    }
                }
            }

            //check other lanes that are not allowed
            if (waypointIndex == -1)
            {
                if (_trafficWaypointsDataHandler.HasOtherLanes(oldWaypointIndex))
                {
                    List<int> possibleWaypoints = _trafficWaypointsDataHandler.GetOtherLanesWithConditions(oldWaypointIndex);
                    if (possibleWaypoints.Count > 0)
                    {
                        waypointIndex = possibleWaypoints[Random.Range(0, possibleWaypoints.Count)];
                    }
                }
            }
            return waypointIndex;
        }


        internal int GetAgentIndexAtTarget(int waypointIndex)
        {
            for (int i = 0; i < _target.Length; i++)
            {
                if (_target[i] == waypointIndex)
                {
                    return i;
                }
            }
            return -1;
        }


        internal void RegisterPlayer(int id, int waypointIndex)
        {
            if (!_playerTarget.ContainsKey(id))
            {
                _playerTarget.Add(id, waypointIndex);
            }
        }


        internal void UpdatePlayerWaypoint(int id, int waypointIndex)
        {
            _playerTarget[id] = waypointIndex;
        }


        /// <summary>
        /// Check if waypoint is a target for another agent
        /// </summary>
        /// <param name="waypointIndex"></param>
        /// <returns></returns>
        internal bool IsThisWaypointATarget(int waypointIndex)
        {
            for (int i = 0; i < _target.Length; i++)
            {
                if (_target[i] == waypointIndex)
                {
                    return true;
                }
            }

            return _playerTarget.ContainsValue(waypointIndex);
        }


        internal bool AreTheseWaypointsATarget(int[] waypointsToCheck)
        {
            return _target.Intersect(waypointsToCheck).Any() || _playerTarget.Values.Any(v => waypointsToCheck.Contains(v));
        }


        internal bool HaveCommonNeighbors(int fromWaypointIndex, int toWaypointIndex, int level = 0)
        {
            if (level == 0)
            {
                if (_trafficWaypointsDataHandler.GetNeighbors(fromWaypointIndex).Intersect(_trafficWaypointsDataHandler.GetNeighbors(toWaypointIndex)).Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        /// <summary>
        /// Set next waypoint and trigger the required events
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="waypointIndex"></param>
        internal void SetNextWaypoint(int vehicleIndex, int waypointIndex)
        {
            bool stop = false;
            if (_hasPath[vehicleIndex])
            {
                Queue<int> queue;
                if (_pathToDestination.TryGetValue(vehicleIndex, out queue))
                {
                    queue.Dequeue();
                    if (queue.Count == 0)
                    {
                        stop = true;
                    }
                }
            }

            SetTargetWaypoint(vehicleIndex, waypointIndex);
            int targetWaypointIndex = GetTargetWaypointIndex(vehicleIndex);
            if (stop == true)
            {
                WaypointEvents.TriggerStopStateChangedEvent(vehicleIndex, true);
            }
            if (_trafficWaypointsDataHandler.IsStop(targetWaypointIndex))
            {
                WaypointEvents.TriggerStopStateChangedEvent(vehicleIndex, true);
            }
            if (_trafficWaypointsDataHandler.IsGiveWay(targetWaypointIndex))
            {
                WaypointEvents.TriggerGiveWayStateChangedEvent(vehicleIndex, true);
            }

            if (_trafficWaypointsDataHandler.IsComplexGiveWay(targetWaypointIndex))
            {
                WaypointEvents.TriggerGiveWayStateChangedEvent(vehicleIndex, true);
            }
        }


        /// <summary>
        /// Remove target waypoint for the agent at index
        /// </summary>
        /// <param name="agentIndex"></param>
        internal void RemoveAgent(int agentIndex)
        {
            //MarkWaypointAsPassed(agentIndex);
            SetTargetWaypointIndex(agentIndex, -1);
        }


        /// <summary>
        /// Directly set the target waypoint for the vehicle at index.
        /// Used to set first waypoint after vehicle initialization
        /// </summary>
        /// <param name="agentIndex"></param>
        /// <param name="waypointIndex"></param>
        internal void SetTargetWaypoint(int agentIndex, int waypointIndex)
        {
            MarkWaypointAsPassed(agentIndex);
            SetTargetWaypointIndex(agentIndex, waypointIndex);
        }


        internal bool CanContinueStraight(int vehicleIndex, VehicleTypes vehicleType)
        {
            int targetWaypointIndex = GetTargetWaypointIndex(vehicleIndex);
            if (_trafficWaypointsDataHandler.HasNeighbors(targetWaypointIndex))
            {
                if (_hasPath[vehicleIndex])
                {
                    Queue<int> queue;
                    if (_pathToDestination.TryGetValue(vehicleIndex, out queue))
                    {
                        if (queue.Count > 0)
                        {
                            int nextWaypoint = queue.Peek();
                            if (!_trafficWaypointsDataHandler.HasWaypointInNeighbors(targetWaypointIndex, nextWaypoint))
                            {
                                return false;
                            }
                        }
                    }
                }
                if (_trafficWaypointsDataHandler.HasNeighborsForVehicleType(targetWaypointIndex, vehicleType))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Check if can switch to target waypoint
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal bool CanEnterIntersection(int vehicleIndex)
        {
            int waypointIdex = GetTargetWaypointIndex(vehicleIndex);

            var intersections = _trafficWaypointsDataHandler.GetAssociatedIntersections(waypointIdex);
            foreach (var intersection in intersections)
            {
                if (intersection.IsPathFree(waypointIdex) == false)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Check if the previous waypoints are free
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="freeWaypointsNeeded"></param>
        /// <param name="possibleWaypoints"></param>
        /// <returns></returns>
        internal bool AllPreviousWaypointsAreFree(int vehicleIndex, float distance, int waypointToCheck, ref int incomingCarIndex)
        {
            return IsTargetFree(waypointToCheck, distance, GetTargetWaypointIndex(vehicleIndex), vehicleIndex, ref incomingCarIndex);
        }


        /// <summary>
        /// Check what vehicle is in front
        /// </summary>
        /// <param name="vehicleIndex1"></param>
        /// <param name="vehicleIndex2"></param>
        /// <returns>
        /// 1-> if 1 is in front of 2
        /// 2-> if 2 is in front of 1
        /// 0-> if it is not possible to determine
        /// </returns>
        internal int IsInFront(int vehicleIndex1, int vehicleIndex2)
        {
            //compares waypoints to determine which vehicle is in front 
            int distance = 0;
            int[] neighbors = _trafficWaypointsDataHandler.GetNeighbors(GetTargetWaypointIndex(vehicleIndex1));
            //if no neighbors are available -> not possible to determine
            if (neighbors.Length == 0)
            {
                return 0;
            }

            //check next 10 waypoints to find waypoint 2
            int startWaypointIndex = neighbors[0];
            while (startWaypointIndex != GetTargetWaypointIndex(vehicleIndex2) && distance < 10)
            {
                distance++;
                if (!_trafficWaypointsDataHandler.HasNeighbors(startWaypointIndex))
                {
                    //if not found -> not possible to determine
                    return 0;
                }
                startWaypointIndex = _trafficWaypointsDataHandler.GetNeighbors(startWaypointIndex)[0];
            }


            int distance2 = 0;
            neighbors = _trafficWaypointsDataHandler.GetNeighbors(GetTargetWaypointIndex(vehicleIndex2));
            if (neighbors.Length == 0)
            {
                return 0;
            }

            startWaypointIndex = neighbors[0];
            while (startWaypointIndex != GetTargetWaypointIndex(vehicleIndex1) && distance2 < 10)
            {
                distance2++;
                if (!_trafficWaypointsDataHandler.HasNeighbors(startWaypointIndex))
                {
                    //if not found -> not possible to determine
                    return 0;
                }
                startWaypointIndex = _trafficWaypointsDataHandler.GetNeighbors(startWaypointIndex)[0];
            }

            //if no waypoints found -> not possible to determine
            if (distance == 10 && distance2 == 10)
            {
                return 0;
            }

            if (distance2 > distance)
            {
                return 2;
            }

            return 1;
        }


        /// <summary>
        /// Check if 2 vehicles have the same target
        /// </summary>
        /// <param name="vehicleIndex1"></param>
        /// <param name="VehicleIndex2"></param>
        /// <returns></returns>
        internal bool IsSameTarget(int vehicleIndex1, int VehicleIndex2)
        {
            return GetTargetWaypointIndex(vehicleIndex1) == GetTargetWaypointIndex(VehicleIndex2);
        }


        /// <summary>
        /// Get rotation of the target waypoint
        /// </summary>
        /// <param name="agentIndex"></param>
        /// <returns></returns>
        internal Quaternion GetTargetWaypointRotation(int agentIndex)
        {
            int[] neighbors = _trafficWaypointsDataHandler.GetNeighbors(GetTargetWaypointIndex(agentIndex));
            if (neighbors.Length == 0)
            {
                return Quaternion.identity;
            }
            return Quaternion.LookRotation(_trafficWaypointsDataHandler.GetPosition(neighbors[0]) - _trafficWaypointsDataHandler.GetPosition(GetTargetWaypointIndex(agentIndex)));
        }


        /// <summary>
        /// Check if a change of lane is possible
        /// Used to overtake and give way
        /// </summary>
        /// <param name="agentIndex"></param>
        /// <param name="vehicleType"></param>
        /// <returns></returns>
        internal int GetOtherLaneWaypointIndex(int agentIndex, VehicleTypes vehicleType, RoadSide side = RoadSide.Any, Vector3 forwardVector = default)
        {
            int waypointIndex = PeekPoint(agentIndex);
            if (waypointIndex != -1)
            {
                return waypointIndex;
            }

            int currentWaypointIndex = GetTargetWaypointIndex(agentIndex);

            if (_trafficWaypointsDataHandler.HasOtherLanes(currentWaypointIndex))
            {
                List<int> possibleWaypoints = _trafficWaypointsDataHandler.GetOtherLanesWithConditions(currentWaypointIndex, vehicleType);
                if (possibleWaypoints.Count > 0)
                {
                    return GetSideWaypoint(possibleWaypoints, currentWaypointIndex, side, forwardVector);
                }
            }

            return -1;
        }


        internal int GetNeighborCellWaypoint(int row, int column, int depth, VehicleTypes carType, Vector3 playerPosition, Vector3 playerDirection, bool useWaypointPriority)
        {

            //get all cell neighbors for the specified depth
            List<Vector2Int> neighbors = _gridDataHandler.GetCellNeighbors(row, column, depth, false);

            for (int i = neighbors.Count - 1; i >= 0; i--)
            {
                if (!_gridDataHandler.HasTrafficSpawnWaypoints(neighbors[i]))
                {
                    neighbors.RemoveAt(i);
                }
            }

            //if neighbors exists
            if (neighbors.Count > 0)
            {
                return ApplyNeighborSelectorMethod(neighbors, playerPosition, playerDirection, carType, useWaypointPriority);
            }

            return -1;
        }


        /// <summary>
        /// Set the default waypoint generating method
        /// </summary>
        /// <param name="spawnWaypointSelector"></param>
        internal void SetSpawnWaypointSelector(SpawnWaypointSelector spawnWaypointSelector)
        {
            _spawnWaypointSelector = spawnWaypointSelector;
        }


        internal void SetAgentPath(int agentIndex, Queue<int> pathWaypoints)
        {
            if (!_pathToDestination.ContainsKey(agentIndex))
            {
                _pathToDestination.Add(agentIndex, pathWaypoints);
                _hasPath[agentIndex] = true;
            }
            _pathToDestination[agentIndex] = pathWaypoints;
        }


        internal void RemoveAgentPath(int agentIndex)
        {
            _pathToDestination.Remove(agentIndex);
            _hasPath[agentIndex] = false;
        }


        internal bool HasPath(int agentIndex)
        {
            return _hasPath[agentIndex];
        }


        internal Queue<int> GetPath(int agentIndex)
        {
            if (_pathToDestination.ContainsKey(agentIndex))
            {
                return _pathToDestination[agentIndex];
            }
            return new Queue<int>();
        }


        private void SetTargetWaypointIndex(int agentIndex, int waypointIndex)
        {
            //set current target
            _target[agentIndex] = waypointIndex;
        }


        /// <summary>
        /// called when a waypoint was passed
        /// </summary>
        /// <param name="vehicleIndex"></param>
        private void MarkWaypointAsPassed(int vehicleIndex)
        {
            if (GetTargetWaypointIndex(vehicleIndex) != -1)
            {
                int waypointIndex = GetTargetWaypointIndex(vehicleIndex);
                if (_trafficWaypointsDataHandler.IsExit(waypointIndex))
                {
                    var intersections = _trafficWaypointsDataHandler.GetAssociatedIntersections(waypointIndex);
                    foreach (var intersection in intersections)
                    {
                        intersection.VehicleLeft(vehicleIndex);
                    }
                }

                if (_trafficWaypointsDataHandler.IsEnter(waypointIndex))
                {
                    var intersections = _trafficWaypointsDataHandler.GetAssociatedIntersections(waypointIndex);
                    foreach (var intersection in intersections)
                    {
                        intersection.VehicleEnter(vehicleIndex);
                    }
                }

                if (_trafficWaypointsDataHandler.IsTriggerEvent(waypointIndex))
                {
                    Events.TriggerWaypointReachedEvent(vehicleIndex, waypointIndex, _trafficWaypointsDataHandler.GetEventData(waypointIndex));
                }
            }
        }


        /// <summary>
        /// Check if previous waypoints are free
        /// </summary>
        /// <param name="waypointIndex"></param>
        /// <param name="level"></param>
        /// <param name="initialWaypointIndex"></param>
        /// <returns></returns>
        private bool IsTargetFree(int waypointIndex, float distance, int initialWaypointIndex, int currentCarIndex, ref int incomingCarIndex)
        {
#if UNITY_EDITOR
            if (_debugGiveWay)
            {
                Debug.DrawLine(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex), Color.green, 1);
            }
#endif
            if (distance <= 0)
            {
#if UNITY_EDITOR
                if (_debugGiveWay)
                {
                    Debug.DrawLine(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex), Color.green, 1);
                }
#endif
                return true;
            }
            if (waypointIndex == initialWaypointIndex)
            {
#if UNITY_EDITOR
                if (_debugGiveWay)
                {
                    Debug.DrawLine(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex), Color.white, 1);
                }
#endif
                return true;
            }
            if (IsThisWaypointATarget(waypointIndex))
            {
                incomingCarIndex = GetAgentIndexAtTarget(waypointIndex);
                if (GetTargetWaypointIndex(currentCarIndex) == waypointIndex)
                {
#if UNITY_EDITOR
                    if (_debugGiveWay)
                    {
                        Debug.DrawLine(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex), Color.blue, 1);
                    }
#endif
                    return true;
                }
                else
                {
#if UNITY_EDITOR
                    if (_debugGiveWay)
                    {
                        Debug.DrawLine(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex), Color.red, 1);
                    }
#endif
                    return false;
                }
            }
            else
            {
                if (!_trafficWaypointsDataHandler.HasPrevs(waypointIndex))
                {
                    return true;
                }
                distance -= Vector3.Distance(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex));
                var prevs = _trafficWaypointsDataHandler.GetPrevs(waypointIndex);
                for (int i = 0; i < prevs.Length; i++)
                {
                    if (!IsTargetFree(prevs[i], distance, initialWaypointIndex, currentCarIndex, ref incomingCarIndex))
                    {
                        if (_debugGiveWay)
                        {
                            Debug.DrawLine(_trafficWaypointsDataHandler.GetPosition(waypointIndex), _trafficWaypointsDataHandler.GetPosition(initialWaypointIndex), Color.magenta, 1);
                        }
                        return false;
                    }
                }
            }
            return true;
        }


        private int PeekPoint(int agentIndex)
        {
            Queue<int> queue;
            if (_pathToDestination.TryGetValue(agentIndex, out queue))
            {
                if (queue.Count > 0)
                {
                    return queue.Peek();
                }
                return -2;
            }
            return -1;
        }


        private int GetSideWaypoint(List<int> waypointIndexes, int currentWaypointIndex, RoadSide side, Vector3 forwardVector)
        {
            switch (side)
            {
                case RoadSide.Any:
                    return waypointIndexes[Random.Range(0, waypointIndexes.Count)];
                case RoadSide.Left:
                    for (int i = 0; i < waypointIndexes.Count; i++)
                    {
                        if (Vector3.SignedAngle(_trafficWaypointsDataHandler.GetPosition(waypointIndexes[i]) - _trafficWaypointsDataHandler.GetPosition(currentWaypointIndex), forwardVector, Vector3.up) > 5)
                        {
                            return waypointIndexes[i];
                        }
                    }
                    break;
                case RoadSide.Right:
                    for (int i = 0; i < waypointIndexes.Count; i++)
                    {
                        if (Vector3.SignedAngle(_trafficWaypointsDataHandler.GetPosition(waypointIndexes[i]) - _trafficWaypointsDataHandler.GetPosition(currentWaypointIndex), forwardVector, Vector3.up) < -5)
                        {
                            return waypointIndexes[i];
                        }
                    }
                    break;
            }

            return -1;
        }


        private void TrafficLightChanged(int waypointIndex, bool newValue)
        {
            if (_trafficWaypointsDataHandler.IsStop(waypointIndex) != newValue)
            {
                _trafficWaypointsDataHandler.SetStopValue(waypointIndex, newValue);
                for (int i = 0; i < _target.Length; i++)
                {
                    if (_target[i] == waypointIndex)
                    {
                        WaypointEvents.TriggerStopStateChangedEvent(i, newValue);
                    }
                }
            }
        }


        private int ApplyNeighborSelectorMethod(List<Vector2Int> neighbors, Vector3 playerPosition, Vector3 playerDirection, VehicleTypes carType, bool useWaypointPriority)
        {
            try
            {
                return _spawnWaypointSelector(neighbors, playerPosition, playerDirection, carType, useWaypointPriority);
            }
            catch (System.Exception e)
            {
                Debug.LogError(TrafficSystemErrors.NoNeighborSelectorMethod(e.Message));
                return DefaultDelegates.GetRandomSpawnWaypoint(neighbors, playerPosition, playerDirection, carType, useWaypointPriority);
            }
        }


        /// <summary>
        /// Cleanup
        /// </summary>
        public void OnDestroy()
        {
            WaypointEvents.onTrafficLightChanged -= TrafficLightChanged;
        }
    }
}