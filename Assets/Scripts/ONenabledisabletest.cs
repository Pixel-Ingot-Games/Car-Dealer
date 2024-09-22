using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ONenabledisabletest : MonoBehaviour
{

    public UnityEvent onEnable;
    public UnityEvent onDisable;

    private void OnEnable()
    {
        onEnable.Invoke();
    }
    public void OnDisable()
    {
        onDisable.Invoke();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
