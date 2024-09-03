using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using Unity.Collections;
#if GLEY_TRAFFIC_SYSTEM
using Unity.Mathematics;
#endif
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Draw helping gizmos on scene.
    /// </summary>
    internal class DebugManager
    {
#if UNITY_EDITOR
#if GLEY_TRAFFIC_SYSTEM
        private DebugSettings _debugSettings;
        private AllVehiclesDataHandler _allVehiclesDataHandler;
        private WaypointManager _waypointManager;
        private DrivingAI _drivingAI;
        private PathFindingDataHandler _pathFindingDataHandler;
        private AllIntersectionsDataHandler _allIntersectionsDataHandler;
        private TrafficWaypointsDataHandler _trafficWaypointsDataHandler;
        private VehiclePositioningSystem _vehiclePositioningSystem;
        private GridDataHandler _gridDataHandler;

        public DebugManager(DebugSettings debugSettings, AllVehiclesDataHandler allVehiclesDataHandler, WaypointManager waypointManager, DrivingAI drivingAI, PathFindingDataHandler pathFindingDataHandler, AllIntersectionsDataHandler allIntersectionsDataHandler, 
            TrafficWaypointsDataHandler trafficWaypointsDataHandler, VehiclePositioningSystem vehiclePositioningSystem, GridDataHandler gridDataHandler)
        {
            _debugSettings = debugSettings;
            _allVehiclesDataHandler = allVehiclesDataHandler;
            _waypointManager = waypointManager;
            _drivingAI = drivingAI;
            _pathFindingDataHandler = pathFindingDataHandler;
            _allIntersectionsDataHandler = allIntersectionsDataHandler;
            _trafficWaypointsDataHandler = trafficWaypointsDataHandler;
            _vehiclePositioningSystem = vehiclePositioningSystem;
            _gridDataHandler = gridDataHandler;
        }

        internal void Update(int nrOfVehicles, int totalWheels, NativeArray<float3> wheelSuspensionPosition, NativeArray<float3> wheelSuspensionForce, NativeArray<int> wheelAssociatedCar)
        {
            if (_debugSettings.drawBodyForces)
            {
                DrawBodyForces(nrOfVehicles, totalWheels, wheelSuspensionPosition, wheelSuspensionForce, wheelAssociatedCar);
            }

            if (_debugSettings.debug)
            {
                DrowObstacleLine(nrOfVehicles);
            }
        }



        internal void DrawGizmos()
        {
            if (_debugSettings.debug)
            {
                DebugVehicleActions(_debugSettings.debugSpeed, _debugSettings.debugPathFinding);
            }

            if (_debugSettings.debugIntersections)
            {
                DebugIntersections();
            }

            if (_debugSettings.debugWaypoints)
            {
                DebugWaypoints();
            }

            if (_debugSettings.debugDisabledWaypoints)
            {
                DebugDisabledWaypoints();
            }

            if(_debugSettings.DebugSpawnWaypoints)
            {
                DebugSpawnWaypoints();
            }

            if(_debugSettings.DebugPlayModeWaypoints)
            {
                DebugPlayModeWaypoints();
            }
        }

        private void DebugPlayModeWaypoints()
        {
            if (Application.isPlaying)
            {
                Vector3 position;
                var allWaypoints = _gridDataHandler.GetAllTrafficPlayModeWaypoints();
                for (int i = 0; i < allWaypoints.Count; i++)
                {
                    Gizmos.color = Color.blue;
                    position = _trafficWaypointsDataHandler.GetPosition(allWaypoints[i]);
                    if (_debugSettings.ShowPosition)
                    {
                        Gizmos.DrawSphere(position, 0.4f);
                    }
                    if(_debugSettings.ShowIndex)
                    {
                        Handles.Label(position, allWaypoints[i].ToString());
                    }
                }
            }
        }


        private void DebugSpawnWaypoints()
        {
            if (Application.isPlaying)
            {
                var allWaypoints = _gridDataHandler.GetAllTrafficSpawnWaypoints();
                for (int i = 0; i < allWaypoints.Count; i++)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(_trafficWaypointsDataHandler.GetPosition(allWaypoints[i].WaypointIndex), 0.5f);
                }
            }
        }

        internal bool IsDebugWaypointsEnabled()
        {
            return _debugSettings.debugWaypoints;
        }

        private void DrowObstacleLine(int nrOfVehicles)
        {
            for (int i = 0; i < nrOfVehicles; i++)
            {
                if (!_allVehiclesDataHandler.GetClosestObstacle(i).Equals(float3.zero))
                {
                    Debug.DrawLine(_allVehiclesDataHandler.GetClosestObstacle(i), _vehiclePositioningSystem.GetPosition(i), Color.magenta);
                }
            }
        }

        private void DrawBodyForces(int nrOfVehicles, int totalWheels, NativeArray<float3> wheelSuspensionPosition, NativeArray<float3> wheelSuspensionForce, NativeArray<int> wheelAssociatedCar)
        {
            for (int i = 0; i < nrOfVehicles; i++)
            {
                Debug.DrawRay(_allVehiclesDataHandler.GetRigidbody(i).transform.TransformPoint(_allVehiclesDataHandler.GetRigidbody(i).centerOfMass), _allVehiclesDataHandler.GetVelocity(i), Color.red);
                if (_allVehiclesDataHandler.HasTrailer(i))
                {
                    Vector3 localVelocity = _allVehiclesDataHandler.GetTrailerRigidbody(i).transform.InverseTransformVector(_allVehiclesDataHandler.GetTrailerRigidbody(i).velocity);
                    Debug.DrawRay(_allVehiclesDataHandler.GetTrailerRigidbody(i).transform.TransformPoint(_allVehiclesDataHandler.GetTrailerRigidbody(i).centerOfMass), new Vector3(-localVelocity.x, 0, 0) * 100, Color.green, Time.deltaTime, false);
                    Debug.DrawRay(_allVehiclesDataHandler.GetTrailerRigidbody(i).transform.TransformPoint(_allVehiclesDataHandler.GetTrailerRigidbody(i).centerOfMass), _allVehiclesDataHandler.GetTrailerRigidbody(i).velocity, Color.red);
                }
            }

            for (int j = 0; j < totalWheels; j++)
            {
                Debug.DrawRay(wheelSuspensionPosition[j], wheelSuspensionForce[j] / _allVehiclesDataHandler.GetSpringForce(wheelAssociatedCar[j]), Color.yellow);
            }
        }


        private void DebugDisabledWaypoints()
        {
            for (int i = 0; i < _waypointManager.GetDisabledWaypoints().Count; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(_trafficWaypointsDataHandler.GetPosition(_waypointManager.GetDisabledWaypoints()[i]), 1);
            }

        }

        private void DebugWaypoints()
        {
            for (int i = 0; i < _waypointManager.GetTargetWaypoints().Length; i++)
            {
                if (_waypointManager.GetTargetWaypoints()[i] != TrafficSystemConstants.INVALID_VEHICLE_INDEX)
                {
                    Gizmos.color = Color.green;
                    Vector3 position = _trafficWaypointsDataHandler.GetPosition(_waypointManager.GetTargetWaypoints()[i]);
                    Gizmos.DrawSphere(position, 1);
                    position.y += 1.5f;
                    Handles.Label(position, i.ToString());
                }
            }
        }

        private void DebugIntersections()
        {
            var allIntersections = _allIntersectionsDataHandler.GetAllIntersections();
            for (int k = 0; k < allIntersections.Length; k++)
            {
                var stopWaypoints = allIntersections[k].GetStopWaypoints();
                for (int i = 0; i < stopWaypoints.Count; i++)
                {
                    if (_trafficWaypointsDataHandler.IsStop(stopWaypoints[i]) == true)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(_trafficWaypointsDataHandler.GetPosition(stopWaypoints[i]), 1);
                    }
                }


                //priority intersections
                if (allIntersections[k].GetType().Equals(typeof(PriorityIntersection)))
                {
                    PriorityIntersection intersection = (PriorityIntersection)allIntersections[k];
                    string text = $"In intersection \nVehicles {intersection.GetCarsInIntersection()}";
#if GLEY_PEDESTRIAN_SYSTEM
                    text += $"\nPedestrians {intersection.GetPedestriansCrossing().Count}";
#endif
                    Handles.Label(intersection.GetPosition(), text);
                    for (int i = 0; i < intersection.GetWaypointsToCkeck().Count; i++)
                    {
                        Handles.color = intersection.GetWaypointColors()[i];
                        Handles.DrawWireDisc(_trafficWaypointsDataHandler.GetPosition(intersection.GetWaypointsToCkeck()[i]), Vector3.up, 1);
                    }
                }

                //priority crossings
                if (allIntersections[k].GetType().Equals(typeof(PriorityCrossing)))
                {
                    PriorityCrossing intersection = (PriorityCrossing)allIntersections[k];
                    string text = $"Crossing \n";
#if GLEY_PEDESTRIAN_SYSTEM
                    text += $"\nPedestrians {intersection.GetPedestriansCrossing().Count}";
#endif
                    Handles.Label(intersection.GetPosition(), text);
                    for (int i = 0; i < intersection.GetWaypointsToCkeck().Length; i++)
                    {
                        Handles.color = intersection.GetWaypointColors();
                        Handles.DrawWireDisc(_trafficWaypointsDataHandler.GetPosition(intersection.GetWaypointsToCkeck()[i]), Vector3.up, 1);
                    }
                }

#if GLEY_PEDESTRIAN_SYSTEM
#if GLEY_TRAFFIC_SYSTEM
                if (Gley.PedestrianSystem.Internal.PedestrianManager.Instance.IsInitialized())
                {
                    int[] pedestrianStopWaypoints = allIntersections[k].GetPedStopWaypoint();
                    for (int l = 0; l < pedestrianStopWaypoints.Length; l++)
                    {
                        if (Gley.PedestrianSystem.Internal.PedestrianManager.Instance.PedestrianWaypointsDataHandler.IsStop(pedestrianStopWaypoints[l]))
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawSphere(Gley.PedestrianSystem.Internal.PedestrianManager.Instance.PedestrianWaypointsDataHandler.GetPosition(pedestrianStopWaypoints[l]), 1);
                        }
                    }
                }
