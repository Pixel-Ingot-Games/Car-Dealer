using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1 : MonoBehaviour
{
    public int maxThiefToCatchCount;
    private int currentThiefCatchCount;

    private void OnEnable()
    {
        CollionHandler.OnMaxHit += OnThiefCatch;
    }
    private void OnThiefCatch()
    {
        currentThiefCatchCount++;

        if(currentThiefCatchCount>=maxThiefToCatchCount)
        {

            ////// YAHAN SE AP CUT SCENE DHEKHA SKTIN
            Debug.Log(currentThiefCatchCount+ " Thief's Car Get Caught!");
            GameManager.Instance.EndCutScene.SetActive(true);
        }
    }
    private void OnDisbale()
    {
        CollionHandler.OnMaxHit -= OnThiefCatch;
    }
}
