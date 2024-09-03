using System.Collections.Generic;
using UnityEngine;
using Gley.UrbanSystem.Internal;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Decides what the vehicle will do next based on received information
    /// </summary>
    internal class DrivingAI : IDestroyable
    {
        private readonly ActiveActions[] _driveActions;
        private readonly BlinkType[] _blinkTypes;
        private readonly float[] _waypointSpeed;
        private readonly DriveActions[] _currentActiveAction;
        private readonly DriveActions[] _movingActions = new DriveActions[]
        {
            DriveActions.AvoidReverse,
            DriveActions.Reverse,
            DriveActions.StopInDistance,
            DriveActions.Follow,
            DriveActions.Overtake
        };

        private readonly WaypointManager _waypointManager;
        private readonly TrafficWaypointsDataHandler _trafficWaypointsDataHandler;
        private readonly AllVehiclesDataHandler _allVehiclesDataHandler;
        private readonly VehiclePositioningSystem _vehiclePositioningSystem;
        private readonly PositionValidator _positionValidator;

        private PlayerInTrigger _playerInTrigger;
        private DynamicObstacleInTrigger _dynamicObstacleInTrigger;
        private BuildingInTrigger _buildingInTrigger;
        private VehicleCrash _vehicleCrash;


        /// <summary>
        /// Initialize Driving AI
        /// </summary>
        internal DrivingAI(int nrOfVehicles, WaypointManager waypointManager, TrafficWaypointsDataHandler trafficWaypointsDataHandler, AllVehiclesDataHandler trafficVehicles, VehiclePositioningSystem vehiclePositioningSystem, PositionValidator positionValidator,
            PlayerInTrigger playerInTrigger, DynamicObstacleInTrigger dynamicObstacleInTrigger, BuildingInTrigger buildingInTrigger, VehicleCrash vehicleCrash)
        {
            _waypointManager = waypointManager;
            _trafficWaypointsDataHandler = trafficWaypointsDataHandler;
            _allVehiclesDataHandler = trafficVehicles;
            _vehiclePositioningSystem = vehiclePositioningSystem;
            _positionValidator = positionValidator;
            _playerInTrigger = playerInTrigger;
            _dynamicObstacleInTrigger = dynamicObstacleInTrigger;
            _buildingInTrigger = buildingInTrigger;

            _driveActions = new ActiveActions[nrOfVehicles];
            _waypointSpeed = new float[nrOfVehicles];
            _blinkTypes = new BlinkType[nrOfVehicles];
            _currentActiveAction = new DriveActions[nrOfVehicles];

            SetVehicleCrashDelegate(vehicleCrash);

            for (int i = 0; i < nrOfVehicles; i++)
            {
                _driveActions[i] = new ActiveActions(new List<DriveAction>());
            }

            Assign();

            //triggered every time a new object is seen by the front trigger
            VehicleEvents.onObjectInTrigger += ObjectInTriggerHandler;
            //triggered every time there are no objects left in trigger
            VehicleEvents.onTriggerCleared += TriggerClearedHandler;
            WaypointEvents.onStopStateChanged += StopStateChangedHandler;
            WaypointEvents.onGiveWayStateChanged += GiveWayStateChangedHandler;
        }


        public void Assign()
        {
            DestroyableManager.Instance.Register(this);
        }


        internal void SetPlayerInTriggerDelegate(PlayerInTrigger newDelegate)
        {
            _playerInTrigger = newDelegate;
        }


        internal void SetDynamicObstacleInTriggerDelegate(DynamicObstacleInTrigger newDelegate)
        {
            _dynamicObstacleInTrigger = newDelegate;
        }


        internal void SetBuildingInTriggerDelegate(BuildingInTrigger newDelegate)
        {
            _buildingInTrigger = newDelegate;
        }


        internal void SetVehicleCrashDelegate(VehicleCrash newDelegate)
        {
            Events.onVehicleCrashed -= _vehicleCrash;
            _vehicleCrash = newDelegate;
            Events.onVehicleCrashed += _vehicleCrash;
        }


        /// <summary>
        /// Reset all pending actions, used when a vehicle is respawned
        /// </summary>
        /// <param name="vehicleIndex"></param>
        internal void VehicleActivated(int vehicleIndex)
        {
            _waypointSpeed[vehicleIndex] = _trafficWaypointsDataHandler.GetMaxSpeed(_waypointManager.GetTargetWaypointIndex(vehicleIndex));
            SetBlinkType(vehicleIndex, BlinkType.Stop, true);
            AIEvents.TriggetChangeDrivingStateEvent(vehicleIndex, _currentActiveAction[vehicleIndex], GetActionValue(_currentActiveAction[vehicleIndex], vehicleIndex));
        }


        internal void RemoveVehicle(int index)
        {
            _driveActions[index] = new ActiveActions(new List<DriveAction>());
            _currentActiveAction[index] = DriveActions.Forward;
        }


        internal void RemoveDriveAction(int index, DriveActions newAction)
        {
            //car is out of trigger -> remove current action
            if (newAction == DriveActions.Continue)
            {
                // remove all active actions
                _driveActions[index].RemoveAll(_movingActions);
            }
            else
            {
                //remove just current action
                _driveActions[index].Remove(newAction);
            }
            ApplyAction(index);
        }


        internal void TimedActionEnded(int index)
        {
            switch (_currentActiveAction[index])
            {
                case DriveActions.Follow:
                    AddDriveAction(index, DriveActions.Overtake);
                    break;
                case DriveActions.Reverse:
                case DriveActions.StopTemp:
                case DriveActions.AvoidReverse:
                    RemoveDriveAction(index, _currentActiveAction[index]);
                    _allVehiclesDataHandler.CurrentVehicleActionDone(index);
                    break;
                default:
                    RemoveDriveAction(index, _currentActiveAction[index]);
                    break;
            }
        }


        internal void WaypointRequested(int vehicleIndex, VehicleTypes vehicleType, bool clearPath)
        {
            int freeWaypointIndex;
            for (int i = 0; i < _driveActions[vehicleIndex].CurrentActiveActions.Count; i++)
            {
                DriveActions activeAction = _driveActions[vehicleIndex].CurrentActiveActions[i].ActionType;

                switch (activeAction)
                {
                    case DriveActions.StopInPoint:
                        //if current action is stop in point -> no new waypoint is needed
                        if (_waypointManager.HasPath(vehicleIndex))
                        {
                            if (_waypointManager.GetPath(vehicleIndex).Count == 0)
                            {
                                Events.TriggerDestinationReachedEvent(vehicleIndex);
                                RemoveDriveAction(vehicleIndex, DriveActions.StopInPoint);
                            }
                        }
                        return;

                    case DriveActions.ChangeLane:
                        //if the current vehicle can overtake
                        if (_driveActions[vehicleIndex].CurrentActiveActions[i].Side == RoadSide.Any)
                        {
                            freeWaypointIndex = _waypointManager.GetOtherLaneWaypointIndex(vehicleIndex, vehicleType);
                        }
                        else
                        {
                            freeWaypointIndex = _waypointManager.GetOtherLaneWaypointIndex(vehicleIndex, vehicleType, _driveActions[vehicleIndex].CurrentActiveActions[i].Side, _allVehiclesDataHandler.GetForwardVector(vehicleIndex));
                        }

                        if (freeWaypointIndex == -1)
                        {
                            //if cannot change lane
                            ContinueStraight(vehicleIndex, vehicleType);
                            if (clearPath)
                            {
                                if (!_trafficWaypointsDataHandler.GetName(_waypointManager.GetTargetWaypointIndex(vehicleIndex)).Contains("Connect"))
                                {
                                    _allVehiclesDataHandler.SetMaxSpeed(vehicleIndex, _allVehiclesDataHandler.GetMaxSpeed(vehicleIndex) * 0.7f);
                                }
                                else
                                {
                                    _allVehiclesDataHandler.ResetMaxSpeed(vehicleIndex);
                                }
                            }
                        }
                        else
                        {
                            if (clearPath)
                            {
                                _allVehiclesDataHandler.ResetMaxSpeed(vehicleIndex);
                            }
                            Blink(BlinkReasons.Overtake, vehicleIndex, freeWaypointIndex);
                            //can overtake, make sure path is free
                            if (AllClear(vehicleIndex, freeWaypointIndex, clearPath))
                            {
                                if (!clearPath)
                                {
                                    RemoveDriveAction(vehicleIndex, DriveActions.ChangeLane);
                                }
                                SetNextWaypoint(vehicleIndex, freeWaypointIndex);
                            }
                            else
                            {
                                ContinueStraight(vehicleIndex, vehicleType);
                            }
                        }
                        return;

                    case DriveActions.Overtake:
                        //if the current vehicle can overtake
                        freeWaypointIndex = _waypointManager.GetOtherLaneWaypointIndex(vehicleIndex, vehicleType);
                        if (freeWaypointIndex == TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
                        {
                            //if cannot change lane
                            ContinueStraight(vehicleIndex, vehicleType);
                        }
                        else
                        {
                            Blink(BlinkReasons.Overtake, vehicleIndex, freeWaypointIndex);
                            //can overtake, make sure path is free
                            if (AllClear(vehicleIndex, freeWaypointIndex, false))
                            {
                                //if can change lane -> start blinking
                                SetNextWaypoint(vehicleIndex, freeWaypointIndex);
                            }
                            else
                            {
                                ContinueStraight(vehicleIndex, vehicleType);
                            }
                        }
                        return;

                    case DriveActions.GiveWay:
                        if (_trafficWaypointsDataHandler.IsInIntersection(_waypointManager.GetTargetWaypointIndex(vehicleIndex)))
                        {
                            if (_waypointManager.CanEnterIntersection(vehicleIndex))
                            {
                                freeWaypointIndex = _waypointManager.GetCurrentLaneWaypointIndex(vehicleIndex, vehicleType);
                                if (freeWaypointIndex != -1)
                                {
                                    RemoveDriveAction(vehicleIndex, DriveActions.GiveWay);
                                    Blink(BlinkReasons.ChangeLane, vehicleIndex, freeWaypointIndex);
                                    SetNextWaypoint(vehicleIndex, freeWaypointIndex);
                                }
                            }
                        }
                        else
                        {
                            int currentWaypointIndex = _waypointManager.GetTargetWaypointIndex(vehicleIndex);
                            if (_trafficWaypointsDataHandler.IsComplexGiveWay(currentWaypointIndex))
                            {
                                freeWaypointIndex = _waypointManager.GetCurrentLaneWaypointIndex(vehicleIndex, vehicleType);
                                if (freeWaypointIndex != TrafficSystemConstants.INVALID_WAYPOINT_INDEX)
                                {
                                    Blink(BlinkReasons.GiveWay, vehicleIndex, freeWaypointIndex);

                                    if (!_waypointManager.AreTheseWaypointsATarget(_trafficWaypointsDataHandler.GetGiveWayWaypointList(currentWaypointIndex)))
                                    {
                                        RemoveDriveAction(vehicleIndex, DriveActions.GiveWay);
                                        SetNextWaypoint(vehicleIndex, freeWaypointIndex);
                                    }
                                }
                                return;
                            }

                            freeWaypointIndex = _waypointManager.GetOtherLaneWaypointIndex(vehicleIndex, vehicleType);
                            if (freeWaypointIndex == -1)
                            {
                                freeWaypointIndex = _waypointManager.GetCurrentLaneWaypointIndex(vehicleIndex, vehicleType);
                            }


                            if (freeWaypointIndex != -1)
                            {
                                Blink(BlinkReasons.GiveWay, vehicleIndex, freeWaypointIndex);
                                if (_trafficWaypointsDataHandler.IsZipperGiveWay(freeWaypointIndex))
                                {
                                    if (!_waypointManager.IsThisWaypointATarget(freeWaypointIndex))
                                    {
                                        RemoveDriveAction(vehicleIndex, DriveActions.GiveWay);
                                        SetNextWaypoint(vehicleIndex, freeWaypointIndex);
                                    }
                                    return;
                                }

                                if (AllClear(vehicleIndex, freeWaypointIndex, clearPath))
                                {
                                    if (_trafficWaypointsDataHandler.HasNeighbors(currentWaypointIndex))
                                    {
                                        RemoveDriveAction(vehicleIndex, DriveActions.GiveWay);
                                    }
                                    SetNextWaypoint(vehicleIndex, freeWaypointIndex);
                                }
                            }
                            else
                            {
                                Blink(BlinkReasons.NoWaypoint, vehicleIndex, freeWaypointIndex);
                                AddDriveAction(vehicleIndex, DriveActions.NoWaypoint);
                            }

                        }
                        //If current vehicle has to give way -> wait until new waypoint is free
                        return;

                }
            }
            //if current vehicle is in no special state -> set next waypoint without any special requirements
            freeWaypointIndex = _waypointManager.GetCurrentLaneWaypointIndex(vehicleIndex, vehicleType);

            if (freeWaypointIndex >= 0)
            {
                Blink(BlinkReasons.None, vehicleIndex, freeWaypointIndex);
                SetNextWaypoint(vehicleIndex, freeWaypointIndex);

                if (!_waypointManager.CanContinueStraight(vehicleIndex, vehicleType))
                {
                    AddDriveAction(vehicleIndex, DriveActions.GiveWay);
                }

                //remove the no waypoint action if waypoints are found -> used for temporary disable waypoints
                if (_driveActions[vehicleIndex].CurrentActiveActions.Count > 0)
                {
                    if (_driveActions[vehicleIndex].CurrentActiveActions[0].ActionType == DriveActions.NoWaypoint)
                    {
                        RemoveDriveAction(vehicleIndex, DriveActions.NoWaypoint);
                    }

                    if (_driveActions[vehicleIndex].CurrentActiveActions[0].ActionType == DriveActions.NoPath)
                    {
                        RemoveDriveAction(vehicleIndex, DriveActions.NoPath);
                    }
                }
            }
            else
            {
                NoWaypointsAvailable(vehicleIndex, freeWaypointIndex);
            }
        }


        internal void SetBlinkType(int vehicleIndex, BlinkType value, bool reset = false)
        {
            if (reset)
            {
                _blinkTypes[vehicleIndex] = BlinkType.Stop;
            }
            if (value == BlinkType.Stop && _blinkTypes[vehicleIndex] == BlinkType.Hazard)
            {
                return;
            }
            if (_blinkTypes[vehicleIndex] != value)
            {

                _blinkTypes[vehicleIndex] = value;
                _allVehiclesDataHandler.SetBlinkLights(vehicleIndex, value);
            }
        }


        internal void SetHazardLights(int vehicleIndex, bool activate)
        {
            if (activate)
            {
                SetBlinkType(vehicleIndex, BlinkType.Hazard);
            }
            else
            {
                SetBlinkType(vehicleIndex, BlinkType.Stop, true);
            }
        }


        internal void ChangeLane(bool active, int vehicleIndex, RoadSide side)
        {
            if (active)
            {
                AddDriveAction(vehicleIndex, DriveActions.ChangeLane, false, side);
            }
            else
            {
                RemoveDriveAction(vehicleIndex, DriveActions.ChangeLane);
            }
        }


        internal void AddDriveAction(int index, DriveActions newAction, bool force = false, RoadSide side = RoadSide.Any)
        {
            if (newAction == DriveActions.ForceForward)
            {
                force = true;
                newAction = DriveActions.Forward;
            }

            if (force)
            {
                _driveActions[index].CurrentActiveActions.Clear();
            }

            //if the new action is not already in the list-> add it in the required position based on priority
            if (!_driveActions[index].Contains(newAction))
            {
                bool added = false;
                for (int i = 0; i < _driveActions[index].CurrentActiveActions.Count; i++)
                {
                    if (_driveActions[index].CurrentActiveActions[i].ActionType < newAction)
                    {
                        _driveActions[index].CurrentActiveActions.Insert(i, new DriveAction(newAction, side));
                        added = true;
                        break;
                    }
                }
                if (added == false)
                {
                    _driveActions[index].Add(new DriveAction(newAction, side));
                }
                ApplyAction(index);
            }

        }


        /// <summary>
        /// Compute current maximum available speed in m/s
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal float GetMaxSpeedMS(int index)
        {
            return ComputeMaxPossibleSpeed(index) / 3.6f;
        }


        internal float GetWaypointSpeed(int vehicleIndex)
        {
            return _waypointSpeed[vehicleIndex];
        }


        /// <summary>
        /// Based on position, heading, and speed decide what is the next action of the vehicle
        /// </summary>
        /// <param name="myIndex"></param>
        /// <param name="otherIndex"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        private DriveActions GetTriggerAction(int myIndex, Collider other)
        {
            VehicleComponent otherVehicle = other.attachedRigidbody.GetComponent<VehicleComponent>();
            int otherIndex = otherVehicle.ListIndex;
            //if it already in other vehicle trigger, stop
            if (otherVehicle.AlreadyCollidingWith(_allVehiclesDataHandler.GetAllColliders(myIndex)))
            {
                if (otherVehicle.CurrentAction != DriveActions.StopTemp && otherVehicle.CurrentAction != DriveActions.StopInDistance && otherVehicle.CurrentAction != DriveActions.GiveWay)
                {
                    return DriveActions.StopTemp;
                }
                else
                {
                    return DriveActions.ForceForward;
                }
            }

            //if reverse is true, means that other car is reversing so I have to reverse
            //if (reverse)
            //{
            //    return SpecialDriveActionTypes.Reverse;
            //}

            bool sameOrientation = _vehiclePositioningSystem.IsSameOrientation(_allVehiclesDataHandler.GetHeading(myIndex), _allVehiclesDataHandler.GetHeading(otherIndex));

            //if other car is stationary
            if (_allVehiclesDataHandler.GetCurrentAction(otherIndex) == DriveActions.StopInDistance || _allVehiclesDataHandler.GetCurrentAction(otherIndex) == DriveActions.StopInPoint || _allVehiclesDataHandler.GetCurrentAction(otherIndex) == DriveActions.GiveWay)
            {
                if (sameOrientation)
                {
                    //if the orientation is the same I stop too
                    return DriveActions.StopInDistance;
                }
            }
            else
            {
                bool sameHeading = _vehiclePositioningSystem.IsSameHeading(_allVehiclesDataHandler.GetForwardVector(otherIndex), _allVehiclesDataHandler.GetForwardVector(myIndex));
                bool otherIsGoingForward = _vehiclePositioningSystem.IsGoingForward(_allVehiclesDataHandler.GetVelocity(otherIndex), _allVehiclesDataHandler.GetHeading(otherIndex));


                if (sameOrientation == false && sameHeading == false)
                {
                    //not same orientation -> going in opposite direction-> try to avoid it
                    //return DriveActions.AvoidForward;
                }
                else
                {
                    //same orientation but different moving direction 
                    if (otherIsGoingForward == false)
                    {
                        // other car is going in reverse so I should also
                        return DriveActions.Reverse;
                    }
                }

                if (sameHeading == false)
                {
                    //going back and hit something -> wait
                    return DriveActions.StopTemp;
                }
                else
                {
                    //follow the car in front
                    if (_allVehiclesDataHandler.GetVelocity(myIndex).sqrMagnitude > 5 && _allVehiclesDataHandler.GetVelocity(otherIndex).sqrMagnitude > 5)
                    {
                        //if the relative angle between the 2 cars is small enough -> follow
                        if (Mathf.Abs(Vector3.SignedAngle(_allVehiclesDataHandler.GetForwardVector(otherIndex), _allVehiclesDataHandler.GetForwardVector(myIndex), Vector3.up)) < 35)
                        {
                            return DriveActions.Follow;
                        }
                    }
                }
                //if nothing worked, stop in distance
                return DriveActions.StopInDistance;
            }
            //continue forward
            return DriveActions.Forward;
        }


        private void ObjectInTriggerHandler(int vehicleIndex, ObstacleTypes obstacleType, Collider other)
        {
            switch (obstacleType)
            {
                case ObstacleTypes.TrafficVehicle:
                    AddDriveAction(vehicleIndex, GetTriggerAction(vehicleIndex, other));
                    break;
                case ObstacleTypes.Player:
                    PlayerInTrigger(vehicleIndex, other);
                    break;
                case ObstacleTypes.DynamicObject:
                    DynamicObjectInTrigger(vehicleIndex, other);
                    break;
                case ObstacleTypes.StaticObject:
                    BuildingObjectInTrigger(vehicleIndex, other);
                    break;
            }
        }


        private void PlayerInTrigger(int vehicleIndex, Collider other)
        {
            _playerInTrigger?.Invoke(vehicleIndex, other);
        }


        private void DynamicObjectInTrigger(int vehicleIndex, Collider other)
        {
            _dynamicObstacleInTrigger?.Invoke(vehicleIndex, other);
        }


        private void BuildingObjectInTrigger(int vehicleIndex, Collider other)
        {
            _buildingInTrigger?.Invoke(vehicleIndex, other);
        }


        /// <summary>
        /// Apply the first action from list
        /// </summary>
        /// <param name="index"></param>
        private void ApplyAction(int index)
        {
            //if trigger is true, other vehicles needs to be alerted that the current action changed
            bool trigger = false;
            if (_driveActions[index].CurrentActiveActions.Count == 0)
            {
                //if list is empty, go forward by default 
                _currentActiveAction[index] = DriveActions.Forward;
                trigger = true;
            }
            else
            {
                if (_currentActiveAction[index] != _driveActions[index].CurrentActiveActions[0].ActionType)
                {
                    trigger = true;
                    _currentActiveAction[index] = _driveActions[index].CurrentActiveActions[0].ActionType;
                }
            }

            //if (currentActiveAction[index] != SpecialDriveActionTypes.Follow && currentActiveAction[index] != SpecialDriveActionTypes.Overtake)
            //{
            //    //reset follow speed if no longer follow a vehicle
            //    Debug.Log("RESET FOLLWO SPEED " + index);
            //    vehicleToFollow[index] = -1;
            AIEvents.TriggerChangeDestinationEvent(index);
            //}

            if (trigger)
            {
                //trigger corresponding events based on new action

                switch (_currentActiveAction[index])
                {
                    case DriveActions.Reverse:
                    case DriveActions.AvoidReverse:
                    case DriveActions.StopInDistance:
                    case DriveActions.StopInPoint:
                    case DriveActions.GiveWay:
                        AIEvents.TriggerNotifyVehiclesEvent(index, _allVehiclesDataHandler.GetCollider(index));
                        break;
                }
                AIEvents.TriggetChangeDrivingStateEvent(index, _currentActiveAction[index], GetActionValue(_currentActiveAction[index], index));
            }
        }


        /// <summary>
        /// Returns execution times for each action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private float GetActionValue(TrafficSystem.DriveActions action, int index)
        {
            switch (action)
            {
                case DriveActions.Reverse:
                    return 5;
                case DriveActions.StopTemp:
                    return Random.Range(3, 5);
                case DriveActions.Follow:
                    return 1;
                default:
                    return Mathf.Infinity;
            }
        }


        /// <summary>
        /// Called when a waypoint state changed to update the current vehicle actions
        /// </summary>
        /// <param name="index"></param>
        /// <param name="stopState"></param>
        /// <param name="giveWayState"></param>
        private void StopStateChangedHandler(int index, bool stopState)
        {
            if (stopState == true)
            {
                AddDriveAction(index, DriveActions.StopInPoint);
            }
            else
            {
                RemoveDriveAction(index, DriveActions.StopInPoint);
            }
        }


        private void GiveWayStateChangedHandler(int index, bool giveWayState)
        {
            if (giveWayState)
            {
                AddDriveAction(index, DriveActions.GiveWay);
            }
            else
            {
                RemoveDriveAction(index, DriveActions.GiveWay);
            }

        }


        private bool AllClear(int vehicleIndex, int freeWaypointIndex, bool clearPath)
        {
           
            //get the average speed of the car
            float maxWaypointSpeed = _trafficWaypointsDataHandler.GetMaxSpeed(freeWaypointIndex);
            float maxCarSpeed = Mathf.Min(maxWaypointSpeed, _allVehiclesDataHandler.GetMaxSpeed(vehicleIndex));
            //average between current speed and max speed
            float averageSpeed = (_allVehiclesDataHandler.GetCurrentSpeed(vehicleIndex) + maxCarSpeed) / 2 / 3.6f;

            //calculate the distance to the next waypoint 
            float distance = Vector3.Distance(_trafficWaypointsDataHandler.GetPosition(_waypointManager.GetTargetWaypointIndex(vehicleIndex)), _trafficWaypointsDataHandler.GetPosition(freeWaypointIndex));

            //time it takes for the car to reach next waypoint
            float time = distance / averageSpeed;
            //distance needed to be free on the road
            float distanceToCheck = maxWaypointSpeed * time;

            if (clearPath)
            {
                distanceToCheck *= 0.1f;
            }

            int incomingCarIndex = -1;
            //if everything is free -> can go
            if (_waypointManager.AllPreviousWaypointsAreFree(vehicleIndex, distanceToCheck, freeWaypointIndex, ref incomingCarIndex))
            {
                return true;
            }
            else
            {
                //check speed
                if (incomingCarIndex != TrafficSystemConstants.INVALID_VEHICLE_INDEX)
                {
                    if (_allVehiclesDataHandler.GetCurrentSpeed(incomingCarIndex) < 1)
                    {
                        if (!_waypointManager.IsThisWaypointATarget(freeWaypointIndex))
                        {
                            VehicleComponent vehicle = _allVehiclesDataHandler.GetVehicle(vehicleIndex);
                            if (_positionValidator.IsPositionFree(_trafficWaypointsDataHandler.GetPosition(freeWaypointIndex), vehicle.length, vehicle.coliderHeight, vehicle.ColliderWidth, _waypointManager.GetNextOrientation(freeWaypointIndex)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        private void ContinueStraight(int vehicleIndex, VehicleTypes vehicleType)
        {
            //get new waypoint on the same lane
            int freeWaypointIndex = _waypointManager.GetCurrentLaneWaypointIndex(vehicleIndex, vehicleType);
            if (freeWaypointIndex >= 0)
            {
                SetNextWaypoint(vehicleIndex, freeWaypointIndex);
            }
            else
            {
                Blink(BlinkReasons.NoWaypoint, vehicleIndex, freeWaypointIndex);
                NoWaypointsAvailable(vehicleIndex, freeWaypointIndex);
            }
        }


        private void NoWaypointsAvailable(int vehicleIndex, int waypointIndex)
        {
            if (waypointIndex == -2)
            {
                AddDriveAction(vehicleIndex, DriveActions.NoPath);
                Events.TriggerDestinationReachedEvent(vehicleIndex);
                return;
            }

            AddDriveAction(vehicleIndex, DriveActions.NoWaypoint);
        }


        private void SetNextWaypoint(int vehicleIndex, int freeWaypointIndex)
        {
            _waypointManager.SetNextWaypoint(vehicleIndex, freeWaypointIndex);
            _waypointSpeed[vehicleIndex] = _trafficWaypointsDataHandler.GetMaxSpeed(_waypointManager.GetTargetWaypointIndex(vehicleIndex));
            AIEvents.TriggerChangeDestinationEvent(vehicleIndex);
        }


        /// <summary>
        /// Determine if blink is required
        /// </summary>
        private void Blink(BlinkReasons blinkReason, int index, int newWaypointindex)
        {
            if (blinkReason == BlinkReasons.NoWaypoint)
            {
                SetBlinkType(index, BlinkType.Hazard);
                return;
            }

            int oldWaypointIndex = _waypointManager.GetTargetWaypointIndex(index);
            Vector3 forward = _allVehiclesDataHandler.GetForwardVector(index);
            int targetWaypointIndex = newWaypointindex;
            if (blinkReason == BlinkReasons.None)
            {
                if (_trafficWaypointsDataHandler.GetNeighbors(oldWaypointIndex).Length > 1)
                {
                    blinkReason = BlinkReasons.ChangeLane;
                }
            }

            switch (blinkReason)
            {
                case BlinkReasons.Overtake:
                case BlinkReasons.GiveWay:
                    float angle = Vector3.SignedAngle(forward, _trafficWaypointsDataHandler.GetPosition(newWaypointindex) - _trafficWaypointsDataHandler.GetPosition(oldWaypointIndex), Vector3.up);
                    SetBlinkType(index, DetermineBlinkDirection(angle));
                    break;

                case BlinkReasons.ChangeLane:
                    for (int i = 0; i < 5; i++)
                    {
                        if (_trafficWaypointsDataHandler.HasNeighbors(targetWaypointIndex))
                        {
                            targetWaypointIndex = _trafficWaypointsDataHandler.GetNeighbors(targetWaypointIndex)[0];
                        }
                    }
                    angle = Vector3.SignedAngle(_trafficWaypointsDataHandler.GetPosition(oldWaypointIndex) - _trafficWaypointsDataHandler.GetPosition(_trafficWaypointsDataHandler.GetPrevs(oldWaypointIndex)[0]), _trafficWaypointsDataHandler.GetPosition(targetWaypointIndex) - _trafficWaypointsDataHandler.GetPosition(oldWaypointIndex), Vector3.up);
                    SetBlinkType(index, DetermineBlinkDirection(angle));
                    break;

                case BlinkReasons.None:
                    if (_trafficWaypointsDataHandler.HasNeighbors(newWaypointindex))
                    {
                        targetWaypointIndex = _trafficWaypointsDataHandler.GetNeighbors(targetWaypointIndex)[0];
                        angle = Vector3.SignedAngle(_trafficWaypointsDataHandler.GetPosition(oldWaypointIndex) - _trafficWaypointsDataHandler.GetPosition(newWaypointindex), _trafficWaypointsDataHandler.GetPosition(oldWaypointIndex) - _trafficWaypointsDataHandler.GetPosition(targetWaypointIndex), Vector3.up);
                        if (Mathf.Abs(angle) < 1)
                        {
                            SetBlinkType(index, BlinkType.Stop);
                        }
                    }
                    break;

                case BlinkReasons.NoWaypoint:
                    SetBlinkType(index, BlinkType.Hazard);
                    break;
            }
        }


        /// <summary>
        /// Determine the blink direction
        /// </summary>
        private BlinkType DetermineBlinkDirection(float angle)
        {
            if (angle > 5)
            {
                return BlinkType.BlinkRight;
            }

            if (angle < -5)
            {
                return BlinkType.BlinkLeft;
            }
            return BlinkType.Stop;
        }


        private float ComputeMaxPossibleSpeed(int index)
        {
            float maxSpeed;

            if (_currentActiveAction[index] == DriveActions.Follow || _currentActiveAction[index] == DriveActions.Overtake)
            {
                maxSpeed = Mathf.Min(_allVehiclesDataHandler.GetFollowSpeed(index) * 3.6f, _allVehiclesDataHandler.GetMaxSpeed(index), _waypointSpeed[index]);
            }
            else
            {
                maxSpeed = Mathf.Min(_allVehiclesDataHandler.GetMaxSpeed(index), _waypointSpeed[index]);
            }

            return maxSpeed;
        }


        private void TriggerClearedHandler(int vehicleIndex)
        {
            RemoveDriveAction(vehicleIndex, DriveActions.Continue);
        }


        /// <summary>
        /// Events cleanup
        /// </summary>
        public void OnDestroy()
        {
            Events.onVehicleCrashed -= _vehicleCrash;
            VehicleEvents.onTriggerCleared -= TriggerClearedHandler;
            WaypointEvents.onStopStateChanged -= StopStateChangedHandler;
            WaypointEvents.onGiveWayStateChanged -= GiveWayStateChangedHandler;
            VehicleEvents.onObjectInTrigger -= ObjectInTriggerHandler;
        }
    }
}


