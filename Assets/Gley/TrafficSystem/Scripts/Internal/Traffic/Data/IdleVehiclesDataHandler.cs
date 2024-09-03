using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Access idle vehicles.
    /// </summary>
    internal class IdleVehiclesDataHandler
    {
        private IdleVehiclesData _idleVehiclesData;


        internal IdleVehiclesDataHandler(IdleVehiclesData idleVehiclesData)
        {
            _idleVehiclesData = idleVehiclesData;
        }


        /// <summary>
        /// Get an available vehicle to be instantiated
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal VehicleComponent GetAndRemoveVehicle(int vehicleIndex)
        {
            VehicleComponent vehicle = GetVehicle(vehicleIndex);
            RemoveVehicle(vehicleIndex);
            return vehicle;
        }


        internal VehicleComponent PeakIdleVehicle(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex);
        }


        internal int GetRandomIndex()
        {
            if (!HasIdleVehicles())
            {
                return TrafficSystemConstants.INVALID_WAYPOINT_INDEX;
            }
            return Random.Range(0, GetNumberOfVehicles());
        }


        /// <summary>
        /// Get a random index of an idle vehicle
        /// </summary>
        /// <returns></returns>
        internal int GetIdleVehicleIndex(VehicleTypes type)
        {
            var possibleVehicles = GetAllVehiclesOfType(type);

            if (possibleVehicles.Count > 0)
            {
                return GetVehicleIndex(possibleVehicles[Random.Range(0, possibleVehicles.Count)]);
            }

            return TrafficSystemConstants.INVALID_WAYPOINT_INDEX;
        }


        /// <summary>
        /// Get the vehicle type of a given vehicle index
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal VehicleTypes GetIdleVehicleType(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).vehicleType;
        }


        internal float GetFrontWheelOffset(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).frontTrigger.localPosition.z;
        }


        /// <summary>
        /// Get the length of the given vehicle index
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal float GetHalfVehicleLength(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).length / 2;
        }


        /// <summary>
        /// Get the height of the given vehicle index
        /// </summary>
        /// <param name="vehicleIndex"></param>
        /// <returns></returns>
        internal float GetIdleVehicleHeight(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).coliderHeight;
        }


        internal float GetIdleVehicleWidth(int vehicleIndex)
        {
            return GetVehicle(vehicleIndex).ColliderWidth / 2;
        }


        internal void AddVehicle(VehicleComponent vehicle)
        {
            if (!GetAllVehicles().Contains(vehicle))
            {
                if (vehicle.gameObject.activeSelf == false)
                {
                    GetAllVehicles().Add(vehicle);
                }
            }
        }


        internal void RemoveVehicle(VehicleComponent vehicle)
        {
            GetAllVehicles().Remove(vehicle);
        }


        private List<VehicleComponent> GetAllVehiclesOfType(VehicleTypes type)
        {
            return GetAllVehicles().Where(cond => cond.vehicleType == type).ToList();
        }


        private VehicleComponent GetVehicle(int vehicleIndex)
        {
            return GetAllVehicles()[vehicleIndex];
        }


        private int GetVehicleIndex(VehicleComponent vehicle)
        {
            return GetAllVehicles().IndexOf(vehicle);
        }


        private void RemoveVehicle(int vehicleIndex)
        {
            GetAllVehicles().RemoveAt(vehicleIndex);
        }


        private bool HasIdleVehicles()
        {
            return GetNumberOfVehicles() > 0;
        }


        private int GetNumberOfVehicles()
        {
            return GetAllVehicles().Count;
        }


        private List<VehicleComponent> GetAllVehicles()
        {
            return _idleVehiclesData.IdleVehicles;
        }
    }
}
