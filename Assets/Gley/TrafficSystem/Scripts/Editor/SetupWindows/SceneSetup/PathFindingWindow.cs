using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class PathFindingWindow : TrafficSetupWindow
    {
        private readonly float _scrollAdjustment = 171;

        private List<int> _penalties;
        private WaypointSettings[] _waypointsOfInterest;
        private TrafficWaypointEditorData _trafficWaypointData;
        private TrafficWaypointDrawer _waypointDrawer;
        private bool _showPenaltyEditedWaypoints;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _trafficWaypointData = new TrafficWaypointEditorData();
            _waypointDrawer = new TrafficWaypointDrawer(_trafficWaypointData);
            _waypointsOfInterest = _trafficWaypointData.GetPenlatyEditedWaypoints();
            _penalties = GetDifferentPenalties(_trafficWaypointData.GetAllWaypoints());
            if (_editorSave.PathFindingRoutes.RoutesColor.Count < _penalties.Count)
            {
                int nrOfColors = _penalties.Count - _editorSave.PathFindingRoutes.RoutesColor.Count;
                for (int i = 0; i < nrOfColors; i++)
                {
                    _editorSave.PathFindingRoutes.RoutesColor.Add(Color.white);
                    _editorSave.PathFindingRoutes.Active.Add(true);
                }
            }
            _waypointDrawer.onWaypointClicked += WaypointClicked;
            return this;
        }


        internal override void DrawInScene()
        {
            if (_showPenaltyEditedWaypoints)
            {
                _waypointDrawer.ShowPenaltyEditedWaypoints(_editorSave.EditorColors.WaypointColor);
            }
            else
            {
                for (int i = 0; i < _penalties.Count; i++)
                {
                    if (_editorSave.PathFindingRoutes.Active[i])
                    {
                        _waypointDrawer.ShowWaypointsWithPenalty(_penalties[i], _editorSave.PathFindingRoutes.RoutesColor[i]);
                    }
                }
            }
            base.DrawInScene();
        }


        protected override void TopPart()
        {
            base.TopPart();
            _editorSave.PathFindingEnabled = EditorGUILayout.Toggle("Enable Path Finding", _editorSave.PathFindingEnabled);

            EditorGUI.BeginChangeCheck();
            _showPenaltyEditedWaypoints = EditorGUILayout.Toggle("Show Edited Waypoints", _showPenaltyEditedWaypoints);

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            if (_showPenaltyEditedWaypoints)
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
                    EditorGUILayout.LabelField("No priority edited waypoints");
                }
            }
            else
            {
                EditorGUILayout.LabelField("Waypoint Penalties: ");
                for (int i = 0; i < _penalties.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(_penalties[i].ToString(), GUILayout.MaxWidth(50));
                    _editorSave.PathFindingRoutes.RoutesColor[i] = EditorGUILayout.ColorField(_editorSave.PathFindingRoutes.RoutesColor[i]);
                    Color oldColor = GUI.backgroundColor;
                    if (_editorSave.PathFindingRoutes.Active[i])
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    if (GUILayout.Button("View"))
                    {
                        _editorSave.PathFindingRoutes.Active[i] = !_editorSave.PathFindingRoutes.Active[i];
                        SceneView.RepaintAll();
                    }

                    GUI.backgroundColor = oldColor;
                    EditorGUILayout.EndHorizontal();
                }
            }
            base.ScrollPart(width, height);
            EditorGUILayout.EndScrollView();
        }


        protected override void BottomPart()
        {
            if (GUILayout.Button("Save"))
            {
                Save();
            }
            base.BottomPart();
        }


        protected void OpenEditWindow(int index)
        {
            SettingsWindow.SetSelectedWaypoint(_waypointsOfInterest[index]);
            GleyUtilities.TeleportSceneCamera(_waypointsOfInterest[index].transform.position);
            _window.SetActiveWindow(typeof(EditWaypointWindow), true);
        }


        private List<int> GetDifferentPenalties(WaypointSettings[] allWaypoints)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < allWaypoints.Length; i++)
            {
                if (!result.Contains(allWaypoints[i].penalty))
                {
                    result.Add(allWaypoints[i].penalty);
                }
            }
            return result;
        }


        private void WaypointClicked(WaypointSettings clickedWaypoint, bool leftClick)
        {
            _window.SetActiveWindow(typeof(EditWaypointWindow), true);
        }


        private void Save()
        {
            Debug.Log("Save");
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
