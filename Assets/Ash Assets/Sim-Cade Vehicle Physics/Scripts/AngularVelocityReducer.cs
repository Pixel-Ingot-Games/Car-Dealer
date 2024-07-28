using UnityEngine;

public class AngularVelocityReducer : MonoBehaviour
{
    // Enumeration for the axis
    public enum Axis { X, Y, Z };

    // Reference to the Rigidbody component
    public Rigidbody rb;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    public void cancelAngularSpeed_local_X(float Amount)
    {
        CancelLocalAxisAngularSpeed(Amount, rb.transform.right);
    }
    public void cancelAngularSpeed_local_Y(float Amount)
    {
        CancelLocalAxisAngularSpeed(Amount, rb.transform.up);
    }
    public void cancelAngularSpeed_local_Z(float Amount)
    {
        CancelLocalAxisAngularSpeed(Amount, rb.transform.forward);
    }

    // Function to cancel a percentage of the angular speed along a specified local axis
    public void CancelLocalAxisAngularSpeed(float percentage, Vector3 axis)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody not assigned or found!");
            return;
        }

        // Ensure percentage is between 0 and 1
        percentage = Mathf.Clamp01(percentage);

        Vector3 angularVel = rb.angularVelocity;
        Vector3 angVelInAxis = Vector3.Project(angularVel, axis);
        angularVel -= angVelInAxis * percentage;

        // Convert the modified angular velocity back to world space and apply it
        rb.angularVelocity = angularVel;
    }
}
