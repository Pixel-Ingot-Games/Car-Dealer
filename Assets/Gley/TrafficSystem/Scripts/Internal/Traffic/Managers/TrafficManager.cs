#if GLEY_TRAFFIC_SYSTEM
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Gley.UrbanSystem.Internal;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using UnityEngine.Events;



#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Internal;
#endif

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// This is the core class of the system, it controls everything else
    /// </summary>
    internal class TrafficManager : MonoBehaviour
    {
        #region Variables
        //transforms to update
        private TransformAccessArray _vehicleTrigger;
        private TransformAccessArray _suspensionConnectPoints;
        private TransformAccessArray _wheelsGraphics;

        private NativeArray<float3> _activeCameraPositions;

        //properties for each vehicle
        private NativeArray<DriveActions> _vehicleSpecialDriveAction;
        private NativeArray<VehicleTypes> _vehicleType;
        private Rigidbody[] _vehicleRigidbody;
        private Dictionary<int, Rigidbody> _trailerRigidbody;

        private NativeArray<float3> _vehicleDownDirection;
        private NativeArray<float3> _vehicleForwardDirection;
        private NativeArray<float3> _trailerForwardDirection;
        private NativeArray<float3> _triggerForwardDirection;
        private NativeArray<float3> _vehicleRightDirection;
        private NativeArray<float3> _trailerRightDirection;
        private NativeArray<float3> _vehicleTargetWaypointPosition;
        private NativeArray<float3> _vehiclePosition;
        private NativeArray<float3> _vehicleGroundDirection;
        private NativeArray<float3> _vehicleForwardForce;
        private NativeArray<float3> _trailerForwardForce;


        private NativeArray<float3> _vehicleVelocity;
        private NativeArray<float3> _trailerVelocity;
        private NativeArray<float3> _closestObstacle;
        private NativeArray<float> _wheelSpringForce;
        private NativeArray<float> _vehicleMaxSteer;
        private NativeArray<float> _vehicleRotationAngle;
        private NativeArray<float> _vehiclePowerStep;
        private NativeArray<float> _vehicleBrakeStep;
        private NativeArray<float> _vehicleActionValue;
        private NativeArray<float> _vehicleDrag;
        private NativeArray<float> _massDifference;
        private NativeArray<float> _trailerDrag;
        private NativeArray<float> _vehicleMaxSpeed;
        private NativeArray<float> _vehicleLength;
        private NativeArray<float> _vehicleWheelDistance;
        private NativeArray<float> _vehicleSteeringStep;
        private NativeArray<float> _vehicleDistanceToStop;
        private NativeArray<int> _vehicleStartWheelIndex;//start index for the wheels of car i (dim nrOfCars)
        private NativeArray<int> _vehicleEndWheelIndex; //number of wheels that car with index i has (nrOfCars)
        private NativeArray<int> _vehicleNrOfWheels;
        private NativeArray<int> _vehicleListIndex;
        private NativeArray<int> _vehicleGear;
        private NativeArray<int> _trailerNrWheels;
        private NativeArray<bool> _vehicleReadyToRemove;
        private NativeArray<bool> _vehicleIsBraking;
        private NativeArray<bool> _vehicleNeedWaypoint;
        private NativeArray<bool> _ignoreVehicle;

        //properties for each wheel
        private NativeArray<RaycastHit> _wheelRaycatsResult;
        private NativeArray<RaycastCommand> _wheelRaycastCommand;
        private NativeArray<float3> _wheelSuspensionPosition;
        private NativeArray<float3> _wheelGroundPosition;
        private NativeArray<float3> _wheelVelocity;
        private NativeArray<float3> _wheelRightDirection;
        private NativeArray<float3> _wheelNormalDirection;
        private NativeArray<float3> _wheelSuspensionForce;
        private NativeArray<float3> _wheelSideForce;
        private NativeArray<float> _wheelRotation;
        private NativeArray<float> _wheelRadius;
        private NativeArray<float> _wheelRaycatsDistance;
        private NativeArray<float> _wheelMaxSuspension;
        private NativeArray<float> _wheelSpringStiffness;

        private NativeArray<int> _wheelSign;
        private NativeArray<int> _wheelAssociatedCar; //index of the car that contains the wheel
        private NativeArray<bool> _wheelCanSteer;

        //properties that should be on each wheel
        private NativeArray<float> _turnAngle;
        private NativeArray<float> _raycastLengths;
        private NativeArray<float> _wCircumferences;

        //jobs
        private UpdateWheelJob _updateWheelJob;
        private UpdateTriggerJob _updateTriggerJob;
        private DriveJob _driveJob;
        private WheelJob _wheelJob;
        private JobHandle _raycastJobHandle;
        private JobHandle _updateWheelJobHandle;
        private JobHandle _updateTriggerJobHandle;
        private JobHandle _driveJobHandle;
        private JobHandle _wheelJobHandle;

        //additional properties
        private Transform[] _activeCameras;
        private LayerMask _roadLayers;
        private Vector3 _forward;
        private Vector3 _up;
        private float _distanceToRemove;
        private float _minDistanceToAdd;
        private int _nrOfVehicles;
        private int _nrOfJobs;
        private int _indexToRemove;
        private int _totalWheels;
        private int _activeSquaresLevel;
        private int _activeCameraIndex;
        private bool _initialized;
#pragma warning disable 0649
        private bool _clearPath;
        private RoadSide _side;
#pragma warning restore 0649
        #endregion

        private static TrafficManager _instance;
        internal static TrafficManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (MonoBehaviourUtilities.TryGetSceneScript<TrafficWaypointsData>(out var result))
                    {
                        _instance = result.Value.gameObject.AddComponent<TrafficManager>();
                    }
                    else
                    {
                        Debug.LogError(result.Error);
                        Debug.LogError(TrafficSystemErrors.FatalError);
                    }

                }
                return _instance;
            }
        }

        public static bool Exists
        {
            get
            {
                return _instance != null;
            }
        }

        private AllVehiclesDataHandler _allVehiclesDataHandler;
        internal AllVehiclesDataHandler AllVehiclesDataHandler
        {
            get
            {
                if (_allVehiclesDataHandler != null)
                {
                    return _allVehiclesDataHandler;
                }

                return ReturnError<AllVehiclesDataHandler>();
            }
        }

        private DensityManager _densityManager;
        internal DensityManager DensityManager
        {
            get
            {
                if (_densityManager != null)
                {
                    return _densityManager;
                }
                return ReturnError<DensityManager>();
            }
        }

        private SoundManager _soundManager;
        internal SoundManager SoundManager
        {
            get
            {
                if (_soundManager != null)
                {
                    return _soundManager;
                }
                return ReturnError<SoundManager>();
            }
        }


        private DrivingAI _drivingAI;
        internal DrivingAI DrivingAI
        {
            get
            {
                if (_drivingAI != null)
                {
                    return _drivingAI;
                }
                return ReturnError<DrivingAI>();
            }
        }

        private WaypointManager _waypointManager;
        internal WaypointManager WaypointManager
        {
            get
            {
                if (_waypointManager != null)
                {
                    return _waypointManager;
                }
                return ReturnError<WaypointManager>();
            }
        }

        private TrafficWaypointsDataHandler _trafficWaypointsDataHandler;
        internal TrafficWaypointsDataHandler TrafficWaypointsDataHandler
        {
            get
            {
                if (_trafficWaypointsDataHandler != null)
                {
                    return _trafficWaypointsDataHandler;
                }
                return ReturnError<TrafficWaypointsDataHandler>();
            }
        }

        private TrafficModules _trafficModules;
        internal TrafficModules TrafficModules
        {
            get
            {
                if (_trafficModules != null)
                {
                    return _trafficModules;
                }
                return ReturnError<TrafficModules>();
            }
        }



        private IntersectionManager _intersectionManager;
        internal IntersectionManager IntersectionManager
        {
            get
            {
                if (_intersectionManager != null)
                {
                    return _intersectionManager;
                }
                return ReturnError<IntersectionManager>();
            }
        }

        private AllIntersectionsDataHandler _allIntersectionsHandler;
        internal AllIntersectionsDataHandler AllIntersectionsHandler
        {
            get
            {
                if (_allIntersectionsHandler != null)
                {
                    return _allIntersectionsHandler;
                }
                return ReturnError<AllIntersectionsDataHandler>();
            }
        }

        private VehiclePositioningSystem _vehiclePositioningSystem;
        internal VehiclePositioningSystem VehiclePositioningSystem
        {
            get
            {
                if (_vehiclePositioningSystem != null)
                {
                    return _vehiclePositioningSystem;
                }
                return ReturnError<VehiclePositioningSystem>();
            }
        }

        private PathFindingManager _pathFindingManager;
        internal PathFindingManager PathFindingManager
        {
            get
            {
                if (TrafficModules.PathFinding)
                {
                    if (_pathFindingManager != null)
                    {
                        return _pathFindingManager;
                    }
                    else
                    {
                        Debug.LogError(TrafficSystemErrors.NullPathFindingData);
                    }
                }
                else
                {
                    Debug.LogError(TrafficSystemErrors.NoPathFindingWaypoints);
                }
                return null;
            }
        }

        private IntersectionsDataHandler _intersectionsDataHandler;
        internal IntersectionsDataHandler IntersectionsDataHandler
        {
            get
            {
                if (_intersectionsDataHandler != null)
                {
                    return _intersectionsDataHandler;
                }
                return ReturnError<IntersectionsDataHandler>();
            }
        }

        private ActiveCellsManager _activeCellsManager;
        internal ActiveCellsManager ActiveCellsManager
        {
            get
            {
                if (_activeCellsManager != null)
                {
                    return _activeCellsManager;
                }
                return ReturnError<ActiveCellsManager>();
            }
        }

        private GridDataHandler _gridDataHandler;
        internal GridDataHandler GridDataHandler
        {
            get
            {
                if (_gridDataHandler != null)
                {
                    return _gridDataHandler;
                }
                return ReturnError<GridDataHandler>();
            }
        }

        private PathFindingDataHandler _pathFindingDataHandler;
        //internal PathFindingDataHandler PathFindingDataHandler
        //{
        //    get
        //    {
        //        if (_pathFindingDataHandler != null)
        //        {
        //            return _pathFindingDataHandler;
        //        }
        //        return ReturnError<PathFindingDataHandler>();
        //    }
        //}

        private TimeManager _timeManager;
        internal TimeManager TimeManager
        {
            get
            {
                if (_timeManager != null)
                {
                    return _timeManager;
                }
                return ReturnError<TimeManager>();
            }
        }

        private DebugManager _debugManager;
        internal DebugManager DebugManager
        {
            get
            {
                if (_debugManager != null)
                {
                    return _debugManager;
                }
                return ReturnError<DebugManager>();
            }
        }

        T ReturnError<T>()
        {
            StackTrace stackTrace = new StackTrace();
            string callingMethodName = string.Empty;
            if (stackTrace.FrameCount >= 3)
            {
                StackFrame callingFrame = stackTrace.GetFrame(1);
                callingMethodName = callingFrame.GetMethod().Name;
            }
            Debug.LogError(TrafficSystemErrors.PropertyError(callingMethodName));
            return default;
        }


        #region TrafficInitialization
        /// <summary>
        /// Initialize the traffic 
        /// </summary>
        internal void Initialize(Transform[] activeCameras, int nrOfVehicles, VehiclePool vehiclePool, TrafficOptions trafficOptions)
        {
            //safety checks
            var layerSetup = Resources.Load<LayerSetup>(TrafficSystemConstants.layerSetupData);
            if (layerSetup == null)
            {
                Debug.LogError(TrafficSystemErrors.LayersNotConfigured);
                return;
            }

            // Load grid data.
            if (MonoBehaviourUtilities.TryGetSceneScript<GridData>(out var resultGridData))
            {
                if (resultGridData.Value.IsValid(out var error))
                {
                    _gridDataHandler = new GridDataHandler(resultGridData.Value);
                }
                else
                {
                    Debug.LogError(error);
                    return;
                }
            }
            else
            {
                Debug.LogError(resultGridData.Error);
                return;
            }

            // Load waypoints data.
            if (MonoBehaviourUtilities.TryGetSceneScript<TrafficWaypointsData>(out var resultWaypointsData))
            {
                if (resultWaypointsData.Value.IsValid(out var error))
                {
                    _trafficWaypointsDataHandler = new TrafficWaypointsDataHandler(resultWaypointsData.Value);
                }
                else
                {
                    Debug.LogError(error);
                    return;
                }
            }
            else
            {
                Debug.LogError(resultWaypointsData.Error);
                return;
            }

            // Load intersection data.
            if (MonoBehaviourUtilities.TryGetSceneScript<IntersectionsData>(out var resultIntersectionsData))
            {
                if (resultIntersectionsData.Value.IsValid(out var error))
                {
                    _intersectionsDataHandler = new IntersectionsDataHandler(resultIntersectionsData.Value);
                }
                else
                {
                    Debug.LogError(error);
                    return;
                }
            }
            else
            {
                Debug.LogError(resultIntersectionsData.Error);
                return;
            }

            gameObject.AddComponent<CoroutineManager>();

            // Load pedestrian data if available
            IPedestrianWaypointsDataHandler pedestrianWaypointsDataHandler = new DummyPedestrianWaypointsDataHandler();
#if GLEY_PEDESTRIAN_SYSTEM
            if (MonoBehaviourUtilities.TryGetSceneScript<PedestrianWaypointsData>(out var resultPedestrianWaypoints))
            {
                if (resultPedestrianWaypoints.Value.IsValid(out var error))
                {
                    pedestrianWaypointsDataHandler = new PedestrianWaypointsDataHandler(resultPedestrianWaypoints.Value);
                }
                else
                {
                    Debug.LogWarning($"{TrafficSystemErrors.NoPedestrianWaypoints}\n{error}");
                }
            }
            else
            {
                Debug.LogWarning($"{TrafficSystemErrors.NoPedestrianWaypoints}\n{resultPedestrianWaypoints.Error}");
            }
#endif

            if (MonoBehaviourUtilities.TryGetObjectScript<TrafficModules>(TrafficSystemConstants.PlayHolder, out var trafficModules))
            {
                _trafficModules = trafficModules.Value;
            }
            else
            {
                Debug.LogError(trafficModules.Error);
                return;
            }


            if (TrafficModules.PathFinding)
            {
                // Load path finding data.
                if (MonoBehaviourUtilities.TryGetObjectScript<PathFindingData>(TrafficSystemConstants.PlayHolder, out var resultPathFindingData))
                {
                    if (resultPathFindingData.Value.IsValid(out var error))
                    {
                        _pathFindingDataHandler = new PathFindingDataHandler(resultPathFindingData.Value);
                    }
                    else
                    {
                        Debug.LogError(error);
                        return;
                    }
                }
                else
                {
                    Debug.LogError(resultPathFindingData.Error);
                    return;
                }
            }


            if (vehiclePool.trafficCars.Length <= 0)
            {
                Debug.LogError(TrafficSystemErrors.NoVehiclesAvailable);
                return;
            }

            if (nrOfVehicles <= 0)
            {
                Debug.LogError(TrafficSystemErrors.InvalidNrOfVehicles);
                return;
            }

            _nrOfVehicles = nrOfVehicles;
            _activeCameras = activeCameras;
            _activeSquaresLevel = trafficOptions.activeSquaresLevel;
            _roadLayers = layerSetup.roadLayers;
            _up = Vector3.up;

            _soundManager = new SoundManager(trafficOptions.masterVolume);

            // Compute total wheels
            var allVehiclesData = new AllVehiclesData(transform, vehiclePool, nrOfVehicles, layerSetup.buildingsLayers, layerSetup.obstaclesLayers, layerSetup.playerLayers, layerSetup.roadLayers, trafficOptions.lightsOn, trafficOptions.ModifyTriggerSize);
            _allVehiclesDataHandler = new AllVehiclesDataHandler(allVehiclesData);
            _totalWheels = AllVehiclesDataHandler.GetTotalWheels();

            //initialize arrays
            _wheelSuspensionPosition = new NativeArray<float3>(_totalWheels, Allocator.Persistent);
            _wheelVelocity = new NativeArray<float3>(_totalWheels, Allocator.Persistent);
            _wheelGroundPosition = new NativeArray<float3>(_totalWheels, Allocator.Persistent);
            _wheelNormalDirection = new NativeArray<float3>(_totalWheels, Allocator.Persistent);
            _wheelRightDirection = new NativeArray<float3>(_totalWheels, Allocator.Persistent);
            _wheelRaycatsDistance = new NativeArray<float>(_totalWheels, Allocator.Persistent);
            _wheelRadius = new NativeArray<float>(_totalWheels, Allocator.Persistent);
            _wheelAssociatedCar = new NativeArray<int>(_totalWheels, Allocator.Persistent);
            _wheelCanSteer = new NativeArray<bool>(_totalWheels, Allocator.Persistent);
            _wheelSuspensionForce = new NativeArray<float3>(_totalWheels, Allocator.Persistent);
            _wheelMaxSuspension = new NativeArray<float>(_totalWheels, Allocator.Persistent);
            _wheelSpringStiffness = new NativeArray<float>(_totalWheels, Allocator.Persistent);
            _wheelRaycatsResult = new NativeArray<RaycastHit>(_totalWheels, Allocator.Persistent);
            _wheelRaycastCommand = new NativeArray<RaycastCommand>(_totalWheels, Allocator.Persistent);
            _wheelSpringForce = new NativeArray<float>(_totalWheels, Allocator.Persistent);
            _wheelSideForce = new NativeArray<float3>(_totalWheels, Allocator.Persistent);

            _vehicleTrigger = new TransformAccessArray(nrOfVehicles);
            _vehicleSpecialDriveAction = new NativeArray<TrafficSystem.DriveActions>(nrOfVehicles, Allocator.Persistent);
            _vehicleType = new NativeArray<VehicleTypes>(nrOfVehicles, Allocator.Persistent);
            _vehicleForwardForce = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _trailerForwardForce = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);

            _vehiclePosition = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _vehicleGroundDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _vehicleDownDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _vehicleRightDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _trailerRightDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _vehicleForwardDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _trailerForwardDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _triggerForwardDirection = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _vehicleTargetWaypointPosition = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _vehicleVelocity = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _trailerVelocity = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);
            _closestObstacle = new NativeArray<float3>(nrOfVehicles, Allocator.Persistent);

            _wheelRotation = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _turnAngle = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleDrag = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _massDifference = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _trailerDrag = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleSteeringStep = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleDistanceToStop = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleMaxSpeed = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleLength = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleWheelDistance = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehiclePowerStep = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleBrakeStep = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _raycastLengths = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _wCircumferences = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleRotationAngle = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleMaxSteer = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);
            _vehicleActionValue = new NativeArray<float>(nrOfVehicles, Allocator.Persistent);

            _wheelSign = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);
            _vehicleListIndex = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);
            _vehicleEndWheelIndex = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);
            _vehicleStartWheelIndex = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);
            _vehicleNrOfWheels = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);
            _trailerNrWheels = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);

            _vehicleReadyToRemove = new NativeArray<bool>(nrOfVehicles, Allocator.Persistent);
            _vehicleNeedWaypoint = new NativeArray<bool>(nrOfVehicles, Allocator.Persistent);
            _vehicleIsBraking = new NativeArray<bool>(nrOfVehicles, Allocator.Persistent);
            _ignoreVehicle = new NativeArray<bool>(nrOfVehicles, Allocator.Persistent);
            _vehicleGear = new NativeArray<int>(nrOfVehicles, Allocator.Persistent);

            _vehicleRigidbody = new Rigidbody[nrOfVehicles];
            _trailerRigidbody = new Dictionary<int, Rigidbody>();

            //initialize other managers
            _activeCameraPositions = new NativeArray<float3>(activeCameras.Length, Allocator.Persistent);
            for (int i = 0; i < _activeCameraPositions.Length; i++)
            {
                _activeCameraPositions[i] = activeCameras[i].position;
            }

            if (trafficOptions.distanceToRemove < 0)
            {
                float cellSize = GridDataHandler.GetCellSize();
                trafficOptions.distanceToRemove = 2 * cellSize + cellSize / 2;
            }

            if (trafficOptions.minDistanceToAdd < 0)
            {
                float cellSize = GridDataHandler.GetCellSize();
                trafficOptions.minDistanceToAdd = cellSize + cellSize / 2;
            }

            _distanceToRemove = trafficOptions.distanceToRemove * trafficOptions.distanceToRemove;
            _minDistanceToAdd = trafficOptions.minDistanceToAdd;

            _timeManager = new TimeManager();
            bool debugDensity = false;
            bool debugGiveWay = false;
