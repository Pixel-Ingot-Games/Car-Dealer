using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalAxisTorqueController : MonoBehaviour
{
    public Rigidbody rb; // Reference to the Rigidbody component
    public KeyCode rotateLeftKey = KeyCode.Q; // Default key for rotating left
    public KeyCode rotateRightKey = KeyCode.E; // Default key for rotating right
    public float torqueAmount = 10f; // Amount of torque to apply

    void Awake()
    {
        // Ensure there's a Rigidbody component attached to the GameObject
        if (!rb) rb = GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        // Check for player input and apply torque accordingly
        if (Input.GetKey(rotateLeftKey))
        {
            ApplyTorque(-torqueAmount);
        }
        else if (Input.GetKey(rotateRightKey))
        {
            ApplyTorque(torqueAmount);
        }
    }

    void ApplyTorque(float amount)
    {
        // Apply torque in the local up axis
        rb.AddTorque(transform.up * amount, ForceMode.Acceleration);
    }
}
