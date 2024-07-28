using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ashsvp
{
    public class AudioSystem : MonoBehaviour
    {
        public AudioSource engineSound;
        public AudioSource GearSound;
        [Range(0, 1)]
        public float minPitch;
        [Range(1, 3)]
        public float maxPitch;
        private GearSystem gearSystem;

        private SimcadeVehicleController SimcadeVehicleController;
        private Rigidbody vehicle_rb;

        private void Start()
        {
            gearSystem = GetComponent<GearSystem>();
            SimcadeVehicleController = GetComponent<SimcadeVehicleController>();
            vehicle_rb = GetComponent<Rigidbody>();
            engineSound.pitch = minPitch;
        }

        private void Update()
        {
            soundManager();
        }

        void soundManager()
        {

            float speed = gearSystem.VehicleSpeed;

            float enginePitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(speed) / gearSystem.gearSpeeds[Mathf.Clamp(gearSystem.currentGear, 0, 4)]);
            //if (SimcadeVehicleController.vehicleIsGrounded)
            //{
            //    engineSound.pitch = Mathf.MoveTowards(engineSound.pitch, enginePitch, 2f * Time.deltaTime);
            //}

            if (Mathf.Abs(SimcadeVehicleController.accelerationInput) > 0.1f)
            {
                engineSound.volume = Mathf.MoveTowards(engineSound.volume, 1, 1f * Time.deltaTime);
            }
            else
            {
                engineSound.volume = Mathf.MoveTowards(engineSound.volume, 0.5f, 1f * Time.deltaTime);
            }


            if (SimcadeVehicleController.vehicleIsGrounded)
            {
                if (Mathf.Abs(SimcadeVehicleController.accelerationInput) > 0.1f && SimcadeVehicleController.localVehicleVelocity.magnitude < 5f && Mathf.Abs(SimcadeVehicleController.handbrakeInput) > 0.1f)
                {
                    engineSound.pitch = Mathf.MoveTowards(engineSound.pitch, maxPitch, Time.deltaTime);
                    if (engineSound.pitch > maxPitch - 0.05f)
                    {
                        engineSound.pitch -= 0.2f;
                    }
                }
                else
                {
                    engineSound.pitch = Mathf.MoveTowards(engineSound.pitch, enginePitch, 2f * Time.deltaTime);
                }
            }
            else
            {
                if (Mathf.Abs(SimcadeVehicleController.accelerationInput) > 0.1f)
                {
                    engineSound.pitch = Mathf.MoveTowards(engineSound.pitch, maxPitch, Time.deltaTime);
                    if (engineSound.pitch > maxPitch - 0.05f  && gearSystem.currentGear <= 1)
                    {
                        engineSound.pitch -= 0.2f;
                    }
                }
                else
                {
                    engineSound.pitch = Mathf.MoveTowards(engineSound.pitch, enginePitch, Time.deltaTime);
                }
            }



        }
    }
}
