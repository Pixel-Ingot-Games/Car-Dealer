#if GLEY_TRAFFIC_SYSTEM
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;


namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Controls the number of active vehicles
    /// </summary>
    internal class DensityManager
    {
        private readonly Queue<RequestVehicle> _requestedVehicles;
        private readonly AllVehiclesDataHandler _trafficVehicles;
        private readonly IdleVehiclesDataHandler _idleVehiclesDataHandler;
        private readonly PositionValidator _positionValidator;
        private readonly GridDataHandler _gridDataHandler;
        private readonly WaypointManager _waypointManager;
        private readonly TrafficWaypointsDataHandler _trafficWaypointsDataHandler;
        private readonly bool _useWaypointPriority;
        private readonly bool _debugDensity;

        private int _maxNrOfVehicles;
        private int _currentNrOfVehicles;
        private int _activeSquaresLevel;
       

        private class RequestVehicle
        {
            internal UnityAction<VehicleComponent, int> CompleteMethod;
            internal List<int> Path;
            internal VehicleComponent Vehicle;
            internal VehicleTypes Type;
            internal Category Category;
            internal int Waypoint;


            internal RequestVehicle(int waypoint, VehicleTypes type, Category category, VehicleComponent vehicle, UnityAction<VehicleComponent, int> completeMethod, List<int> path)
            {
                Waypoint = waypoint;
                Type = type;
                Category = category;
                Vehicle = vehicle;
                CompleteMethod = completeMethod;
                Path = path;
            }
        }


        private enum Category
        {
            Idle,
            Ignored,
        }


        internal DensityManager(AllVehiclesDataHandler trafficVehicles, WaypointManager waypointManager, TrafficWaypointsDataHandler trafficWaypointsDataHandler, GridDataHandler gridDataHandler, PositionValidator positionValidator, NativeArray<float3> activeCameraPositions, int maxNrOfVehicles, Vector3 playerPosition, Vector3 playerDirection, int activeSquaresLevel, bool useWaypointPriority, int initialDensity, Area disableWaypointsArea, bool debugDensity)
        {
            _positionValidator = positionValidator;
            _trafficVehicles = trafficVehicles;
            _waypointManager = waypointManager;
            _activeSquaresLevel = activeSquaresLevel;
            _gridDataHandler = gridDataHandler;
            _maxNrOfVehicles = maxNrOfVehicles;
            _useWaypointPriority = useWaypointPriority;
            _trafficWaypointsDataHandler = trafficWaypointsDataHandler;
            _requestedVehicles = new Queue<RequestVehicle>();
            _debugDensity = debugDensity;

            //disable loaded vehicles
            var idleVehicles = new List<VehicleComponent>();
            for (int i = 0; i < maxNrOfVehicles; i++)
            {
                var vehicle = _trafficVehicles.GetVehicle(i);
                if (!vehicle.excluded)
                {
                    idleVehicles.Add(vehicle);
                }
            }

            _idleVehiclesDataHandler = new IdleVehiclesDataHandler(new IdleVehiclesData(idleVehicles));

            var gridCells = new List<CellData>();
            for (int i = 0; i < activeCameraPositions.Length; i++)
            {
                gridCells.Add(_gridDataHandler.GetCell(activeCameraPositions[i].x, activeCameraPositions[i].z));
            }

            if (initialDensity >= 0)
            {
                SetTrafficDensity(initialDensity);
            }

            if (disableWaypointsArea.radius > 0)
            {
                DisableAreaWaypoints(new Area(disableWaypointsArea));
            }

            LoadInitialVehicles(gridCells, playerPosition, playerDirection);
        }


        /// <summary>
        /// Change vehicle density
        /// </summary>
        /// <param name="nrOfVehicles">cannot be greater than max vehicle number set on initialize</param>
        internal void SetTrafficDensity(int nrOfVehicles)
        {
            _maxNrOfVehicles = nrOfVehicles;
        }


        internal void UpdateActiveSquares(int newLevel)
        {
            _activeSquaresLevel = newLevel;
        }


        /// <summary>
        /// Ads new vehicles if required
        /// </summary>
        internal void UpdateVehicleDensity(Vector3 playerPosition, Vector3 playerDirection, Vector3 activeCameraPosition)
        {
            if (_currentNrOfVehicles < _maxNrOfVehicles)
            {
                CellData gridCell = _gridDataHandler.GetCell(activeCameraPosition);
                BeginAddVehicleProcess(playerPosition, playerDirection, gridCell, false);
            }
        }


        internal void AddExcludedVehicle(int vehicleIndex, Vector3 position, UnityAction<VehicleComponent, int> completeMethod)
        {
            if (position == Vector3.zero)
            {
                return;
            }

            if (!_trafficVehicles.VehicleIsExcluded(vehicleIndex))
            {
                Debug.LogWarning($"vehicleIndex {vehicleIndex} is not marked as ignored, it will not be instantiated");
                return;
            }
            VehicleComponent vehicle = _trafficVehicles.GetExcludedVehicle(vehicleIndex);
            VehicleTypes type = vehicle.VehicleType;
            int waypointIndex = GetClosestSpawnWaypoint(position, type);
            if (waypointIndex != TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
            {
                _requestedVehicles.Enqueue(new RequestVehicle(waypointIndex, type, Category.Ignored, _trafficVehicles.GetExcludedVehicle(vehicleIndex), completeMethod, null));
            }
            else
            {
                Debug.LogWarning("No waypoint found!");
            }
        }


        internal void AddVehicleAtPosition(Vector3 position, VehicleTypes type, UnityAction<VehicleComponent, int> completeMethod, List<int> path)
        {
            int waypointIndex = GetClosestSpawnWaypoint(position, type);

            if (waypointIndex == TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
            {
                Debug.LogWarning("There are no free waypoints in the current cell");
                return;
            }

            _requestedVehicles.Enqueue(new RequestVehicle(waypointIndex, type, Category.Idle, null, completeMethod, path));
        }


        /// <summary>
        /// Remove a vehicle if required
        /// </summary>
        /// <param name="vehicleIndex">vehicle to remove</param>
        /// <param name="force">remove the vehicle even if not all conditions for removing are met</param>
        /// <returns>true if a vehicle was really removed</returns>
        internal void RemoveVehicle(int vehicleIndex)
        {        
            _trafficVehicles.RemoveVehicle(vehicleIndex);
            var vehicle = _trafficVehicles.GetVehicle(vehicleIndex);
            _idleVehiclesDataHandler.AddVehicle(vehicle);
            _currentNrOfVehicles--;
        }


        /// <summary>
        /// Update the active camera used to determine if a vehicle is in view
        /// </summary>
        /// <param name="activeCamerasPosition"></param>
        internal void UpdateCameraPositions(Transform[] activeCameras)
        {
            _positionValidator.UpdateCamera(activeCameras);
        }


        internal void ExcludeVehicleFromSystem(int vehicleIndex)
        {
            _trafficVehicles.SetExcludedValue(vehicleIndex, true);
            _idleVehiclesDataHandler.RemoveVehicle(_trafficVehicles.GetVehicle(vehicleIndex));
        }


        internal void AddExcludecVehicleToSystem(int vehicleIndex)
        {
            _trafficVehicles.SetExcludedValue(vehicleIndex, false);
            _idleVehiclesDataHandler.AddVehicle(_trafficVehicles.GetVehicle(vehicleIndex));
        }


        /// <summary>
        /// Makes waypoints on a given radius unavailable
        /// </summary>
        internal void DisableAreaWaypoints(Area area)
        {
            Debug.Log(area.radius);
            CellData cell = _gridDataHandler.GetCell(area.center);
            List<Vector2Int> neighbors = _gridDataHandler.GetCellNeighbors(cell.CellProperties.Row, cell.CellProperties.Column, Mathf.CeilToInt(area.radius * 2 / _gridDataHandler.GetCellSize()), false);
            for (int i = neighbors.Count - 1; i >= 0; i--)
            {
                cell = _gridDataHandler.GetCell(neighbors[i]);
                for (int j = 0; j < cell.TrafficWaypointsData.Waypoints.Count; j++)
                {
                    int waypointIndex = cell.TrafficWaypointsData.Waypoints[j];
                    if (Vector3.SqrMagnitude(area.center - _trafficWaypointsDataHandler.GetPosition(waypointIndex)) < area.sqrRadius)
                    {
                        _waypointManager.AddDisabledWaypoint(waypointIndex);
                    }
                }
            }
        }


        internal int GetClosestSpawnWaypoint(Vector3 position, VehicleTypes type)
        {
            List<SpawnWaypoint> possibleWaypoints = _gridDataHandler.GetTrafficSpawnWaypoipointsAroundPosition(position, (int)type);

            if (possibleWaypoints.Count == 0)
                return -1;

            float distance = float.MaxValue;
            int waypointIndex = -1;
            for (int i = 0; i < possibleWaypoints.Count; i++)
            {
                float newDistance = Vector3.SqrMagnitude(_trafficWaypointsDataHandler.GetPosition(possibleWaypoints[i].WaypointIndex) - position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    waypointIndex = possibleWaypoints[i].WaypointIndex;
                }
            }
            return waypointIndex;
        }


        /// <summary>
        /// Add all vehicles around the player even if they are inside players view
        /// </summary>
        /// <param name="currentGridRow"></param>
        /// <param name="currentGridColumn"></param>
        private void LoadInitialVehicles(List<CellData> gridCells, Vector3 playerPosition, Vector3 playerDirection)
        {
            for (int i = 0; i < _maxNrOfVehicles; i++)
            {
                int cellIndex = UnityEngine.Random.Range(0, gridCells.Count);
                BeginAddVehicleProcess(playerPosition, playerDirection, gridCells[cellIndex], true);
            }
        }


        private void BeginAddVehicleProcess(Vector3 playerPosition, Vector3 playerDirection, CellData gridCell, bool ignorLOS)
        {
            if (_requestedVehicles.Count == 0)
            {
                AddVehicleOnArea(playerPosition, playerDirection, gridCell, ignorLOS);
            }
            else
            {
                //add specific vehicle on position
                var requested = _requestedVehicles.Peek();
                switch (requested.Category)
                {
                    case Category.Idle:
                        if (requested.Vehicle == null)
                        {
                            int idleVehicleIndex = _idleVehiclesDataHandler.GetIdleVehicleIndex(requested.Type);
                            //if an idle vehicle does not exists
                            if (idleVehicleIndex == TrafficSystemConstants.INVALID_VEHICLE_INDEX)
                            {
                                if (_debugDensity)
                                {
                                    Debug.Log($"Density: No vehicle of type {requested.Type} is idle");
                                }
                                AddVehicleOnArea(playerPosition, playerDirection, gridCell, ignorLOS);
                                return;
                            }
                            requested.Vehicle = _idleVehiclesDataHandler.GetAndRemoveVehicle(idleVehicleIndex);
                            if (requested.Vehicle == null)
                            {
                                if (_debugDensity)
                                {
                                    Debug.Log($"Density: Vehicle with index {idleVehicleIndex} is null");
                                }
                                return;
                            }
                        }
                        break;

                    case Category.Ignored:

                        if (requested.Vehicle.gameObject.activeSelf)
                        {
                            AddVehicleOnArea(playerPosition, playerDirection, gridCell, ignorLOS);
                            return;
                        }
                        break;
                }


                if (AddVehicle(true, requested.Waypoint, requested.Vehicle))
                {
                    var request = _requestedVehicles.Dequeue();
                    request.CompleteMethod?.Invoke(request.Vehicle, request.Waypoint);
                    if (request.Path != null)
                    {
                        _waypointManager.SetAgentPath(request.Vehicle.ListIndex, new Queue<int>(request.Path));
                    }
                }
                else
                {
                    AddVehicleOnArea(playerPosition, playerDirection, gridCell, ignorLOS);
                }
            }
        }


        private void AddVehicleOnArea(Vector3 playerPosition, Vector3 playerDirection, CellData gridCell, bool ignorLOS)
        {
            //add any vehicle on area
            int idleVehicleIndex = _idleVehiclesDataHandler.GetRandomIndex();

            //if an idle vehicle does not exists
            if (idleVehicleIndex == TrafficSystemConstants.INVALID_VEHICLE_INDEX)
            {
                if (_debugDensity)
                {
                    Debug.Log("Density: No idle vehicle found");
                }
                return;
            }

            int freeWaypointIndex = _waypointManager.GetNeighborCellWaypoint(gridCell.CellProperties.Row, gridCell.CellProperties.Column, _activeSquaresLevel, _idleVehiclesDataHandler.GetIdleVehicleType(idleVehicleIndex), playerPosition, playerDirection, _useWaypointPriority);
            //Debug.Log(freeWaypointIndex);
            //freeWaypointIndex = 3;

            if (freeWaypointIndex == TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
            {
                if (_debugDensity)
                {
                    Debug.Log("Density: No free waypoint found");
                }
                return;
            }

            AddVehicle(ignorLOS, freeWaypointIndex, _idleVehiclesDataHandler.PeakIdleVehicle(idleVehicleIndex));
        }


        /// <summary>
        /// Trying to load an idle vehicle if exists
        /// </summary>
        private bool AddVehicle(bool firstTime, int freeWaypointIndex, VehicleComponent vehicle)
        {
            //Debug.Log(freeWaypointIndex);
            //if a valid waypoint was found, check if it was not manually disabled
            if (_trafficWaypointsDataHandler.IsTemporaryDisabled(freeWaypointIndex))
            {
                if (_debugDensity)
                {
                    Debug.Log("Density: waypoint is disabled");
                }
                return false;
            }

            //check if the car type can be instantiated on selected waypoint
            if (!_positionValidator.IsValid(_trafficWaypointsDataHandler.GetPosition(freeWaypointIndex), vehicle.length * 2, vehicle.coliderHeight, vehicle.ColliderWidth, firstTime, vehicle.frontTrigger.localPosition.z, _waypointManager.GetNextOrientation(freeWaypointIndex)))
            {
                return false;
            }

            Quaternion trailerRotaion = Quaternion.identity;
            if (vehicle.trailer != null)
            {
                trailerRotaion = _waypointManager.GetPrevOrientation(freeWaypointIndex);
                if (trailerRotaion == Quaternion.identity)
                {
                    trailerRotaion = _waypointManager.GetNextOrientation(freeWaypointIndex);
                }

                if (!_positionValidator.CheckTrailerPosition(_trafficWaypointsDataHandler.GetPosition(freeWaypointIndex), _waypointManager.GetNextOrientation(freeWaypointIndex), trailerRotaion, vehicle))
                {
                    return false;
                }
            }

            _currentNrOfVehicles++;
            int vehicleIndex = vehicle.ListIndex;
            _waypointManager.SetTargetWaypoint(vehicleIndex, freeWaypointIndex);
            _trafficVehicles.ActivateVehicle(vehicle, _trafficWaypointsDataHandler.GetPosition(_waypointManager.GetTargetWaypointIndex(vehicleIndex)), _waypointManager.GetTargetWaypointRotation(vehicleIndex), trailerRotaion);
            _idleVehiclesDataHandler.RemoveVehicle(vehicle);
            Events.TriggerVehicleAddedEvent(vehicleIndex);
            return true;
        }
    }
}
#endif