using Gley.UrbanSystem.Internal;
using System.Collections.Generic;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Create and store intersections at runtime.
    /// </summary>
    internal class AllIntersectionsData
    {
        private readonly GenericIntersection[] _allIntersections;
        private readonly TrafficLightsIntersection[] _trafficLightsIntersections;
        private readonly PriorityIntersection[] _priorityIntersections;
        private readonly TrafficLightsCrossing[] _trafficLightsCrossings;
        private readonly PriorityCrossing[] _priorityCrossings;

        internal GenericIntersection[] AllIntersections => _allIntersections;
        internal TrafficLightsIntersection[] TrafficLightsIntersections => _trafficLightsIntersections;
        internal PriorityIntersection[] PriorityIntersections => _priorityIntersections;
        internal TrafficLightsCrossing[] TrafficLightsCrossings => _trafficLightsCrossings;
        internal PriorityCrossing[] PriorityCrossings => _priorityCrossings;


        internal AllIntersectionsData(IntersectionsDataHandler intersectionsDataHandler, TrafficWaypointsDataHandler trafficWaypointsDataHandler, IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler, TrafficLightsBehaviour trafficLightsBehaviour, float greenLightTime, float yellowLightTime)
        {
            var allIntersectionTypes = intersectionsDataHandler.GetIntersectionData();
            _allIntersections = new GenericIntersection[allIntersectionTypes.Length];
            var trafficLightsIntersections = new List<TrafficLightsIntersection>();
            var priorityIntersections = new List<PriorityIntersection>();
            var trafficLightsCrossings = new List<TrafficLightsCrossing>();
            var priorityCrossings = new List<PriorityCrossing>();

            for (int i = 0; i < allIntersectionTypes.Length; i++)
            {
                switch (allIntersectionTypes[i].Type)
                {
                    case IntersectionType.TrafficLights:

                        trafficLightsIntersections.Add(new TrafficLightsIntersection(intersectionsDataHandler.GetTrafficLightsIntersectionData(allIntersectionTypes[i].OtherListIndex), trafficWaypointsDataHandler, pedestrianWaypointsDataHandler, trafficLightsBehaviour, greenLightTime, yellowLightTime));
                        _allIntersections[i] = trafficLightsIntersections[trafficLightsIntersections.Count - 1];
                        break;
                    case IntersectionType.Priority:
                        priorityIntersections.Add(new PriorityIntersection(intersectionsDataHandler.GetPriorityIntersectionData(allIntersectionTypes[i].OtherListIndex), trafficWaypointsDataHandler, pedestrianWaypointsDataHandler));
                        _allIntersections[i] = priorityIntersections[priorityIntersections.Count - 1];
                        break;
                    case IntersectionType.LightsCrossing:
                        trafficLightsCrossings.Add(new TrafficLightsCrossing(intersectionsDataHandler.GetTrafficLightsCrossingData(allIntersectionTypes[i].OtherListIndex), trafficWaypointsDataHandler, pedestrianWaypointsDataHandler, trafficLightsBehaviour));
                        _allIntersections[i] = trafficLightsCrossings[trafficLightsCrossings.Count - 1];
                        break;
                    case IntersectionType.PriorityCrossing:
                        priorityCrossings.Add(new PriorityCrossing(intersectionsDataHandler.GetPriorityCrossingData(allIntersectionTypes[i].OtherListIndex), trafficWaypointsDataHandler, pedestrianWaypointsDataHandler));
                        _allIntersections[i] = priorityCrossings[priorityCrossings.Count - 1];
                        break;
                }
            }
            _priorityCrossings = priorityCrossings.ToArray();
            _priorityIntersections = priorityIntersections.ToArray();
            _trafficLightsCrossings = trafficLightsCrossings.ToArray();
            _trafficLightsIntersections = trafficLightsIntersections.ToArray();
        }
    }
}