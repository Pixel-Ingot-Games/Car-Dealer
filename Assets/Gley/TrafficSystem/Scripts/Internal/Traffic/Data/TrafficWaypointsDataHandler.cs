using Gley.UrbanSystem.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Access play mode waypoints data.
    /// </summary>
    internal class TrafficWaypointsDataHandler
    {
        private readonly TrafficWaypointsData _trafficWaypointsData;


        internal TrafficWaypointsDataHandler(TrafficWaypointsData data)
        {
            _trafficWaypointsData = data;
        }


        #region Set
        internal void SetTemperaryDisabledValue(int waypointIndex, bool value)
        {
            GetWaypoint(waypointIndex).TemporaryDisabled = value;
        }


        internal void SetTemperaryDisabledValue(List<int> waypointIndexes, bool value)
        {
            for (int i = 0; i < waypointIndexes.Count; i++)
            {
                SetTemperaryDisabledValue(waypointIndexes[i], value);
            }
        }


        internal void SetIntersection(int waypointIndex, IIntersection intersection, bool giveWay, bool stop, bool enter, bool exit)
        {
            GetWaypoint(waypointIndex).SetIntersection(intersection, giveWay, stop, enter, exit);
        }


        internal void SetStopValue(int waypointIndex, bool value)
        {
            GetWaypoint(waypointIndex).Stop = value;
        }


        private void SetTriggerEventValue(int waypointIndex, bool value)
        {
            GetWaypoint(waypointIndex).TriggerEvent = value;
        }


        private void SetGiveWayValue(int waypointIndex, bool value)
        {
            GetWaypoint(waypointIndex).GiveWay = value;
        }


        internal void SetEventData(int waypointIndex, string data)
        {
            if (data != null)
            {
                SetTriggerEventValue(waypointIndex, true);
            }
            else
            {
                SetTriggerEventValue(waypointIndex, false);
            }
            GetWaypoint(waypointIndex).EventData = data;
        }
        #endregion


        #region Get
        internal TrafficWaypoint GetWaypointFromIndex(int waypointIndex)
        {
            if (IsWaypointIndexValid(waypointIndex))
            {
                return GetWaypoint(waypointIndex);
            }
            return null;
        }


        internal VehicleTypes[] GetAllowedVehicles(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).AllowedVehicles;
        }


        internal int[] GetNeighbors(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Neighbors;
        }


        internal int[] GetOtherLanes(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).OtherLanes;
        }


        internal int[] GetPrevs(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Prev;
        }


        internal int[] GetGiveWayWaypointList(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).GiveWayList;
        }


        internal List<int> GetNeighborsWithConditions(int waypointIndex, VehicleTypes vehicleType)
        {
            List<int> result = new List<int>();
            var allNeighbors = GetNeighbors(waypointIndex);
            for (int i = 0; i < allNeighbors.Length; i++)
            {
                if (GetAllowedVehicles(allNeighbors[i]).Contains(vehicleType) && !IsTemporaryDisabled(allNeighbors[i]))
                {
                    result.Add(allNeighbors[i]);
                }
            }
            return result;
        }


        internal List<int> GetNeighborsWithConditions(int waypointIndex)
        {
            List<int> result = new List<int>();
            var allNeighbors = GetNeighbors(waypointIndex);
            for (int i = 0; i < allNeighbors.Length; i++)
            {
                if (!IsTemporaryDisabled(allNeighbors[i]))
                {
                    result.Add(allNeighbors[i]);
                }
            }
            return result;
        }


        internal List<int> GetOtherLanesWithConditions(int waypointIndex, VehicleTypes vehicleType)
        {
            List<int> result = new List<int>();
            var allNeighbors = GetOtherLanes(waypointIndex);
            for (int i = 0; i < allNeighbors.Length; i++)
            {
                if (GetAllowedVehicles(allNeighbors[i]).Contains(vehicleType) && !IsTemporaryDisabled(allNeighbors[i]))
                {
                    result.Add(allNeighbors[i]);
                }
            }
            return result;
        }


        internal List<int> GetOtherLanesWithConditions(int waypointIndex)
        {
            List<int> result = new List<int>();
            var allNeighbors = GetOtherLanes(waypointIndex);
            for (int i = 0; i < allNeighbors.Length; i++)
            {
                if (!IsTemporaryDisabled(allNeighbors[i]))
                {
                    result.Add(allNeighbors[i]);
                }
            }
            return result;
        }


        internal Vector3 GetPosition(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Position;
        }


        internal string GetName(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Name;
        }


        internal float GetLaneWidth(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).LaneWidth;
        }


        internal string GetEventData(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).EventData;
        }


        internal float GetMaxSpeed(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).MaxSpeed;
        }


        internal bool HasNeighbors(int waypointIndex)
        {
            return GetNeighbors(waypointIndex).Length > 0;
        }


        internal bool HasPrevs(int waypointIndex)
        {
            return GetPrevs(waypointIndex).Length > 0;
        }


        internal List<IIntersection> GetAssociatedIntersections(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).AssociatedIntersections;
        }


        internal bool HasNeighborsForVehicleType(int waypointIndex, VehicleTypes vehicleType)
        {
            return GetNeighborsWithConditions(waypointIndex, vehicleType).Count > 0;
        }


        internal bool HasWaypointInNeighbors(int waypointIndex, int waypointToCheck)
        {
            return GetNeighbors(waypointIndex).Contains(waypointToCheck);
        }


        internal bool HasOtherLanes(int waypointIndex)
        {
            return GetOtherLanes(waypointIndex).Length > 0;
        }


        internal bool IsTemporaryDisabled(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).TemporaryDisabled;
        }


        internal bool IsInIntersection(int waypointIndex)
        {
            return GetAssociatedIntersections(waypointIndex) != null;
        }


        internal bool IsComplexGiveWay(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).ComplexGiveWay;
        }


        internal bool IsZipperGiveWay(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).ZipperGiveWay;
        }


        internal bool IsStop(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Stop;
        }


        internal bool IsGiveWay(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).GiveWay;
        }


        internal bool IsExit(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Exit;
        }


        internal bool IsEnter(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).Enter;
        }


        internal bool IsTriggerEvent(int waypointIndex)
        {
            return GetWaypoint(waypointIndex).TriggerEvent;
        }
        #endregion


        private bool IsWaypointIndexValid(int waypointIndex)
        {
            if (waypointIndex < 0)
            {
                Debug.LogError($"Waypoint index {waypointIndex} should be >= 0");
                return false;
            }

            if (waypointIndex >= _trafficWaypointsData.AllTrafficWaypoints.Length)
            {
                Debug.LogError($"Waypoint index {waypointIndex} should be < {_trafficWaypointsData.AllTrafficWaypoints.Length}");
                return false;
            }

            if (_trafficWaypointsData.AllTrafficWaypoints[waypointIndex] == null)
            {
                Debug.LogError($"Waypoint at {waypointIndex} is null, Verify the setup");
                return false;
            }

            return true;
        }


        private TrafficWaypoint GetWaypoint(int waypointIndex)
        {
            return _trafficWaypointsData.AllTrafficWaypoints[waypointIndex];
        }
    }
}
