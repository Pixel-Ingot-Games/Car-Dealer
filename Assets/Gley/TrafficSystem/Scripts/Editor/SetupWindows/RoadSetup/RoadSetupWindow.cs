using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class RoadSetupWindow : SetupWindowBase
    {
        private string _createRoad;
        private string _connectRoads;
        private string _viewRoads;

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _createRoad = "Create Road";
            _connectRoads = "Connect Roads";
            _viewRoads = "View Roads";
            return this;
        }


        protected override void TopPart()
        {
            base.TopPart();
            EditorGUILayout.LabelField("Select action:");
            EditorGUILayout.Space();

            if (GUILayout.Button(_createRoad))
            {
                _window.SetActiveWindow(typeof(CreateRoadWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button(_connectRoads))
            {
                _window.SetActiveWindow(typeof(ConnectRoadsWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button(_viewRoads))
            {
                _window.SetActiveWindow(typeof(ViewRoadsWindow), true);
            }
        }
    }
}