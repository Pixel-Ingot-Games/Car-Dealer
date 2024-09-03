using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class WaypointSetupWindow : SetupWindowBase
    {
        protected override void ScrollPart(float width, float height)
        {
            EditorGUILayout.LabelField("Select action:");
            EditorGUILayout.Space();

            if (GUILayout.Button("Show All Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowAllWaypoints), true);     
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Disconnected Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowDisconnectedWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Vehicle Edited Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowVehicleTypeEditedWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Speed Edited Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowSpeedEditedWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Priority Edited Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowPriorityEditedWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Give Way Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowGiveWayWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Complex Give Way Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowComplexGiveWayWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Zipper Give Way Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowZipperGiveWayWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Event Waypoints"))
            {
                _window.SetActiveWindow(typeof(ShowEventWaypoints), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Show Vehicle Path Problems"))
            {
                _window.SetActiveWindow(typeof(ShowVehiclePathProblems), true);
            }
            EditorGUILayout.Space();

            base.ScrollPart(width, height);
        }
    }
}
