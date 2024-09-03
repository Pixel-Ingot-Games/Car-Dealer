#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Internal;
#endif
using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficLightsIntersectionWindow : IntersectionWindow
    {
        private readonly float _scrollAdjustment = 242;

        private List<GameObject> _pedestrianRedLightObjects = new List<GameObject>();
        private List<GameObject> _pedestrianGreenLightObjects = new List<GameObject>();
        private TrafficLightsIntersectionSettings _selectedTrafficLightsIntersection;

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);       
            _selectedTrafficLightsIntersection = _selectedIntersection as TrafficLightsIntersectionSettings;
#if GLEY_PEDESTRIAN_SYSTEM
            _pedestrianRedLightObjects = _selectedTrafficLightsIntersection.pedestrianRedLightObjects;
            _pedestrianGreenLightObjects = _selectedTrafficLightsIntersection.pedestrianGreenLightObjects;
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
            _selectedTrafficLightsIntersection.greenLightTime = EditorGUILayout.FloatField("Green Light Time", _selectedTrafficLightsIntersection.greenLightTime);
            _selectedTrafficLightsIntersection.yellowLightTime = EditorGUILayout.FloatField("Yellow Light Time", _selectedTrafficLightsIntersection.yellowLightTime);
            _selectedTrafficLightsIntersection.setGreenLightTimePerRoad = EditorGUILayout.Toggle("Set Green Light Per Road", _selectedTrafficLightsIntersection.setGreenLightTimePerRoad);
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
                    AddWaypointToList(clickedWaypoint, _directionWaypoints);
                    break;
                case WindowActions.AssignPedestrianWaypoints:
                    AddWaypointToList(clickedWaypoint, _pedestrianWaypoints);
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
            ViewPedestrianWaypoints();
            EditorGUILayout.Space();

            ViewDirectionWaypoints();
            EditorGUILayout.Space();
#endif
            ViewExitWaypoints();
        }
        #region StopWaypoints


        private void ViewAssignedStopWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Stop waypoints:", "The vehicle will stop at this point until the intersection allows it to continue. " +
               "\nEach road that enters in intersection should have its own set of stop waypoints"));
            Color oldColor;
            for (int i = 0; i < _stopWaypoints.Count; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Road " + (i + 1));
                if (_selectedTrafficLightsIntersection.setGreenLightTimePerRoad)
                {
                    if (_stopWaypoints[i].greenLightTime == 0)
                    {
                        _stopWaypoints[i].greenLightTime = _selectedTrafficLightsIntersection.greenLightTime;
                    }
                    _stopWaypoints[i].greenLightTime = EditorGUILayout.FloatField("Green Light Time", _stopWaypoints[i].greenLightTime);
                }
                else
                {
                    _stopWaypoints[i].greenLightTime = _selectedTrafficLightsIntersection.greenLightTime;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                DisplayList(_stopWaypoints[i].roadWaypoints, ref _stopWaypoints[i].draw);
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();


                if (GUILayout.Button("Assign Road"))
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
            EditorGUILayout.LabelField(new GUIContent("Stop waypoints:", "The vehicle will stop at this point until the intersection allows it to continue. " +
               "\nEach road that enters in intersection should have its own set of stop waypoints"));
            Color oldColor;


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Road " + (_selectedRoad + 1));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            DisplayList(_stopWaypoints[_selectedRoad].roadWaypoints, ref _stopWaypoints[_selectedRoad].draw);
            EditorGUILayout.Space();

            oldColor = GUI.backgroundColor;
            if (_stopWaypoints[_selectedRoad].draw == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Road Waypoints"))
            {
                ViewRoadWaypoints(_selectedRoad);

            }
            GUI.backgroundColor = oldColor;

            EditorGUILayout.Space();
            AddLightObjects("Red Light", _stopWaypoints[_selectedRoad].redLightObjects);
            AddLightObjects("Yellow Light", _stopWaypoints[_selectedRoad].yellowLightObjects);
            AddLightObjects("Green Light", _stopWaypoints[_selectedRoad].greenLightObjects);
            EditorGUILayout.Space();
            if (GUILayout.Button("Done"))
            {
                Cancel();
            }

            EditorGUILayout.EndVertical();
        }


        private void ViewRoadWaypoints(int i)
        {
            _stopWaypoints[i].draw = !_stopWaypoints[i].draw;
            for (int j = 0; j < _stopWaypoints[i].roadWaypoints.Count; j++)
            {
                _stopWaypoints[i].roadWaypoints[j].draw = _stopWaypoints[i].draw;
            }
        }
        #endregion

#if GLEY_PEDESTRIAN_SYSTEM
        #region PedestrianWaypoints
        private void ViewPedestrianWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Pedestrian waypoints:", "Pedestrian waypoints are used for waiting before crossing the road. " +
                "Pedestrians will stop on those waypoints and wait for green color."));
            if (_selectedTrafficLightsIntersection.setGreenLightTimePerRoad)
            {
                if (_selectedTrafficLightsIntersection.pedestrianGreenLightTime == 0)
                {
                    _selectedTrafficLightsIntersection.pedestrianGreenLightTime = _selectedTrafficLightsIntersection.greenLightTime;
                }
                _selectedTrafficLightsIntersection.pedestrianGreenLightTime = EditorGUILayout.FloatField("Green Light Time", _selectedTrafficLightsIntersection.pedestrianGreenLightTime);
            }

            EditorGUILayout.EndHorizontal(); EditorGUILayout.Space();
            DisplayList(_pedestrianWaypoints, ref _editorSave.showPedestrianWaypoints);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Pedestrian Waypoints"))
            {
                _selectedRoad = -1;
                _currentAction = WindowActions.AssignPedestrianWaypoints;
            }
            Color oldColor = GUI.backgroundColor;
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

        }


        private void AddPedestrianWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Pedestrian stop waypoints:", "pedestrians will stop at this point until the intersection allows them to continue. " +
            "\nEach crossing in intersection should have its own set of stop waypoints corresponding to its road"));

            DisplayList(_pedestrianWaypoints, ref _editorSave.showPedestrianWaypoints);

            EditorGUILayout.Space();
            Color oldColor = GUI.backgroundColor;
            if (_editorSave.showPedestrianWaypoints == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Pedestrian Waypoints"))
            {
                ViewAllPedestrianWaypoints();
            }
            GUI.backgroundColor = oldColor;

            AddLightObjects("Red Light - Pedestrians", _pedestrianRedLightObjects);
            AddLightObjects("Green Light - Pedestrians", _pedestrianGreenLightObjects);

            if (GUILayout.Button("Done"))
            {
                Cancel();
            }
            EditorGUILayout.EndVertical();
        }


        private void ViewAllPedestrianWaypoints()
        {
            _editorSave.showPedestrianWaypoints = !_editorSave.showPedestrianWaypoints;
            for (int i = 0; i < _pedestrianWaypoints.Count; i++)
            {
                _pedestrianWaypoints[i].draw = _editorSave.showPedestrianWaypoints;
            }
        }
        #endregion


        #region DirectionWaypoints
        private void ViewDirectionWaypoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent("Crossing direction waypoints:", "For each stop waypoint a direction needs to be specified\n" +
                "Only if a pedestrian goes to that direction it will stop, otherwise it will pass through stop waypoint"));
            EditorGUILayout.Space();
            DisplayList(_directionWaypoints, ref _editorSave.showDirectionWaypoints);

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

            DisplayList(_directionWaypoints, ref _editorSave.showDirectionWaypoints);

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
#endif

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
