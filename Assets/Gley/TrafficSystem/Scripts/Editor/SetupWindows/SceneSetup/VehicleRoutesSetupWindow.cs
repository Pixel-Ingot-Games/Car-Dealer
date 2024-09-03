using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class VehicleRoutesSetupWindow : TrafficSetupWindow
    {
        private readonly float _scrollAdjustment = 104;

        private TrafficWaypointEditorData _trafficWaypointData;
        private TrafficWaypointDrawer _waypointDrawer;
        private int _nrOfVehicles;



        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _trafficWaypointData = new TrafficWaypointEditorData();
            _waypointDrawer = new TrafficWaypointDrawer(_trafficWaypointData);
            _waypointDrawer.onWaypointClicked += WaypointClicked;
            _nrOfVehicles = System.Enum.GetValues(typeof(VehicleTypes)).Length;
            if (_editorSave.AgentRoutes.RoutesColor.Count < _nrOfVehicles)
            {
                for (int i = _editorSave.AgentRoutes.RoutesColor.Count; i < _nrOfVehicles; i++)
                {
                    _editorSave.AgentRoutes.RoutesColor.Add(Color.white);
                    _editorSave.AgentRoutes.Active.Add(true);
                }
            }
            
            return this;
        }


        internal override void DrawInScene()
        {
            for (int i = 0; i < _nrOfVehicles; i++)
            {
                if (_editorSave.AgentRoutes.Active[i])
                {
                    _waypointDrawer.ShowWaypointsWithVehicle(i, _editorSave.AgentRoutes.RoutesColor[i]);
                }
            }

            base.DrawInScene();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            EditorGUILayout.LabelField("Vehicle Routes: ");
            for (int i = 0; i < _nrOfVehicles; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(((VehicleTypes)i).ToString(), GUILayout.MaxWidth(150));
                _editorSave.AgentRoutes.RoutesColor[i] = EditorGUILayout.ColorField(_editorSave.AgentRoutes.RoutesColor[i]);
                Color oldColor = GUI.backgroundColor;
                if (_editorSave.AgentRoutes.Active[i])
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("View", GUILayout.MaxWidth(BUTTON_DIMENSION)))
                {
                    _editorSave.AgentRoutes.Active[i] = !_editorSave.AgentRoutes.Active[i];
                    SceneView.RepaintAll();
                }
                GUI.backgroundColor = oldColor;
                EditorGUILayout.EndHorizontal();
            }

            base.ScrollPart(width, height);
            EditorGUILayout.EndScrollView();
        }


        private void WaypointClicked(WaypointSettingsBase clickedWaypoint, bool leftClick)
        {
            _window.SetActiveWindow(typeof(EditWaypointWindow), true);
        }


        internal override void DestroyWindow()
        {
            if (_waypointDrawer != null)
            {
                _waypointDrawer.onWaypointClicked -= WaypointClicked;
                _waypointDrawer.OnDestroy();
            }
            base.DestroyWindow();
        }
    }
}