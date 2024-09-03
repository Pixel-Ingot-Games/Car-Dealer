#if GLEY_TRAFFIC_SYSTEM
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Handles driving part of the vehicle
    /// </summary>
    [BurstCompile]
    public struct DriveJob : IJobParallelFor
    {
        public NativeArray<float3> BodyForce;
        public NativeArray<float3> TrailerForce;
        public NativeArray<float> ActionValue;
        public NativeArray<float> WheelRotation;
        public NativeArray<float> TurnAngle;
        public NativeArray<float> VehicleRotationAngle;
        public NativeArray<int> Gear;
        public NativeArray<bool> ReadyToRemove;
        public NativeArray<bool> NeedsWaypoint;
        public NativeArray<bool> IsBraking;

        [ReadOnly] public NativeArray<TrafficSystem.DriveActions> SpecialDriveAction;
        [ReadOnly] public NativeArray<float3> TargetWaypointPosition;
        [ReadOnly] public NativeArray<float3> AllBotsPosition;
        [ReadOnly] public NativeArray<float3> GroundDirection;
        [ReadOnly] public NativeArray<float3> ForwardDirection;
        [ReadOnly] public NativeArray<float3> RightDirection;
        [ReadOnly] public NativeArray<float3> TrailerRightDirection;
        [ReadOnly] public NativeArray<float3> TrailerForwardDirection;
        [ReadOnly] public NativeArray<float3> TriggerForwardDirection;
        [ReadOnly] public NativeArray<float3> DownDirection;
        [ReadOnly] public NativeArray<float3> CarVelocity;
        [ReadOnly] public NativeArray<float3> TrailerVelocity;
        [ReadOnly] public NativeArray<float3> CameraPositions;
        [ReadOnly] public NativeArray<float3> ClosestObstacle;
        [ReadOnly] public NativeArray<float> WheelCircumferences;
        [ReadOnly] public NativeArray<float> MaxSteer;
        [ReadOnly] public NativeArray<float> PowerStep;
        [ReadOnly] public NativeArray<float> BrakeStep;
        [ReadOnly] public NativeArray<float> Drag;
        [ReadOnly] public NativeArray<float> TrailerDrag;
        [ReadOnly] public NativeArray<float> MaxSpeed;
        [ReadOnly] public NativeArray<float> WheelDistance;
        [ReadOnly] public NativeArray<float> SteeringStep;
        [ReadOnly] public NativeArray<float> VehicleLength;
        [ReadOnly] public NativeArray<float> MassDifference;
        [ReadOnly] public NativeArray<float> DistanceToStop;
        [ReadOnly] public NativeArray<int> WheelSign;
        [ReadOnly] public NativeArray<int> NrOfWheels;
        [ReadOnly] public NativeArray<int> TrailerNrOfWheels;
        [ReadOnly] public float3 WorldUp;
        [ReadOnly] public float DistanceToRemove;
        [ReadOnly] public float FixedDeltaTime;


        private float3 _waypointDirection;
        private float _minWaypointDistance;
        private float _targetSpeed; //required car speed in next frame
        private float _currentSpeed; //speed in current frame
        private float _dotProduct;
        private float _waypointDistance;
        private float _angle;
        private bool _avoidBackward;
        private bool _avoidForward;


        public void Execute(int index)
        {
            //reset variables
            _avoidForward = false;
            _avoidBackward = false;
            IsBraking[index] = false;

            //compute current frame values
            float3 forwardVelocity = ForwardDirection[index] * Vector3.Dot(CarVelocity[index], ForwardDirection[index]);
            _targetSpeed = math.length(forwardVelocity);
            _currentSpeed = _targetSpeed * math.sign(Vector3.Dot(forwardVelocity, ForwardDirection[index]));
            _waypointDirection = TargetWaypointPosition[index] - AllBotsPosition[index];
            _dotProduct = math.dot(_waypointDirection, ForwardDirection[index]);// used to check if vehicle passed the current waypoint
            _waypointDirection.y = 0;
            _waypointDistance = math.distance(TargetWaypointPosition[index], AllBotsPosition[index]);

            //Debug.Log(forwardDirection[index]);

            //change the distance to change waypoints based on vehicle speed
            //at 50 kmh -> min distance =1.5
            //at 100 kmh -> min distance =2.5
            //kmh to ms => 50/3.6 = 13.88
            if (_currentSpeed < 13.88f)
            {
                _minWaypointDistance = 1.5f;
            }
            else
            {
                _minWaypointDistance = 1.5f + (_currentSpeed * 3.6f - 50) / 50;
            }

            //compute acceleration based on the current vehicle actions
            Drive(index);

            //compute forces required for the target speed to be achieved
            ComputeBodyForce(index, MaxSpeed[index], Gear[index]);

            //check if a new waypoint is required
            ChangeWaypoint(index);

            //compute the wheel turn amount
            ComputeWheelRotationAngle(index);

            //compute steering angle
            ComputeSteerAngle(index, MaxSteer[index], _targetSpeed);

            //check if vehicle is far enough for the player and it can be removed
            RemoveVehicle(index);
        }


        #region Drive
        /// <summary>
        /// Compute acceleration value based on the current vehicle`s driving actions
        /// </summary>
        /// <param name="index">index of the current vehicle </param>
        private void Drive(int index)
        {
            if (ActionValue[index] != math.INFINITY)
            {
                ActionValue[index] -= FixedDeltaTime;
            }

            switch (SpecialDriveAction[index])
            {
                case DriveActions.Reverse:
                    Reverse(index);
                    break;
                case DriveActions.AvoidReverse:
                    AvoidReverse(index);
                    break;
                case DriveActions.StopTemp:
                case DriveActions.NoWaypoint:
                case DriveActions.NoPath:
                case DriveActions.Stop:
                    StopNow(index);
                    break;
                case DriveActions.StopInDistance:
                    StopInDistance(index);
                    break;
                case DriveActions.StopInPoint:
                case DriveActions.GiveWay:
                    StopInPoint(index);
                    break;
                case DriveActions.Follow:
                case DriveActions.Overtake:
                    Follow(index, MaxSpeed[index]);
                    break;
                default:
                    Forward(index, MaxSpeed[index]);
                    break;
            }
        }

        /// <summary>
        /// Normal drive
        /// </summary>
        /// <param name="index"></param>
        /// <param name="maxSpeed">max possible speed of the current vehicle</param>
        void Forward(int index, float maxSpeed)
        {
            if (IsInCorrectGear(index))
            {
                //slow down in corners
                if (maxSpeed / _targetSpeed < 1.5)
                {
                    if (math.abs(TurnAngle[index]) > 5)
                    {
                        //Debug.Log(1 + math.abs(turnAngle[index]) / maxSteer[index]);
                        //ApplyBrakes(index, 1 + math.abs(turnAngle[index]) / maxSteer[index]);
                        ApplyBrakes(index, 1);
                        //isBraking[index] = true;
                        return;
                    }
                }

                //set speed exactly to max speed if it is close
                float speedDifference = _targetSpeed - maxSpeed;
                if (math.abs(speedDifference) < PowerStep[index] || math.abs(speedDifference) < BrakeStep[index])
                {
                    _targetSpeed = maxSpeed;
                    return;
                }

                //brake if the vehicle runs faster than the max allowed speed
                if (_targetSpeed > maxSpeed)
                {
                    //for the brake lights to be active only when hard brakes are needed, to avoid short blinking
                    if (speedDifference > 1)
                    {
                        //turn on braking lights
                        IsBraking[index] = true;
                    }
                    ApplyBrakes(index, 1);
                    return;
                }
                ApplyAcceleration(index);
            }
            else
            {
                ApplyBrakes(index, 1);
                PutInDrive(index);
            }
        }


        /// <summary>
        /// Go backwards
        /// </summary>
        /// <param name="index"></param>
        void Reverse(int index)
        {
            if (IsInCorrectGear(index))
            {
                ApplyAcceleration(index);
            }
            else
            {
                ApplyBrakes(index, 1);
                PutInReverse(index);
            }
        }


        /// <summary>
        /// Go backwards in opposite direction
        /// </summary>
        /// <param name="index"></param>
        private void AvoidReverse(int index)
        {
            if (IsInCorrectGear(index))
            {
                AvoidBackward();
                Reverse(index);
            }
            else
            {
                StopInDistance(index);
                PutInReverse(index);
            }
        }


        /// <summary>
        /// Stop vehicle immediately
        /// </summary>
        /// <param name="index"></param>
        void StopNow(int index)
        {
            _targetSpeed = 0;
            IsBraking[index] = true;
        }


        /// <summary>
        /// Stop the car in a given distance
        /// </summary>
        /// <param name="index"></param>
        private void StopInDistance(int index)
        {
            float stopDistance = math.distance(ClosestObstacle[index], AllBotsPosition[index]) - DistanceToStop[index];
            IsBraking[index] = true;

            if (stopDistance <= 0)
            {
                StopNow(index);
                return;
            }

            if (_currentSpeed <= BrakeStep[index])
            {
                StopNow(index);
                return;
            }

            float velocityPerFrame = _currentSpeed * FixedDeltaTime;
            int nrOfFrames = (int)(stopDistance / velocityPerFrame) + 1;
            int brakeNrOfFrames = (int)(_currentSpeed / BrakeStep[index]);
            if (brakeNrOfFrames >= nrOfFrames)
            {
                ApplyBrakes(index, (float)brakeNrOfFrames / nrOfFrames);
            }
        }


        /// <summary>
        /// Stop the vehicle precisely on a waypoint
        /// </summary>
        /// <param name="index"></param>
        void StopInPoint(int index)
        {
            //if there is something in trigger closer -> stop 
            if (!ClosestObstacle[index].Equals(float3.zero))
            {
                if (math.distance(ClosestObstacle[index], AllBotsPosition[index]) < _waypointDistance)
                {
                    StopInDistance(index);
                }
            }

            //stop if the waypoint is behind the vehicle
            if (_dotProduct < 0)
            {
                StopNow(index);
                return;
            }

            //compute per frame velocity
            float velocityPerFrame = _currentSpeed * FixedDeltaTime;

            //check number of frames required to reach next waypoint
            int nrOfFrames = (int)(_waypointDistance / velocityPerFrame);

            if (nrOfFrames < 0)
            {
                nrOfFrames = int.MaxValue;
            }

            //if vehicle is in target -> stop
            if (nrOfFrames == 0)
            {
                StopNow(index);
                return;
            }

            //number of frames required to brake
            int brakeNrOfFrames = (int)(_currentSpeed / BrakeStep[index]);
            //calculate the required brake power 
            if (brakeNrOfFrames > nrOfFrames)
            {
                ApplyBrakes(index, (float)brakeNrOfFrames / nrOfFrames);
            }
            else
            {
                //if target waypoint is far -> accelerate
                if (nrOfFrames - brakeNrOfFrames > 60)
                {
                    ApplyAcceleration(index);
                    return;
                }
            }
            //turn on the brake lights
            IsBraking[index] = true;
        }


        /// <summary>
        /// Opposite direction is required forward 
        /// </summary>
        /// <param name="index"></param>
        void AvoidForward(int index)
        {
            _avoidForward = true;
            Forward(index, MaxSpeed[index]);
        }


        /// <summary>
        /// Follow the front vehicle
        /// </summary>
        /// <param name="index"></param>
        /// <param name="followSpeed">the speed of the front vehicle</param>
        private void Follow(int index, float followSpeed)
        {
            float distance = math.distance(ClosestObstacle[index], AllBotsPosition[index]) - DistanceToStop[index];
            if (distance < 0)
            {
                //distance is dangerously close apply emergency brake
                _targetSpeed = followSpeed - 1;
                return;
            }

            float speedDifference = _targetSpeed - followSpeed;

            //current vehicle moves slower then the vehicle it follows
            if (speedDifference <= 0)
            {
                //speeds are close enough, match them
                if (math.abs(speedDifference) < math.max(PowerStep[index], BrakeStep[index]))
                {
                    _targetSpeed = followSpeed;
                    return;
                }
                //vehicle needs to accelerate to catch the followed vehicle
                ApplyAcceleration(index);
                return;
            }

            //compute per frame velocity
            float velocityPerFrame = (speedDifference) * FixedDeltaTime;

            //check number of frames required to slow down
            int nrOfFrames = (int)(distance / velocityPerFrame);

            //if nr of frames = 0 => distance is 0, it means that the 2 cars are close enough, set the speed to be equal to the follow speed
            if (nrOfFrames == 0)
            {
                _targetSpeed = followSpeed;
                return;
            }

            //number of frames required to brake
            int brakeNrOfFrames = (int)(speedDifference / BrakeStep[index]);

            //calculate the required brake power 
            if (brakeNrOfFrames >= nrOfFrames)
            {
                if (nrOfFrames > 0)
                {
                    ApplyBrakes(index, (float)brakeNrOfFrames / nrOfFrames);
                }
            }
        }
        #endregion


        /// <summary>
        /// Compute the next frame force to be applied to RigidBody
        /// </summary>
        /// <param name="index">current vehicle index</param>
        /// <param name="targetVelocity">target linear speed</param>
        /// <returns></returns>
        private void ComputeBodyForce(int index, float maxSpeed, int gear)
        {
            //set speed limit
            if (maxSpeed == 0 || (math.sign(_targetSpeed * gear) != math.sign(_currentSpeed) && math.abs(_currentSpeed) > 0.01f))
            {
                _targetSpeed = 0;
            }
            maxSpeed = math.max(_targetSpeed, maxSpeed);
            if (gear == -1)
            {
                if (_targetSpeed < -maxSpeed / 5)
                {
                    _targetSpeed = -maxSpeed / 5;
                }
            }
            else
            {
                if (_targetSpeed > maxSpeed)
                {
                    _targetSpeed = maxSpeed;
                }
            }

            //Debug.Log(maxSpeed * 3.6 + " " + _targetSpeed * 3.6f + " " + currentSpeed * 3.6f);

            float dSpeed = _targetSpeed * gear - _currentSpeed;
            float velocity = dSpeed + GetDrag(_targetSpeed, Drag[index], FixedDeltaTime);

            //if has trailer
            if (TrailerNrOfWheels[index] > 0)
            {
                TrailerForce[index] = -TrailerRightDirection[index] * Vector3.Dot(TrailerVelocity[index], TrailerRightDirection[index]) / TrailerNrOfWheels[index];

                if (_targetSpeed != 0)
                {
                    velocity += dSpeed * MassDifference[index] + GetDrag(_targetSpeed, TrailerDrag[index], FixedDeltaTime);
                }
            }

            BodyForce[index] = velocity * ForwardDirection[index] / NrOfWheels[index];
        }


        /// <summary>
        /// Check if new waypoint is required
        /// </summary>
        /// <param name="index">current vehicle index</param>
        void ChangeWaypoint(int index)
        {
            if (_waypointDistance < _minWaypointDistance || (_dotProduct < 0 && _waypointDistance < _minWaypointDistance * 5))
            {
                NeedsWaypoint[index] = true;
            }
        }


        /// <summary>
        /// Compute the wheel turn amount
        /// </summary>
        /// <param name="index">current vehicle index</param>
        void ComputeWheelRotationAngle(int index)
        {
            WheelRotation[index] += (360 * (_currentSpeed / WheelCircumferences[index]) * FixedDeltaTime);
        }


        /// <summary>
        /// Compute the required steering angle
        /// </summary>
        /// <param name="index"></param>
        /// <param name="maxSteer"></param>
        /// <param name="targetVelocity"></param>
        void ComputeSteerAngle(int index, float maxSteer, float targetVelocity)
        {
            float currentTurnAngle = TurnAngle[index];
           
            float currentStep = SteeringStep[index];
            //increase turn angle with speed
            if (targetVelocity > 14)
            {
                //if speed is greater than 50 km/h increase the turn speed;
                currentStep *= targetVelocity / 14;
            }
            float wheelAngle = SignedAngle(TriggerForwardDirection[index], _waypointDirection, WorldUp);
            //determine the target angle
            if (_avoidBackward)
            {
                _angle = WheelSign[index] * -maxSteer;
            }
            else
            {
                if (_avoidForward)
                {
                    _angle = maxSteer;
                }
                else
                {
                    _angle = SignedAngle(ForwardDirection[index], _waypointDirection, WorldUp);
                }
            }

            if (!_avoidBackward && !_avoidForward)
            {
                //if car is stationary, do not turn the wheels
                if (_currentSpeed < 1)
                {
                    if (SpecialDriveAction[index] != TrafficSystem.DriveActions.StopInDistance && SpecialDriveAction[index] != TrafficSystem.DriveActions.ChangeLane)
                    {
                        _angle = 0;
                    }
                }

                //check if the car can turn at current speed         
                float framesToReach = _waypointDistance / (targetVelocity * FixedDeltaTime);
                //if it is close to the waypoint turn at normal speed 
                if (framesToReach > 5)
                {
                    //calculate the number of frames required to rotate to the target amount
                    float framesToRotate = math.abs(_angle - currentTurnAngle) / currentStep;

                    //car is too fast for this corner
                    //increase the speed turn amount to be able to corner
                    if (framesToRotate > framesToReach + 5)
                    {
                        currentStep *= framesToRotate / framesToReach;
                    }
                    else
                    {
                        //used to straight the wheels after a curve
                        if (math.sign(_angle) != math.sign(wheelAngle) && math.abs(_angle - wheelAngle) > 10)
                        {
                            currentStep *= framesToRotate / 5;
                        }
                    }
                }
            }
            //apply turning speed
            if (_angle - currentTurnAngle < -currentStep)
            {
                currentTurnAngle -= currentStep;
            }
            else
            {
                if (_angle - currentTurnAngle > currentStep)
                {
                    currentTurnAngle += currentStep;
                }
                else
                {
                    currentTurnAngle = _angle;

                }
            }

            //if the wheel sign and turn angle sign are different the wheel is heading to the destination waypoint, it just needs to keep that direction
            //no additional turning is required so keep the waypoint direction
            if (math.sign(currentTurnAngle) != math.sign(wheelAngle) && math.abs(wheelAngle) < 5)
            {
                currentTurnAngle = _angle;
            }

            //clamp the value
            if (currentTurnAngle > maxSteer)
            {
                currentTurnAngle = maxSteer;
            }

            if (currentTurnAngle < -maxSteer)
            {
                currentTurnAngle = -maxSteer;
            }

            //currentTurnAngle = angle;

            //compute the body turn angle based on wheel turn amount
            float turnRadius = WheelDistance[index] / math.tan(math.radians(currentTurnAngle));
            VehicleRotationAngle[index] = (180 * targetVelocity * FixedDeltaTime) / (math.PI * turnRadius) * Gear[index];
            TurnAngle[index] = currentTurnAngle;
        }


        /// <summary>
        /// Checks if the vehicle can be removed from scene
        /// </summary>
        /// <param name="index">the list index of the vehicle</param>
        void RemoveVehicle(int index)
        {
            bool remove = true;
            for (int i = 0; i < CameraPositions.Length; i++)
            {
                if (math.distancesq(AllBotsPosition[index], CameraPositions[i]) < DistanceToRemove)
                {
                    remove = false;
                    break;
                }
            }
            ReadyToRemove[index] = remove;
        }


        /// <summary>
        /// Determine if a car has can change the heading direction
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsInCorrectGear(int index)
        {
            switch (SpecialDriveAction[index])
            {
                case TrafficSystem.DriveActions.Reverse:
                case TrafficSystem.DriveActions.AvoidReverse:
                    if (Gear[index] != -1)
                    {
                        return false;
                    }
                    break;

                default:
                    if (Gear[index] != 1)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }


        void PutInDrive(int index)
        {
            if (_targetSpeed == 0)
            {
                if (math.abs(_currentSpeed) < 0.0001)
                {
                    Gear[index] = 1;
                }
            }
        }


        void PutInReverse(int index)
        {
            if (_targetSpeed == 0)
            {
                if (math.abs(_currentSpeed) < 0.0001f)
                {
                    Gear[index] = -1;
                }
            }
        }


        /// <summary>
        /// Accelerate current vehicle
        /// </summary>
        /// <param name="index"></param>
        void ApplyAcceleration(int index)
        {
            _targetSpeed += PowerStep[index];
        }


        /// <summary>
        /// Brake the vehicle
        /// </summary>
        /// <param name="index"></param>
        /// <param name="power"></param>
        void ApplyBrakes(int index, float power)
        {
            //this is a workaround to mediate the brake power 
            power /= 2;
            if(power<1)
            {
                power = 1;
            }
            _targetSpeed -= BrakeStep[index] * power;
            if (_targetSpeed < 0)
            {
                StopNow(index);
            }
        }


        /// <summary>
        /// Opposite direction is required in reverse
        /// </summary>
        void AvoidBackward()
        {
            _avoidBackward = true;
        }


        /// <summary>
        /// Compute sign angle between 2 directions
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        float SignedAngle(float3 dir1, float3 dir2, float3 normal)
        {
            if (dir1.Equals(float3.zero))
            {
                return 0;
            }
            dir1 = math.normalize(dir1);
            return math.degrees(math.atan2(math.dot(math.cross(dir1, dir2), normal), math.dot(dir1, dir2)));
        }


        /// <summary>
        /// Compensate the drag from the physics engine
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetSpeed"></param>
        /// <returns></returns>
        float GetDrag(float targetSpeed, float drag, float fixedDeltaTime)
        {
            float result = targetSpeed / (1 - drag * fixedDeltaTime) - targetSpeed;
            return result;
        }
    }
}
#endif