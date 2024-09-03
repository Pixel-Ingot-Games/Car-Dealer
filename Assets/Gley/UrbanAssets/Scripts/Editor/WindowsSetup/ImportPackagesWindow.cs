using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class ImportPackagesWindow : SetupWindowBase
    {
        private string _message;

        protected override void TopPart()
        {
            EditorGUILayout.LabelField("Required Packages:");
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Burst");
            if(GUILayout.Button("Install"))
            {
                Gley.Common.ImportRequiredPackages.ImportPackage("com.unity.burst",UpdateMethod);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Install All"))
            {
                Gley.Common.ImportRequiredPackages.ImportPackage("com.unity.burst", UpdateMethod);
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(_message);

            base.TopPart();
        }


        private void UpdateMethod(string message)
        {
            this._message = message;
            if (message != "InProgress")
            {
                Debug.Log(message);
            }
        }
    }
}
