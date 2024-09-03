using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class SceneSetupWindow : SetupWindowBase
    {
        protected override void ScrollPart(float width, float height)
        {
            EditorGUILayout.LabelField("Select action:");
            EditorGUILayout.Space();

            if (GUILayout.Button("Layer Setup"))
            {
                _window.SetActiveWindow(typeof(LayerSetupWindow), true);
            }
            EditorGUILayout.Space();


            if (GUILayout.Button("Grid Setup"))
            {
                _window.SetActiveWindow(typeof(GridSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Vehicle Type Setup"))
            {
                _window.SetActiveWindow(typeof(VehicleTypesWindow), true);
            }
            EditorGUILayout.Space();

            base.ScrollPart(width, height);
        }
    }
}
