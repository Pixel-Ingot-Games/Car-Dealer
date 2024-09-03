using Gley.UrbanSystem.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores waypoint properties
    /// </summary>
    public class WaypointSettings : WaypointSettingsBase
    {
        public List<WaypointSettings> otherLanes;
        public List<WaypointSettings> giveWayList;
        public List<VehicleTypes> allowedCars;
        public float laneWidth;
        public int maxSpeed;

        public bool giveWay;
        public bool enter;
        public bool exit;
        public bool speedLocked;
        public bool carsLocked;
        public bool complexGiveWay;
        public bool zipperGiveWay;

        public override void Initialize()
        {
            base.Initialize();
            otherLanes = new List<WaypointSettings>();
            giveWayList = new List<WaypointSettings>();
        }

        public void ResetProperties()
        {
            enter = false;
            exit = false;
        }

        public override void VerifyAssignments(bool showPrevsWarning)
        {
            base.VerifyAssignments(showPrevsWarning);

            if (showPrevsWarning)
            {
                if (prev.Count == 0)
                {
                    Debug.LogWarning(UrbanSystemErrors.NoPrevs(name), gameObject);
                }
            }

            if (otherLanes == null)
            {
                otherLanes = new List<WaypointSettings>();
            }

            for (int j = otherLanes.Count - 1; j >= 0; j--)
            {
                if (otherLanes[j] == null)
                {
                    otherLanes.RemoveAt(j);
                }
            }

            if (giveWayList == null)
            {
                giveWayList = new List<WaypointSettings>();
            }

            for (int j = giveWayList.Count - 1; j >= 0; j--)
            {
                if (giveWayList[j] == null)
                {
                    giveWayList.RemoveAt(j);
                }
            }

            if (allowedCars == null)
            {
                allowedCars = new List<VehicleTypes>();
            }

            for (int i = allowedCars.Count - 1; i >= 0; i--)
            {
                if (!IsValid((int)allowedCars[i]))
                {
                    allowedCars.RemoveAt(i);
                }
            }

            if (laneWidth == 0)
            {
                if (name.Contains(Gley.UrbanSystem.Internal.UrbanSystemConstants.Connect))
                {
                    laneWidth = 4;
                }
                else
                {
                    laneWidth = transform.parent.parent.parent.GetComponent<Road>().laneWidth;
                }
            }
        }


        public void SetVehicleTypesForAllNeighbors(List<VehicleTypes> allowedVehicles)
        {
            Queue<WaypointSettings> queue = new Queue<WaypointSettings>();
            HashSet<WaypointSettingsBase> visited = new HashSet<WaypointSettingsBase>();

            // Start with the current waypoint
            queue.Enqueue(this);
            visited.Add(this);
            carsLocked = false;

            while (queue.Count > 0)
            {
                WaypointSettings current = queue.Dequeue();
                if (!current.carsLocked)
                {
                    current.allowedCars = allowedVehicles;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(current);
#endif
                    // Enqueue all unvisited neighbors
                    foreach (WaypointSettingsBase neighbor in current.neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue((WaypointSettings)neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }
            if (allowedVehicles.Count > 0)
            {
                carsLocked = true;
            }
            Debug.Log("Done");
        }


        public void SetSpeedForAllNeighbors(int newSpeed)
        {
            Queue<WaypointSettings> queue = new Queue<WaypointSettings>();
            HashSet<WaypointSettingsBase> visited = new HashSet<WaypointSettingsBase>();

            // Start with the current waypoint
            queue.Enqueue(this);
            visited.Add(this);
            speedLocked = false;

            while (queue.Count > 0)
            {
                WaypointSettings current = queue.Dequeue();
                if (!current.speedLocked)
                {
                    current.maxSpeed = newSpeed;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(current);
#endif
                    // Enqueue all unvisited neighbors
                    foreach (WaypointSettingsBase neighbor in current.neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue((WaypointSettings)neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }
            if (newSpeed != 0)
            {
                speedLocked = true;
            }
            Debug.Log("Done");
        }


        private bool IsValid(int value)
        {
            return Enum.IsDefined(typeof(VehicleTypes), value);
        }
    }
}