#if UNITY_EDITOR
            var debugSettings = DebugOptions.LoadOrCreateDebugSettings();
            debugDensity = debugSettings.debugDensity;
            debugGiveWay = debugSettings.DebugGiveWay;
#endif

            var positionValidator = new PositionValidator(_activeCameras, layerSetup.trafficLayers, layerSetup.playerLayers, layerSetup.buildingsLayers, _minDistanceToAdd, debugDensity);

            _waypointManager = new WaypointManager(_trafficWaypointsDataHandler, GridDataHandler, nrOfVehicles, trafficOptions.SpawnWaypointSelector, debugGiveWay);

            // Load play mode intersections.
            var allIntersectionsData = new AllIntersectionsData(IntersectionsDataHandler, TrafficWaypointsDataHandler, pedestrianWaypointsDataHandler, trafficOptions.TrafficLightsBehaviour, trafficOptions.greenLightTime, trafficOptions.yellowLightTime);
            _allIntersectionsHandler = new AllIntersectionsDataHandler(allIntersectionsData);

            _vehiclePositioningSystem = new VehiclePositioningSystem(nrOfVehicles, WaypointManager, TrafficWaypointsDataHandler);

            _drivingAI = new DrivingAI(nrOfVehicles, WaypointManager, TrafficWaypointsDataHandler, AllVehiclesDataHandler, VehiclePositioningSystem, positionValidator,
                trafficOptions.PlayerInTrigger, trafficOptions.DynamicObstacleInTrigger, trafficOptions.BuildingInTrigger, trafficOptions.VehicleCrash);

            //initialize all vehicles
            var tempWheelOrigin = new Transform[_totalWheels];
            var tempWheelGraphic = new Transform[_totalWheels];
            int wheelIndex = 0;
            for (int i = 0; i < nrOfVehicles; i++)
            {
                VehicleComponent vehicle = AllVehiclesDataHandler.GetVehicle(i);
                VehiclePositioningSystem.AddCar(vehicle.GetFrontAxle());
                _vehicleTrigger.Add(vehicle.frontTrigger);
                _vehicleRigidbody[i] = vehicle.rb;
                _vehicleSteeringStep[i] = vehicle.GetSteeringStep();
                _vehicleDistanceToStop[i] = vehicle.distanceToStop;
                _vehicleWheelDistance[i] = vehicle.wheelDistance;
                _vehicleDrag[i] = vehicle.rb.drag;

                _raycastLengths[i] = vehicle.GetRaycastLength();
                _wCircumferences[i] = vehicle.GetWheelCircumference();
                _vehicleMaxSteer[i] = vehicle.MaxSteer;
                _vehicleStartWheelIndex[i] = wheelIndex;
                _vehicleNrOfWheels[i] = vehicle.GetNrOfWheels();
                _vehicleEndWheelIndex[i] = _vehicleStartWheelIndex[i] + _vehicleNrOfWheels[i];
                _trailerNrWheels[i] = vehicle.GetTrailerWheels();

                _vehicleLength[i] = vehicle.length;

                for (int j = 0; j < _vehicleNrOfWheels[i]; j++)
                {
                    tempWheelOrigin[wheelIndex] = vehicle.allWheels[j].wheelTransform;
                    tempWheelGraphic[wheelIndex] = vehicle.allWheels[j].wheelTransform.GetChild(0);
                    _wheelCanSteer[wheelIndex] = vehicle.allWheels[j].wheelPosition == Wheel.WheelPosition.Front;
                    _wheelRadius[wheelIndex] = vehicle.allWheels[j].wheelRadius;
                    _wheelMaxSuspension[wheelIndex] = vehicle.allWheels[j].maxSuspension;
                    _wheelSpringStiffness[wheelIndex] = vehicle.GetSpringStiffness();
                    _wheelAssociatedCar[wheelIndex] = i;
                    _wheelSpringForce[wheelIndex] = vehicle.SpringForce;
                    wheelIndex++;
                }
                if (vehicle.trailer != null)
                {
                    TrailerComponent trailer = vehicle.trailer;
                    for (int j = 0; j < vehicle.trailer.GetNrOfWheels(); j++)
                    {
                        tempWheelOrigin[wheelIndex] = trailer.allWheels[j].wheelTransform;
                        tempWheelGraphic[wheelIndex] = trailer.allWheels[j].wheelTransform.GetChild(0);
                        _wheelCanSteer[wheelIndex] = false;
                        _wheelRadius[wheelIndex] = trailer.allWheels[j].wheelRadius;
                        _wheelMaxSuspension[wheelIndex] = trailer.allWheels[j].maxSuspension;
                        _wheelSpringStiffness[wheelIndex] = trailer.GetSpringStiffness();
                        _wheelAssociatedCar[wheelIndex] = i;
                        _wheelSpringForce[wheelIndex] = trailer.GetSpringForce();
                        wheelIndex++;
                    }
                    _vehicleEndWheelIndex[i] += trailer.GetNrOfWheels();
                    _trailerDrag[i] = trailer.rb.drag;
                    _massDifference[i] = (trailer.rb.mass / vehicle.rb.mass) * (trailer.joint.connectedMassScale / trailer.joint.massScale);
                    _trailerRigidbody.Add(i, trailer.rb);
                }

                _vehicleListIndex[i] = vehicle.ListIndex;
                _vehicleType[i] = vehicle.VehicleType;
            }

            _suspensionConnectPoints = new TransformAccessArray(tempWheelOrigin);
            _wheelsGraphics = new TransformAccessArray(tempWheelGraphic);

            //set the number of jobs based on processor count
            if (SystemInfo.processorCount != 0)
            {
                _nrOfJobs = _totalWheels / SystemInfo.processorCount + 1;
            }
            else
            {
                _nrOfJobs = nrOfVehicles / 4;
            }

            //add events
            AIEvents.onChangeDrivingState += UpdateDrivingState;
            AIEvents.onChangeDestination += DestinationChanged;
            Events.onVehicleAdded += NewVehicleAdded;

            //initialize the remaining managers
            var activeIntersectionManager = new ActiveIntersectionsManager(AllIntersectionsHandler);

            _densityManager = new DensityManager(AllVehiclesDataHandler, WaypointManager, TrafficWaypointsDataHandler, GridDataHandler, positionValidator, _activeCameraPositions, nrOfVehicles, activeCameras[0].position, activeCameras[0].forward, _activeSquaresLevel, trafficOptions.useWaypointPriority, trafficOptions.initialDensity, trafficOptions.disableWaypointsArea, debugDensity);

            _intersectionManager = new IntersectionManager();

            _activeCellsManager = new ActiveCellsManager(_activeCameraPositions, GridDataHandler, trafficOptions.activeSquaresLevel);

            if (TrafficModules.PathFinding)
            {
                _pathFindingManager = new PathFindingManager(GridDataHandler, _pathFindingDataHandler);
            }
