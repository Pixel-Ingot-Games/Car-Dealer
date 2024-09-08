using System.Collections;
using DhafinFawwaz.AnimationUILib;
using TMPro;
using UnityEngine;
//using UnityEngine.iOS;

public class InGameUIHandler : Singleton<InGameUIHandler>
{
    public TMP_Text coinTxt;
    public GameObject levelCompleteConfetti,collectibleConfetti;
    public GameObject gameplayUI,mobileInputs;

    public AnimationUI levelCompleteAnimation;
    public AnimationUI levelFailedAnimation;
    public AnimationUI levelPausedAnimation;
    
    
    private void OnEnable()
    {
        Coin.onCoinCollection += OnCoinCollection;
    }
    private void Start()
    {
        UpdateCoinUI();
    }


    public void PauseState(int state)
    {
        Time.timeScale = state;
    }
    
    
    private void OnCoinCollection()
    {
        FunctionTimer.CreateCountdownWithSlider(1.5f).OnComplete(() =>
        {
            EconomyManager.Instance.AddCurrency(EconomyManager.CurrencyType.Coins, 10);

            StartCoroutine(UpdateCoinsTextCoroutine(EconomyManager.Instance.GetCoins()));
        });

    }

    private void UpdateCoinUI()
    {
        coinTxt.text = EconomyManager.Instance.GetCoins().ToString();

    }
    private void OnDisable()
    {
        Coin.onCoinCollection -= OnCoinCollection;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator UpdateCoinsTextCoroutine(int targetValue, float duration = 0.5f)
    {
        var startValue = int.Parse(coinTxt.text);
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var percentageComplete = Mathf.Clamp01(elapsedTime / duration);
            var currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, percentageComplete));
            coinTxt.text = currentValue.ToString();
            yield return null;
        }

        UpdateCoinUI();
    }
    public  void ShowConfetti()
    {
        AudioManager.Instance.Play(SoundName.Confetti);
      collectibleConfetti.SetActive(true);
    }   
    // ReSharper disable Unity.PerformanceAnalysis
    public  void ShowLevelCompleteConfetti()
    {
        AudioManager.Instance.Play(SoundName.Confetti);
        levelCompleteConfetti.SetActive(true);
    }

    public void LevelCompleteReward()
    {
       var currentCoins= EconomyManager.Instance.GetCoins();
       currentCoins += 1000;
       EconomyManager.Instance.AddCurrency(EconomyManager.CurrencyType.Coins,currentCoins);
    }

    public void RateUS()
    {
        if (GameConstant.CurrentLevel%2==0)
        {
           // Device.RequestStoreReview();
        }
    }
}
