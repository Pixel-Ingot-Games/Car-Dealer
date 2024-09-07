using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FunctionTimer
{
    private static List<FunctionTimer> activeTimerList;
    private static GameObject initGameObject;

    private static void InitIfNeeded()
    {
        if (initGameObject) return;
        initGameObject = new GameObject("FunctionTimer_InitGameObject");
        activeTimerList = new List<FunctionTimer>();
    }
    public static void StopTimerByName(string timerName)
    {
        InitIfNeeded();
        foreach (var timer in activeTimerList.Where(timer => timer.timerName == timerName))
        {
            timer.Stop();
            timer._imageFill.gameObject.SetActive(false);
            
            return; // Stop after stopping the first matching timer
        }
        Debug.LogWarning("Timer with name '" + timerName + "' not found.");
    }

    public static FunctionTimer CreateCountdownWithSlider(float timer, string timerName = null, Slider slider = null, bool countdown = false)
    {
        InitIfNeeded();
        GameObject gameObject = new GameObject("FunctionTimer", typeof(MonoBehaviourHook));

        FunctionTimer functionTimer = new FunctionTimer(timer, timerName, gameObject, slider, countdown);

        gameObject.GetComponent<MonoBehaviourHook>().onUpdate = functionTimer.Update;

        activeTimerList.Add(functionTimer);

        return functionTimer;
    }
  
    public static FunctionTimer CreateCountdownWithImageFill(float timer, string timerName = null, Image imageFill = null)
    {
        InitIfNeeded();
        GameObject gameObject = new GameObject("FunctionTimer", typeof(MonoBehaviourHook));

        FunctionTimer functionTimer = new FunctionTimer(timer, timerName, gameObject, imageFill);

        gameObject.GetComponent<MonoBehaviourHook>().onUpdate = functionTimer.Update;

        activeTimerList.Add(functionTimer);

        return functionTimer;
    }


    #region Constructor Variables
    private Slider _slider;
    private Image _imageFill;
    private Action onComplete;
    private Action onStart;
    private Action onValueUpdate;
    private float timer;
    private float _currentTime;
    private string timerName;
    private GameObject gameObject;
    private bool isDestroyed;
    private bool _countdown;
    private bool startTriggered;
    #endregion

    #region Constructors
    private FunctionTimer(float timer, string timerName, GameObject gameObject, Slider slider = null, bool countdown = false)
    {
        this.timer = timer;
        this.timerName = timerName;
        this.gameObject = gameObject;
        this._countdown = countdown;
        _currentTime = 0;
        startTriggered = false;
        isDestroyed = false;
        if (slider != null)
        {
            _slider = slider;
            _slider.maxValue = this.timer;
            _slider.value = _countdown ? timer : 0f; // Initialize fill amount
        }
    }

    private FunctionTimer(float timer, string timerName, GameObject gameObject, Image imageFill = null)
    {
        this.timer = timer;
        this.timerName = timerName;
        this.gameObject = gameObject;
        this._countdown = false;
        _currentTime = 0;
        startTriggered = false;
        isDestroyed = false;
        if (imageFill == null) return;
        _imageFill = imageFill;
        _imageFill.type = Image.Type.Filled;
        _imageFill.fillAmount = _countdown ? 1f : 0f; // Initialize fill amount
    }
    #endregion

    #region CallBack Events
    public FunctionTimer OnComplete(Action action)
    {
        this.onComplete = action;
        return this;
    }

    public FunctionTimer OnStart(Action action)
    {
        this.onStart = action;
        return this;
    }

    public FunctionTimer OnValueUpdate(Action action)
    {
        this.onValueUpdate = action;
        return this;
    }
    #endregion
    public void Update()
    {
        if (!isDestroyed)
        {
            if (!startTriggered)
            {
                if (_slider)
                {
                    _slider.gameObject.SetActive(true);
                }
                else if (_imageFill)
                {
                    _imageFill.gameObject.SetActive(true);
                }
                onStart?.Invoke();
                startTriggered = true;
            }
            onValueUpdate?.Invoke();
            if (_countdown)
            {
                timer -= Time.deltaTime;
                if (_slider != null)
                {
                    _slider.value = timer;
                }
                if (timer <= 0)
                {
                    onComplete?.Invoke();
                    DestroySelf();
                    Debug.Log("<color=red>Timer completed: " + timerName + "</color>");
                }
            }
            else
            {
                _currentTime += Time.deltaTime;
                if (_slider != null)
                {
                    _slider.value = _currentTime;
                }
                else if (_imageFill != null)
                {
                    _imageFill.fillAmount = _currentTime / this.timer;
                }
                if (_currentTime >= this.timer)
                {
                    if (_slider)
                    {
                        _slider.gameObject.SetActive(false);
                    }
                    else if (_imageFill)
                    {
                        _imageFill.gameObject.SetActive(false);
                    }
                    onComplete?.Invoke();
                    DestroySelf();
                }
            }
        }
    }

    private void DestroySelf()
    {
        isDestroyed = true;
        UnityEngine.Object.Destroy(gameObject);
        RemoveTimer(this);
    }

    private void RemoveTimer(FunctionTimer functionTimer)
    {
        InitIfNeeded();
        activeTimerList.Remove(functionTimer);
    }
    public void Stop()
    {
        if (!isDestroyed)
        {
            DestroySelf();
            Debug.Log("<color=orange>Timer stopped: " + timerName + "</color>");
        }
    }
    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action onUpdate;

        private void Update()
        {
            onUpdate?.Invoke();
        }
    }
}
