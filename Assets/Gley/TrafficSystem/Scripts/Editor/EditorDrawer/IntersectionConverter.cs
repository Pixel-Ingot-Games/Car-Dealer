using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEngine;
#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Internal;
using Gley.PedestrianSystem.Editor;
#endif

namespace Gley.TrafficSystem.Editor
{
    /// <summary>
    /// Convert editor intersections to play mode intersections.
    /// </summary>
    public class IntersectionConverter
    {
        private IntersectionEditorData _intersectionData;
        private TrafficWaypointEditorData _waypointData;

        public IntersectionConverter()
        {
            _intersectionData = new IntersectionEditorData();
            _waypointData = new TrafficWaypointEditorData();
        }

        public void ConvertAllIntersections()
        {
            ConvertIntersections();
            AssignIntersectionsToCell();
            AddPedestrianWaypoints();
        }

        private void ConvertIntersections()
        {
            var allEditorIntersections = _intersectionData.GetAllIntersections();
            var _allEditorWaypoints = _waypointData.GetAllWaypoints();

            List<PriorityIntersectionData> priorityIntersections = new List<PriorityIntersectionData>();
            List<TrafficLightsIntersectionData> lightsIntersections = new List<TrafficLightsIntersectionData>();
            List<TrafficLightsCrossingData> trafficLightsCrossings = new List<TrafficLightsCrossingData>();
            List<PriorityCrossingData> priorityCrossings = new List<PriorityCrossingData>();
            var AllIntersections = new Internal.IntersectionDataType[allEditorIntersections.Length];

            for (int i = 0; i < allEditorIntersections.Length; i++)
            {
                if (allEditorIntersections[i].GetType().Equals(typeof(TrafficLightsIntersectionSettings)))
                {
                    TrafficLightsIntersectionData intersection = ((TrafficLightsIntersectionSettings)allEditorIntersections[i]).ToPlayModeIntersection(_allEditorWaypoints);
                    lightsIntersections.Add(intersection);
                    AllIntersections[i] = new Internal.IntersectionDataType(IntersectionType.TrafficLights, lightsIntersections.Count - 1, intersection.Name);
                }

                if (allEditorIntersections[i].GetType().Equals(typeof(TrafficLightsCrossingSettings)))
                {
                    TrafficLightsCrossingData intersection = ((TrafficLightsCrossingSettings)allEditorIntersections[i]).ToPlayModeIntersection(_allEditorWaypoints);
                    trafficLightsCrossings.Add(intersection);
                    AllIntersections[i] = new Internal.IntersectionDataType(IntersectionType.LightsCrossing, trafficLightsCrossings.Count - 1, intersection.Name);
                }


                if (allEditorIntersections[i].GetType().Equals(typeof(PriorityIntersectionSettings)))
                {
                    PriorityIntersectionData intersection = ((PriorityIntersectionSettings)allEditorIntersections[i]).ToPlayModeIntersection(_allEditorWaypoints);
                    priorityIntersections.Add(intersection);
                    AllIntersections[i] = new Internal.IntersectionDataType(IntersectionType.Priority, priorityIntersections.Count - 1, intersection.Name);
                }

                if (allEditorIntersections[i].GetType().Equals(typeof(PriorityCrossingSettings)))
                {
                    PriorityCrossingData intersection = ((PriorityCrossingSettings)allEditorIntersections[i]).ToPlayModeIntersection(_allEditorWaypoints);
                    priorityCrossings.Add(intersection);
                    AllIntersections[i] = new Internal.IntersectionDataType(IntersectionType.PriorityCrossing, priorityCrossings.Count - 1, intersection.Name);
                }
            }

            var trafficIntersectionsData = MonoBehaviourUtilities.GetOrCreateObjectScript<IntersectionsData>(TrafficSystemConstants.PlayHolder, false);
            trafficIntersectionsData.SetTrafficIntersectionData(
                AllIntersections,
                lightsIntersections.ToArray(),
                priorityIntersections.ToArray(),
                trafficLightsCrossings.ToArray(),
                priorityCrossings.ToArray());
        }

        private void AssignIntersectionsToCell()
        {
            GridDataHandler gridDataHandler;
            if (MonoBehaviourUtilities.TryGetSceneScript<GridData>(out var gridData))
            {
                gridDataHandler = new GridDataHandler(gridData.Value);
            }
            else
            {
                Debug.LogError(gridData.Error);
                return;
            }

            var allEditorIntersections = _intersectionData.GetAllIntersections();

            for (int i = 0; i < allEditorIntersections.Length; i++)
            {
                List<IntersectionStopWaypointsSettings> intersectionWaypoints = allEditorIntersections[i].GetAssignedWaypoints();
                for (int j = 0; j < intersectionWaypoints.Count; j++)
                {
                    for (int k = 0; k < intersectionWaypoints[j].roadWaypoints.Count; k++)
                    {
                        var cellData = gridDataHandler.GetCell(intersectionWaypoints[j].roadWaypoints[k].transform.position);
                        gridDataHandler.AddIntersection(cellData, i);
                    }
                }
            }
        }

