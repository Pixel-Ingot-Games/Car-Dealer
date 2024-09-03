using Gley.UrbanSystem.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Controls the traffic light intersections. 
    /// </summary>
    internal class TrafficLightsIntersection : GenericIntersection
    {
        private readonly TrafficLightsIntersectionData _trafficLightsIntersectionData;
        private readonly TrafficLightsColor[] _intersectionState;
        private readonly float[] _roadGreenLightTime;

        private TrafficLightsBehaviour _trafficLightsBehaviour;
        private float _currentTime;
        private int _nrOfRoads;
        private int _currentRoad;
        private bool _yellowLight;
        private bool _stopUpdate;
        private bool _hasPedestrians;


        /// <summary>
        /// Constructor used for conversion from editor intersection type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stopWaypoints"></param>
        /// <param name="greenLightTime"></param>
        /// <param name="yellowLightTime"></param>
        internal TrafficLightsIntersection(TrafficLightsIntersectionData trafficLightsIntersectionData, TrafficWaypointsDataHandler trafficWaypointsDataHandler, IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler, TrafficLightsBehaviour trafficLightsBehaviour, float greenLightTime, float yellowLightTime)
        {
            _trafficLightsIntersectionData = trafficLightsIntersectionData;
            SetTrafficLightsBehaviour(trafficLightsBehaviour);

            _nrOfRoads = _trafficLightsIntersectionData.StopWaypoints.Length;

            GetPedestrianRoads(pedestrianWaypointsDataHandler);

            _roadGreenLightTime = new float[_nrOfRoads];
            for (int i = 0; i < _trafficLightsIntersectionData.StopWaypoints.Length; i++)
            {
                _roadGreenLightTime[i] = _trafficLightsIntersectionData.StopWaypoints[i].greenLightTime;
            }

            SetPedestrianGreenLightTime();

            if (_nrOfRoads == 0)
            {
                Debug.LogWarning("Intersection " + _trafficLightsIntersectionData.Name + " has some unassigned references");
                return;
            }

            _carsInIntersection = new List<int>();

            for (int i = 0; i < _trafficLightsIntersectionData.ExitWaypoints.Length; i++)
            {
                trafficWaypointsDataHandler.SetIntersection(_trafficLightsIntersectionData.ExitWaypoints[i], this, false, false, false, true);
            }
            for (int i = 0; i < _trafficLightsIntersectionData.StopWaypoints.Length; i++)
            {
                for (int j = 0; j < _trafficLightsIntersectionData.StopWaypoints[i].roadWaypoints.Length; j++)
                {
                    trafficWaypointsDataHandler.SetIntersection(_trafficLightsIntersectionData.StopWaypoints[i].roadWaypoints[j], this, false, true, true, false);
                }
            }

            _intersectionState = new TrafficLightsColor[_nrOfRoads];

            _currentRoad = Random.Range(0, _nrOfRoads);
            ChangeCurrentRoadColors(_currentRoad, TrafficLightsColor.Green);
            ChangeAllRoadsExceptSelectd(_currentRoad, TrafficLightsColor.Red);
            ApplyColorChanges();

            _currentTime = 0;
            if (greenLightTime >= 0)
            {
                for (int i = 0; i < _roadGreenLightTime.Length; i++)
                {
                    _roadGreenLightTime[i] = greenLightTime;
                }
            }
            if (yellowLightTime >= 0)
            {
                _trafficLightsIntersectionData.YellowLightTime = yellowLightTime;
            }

            for (int i = 0; i < _roadGreenLightTime.Length; i++)
            {
                if (_roadGreenLightTime[i] == 0)
                {
                    _roadGreenLightTime[i] = _trafficLightsIntersectionData.GreenLightTime;
                }
            }
        }


        public override void PedestrianPassed(int pedestrianIndex)
        {

        }


        public override bool IsPathFree(int waypointIndex)
        {
            return false;
        }


        internal override string GetName()
        {
            return _trafficLightsIntersectionData.Name;
        }


        internal override int[] GetPedStopWaypoint()
        {
            return _trafficLightsIntersectionData.PedestrianWaypoints;
        }


        /// <summary>
        /// Change traffic lights color
        /// </summary>
        internal override void UpdateIntersection(float realtimeSinceStartup)
        {
            if (_stopUpdate)
                return;
            if (_yellowLight == false)
            {
                if (realtimeSinceStartup - _currentTime > _roadGreenLightTime[_currentRoad])
                {
                    ChangeCurrentRoadColors(_currentRoad, TrafficLightsColor.YellowGreen);
                    ChangeCurrentRoadColors(GetValidValue(_currentRoad + 1), TrafficLightsColor.YellowRed);
                    ApplyColorChanges();
                    _yellowLight = true;
                    _currentTime = realtimeSinceStartup;
                }
            }
            else
            {
                if (realtimeSinceStartup - _currentTime > _trafficLightsIntersectionData.YellowLightTime)
                {
                    if (_carsInIntersection.Count == 0 || _trafficLightsIntersectionData.ExitWaypoints.Length == 0)
                    {
                        ChangeCurrentRoadColors(_currentRoad, TrafficLightsColor.Red);
                        _currentRoad++;
                        _currentRoad = GetValidValue(_currentRoad);
                        ChangeCurrentRoadColors(_currentRoad, TrafficLightsColor.Green);
                        _yellowLight = false;
                        _currentTime = realtimeSinceStartup;
                        ApplyColorChanges();
                    }
                }
            }
        }


        internal override List<int> GetStopWaypoints()
        {
            var result = new List<int>();
            for (int i = 0; i < _trafficLightsIntersectionData.StopWaypoints.Length; i++)
            {
                result.AddRange(_trafficLightsIntersectionData.StopWaypoints[i].roadWaypoints);
            }
            return result;
        }


        /// <summary>
        /// Used to set up custom behavior for traffic lights
        /// </summary>
        /// <param name="trafficLightsBehaviour"></param>
        internal void SetTrafficLightsBehaviour(TrafficLightsBehaviour trafficLightsBehaviour)
        {
            _trafficLightsBehaviour = trafficLightsBehaviour;
        }


        internal void SetGreenRoad(int roadIndex, bool doNotChangeAgain)
        {
            _stopUpdate = doNotChangeAgain;
            ChangeCurrentRoadColors(roadIndex, TrafficLightsColor.Green);
            ChangeAllRoadsExceptSelectd(roadIndex, TrafficLightsColor.Red);
            ApplyColorChanges();
        }


        /// <summary>
        /// After all intersection changes have been made this method apply them to the waypoint system and traffic lights 
        /// </summary>
        private void ApplyColorChanges()
        {
            for (int i = 0; i < _intersectionState.Length; i++)
            {
                //change waypoint color
                UpdateCurrentIntersectionWaypoints(i, _intersectionState[i] != TrafficLightsColor.Green);

                if (i < _trafficLightsIntersectionData.StopWaypoints.Length)
                {
                    //change traffic lights color
                    _trafficLightsBehaviour?.Invoke(_intersectionState[i], _trafficLightsIntersectionData.StopWaypoints[i].redLightObjects, _trafficLightsIntersectionData.StopWaypoints[i].yellowLightObjects, _trafficLightsIntersectionData.StopWaypoints[i].greenLightObjects, _trafficLightsIntersectionData.Name);
                }
            }
        }


        /// <summary>
        /// Trigger state changes for specified waypoints
        /// </summary>
        /// <param name="road"></param>
        /// <param name="stop"></param>
        private void UpdateCurrentIntersectionWaypoints(int road, bool stop)
        {
            if (_hasPedestrians && road >= _trafficLightsIntersectionData.StopWaypoints.Length)
            {
                TriggerPedestrianWaypointsUpdate(stop);
                return;
            }

            for (int j = 0; j < _trafficLightsIntersectionData.StopWaypoints[road].roadWaypoints.Length; j++)
            {
                WaypointEvents.TriggerTrafficLightChangedEvent(_trafficLightsIntersectionData.StopWaypoints[road].roadWaypoints[j], stop);
            }
        }


        /// <summary>
        /// Change color for specified road
        /// </summary>
        /// <param name="currentRoad"></param>
        /// <param name="newColor"></param>
        private void ChangeCurrentRoadColors(int currentRoad, TrafficLightsColor newColor)
        {
            if (currentRoad < _intersectionState.Length)
            {
                _intersectionState[currentRoad] = newColor;
            }
            else
            {
                Debug.LogError(currentRoad + "is grated than the max number of roads for intersection " + _trafficLightsIntersectionData.Name);
            }
        }


        /// <summary>
        /// Change color for all roads except the specified one
        /// </summary>
        /// <param name="currentRoad"></param>
        /// <param name="newColor"></param>
        private void ChangeAllRoadsExceptSelectd(int currentRoad, TrafficLightsColor newColor)
        {
            for (int i = 0; i < _intersectionState.Length; i++)
            {
                if (i != currentRoad)
                {
                    _intersectionState[i] = newColor;
                }
            }
        }


        /// <summary>
        /// Correctly increment the road number
        /// </summary>
        /// <param name="roadNumber"></param>
        /// <returns></returns>
        private int GetValidValue(int roadNumber)
        {
            if (roadNumber >= _nrOfRoads)
            {
                roadNumber = roadNumber % _nrOfRoads;
            }
            if (roadNumber < 0)
            {
                roadNumber = _nrOfRoads + roadNumber;
            }
            return roadNumber;
        }


        private void GetPedestrianRoads(IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler)
        {
            if (_trafficLightsIntersectionData.PedestrianWaypoints.Length > 0)
            {
                _hasPedestrians = true;
                _nrOfRoads += 1;
                pedestrianWaypointsDataHandler.SetIntersection(_trafficLightsIntersectionData.PedestrianWaypoints, this);
            }
        }


        private void SetPedestrianGreenLightTime()
        {
            if (_hasPedestrians)
            {
                _roadGreenLightTime[_roadGreenLightTime.Length - 1] = _trafficLightsIntersectionData.PedestrianGreenLightTime;
            }
        }


        private void TriggerPedestrianWaypointsUpdate(bool stop)
        {
#if GLEY_PEDESTRIAN_SYSTEM
            if (!PedestrianSystem.API.IsInitialized())
            {
                CoroutineManager.StartStaticCoroutine(WaitForInitialization(stop));
                return;
            }

            for (int i = 0; i < _trafficLightsIntersectionData.RedLightObjects.Length; i++)
            {
                if (_trafficLightsIntersectionData.RedLightObjects[i].activeSelf != stop)
                {
                    _trafficLightsIntersectionData.RedLightObjects[i].SetActive(stop);
                }
            }

            for (int i = 0; i < _trafficLightsIntersectionData.GreenLightObjects.Length; i++)
            {
                if (_trafficLightsIntersectionData.GreenLightObjects[i].activeSelf != !stop)
                {
                    _trafficLightsIntersectionData.GreenLightObjects[i].SetActive(!stop);
                }
            }

            for (int i = 0; i < _trafficLightsIntersectionData.PedestrianWaypoints.Length; i++)
            {
                PedestrianSystem.Events.TriggerStopStateChangedEvent(_trafficLightsIntersectionData.PedestrianWaypoints[i], stop);
            }
#endif
        }


#if GLEY_PEDESTRIAN_SYSTEM
        IEnumerator WaitForInitialization(bool stop)
        {
            if (!Gley.PedestrianSystem.Internal.PedestrianManager.Exists)
            {
                yield break;
            }
            while (!PedestrianSystem.API.IsInitialized())
            {
                yield return null;
            }
            TriggerPedestrianWaypointsUpdate(stop);
        }
#endif
    }
}
