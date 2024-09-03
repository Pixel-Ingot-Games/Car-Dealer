#if UNITY_EDITOR
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Converts waypoints from editor version to runtime version
    /// </summary>
    public static class WaypointExtensionMethods
    {
        public static List<int> ToListIndex(this WaypointSettings[] editorWaypoints, WaypointSettings[] allWaypoints)
        {
            var result = new List<int>();

            for (int i = 0; i < editorWaypoints.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < allWaypoints.Length; j++)
                {
                    if (editorWaypoints[i] == allWaypoints[j])
                    {
                        found = true;
                        result.Add(j);
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"{editorWaypoints[i].name} not found in allWaypoints", editorWaypoints[i]);
                }

            }
            return result;
        }


        public static int[] ToListIndex(this List<WaypointSettingsBase> editorWaypoints, WaypointSettings[] allWaypoints)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < editorWaypoints.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < allWaypoints.Length; j++)
                {
                    if (editorWaypoints[i] == allWaypoints[j])
                    {
                        found = true;
                        result.Add(j);
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"{editorWaypoints[i].name} not found in allWaypoints", editorWaypoints[i]);
                }

            }
            return result.ToArray();
        }


        public static int[] ToListIndex(this List<WaypointSettings> editorWaypoints, WaypointSettings[] allWaypoints)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < editorWaypoints.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < allWaypoints.Length; j++)
                {
                    if (editorWaypoints[i] == allWaypoints[j])
                    {
                        found = true;
                        result.Add(j);
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"{editorWaypoints[i].name} not found in allWaypoints", editorWaypoints[i]);
                }

            }
            return result.ToArray();
        }


        public static int ToListIndex(this WaypointSettings editorWaypoint, WaypointSettings[] allWaypoints)
        {
            for (int i = 0; i < allWaypoints.Length; i++)
            {
                if (editorWaypoint == allWaypoints[i])
                {
                    return i;
                }
            }
            return -1;
        }


        public static TrafficWaypoint[] ToPlayWaypoints(this WaypointSettings[] editorWaypoints, WaypointSettings[] allWaypoints)
        {
            TrafficWaypoint[] result = new TrafficWaypoint[editorWaypoints.Length];
            if (editorWaypoints != null)
            {
                for (int i = 0; i < editorWaypoints.Length; i++)
                {
                    result[i] = editorWaypoints[i].ToPlayWaypoint(allWaypoints);
                }
            }
            return result;
        }

        public static TrafficWaypoint ToPlayWaypoint(this WaypointSettings editorWaypoint, WaypointSettings[] allWaypoints)
        {
            return new TrafficWaypoint(editorWaypoint.name,
                editorWaypoint.ToListIndex(allWaypoints),
                editorWaypoint.transform.position,
                editorWaypoint.allowedCars,
                editorWaypoint.neighbors.ToListIndex(allWaypoints),
                editorWaypoint.prev.ToListIndex(allWaypoints),
                editorWaypoint.otherLanes.ToListIndex(allWaypoints),
                editorWaypoint.maxSpeed,
                editorWaypoint.giveWay,
                editorWaypoint.complexGiveWay,
                editorWaypoint.zipperGiveWay,
                editorWaypoint.triggerEvent,
                editorWaypoint.laneWidth,
                editorWaypoint.eventData,
                editorWaypoint.giveWayList.ToListIndex(allWaypoints));
        }


        public static LightsStopWaypoints[] ToLightsStopWaypointsArray(this List<IntersectionStopWaypointsSettings> giveWayWaypoints, WaypointSettings[] allWaypoints)
        {
            List<LightsStopWaypoints> result = new List<LightsStopWaypoints>();
            for (int i = 0; i < giveWayWaypoints.Count; i++)
            {
                result.Add(new LightsStopWaypoints(giveWayWaypoints[i].roadWaypoints.ToListIndex(allWaypoints), giveWayWaypoints[i].redLightObjects.ToArray(), giveWayWaypoints[i].yellowLightObjects.ToArray(), giveWayWaypoints[i].greenLightObjects.ToArray(), giveWayWaypoints[i].greenLightTime));
            }
            return result.ToArray();
        }


        public static PriorityStopWaypoints[] ToPriorityStopWaypointsArray(this List<IntersectionStopWaypointsSettings> giveWayWaypoints, WaypointSettings[] allWaypoints)
        {
            List<PriorityStopWaypoints> result = new List<PriorityStopWaypoints>();
            for (int i = 0; i < giveWayWaypoints.Count; i++)
            {
                result.Add(new PriorityStopWaypoints(giveWayWaypoints[i].roadWaypoints.ToListIndex(allWaypoints), giveWayWaypoints[i].greenLightTime));
            }
            return result.ToArray();
        }
    }
}
#endif
