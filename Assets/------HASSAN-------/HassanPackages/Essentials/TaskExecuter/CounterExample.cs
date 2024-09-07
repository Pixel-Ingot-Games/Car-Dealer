using TMPro;
using UnityEngine;

public class CounterExample : MonoBehaviour
{
    private TaskExecutor taskExecutor;
  //  public TMP_Text counter;

    private int currentCount = 0;
    private const int maxCount = 20;

    public void StartCounting()
    {
        // Ensure previous task executor is disposed of before creating a new one
        taskExecutor?.Dispose();

        taskExecutor = new TaskExecutor(CountFromZeroToThousand);

        /*taskExecutor.OnStart(() => Debug.Log("COUNT STARTED"))
                    .OnUpdate(() => UpdateCounterText())
                    .OnComplete(() => counter.text = "COUNT COMPLETED")
                    .OnPause(() => counter.text = "COUNT PAUSED")
                    .OnResume(() => counter.text = "COUNT RESUMED");*/     
        
        taskExecutor.OnStart(() => Debug.Log("COUNT STARTED"))
                    .OnUpdate(() => Debug.Log("COUNTing "+ currentCount))
                    .OnComplete(() => Debug.Log("COMPLETE "))
                    .OnPause(() => Debug.Log("COUNT PAUSED " ))
                    .OnResume(() => Debug.Log("RESUME "));

        _ = taskExecutor.StartAsync();
    }

    private void UpdateCounterText()
    {
        // Make sure this runs on the main thread, in case the TaskExecutor is using it incorrectly
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
           // counter.text = currentCount.ToString();
        });
    }

    private void CountFromZeroToThousand()
    {
        if (currentCount < maxCount)
        {
            currentCount++;
        }
        else
        {
            taskExecutor.Stop(); // Stop the executor once the count reaches 1000
        }
    }

    public void StopCounting()
    {
        taskExecutor?.Stop(); // Method to stop counting anytime
        currentCount = 0; // Reset the count
        UpdateCounterText(); // Reset UI text
    }

    public void PauseCounting()
    {
        taskExecutor?.Pause(); // Method to pause counting
    }

    public void ResumeCounting()
    {
        taskExecutor?.Resume(); // Method to resume counting
    }

    private void OnDestroy()
    {
        taskExecutor?.Dispose(); // Ensure cleanup when the game object is destroyed
    }

    private void OnApplicationQuit()
    {
        taskExecutor?.Dispose(); // Ensure cleanup when the application is quitting
    }
}
