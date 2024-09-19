using System;
using UnityEngine;

public class CollionHandler : MonoBehaviour
{
    public int maxCollisonCount;
    private int currentCollisonCount;
    public static Action OnMaxHit;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PoliceCar"))
        {
            currentCollisonCount++;
            if(currentCollisonCount >= maxCollisonCount)
            {
                OnMaxHit?.Invoke();
            }
        }
    }
}
