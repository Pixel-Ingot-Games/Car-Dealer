using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    public GameObject[] lvls;
    private void OnEnable()
    {
        foreach (var lvl in lvls)
        {
            lvl.SetActive(false);
        }

        var levelIndex = (GameConstant.CurrentLevel - 1) % lvls.Length;

        if (lvls.Length > 0 && levelIndex >= 0 && levelIndex < lvls.Length)
        {
            lvls[levelIndex].SetActive(true);
        }
    }

    public void NextLevel()
    {
        // Retrieve and increment the current level index
        var newLevelIndex = ++GameConstant.CurrentLevel;

        // Retrieve the existing unlocked levels from PlayerPrefs
        var unlockedLevelsStr = PlayerPrefs.GetString("UnlockedLevels", "1");

        // Convert the existing levels to a HashSet for efficient lookup and update
        var unlockedLevels = string.IsNullOrEmpty(unlockedLevelsStr)
            ? new HashSet<int>()
            : new HashSet<int>(unlockedLevelsStr.Split(',').Select(int.Parse));

        // Add the new level if it�s not already present
        unlockedLevels.Add(newLevelIndex);

        // Save the updated list back to PlayerPrefs as a comma-separated string
        PlayerPrefs.SetString("UnlockedLevels", string.Join(",", unlockedLevels));
    }

}
