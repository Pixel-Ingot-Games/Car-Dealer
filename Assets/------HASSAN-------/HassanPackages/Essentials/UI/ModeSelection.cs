using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelection : MonoBehaviour
{
    public Modes[] chooseMode;
    public Button careerNextBtn, freeNextBtn;
    private Modes _chosenMode;

    private void OnEnable()
    {
        // Initially hide both next buttons
        careerNextBtn.gameObject.SetActive(false);
        freeNextBtn.gameObject.SetActive(false);

        var unlockedLevelsStr = PlayerPrefs.GetString("UnlockedLevels", "1");

        // Convert the existing levels to a HashSet for efficient lookup and update
        var unlockedLevels = string.IsNullOrEmpty(unlockedLevelsStr)
            ? new HashSet<int>()
            : new HashSet<int>(unlockedLevelsStr.Split(',').Select(int.Parse));

        // Check if at least 2 levels are unlocked to unlock Free mode
        var freeModeUnlocked = unlockedLevels.Count >= 2;

        // Loop through the array with a for loop to modify elements
        for (var i = 0; i < chooseMode.Length; i++)
        {
            var mode = chooseMode[i];

            // Update the unlocked state based on the level count
            if (mode.modeName == ModeName.Free)
            {
                mode.isUnlocked = freeModeUnlocked;
            }

            // Update UI elements based on the unlocked state
            mode.selectedModeUI.SetActive(false);  // Hide selection UI by default

            if (mode.isUnlocked)
            {
                mode.lockedUI.SetActive(false);  // Hide locked UI
                mode.modeBtn.interactable = true;  // Enable button
            }
            else
            {
                mode.lockedUI.SetActive(true);  // Show locked UI
                mode.modeBtn.interactable = false;  // Disable button
            }

            // Update the modified mode back into the array
            chooseMode[i] = mode;
        }
    }


    public void SelectedMode(int id)
    {
        AudioManager.Instance.Play(SoundName.Click);
        foreach (var mode in chooseMode)
        {
            mode.selectedModeUI.SetActive(false);

            if (id == mode.modeId)
            {
                _chosenMode = mode;
                _chosenMode.selectedModeUI.SetActive(true);
                
                if (_chosenMode.modeName == ModeName.Career)
                {
                    careerNextBtn.gameObject.SetActive(true);
                    freeNextBtn.gameObject.SetActive(false);
                }
                else
                {
                    careerNextBtn.gameObject.SetActive(false);
                    freeNextBtn.gameObject.SetActive(_chosenMode.isUnlocked);
                }
            }
            else
            {
                mode.selectedModeUI.SetActive(false);
            }
        }
    }
}

[System.Serializable]
public struct Modes
{
    public int modeId;
    public bool isUnlocked;
    public GameObject selectedModeUI, lockedUI;
    public ModeName modeName;
    public Button modeBtn;
}

public enum ModeName
{
    Career, Free
}
