using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] private Character[] characters;
    [SerializeField] private Button selectBtn;
    [SerializeField] private Button purchaseBtn;
    [SerializeField] private TMP_Text characterPrice;

    // UI elements for displaying character attributes
    [SerializeField] private Image healthSlider;
    [SerializeField] private Image staminaSlider;
    [SerializeField] private Image powerSlider;
    [SerializeField] private Image geneticsSlider;

    private int _currentCharacterIndex;
    private Character _selectedCharacter,_previousCharacter;
    private const string UnlockedCharactersKey = "UnlockedCharacters";

/*    private void Awake()
    {
        // Display the first character by default
        LoadCharacterStates();
        UpdateCharacterUI();
        DisplayCharacter(_currentCharacterIndex);
    }*/

    private void OnEnable()
    {
        selectBtn.gameObject.SetActive(false);
        purchaseBtn.gameObject.SetActive(false);
        _selectedCharacter=characters[0];
        _previousCharacter=characters[0];
        UpdateCharacterSliders(_selectedCharacter);
        LoadCharacterStates();
        UpdateCharacterUI();
        DisplayCharacter(_currentCharacterIndex);
    }

    public void SelectCharacter()
    {
        GameConstant.HorseBodyMaterial = _currentCharacterIndex;
        SceneManager.LoadScene("Gameplay");
    }

    public void NextCharacter()
    {
        _currentCharacterIndex++;
        if (_currentCharacterIndex >= characters.Length)
        {
            _currentCharacterIndex = 0; // Wrap around to the first character
        }
        DisplayCharacter(_currentCharacterIndex);


    }

    public void PreviousCharacter()
    {
        _currentCharacterIndex--;
        if (_currentCharacterIndex < 0)
        {
            _currentCharacterIndex = characters.Length - 1; // Wrap around to the last character
        }
        DisplayCharacter(_currentCharacterIndex);
    }


    private void DisplayCharacter(int index)
    {
        _previousCharacter = _selectedCharacter;
        _selectedCharacter = characters[index];
        if (Camera.main != null)
        {
            Camera.main.transform.DOMove(_selectedCharacter.camPosition.position, 1f).SetEase(Ease.InOutQuad);
            Camera.main.transform.DORotateQuaternion(_selectedCharacter.camPosition.rotation, 1f).SetEase(Ease.InOutQuad);
        }
        UpdateCharacterSliders(_selectedCharacter);
        UpdateCharacterUI();
    }

    private void UpdateCharacterSliders(Character character)
    {
        DOTween.To(() => healthSlider.fillAmount, x => healthSlider.fillAmount = x, character.Health, 0.5f);
        DOTween.To(() => staminaSlider.fillAmount, x => staminaSlider.fillAmount = x, character.Stamina, 0.5f);
        DOTween.To(() => powerSlider.fillAmount, x => powerSlider.fillAmount = x, character.Power, 0.5f);
        DOTween.To(() => geneticsSlider.fillAmount, x => geneticsSlider.fillAmount = x, character.Genetics, 0.5f);
    }


    private void UnlockCharacter()
    {
        if (_selectedCharacter == null || _selectedCharacter.isUnlocked) return;
        _selectedCharacter.isUnlocked = true;
        SaveCharacterStates();
        UpdateCharacterUI();
    }

    public void PurchaseCharacter()
    {
        if (EconomyManager.Instance.GetCoins()>=_selectedCharacter.Price)
        {
            EconomyManager.Instance.DeductCurrency(EconomyManager.CurrencyType.Coins,
               _selectedCharacter.Price);
            UnlockCharacter();
        }
        else
        {
            UIManager.Instance.ShowPanelOverlay<NotEnoughMoneyPanel>();
        }
    }

    private void SaveCharacterStates()
    {
        var unlockedCharacters = characters.Where(c => c.isUnlocked).Select(c => c.CharacterId).ToArray();
        PlayerPrefs.SetString(UnlockedCharactersKey, string.Join(",", unlockedCharacters));
        PlayerPrefs.Save();
    }

    private void LoadCharacterStates()
    {
        var unlockedCharactersString = PlayerPrefs.GetString(UnlockedCharactersKey, "0");
        var unlockedCharacters = new HashSet<int>(unlockedCharactersString.Split(',').Select(int.Parse));

        foreach (var character in characters)
        {
            character.isUnlocked = unlockedCharacters.Contains(character.CharacterId);
        }
    }

    private void UpdateCharacterUI()
    {
        selectBtn.gameObject.SetActive(_selectedCharacter.isUnlocked);
        purchaseBtn.gameObject.SetActive(!_selectedCharacter.isUnlocked);
        characterPrice.text = _selectedCharacter.Price.ToString();
    }
}

[System.Serializable]
public class Character
{
    [SerializeField] private int characterId;
    [SerializeField] private int price;

    [Range(0, 1)][SerializeField] private float health;
    [Range(0, 1)][SerializeField] private float stamina;
    [Range(0, 1)][SerializeField] private float power;
    [Range(0, 1)][SerializeField] private float genetics;
    
    public Transform camPosition;
    public int CharacterId => characterId;
    public int Price => price;
    public float Health => health;
    public float Stamina => stamina;
    public float Power => power;
    public float Genetics => genetics;
    public bool isUnlocked;
}
