#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Editor;
using Gley.PedestrianSystem.Internal;
#endif
using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class IntersectionWindow : TrafficSetupWindow
    {
        protected enum WindowActions
        {
            None,
            AssignRoadWaypoints,
            AssignPedestrianWaypoints,
            AddDirectionWaypoints,
            AddExitWaypoints
        }

#if GLEY_PEDESTRIAN_SYSTEM
        protected List<PedestrianWaypointSettings> _pedestrianWaypoints;
        protected List<PedestrianWaypointSettings> _directionWaypoints;
        protected PedestrianWaypointEditorData _pedestrianWaypointData;
        protected PedestrianWaypointDrawer _pedestrianWaypointDrawer;
#endif
        protected List<IntersectionStopWaypointsSettings> _stopWaypoints;
        protected List<WaypointSettings> _exitWaypoints;
        protected GenericIntersectionSettings _selectedIntersection;
        protected WindowActions _currentAction;
        protected int _selectedRoad;
        protected bool _hideWaypoints;
        protected bool _hideConnections;

        private TrafficWaypointEditorData _trafficWaypointData;
        private TrafficWaypointDrawer _trafficWaypointDrawer;
        private IntersectionEditorData _intersectionData;
        private IntersectionDrawer _intersectionDrawer;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _selectedIntersection = SettingsWindow.GetSelectedIntersection();
            _stopWaypoints = _selectedIntersection.GetAssignedWaypoints();
            _exitWaypoints = _selectedIntersection.GetExitWaypoints();


            _trafficWaypointData = new TrafficWaypointEditorData();
            _trafficWaypointDrawer = new TrafficWaypointDrawer(_trafficWaypointData);
            _intersectionData = new IntersectionEditorData();
            _intersectionDrawer = new IntersectionDrawer(_intersectionData);
#if GLEY_PEDESTRIAN_SYSTEM
            _pedestrianWaypoints = _selectedIntersection.GetPedestrianWaypoints();
            _directionWaypoints = _selectedIntersection.GetDirectionWaypoints();
            _pedestrianWaypointData = new PedestrianWaypointEditorData();
            _pedestrianWaypointDrawer = new PedestrianWaypointDrawer(_pedestrianWaypointData);
#endif

            _selectedRoad = -1;
            name = _selectedIntersection.name;
            _currentAction = WindowActions.None;

            _trafficWaypointDrawer.onWaypointClicked += TrafficWaypointClicked;
            return this;
        }

        internal override void DrawInScene()
        {
            base.DrawInScene();
            switch (_currentAction)
            {
                case WindowActions.None:
                    _intersectionDrawer.DrawExitWaypoints(_selectedIntersection, _editorSave.EditorColors.ExitWaypointsColor);
                    _intersectionDrawer.DrawStopWaypoints(_selectedIntersection, int.MaxValue, _editorSave.EditorColors.StopWaypointsColor, _editorSave.EditorColors.LabelColor);
#if GLEY_PEDESTRIAN_SYSTEM
                    _intersectionDrawer.DrawPedestrianWaypoints(_selectedIntersection, int.MaxValue, _editorSave.EditorColors.StopWaypointsColor);
                    _intersectionDrawer.DrawDirectionWaypoints(_selectedIntersection, _editorSave.EditorColors.ExitWaypointsColor);
#endif
                    break;

                case WindowActions.AssignRoadWaypoints:
                    _trafficWaypointDrawer.ShowIntersectionWaypoints(_editorSave.EditorColors.WaypointColor, _hideConnections);
                    _intersectionDrawer.DrawStopWaypoints(_selectedIntersection, _selectedRoad, _editorSave.EditorColors.StopWaypointsColor, _editorSave.EditorColors.LabelColor);
                    break;

                case WindowActions.AddExitWaypoints:
                    _trafficWaypointDrawer.ShowIntersectionWaypoints(_editorSave.EditorColors.WaypointColor, _hideConnections);
                    _intersectionDrawer.DrawExitWaypoints(_selectedIntersection, _editorSave.EditorColors.ExitWaypointsColor);
                    _intersectionDrawer.DrawStopWaypoints(_selectedIntersection, int.MaxValue, _editorSave.EditorColors.StopWaypointsColor, _editorSave.EditorColors.LabelColor);
                    break;

#if GLEY_PEDESTRIAN_SYSTEM
                case WindowActions.AssignPedestrianWaypoints:
                    _pedestrianWaypointDrawer.ShowIntersectionWaypoints(_editorSave.EditorColors.WaypointColor);
                    _intersectionDrawer.DrawPedestrianWaypoints(_selectedIntersection, _selectedRoad, _editorSave.EditorColors.StopWaypointsColor);
                    if (_selectedRoad != -1)
                    {
                        _intersectionDrawer.DrawStopWaypoints(_selectedIntersection, _selectedRoad, _editorSave.EditorColors.StopWaypointsColor, _editorSave.EditorColors.LabelColor);
                    }
                    break;

                case WindowActions.AddDirectionWaypoints:
                    _pedestrianWaypointDrawer.DrawPossibleDirectionWaypoints(_selectedIntersection.GetPedestrianWaypoints(), _editorSave.EditorColors.WaypointColor);
                    _intersectionDrawer.DrawPedestrianWaypoints(_selectedIntersection, int.MaxValue, _editorSave.EditorColors.StopWaypointsColor);
                    _intersectionDrawer.DrawDirectionWaypoints(_selectedIntersection, _editorSave.EditorColors.ExitWaypointsColor);
                    break;
#endif
            }
        }


        private void TrafficWaypointClicked(WaypointSettings clickedWaypoint, bool leftClick)
        {
            switch (_currentAction)
            {
                case WindowActions.AssignRoadWaypoints:
                    AddWaypointToList(clickedWaypoint, _stopWaypoints[_selectedRoad].roadWaypoints);
                    break;
                case WindowActions.AddExitWaypoints:
                    AddWaypointToList(clickedWaypoint, _exitWaypoints);
                    break;
            }
            SceneView.RepaintAll();
            SettingsWindowBase.TriggerRefreshWindowEvent();
        }


        protected void DisplayList<T>(List<T> list, ref bool globalDraw) where T : WaypointSettingsBase
        {
            if (list == null)
                return;
            Color oldColor;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    continue;
                }
                EditorGUILayout.BeginHorizontal();

                list[i] = (T)EditorGUILayout.ObjectField(list[i], typeof(T), true);

                oldColor = GUI.backgroundColor;
                if (list[i].draw == true)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("View"))
                {
                    ViewWaypoint(list[i], ref globalDraw);
                }
                GUI.backgroundColor = oldColor;

                if (GUILayout.Button("Delete"))
                {
                    list.RemoveAt(i);
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
        }


        private void ViewWaypoint(WaypointSettingsBase waypoint, ref bool globalDraw)
        {
            waypoint.draw = !waypoint.draw;
            if (waypoint.draw == false)
            {
                globalDraw = false;
            }
            SceneView.RepaintAll();
        }


        protected void ViewAllDirectionWaypoints()
        {
#if GLEY_PEDESTRIAN_SYSTEM
            _editorSave.showDirectionWaypoints = !_editorSave.showDirectionWaypoints;
            for (int i = 0; i < _directionWaypoints.Count; i++)
            {
                _directionWaypoints[i].draw = _editorSave.showDirectionWaypoints;
            }
#endif
        }


        protected void ViewAllExitWaypoints()
        {
            _editorSave.showExitWaypoints = !_editorSave.showExitWaypoints;
            for (int i = 0; i < _exitWaypoints.Count; i++)
            {
                _exitWaypoints[i].draw = _editorSave.showExitWaypoints;
            }
        }


        protected void AddLightObjects(string title, List<GameObject> objectsList)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title + ":");
            for (int i = 0; i < objectsList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                objectsList[i] = (GameObject)EditorGUILayout.ObjectField(objectsList[i], typeof(GameObject), true);

                if (GUILayout.Button("Delete"))
                {
                    objectsList.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add " + title + " Objects"))
            {
                objectsList.Add(null);
            }
            EditorGUILayout.EndVertical();
        }


        protected void AddWaypointToList<T>(T waypoint, List<T> listToAdd) where T : WaypointSettingsBase
        {
            if (!listToAdd.Contains(waypoint))
            {
                waypoint.draw = true;
                listToAdd.Add(waypoint);
            }
            else
            {
                listToAdd.Remove(waypoint);
            }
        }


        protected void SaveSettings()
        {
            _selectedIntersection.gameObject.name = name;
            if (_stopWaypoints.Count > 0)
            {
                Vector3 position = new Vector3();
                int nr = 0;
                for (int i = 0; i < _stopWaypoints.Count; i++)
                {
                    for (int j = 0; j < _stopWaypoints[i].roadWaypoints.Count; j++)
                    {
                        position += _stopWaypoints[i].roadWaypoints[j].transform.position;
                        nr++;
                    }
                }
                _selectedIntersection.transform.position = position / nr;
            }
        }


        protected void Cancel()
        {
            _selectedRoad = -1;
            _currentAction = WindowActions.None;
            SceneView.RepaintAll();
        }


        internal override void DestroyWindow()
        {
            if (_trafficWaypointDrawer != null)
            {
                _trafficWaypointDrawer.onWaypointClicked -= TrafficWaypointClicked;
            }

            _trafficWaypointDrawer?.OnDestroy();
            _intersectionDrawer?.OnDestroy();
#if GLEY_PEDESTRIAN_SYSTEM
            _pedestrianWaypointDrawer?.OnDestroy();
#endif
            base.DestroyWindow();
        }
    }
}
