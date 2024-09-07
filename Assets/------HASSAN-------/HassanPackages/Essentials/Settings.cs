using UnityEngine;

    public class Settings : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject soundOn;
        [SerializeField] private GameObject soundOff;
        [SerializeField] private GameObject musicOn;
        [SerializeField] private GameObject musicOff;
        [SerializeField] private GameObject hapticsOn;
        [SerializeField] private GameObject hapticsOff;

        private const string SoundKey = "Sound";
        private const string MusicKey = "Music";
        private const string HapticsKey = "Haptics";
        
        private void Start()
        {
            InitializePlayerPrefs();
            UpdateUI();
        }

        private static void InitializePlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(SoundKey)) PlayerPrefs.SetInt(SoundKey, 1);
            if (!PlayerPrefs.HasKey(MusicKey)) PlayerPrefs.SetInt(MusicKey, 1);
            if (!PlayerPrefs.HasKey(HapticsKey)) PlayerPrefs.SetInt(HapticsKey, 1);
        }

        private void UpdateUI()
        {
            ToggleSound(PlayerPrefs.GetInt(SoundKey) == 1);
            ToggleMusic(PlayerPrefs.GetInt(MusicKey) == 1);
            ToggleHaptics(PlayerPrefs.GetInt(HapticsKey) == 1);
        }

        public void ToggleSound(bool isOn)
        {
            SetUIState(isOn, soundOn, soundOff);
            PlayerPrefs.SetInt(SoundKey, isOn ? 1 : 0);
            AudioManager.Instance.SetMasterVolume(isOn ? 1 : 0);
        }

        public void ToggleMusic(bool isOn)
        {
            SetUIState(isOn, musicOn, musicOff);
            PlayerPrefs.SetInt(MusicKey, isOn ? 1 : 0);
            AudioManager.Instance.SetMusicVolume(isOn ? 1 : 0);
        }

        public void ToggleHaptics(bool isOn)
        {
            SetUIState(isOn, hapticsOn, hapticsOff);
            PlayerPrefs.SetInt(HapticsKey, isOn ? 1 : 0);
            GameConstant.Haptics = isOn;
        }

        private static void SetUIState(bool isOn, GameObject onObject, GameObject offObject)
        {
            onObject.SetActive(isOn);
            offObject.SetActive(!isOn);
        }
    }

