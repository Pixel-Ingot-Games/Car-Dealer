
using UnityEngine.Events;
[System.Serializable]
public class  TimedEvent
{
    public float delay; // Delay in seconds before the function is called
    public UnityEvent onEvent; // Event to be invoked
}