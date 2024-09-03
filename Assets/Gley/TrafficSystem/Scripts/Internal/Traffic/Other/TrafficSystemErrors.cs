namespace Gley.TrafficSystem.Internal
{
    public static class TrafficSystemErrors
    {
        public static string FatalError => "Traffic System will not work";
        public static string NullWaypointData => "Waypoints data is null";
        public static string NoWaypointsFound => "No waypoints found";
        public static string NoPathFindingWaypoints => "PathFindng not enabled.";
        public static string NullPathFindingData => "Path Finding data is null.";
        public static string NullIntersectionData => "Intersection data is null";
        public static string NoPedestrianWaypoints => "You are using the Pedestrian System, but no pedestrian waypoints are found.";
        public static string LayersNotConfigured => "Layers are not configured. Go to Tools->Gley->Traffic System->Scene Setup->Layer Setup";
        public static string NoVehiclesAvailable => "No vehicles available to instantiate. Make sure your the vehicle pool has at least one vehicle";
        public static string InvalidNrOfVehicles => "Nr. of vehicles needs to be greater than 1";
        public static string NoNeighborSelectorMethod(string message)
        {
            return $"Neighbor selector method has the following error: {message}";
        }
        public static string PropertyError(string callingMethodName)
        {
            return $"Mobile Traffic System is not initialized. Call Gley.TraficSystem.Initialize() before calling {callingMethodName}";
        }
    }
}