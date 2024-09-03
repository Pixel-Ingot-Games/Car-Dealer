using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem
{
    /// <summary>
    /// Add this script on a vehicle prefab and configure the required parameters
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [HelpURL("https://gley.gitbook.io/mobile-traffic-system/setup-guide/vehicle-implementation")]
    public class VehicleComponent : MonoBehaviour, ITrafficParticipant
    {
        [Header("Object References")]
        [Tooltip("RigidBody of the vehicle")]
        public Rigidbody rb;
        [Tooltip("Empty GameObject used to rotate the vehicle from the correct point")]
        public Transform carHolder;
        [Tooltip("Front trigger used to detect obstacle. It is automatically generated")]
        public Transform frontTrigger;
        [Tooltip("Assign this object if you need a hard shadow on your vehicle, leave it black otherwise")]
        public Transform shadowHolder;

        [Header("Wheels")]
        [Tooltip("All vehicle wheels and their properties")]
        public Wheel[] allWheels;
        [Tooltip("Max wheel turn amount in degrees")]
        public float maxSteer = 30;
        [Tooltip("If suspension is set to 0, the value of suspension will be half of the wheel radius")]
        public float maxSuspension = 0f;
        [Tooltip("How rigid the suspension will be. Higher the value -> more rigid the suspension")]
        public float springStiffness = 5;


        [Header("Car Properties")]
        [Tooltip("Vehicle type used for making custom paths")]
        public VehicleTypes vehicleType;
        [Tooltip("Min vehicle speed. Actual vehicle speed is picked random between min and max")]
        public int minPossibleSpeed = 40;
        [Tooltip("Max vehicle speed")]
        public int maxPossibleSpeed = 90;
        [Tooltip("Time in seconds to reach max speed (acceleration)")]
        public float accelerationTime = 10;
        [Tooltip("Distance to keep from an obstacle/vehicle")]
        public float distanceToStop = 3;
        [Tooltip("Car starts braking when an obstacle enters trigger. Total length of the trigger = distanceToStop+minTriggerLength")]
        public float triggerLength = 4;

        [HideInInspector]
        public bool updateTrigger = false;
        [HideInInspector]
        public float maxTriggerLength = 10;
        [HideInInspector]
        public TrailerComponent trailer;
        [HideInInspector]
        public Transform trailerConnectionPoint;
        [HideInInspector]
        public float length = 0;
        [HideInInspector]
        public float coliderHeight = 0;
        [HideInInspector]
        public float wheelDistance;
        [HideInInspector]
        public VisibilityScript visibilityScript;
        [HideInInspector]
        public bool excluded;


        private List<ITrafficParticipant> _vehiclesToFollow;
        private Collider[] _allColliders;
        private List<Obstacle> _obstacleList;
        private Transform _frontAxle;
        private BoxCollider _frontCollider;
        private ModifyTriggerSize _modifyTriggerSize;
        private EngineSoundComponent _engineSound;
        private LayerMask _buildingLayers;
        private LayerMask _obstacleLayers;
        private LayerMask _playerLayers;
        private LayerMask _roadLayers;
        private IVehicleLightsComponent _vehicleLights;
        private DriveActions _currentAction;
        private float _springForce;
        private float _maxSpeed;
        private float _storedMaxSpeed;
        private float _minTriggerLength;
        private float _colliderWidth;
        private int _listIndex;
        private bool _lightsOn;

        public Collider[] AllColliders => _allColliders;
        public DriveActions CurrentAction => _currentAction;
        public float ColliderWidth => _colliderWidth;
        public float MaxSpeed => _maxSpeed;
        public float SpringForce => _springForce;
        public int ListIndex => _listIndex;
        public VehicleTypes VehicleType => vehicleType;
        public float MaxSteer => maxSteer;

        private readonly struct Obstacle
        {
            private readonly Collider _collider;
            private readonly bool _isConvex;

            internal readonly Collider Collider => _collider;
            internal readonly bool IsConvex => _isConvex;
            public Obstacle(Collider collider, bool isConvex)
            {
                _collider = collider;
                _isConvex = isConvex;
            }
        }


        /// <summary>
        /// Initialize vehicle
        /// </summary>
        /// <param name="buildingLayers">static colliders to interact with</param>
        /// <param name="obstacleLayers">dynamic colliders to interact with</param>
        /// <param name="playerLayers">player colliders to interact with</param>
        /// <returns>the vehicle</returns>
        public virtual VehicleComponent Initialize(LayerMask buildingLayers, LayerMask obstacleLayers, LayerMask playerLayers, LayerMask roadLayers, bool lightsOn, ModifyTriggerSize modifyTriggerSize)
        {
            _buildingLayers = buildingLayers;
            _obstacleLayers = obstacleLayers;
            _playerLayers = playerLayers;
            _roadLayers = roadLayers;
            _modifyTriggerSize = modifyTriggerSize;
            _allColliders = GetComponentsInChildren<Collider>();
            _springForce = ((rb.mass * -Physics.gravity.y) / allWheels.Length);

            _frontCollider = frontTrigger.GetChild(0).GetComponent<BoxCollider>();
            _colliderWidth = _frontCollider.size.x;
            _minTriggerLength = _frontCollider.size.z;
            DeactivateVehicle();

            //compute center of mass based on the wheel position
            Vector3 centerOfMass = Vector3.zero;
            for (int i = 0; i < allWheels.Length; i++)
            {
                allWheels[i].wheelTransform.Translate(Vector3.up * (allWheels[i].maxSuspension / 2 + allWheels[i].wheelRadius));
                centerOfMass += allWheels[i].wheelTransform.position;
            }
            rb.centerOfMass = centerOfMass / allWheels.Length;

            //set additional components
            _engineSound = GetComponent<EngineSoundComponent>();
            if (_engineSound)
            {
                _engineSound.Initialize();
            }

            _lightsOn = lightsOn;
            _vehicleLights = GetComponent<VehicleLightsComponent>();
            if (_vehicleLights == null)
            {
                _vehicleLights = GetComponent<VehicleLightsComponentV2>();
            }
            if (_vehicleLights != null)
            {
                _vehicleLights.Initialize();
            }

            if (trailer != null)
            {
                trailer.Initialize(this);
            }

            return this;
        }


        /// <summary>
        /// CHeck trigger objects
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger)
            {
                ObstacleTypes obstacleType = GetObstacleTypes(other);
                if (obstacleType == ObstacleTypes.TrafficVehicle || obstacleType == ObstacleTypes.Player)
                {
                    AddVehichleToFollow(other);
                }
                if (obstacleType != ObstacleTypes.Other && obstacleType != ObstacleTypes.Road)
                {
                    NewColliderHit(other);
                    VehicleEvents.TriggerObjectInTriggerEvent(_listIndex, obstacleType, other);
                }
            }
        }


        /// <summary>
        /// Check for collisions
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            Events.TriggerVehicleCrashEvent(_listIndex, GetObstacleTypes(collision.collider), collision.collider);
        }


        /// <summary>
        /// Remove a collider from the list
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnTriggerExit(Collider other)
        {
            if (!other.isTrigger)
            {
                //TODO this should only trigger if objects of interest are doing trigger exit
                if (other.gameObject.layer == gameObject.layer ||
                    (_buildingLayers == (_buildingLayers | (1 << other.gameObject.layer))) ||
                    (_obstacleLayers == (_obstacleLayers | (1 << other.gameObject.layer))) ||
                    (_playerLayers == (_playerLayers | (1 << other.gameObject.layer))))
                {
                    _obstacleList.RemoveAll(cond => cond.Collider == other);
                    if (_obstacleList.Count == 0)
                    {
                        VehicleEvents.TriggerTriggerClearedEvent(_listIndex);
                    }
                    Rigidbody otherRb = other.attachedRigidbody;
                    if (otherRb != null)
                    {
                        if (otherRb.GetComponent<ITrafficParticipant>() != null)
                        {
                            _vehiclesToFollow.Remove(otherRb.GetComponent<ITrafficParticipant>());
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Apply new trigger size delegate
        /// </summary>
        /// <param name="triggerSizeModifier"></param>
        internal void SetTriggerSizeModifierDelegate(ModifyTriggerSize triggerSizeModifier)
        {
            _modifyTriggerSize = triggerSizeModifier;
        }


        /// <summary>
        /// Add a vehicle on scene
        /// </summary>
        /// <param name="position"></param>
        /// <param name="vehicleRotation"></param>
        /// <param name="masterVolume"></param>
        public virtual void ActivateVehicle(Vector3 position, Quaternion vehicleRotation, Quaternion trailerRotation)
        {
            _storedMaxSpeed = _maxSpeed = Random.Range(minPossibleSpeed, maxPossibleSpeed);

            gameObject.transform.SetPositionAndRotation(position, vehicleRotation);

            //position vehicle with front wheels on the waypoint
            float distance = Vector3.Distance(position, frontTrigger.transform.position);
            transform.Translate(-transform.forward * distance, Space.World);

            if (trailer != null)
            {
                trailer.transform.rotation = trailerRotation;
            }

            gameObject.SetActive(true);


            if (_engineSound)
            {
                _engineSound.Play(0);
            }

            SetMainLights(_lightsOn);

            AIEvents.onNotifyVehicles += AVehicleChengedState;
        }


        /// <summary>
        /// Remove a vehicle from scene
        /// </summary>
        public virtual void DeactivateVehicle()
        {
            //Debug.Log(trailer);
            gameObject.SetActive(false);
            _obstacleList = new List<Obstacle>();
            _vehiclesToFollow = new List<ITrafficParticipant>();
            visibilityScript.Reset();
            if (_engineSound)
            {
                _engineSound.Stop();
            }

            if (_vehicleLights != null)
            {
                _vehicleLights.DeactivateLights();
            }
            AIEvents.onNotifyVehicles -= AVehicleChengedState;
            if (trailer)
            {
                trailer.DeactivateVehicle();
            }
        }


        /// <summary>
        /// Compute the ground direction vector used to apply forces, and update the shadow
        /// </summary>
        /// <returns>ground direction</returns>
        public Vector3 GetGroundDirection()
        {
            Vector3 frontPoint = Vector3.zero;
            int nrFront = 0;
            Vector3 backPoint = Vector3.zero;
            int nrBack = 0;
            for (int i = 0; i < allWheels.Length; i++)
            {
                if (allWheels[i].wheelPosition == Wheel.WheelPosition.Front)
                {
                    nrFront++;
                    frontPoint += allWheels[i].wheelGraphics.position;
                }
                else
                {
                    nrBack++;
                    backPoint += allWheels[i].wheelGraphics.position;
                }
            }
            Vector3 groundDirection = (frontPoint / nrFront - backPoint / nrBack).normalized;
            if (shadowHolder)
            {
                Vector3 centerPoint = (frontPoint / nrFront + backPoint / nrBack) / 2 - transform.up * (allWheels[0].wheelRadius - 0.1f);
                shadowHolder.rotation = Quaternion.LookRotation(groundDirection);
                shadowHolder.position = new Vector3(shadowHolder.position.x, centerPoint.y, shadowHolder.position.z);

            }
            return groundDirection;
        }


        /// <summary>
        /// Computes the acceleration per frame
        /// </summary>
        /// <returns></returns>
        public float GetPowerStep()
        {
            int nrOfFrames = (int)(accelerationTime / Time.fixedDeltaTime);
            float targetSpeedMS = _maxSpeed / 3.6f;
            return targetSpeedMS / nrOfFrames;
        }


        /// <summary>
        /// Computes steering speed per frame
        /// </summary>
        /// <returns></returns>
        public float GetSteeringStep()
        {
            return maxSteer * Time.fixedDeltaTime * 2;
        }


        /// <summary>
        /// Computes brake step per frame
        /// </summary>
        /// <returns></returns>
        public float GetBrakeStep()
        {
            int nrOfFrames = (int)(accelerationTime / 4 / Time.fixedDeltaTime);
            float targetSpeedMS = _maxSpeed / 3.6f;
            return targetSpeedMS / nrOfFrames;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Max RayCast length</returns>
        public float GetRaycastLength()
        {
            return allWheels[0].raycastLength;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Wheel circumference</returns>
        public float GetWheelCircumference()
        {
            return allWheels[0].wheelCircumference;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Vehicle velocity vector</returns>
        public Vector3 GetVelocity()
        {
            return rb.velocity;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Current speed in kmh</returns>
        public float GetCurrentSpeed()
        {
            return GetVelocity().magnitude * 3.6f;
        }


        /// <summary>
        /// Returns current speed in m/s
        /// </summary>
        /// <returns></returns>
        public float GetCurrentSpeedMS()
        {
            return GetVelocity().magnitude;
        }


        /// <summary>
        /// Used to verify is the current collider is included in other vehicle trigger
        /// </summary>
        /// <returns>first collider from collider list</returns>
        public Collider GetCollider()
        {
            return _allColliders[0];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Trigger orientation</returns>
        public Vector3 GetHeading()
        {
            return frontTrigger.forward;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>vehicle orientation</returns>
        public Vector3 GetForwardVector()
        {
            return transform.forward;
        }


        /// <summary>
        /// Set the list index for current vehicle
        /// </summary>
        /// <param name="index">new list index</param>
        public void SetIndex(int index)
        {
            _listIndex = index;
        }


        /// <summary>
        /// Check if the vehicle is not in view
        /// </summary>
        /// <returns></returns>
        public bool CanBeRemoved()
        {
            return visibilityScript.IsNotInView();
        }


        /// <summary>
        /// A vehicle stopped reversing check for new action 
        /// </summary>
        public void CurrentVehicleActionDone()
        {
            if (_obstacleList.Count > 0)
            {
                for (int i = 0; i < _obstacleList.Count; i++)
                {
                    ObstacleTypes obstacleType = GetObstacleTypes(_obstacleList[i].Collider);
                    if (obstacleType != ObstacleTypes.Other)
                    {
                        VehicleEvents.TriggerObjectInTriggerEvent(_listIndex, obstacleType, _obstacleList[i].Collider);
                    }
                }
            }
            else
            {
                VehicleEvents.TriggerTriggerClearedEvent(_listIndex);
            }
        }


        /// <summary>
        /// Creates a GameObject that is used to reach waypoints 
        /// </summary>
        /// <returns>the front wheel position of the vehicle</returns>
        public Transform GetFrontAxle()
        {
            if (_frontAxle == null)
            {
                _frontAxle = new GameObject("FrontAxle").transform;
                _frontAxle.transform.SetParent(frontTrigger.parent);
                _frontAxle.transform.position = frontTrigger.position;
            }
            return _frontAxle;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>number of vehicle wheels</returns>
        public int GetNrOfWheels()
        {
            return allWheels.Length;
        }


        /// <summary>
        /// Returns the nr of wheels of the trailer
        /// </summary>
        /// <returns></returns>
        public int GetTrailerWheels()
        {
            if (trailer == null)
            {
                return 0;
            }
            return trailer.GetNrOfWheels();
        }


        /// <summary>
        /// Set the new vehicle action
        /// </summary>
        /// <param name="currentAction"></param>
        public void SetCurrentAction(DriveActions currentAction)
        {
            _currentAction = currentAction;
        }


        /// <summary>
        /// Returns the position of the closest obstacle inside the front trigger.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetClosestObstacle()
        {
            if (_obstacleList.Count > 0)
            {
                if (!_obstacleList[0].IsConvex)
                {
                    return frontTrigger.position;
                }

                Vector3 result = _obstacleList[0].Collider.ClosestPoint(frontTrigger.position);

                float minDistance = Vector3.SqrMagnitude(result - frontTrigger.position);

                for (int i = 1; i < _obstacleList.Count; i++)
                {
                    Vector3 closestPoint = _obstacleList[i].Collider.ClosestPoint(frontTrigger.position);
                    float distance = Vector3.SqrMagnitude(closestPoint - frontTrigger.position);
                    if (Vector3.SqrMagnitude(closestPoint - frontTrigger.position) < minDistance)
                    {
                        result = closestPoint;
                        minDistance = distance;
                    }
                }
                return result;
            }
            return Vector3.zero;
        }


        /// <summary>
        /// Check if current collider is from a new object
        /// </summary>
        /// <param name="colliders"></param>
        /// <returns></returns>
        internal bool AlreadyCollidingWith(Collider[] colliders)
        {
            for (int i = 0; i < _obstacleList.Count; i++)
            {
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (_obstacleList[i].Collider == colliders[j])
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Remove a collider from the trigger if the collider was destroyed
        /// </summary>
        /// <param name="collider"></param>
        public void ColliderRemoved(Collider collider)
        {
            if (_obstacleList != null)
            {
                if (_obstacleList.Any(cond => cond.Collider == collider))
                {
                    OnTriggerExit(collider);
                }
            }
        }


        /// <summary>
        /// Removed a list of colliders from the trigger if the colliders ware destroyed
        /// </summary>
        /// <param name="colliders"></param>
        public void ColliderRemoved(Collider[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (_obstacleList.Any(cond => cond.Collider == colliders[i]))
                {
                    OnTriggerExit(colliders[i]);
                }
            }
        }


        //update the lights component if required
        #region Lights
        internal void SetMainLights(bool on)
        {
            if (on != _lightsOn)
            {
                _lightsOn = on;
            }
            if (_vehicleLights != null)
            {
                _vehicleLights.SetMainLights(on);
            }
        }


        public void SetReverseLights(bool active)
        {
            if (_vehicleLights != null)
            {
                _vehicleLights.SetReverseLights(active);
            }
        }


        public void SetBrakeLights(bool active)
        {
            if (_vehicleLights != null)
            {
                _vehicleLights.SetBrakeLights(active);
            }
        }


        public virtual void SetBlinker(BlinkType blinkType)
        {
            if (_vehicleLights != null)
            {
                _vehicleLights.SetBlinker(blinkType);
            }
        }


        public void UpdateLights(float realtimeSinceStartup)
        {
            if (_vehicleLights != null)
            {
                _vehicleLights.UpdateLights(realtimeSinceStartup);
            }
        }
        #endregion


        //update the sound component if required
        #region Sound
        public void UpdateEngineSound(float masterVolume)
        {
            if (_engineSound)
            {
                _engineSound.UpdateEngineSound(GetCurrentSpeed(), _maxSpeed, masterVolume);
            }
        }
        #endregion


        /// <summary>
        /// Returns the size of the trigger
        /// </summary>
        /// <returns></returns>
        internal float GetTriggerSize()
        {
            return _frontCollider.size.z - 2;
        }


        /// <summary>
        /// Modify the dimension of the front trigger
        /// </summary>
        internal void UpdateColliderSize()
        {
            if (updateTrigger)
            {
                _modifyTriggerSize?.Invoke(GetVelocity().magnitude * 3.6f, _frontCollider, _storedMaxSpeed, _minTriggerLength, maxTriggerLength);
            }
        }


        /// <summary>
        /// Get the gollow speed
        /// </summary>
        /// <returns></returns>
        internal float GetFollowSpeed()
        {
            if (_vehiclesToFollow.Count == 0)
            {
                return Mathf.Infinity;
            }
            return _vehiclesToFollow.Min(cond => cond.GetCurrentSpeedMS());
        }


        /// <summary>
        /// Set max speed for the current vehicle
        /// </summary>
        /// <param name="speed"></param>
        internal void SetMaxSpeed(float speed)
        {
            _maxSpeed = speed;
            if (_maxSpeed < 5)
            {
                _maxSpeed = 0;
            }
        }


        /// <summary>
        /// Reset max speed to the original one
        /// </summary>
        internal void ResetMaxSpeed()
        {
            SetMaxSpeed(_storedMaxSpeed);
        }


        /// <summary>
        /// Returns the stiffness of the springs
        /// </summary>
        /// <returns></returns>
        internal float GetSpringStiffness()
        {
            return springStiffness;
        }


        /// <summary>
        /// Determines which vehicle should be followed
        /// </summary>
        /// <param name="other"></param>
        private void AddVehichleToFollow(Collider other)
        {
            Rigidbody otherRb = other.attachedRigidbody;
            if (otherRb != null)
            {
                if (otherRb.GetComponent<ITrafficParticipant>() != null)
                {
                    _vehiclesToFollow.Add(otherRb.GetComponent<ITrafficParticipant>());
                }
            }
        }


        /// <summary>
        /// Returns the type of obstacle that just entered the front trigger
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private ObstacleTypes GetObstacleTypes(Collider other)
        {
            bool carHit = other.gameObject.layer == gameObject.layer;
            //possible vehicle hit
            if (carHit)
            {
                Rigidbody otherRb = other.attachedRigidbody;
                if (otherRb != null)
                {
                    if (otherRb.GetComponent<VehicleComponent>() != null)
                    {
                        return ObstacleTypes.TrafficVehicle;
                    }
                }
                //if it is on traffic layer but it lacks a vehicle component, it is a dynamic object
                return ObstacleTypes.DynamicObject;
            }
            else
            {
                //trigger the corresponding event based on object layer
                if (_buildingLayers == (_buildingLayers | (1 << other.gameObject.layer)))
                {
                    return ObstacleTypes.StaticObject;
                }
                else
                {
                    if (_obstacleLayers == (_obstacleLayers | (1 << other.gameObject.layer)))
                    {
                        return ObstacleTypes.DynamicObject;
                    }
                    else
                    {
                        if (_playerLayers == (_playerLayers | (1 << other.gameObject.layer)))
                        {
                            return ObstacleTypes.Player;
                        }
                        else
                        {
                            if (_roadLayers == (_roadLayers | (1 << other.gameObject.layer)))
                            {
                                return ObstacleTypes.Road;
                            }
                        }
                    }
                }
            }
            return ObstacleTypes.Other;
        }


        /// <summary>
        /// Every time a new collider is hit it is added inside the list
        /// </summary>
        /// <param name="other"></param>
        private void NewColliderHit(Collider other)
        {
            if (!_obstacleList.Any(cond => cond.Collider == other))
            {
                bool isConvex = true;
                if (other is MeshCollider)
                {
                    isConvex = ((MeshCollider)other).convex;
                }

                _obstacleList.Add(new Obstacle(other, isConvex));
            }
        }


        /// <summary>
        /// When another vehicle changes his state, check if the current vehicle is affected and respond accordingly
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="collider"></param>
        /// <param name="newAction"></param>
        private void AVehicleChengedState(int vehicleIndex, Collider collider)
        {
            //if that vehicle is in the bot trigger
            if (_obstacleList.Any(cond => cond.Collider == collider))
            {
                if (_obstacleList.Count > 0)
                {
                    for (int i = 0; i < _obstacleList.Count; i++)
                    {
                        ObstacleTypes obstacleType = GetObstacleTypes(_obstacleList[i].Collider);
                        if (obstacleType != ObstacleTypes.Other)
                        {
                            VehicleEvents.TriggerObjectInTriggerEvent(_listIndex, obstacleType, _obstacleList[i].Collider);
                        }
                    }
                }
                else
                {
                    VehicleEvents.TriggerTriggerClearedEvent(_listIndex);
                }
            }
        }


        /// <summary>
        /// Removes active events
        /// </summary>
        private void OnDestroy()
        {
            AIEvents.onNotifyVehicles -= AVehicleChengedState;
        }
    }
}