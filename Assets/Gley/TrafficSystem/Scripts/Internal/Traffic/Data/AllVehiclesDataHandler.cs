using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Access all traffic vehicles.
    /// </summary>
    internal class AllVehiclesDataHandler
    {
        private readonly AllVehiclesData _allVehiclesData;


        internal AllVehiclesDataHandler(AllVehiclesData allVehiclesData)
        {
            _allVehiclesData = allVehiclesData;
        }


        /// <summary>
        /// Get entire vehicle list
        /// </summary>
        /// <returns></returns>
        internal VehicleComponent[] GetAllVehicles()
        {
            return _allVehiclesData.AllVehicles;
        }


        internal VehicleComponent GetVehicle(int vehicleIndex)
        {
            return GetAllVehicles()[vehicleIndex];
        }


        internal Rigidbody GetRigidbody(int vehicleIndex)
        {
            return GetAllVehicles()[vehicleIndex].rb;
        }


        internal Rigidbody GetTrailerRigidbody(int vehicleIndex)
        {
            return GetAllVehicles()[vehicleIndex].trailer.rb;
        }


        internal List<VehicleComponent> GetExcludedVehicleList()
        {
            var result = new List<VehicleComponent>();
            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                if (GetVehicle(i).excluded)
                {
                    result.Add(GetVehicle(i));
                }
            }
            return result;
        }


        internal int GetExcludedVehicleIndex(GameObject vehicle)
        {
            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                if (GetVehicle(i).excluded)
                {
                    if (GetVehicle(i).gameObject == vehicle)
                    {
                        return i;
                    }
                }
            }
            return TrafficSystemConstants.INVALID_VEHICLE_INDEX;
        }


        internal bool VehicleIsExcluded(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).excluded;
        }


        internal VehicleComponent GetExcludedVehicle(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex);
        }


        internal void SetExcludedValue(int vehicleIndex, bool excluded)
        {
            GetVehicle(vehicleIndex).excluded = excluded;
        }


        /// <summary>
        /// Set reverse lights if required on a specific vehicle
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="active"></param>
        internal void SetReverseLights(int vehicleIndex, bool active)
        {
            GetVehicle(vehicleIndex).SetReverseLights(active);
        }


        /// <summary>
        /// Set brake lights if required on a specific vehicle
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="active"></param>
        internal void SetBrakeLights(int vehicleIndex, bool active)
        {
            GetVehicle(vehicleIndex).SetBrakeLights(active);
        }


        internal VehicleTypes GetVehicleType(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).vehicleType;
        }


        /// <summary>
        /// Remove the given vehicle from scene
        /// </summary>
        /// <param name="vehicleIndex"></param>
        internal void RemoveVehicle(int vehicleIndex)
        {
            GetVehicle(vehicleIndex).DeactivateVehicle();

            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                GetVehicle(i).ColliderRemoved(GetAllColliders(vehicleIndex));
            }
        }


        /// <summary>
        /// Check if the given vehicle can be removed from scene
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal bool CanBeRemoved(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).CanBeRemoved();
        }


        /// <summary>
        /// Get the current velocity of the given vehicle index
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal Vector3 GetVelocity(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetVelocity();
        }


        /// <summary>
        /// Get the speed of the given vehicle index
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal float GetCurrentSpeed(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetCurrentSpeed();
        }


        //if a vehicle has a traffic participant in trigger, it will return the minimum speed of all participants
        internal float GetFollowSpeed(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetFollowSpeed();
        }


        internal float GetVehicleLength(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).length;
        }


        internal float GetVehicleWidth(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).ColliderWidth;
        }


        /// <summary>
        /// Activate a vehicle on scene
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="position"></param>
        /// <param name="vehicleRotation"></param>
        internal void ActivateVehicle(VehicleComponent vehicle, Vector3 position, Quaternion vehicleRotation, Quaternion trailerRotation)
        {
            vehicle.ActivateVehicle(position, vehicleRotation, trailerRotation);
        }


        /// <summary>
        /// Get the vehicle collider
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal Collider GetCollider(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetCollider();
        }


        /// <summary>
        /// Get the vehicle moving direction
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal Vector3 GetHeading(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetHeading();
        }


        /// <summary>
        /// Get the vehicles forward direction
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Vector3 GetForwardVector(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetForwardVector();
        }


        /// <summary>
        /// Set a new action for a vehicle
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="currentAction"></param>
        internal void SetCurrentAction(int vehicleIndex, TrafficSystem.DriveActions currentAction)
        {
            GetVehicle(vehicleIndex).SetCurrentAction(currentAction);
        }


        /// <summary>
        /// The give vehicle has stopped reversing
        /// </summary>
        /// <param name="vehicleIndex"></param>
        internal void CurrentVehicleActionDone(int vehicleIndex)
        {
            GetVehicle(vehicleIndex).CurrentVehicleActionDone();
        }


        /// <summary>
        /// Get the current action for the given vehicle index
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal TrafficSystem.DriveActions GetCurrentAction(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).CurrentAction;
        }


        /// <summary>
        /// Get the vehicles max speed
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal float GetMaxSpeed(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).MaxSpeed;
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal float GetPossibleMaxSpeed(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).maxPossibleSpeed;
        }


        /// <summary>
        /// Set the corresponding blinker for the vehicle
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <param name="blinkType"></param>
        internal void SetBlinkLights(int vehicleIndex, BlinkType blinkType)
        {
            GetVehicle(vehicleIndex).SetBlinker(blinkType);
        }


        /// <summary>
        /// Get the spring force of the vehicle
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal float GetSpringForce(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).SpringForce;
        }


        /// <summary>
        /// Get the power step (acceleration) of the vehicle
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal float GetPowerStep(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetPowerStep();
        }


        /// <summary>
        /// Get the brake power step of the vehicle
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal float GetBrakeStep(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetBrakeStep();
        }


        /// <summary>
        /// Get ground orientation vector
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Vector3 GetGroundDirection(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetGroundDirection();
        }


        /// <summary>
        /// Update additional components from the vehicle if needed
        /// </summary>
        /// <param name="vehicleIndex"></param>
        internal void UpdateVehicleScripts(int vehicleIndex, float volume, float realTimeSinceStartup)
        {
            GetVehicle(vehicleIndex).UpdateEngineSound(volume);
            GetVehicle(vehicleIndex).UpdateLights(realTimeSinceStartup);
            GetVehicle(vehicleIndex).UpdateColliderSize();
        }


        /// <summary>
        /// Update main lights of the vehicle
        /// </summary>
        /// <param name="on"></param>
        internal void UpdateVehicleLights(bool on)
        {
            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                GetVehicle(i).SetMainLights(on);
            }
        }


        internal void TriggerColliderRemovedEvent(Collider[] colliders)
        {
            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                GetVehicle(i).ColliderRemoved(colliders);
            }
        }


        internal float GetTriggerSize(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetTriggerSize();
        }


        internal void ModifyTriggerSize(int vehicleIndex, ModifyTriggerSize modifyTriggerSizeDelegate)
        {
            if (vehicleIndex < 0)
            {
                for (int i = 0; i < GetAllVehicles().Length; i++)
                {
                    GetVehicle(i).SetTriggerSizeModifierDelegate(modifyTriggerSizeDelegate);
                }
            }
            else
            {
                GetVehicle(vehicleIndex).SetTriggerSizeModifierDelegate(modifyTriggerSizeDelegate);
            }
        }


