using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal abstract class ShowWaypointsTrafficBase : TrafficSetupWindow
    {
        

        protected WaypointSettings[] _waypointsOfInterest;
        protected TrafficWaypointEditorData _trafficWaypointData;
        protected TrafficWaypointDrawer _trafficWaypointDrawer;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _trafficWaypointData = new TrafficWaypointEditorData();
            _trafficWaypointDrawer = new TrafficWaypointDrawer(_trafficWaypointData);
            _trafficWaypointDrawer.onWaypointClicked += WaypointClicked;
            base.Initialize(windowProperties, window);
            return this;
        }


        protected override void TopPart()
        {
            base.TopPart();
            EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginHorizontal();
            _editorSave.ShowConnections = EditorGUILayout.Toggle("Show Connections", _editorSave.ShowConnections, GUILayout.Width(TOGGLE_WIDTH));
            _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField(_editorSave.EditorColors.WaypointColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _editorSave.showOtherLanes = EditorGUILayout.Toggle("Show Lane Change", _editorSave.showOtherLanes, GUILayout.Width(TOGGLE_WIDTH));
            _editorSave.EditorColors.LaneChangeColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LaneChangeColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _editorSave.showSpeed = EditorGUILayout.Toggle("Show Speed", _editorSave.showSpeed, GUILayout.Width(TOGGLE_WIDTH));
            _editorSave.EditorColors.SpeedColor = EditorGUILayout.ColorField(_editorSave.EditorColors.SpeedColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _editorSave.ShowVehicles = EditorGUILayout.Toggle("Show Cars", _editorSave.ShowVehicles, GUILayout.Width(TOGGLE_WIDTH));
            _editorSave.EditorColors.AgentColor = EditorGUILayout.ColorField(_editorSave.EditorColors.AgentColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _editorSave.ShowPriority = EditorGUILayout.Toggle("Show Waypoint Priority", _editorSave.ShowPriority, GUILayout.Width(TOGGLE_WIDTH));
            _editorSave.EditorColors.PriorityColor = EditorGUILayout.ColorField(_editorSave.EditorColors.PriorityColor);
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
        }


        protected override void ScrollPart(float width, float height)
        {
            if (_waypointsOfInterest != null)
            {
                if (_waypointsOfInterest.Length == 0)
                {
                    EditorGUILayout.LabelField("No " + GetWindowTitle());
                }
                for (int i = 0; i < _waypointsOfInterest.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(_waypointsOfInterest[i].name);
                    if (GUILayout.Button("View", GUILayout.Width(BUTTON_DIMENSION)))
                    {
                        GleyUtilities.TeleportSceneCamera(_waypointsOfInterest[i].transform.position);
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Edit", GUILayout.Width(BUTTON_DIMENSION)))
                    {
                        OpenEditWindow(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No " + GetWindowTitle());
            }
            base.ScrollPart(width, height);
        }


        protected void OpenEditWindow(int index)
        {
            SettingsWindow.SetSelectedWaypoint((WaypointSettings)_waypointsOfInterest[index]);
            GleyUtilities.TeleportSceneCamera(_waypointsOfInterest[index].transform.position);
            _window.SetActiveWindow(typeof(EditWaypointWindow), true);
        }


        protected virtual void WaypointClicked(WaypointSettingsBase clickedWaypoint, bool leftClick)
        {
            _window.SetActiveWindow(typeof(EditWaypointWindow), true);
        }


        internal override void DestroyWindow()
        {
            if (_trafficWaypointDrawer != null)
            {
                _trafficWaypointDrawer.onWaypointClicked -= WaypointClicked;
                _trafficWaypointDrawer.OnDestroy();
            }

            base.DestroyWindow();
        }
    }
}