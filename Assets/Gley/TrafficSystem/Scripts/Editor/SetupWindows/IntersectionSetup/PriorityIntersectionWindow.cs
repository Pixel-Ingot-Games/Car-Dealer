#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Internal;
#endif
using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class PriorityIntersectionWindow : IntersectionWindow
    {
        private readonly float _scrollAdjustment = 182;

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
#if GLEY_PEDESTRIAN_SYSTEM
            _pedestrianWaypointDrawer.OnWaypointClicked += PedestrianWaypointClicked;
#endif
            return this;
        }


        protected override void TopPart()
        {
            name = EditorGUILayout.TextField("Intersection Name", name);
            if (GUILayout.Button("Save"))
            {
                SaveSettings();
            }
            EditorGUI.BeginChangeCheck();
            _hideWaypoints = EditorGUILayout.Toggle("Hide Waypoints ", _hideWaypoints);
            _hideConnections = EditorGUILayout.Toggle("Hide Connections ", _hideConnections);
            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                _window.BlockClicks(!_hideWaypoints);
            }
            base.TopPart();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));

            switch (_currentAction)
            {
                case WindowActions.None:
                    IntersectionOverview();
                    break;
                case WindowActions.AssignRoadWaypoints:
                    AddTrafficWaypoints();
                    break;
                case WindowActions.AddExitWaypoints:
                    AddExitWaypoints();
                    break;
#if GLEY_PEDESTRIAN_SYSTEM
                case WindowActions.AddDirectionWaypoints:
                    AddDirectionWaypoints();
                    break;
                case WindowActions.AssignPedestrianWaypoints:
                    AddPedestrianWaypoints();
                    break;
#endif
            }
            base.ScrollPart(width, height);
            GUILayout.EndScrollView();
        }


#if GLEY_PEDESTRIAN_SYSTEM
        private void PedestrianWaypointClicked(PedestrianWaypointSettings clickedWaypoint, bool leftClick)
        {
            switch (_currentAction)
            {
                case WindowActions.AddDirectionWaypoints:
                    int road = GetRoadFromWaypoint(clickedWaypoint);
                    AddWaypointToList(clickedWaypoint, _stopWaypoints[road].directionWaypoints);
                    _directionWaypoints = _selectedIntersection.GetDirectionWaypoints();
                    break;
                case WindowActions.AssignPedestrianWaypoints:
                    AddWaypointToList(clickedWaypoint, _stopWaypoints[_selectedRoad].pedestrianWaypoints);
                    _pedestrianWaypoints = _selectedIntersection.GetPedestrianWaypoints();
                    break;
            }
            SceneView.RepaintAll();
            SettingsWindowBase.TriggerRefreshWindowEvent();
        }
#endif


        private void IntersectionOverview()
        {
            ViewAssignedStopWaypoints();
            EditorGUILayout.Space();      
#if GLEY_PEDESTRIAN_SYSTEM
            ViewDirectionWaypoints();
            EditorGUILayout.Space();
#endif
            ViewExitWaypoints();
        }
        #region StopWaypoints


        private void ViewAssignedStopWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Color oldColor;
            for (int i = 0; i < _stopWaypoints.Count; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Road " + (i + 1));
                EditorGUILayout.LabelField(new GUIContent("Stop waypoints:", "The vehicle will stop at this point until the intersection allows it to continue. " +
                "\nEach road that enters in intersection should have its own set of stop waypoints"));

                DisplayList(_stopWaypoints[i].roadWaypoints, ref _stopWaypoints[i].draw);
#if GLEY_PEDESTRIAN_SYSTEM
                EditorGUILayout.LabelField("Pedestrian waypoints:");
                DisplayList(_stopWaypoints[i].pedestrianWaypoints, ref _stopWaypoints[i].draw);
#endif
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Assign Road Waypoints"))
                {
                    _selectedRoad = i;
                    _currentAction = WindowActions.AssignRoadWaypoints;
                }

                oldColor = GUI.backgroundColor;
                if (_stopWaypoints[i].draw == true)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("View Road Waypoints"))
                {
                    ViewRoadWaypoints(i);
                }
                GUI.backgroundColor = oldColor;

                if (GUILayout.Button("Delete Road"))
                {
                    _stopWaypoints.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
#if GLEY_PEDESTRIAN_SYSTEM
                if (GUILayout.Button("Assign Pedestrian Waypoints"))
                {
                    _selectedRoad = i;
                    _currentAction = WindowActions.AssignPedestrianWaypoints;
                }
#endif
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Road"))
            {
                _stopWaypoints.Add(new IntersectionStopWaypointsSettings());
            }
            EditorGUILayout.EndVertical();
        }



        private void AddTrafficWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Road " + (_selectedRoad + 1));
            EditorGUILayout.LabelField(new GUIContent("Stop waypoints:", "The vehicle will stop at this point until the intersection allows it to continue. " +
            "\nEach road that enters in intersection should have its own set of stop waypoints"));

            DisplayList(_stopWaypoints[_selectedRoad].roadWaypoints, ref _stopWaypoints[_selectedRoad].draw);

            EditorGUILayout.Space();
            Color oldColor = GUI.backgroundColor;
            if (_stopWaypoints[_selectedRoad].draw == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Road Waypoints"))
            {
                ViewRoadWaypoints(_selectedRoad);
            }
            GUI.backgroundColor = oldColor;

            if (GUILayout.Button("Done"))
            {
                Cancel();
            }
            EditorGUILayout.EndVertical();
        }


