#if GLEY_TRAFFIC_SYSTEM
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Applies rotation and position to the vehicle wheels 
    /// </summary>
    [BurstCompile]
    public struct UpdateWheelJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float3> WheelsOrigin;
        [ReadOnly] public NativeArray<float3> DownDirection;
        [ReadOnly] public NativeArray<float> WheelRotation;
        [ReadOnly] public NativeArray<float> TurnAngle;
        [ReadOnly] public NativeArray<float> WheelRadius;
        [ReadOnly] public NativeArray<float> RayCastDistance;
        [ReadOnly] public NativeArray<float> MaxSuspension;
        [ReadOnly] public NativeArray<int> VehicleIndex;
        [ReadOnly] public NativeArray<bool> CanSteer;
        [ReadOnly] public int NrOfVehicles;

        public void Execute(int index, TransformAccess transform)
        {
            //apply suspension
            if (RayCastDistance[index] != 0)
            {
                //wheel is on ground
                transform.position = WheelsOrigin[index] + DownDirection[VehicleIndex[index]] * (RayCastDistance[index] - WheelRadius[index]);
            }
            else
            {
                //wheel is in the air
                transform.position = WheelsOrigin[index] + (DownDirection[VehicleIndex[index]] * MaxSuspension[VehicleIndex[index]]);
            }

            //apply rotation
            if (CanSteer[index])
            {
                transform.localRotation = quaternion.EulerXYZ(math.radians(WheelRotation[VehicleIndex[index]]), math.radians(TurnAngle[VehicleIndex[index]]), 0);
            }
            else
            {
                transform.localRotation = quaternion.EulerXYZ(math.radians(WheelRotation[VehicleIndex[index]]), 0, 0);
            }
        }
    }
}
#endif
