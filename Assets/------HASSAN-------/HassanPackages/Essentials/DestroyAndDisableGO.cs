
using UnityEngine;
using UnityEngine.Events;

public class DestroyAndDisableGO : MonoBehaviour
{
    [Header("Settings")]
    public bool isThisGO;
    public float duration;
    public UnityEvent onTimeFinish;

    [Header("Specify GameObject if not using this GameObject")]
    public GameObject targetGO;

    private void OnEnable()
    {
        FunctionTimer.CreateCountdownWithSlider(duration).OnComplete(() =>
        {
            onTimeFinish?.Invoke();
        });
    }

    public void ExecuteDestroy()
    {
        if (isThisGO)
        {
            Destroy(gameObject);
        }
        else
        {
            if (targetGO != null)
            {
                Destroy(targetGO);
            }
            else
            {
                Debug.LogWarning("targetGO is not assigned.");
            }
        }
    }

    public void ExecuteDisable()
    {
        if (isThisGO)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (targetGO != null)
            {
                targetGO.SetActive(false);
            }
            else
            {
                Debug.LogWarning("targetGO is not assigned.");
            }
        }
    }
}
