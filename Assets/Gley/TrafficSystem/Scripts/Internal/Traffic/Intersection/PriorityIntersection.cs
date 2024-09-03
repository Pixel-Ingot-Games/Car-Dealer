using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Controls the priority intersection.
    /// </summary>
    internal class PriorityIntersection : GenericIntersection, IDestroyable
    {
        private readonly List<int> _waypointsToCkeck;
        private readonly List<Color> _waypointColor;
        private readonly float _requiredTime;

        private List<PedestrianCrossing> _pedestriansCrossing;
        private PriorityIntersectionData _priorityIntersectionData;
        private Vector3 _position;
        private float _currentTime;
        private int _currentRoadIndex;
        private int _tempRoadIndex;
        private bool _changeRequested;


        internal PriorityIntersection(PriorityIntersectionData priorityIntersectionData, TrafficWaypointsDataHandler trafficWaypointsDataHandler, IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler)
        {
            _priorityIntersectionData = priorityIntersectionData;
            for (int i = 0; i < _priorityIntersectionData.ExitWaypoints.Length; i++)
            {
                trafficWaypointsDataHandler.SetIntersection(_priorityIntersectionData.ExitWaypoints[i], this, false, false, false, true);
            }
            int nr = 0;
            for (int i = 0; i < _priorityIntersectionData.StopWaypoints.Length; i++)
            {
                for (int j = 0; j < _priorityIntersectionData.StopWaypoints[i].roadWaypoints.Length; j++)
                {
                    trafficWaypointsDataHandler.SetIntersection(_priorityIntersectionData.StopWaypoints[i].roadWaypoints[j], this, true, false, true, false);
                    _position += trafficWaypointsDataHandler.GetPosition(_priorityIntersectionData.StopWaypoints[i].roadWaypoints[j]);
                    nr++;
                }
            }
            _position = _position / nr;

            InitializePedestrianWaypoints(pedestrianWaypointsDataHandler);

            _carsInIntersection = new List<int>();
            _requiredTime = 3;
            _waypointsToCkeck = new List<int>();
            _waypointColor = new List<Color>();
            Assign();
        }


        public void Assign()
        {
            DestroyableManager.Instance.Register(this);
        }


        /// <summary>
        /// Check if the intersection road is free and update intersection priority
        /// </summary>
        /// <param name="waypointIndex"></param>
        /// <returns></returns>
        public override bool IsPathFree(int waypointIndex)
        {
            int road = 0;
            for (int i = 0; i < _priorityIntersectionData.StopWaypoints.Length; i++)
            {
                //if the waypoint is in the enter waypoints list needs to be verified if is free
                if (_priorityIntersectionData.StopWaypoints[i].roadWaypoints.Contains(waypointIndex))
                {
                    if (!_waypointsToCkeck.Contains(waypointIndex))
                    {
                        _waypointsToCkeck.Add(waypointIndex);
                        _waypointColor.Add(Color.green);
                    }
                    road = i;
                    if (IsPedestrianCrossing(road))
                    {
                        _waypointColor[_waypointsToCkeck.IndexOf(waypointIndex)] = Color.red;
                        return false;
                    }
                    bool stopChange = false;
                    //if vehicle is on current road, wait to pass before changing the road priority
                    if (i == _currentRoadIndex)
                    {
                        _changeRequested = false;
                        stopChange = true;
                    }

                    //construct priority if vehicle is not on the priority road
                    if (stopChange == false)
                    {
                        if (_tempRoadIndex == _currentRoadIndex)
                        {
                            _tempRoadIndex = road;
                            _changeRequested = true;
                            _currentTime = Time.timeSinceLevelLoad;
                        }
                    }
                    break;
                }
            }

            int index = _waypointsToCkeck.IndexOf(waypointIndex);

            //if a new vehicle is requesting access to intersection but there are vehicles on intersection -> wait
            if (_changeRequested == true)
            {
                if (_carsInIntersection.Count >= 1)
                {
                    _waypointColor[index] = Color.red;
                    return false;
                }
                _changeRequested = false;
                _currentRoadIndex = _tempRoadIndex;
            }

            //if the number of vehicles in intersection is <3 -> permit access
            if (_carsInIntersection.Count <= 3)
            {
                if (_priorityIntersectionData.StopWaypoints[_currentRoadIndex].roadWaypoints.Contains(waypointIndex))
                {
                    _waypointColor[index] = Color.green;
                    return true;
                }
            }

            //after some time change the priority road
            if (Time.timeSinceLevelLoad - _currentTime > _requiredTime)
            {
                _tempRoadIndex = road;
                _changeRequested = true;
                _currentTime = Time.timeSinceLevelLoad;
            }
            _waypointColor[index] = Color.red;
            return false;
        }


        public override void PedestrianPassed(int pedestrianIndex)
        {
            PedestrianCrossing ped = _pedestriansCrossing.FirstOrDefault(cond => cond.PedestrianIndex == pedestrianIndex);
            if (ped != null)
            {
                if (ped.Crossing == false)
                {
                    ped.Crossing = true;
                }
                else
                {
                    _pedestriansCrossing.Remove(ped);
                }
            }
        }


        internal override string GetName()
        {
            return _priorityIntersectionData.Name;
        }


        internal override void ResetIntersection()
        {
            base.ResetIntersection();
            _pedestriansCrossing = new List<PedestrianCrossing>();
        }

        internal Vector3 GetPosition()
        {
            return _position;
        }


        internal List<int> GetWaypointsToCkeck()
        {
            return _waypointsToCkeck;
        }


        internal List<Color> GetWaypointColors()
        {
            return _waypointColor;
        }

        internal override List<int> GetStopWaypoints()
        {
            var result = new List<int>();
            for (int i = 0; i < _priorityIntersectionData.StopWaypoints.Length; i++)
            {
                result.AddRange(_priorityIntersectionData.StopWaypoints[i].roadWaypoints);
            }
            return result;
        }


        internal override void UpdateIntersection(float realtimeSinceStartup)
        {

        }


        internal int GetCarsInIntersection()
        {
            return _carsInIntersection.Count;
        }


        internal List<PedestrianCrossing> GetPedestriansCrossing()
        {
            return _pedestriansCrossing;
        }


        internal override int[] GetPedStopWaypoint()
        {
            return new int[0];
        }


        private void InitializePedestrianWaypoints(IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler)
        {
            _pedestriansCrossing = new List<PedestrianCrossing>();
#if GLEY_PEDESTRIAN_SYSTEM
            for(int i=0;i<_priorityIntersectionData.StopWaypoints.Length;i++)
            {
                pedestrianWaypointsDataHandler.SetIntersection(_priorityIntersectionData.StopWaypoints[i].pedestrianWaypoints, this);
            }
            PedestrianSystem.Events.OnStreetCrossing += PedestrianWantsToCross;
#endif
        }


        private void PedestrianWantsToCross(int pedestrianIndex, IIntersection intersection, int waypointIndex)
        {
            if (intersection == this)
            {
                int road = GetRoadToCross(waypointIndex);
                _pedestriansCrossing.Add(new PedestrianCrossing(pedestrianIndex, road));
            }
        }


        private int GetRoadToCross(int waypoint)
        {
            for (int i = 0; i < _priorityIntersectionData.StopWaypoints.Length; i++)
            {
                for (int j = 0; j < _priorityIntersectionData.StopWaypoints[i].pedestrianWaypoints.Length; j++)
                {
                    if (_priorityIntersectionData.StopWaypoints[i].pedestrianWaypoints[j] == waypoint)
                    {
                        return i;
                    }
                }
            }
            Debug.LogError("Not Good - verify pedestrians assignments in priority intersection");
            return -1;
        }


        private bool IsPedestrianCrossing(int road)
        {
            if (_pedestriansCrossing.Count == 0)
            {
                return false;
            }
            return _pedestriansCrossing.FirstOrDefault(cond => cond.Road == road) != null;
        }


        public void OnDestroy()
        {
#if GLEY_PEDESTRIAN_SYSTEM
            PedestrianSystem.Events.OnStreetCrossing -= PedestrianWantsToCross;
#endif
        }
    }
}