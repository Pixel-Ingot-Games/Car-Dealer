using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] public List<UIPanel> panels;

    private UIPanel currentPanel, currentOverlayPanel;

    #region Show Panel
    // Show panel by type
    public void ShowPanel<T>() where T : UIPanel
    {
        UIPanel panelToShow = panels.Find(panel => panel is T);

        if (panelToShow == null)
        {
            Debug.LogWarning($"Panel of type {typeof(T)} not found!");
            return;
        }

        if (currentPanel != null && currentPanel != panelToShow)
        {
            currentPanel.Hide();
        }

        panelToShow.Show();
        currentPanel = panelToShow;
    }


    public void ShowPanel<T>(Action onShowAction) where T : UIPanel
    {
        ShowPanel<T>();
        onShowAction?.Invoke();
    }
    // Show panel by name
    public void ShowPanel(UIPanel panelName)
    {
        //UIPanel panelToShow = panels.Find(panel => panel.GetType().Name == panelName);

        if (panelName == null)
        {
            Debug.LogWarning($"Panel with name {panelName} not found!");
            return;
        }

        SwitchPanel(panelName);
    }


    public void ShowPanelOverlay<T>() where T : UIPanel
    {
        UIPanel panelToShow = panels.Find(panel => panel is T);

        if (panelToShow == null)
        {
            Debug.LogWarning($"Panel of type {typeof(T)} not found!");
            return;
        }

        if (currentOverlayPanel != null && currentOverlayPanel != panelToShow)
        {
            currentOverlayPanel.Hide();
        }

        panelToShow.Show();
        currentOverlayPanel = panelToShow;
    }
    public void ShowPanelOverlay<T>(Action onShowAction) where T : UIPanel
    {
        ShowPanelOverlay<T>();
        onShowAction?.Invoke();
    }

    public void ShowPanelOverlay(UIPanel panelToShow)
    {

        if (currentOverlayPanel != null && currentOverlayPanel != panelToShow)
        {
            currentOverlayPanel.Hide();
        }
        panelToShow.Show();
        currentOverlayPanel = panelToShow;
    }
    private void SwitchPanel(UIPanel panelToShow)
    {
        if (currentPanel != null && currentPanel != panelToShow)
        {
            currentPanel.Hide();
        }

        panelToShow.Show();
        currentPanel = panelToShow;
    }
    #endregion
    #region Hide Panels
    public void HideCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.Hide();
            currentPanel = null;
        }
    }
    public void HideCurrentOverlayPanel()
    {
        if (currentOverlayPanel != null)
        {
            currentOverlayPanel.Hide();
            currentOverlayPanel = null;
        }
    }
    public void HidePanel(UIPanel panelName)
    {
        if (panelName != null)
        {
            panelName.Hide();
            if (panelName == currentPanel)
            {
                currentPanel = null;
            }
        }
    }
    public void HideOverlayPanel(UIPanel panelName)
    {
        if (panelName != null)
        {
            panelName.Hide();
            if (currentOverlayPanel == panelName)
            {
                currentOverlayPanel = null;
            }

        }
    }
    #endregion
}
