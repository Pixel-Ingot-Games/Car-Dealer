using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficWaypointCreator
    {
        internal TrafficWaypointCreator Initialize()
        {
            return this;
        }


        internal Transform CreateWaypoint(Transform parent, Vector3 waypointPosition, string name, List<int> allowedCars, int maxSpeed, float laneWidth)
        {
            GameObject go = MonoBehaviourUtilities.CreateGameObject(name, parent, waypointPosition, true);
            WaypointSettings waypointScript = go.AddComponent<WaypointSettings>();
            waypointScript.Initialize();
            waypointScript.allowedCars = allowedCars.Cast<VehicleTypes>().ToList();
            waypointScript.maxSpeed = maxSpeed;
            waypointScript.laneWidth = laneWidth;
            return go.transform;
        }
    }
}
