using System.Collections.Generic;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores all idle vehicles.
    /// </summary>
    internal class IdleVehiclesData
    {
        private readonly List<VehicleComponent> _idleVehicles;

        internal List<VehicleComponent> IdleVehicles => _idleVehicles;


        internal IdleVehiclesData (List<VehicleComponent> idleVehicles)
        {
            _idleVehicles = idleVehicles;
        }
    }
}