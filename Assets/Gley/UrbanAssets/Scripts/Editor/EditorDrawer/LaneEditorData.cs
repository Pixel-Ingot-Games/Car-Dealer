using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal struct LaneHolder<T> where T : WaypointSettingsBase
    {
        internal string Name { get; private set; }
        internal T[] Waypoints { get; private set; }
        internal bool IsClosed { get; private set; }

        internal LaneHolder(string name, T[] waypoints, bool isClosed)
        {
            Name = name;
            Waypoints = waypoints;
            IsClosed = isClosed;
        }
    }


    internal class LaneEditorData<T, R> : EditorData where T : RoadBase where R : WaypointSettingsBase
    {
        private RoadEditorData<T> _roadData;
        private Dictionary<T, LaneHolder<R>[]> _allLanes;


        protected LaneEditorData(RoadEditorData<T> roadData)
        {
            _roadData = roadData;
            LoadAllData();
        }


        internal LaneHolder<R>[] GetRoadLanes(T road)
        {
            _allLanes.TryGetValue(road, out var lanes);
            return lanes;
        }


        protected override void LoadAllData()
        {
            _allLanes = new Dictionary<T, LaneHolder<R>[]>();
            var allRoads = _roadData.GetAllRoads();
            for (int i = 0; i < allRoads.Length; i++)
            {
                LaneHolder<R>[] lanes = LoadLanes(allRoads[i]);
                if (lanes != null)
                {
                    _allLanes.Add(allRoads[i], lanes);
                }
            }
        }


        private LaneHolder<R>[] LoadLanes(T road)
        {
            Transform lanesHolder = road.transform.Find(UrbanSystem.Internal.UrbanSystemConstants.LanesHolderName);
            if (lanesHolder == null || lanesHolder.childCount == 0)
            {
                if (!road.justCreated)
                {
                    Debug.LogWarning($"{road.name} has no lanes. Go to Edit {typeof(T).Name} Window and press Generate Waypoints", road);
                }
                return null;
            }

            var laneList = new List<LaneHolder<R>>();
            for (int i = 0; i < lanesHolder.childCount; i++)
            {
                int nrOfWaypoints = lanesHolder.GetChild(i).childCount;
                if (nrOfWaypoints == 0)
                {
                    Debug.LogWarning($"Lane {lanesHolder.GetChild(i)} from road {road.name} has no Waypoints. Go to Edit {typeof(T).Name} Window and press Generate Waypoints", road);
                    return null;
                }
                Transform lane = lanesHolder.GetChild(i);
                var laneWaypoints = new List<R>();
                for (int j = 0; j < nrOfWaypoints; j++)
                {
                    R waypointScript = lane.GetChild(j).GetComponent<R>();
                    if (waypointScript != null)
                    {
                        waypointScript.VerifyAssignments(false);
                        waypointScript.position = waypointScript.transform.position;
                        laneWaypoints.Add(waypointScript);
                    }
                }
                if (laneWaypoints[laneWaypoints.Count - 1].neighbors.Contains(laneWaypoints[0]))
                {
                    laneList.Add(new LaneHolder<R>(lane.name, laneWaypoints.ToArray(), true));
                }
                else
                {
                    laneList.Add(new LaneHolder<R>(lane.name, laneWaypoints.ToArray(), false));
                }
            }
            return laneList.ToArray();
        }
    }
}