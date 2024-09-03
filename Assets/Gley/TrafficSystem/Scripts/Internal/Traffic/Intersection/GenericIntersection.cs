using Gley.UrbanSystem.Internal;
using System.Collections.Generic;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Base class for all intersections
    /// </summary>
    [System.Serializable]
    public abstract class GenericIntersection : IIntersection
    {
        protected List<int> _carsInIntersection;

        #region InterfactImplementation
        public abstract bool IsPathFree(int waypointIndex);

        public void VehicleEnter(int vehicleIndex)
        {
            _carsInIntersection.Add(vehicleIndex);
        }

        public void VehicleLeft(int vehicleIndex)
        {
            _carsInIntersection.Remove(vehicleIndex);
        }

        public abstract void PedestrianPassed(int agentIndex);
        #endregion

        internal abstract void UpdateIntersection(float realtimeSinceStartup);

        internal abstract int[] GetPedStopWaypoint();

        internal abstract string GetName();

        internal abstract List<int> GetStopWaypoints();

        internal void RemoveVehicle(int index)
        {
            VehicleLeft(index);
        }

        internal virtual void ResetIntersection()
        {
            _carsInIntersection = new List<int>();
        }
    }
}