using Gley.UrbanSystem.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Controls the traffic lights crossing intersection type.
    /// </summary>
    internal class TrafficLightsCrossing : GenericIntersection
    {
        private readonly TrafficLightsCrossingData _trafficLightsCrossingData;

        private TrafficLightsBehaviour _trafficLightsBehaviour;
        private TrafficLightsColor _intersectionState;
        private float _currentTime;
        private float _currentTimer;


        internal TrafficLightsCrossing(TrafficLightsCrossingData trafficLightsCrossingData, TrafficWaypointsDataHandler trafficWaypointsDataHandler, IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler, TrafficLightsBehaviour trafficLightsBehaviour)
        {
            _trafficLightsCrossingData = trafficLightsCrossingData;
            SetTrafficLightsBehaviour(trafficLightsBehaviour);
            GetPedestrianRoads(pedestrianWaypointsDataHandler);

            for (int i = 0; i < _trafficLightsCrossingData.ExitWaypoints.Length; i++)
            {
                trafficWaypointsDataHandler.SetIntersection(_trafficLightsCrossingData.ExitWaypoints[i], this, false, false, false, true);
            }

            for (int i = 0; i < _trafficLightsCrossingData.StopWaypoints.Length; i++)
            {
                for (int j = 0; j < _trafficLightsCrossingData.StopWaypoints[i].roadWaypoints.Length; j++)
                {
                    trafficWaypointsDataHandler.SetIntersection(_trafficLightsCrossingData.StopWaypoints[i].roadWaypoints[j], this, false, true, true, false);
                }
            }

            _carsInIntersection = new List<int>();
            _intersectionState = TrafficLightsColor.Green;
            ApplyColorChanges();
            _currentTime = Random.Range(0, 10);
        }


        public override bool IsPathFree(int waypointIndex)
        {
            return false;
        }


        public override void PedestrianPassed(int pedestrianIndex)
        {

        }


        internal void SetTrafficLightsBehaviour(TrafficLightsBehaviour trafficLightsBehaviour)
        {
            _trafficLightsBehaviour = trafficLightsBehaviour;
        }


        /// <summary>
        /// Change traffic lights color
        /// </summary>
        internal override void UpdateIntersection(float realtimeSinceStartup)
        {
            _currentTimer = realtimeSinceStartup - _currentTime;
            switch (_intersectionState)
            {
                case TrafficLightsColor.Green:
                    if (_currentTimer > _trafficLightsCrossingData.GreenLightTime)
                    {
                        _intersectionState = TrafficLightsColor.YellowGreen;
                        ApplyColorChanges();
                        _currentTime = realtimeSinceStartup;
                    }
                    break;

                case TrafficLightsColor.YellowGreen:
                    if (_currentTimer > _trafficLightsCrossingData.YellowLightTime)
                    {
                        _intersectionState = TrafficLightsColor.Red;
                        ApplyColorChanges();
                        _currentTime = realtimeSinceStartup;
                    }
                    break;

                case TrafficLightsColor.Red:
                    if (_currentTimer > _trafficLightsCrossingData.RedLightTime)
                    {
                        _intersectionState = TrafficLightsColor.YellowRed;
                        ApplyColorChanges();
                        _currentTime = realtimeSinceStartup;
                    }
                    break;

                case TrafficLightsColor.YellowRed:
                    if (_currentTimer > _trafficLightsCrossingData.YellowLightTime)
                    {
                        _intersectionState = TrafficLightsColor.Green;
                        ApplyColorChanges();
                        _currentTime = realtimeSinceStartup;
                    }
                    break;
            }
        }


        internal override string GetName()
        {
            return _trafficLightsCrossingData.Name;
        }


        /// <summary>
        /// Used for editor applications
        /// </summary>
        /// <returns></returns>
        internal LightsStopWaypoints[] GetWaypoints()
        {
            return _trafficLightsCrossingData.StopWaypoints;
        }


        internal override List<int> GetStopWaypoints()
        {
            var result = new List<int>();
            for (int i = 0; i < _trafficLightsCrossingData.StopWaypoints.Length; i++)
            {
                result.AddRange(_trafficLightsCrossingData.StopWaypoints[i].roadWaypoints);
            }
            return result;
        }


        internal TrafficLightsColor GetCrossingState()
        {
            return _intersectionState;
        }


        internal override int[] GetPedStopWaypoint()
        {
            return _trafficLightsCrossingData.PedestrianWaypoints;
        }


        /// <summary>
        /// After all intersection changes have been made this method apply them to the waypoint system and traffic lights 
        /// </summary>
        private void ApplyColorChanges()
        {
            //change waypoint color
            UpdateCurrentIntersectionWaypoints(0, _intersectionState != TrafficLightsColor.Green);
            TriggerPedestrianWaypointsUpdate(_intersectionState != TrafficLightsColor.Red);
            _trafficLightsBehaviour?.Invoke(_intersectionState, _trafficLightsCrossingData.StopWaypoints[0].redLightObjects, _trafficLightsCrossingData.StopWaypoints[0].yellowLightObjects, _trafficLightsCrossingData.StopWaypoints[0].greenLightObjects, _trafficLightsCrossingData.Name);
        }


        /// <summary>
        /// Trigger state changes for specified waypoints
        /// </summary>
        /// <param name="road"></param>
        /// <param name="stop"></param>
        private void UpdateCurrentIntersectionWaypoints(int road, bool stop)
        {
            for (int j = 0; j < _trafficLightsCrossingData.StopWaypoints[road].roadWaypoints.Length; j++)
            {
                WaypointEvents.TriggerTrafficLightChangedEvent(_trafficLightsCrossingData.StopWaypoints[road].roadWaypoints[j], stop);
            }
        }


        private void GetPedestrianRoads(IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler)
        {
            if (_trafficLightsCrossingData.PedestrianWaypoints.Length > 0)
            {
                pedestrianWaypointsDataHandler.SetIntersection(_trafficLightsCrossingData.PedestrianWaypoints, this);
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

            for (int i = 0; i < _trafficLightsCrossingData.RedLightObjects.Length; i++)
            {
                if (_trafficLightsCrossingData.RedLightObjects[i].activeSelf != stop)
                {
                    _trafficLightsCrossingData.RedLightObjects[i].SetActive(stop);
                }
            }

            for (int i = 0; i < _trafficLightsCrossingData.GreenLightObjects.Length; i++)
            {
                if (_trafficLightsCrossingData.GreenLightObjects[i].activeSelf != !stop)
                {
                    _trafficLightsCrossingData.GreenLightObjects[i].SetActive(!stop);
                }
            }

            for (int i = 0; i < _trafficLightsCrossingData.PedestrianWaypoints.Length; i++)
            {
                PedestrianSystem.Events.TriggerStopStateChangedEvent(_trafficLightsCrossingData.PedestrianWaypoints[i], stop);
            }
#endif
        }


#if GLEY_PEDESTRIAN_SYSTEM
        IEnumerator WaitForInitialization(bool stop)
        {
            if(!Gley.PedestrianSystem.Internal.PedestrianManager.Exists)
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
