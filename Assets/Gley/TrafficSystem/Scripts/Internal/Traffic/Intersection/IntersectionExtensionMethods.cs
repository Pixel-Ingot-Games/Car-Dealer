#if UNITY_EDITOR
namespace Gley.TrafficSystem.Internal
{
    public static class IntersectionExtensionMethods
    {
        /// <summary>
        /// Converts editor priority intersection to runtime priority intersection
        /// </summary>
        /// <param name="priorityIntersection"></param>
        /// <param name="allWaypoints"></param>
        /// <returns></returns>
        public static PriorityIntersectionData ToPlayModeIntersection(this PriorityIntersectionSettings priorityIntersection, WaypointSettings[] allWaypoints)
        {
            return new PriorityIntersectionData(priorityIntersection.name, priorityIntersection.enterWaypoints.ToPriorityStopWaypointsArray(allWaypoints), priorityIntersection.exitWaypoints.ToListIndex(allWaypoints));
        }


        public static PriorityCrossingData ToPlayModeIntersection(this PriorityCrossingSettings priorityIntersection, WaypointSettings[] allWaypoints)
        {
            return new PriorityCrossingData(priorityIntersection.name, priorityIntersection.enterWaypoints.ToPriorityStopWaypointsArray(allWaypoints), priorityIntersection.exitWaypoints.ToListIndex(allWaypoints));
        }

        /// <summary>
        /// Converts editor traffic lights intersection to runtime traffic lights intersection
        /// </summary>
        /// <param name="trafficLightsIntersection"></param>
        /// <param name="allWaypoints"></param>
        /// <returns></returns>
        public static TrafficLightsIntersectionData ToPlayModeIntersection(this TrafficLightsIntersectionSettings trafficLightsIntersection, WaypointSettings[] allWaypoints)
        {
            return new TrafficLightsIntersectionData(
                trafficLightsIntersection.name, 
                trafficLightsIntersection.stopWaypoints.ToLightsStopWaypointsArray(allWaypoints), 
                trafficLightsIntersection.greenLightTime, 
                trafficLightsIntersection.yellowLightTime, 
                trafficLightsIntersection.exitWaypoints.ToListIndex(allWaypoints)
            );
        }

        public static TrafficLightsCrossingData ToPlayModeIntersection(this TrafficLightsCrossingSettings trafficLightsIntersection, WaypointSettings[] allWaypoints)
        {
            return new TrafficLightsCrossingData(
                trafficLightsIntersection.name,
                trafficLightsIntersection.stopWaypoints.ToLightsStopWaypointsArray(allWaypoints),
                trafficLightsIntersection.greenLightTime,
                trafficLightsIntersection.yellowLightTime,
                trafficLightsIntersection.redLightTime,
                trafficLightsIntersection.exitWaypoints.ToListIndex(allWaypoints)
            ); 
        }
    }
}
#endif