        private void AddPedestrianWaypoints()
        {
#if GLEY_PEDESTRIAN_SYSTEM
            IntersectionsDataHandler trafficIntersectionsDatahandler;
            if (MonoBehaviourUtilities.TryGetSceneScript<IntersectionsData>(out var trafficIntersectionData))
            {
                trafficIntersectionsDatahandler = new IntersectionsDataHandler(trafficIntersectionData.Value);
            }
            else
            {
                Debug.LogError(trafficIntersectionData.Error);
                return;
            }
            var allIntersections = trafficIntersectionsDatahandler.GetIntersectionData();
            var allEditorIntersections = _intersectionData.GetAllIntersections();
            var _allPedestrianEditorWaypoints = new PedestrianWaypointEditorData().GetAllWaypoints();

            for (int i = 0; i < allIntersections.Length; i++)
            {
                switch (allIntersections[i].Type)
                {
                    case IntersectionType.Priority:
                        PriorityIntersectionData priorityInersection = trafficIntersectionsDatahandler.GetPriorityIntersectionData(allIntersections[i].OtherListIndex);
                        PriorityIntersectionSettings priorityIntersectionEditor = (PriorityIntersectionSettings)allEditorIntersections[i];
                        for (int j = 0; j < priorityIntersectionEditor.enterWaypoints.Count; j++)
                        {
                            priorityInersection.AddPedestrianWaypoints(j, priorityIntersectionEditor.enterWaypoints[j].pedestrianWaypoints.ToListIndex(_allPedestrianEditorWaypoints), priorityIntersectionEditor.enterWaypoints[j].directionWaypoints.ToListIndex(_allPedestrianEditorWaypoints));
                        }
                        break;
                    case IntersectionType.PriorityCrossing:
                        PriorityCrossingData priorityCrossing = trafficIntersectionsDatahandler.GetPriorityCrossingData(allIntersections[i].OtherListIndex);
                        PriorityCrossingSettings priorityCrossingEditor = (PriorityCrossingSettings)allEditorIntersections[i];
                        for (int j = 0; j < priorityCrossingEditor.enterWaypoints.Count; j++)
                        {
                            priorityCrossing.AddPedestrianWaypoints(j, priorityCrossingEditor.enterWaypoints[j].pedestrianWaypoints.ToListIndex(_allPedestrianEditorWaypoints), priorityCrossingEditor.enterWaypoints[j].directionWaypoints.ToListIndex(_allPedestrianEditorWaypoints));
                        }
                        break;
                    case IntersectionType.TrafficLights:
                        TrafficLightsIntersectionData trafficLightsIntersection = trafficIntersectionsDatahandler.GetTrafficLightsIntersectionData(allIntersections[i].OtherListIndex);
                        TrafficLightsIntersectionSettings trafficLightsIntersectionEditor = (TrafficLightsIntersectionSettings)allEditorIntersections[i];
                        trafficLightsIntersection.AddPedestrianWaypoints(trafficLightsIntersectionEditor.pedestrianWaypoints.ToListIndex(_allPedestrianEditorWaypoints), trafficLightsIntersectionEditor.directionWaypoints.ToListIndex(_allPedestrianEditorWaypoints), trafficLightsIntersectionEditor.pedestrianRedLightObjects.ToArray(), trafficLightsIntersectionEditor.pedestrianGreenLightObjects.ToArray(), trafficLightsIntersectionEditor.pedestrianGreenLightTime);

                        break;
                    case IntersectionType.LightsCrossing:
                        TrafficLightsCrossingData trafficLightsCrossing = trafficIntersectionsDatahandler.GetTrafficLightsCrossingData(allIntersections[i].OtherListIndex);
                        TrafficLightsCrossingSettings trafficLightsCrossingEditor = (TrafficLightsCrossingSettings)allEditorIntersections[i];
                        trafficLightsCrossing.AddPedestrianWaypoints(trafficLightsCrossingEditor.pedestrianWaypoints.ToListIndex(_allPedestrianEditorWaypoints), trafficLightsCrossingEditor.directionWaypoints.ToListIndex(_allPedestrianEditorWaypoints), trafficLightsCrossingEditor.pedestrianRedLightObjects.ToArray(), trafficLightsCrossingEditor.pedestrianGreenLightObjects.ToArray());
                        break;
                }
            }
#endif
        }
    }
}