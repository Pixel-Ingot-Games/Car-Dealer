using TMPro;
using UnityEngine;

public class EconomyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;

    private void OnEnable()
    {
        // Subscribe to events
        EconomyManager.Instance.OnCoinsChanged += UpdateCoins;
        EconomyManager.Instance.OnGemsChanged += UpdateGems;
        UpdateUI();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EconomyManager.Instance.OnCoinsChanged -= UpdateCoins;
        EconomyManager.Instance.OnGemsChanged -= UpdateGems;
    }

    public void UpdateCoins(int coins)
    {
        if (coinsText)
        {
            coinsText.text = coins.ToString();
        }
    }

    public void UpdateGems(int gems)
    {
        if (gemsText)
        {
            gemsText.text = gems.ToString();
        }
    }

    public void UpdateUI()
    {
        var economyManager = EconomyManager.Instance;
        if (economyManager != null)
        {
            UpdateCoins(economyManager.GetCoins());
            UpdateGems(economyManager.GetGems());
        }
    }
}