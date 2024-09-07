using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float initialTime = 10f; 
    [SerializeField] private TMP_Text timerTxt; 


    private float remainingTime;
    private bool isTimerRunning = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onTimerEnd; // Event to invoke when the timer reaches zero

    private void Start()
    {
        // Initialize the timer
        InitializeTimer();
 
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    /// <summary>
    /// Starts the countdown timer.
    /// </summary>
    public void StartTimer()
    {
        if (isTimerRunning) return;
        isTimerRunning = true;
        remainingTime = initialTime;
    }

    /// <summary>
    /// Pauses the countdown timer.
    /// </summary>
    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    /// <summary>
    /// Resumes the countdown timer if it is paused.
    /// </summary>
    public void ResumeTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
        }
    }

    /// <summary>
    /// Resets the countdown timer to the initial time.
    /// </summary>
    public void ResetTimer()
    {
        remainingTime = initialTime;
    }

    /// <summary>
    /// Adds a specified amount of time to the remaining time.
    /// </summary>
    /// <param name="timeToAdd">Time to add in seconds.</param>
    public void AddTime(float timeToAdd)
    {
        if (timeToAdd > 0)
        {
            remainingTime += timeToAdd;
        }
    }

    /// <summary>
    /// Initializes the timer with the initial time.
    /// </summary>
    private void InitializeTimer()
    {
        remainingTime = initialTime;
    }

    /// <summary>
    /// Updates the remaining time and checks if the timer has ended.
    /// </summary>
    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            TimerEnded();
        }
        timerTxt.text = "TIME:-  " + remainingTime.ToString("F0");
    }

    /// <summary>
    /// Invokes the event when the timer ends.
    /// </summary>
    private void TimerEnded()
    {
        isTimerRunning = false; // Ensure the timer stops
        onTimerEnd?.Invoke();
    }

    /// <summary>
    /// Sets the initial time and resets the timer.
    /// </summary>
    /// <param name="newInitialTime">New initial time in seconds.</param>
    public void SetInitialTime(float newInitialTime)
    {
        initialTime = newInitialTime;
        ResetTimer();
    }
}
