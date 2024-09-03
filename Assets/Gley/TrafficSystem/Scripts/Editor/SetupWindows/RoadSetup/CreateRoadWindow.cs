using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class CreateRoadWindow : TrafficSetupWindow
    {
        private List<Road> _roadsOfInterest;
        private TrafficRoadCreator _trafficRoadCreator;
        private TrafficRoadData _trafficRoadData;
        private TrafficRoadDrawer _trafficRoadDrawer;
        private TrafficLaneData _trafficLaneData;
        private TrafficLaneDrawer _trafficLaneDrawer;
        private Vector3 _firstClick;
        private Vector3 _secondClick;
        private int _nrOfRoads;

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _trafficRoadData = new TrafficRoadData();
            _trafficLaneData = new TrafficLaneData(_trafficRoadData);
            
            _trafficRoadCreator = new TrafficRoadCreator(_trafficRoadData);
                   
            _trafficRoadDrawer = new TrafficRoadDrawer(_trafficRoadData);            
            _trafficLaneDrawer = new TrafficLaneDrawer(_trafficLaneData);

            base.Initialize(windowProperties, window);
            return this;
        }


        internal override void DrawInScene()
        {
            if (_firstClick != Vector3.zero)
            {
                Handles.SphereHandleCap(0, _firstClick, Quaternion.identity, 1, EventType.Repaint);
            }

            if (_editorSave.ViewOtherRoads)
            {
                _roadsOfInterest= _trafficRoadDrawer.ShowAllRoads(MoveTools.None, _editorSave.EditorColors.RoadColor, _editorSave.EditorColors.AnchorPointColor, _editorSave.EditorColors.ControlPointColor, _editorSave.EditorColors.LabelColor, _editorSave.ViewLabels);

                if (_roadsOfInterest.Count != _nrOfRoads)
                {
                    _nrOfRoads = _roadsOfInterest.Count;
                    SettingsWindowBase.TriggerRefreshWindowEvent();
                }
                if(_editorSave.ViewRoadLanes)
                {
                    for (int i = 0; i < _nrOfRoads; i++)
                    {
                        _trafficLaneDrawer.DrawAllLanes(_roadsOfInterest[i], _editorSave.ViewRoadWaypoints, _editorSave.viewRoadLaneChanges, _editorSave.ViewLabels, _editorSave.EditorColors.LaneColor, _editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.DisconnectedColor, _editorSave.EditorColors.LaneChangeColor, _editorSave.EditorColors.LabelColor);
                    }
                }
            }
            base.DrawInScene();
        }


        protected override void TopPart()
        {
            base.TopPart();
            EditorGUILayout.LabelField("Press SHIFT + Left Click to add a road point");
            EditorGUILayout.LabelField("Press SHIFT + Right Click to remove a road point");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("If you are not able to draw, make sure your ground/road is on the layer marked as Road inside Layer Setup");
            EditorGUILayout.Space();
            _editorSave.leftSideTraffic = EditorGUILayout.Toggle("LeftSideTraffic", _editorSave.leftSideTraffic);
        }


        protected override void ScrollPart(float width, float height)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            _editorSave.ViewOtherRoads = EditorGUILayout.Toggle("View Other Roads", _editorSave.ViewOtherRoads, GUILayout.Width(TOGGLE_WIDTH));
            _editorSave.EditorColors.RoadColor = EditorGUILayout.ColorField(_editorSave.EditorColors.RoadColor);
            EditorGUILayout.EndHorizontal();

            if (_editorSave.ViewOtherRoads)
            {
                EditorGUILayout.BeginHorizontal();
                _editorSave.ViewLabels = EditorGUILayout.Toggle("View Labels", _editorSave.ViewLabels, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.LabelColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LabelColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _editorSave.ViewRoadLanes = EditorGUILayout.Toggle("View Lanes", _editorSave.ViewRoadLanes, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.LaneColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LaneColor);
                EditorGUILayout.EndHorizontal();

                if (_editorSave.ViewRoadLanes)
                {
                    EditorGUILayout.BeginHorizontal();
                    _editorSave.ViewRoadWaypoints = EditorGUILayout.Toggle("View Waypoints", _editorSave.ViewRoadWaypoints, GUILayout.Width(TOGGLE_WIDTH));
                    _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField(_editorSave.EditorColors.WaypointColor);
                    _editorSave.EditorColors.DisconnectedColor = EditorGUILayout.ColorField(_editorSave.EditorColors.DisconnectedColor);
                    if (_editorSave.ViewRoadWaypoints == false)
                    {
                        _editorSave.viewRoadLaneChanges = false;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _editorSave.viewRoadLaneChanges = EditorGUILayout.Toggle("View Lane Changes", _editorSave.viewRoadLaneChanges, GUILayout.Width(TOGGLE_WIDTH));
                    if (_editorSave.viewRoadLaneChanges == true)
                    {
                        _editorSave.ViewRoadWaypoints = true;
                    }
                    _editorSave.EditorColors.LaneChangeColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LaneChangeColor);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.EndChangeCheck();

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
            base.ScrollPart(width, height);
        }

        internal override void LeftClick(Vector3 mousePosition, bool clicked)
        {
            if (_firstClick == Vector3.zero)
            {
                _firstClick = mousePosition;
            }
            else
            {
                _secondClick = mousePosition;
                CreateRoad();
            }
            base.LeftClick(mousePosition, clicked);
        }

        void CreateRoad()
        {
            Road selectedRoad = _trafficRoadCreator.Create(
                _editorSave.nrOfLanes, 
                _editorSave.LaneWidth, 
                _editorSave.WaypointDistance, 
                TrafficSystemConstants.roadName, 
                _firstClick, 
                _secondClick,
                _editorSave.maxSpeed, 
                System.Enum.GetValues(typeof(VehicleTypes)).Length, 
                _editorSave.leftSideTraffic,
                _editorSave.otherLaneLinkDistance);
            SettingsWindow.SetSelectedRoad(selectedRoad);
            _window.SetActiveWindow(typeof(EditRoadWindow), false);
            _firstClick = Vector3.zero;
            _secondClick = Vector3.zero;
        }


        internal override void UndoAction()
        {
            base.UndoAction();
            if (_secondClick == Vector3.zero)
            {
                if (_firstClick != Vector3.zero)
                {
                    _firstClick = Vector3.zero;
                }
            }
        }


        internal override void DestroyWindow()
        {
            _trafficRoadDrawer?.OnDestroy();
            _trafficLaneDrawer?.OnDestroy();
            base.DestroyWindow();
        }
    }
}