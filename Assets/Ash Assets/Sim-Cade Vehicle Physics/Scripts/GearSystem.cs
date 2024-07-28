using System.Collections;
using UnityEngine;
using Unity;

namespace Ashsvp
{
    public class GearSystem : MonoBehaviour
    {
        public float VehicleSpeed;
        public int currentGear;
        private SimcadeVehicleController vehicleController;
        public int[] gearSpeeds = new int[] { 40, 80, 120, 160, 220 };

        public AudioSystem AudioSystem;

        private int currentGearTemp;
        void Start()
        {
            vehicleController = GetComponent<SimcadeVehicleController>();
            currentGear = 1;

        }

        void Update()
        {
            float velocityMag = Vector3.ProjectOnPlane( vehicleController.localVehicleVelocity, transform.up).magnitude;
            if (vehicleController.vehicleIsGrounded)
            {
                velocityMag = vehicleController.localVehicleVelocity.magnitude;
            }

            VehicleSpeed = Mathf.RoundToInt(velocityMag * 3.6f); //car speed in Km/hr

            gearShift();


        }


        void gearShift()
        {
            for (int i = 0; i < gearSpeeds.Length; i++)
            {
                if (VehicleSpeed > gearSpeeds[i])
                {
                    currentGear = i + 1;
                }
                else break;
            }
            if (CurrentGearProperty != currentGear)
            {
                CurrentGearProperty = currentGear;
            }

        }

        public int CurrentGearProperty
        {
            get
            {
                return currentGearTemp;
            }

            set
            {
                currentGearTemp = value;

                if (vehicleController.accelerationInput > 0 && vehicleController.localVehicleVelocity.z > 0 && !AudioSystem.GearSound.isPlaying && vehicleController.vehicleIsGrounded)
                {
                    vehicleController.VehicleEvents.OnGearChange.Invoke();
                    AudioSystem.GearSound.Play();
                    StartCoroutine(shiftingGear());
                }

                AudioSystem.engineSound.volume = 0.5f;
            }
        }

        IEnumerator shiftingGear()
        {
            vehicleController.CanAccelerate = false;
            yield return new WaitForSeconds(0.3f);
            vehicleController.CanAccelerate = true;
        }

    }
}