#endif
#endif
            }
        }

        private void DebugVehicleActions(bool speedDebug, bool debugPathFinding)
        {
            VehicleComponent[] allVehicles = _allVehiclesDataHandler.GetAllVehicles();
            for (int i = 0; i < allVehicles.Length; i++)
            {
                string hasPath = _waypointManager.HasPath(allVehicles[i].ListIndex) ? "Has Path" : "";
                string text = $"{allVehicles[i].ListIndex}. Action {allVehicles[i].CurrentAction} {hasPath} \n";
                if (speedDebug)
                {
                    text += "Current Speed " + allVehicles[i].GetCurrentSpeed().ToString("N1") + "\n" +
                    "Follow Speed " + _allVehiclesDataHandler.GetFollowSpeed(allVehicles[i].ListIndex).ToString("N1") + "\n" +
                    "Waypoint Speed " + _drivingAI.GetWaypointSpeed(allVehicles[i].ListIndex).ToString("N1") + "\n" +
                    "Max Speed" + _allVehiclesDataHandler.GetMaxSpeed(allVehicles[i].ListIndex).ToString("N1") + "\n";
                }

                Handles.Label(allVehicles[i].transform.position + new Vector3(1, 1, 1), text);
                if (debugPathFinding)
                {
                    if (_waypointManager.HasPath(allVehicles[i].ListIndex))
                    {
                        Queue<int> path = _waypointManager.GetPath(allVehicles[i].ListIndex);
                        foreach (int n in path)
                        {
                            Gizmos.color = Color.red;
                            Vector3 position = _pathFindingDataHandler.GetWaypointPosition(n);
                            Gizmos.DrawWireSphere(position, 1);
                            position.y += 1;
                            Handles.Label(position, allVehicles[i].ListIndex.ToString());
                        }
                    }
                }
            }
        }
#endif
#endif
    }
}