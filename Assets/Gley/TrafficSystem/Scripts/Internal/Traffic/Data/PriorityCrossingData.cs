namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Struct to store the intersection road properties for each priority crossing.
    /// </summary>
    [System.Serializable]
    public struct PriorityCrossingData
    {
        public string Name;
        public PriorityStopWaypoints[] StopWaypoints;
        public int[] ExitWaypoints;

        public PriorityCrossingData(string name, PriorityStopWaypoints[] stopWaypoints, int[] exitWaypoints)
        {
            Name = name;
            StopWaypoints = stopWaypoints;
            ExitWaypoints = exitWaypoints;
        }

        public readonly void AddPedestrianWaypoints(int road, int[] pedestrianWaypoints, int[] directionWaypoints)
        {
            StopWaypoints[road].pedestrianWaypoints = pedestrianWaypoints;
            StopWaypoints[road].directionWaypoints = directionWaypoints;
        }
    }
}
