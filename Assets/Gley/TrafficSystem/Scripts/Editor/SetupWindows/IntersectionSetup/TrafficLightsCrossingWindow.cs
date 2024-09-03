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
    internal class TrafficLightsCrossingWindow : IntersectionWindow
    {
        private readonly float _scrollAdjustment = 242;

        private List<GameObject> _pedestrianRedLightObjects = new List<GameObject>();
        private List<GameObject> _pedestrianGreenLightObjects = new List<GameObject>();
        private TrafficLightsCrossingSettings _selectedTrafficLightsCrossing;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _selectedTrafficLightsCrossing = _selectedIntersection as TrafficLightsCrossingSettings;
#if GLEY_PEDESTRIAN_SYSTEM
            _pedestrianRedLightObjects = _selectedTrafficLightsCrossing.pedestrianRedLightObjects;
            _pedestrianGreenLightObjects = _selectedTrafficLightsCrossing.pedestrianGreenLightObjects;
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

            _selectedTrafficLightsCrossing.greenLightTime = EditorGUILayout.FloatField("Green Light Time", _selectedTrafficLightsCrossing.greenLightTime);
            _selectedTrafficLightsCrossing.yellowLightTime = EditorGUILayout.FloatField("Yellow Light Time", _selectedTrafficLightsCrossing.yellowLightTime);
            _selectedTrafficLightsCrossing.redLightTime = EditorGUILayout.FloatField("Red Light Time", _selectedTrafficLightsCrossing.redLightTime);
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

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space();
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
                _pedestrianWaypoints[i].draw =_editorSave.showPedestrianWaypoints;
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
