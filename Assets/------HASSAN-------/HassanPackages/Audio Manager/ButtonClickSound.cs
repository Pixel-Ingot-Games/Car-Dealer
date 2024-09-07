using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ButtonClickSound : ButtonClicks
{
    public SoundName soundName;

    protected override void OnClick()
    {
        AudioManager.Instance.Play(soundName);
    }
}