#if UNITY_EDITOR
            _debugManager = new DebugManager(debugSettings, AllVehiclesDataHandler, WaypointManager, DrivingAI, _pathFindingDataHandler, AllIntersectionsHandler, TrafficWaypointsDataHandler, VehiclePositioningSystem, GridDataHandler);
#endif
            _initialized = true;
        }
        #endregion


        #region API Methods
        internal bool IsInitialized()
        {
            return _initialized;
        }


        internal void ClearPathForSpecialVehicles(bool active, RoadSide side)
        {
            _clearPath = active;
            if (side == RoadSide.Any)
            {
                side = RoadSide.Right;
            }
            this._side = side;
            for (int i = 0; i < _nrOfVehicles; i++)
            {
                if (_vehicleRigidbody[i].gameObject.activeSelf)
                {
                    DrivingAI.ChangeLane(active, i, side);

                    if (_clearPath == false)
                    {
                        AllVehiclesDataHandler.ResetMaxSpeed(i);
                        AIEvents.TriggerChangeDestinationEvent(i);
                    }
                }

            }
        }


        /// <summary>
        /// Removes the vehicles on a given circular area
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        internal void ClearTrafficOnArea(Vector3 center, float radius)
        {
            if (!_initialized)
                return;

            float sqrRadius = radius * radius;
            for (int i = 0; i < _vehiclePosition.Length; i++)
            {
                if (_vehicleRigidbody[i].gameObject.activeSelf)
                {
                    //uses math because of the float3 array
                    if (math.distancesq(center, _vehiclePosition[i]) < sqrRadius)
                    {
                        RemoveVehicle(i, true);
                    }
                }
            }
        }


        /// <summary>
        /// Remove a specific vehicle from the scene
        /// </summary>
        /// <param name="index">index of the vehicle to remove</param>
        internal void RemoveVehicle(GameObject vehicle)
        {
            if (!_initialized)
                return;

            int index = AllVehiclesDataHandler.GetVehicleIndex(vehicle);
            if (index != -1)
            {
                RemoveVehicle(index, true);
            }
            else
            {
                Debug.Log("Vehicle not found");
            }
        }


        /// <summary>
        /// Update active camera that is used to remove vehicles when are not in view
        /// </summary>
        /// <param name="activeCamera">represents the camera or the player prefab</param>
        internal void UpdateCamera(Transform[] activeCameras)
        {
            if (!_initialized)
                return;

            if (activeCameras.Length != _activeCameraPositions.Length)
            {
                _activeCameraPositions = new NativeArray<float3>(activeCameras.Length, Allocator.Persistent);
            }

            this._activeCameras = activeCameras;
            DensityManager.UpdateCameraPositions(activeCameras);

        }


        internal void SetActiveSquaresLevel(int activeSquaresLevel)
        {
            if (!_initialized)
                return;

            this._activeSquaresLevel = activeSquaresLevel;
            DensityManager.UpdateActiveSquares(activeSquaresLevel);
        }


        internal void StopVehicleDriving(GameObject vehicle)
        {
            if (!_initialized)
                return;

            int vehicleIndex = AllVehiclesDataHandler.GetVehicleIndex(vehicle);
            if (vehicleIndex >= 0)
            {
                _ignoreVehicle[vehicleIndex] = true;
            }
        }


        internal void AddVehicleWithPath(Vector3 position, VehicleTypes vehicleType, Vector3 destination, UnityAction<VehicleComponent, int> completeMethod)
        {
            List<int> path = PathFindingManager.GetPath(position, destination, vehicleType);
            if (path != null)
            {
                //aici tre sa vina un callback
                DensityManager.AddVehicleAtPosition(position, vehicleType, completeMethod, path);
            }
        }


        internal void SetDestination(int vehicleIndex, Vector3 position)
        {
            if (PathFindingManager != null)
            {
                var path = PathFindingManager.GetPathToDestination(vehicleIndex, WaypointManager.GetTargetWaypointIndex(vehicleIndex), position, AllVehiclesDataHandler.GetVehicleType(vehicleIndex));
                if (path != null)
                {
                    WaypointManager.SetAgentPath(vehicleIndex, new Queue<int>(path));
                }
            }
        }
        #endregion


        #region EventHandlers
        /// <summary>
        /// Called every time a new vehicle is enabled
        /// </summary>
        /// <param name="vehicleIndex">index of the vehicle</param>
        /// <param name="targetWaypointPosition">target position</param>
        /// <param name="maxSpeed">max possible speed</param>
        /// <param name="powerStep">acceleration power</param>
        /// <param name="brakeStep">brake power</param>
        private void NewVehicleAdded(int vehicleIndex)
        {
            //set new vehicle parameters
            _vehicleTargetWaypointPosition[vehicleIndex] = TrafficWaypointsDataHandler.GetPosition(WaypointManager.GetTargetWaypointIndex(vehicleIndex));

            _vehiclePowerStep[vehicleIndex] = AllVehiclesDataHandler.GetPowerStep(vehicleIndex);
            _vehicleBrakeStep[vehicleIndex] = AllVehiclesDataHandler.GetBrakeStep(vehicleIndex);

            _vehicleIsBraking[vehicleIndex] = false;
            _vehicleNeedWaypoint[vehicleIndex] = false;
            _ignoreVehicle[vehicleIndex] = false;
            _vehicleGear[vehicleIndex] = 1;
            _turnAngle[vehicleIndex] = 0;

            //reset AI
            DrivingAI.VehicleActivated(vehicleIndex);
            _vehicleMaxSpeed[vehicleIndex] = DrivingAI.GetMaxSpeedMS(vehicleIndex);
            _vehicleLength[vehicleIndex] = AllVehiclesDataHandler.GetVehicleLength(vehicleIndex);

            //set initial velocity
            _vehicleRigidbody[vehicleIndex].velocity = VehiclePositioningSystem.GetForwardVector(vehicleIndex) * _vehicleMaxSpeed[vehicleIndex] / 2;
            if (_trailerNrWheels[vehicleIndex] != 0)
            {
                _trailerRigidbody[vehicleIndex].velocity = _vehicleRigidbody[vehicleIndex].velocity;
            }
            //vehicleRigidbody[vehicleIndex].velocity = Vector3.zero;
        }


        /// <summary>
        /// Remove a specific vehicle from the scene
        /// </summary>
        /// <param name="vehicleIndex">index of the vehicle to remove</param>
        internal void RemoveVehicle(int vehicleIndex, bool force)
        {
            if (!_initialized)
                return;
            if (WaypointManager.HasPath(vehicleIndex) && force == false)
            {
                return;
            }
            _vehicleReadyToRemove[_indexToRemove] = false;
            int index = _vehicleListIndex[vehicleIndex];
            IntersectionManager.RemoveVehicle(index);
            WaypointManager.RemoveAgent(index);
            DrivingAI.RemoveVehicle(index);
            DensityManager.RemoveVehicle(index);
            _closestObstacle[index] = Vector3.zero;
            Events.TriggerVehicleRemovedEvent(index);
        }


        /// <summary>
        /// Called every time a vehicle state changes
        /// </summary>
        /// <param name="vehicleIndex">vehicle index</param>
        /// <param name="action">new action</param>
        /// <param name="actionValue">time to execute the action</param>
        private void UpdateDrivingState(int vehicleIndex, TrafficSystem.DriveActions action, float actionValue)
        {
            AllVehiclesDataHandler.SetCurrentAction(vehicleIndex, action);
            _vehicleSpecialDriveAction[vehicleIndex] = action;
            _vehicleActionValue[vehicleIndex] = actionValue;
            if (action == TrafficSystem.DriveActions.AvoidReverse)
            {
                _wheelSign[vehicleIndex] = (int)Mathf.Sign(_turnAngle[vehicleIndex]);
            }
        }


        /// <summary>
        /// Called when waypoint changes
        /// </summary>
        /// <param name="vehicleIndex">vehicle index</param>
        /// <param name="targetWaypointPosition">new waypoint position</param>
        /// <param name="maxSpeed">new possible speed</param>
        /// <param name="blinkType">blinking is required or not</param>
        private void DestinationChanged(int vehicleIndex)
        {
            _vehicleNeedWaypoint[vehicleIndex] = false;
            _vehicleTargetWaypointPosition[vehicleIndex] = GetTargetWaypointPosition(vehicleIndex, _clearPath, _side);
            _vehicleMaxSpeed[vehicleIndex] = DrivingAI.GetMaxSpeedMS(vehicleIndex);
        }
        #endregion


        private Vector3 GetTargetWaypointPosition(int vehicleIndex, bool clearPath, RoadSide side)
        {
            if (!clearPath)
            {
                return TrafficWaypointsDataHandler.GetPosition(WaypointManager.GetTargetWaypointIndex(vehicleIndex));
            }
            else
            {
                //offset target position
                int waypointIndex = WaypointManager.GetTargetWaypointIndex(vehicleIndex);
                Vector3 direction;
                if (TrafficWaypointsDataHandler.HasNeighbors(waypointIndex))
                {
                    direction = TrafficWaypointsDataHandler.GetPosition(TrafficWaypointsDataHandler.GetNeighbors(waypointIndex)[0]) - TrafficWaypointsDataHandler.GetPosition(waypointIndex);
                }
                else
                {
                    direction = TrafficWaypointsDataHandler.GetPosition(waypointIndex) - TrafficWaypointsDataHandler.GetPosition(TrafficWaypointsDataHandler.GetPrevs(waypointIndex)[0]);
                }

                Vector3 offsetDirection = Vector3.Cross(direction.normalized, Vector3.up).normalized;

                Vector3 position = TrafficWaypointsDataHandler.GetPosition(WaypointManager.GetTargetWaypointIndex(vehicleIndex));


                float laneWidth = _trafficWaypointsDataHandler.GetLaneWidth(WaypointManager.GetTargetWaypointIndex(vehicleIndex));
                if (laneWidth == 0)
                {
                    laneWidth = 4;
                }
                float halfCarWidth = AllVehiclesDataHandler.GetVehicleWidth(vehicleIndex) / 2;

                float offset = laneWidth / 2 - halfCarWidth;

                if (side == RoadSide.Left)
                {
                    return position + offsetDirection * offset;
                }
                else
                {
                    return position + offsetDirection * -offset;
                }
            }
        }


        private void FixedUpdate()
        {
            if (!_initialized)
                return;

            #region Suspensions

            //for each wheel check where the ground is by performing a RayCast downwards using job system
            for (int i = 0; i < _totalWheels; i++)
            {
                _wheelSuspensionPosition[i] = _suspensionConnectPoints[i].position;
                _wheelVelocity[i] = _vehicleRigidbody[_wheelAssociatedCar[i]].GetPointVelocity(_wheelSuspensionPosition[i]);
            }

            for (int i = 0; i < _nrOfVehicles; i++)
            {
                if (_vehicleRigidbody[i].IsSleeping())
                {
                    continue;
                }
                if (_trailerNrWheels[i] > 0)
                {
                    _trailerVelocity[i] = _trailerRigidbody[i].velocity;
                    _trailerForwardDirection[i] = _trailerRigidbody[i].transform.forward;
                    _trailerRightDirection[i] = _trailerRigidbody[i].transform.right;

                }

                _vehicleVelocity[i] = _vehicleRigidbody[i].velocity;

                _vehicleDownDirection[i] = -VehiclePositioningSystem.GetUpVector(i);
                _forward = VehiclePositioningSystem.GetForwardVector(i);
                _forward.y = 0;
                _vehicleForwardDirection[i] = _forward;
                _vehicleRightDirection[i] = VehiclePositioningSystem.GetRightVector(i);
                _vehiclePosition[i] = VehiclePositioningSystem.GetPosition(i);
                _vehicleGroundDirection[i] = AllVehiclesDataHandler.GetGroundDirection(i);
                _triggerForwardDirection[i] = _vehicleTrigger[i].transform.forward;
                _closestObstacle[i] = AllVehiclesDataHandler.GetClosestObstacle(i);

                //adapt speed to the front vehicle
                if (_vehicleSpecialDriveAction[i] == TrafficSystem.DriveActions.Overtake || _vehicleSpecialDriveAction[i] == TrafficSystem.DriveActions.Follow)
                {
                    _vehicleMaxSpeed[i] = DrivingAI.GetMaxSpeedMS(i);
                    if (_vehicleMaxSpeed[i] < 2)
                    {
                        DrivingAI.AddDriveAction(i, TrafficSystem.DriveActions.StopInDistance);
                    }
                }
            }

            for (int i = 0; i < _totalWheels; i++)
            {
                if (_vehicleRigidbody[_wheelAssociatedCar[i]].IsSleeping())
                {
                    continue;
                }
#if UNITY_2022_2_OR_NEWER
                _wheelRaycastCommand[i] = new RaycastCommand(_wheelSuspensionPosition[i], _vehicleDownDirection[_wheelAssociatedCar[i]], new QueryParameters(layerMask: _roadLayers), _raycastLengths[_wheelAssociatedCar[i]]);
#else
                _wheelRaycastCommand[i] = new RaycastCommand(_wheelSuspensionPosition[i], _vehicleDownDirection[_wheelAssociatedCar[i]], _raycastLengths[_wheelAssociatedCar[i]], _roadLayers);
#endif
            }
            _raycastJobHandle = RaycastCommand.ScheduleBatch(_wheelRaycastCommand, _wheelRaycatsResult, _nrOfJobs, default);
            _raycastJobHandle.Complete();

            for (int i = 0; i < _totalWheels; i++)
            {
                if (_vehicleRigidbody[_wheelAssociatedCar[i]].IsSleeping())
                {
                    continue;
                }

                _wheelRaycatsDistance[i] = _wheelRaycatsResult[i].distance;
                _wheelNormalDirection[i] = _wheelRaycatsResult[i].normal;
                _wheelGroundPosition[i] = _wheelRaycatsResult[i].point;
            }
            #endregion

            #region Driving

            //execute job for wheel turn and driving
            _wheelJob = new WheelJob()
            {
                WheelSuspensionForce = _wheelSuspensionForce,
                SpringForces = _wheelSpringForce,
                WheelMaxSuspension = _wheelMaxSuspension,
                WheelRayCastDistance = _wheelRaycatsDistance,
                WheelRadius = _wheelRadius,
                WheelNormalDirection = _wheelNormalDirection,
                NrOfVehicleWheels = _vehicleEndWheelIndex,
                StartWheelIndex = _vehicleStartWheelIndex,
                WheelAssociatedVehicle = _wheelAssociatedCar,
                WheelSideForce = _wheelSideForce,
                VehicleNrOfWheels = _vehicleNrOfWheels,
                WheelVelocity = _wheelVelocity,
                WheelRightDirection = _wheelRightDirection,
                SpringStiffness = _wheelSpringStiffness,
            };

            _driveJob = new DriveJob()
            {
                WheelCircumferences = _wCircumferences,
                CarVelocity = _vehicleVelocity,
                FixedDeltaTime = Time.fixedDeltaTime,
                TargetWaypointPosition = _vehicleTargetWaypointPosition,
                AllBotsPosition = _vehiclePosition,
                MaxSteer = _vehicleMaxSteer,
                ForwardDirection = _vehicleForwardDirection,
                WorldUp = _up,
                WheelRotation = _wheelRotation,
                TurnAngle = _turnAngle,
                VehicleRotationAngle = _vehicleRotationAngle,
                ReadyToRemove = _vehicleReadyToRemove,
                NeedsWaypoint = _vehicleNeedWaypoint,
                DistanceToRemove = _distanceToRemove,
                CameraPositions = _activeCameraPositions,
                BodyForce = _vehicleForwardForce,
                DownDirection = _vehicleDownDirection,
                RightDirection = _vehicleRightDirection,
                PowerStep = _vehiclePowerStep,
                BrakeStep = _vehicleBrakeStep,
                SpecialDriveAction = _vehicleSpecialDriveAction,
                ActionValue = _vehicleActionValue,
                WheelSign = _wheelSign,
                IsBraking = _vehicleIsBraking,
                Drag = _vehicleDrag,
                MaxSpeed = _vehicleMaxSpeed,
                Gear = _vehicleGear,
                GroundDirection = _vehicleGroundDirection,
                SteeringStep = _vehicleSteeringStep,
                WheelDistance = _vehicleWheelDistance,
                ClosestObstacle = _closestObstacle,
                VehicleLength = _vehicleLength,
                NrOfWheels = _vehicleNrOfWheels,
                TrailerVelocity = _trailerVelocity,
                TrailerForce = _trailerForwardForce,
                TrailerForwardDirection = _trailerForwardDirection,
                TrailerRightDirection = _trailerRightDirection,
                TrailerNrOfWheels = _trailerNrWheels,
                MassDifference = _massDifference,
                TrailerDrag = _trailerDrag,
                TriggerForwardDirection = _triggerForwardDirection,
                DistanceToStop = _vehicleDistanceToStop,

            };

            _wheelJobHandle = _wheelJob.Schedule(_totalWheels, _nrOfJobs);
            _driveJobHandle = _driveJob.Schedule(_nrOfVehicles, _nrOfJobs);
            _wheelJobHandle.Complete();
            _driveJobHandle.Complete();

            //store job values
            _wheelSuspensionForce = _wheelJob.WheelSuspensionForce;
            _wheelSideForce = _wheelJob.WheelSideForce;
            _wheelRotation = _driveJob.WheelRotation;
            _turnAngle = _driveJob.TurnAngle;
            _vehicleRotationAngle = _driveJob.VehicleRotationAngle;
            _vehicleReadyToRemove = _driveJob.ReadyToRemove;
            _vehicleNeedWaypoint = _driveJob.NeedsWaypoint;
            _vehicleForwardForce = _driveJob.BodyForce;
            _vehicleActionValue = _driveJob.ActionValue;
            _vehicleIsBraking = _driveJob.IsBraking;
            _vehicleGear = _driveJob.Gear;
            _trailerForwardForce = _driveJob.TrailerForce;


            //make vehicle actions based on job results
            for (int i = 0; i < _nrOfVehicles; i++)
            {
                if (!_vehicleRigidbody[i].IsSleeping())
                {
                    int groundedWheels = 0;
                    for (int j = _vehicleStartWheelIndex[i]; j < _vehicleEndWheelIndex[i] - _trailerNrWheels[i]; j++)
                    {
                        if (_wheelRaycatsDistance[j] != 0)
                        {
                            groundedWheels++;

                            //apply suspension
                            _vehicleRigidbody[i].AddForceAtPosition(_wheelSuspensionForce[j], _wheelGroundPosition[j]);

                            //apply friction
                            _vehicleRigidbody[i].AddForceAtPosition(_wheelSideForce[j], _wheelSuspensionPosition[j], ForceMode.VelocityChange);
                            if (_ignoreVehicle[i] == false)
                            {
                                //apply traction
                                _vehicleRigidbody[i].AddForceAtPosition(_vehicleForwardForce[i], _wheelGroundPosition[j], ForceMode.VelocityChange);
                            }
                        }
                        else
                        {
                            //if the wheel is not grounded apply additional gravity to stabilize the vehicle for a more realistic movement
                            _vehicleRigidbody[i].AddForceAtPosition(Physics.gravity * _vehicleRigidbody[i].mass / (_vehicleEndWheelIndex[i] - _vehicleStartWheelIndex[i]), _wheelSuspensionPosition[j]);
                        }
                    }

                    //TODO Change this
                    if (_trailerNrWheels[i] > 0)
                    {
                        for (int j = _vehicleEndWheelIndex[i] - _trailerNrWheels[i]; j < _vehicleEndWheelIndex[i]; j++)
                        {
                            if (_wheelRaycatsDistance[j] != 0)
                            {
                                //if wheel is grounded apply suspension force
                                _trailerRigidbody[i].AddForceAtPosition(_wheelSuspensionForce[j], _wheelGroundPosition[j]);

                                //apply side friction
                                _trailerRigidbody[i].AddForceAtPosition(_trailerForwardForce[i], _wheelSuspensionPosition[j], ForceMode.VelocityChange);

                                if (_vehicleIsBraking[i])
                                {
                                    _trailerRigidbody[i].AddForceAtPosition(_vehicleForwardForce[i], _wheelSuspensionPosition[j], ForceMode.VelocityChange);
                                }
                            }
                            else
                            {
                                //if the wheel is not grounded apply additional gravity to stabilize the vehicle for a more realistic movement
                                _trailerRigidbody[i].AddForceAtPosition(Physics.gravity * _trailerRigidbody[i].mass / _trailerNrWheels[i], _wheelSuspensionPosition[j]);
                            }
                        }
                    }

                    if (_ignoreVehicle[i] == true)
                        continue;

                    //apply rotation 
                    if (groundedWheels != 0)
                    {
                        _vehicleRigidbody[i].MoveRotation(_vehicleRigidbody[i].rotation * Quaternion.Euler(0, _vehicleRotationAngle[i], 0));
                    }
                    //request new waypoint if needed
                    if (_vehicleNeedWaypoint[i] == true)
                    {
                        if (_clearPath)
                        {
                            DrivingAI.AddDriveAction(i, TrafficSystem.DriveActions.ChangeLane, false, _side);
                        }
                        DrivingAI.WaypointRequested(i, _vehicleType[i], _clearPath);
                    }

                    //if current action is finished set a new action
                    if (_vehicleActionValue[i] < 0)
                    {
                        DrivingAI.TimedActionEnded(i);
                    }
                    //update reverse lights
                    if (_vehicleGear[i] < 0)
                    {
                        AllVehiclesDataHandler.SetReverseLights(i, true);
                    }
                    else
                    {
                        AllVehiclesDataHandler.SetReverseLights(i, false);
                    }

                    //update engine and lights components
                    AllVehiclesDataHandler.UpdateVehicleScripts(i, SoundManager.MasterVolume, TimeManager.RealTimeSinceStartup);
                }
            }
            #endregion
        }


        private void Update()
        {
            if (!_initialized)
                return;

            TimeManager.UpdateTime();

            //update brake lights
            for (int i = 0; i < _nrOfVehicles; i++)
            {
                AllVehiclesDataHandler.SetBrakeLights(i, _vehicleIsBraking[i]);
            }

            #region WheelUpdate
            //update wheel graphics
            for (int i = 0; i < _totalWheels; i++)
            {
                _wheelSuspensionPosition[i] = _suspensionConnectPoints[i].position;
                _wheelRightDirection[i] = _suspensionConnectPoints[i].right;
            }

            _updateWheelJob = new UpdateWheelJob()
            {
                WheelsOrigin = _wheelSuspensionPosition,
                DownDirection = _vehicleDownDirection,
                WheelRotation = _wheelRotation,
                TurnAngle = _turnAngle,
                WheelRadius = _wheelRadius,
                MaxSuspension = _wheelMaxSuspension,
                RayCastDistance = _wheelRaycatsDistance,
                NrOfVehicles = _nrOfVehicles,
                CanSteer = _wheelCanSteer,
                VehicleIndex = _wheelAssociatedCar
            };
            _updateWheelJobHandle = _updateWheelJob.Schedule(_wheelsGraphics);
            _updateWheelJobHandle.Complete();
            #endregion

            #region TriggerUpdate
            //update trigger orientation
            _updateTriggerJob = new UpdateTriggerJob()
            {
                TurnAngle = _turnAngle,
            };
            _updateTriggerJobHandle = _updateTriggerJob.Schedule(_vehicleTrigger);
            _updateTriggerJobHandle.Complete();
            #endregion

            #region RemoveVehicles
            //remove vehicles that are too far away and not in view
            _indexToRemove++;
            if (_indexToRemove == _nrOfVehicles)
            {
                _indexToRemove = 0;
            }
            _activeCameraIndex = UnityEngine.Random.Range(0, _activeCameraPositions.Length);
            DensityManager.UpdateVehicleDensity(_activeCameras[_activeCameraIndex].position, _activeCameras[_activeCameraIndex].forward, _activeCameraPositions[_activeCameraIndex]);


            if (_vehicleReadyToRemove[_indexToRemove] == true)
            {

                if (_vehicleRigidbody[_indexToRemove].gameObject.activeSelf)
                {
                    if (AllVehiclesDataHandler.CanBeRemoved(_vehicleListIndex[_indexToRemove]) == true)
                    {
                        RemoveVehicle(_indexToRemove, false);
                    }
                }
            }
            #endregion

            //update additional managers
            for (int i = 0; i < _activeCameras.Length; i++)
            {
                _activeCameraPositions[i] = _activeCameras[i].transform.position;
            }
            IntersectionManager.UpdateIntersections(TimeManager.RealTimeSinceStartup);
            ActiveCellsManager.UpdateGrid(_activeSquaresLevel, _activeCameraPositions);

            #region Debug
#if UNITY_EDITOR
            DebugManager.Update(_nrOfVehicles, _totalWheels, _wheelSuspensionPosition, _wheelSuspensionForce, _wheelAssociatedCar);
#endif
            #endregion
        }

        #region Cleanup
        /// <summary>
        /// Cleanup
        /// </summary>
        private void OnDestroy()
        {
            try
            {
                _wheelSpringForce.Dispose();
                _raycastLengths.Dispose();
                _wCircumferences.Dispose();
                _wheelRadius.Dispose();
                _vehicleVelocity.Dispose();
                _trailerVelocity.Dispose();
                _vehicleMaxSteer.Dispose();
                _suspensionConnectPoints.Dispose();
                _wheelsGraphics.Dispose();
                _wheelGroundPosition.Dispose();
                _wheelVelocity.Dispose();
                _wheelRotation.Dispose();
                _turnAngle.Dispose();
                _wheelRaycatsResult.Dispose();
                _wheelRaycastCommand.Dispose();
                _wheelCanSteer.Dispose();
                _wheelAssociatedCar.Dispose();
                _vehicleEndWheelIndex.Dispose();
                _vehicleStartWheelIndex.Dispose();
                _vehicleNrOfWheels.Dispose();
                _trailerNrWheels.Dispose();
                _vehicleDownDirection.Dispose();
                _vehicleForwardDirection.Dispose();
                _trailerForwardDirection.Dispose();
                _vehicleRotationAngle.Dispose();
                _vehicleRightDirection.Dispose();
                _vehicleTargetWaypointPosition.Dispose();
                _vehiclePosition.Dispose();
                _vehicleGroundDirection.Dispose();
                _vehicleReadyToRemove.Dispose();
                _vehicleListIndex.Dispose();
                _vehicleNeedWaypoint.Dispose();
                _wheelRaycatsDistance.Dispose();
                _wheelRightDirection.Dispose();
                _wheelNormalDirection.Dispose();
                _wheelMaxSuspension.Dispose();
                _wheelSuspensionForce.Dispose();
                _vehicleForwardForce.Dispose();
                _wheelSideForce.Dispose();
                _vehicleSteeringStep.Dispose();
                _vehicleGear.Dispose();
                _vehicleDrag.Dispose();
                _vehicleMaxSpeed.Dispose();
                _vehicleLength.Dispose();
                _vehicleWheelDistance.Dispose();
                _vehiclePowerStep.Dispose();
                _vehicleBrakeStep.Dispose();
                _vehicleTrigger.Dispose();
                _vehicleSpecialDriveAction.Dispose();
                _vehicleType.Dispose();
                _vehicleActionValue.Dispose();
                _wheelSign.Dispose();
                _vehicleIsBraking.Dispose();
                _ignoreVehicle.Dispose();
                _activeCameraPositions.Dispose();
                _closestObstacle.Dispose();
                _trailerForwardForce.Dispose();
                _trailerRightDirection.Dispose();
                _trailerDrag.Dispose();
                _massDifference.Dispose();
                _triggerForwardDirection.Dispose();
                _wheelSuspensionPosition.Dispose();
                _vehicleDistanceToStop.Dispose();
                _wheelSpringStiffness.Dispose();
            }
            catch { }

            AIEvents.onChangeDrivingState -= UpdateDrivingState;
            AIEvents.onChangeDestination -= DestinationChanged;
            Events.onVehicleAdded -= NewVehicleAdded;

            DestroyableManager.Instance.DestroyAll();
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!IsInitialized())
                return;
            DebugManager.DrawGizmos();
        }
#endif
    }
}
#endif