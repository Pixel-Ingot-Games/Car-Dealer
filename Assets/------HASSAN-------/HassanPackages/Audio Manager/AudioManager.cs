using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Audio;

public enum SoundName
{
    None,MainMenu,Gameplay,Click,Typing,Herb,HorseNeigh,RaceTime,EnterWater,Confetti,
    ClockTick,Fire,LevelComplete,LevelFail,WoodCrush,Checkpoint,Coin,Brush,Brush2
}

[Serializable]
public class Sound
{
    [HideInInspector]
    public AudioSource source;
    //--------------------------
    public SoundName name;
    public AudioClip clip;
    public AudioMixerGroup mixer;
    [Range(0, 1)]
    public float volume = 1;
    [Range(0.1f, 3)]
    public float pitch = 1;

    [Space(5)]
    public bool playOnAwake;
    public bool loop;

    //--------------------------
    private Sound()
    {
        volume = 1;
        pitch = 1;
    }
    //--------------------------
    public bool CompareName(SoundName name)
    {
        return name == this.name;
    }
    public void Play()
    {
        if (source != null)
        {
            source.Play();
        }
        else
        {
            Debug.LogError(name + ": Audio Source has not assigned");
        }

    }
    public void PlayOneShot()
    {
        source.PlayOneShot(source.clip);
    }
    public void Pause()
    {
        if (source != null)
        {
            source.Pause();
        }
        else
        {
            Debug.LogError(name + ": Audio Source has not assigned");
        }
    }
    public void Stop()
    {
        if (source != null)
        {
            source.Stop();
        }
        else
        {
            Debug.LogError(name + ": Audio Source has not assigned");
        }
    }
    public void UnPause()
    {
        if (source != null)
        {
            source.UnPause();
        }
        else
        {
            Debug.LogError(name + ": Audio Source has not assigned");
        }
    }

}

public class AudioManager : Singleton<AudioManager> 
{

    //---------------------------------

    [SerializeField] AudioMixer AudioMixer;

    [Space]
    [SerializeField] string masterGroupVolumeName;
    [SerializeField] string musicGroupVolumeName;
    [SerializeField] string sfxGroupVolumeName;

    [Space]
    public Sound[] sounds;


    #region Unity Methods
    void Awake()
    {

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.pitch = s.pitch;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixer;
        }
    }
    private void Start()
    {
        foreach (Sound s in sounds)
        {
            if (s.playOnAwake)
            {
                Play(s.name);
            }
        }
    }
    #endregion

    #region Player Methods
    //Play sound from start or restarting already playing one(else one shot)
    public void Play(SoundName name)
    {
        if (name != SoundName.None)
        {
            if (GetSound(name) == null)
            {
                Debug.Log($"{name} is not in your Sound List");
            }
            else
            {
                GetSound(name).Play();
            }
        }

    }
    public void Play(string name)
    {
        if (name == "None")
        {
            return;
        }
        Enum.TryParse(name, out SoundName soundName);
        Play(soundName);
    }

    //Play multiple instance of a sound
    public void PlayOneShot(string name)
    {
        if (name == "None")
        {
            return;
        }
        SoundName soundName;
        Enum.TryParse(name, out soundName);
        PlayOneShot(soundName);
    }
    public void PlayOneShot(SoundName name)
    {
        if (name == SoundName.None)
        {
            return;
        }
        GetSound(name).PlayOneShot();
    }

    //Pause all insatnces of sound
    public void Pause(SoundName name)
    {
        if (name == SoundName.None)
        {
            return;
        }
        GetSound(name).Pause();
    }
    public void Pause(string name)
    {
        if (name == "None")
        {
            return;
        }
        SoundName soundName;
        Enum.TryParse(name, out soundName);
        Pause(soundName);
    }

    //Unpause all insatnces of sound
    public void UnPause(SoundName name)
    {
        if (name == SoundName.None)
        {
            return;
        }
        GetSound(name).UnPause();
    }
    public void UnPause(string name)
    {
        if (name == "None")
        {
            return;
        }
        SoundName soundName;
        Enum.TryParse(name, out soundName);
        UnPause(soundName);
    }

    //Stop all insatnces of sound
    public void Stop(SoundName name)
    {
        if (name == SoundName.None)
        {
            return;
        }
        GetSound(name).Stop();
    }
    public void Stop(string name)
    {
        if (name == "None")
        {
            return;
        }
        SoundName soundName;
        Enum.TryParse(name, out soundName);
        Stop(soundName);
    }
    #endregion

    #region Mixer Managers
    //Set Volume Of Whole Game Sound
    public void SetMasterVolume(float value)
    {
        SetMixerValue(masterGroupVolumeName, value);
    }

    //Set Volume Of Music Group
    public void SetMusicVolume(float value)
    {
        SetMixerValue(musicGroupVolumeName, value);
    }

    //Set Volume Of SFX Group
    public void SetSFXVolume(float value)
    {
        SetMixerValue(sfxGroupVolumeName, value);
    }


    void SetMixerValue(string floatName, float value)
    {
        if (AudioMixer != null)
        {
            if (value <= 0)
            {
                AudioMixer.SetFloat(floatName, GetMixerValue(0));
            }
            else if (value >= 1)
            {
                AudioMixer.SetFloat(floatName, GetMixerValue(1));
            }
            else
            {
                AudioMixer.SetFloat(floatName, GetMixerValue(value));
            }
        }
        else
        {
            Debug.LogError("Audio Mixer is null");
        }
    }
    float GetMixerValue(float value)
    {
        float x = -80 + (80 * value);
        return x;
    }
    #endregion

    public Sound GetSound(SoundName name)
    {
        foreach (Sound s in sounds)
        {
            if (s.CompareName(name))
            {
                if(s.clip!=null)
                {
                    return s;
                }
                return null;
            }
        }
        return null;
    }
}
