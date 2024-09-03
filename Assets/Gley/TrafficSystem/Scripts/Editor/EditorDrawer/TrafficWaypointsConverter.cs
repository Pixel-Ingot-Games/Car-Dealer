using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    /// <summary>
    /// Convert editor waypoints to play mode waypoints.
    /// </summary>
    public class TrafficWaypointsConverter
    {
        private readonly TrafficWaypointEditorData _trafficWaypointEditorData;
        private readonly IntersectionEditorData _intersectionEditorData;

        public TrafficWaypointsConverter()
        {
            _trafficWaypointEditorData = new TrafficWaypointEditorData();
            _intersectionEditorData = new IntersectionEditorData();
        }

        public void ConvertWaypoints()
        {
            VerifyTrafficWaypoints();
            SetWaypointDistance();
            SetIntersectionProperties();
            ConvertTrafficWaypoints();
            AssignTrafficWaypointsToCell();
            AssignZipperGiveWay();
            GeneratePathfindingWaypoints();
        }

        private void VerifyTrafficWaypoints()
        {
            WaypointSettings[] allTrafficEditorWaypoints = _trafficWaypointEditorData.GetAllWaypoints();

            if (allTrafficEditorWaypoints.Length <= 0)
            {
                Debug.LogWarning("No waypoints found. Go to Tools->Gley->Traffic System->Road Setup and create a road");
                return;
            }
            for (int i = 0; i < allTrafficEditorWaypoints.Length; i++)
            {
                allTrafficEditorWaypoints[i].VerifyAssignments(true);
                allTrafficEditorWaypoints[i].ResetProperties();
            }
        }

        private void SetWaypointDistance()
        {
            var allTrafficEditorWaypoints = _trafficWaypointEditorData.GetAllWaypoints();
            for (int i = 0; i < allTrafficEditorWaypoints.Length; i++)
            {
                allTrafficEditorWaypoints[i].distance = new List<int>();
                for (int j = 0; j < allTrafficEditorWaypoints[i].neighbors.Count; j++)
                {
                    allTrafficEditorWaypoints[i].distance.Add((int)Vector3.Distance(allTrafficEditorWaypoints[i].transform.position, allTrafficEditorWaypoints[i].neighbors[j].transform.position));
                }
            }
        }

        private void SetIntersectionProperties()
        {
            var allEditorIntersections = _intersectionEditorData.GetAllIntersections();
            for (int i = 0; i < allEditorIntersections.Length; i++)
            {
                if (!allEditorIntersections[i].VerifyAssignments())
                    return;

                List<IntersectionStopWaypointsSettings> intersectionWaypoints = allEditorIntersections[i].GetAssignedWaypoints();

                for (int j = 0; j < intersectionWaypoints.Count; j++)
                {
                    for (int k = 0; k < intersectionWaypoints[j].roadWaypoints.Count; k++)
                    {
                        intersectionWaypoints[j].roadWaypoints[k].enter = true;
                    }
                }

                List<WaypointSettings> exitWaypoints = allEditorIntersections[i].GetExitWaypoints();

                for (int j = 0; j < exitWaypoints.Count; j++)
                {
                    exitWaypoints[j].exit = true;
                }
            }
        }


        private void AssignZipperGiveWay()
        {
            if (MonoBehaviourUtilities.TryGetSceneScript<TrafficWaypointsData>(out var result))
            {
                result.Value.AssignZipperGiveWay();
            }
            else
            {
                Debug.LogError(result.Error);
            }
        }

        private void AssignTrafficWaypointsToCell()
        {
            WaypointSettings[] allWaypoints = _trafficWaypointEditorData.GetAllWaypoints();

            GridDataHandler gridDatahandler;
            if (MonoBehaviourUtilities.TryGetSceneScript<GridData>(out var result))
            {
                gridDatahandler = new GridDataHandler(result.Value);
            }
            else
            {
                Debug.LogError(result.Error);
                return;
            }


            WaypointSettings[] giveWayList = GetWaypointsIncludedInGiveWayList(allWaypoints);

            for (int i = allWaypoints.Length - 1; i >= 0; i--)
            {
                if (allWaypoints[i].allowedCars.Count != 0)
                {
                    var cell = gridDatahandler.GetCell(allWaypoints[i].transform.position);
                    gridDatahandler.AddTrafficWaypoint(cell, i);

                    // Waypoints hat are not allowed to spawn on 
                    if (!allWaypoints[i].name.Contains(UrbanSystemConstants.Connect) &&
                        !allWaypoints[i].name.Contains(UrbanSystemConstants.OutWaypointEnding) &&
                        allWaypoints[i].enter == false &&
                        allWaypoints[i].exit == false &&
                        allWaypoints[i].giveWay == false &&
                        !giveWayList.Contains(allWaypoints[i])
                        )
                    {
                        gridDatahandler.AddTrafficSpawnWaypoint(cell, i, allWaypoints[i].allowedCars.Cast<int>().ToArray(), allWaypoints[i].priority);
                    }
                }
            }
        }

        private WaypointSettings[] GetWaypointsIncludedInGiveWayList(WaypointSettings[] allWaypoints)
        {
            List<WaypointSettings> result = new List<WaypointSettings>();
            for (int i = 0; i < allWaypoints.Length; i++)
            {
                result.AddRange(allWaypoints[i].giveWayList);
            }
            return result.Distinct().ToArray();
        }

        private void ConvertTrafficWaypoints()
        {
            WaypointSettings[] allTrafficEditorWaypoints = _trafficWaypointEditorData.GetAllWaypoints();

            // Assign waypoints to MonoBehaviour script.
            MonoBehaviourUtilities.GetOrCreateObjectScript<TrafficWaypointsData>(TrafficSystemConstants.PlayHolder, false).SetTrafficWaypoints(allTrafficEditorWaypoints.ToPlayWaypoints(allTrafficEditorWaypoints));
        }


        private void GeneratePathfindingWaypoints()
        {
            bool pathfindingEnabled = new SettingsLoader(TrafficSystemConstants.windowSettingsPath).LoadSettingsAsset<TrafficSettingsWindowData>().PathFindingEnabled;
            var modules = MonoBehaviourUtilities.GetOrCreateObjectScript<TrafficModules>(TrafficSystemConstants.PlayHolder, false);

            if (pathfindingEnabled)
            {
                var allTrafficEditorWaypoints = _trafficWaypointEditorData.GetAllWaypoints();
                var trafficPathFindingCreator = new TrafficPathFindingCreator();
                trafficPathFindingCreator.GenerateWaypoints(allTrafficEditorWaypoints);
                modules.SetModules(true);
            }
            else
            {
                modules.SetModules(false);
                if (MonoBehaviourUtilities.TryGetObjectScript<PathFindingData>(TrafficSystemConstants.PlayHolder, out var result))
                {
                    GleyPrefabUtilities.DestroyImmediate(result.Value);
                }
            }
        }
    }
}