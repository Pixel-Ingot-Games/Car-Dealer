#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Internal;
#endif
using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class PriorityCrossingWindow : IntersectionWindow
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
                    break;
                case WindowActions.AssignPedestrianWaypoints:
                    AddWaypointToList(clickedWaypoint, _stopWaypoints[_selectedRoad].pedestrianWaypoints);
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
#endif
        }



        private void ViewAssignedStopWaypoints()
        {
            Color oldColor;
            for (int i = 0; i < _stopWaypoints.Count; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent("Stop waypoints:", "The vehicle will stop at this point until the intersection allows it to continue. " +
                "\nEach road that enters in intersection should have its own set of stop waypoints"));

                DisplayList(_stopWaypoints[i].roadWaypoints, ref _stopWaypoints[i].draw);

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

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

#if GLEY_PEDESTRIAN_SYSTEM
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Pedestrian waypoints:");
                EditorGUILayout.Space();
                DisplayList(_stopWaypoints[i].pedestrianWaypoints, ref _stopWaypoints[i].draw);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Assign Pedestrian Waypoints"))
                {
                    _selectedRoad = i;
                    _currentAction = WindowActions.AssignPedestrianWaypoints;
                }

                oldColor = GUI.backgroundColor;
                if (_editorSave.showPedestrianWaypoints == true)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("View Pedestrian Waypoints"))
                {
                    ViewAllPedestrianWaypoints();
                }
                GUI.backgroundColor = oldColor;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
#endif
            }
        }


#if GLEY_PEDESTRIAN_SYSTEM
        private void ViewAllPedestrianWaypoints()
        {
            _editorSave.showPedestrianWaypoints = !_editorSave.showPedestrianWaypoints;
            for (int j = 0; j < _stopWaypoints[0].pedestrianWaypoints.Count; j++)
            {
                _stopWaypoints[0].pedestrianWaypoints[j].draw = _editorSave.showPedestrianWaypoints;
            }
        }
#endif


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
        }


#if GLEY_PEDESTRIAN_SYSTEM
        #region DirectionWaypoints
        private void ViewDirectionWaypoints()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
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
