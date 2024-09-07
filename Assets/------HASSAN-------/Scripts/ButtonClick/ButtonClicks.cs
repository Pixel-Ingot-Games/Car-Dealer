using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class ButtonClicks : MonoBehaviour
{
    protected Button button;
    protected virtual void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
    protected abstract void OnClick();
}
