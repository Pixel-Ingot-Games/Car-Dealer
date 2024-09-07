
using UnityEngine;

public class GameManagers : MonoBehaviour
{
    public void Awake()
    {
        CheckSoundState(PlayerPrefs.GetInt("Sound") == 1);
        CheckMusicState(PlayerPrefs.GetInt("Music") == 1);
    }

    private void CheckSoundState(bool isOn)
    {
        AudioManager.Instance.SetMasterVolume(isOn ? 1 : 0);
    }

    private void CheckMusicState(bool isOn)
    {
        AudioManager.Instance.SetMusicVolume(isOn ? 1 : 0);
    }
}
