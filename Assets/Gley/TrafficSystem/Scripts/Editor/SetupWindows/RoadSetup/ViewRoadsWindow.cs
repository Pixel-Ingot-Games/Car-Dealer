using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class ViewRoadsWindow : TrafficSetupWindow
    {
        private readonly float _nothingSelectedValue = 199;
        private readonly float _viewLanesValue = 219;
        private readonly float _viewWaypointsValue = 239;

        private List<Road> _roadsOfInterest;
        private TrafficRoadCreator _trafficRoadCreator;
        private TrafficRoadData _trafficRoadData;
        private TrafficRoadDrawer _trafficRoadDrawer;
        private TrafficLaneData _trafficLaneData;
        private TrafficLaneDrawer _trafficLaneDrawer;
        private TrafficConnectionCreator _trafficConnectionCreator;
        private TrafficConnectionEditorData _trafficConnectionData;
        private TrafficWaypointCreator _trafficWaypointCreator;
        private string _drawButton = "Draw All Roads";
        private float _scrollAdjustment;
        private bool _drawAllRoads;
        private int _nrOfRoads;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);

            _trafficRoadData = new TrafficRoadData();
            _trafficLaneData = new TrafficLaneData(_trafficRoadData);
            _trafficConnectionData = new TrafficConnectionEditorData(_trafficRoadData);

            _trafficWaypointCreator = new TrafficWaypointCreator();
            _trafficRoadCreator = new TrafficRoadCreator(_trafficRoadData);
            _trafficConnectionCreator = new TrafficConnectionCreator(_trafficConnectionData, _trafficWaypointCreator);
           
            _trafficRoadDrawer = new TrafficRoadDrawer(_trafficRoadData);       
            _trafficLaneDrawer = new TrafficLaneDrawer(_trafficLaneData);

            return this;
        }


        internal override void DrawInScene()
        {
            base.DrawInScene();
            _roadsOfInterest = _trafficRoadDrawer.ShowAllRoads(MoveTools.None, _editorSave.EditorColors.RoadColor, _editorSave.EditorColors.AnchorPointColor, _editorSave.EditorColors.ControlPointColor, _editorSave.EditorColors.LabelColor, _editorSave.ViewLabels);

            if (_roadsOfInterest.Count != _nrOfRoads)
            {
                _nrOfRoads = _roadsOfInterest.Count;
                SettingsWindowBase.TriggerRefreshWindowEvent();
            }
            if (_editorSave.ViewRoadLanes)
            {
                for (int i = 0; i < _nrOfRoads; i++)
                {
                    _trafficLaneDrawer.DrawAllLanes(_roadsOfInterest[i], _editorSave.ViewRoadWaypoints, _editorSave.viewRoadLaneChanges, _editorSave.ViewLabels, _editorSave.EditorColors.LaneColor, _editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.DisconnectedColor, _editorSave.EditorColors.LaneChangeColor, _editorSave.EditorColors.LabelColor);
                }
            }
        }


        protected override void TopPart()
        {
            base.TopPart();

            if (GUILayout.Button(_drawButton))
            {
                _drawAllRoads = !_drawAllRoads;
                if (_drawAllRoads == true)
                {
                    _drawButton = "Clear All";
                }
                else
                {
                    _drawButton = "Draw All Roads";
                }

                _trafficRoadDrawer.SetDrawProperty(_drawAllRoads);
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            _editorSave.EditorColors.RoadColor = EditorGUILayout.ColorField("Road Color", _editorSave.EditorColors.RoadColor);

            if (_editorSave.ViewLabels)
            {
                EditorGUILayout.BeginHorizontal();
                _editorSave.ViewLabels = EditorGUILayout.Toggle("View Labels", _editorSave.ViewLabels, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.LabelColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LabelColor);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                _editorSave.ViewLabels = EditorGUILayout.Toggle("View Labels", _editorSave.ViewLabels);
            }

            if (_editorSave.ViewRoadLanes)
            {
                _scrollAdjustment = _viewLanesValue;
                EditorGUILayout.BeginHorizontal();
                _editorSave.ViewRoadLanes = EditorGUILayout.Toggle("View Lanes", _editorSave.ViewRoadLanes, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.LaneColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LaneColor);
                EditorGUILayout.EndHorizontal();

                if (_editorSave.ViewRoadWaypoints)
                {
                    _scrollAdjustment = _viewWaypointsValue;
                    EditorGUILayout.BeginHorizontal();
                    _editorSave.ViewRoadWaypoints = EditorGUILayout.Toggle("View Waypoints", _editorSave.ViewRoadWaypoints, GUILayout.Width(TOGGLE_WIDTH));
                    _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField(_editorSave.EditorColors.WaypointColor);
                    EditorGUILayout.EndHorizontal();

                    if (_editorSave.viewRoadLaneChanges)
                    {
                        EditorGUILayout.BeginHorizontal();
                        _editorSave.viewRoadLaneChanges = EditorGUILayout.Toggle("View Lane Changes", _editorSave.viewRoadLaneChanges, GUILayout.Width(TOGGLE_WIDTH));
                        _editorSave.EditorColors.LaneChangeColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LaneChangeColor);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        _editorSave.viewRoadLaneChanges = EditorGUILayout.Toggle("View Lane Changes", _editorSave.viewRoadLaneChanges);
                    }
                }
                else
                {
                    _editorSave.ViewRoadWaypoints = EditorGUILayout.Toggle("View Waypoints", _editorSave.ViewRoadWaypoints);
                }
            }
            else
            {
                _scrollAdjustment = _nothingSelectedValue;
                _editorSave.ViewRoadLanes = EditorGUILayout.Toggle("View Lanes", _editorSave.ViewRoadLanes);
            }

            EditorGUI.EndChangeCheck();

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
            EditorGUILayout.Space();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));

            if (_roadsOfInterest != null)
            {
                if (_roadsOfInterest.Count == 0)
                {
                    EditorGUILayout.LabelField("Nothing in view");
                }
                for (int i = 0; i < _roadsOfInterest.Count; i++)
                {
                    DisplayRoad(_roadsOfInterest[i]);
                }
            }
            GUILayout.EndScrollView();
        }


        private void DisplayRoad(Road road)
        {
            if (road == null)
                return;
            EditorGUILayout.BeginHorizontal();
            road.draw = EditorGUILayout.Toggle(road.draw, GUILayout.Width(TOGGLE_DIMENSION));
            GUILayout.Label(road.gameObject.name);

            if (GUILayout.Button("View"))
            {
                GleyUtilities.TeleportSceneCamera(road.transform.position);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Select"))
            {
                SelectWaypoint(road);
            }
            if (GUILayout.Button("Delete"))
            {
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("Delete " + road.name + "?", "Are you sure you want to delete " + road.name + "? \nYou cannot undo this operation.", "Delete", "Cancel"))
                {
                    DeleteCurrentRoad(road);
                }
                EditorGUI.EndChangeCheck();
            }

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
        }


        private void SelectWaypoint(Road road)
        {
            SettingsWindow.SetSelectedRoad(road);
            _window.SetActiveWindow(typeof(EditRoadWindow), true);
        }


        private void DeleteCurrentRoad(Road road)
        {
            _trafficConnectionCreator.DeleteConnectionsWithThisRoad(road);
            _trafficRoadCreator.DeleteCurrentRoad(road);
            Undo.undoRedoPerformed += UndoPerformed;
        }


        protected void UndoPerformed()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
        }


        internal override void DestroyWindow()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
            base.DestroyWindow();

            _trafficRoadDrawer.OnDestroy();
            _trafficLaneDrawer.OnDestroy();
        }
    }
}
