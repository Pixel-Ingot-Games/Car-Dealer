using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2 : MonoBehaviour
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

        if (currentThiefCatchCount >= maxThiefToCatchCount)
        {

         
            Debug.Log(currentThiefCatchCount + " Thief's Car Get Caught!");
            GameManager.Instance.Lvl2EndScene.SetActive(true);
        }
    }
    private void OnDisbale()
    {
        CollionHandler.OnMaxHit -= OnThiefCatch;
    }
}
