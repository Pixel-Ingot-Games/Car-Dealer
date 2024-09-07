using System;
using System.Collections.Concurrent;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            var go = new GameObject("MainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return instance;
    }

    private readonly ConcurrentQueue<Action> executionQueue = new ConcurrentQueue<Action>();

    void Update()
    {
        while (executionQueue.TryDequeue(out var action))
        {
            action();
        }
    }

    public void Enqueue(Action action)
    {
        executionQueue.Enqueue(action);
    }
}
