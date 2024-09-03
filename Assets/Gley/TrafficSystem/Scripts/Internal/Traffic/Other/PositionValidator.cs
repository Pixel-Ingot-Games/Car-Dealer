using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Used to check if a vehicle can be instantiated in a given position
    /// </summary>
    internal class PositionValidator
    {
        private readonly float _minDistanceToAdd;
        private readonly bool _debugDensity;

        private Collider[] _results;
        private Transform[] _activeCameras;
        private LayerMask _trafficLayer;
        private LayerMask _playerLayer;
        private LayerMask _buildingsLayers;




        /// <summary>
        /// Setup dependencies
        /// </summary>
        /// <param name="activeCameras"></param>
        /// <param name="trafficLayer"></param>
        /// <param name="buildingsLayers"></param>
        /// <param name="minDistanceToAdd"></param>
        /// <param name="debugDensity"></param>
        /// <returns></returns>
        internal PositionValidator(Transform[] activeCameras, LayerMask trafficLayer, LayerMask playerLayer, LayerMask buildingsLayers, float minDistanceToAdd, bool debugDensity)
        {
            UpdateCamera(activeCameras);
            _trafficLayer = trafficLayer;
            _playerLayer = playerLayer;
            _minDistanceToAdd = minDistanceToAdd * minDistanceToAdd;
            _buildingsLayers = buildingsLayers;
            _debugDensity = debugDensity;
            _results = new Collider[1];
        }


        /// <summary>
        /// Checks if a vehicle can be instantiated in a given position
        /// </summary>
        /// <param name="position">position to check</param>
        /// <param name="vehicleLength"></param>
        /// <param name="vehicleHeight"></param>
        /// <param name="ignoreLineOfSight">validate position eve if it is in view</param>
        /// <returns></returns>
        internal bool IsValid(Vector3 position, float vehicleLength, float vehicleHeight, float vehicleWidth, bool ignoreLineOfSight, float frontWheelOffset, Quaternion rotation)
        {
            position -= rotation * new Vector3(0, 0, frontWheelOffset);
            for (int i = 0; i < _activeCameras.Length; i++)
            {
                if (!ignoreLineOfSight)
                {
                    //if position if far enough from the player
                    if (Vector3.SqrMagnitude(_activeCameras[i].position - position) < _minDistanceToAdd)
                    {
                        if (!Physics.Linecast(position, _activeCameras[i].position, _buildingsLayers))
                        {
#if UNITY_EDITOR
                            if (_debugDensity)
                            {
                                Debug.Log("Density: Direct view of the camera");
                                Debug.DrawLine(_activeCameras[i].position, position, Color.red, 0.1f);
                            }
#endif
                            return false;
                        }
                        else
                        {
#if UNITY_EDITOR
                            if (_debugDensity)
                            {
                                Debug.DrawLine(_activeCameras[i].position, position, Color.green, 0.1f);
                            }
#endif
                        }
                    }
                }
            }

            //check if the final position is free 
            return IsPositionFree(position, vehicleLength, vehicleHeight, vehicleWidth, rotation);
        }


        /// <summary>
        /// Check if a given position if free
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        internal bool IsPositionFree(Vector3 position, float length, float height, float width, Quaternion rotation)
        {
            if (Physics.OverlapBoxNonAlloc(position, new Vector3(width / 2, height / 2, length / 2), _results, rotation, _trafficLayer | _playerLayer) > 0)
            {
#if UNITY_EDITOR
                if (_debugDensity)
                {
                    Debug.Log("Density: Other obstacle is blocking the waypoint");
                }
#endif
                return false;
            }
            return true;
        }


        internal bool CheckTrailerPosition(Vector3 position, Quaternion vehicleRotation, Quaternion trailerRotation, VehicleComponent vehicle)
        {
            Vector3 translatedPosition = position - vehicleRotation * Vector3.forward * (vehicle.frontTrigger.transform.localPosition.z + vehicle.carHolder.transform.localPosition.z);
            translatedPosition = translatedPosition - trailerRotation * Vector3.forward * vehicle.trailer.length / 2;
            return IsPositionFree(translatedPosition, vehicle.trailer.length, vehicle.trailer.height, vehicle.trailer.width, trailerRotation);
        }


        /// <summary>
        /// Update player camera transform
        /// </summary>
        /// <param name="activeCameras"></param>
        internal void UpdateCamera(Transform[] activeCameras)
        {
            _activeCameras = activeCameras;
        }
    }
}
