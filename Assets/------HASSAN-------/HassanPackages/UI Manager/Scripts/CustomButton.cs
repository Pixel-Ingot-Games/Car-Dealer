using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image), typeof(Button))]
public class CustomButton : MonoBehaviour
{
    private Button buttonComponent;
    [SerializeField] private OverlayType panelType;
    [SerializeField] private UIPanel panelToShow;

    private void Awake()
    {
        if (!TryGetComponent<Button>(out buttonComponent))
        {
            buttonComponent = gameObject.AddComponent<Button>();
        }

        buttonComponent.onClick.AddListener(HandleButtonClick);
    }

    private void HandleButtonClick()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager instance is not available.");
            return;
        }

        switch (panelType)
        {
            case OverlayType.DefaultPanel:
                UIManager.Instance.ShowPanel(panelToShow);

                break;
            case OverlayType.OverlayPanel:
                UIManager.Instance.ShowPanelOverlay(panelToShow);
                break;
            default:
                Debug.LogWarning("Unhandled OverlayType: " + panelType);
                break;
        }
    }
}

public enum OverlayType 
{
    DefaultPanel, 
    OverlayPanel
}
