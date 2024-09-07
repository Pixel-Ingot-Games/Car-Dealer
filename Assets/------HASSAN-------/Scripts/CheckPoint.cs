using System;
using UnityEngine;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
    public GameObject nextCheckPoint;
    public bool finalCheckPoint;

    public float delayToInvokeLevelComplete;
    public UnityEvent onCheckPointCollect;
    public UnityEvent onLevelComplete;

    public void OnCheckPointCollection()
    {
        if (finalCheckPoint)
        {
            ShowLevelComplete();
        }
        else
        {
            CheckPointCollected();
        }
        if (GameConstant.Haptics)
        {
            Haptics.Instance.PlaySimple();
        }
    }
   

    private void CheckPointCollected( )
    {
        AudioManager.Instance.Play(SoundName.Checkpoint);
        onCheckPointCollect?.Invoke();
        InGameUIHandler.Instance.ShowConfetti();
        nextCheckPoint.SetActive(true);
        gameObject.SetActive(false);
    
    }
    private void ShowLevelComplete()
    {
        gameObject.SetActive(false);
        InGameUIHandler.Instance.gameplayUI.SetActive(false);
        InGameUIHandler.Instance.ShowLevelCompleteConfetti();
        FunctionTimer.CreateCountdownWithSlider(delayToInvokeLevelComplete).OnComplete(() =>
        {
            onLevelComplete?.Invoke();
        });
    }



}
