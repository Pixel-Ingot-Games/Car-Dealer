using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameObject coinAttractor;
    public RectTransform canvasTransform;

    public static Action onCoinCollection=null;
    public void OnCollection()
    {
        AudioManager.Instance.Play(SoundName.Coin);
        if(GameConstant.Haptics)
        {
            Haptics.Instance.PlaySimple();
        }
        onCoinCollection?.Invoke();
        Instantiate(coinAttractor,canvasTransform);
        Destroy(gameObject);
    }
}