#if GLEY_PEDESTRIAN_SYSTEM
        private void AddPedestrianWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Road " + (_selectedRoad + 1));
            EditorGUILayout.LabelField(new GUIContent("Pedestrian stop waypoints:", "pedestrians will stop at this point until the intersection allows them to continue. " +
            "\nEach crossing in intersection should have its own set of stop waypoints corresponding to its road"));

            DisplayList(_stopWaypoints[_selectedRoad].pedestrianWaypoints, ref _stopWaypoints[_selectedRoad].draw);

            EditorGUILayout.Space();
            Color oldColor = GUI.backgroundColor;
            if (_stopWaypoints[_selectedRoad].draw == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Road Waypoints"))
            {
                ViewRoadWaypoints(_selectedRoad);
            }
            GUI.backgroundColor = oldColor;

            if (GUILayout.Button("Done"))
            {
                Cancel();
            }
            EditorGUILayout.EndVertical();
        }
#endif


        private void ViewRoadWaypoints(int i)
        {
            _stopWaypoints[i].draw = !_stopWaypoints[i].draw;
            for (int j = 0; j < _stopWaypoints[i].roadWaypoints.Count; j++)
            {
                _stopWaypoints[i].roadWaypoints[j].draw = _stopWaypoints[i].draw;
            }
#if GLEY_PEDESTRIAN_SYSTEM
            for (int j = 0; j < _stopWaypoints[i].pedestrianWaypoints.Count; j++)
            {
                _stopWaypoints[i].pedestrianWaypoints[j].draw = _stopWaypoints[i].draw;
            }
#endif
        }
        #endregion


        #region ExitWaypoints
        private void ViewExitWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Exit waypoints:", "When a vehicle touches an exit waypoint, it is no longer considered inside intersection.\n" +
                "For every lane that exits the intersection a single exit point should be marked"));
            EditorGUILayout.Space();
            DisplayList(_exitWaypoints, ref _editorSave.showExitWaypoints);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Exit Waypoints"))
            {
                _selectedRoad = -1;
                _currentAction = WindowActions.AddExitWaypoints;
            }
            Color oldColor = GUI.backgroundColor;
            if (_editorSave.showExitWaypoints == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Exit Waypoints"))
            {
                ViewAllExitWaypoints();
            }
            GUI.backgroundColor = oldColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }


        private void AddExitWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Exit waypoints:", "When a vehicle touches an exit waypoint, it is no longer considered inside intersection.\n" +
                "For every lane that exits the intersection a single exit point should be marked"));
            EditorGUILayout.Space();
            DisplayList(_exitWaypoints, ref _editorSave.showExitWaypoints);
            EditorGUILayout.Space();
            Color oldColor = GUI.backgroundColor;
            if (_editorSave.showExitWaypoints == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Exit Waypoints"))
            {
                ViewAllExitWaypoints();
            }
            GUI.backgroundColor = oldColor;

            if (GUILayout.Button("Done"))
            {
                Cancel();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

#if GLEY_PEDESTRIAN_SYSTEM
        #region DirectionWaypoints
        private void ViewDirectionWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Crossing direction waypoints:", "For each stop waypoint a direction needs to be specified\n" +
                "Only if a pedestrian goes to that direction it will stop, otherwise it will pass through stop waypoint"));
            EditorGUILayout.Space();

            for (int i = 0; i < _stopWaypoints.Count; i++)
            {
                DisplayList(_stopWaypoints[i].directionWaypoints, ref _editorSave.showDirectionWaypoints);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Direction Waypoints"))
            {
                _selectedRoad = -1;
                _currentAction = WindowActions.AddDirectionWaypoints;
            }
            Color oldColor = GUI.backgroundColor;
            if (_editorSave.showDirectionWaypoints == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Direction Waypoints"))
            {
                ViewAllDirectionWaypoints();
            }
            GUI.backgroundColor = oldColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void AddDirectionWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Crossing direction waypoints:", "For each stop waypoint a direction needs to be specified\n" +
                "Only if a pedestrian goes to that direction it will stop, otherwise it will pass through stop waypoint"));
            EditorGUILayout.Space();

            for (int i = 0; i < _stopWaypoints.Count; i++)
            {
                DisplayList(_stopWaypoints[i].directionWaypoints, ref _editorSave.showDirectionWaypoints);
            }
            EditorGUILayout.Space();

            Color oldColor = GUI.backgroundColor;
            if (_editorSave.showDirectionWaypoints == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Direction Waypoints"))
            {
                ViewAllDirectionWaypoints();
            }
            GUI.backgroundColor = oldColor;

            if (GUILayout.Button("Done"))
            {
                Cancel();
            }
            EditorGUILayout.EndVertical();
        }

        #endregion

        private int GetRoadFromWaypoint(PedestrianWaypointSettings waypoint)
        {
            int road = -1;
            for (int i = 0; i < _stopWaypoints.Count; i++)
            {
                for (int j = 0; j < _stopWaypoints[i].pedestrianWaypoints.Count; j++)
                {
                    for (int k = 0; k < waypoint.prev.Count; k++)
                    {
                        if (waypoint.prev[k] == _stopWaypoints[i].pedestrianWaypoints[j])
                        {
                            road = i;
                            break;
                        }

                    }
                    for (int k = 0; k < waypoint.neighbors.Count; k++)
                    {
                        if (waypoint.neighbors[k] == _stopWaypoints[i].pedestrianWaypoints[j])
                        {
                            road = i;
                            break;
                        }
                    }
                }
            }
            return road;
        }
#endif


        internal override void DestroyWindow()
        {
            EditorUtility.SetDirty(_selectedIntersection);
#if GLEY_PEDESTRIAN_SYSTEM
            if (_pedestrianWaypointDrawer != null)
            {
                _pedestrianWaypointDrawer.OnWaypointClicked -= PedestrianWaypointClicked;
            }
#endif
            base.DestroyWindow();
        }
    }
}
