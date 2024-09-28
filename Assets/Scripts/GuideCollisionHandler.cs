using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideCollisionHandler : MonoBehaviour
{
    // This method is called when another object enters the trigger collider attached to the guide object
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the tag "PoliceCar"
        if (other.CompareTag("PoliceCar"))
        {
            // Destroy the current object (guide)
            Destroy(gameObject);
        }
    }
}
