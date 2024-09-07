using UnityEngine;
using System;

public class EconomyManager : Singleton<EconomyManager>
{
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnGemsChanged;


    private EconomyUI _economyUI;
    public enum CurrencyType
    {
        Coins,
        Gems
    }

    [SerializeField] private int initialCoins = 1000;
    [SerializeField] private int initialGems = 100;

    private int _coins;
    private int _gems;

    private void Awake()
    {
      
        _economyUI = GetComponent<EconomyUI>();
        LoadEconomyData();
        _economyUI.UpdateUI();
    }

 

    public int GetCoins() => _coins;
    public int GetGems() => _gems;

    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;
        switch (type)
        {
            case CurrencyType.Coins:
                _coins += amount;
                OnCoinsChanged?.Invoke(_coins);
                break;
            case CurrencyType.Gems:
                _gems += amount;
                OnGemsChanged?.Invoke(_gems);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        SaveEconomyData();
    }

    public bool DeductCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return false;
        switch (type)
        {
            case CurrencyType.Coins:
                if (_coins >= amount)
                {
                    _coins -= amount;
                    OnCoinsChanged?.Invoke(_coins);
                    SaveEconomyData();
                    _economyUI.UpdateUI();
                    return true;
                }
                break;
            case CurrencyType.Gems:
                if (_gems >= amount)
                {
                    _gems -= amount;
                    OnGemsChanged?.Invoke(_gems);
                    SaveEconomyData();
                    return true;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        return false;
    }

    public bool CanAfford(CurrencyType type, int amount)
    {
        return type switch
        {
            CurrencyType.Coins => _coins >= amount,
            CurrencyType.Gems => _gems >= amount,
            _ => false
        };
    }

    public void ResetEconomy()
    {
        _coins = initialCoins;
        _gems = initialGems;
        OnCoinsChanged?.Invoke(_coins);
        OnGemsChanged?.Invoke(_gems);
        SaveEconomyData();
    }

    private void SaveEconomyData()
    {
        PlayerPrefs.SetInt("Coins", _coins);
        PlayerPrefs.SetInt("Gems", _gems);
        PlayerPrefs.Save();
    }

    private void LoadEconomyData()
    {
        _coins = PlayerPrefs.GetInt("Coins", initialCoins);
        _gems = PlayerPrefs.GetInt("Gems", initialGems);
        OnCoinsChanged?.Invoke(_coins);
        OnGemsChanged?.Invoke(_gems);
    }
}
