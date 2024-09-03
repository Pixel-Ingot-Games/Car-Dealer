using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Struct to store the intersection road properties for each traffic light crossing.
    /// </summary>
    [System.Serializable]
    public class TrafficLightsCrossingData
    {
        public string Name;
        public LightsStopWaypoints[] StopWaypoints;
        public GameObject[] RedLightObjects;
        public GameObject[] GreenLightObjects;
        public int[] ExitWaypoints;
        public int[] PedestrianWaypoints;
        public int[] DirectionWaypoints;
        public float GreenLightTime;
        public float YellowLightTime;
        public float RedLightTime;


        public TrafficLightsCrossingData(string name, LightsStopWaypoints[] stopWaypoints, float greenLightTime, float yellowLightTime, float redLightTime, int[] exitWaypoints)
        {
            Name = name;
            StopWaypoints = stopWaypoints;
            GreenLightTime = greenLightTime;
            YellowLightTime = yellowLightTime;
            RedLightTime = redLightTime;
            ExitWaypoints = exitWaypoints;
        }


        public void AddPedestrianWaypoints(int[] pedestrianWaypoints, int[] directionWaypoints, GameObject[] redLightObjects, GameObject[] greenLightObjects)
        {
            PedestrianWaypoints = pedestrianWaypoints;
            DirectionWaypoints = directionWaypoints;
            RedLightObjects = redLightObjects;
            GreenLightObjects = greenLightObjects;
        }
    }
}
