using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace Ashsvp
{
    public class CustomBodyFriction : MonoBehaviour
    {
        public SimcadeVehicleController SimcadeVehicleController;
        public Collider bodyCollider;
        private PhysicMaterial bodyFrictionMaterial;
        public float LowerBodyFriction = 0f;
        public float UpperBodyFriction = 0.5f;

        private void Start()
        {
            bodyFrictionMaterial = new PhysicMaterial();
            bodyFrictionMaterial.dynamicFriction = LowerBodyFriction;
            bodyFrictionMaterial.staticFriction = LowerBodyFriction;
            bodyFrictionMaterial.bounciness = 0;
            bodyFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
            bodyFrictionMaterial.bounceCombine = PhysicMaterialCombine.Minimum;

            if (bodyCollider != null)
            {
                bodyCollider.material = bodyFrictionMaterial;
            }
            else
            {
                Debug.LogError("BodyFriction: bodyCollider is Null");
            }

            SimcadeVehicleController.VehicleEvents.OnGrounded.AddListener(resetFrictionMatProperties);

        }

        private void resetFrictionMatProperties()
        {
            bodyFrictionMaterial.staticFriction = LowerBodyFriction;
            bodyFrictionMaterial.dynamicFriction = LowerBodyFriction;
        }

        private void OnCollisionEnter(Collision collision)
        {
            CustomBodyFrictionLogic(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            resetFrictionMatProperties();
        }

        private void CustomBodyFrictionLogic(Collision collision)
        {
            Vector3 contactPoint = collision.GetContact(0).point;
            
            // Transform the contact point to the vehicle's local space
            Vector3 localContactPoint = transform.InverseTransformPoint(contactPoint);
            bool isLowerHalf = localContactPoint.y < 0;
            
            // Determine the coefficient of friction based on the contact point's location
            float coefficientOfFriction = isLowerHalf ? LowerBodyFriction : UpperBodyFriction;

            if (SimcadeVehicleController.vehicleIsGrounded)
            {
                coefficientOfFriction = LowerBodyFriction;
            }

            bodyFrictionMaterial.staticFriction = coefficientOfFriction;
            bodyFrictionMaterial.dynamicFriction = coefficientOfFriction;

        }
    }
}