#if GLEY_TRAFFIC_SYSTEM
        internal Unity.Mathematics.float3 GetClosestObstacle(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).GetClosestObstacle();
        }
#endif


        internal Collider[] GetAllColliders(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).AllColliders;
        }


        internal void SetMaxSpeed(int vehicleIndex, float speed)
        {
            GetVehicle(vehicleIndex).SetMaxSpeed(speed);
        }


        internal void ResetMaxSpeed(int vehicleIndex)
        {
            GetVehicle(vehicleIndex).ResetMaxSpeed();
        }


        internal bool HasTrailer(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).trailer != null;
        }


        internal int GetTrailerWheels(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).trailer.GetNrOfWheels();
        }


        internal int GetVehicleIndex(GameObject vehicle)
        {
            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                if (GetVehicle(i).gameObject == vehicle)
                {
                    return i;
                }
            }
            return TrafficSystemConstants.INVALID_WAYPOINT_INDEX;
        }


        internal int GetTotalWheels()
        {
            int totalWheels = 0;
            for (int i = 0; i < GetAllVehicles().Length; i++)
            {
                totalWheels += GetVehicle(i).allWheels.Length;
                if (GetVehicle(i).trailer != null)
                {
                    totalWheels += GetVehicle(i).trailer.allWheels.Length;
                }
            }
            return totalWheels;
        }
    }
}
