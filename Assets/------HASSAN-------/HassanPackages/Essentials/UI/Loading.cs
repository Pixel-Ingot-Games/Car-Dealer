using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class Loading : MonoBehaviour
{
    public Slider slider;
    public float loadingDurationInSeconds = 5.0f; // Set the duration of loading in seconds
    public string nextSceneName;

    private float TargetValue = 1.0f;
    public bool dontChangeScene;
    public bool hideSelf;
    private float _startTime;
    private float _percentageComplete;
    private static readonly Action OnLoadingComplete = null;
    public UnityEvent onLoadingCompleteUe;

    
    private void OnEnable()
    {
        _startTime = Time.time;
    }

    private void Update()
    {
        var elapsedTime = Time.time - _startTime;
        _percentageComplete = elapsedTime / loadingDurationInSeconds;

        if (_percentageComplete < 1.0f)
        {
            slider.value = Mathf.Lerp(0, TargetValue, _percentageComplete);
        }
        else
        {
            slider.value = TargetValue;
            if (!dontChangeScene)
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                
            }
            else
            {

                OnLoadingComplete?.Invoke();
                onLoadingCompleteUe?.Invoke();
                if (hideSelf)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnDisable()
    {
        slider.value = 0;
        _percentageComplete = 0;
    }
}
