using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEventScheduler : MonoBehaviour
{
    public List<TimedEvent> timedEvents;

    public void StartEventCalling()
    {
        StartCoroutine(InvokeTimedEvents());
    }

    private IEnumerator InvokeTimedEvents()
    {
        foreach (var timedEvent in timedEvents)
        {
            yield return new WaitForSeconds(timedEvent.delay);
            timedEvent.onEvent.Invoke();
        }
    }
}