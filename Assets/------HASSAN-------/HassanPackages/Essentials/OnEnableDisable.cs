using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableDisable : MonoBehaviour
{
    public UnityEvent onEnable;
    public UnityEvent onEnableDelay;
    public UnityEvent onDisbale;
    public UnityEvent onDisbaleDelay;
    public float onEnableDelayTimer;
    public float onDisableDelayTimer;
    

    private void OnEnable()
    {
        onEnable?.Invoke();
        Invoke(nameof(OnEnableDelayEvent), onEnableDelayTimer);
    }
    private void OnDisable()
    {
        onDisbale?.Invoke();

        FunctionTimer.CreateCountdownWithSlider(onDisableDelayTimer).OnComplete(() => { onDisbaleDelay?.Invoke(); });
    }

    private void OnEnableDelayEvent()
    {
        onEnableDelay?.Invoke();
    }   
    
    
}
