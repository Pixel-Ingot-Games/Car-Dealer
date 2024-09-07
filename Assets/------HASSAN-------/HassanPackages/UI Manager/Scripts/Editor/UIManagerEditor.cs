using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    private bool showPanels = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIManager uiManager = (UIManager)target;

        showPanels = EditorGUILayout.Foldout(showPanels, "Panels");
        if (showPanels)
        {
            if (uiManager.panels == null || uiManager.panels.Count == 0)
            {
                EditorGUILayout.HelpBox("No panels added", MessageType.Info);
            }
            else
            {
                foreach (UIPanel panel in uiManager.panels)
                {
                    if (panel == null)
                    {
                        EditorGUILayout.HelpBox("A panel in the list is null", MessageType.Warning);
                        continue; // Skip this iteration if the panel is null
                    }

                    if (GUILayout.Button($"Show {panel.GetType().Name}"))
                    {
                        ShowPanelExplicitly(uiManager, panel);
                    }
                }

                if (GUILayout.Button("Hide Current Panel"))
                {
                    uiManager.HideCurrentPanel();
                }    
                if (GUILayout.Button("Hide Current Overlay Panel"))
                {
                    uiManager.HideCurrentOverlayPanel();
                }
            }
        }
    }

    private void ShowPanelExplicitly(UIManager uiManager, UIPanel panel)
    {
        var panelType = panel.GetType();
        var showPanelMethod = typeof(UIManager).GetMethod("ShowPanel", new Type[0]);
        if (showPanelMethod != null)
        {
            var genericMethod = showPanelMethod.MakeGenericMethod(panelType);
            genericMethod.Invoke(uiManager, null);
        }
        else
        {
            Debug.LogWarning("ShowPanel method not found in UIManager.");
        }
    }
}
