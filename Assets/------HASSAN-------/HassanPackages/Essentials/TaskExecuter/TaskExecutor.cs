using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class TaskExecutor : IDisposable
{
    private Action onStart;
    private Action onUpdate;
    private Action onComplete;
    private Action onPause;
    private Action onResume;
    private readonly Action taskAction;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private bool isRunning = false;
    private bool disposed = false;
    private bool isPaused = false;
    private readonly object pauseLock = new();

    public TaskExecutor(Action taskAction)
    {
        this.taskAction = taskAction;
    }

    public TaskExecutor OnStart(Action action)
    {
        this.onStart = action;
        return this;
    }

    public TaskExecutor OnUpdate(Action action)
    {
        this.onUpdate = action;
        return this;
    }

    public TaskExecutor OnComplete(Action action)
    {
        this.onComplete = action;
        return this;
    }

    public TaskExecutor OnPause(Action action)
    {
        this.onPause = action;
        return this;
    }

    public TaskExecutor OnResume(Action action)
    {
        this.onResume = action;
        return this;
    }

    public async Task StartAsync()
    {
        if (isRunning)
        {
            Debug.LogWarning("Task is already running.");
            return;
        }

        isRunning = true;
        onStart?.Invoke();

        try
        {
            while (true)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                lock (pauseLock)
                {
                    while (isPaused)
                    {
                        Monitor.Wait(pauseLock);
                    }
                }

                if (disposed) return; // Exit if disposed

                // Execute the main task
                taskAction?.Invoke();

                // Call the onUpdate callback on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() => onUpdate?.Invoke());

                // Sleep for a short duration to simulate ongoing work and prevent a busy loop
                await Task.Delay(100); // Use async delay instead of Thread.Sleep
            }
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled
        }
        finally
        {
            isRunning = false;
            if (!disposed) // Check if disposed to avoid invoking onComplete if disposed
                UnityMainThreadDispatcher.Instance().Enqueue(() => onComplete?.Invoke());
        }
    }

    public void Stop()
    {
        if (disposed || !isRunning) return; // Do nothing if already disposed or not running
        cancellationTokenSource.Cancel();
    }

    public void Pause()
    {
        if (disposed || !isRunning || isPaused) return; // Do nothing if disposed, not running, or already paused

        lock (pauseLock)
        {
            isPaused = true;
            onPause?.Invoke();
        }
    }

    public void Resume()
    {
        if (disposed || !isRunning || !isPaused) return; // Do nothing if disposed, not running, or not paused

        lock (pauseLock)
        {
            isPaused = false;
            onResume?.Invoke();
            Monitor.Pulse(pauseLock); // Wake up the paused task
        }
    }

    public void Dispose()
    {
        if (disposed) return; // Prevent multiple disposals
        disposed = true;
        Stop();
        cancellationTokenSource.Dispose();
    }
}
