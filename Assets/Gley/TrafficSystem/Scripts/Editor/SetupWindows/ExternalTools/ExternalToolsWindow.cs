using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;
namespace Gley.TrafficSystem.Editor
{
    internal class ExternalToolsWindow : SetupWindowBase
    {
        protected override void TopPart()
        {
            base.TopPart();
            EditorGUILayout.Space();
            if (GUILayout.Button("Easy Roads"))
            {
                _window.SetActiveWindow(typeof(EasyRoadsSetup), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Cidy 2"))
            {
                _window.SetActiveWindow(typeof(CidySetup), true);
            }
            EditorGUILayout.Space();
        }
    }
}
