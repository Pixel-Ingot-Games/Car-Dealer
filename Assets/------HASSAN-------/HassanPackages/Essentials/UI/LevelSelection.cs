using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] private Level[] levels;
    [SerializeField] private Button nextBtn;

    private Level _selectedLevel;
    private const string UnlockedLevelsKey = "UnlockedLevels";

    private void Awake()
    {
        LoadLevelStates();
        UpdateLevelUI();
    }

    private void OnEnable()
    {
        nextBtn.gameObject.SetActive(false);
    }

    public void SelectLevel(int id)
    {
        nextBtn.gameObject.SetActive(true);
        AudioManager.Instance.Play(SoundName.Click);
        foreach (var level in levels)
        {
            level.SelectedLevelUI.SetActive(false);
            if (level.LevelId != id) continue;
            _selectedLevel = level;
            GameConstant.CurrentLevel = id;
            _selectedLevel.SelectedLevelUI.SetActive(true);
        }
    }

    private void UnlockLevel(int id)
    {
        var level = levels.FirstOrDefault(l => l.LevelId == id);
        if (level == null || level.IsUnlocked) return;
        level.IsUnlocked = true;
        SaveLevelStates();
        UpdateLevelUI();

    }

    public void CompleteLevel(int id)
    {
        var currentLevel = levels.FirstOrDefault(l => l.LevelId == id);
        if (currentLevel != null)
        {
            UnlockLevel(id + 1);
        }
    }

    private void SaveLevelStates()
    {
        var unlockedLevels = levels.Where(l => l.IsUnlocked).Select(l => l.LevelId).ToArray();
        PlayerPrefs.SetString(UnlockedLevelsKey, string.Join(",", unlockedLevels));
        PlayerPrefs.Save();
    }



    (int ps, Vector3 pos) GetStat()
    {
        return (5,transform.position);
    }

    private void LoadLevelStates()
    {
        int a= GetStat().ps;

        var unlockedLevelsString = PlayerPrefs.GetString(UnlockedLevelsKey, "1");
        var unlockedLevels = new HashSet<int>(unlockedLevelsString.Split(',').Select(int.Parse));

        foreach (var level in levels)
        {
            level.IsUnlocked = unlockedLevels.Contains(level.LevelId);
        }
    }

    private void UpdateLevelUI()
    {
        foreach (var level in levels)
        {
            level.LockedLevelUI.SetActive(!level.IsUnlocked);
            level.LevelButton.interactable = level.IsUnlocked;
        }
    }
}

[System.Serializable]
public class Level
{
    [SerializeField] private int levelId;
    [SerializeField] private Button levelButton;
    [SerializeField] private GameObject selectedLevelUI;
    [SerializeField] private GameObject lockedLevelUI;

    public int LevelId => levelId;
    public Button LevelButton => levelButton;
    public GameObject SelectedLevelUI => selectedLevelUI;
    public GameObject LockedLevelUI => lockedLevelUI;

    public bool IsUnlocked { get; set; }
}
