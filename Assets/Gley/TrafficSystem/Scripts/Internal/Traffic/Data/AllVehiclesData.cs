using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores all instantiated vehicles.
    /// </summary>
    internal class AllVehiclesData
    {
        private readonly VehicleComponent[] _allVehicles;

        internal VehicleComponent[] AllVehicles => _allVehicles;


        internal AllVehiclesData(Transform parent, VehiclePool vehiclePool, int nrOfVehicles, LayerMask buildingLayers, LayerMask obstacleLayers, LayerMask playerLayers, LayerMask roadLayers, bool lightsOn, ModifyTriggerSize modifyTriggerSize)
        {
            var trafficHolder = MonoBehaviourUtilities.CreateGameObject(TrafficSystemConstants.TrafficHolderName, parent, parent.position, false).transform;
            _allVehicles = new VehicleComponent[nrOfVehicles];
            int currentVehicleIndex = 0;

            // Transform percent into numbers.
            int carsToInstantiate = vehiclePool.trafficCars.Length;
            if (carsToInstantiate > nrOfVehicles)
            {
                carsToInstantiate = nrOfVehicles;
            }
            // Instantiate at least a car from each type.
            for (int i = 0; i < carsToInstantiate; i++)
            {
                _allVehicles[currentVehicleIndex] = LoadVehicle(currentVehicleIndex, vehiclePool.trafficCars[i].vehiclePrefab, trafficHolder, buildingLayers, obstacleLayers, playerLayers, roadLayers, vehiclePool.trafficCars[i].dontInstantiate, lightsOn, modifyTriggerSize);
                currentVehicleIndex++;
            }

            nrOfVehicles -= carsToInstantiate;
            float sum = 0;
            List<float> thresholds = new List<float>();
            for (int i = 0; i < vehiclePool.trafficCars.Length; i++)
            {
                sum += vehiclePool.trafficCars[i].percent;
                thresholds.Add(sum);
            }
            float perCarValue = sum / nrOfVehicles;

            // instantiate remaining vehicles.
            int vehicleIndex = 0;
            for (int i = 0; i < nrOfVehicles; i++)
            {
                while ((i + 1) * perCarValue > thresholds[vehicleIndex])
                {
                    vehicleIndex++;
                    if (vehicleIndex >= vehiclePool.trafficCars.Length)
                    {
                        vehicleIndex = vehiclePool.trafficCars.Length - 1;
                        break;
                    }
                }
                _allVehicles[currentVehicleIndex] = LoadVehicle(currentVehicleIndex, vehiclePool.trafficCars[vehicleIndex].vehiclePrefab, trafficHolder, buildingLayers, obstacleLayers, playerLayers, roadLayers, vehiclePool.trafficCars[vehicleIndex].dontInstantiate, lightsOn, modifyTriggerSize);
                currentVehicleIndex++;
            }
        }


        /// <summary>
        /// Load vehicle in scene
        /// </summary>
        private VehicleComponent LoadVehicle(int vehicleIndex, GameObject carPrefab, Transform parent, LayerMask buildingLayers, LayerMask obstacleLayers, LayerMask playerLayers, LayerMask roadLayers, bool excluded, bool lightsOn, ModifyTriggerSize modifyTriggerSize)
        {
            VehicleComponent vehicle = MonoBehaviourUtilities.Instantiate(carPrefab, Vector3.zero, Quaternion.identity, parent).GetComponent<VehicleComponent>().Initialize(buildingLayers, obstacleLayers, playerLayers, roadLayers, lightsOn, modifyTriggerSize);
            vehicle.SetIndex(vehicleIndex);
            vehicle.name += vehicleIndex;
            vehicle.excluded = excluded;
            vehicle.DeactivateVehicle();
            return vehicle;
        }
    }
}