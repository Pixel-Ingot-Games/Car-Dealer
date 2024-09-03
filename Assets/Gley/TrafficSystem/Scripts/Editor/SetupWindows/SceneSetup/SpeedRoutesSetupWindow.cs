using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class SpeedRoutesSetupWindow : TrafficSetupWindow
    {
        private readonly float _scrollAdjustment = 104;

        private List<int> _speeds;
        private TrafficWaypointEditorData _trafficWaypointData;
        private TrafficWaypointDrawer _waypointDrawer;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _trafficWaypointData = new TrafficWaypointEditorData();
            _waypointDrawer = new TrafficWaypointDrawer(_trafficWaypointData);
            _speeds = GetDifferentSpeeds(_trafficWaypointData.GetAllWaypoints());
            if (_editorSave.speedRoutes.RoutesColor.Count < _speeds.Count)
            {
                int nrOfColors = _speeds.Count - _editorSave.speedRoutes.RoutesColor.Count;
                for (int i = 0; i < nrOfColors; i++)
                {
                    _editorSave.speedRoutes.RoutesColor.Add(Color.white);
                    _editorSave.speedRoutes.Active.Add(true);
                }
            }

            _waypointDrawer.onWaypointClicked += WaypointClicked;
            return this;
        }


        internal override void DrawInScene()
        {
            for (int i = 0; i < _speeds.Count; i++)
            {
                if (_editorSave.speedRoutes.Active[i])
                {
                    _waypointDrawer.ShowWaypointsWithSpeed(_speeds[i], _editorSave.speedRoutes.RoutesColor[i]);
                }
            }

            base.DrawInScene();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            EditorGUILayout.LabelField("SpeedRoutes: ");
            for (int i = 0; i < _speeds.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_speeds[i].ToString(), GUILayout.MaxWidth(50));
                _editorSave.speedRoutes.RoutesColor[i] = EditorGUILayout.ColorField(_editorSave.speedRoutes.RoutesColor[i]);
                Color oldColor = GUI.backgroundColor;
                if (_editorSave.speedRoutes.Active[i])
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("View"))
                {
                    _editorSave.speedRoutes.Active[i] = !_editorSave.speedRoutes.Active[i];
                    SceneView.RepaintAll();
                }

                GUI.backgroundColor = oldColor;
                EditorGUILayout.EndHorizontal();
            }

            base.ScrollPart(width, height);
            EditorGUILayout.EndScrollView();
        }


        private List<int> GetDifferentSpeeds(WaypointSettings[] allWaypoints)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < allWaypoints.Length; i++)
            {
                if (!result.Contains(allWaypoints[i].maxSpeed))
                {
                    result.Add(allWaypoints[i].maxSpeed);
                }
            }
            return result;
        }


        private void WaypointClicked(WaypointSettings clickedWaypoint, bool leftClick)
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
