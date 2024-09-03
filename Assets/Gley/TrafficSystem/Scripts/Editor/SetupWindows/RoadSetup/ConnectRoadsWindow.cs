using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class ConnectRoadsWindow : TrafficSetupWindow
    {
        private readonly float _maxValue = 384;
        private readonly float _minValue = 264;

        private List<ConnectionCurve> _connectionsOfInterest;
        private List<Road> _roadsOfInterest;
        private bool[] _allowedCarIndex;
        private TrafficWaypointCreator _waypointCreator;
        private TrafficRoadData _roadData;
        private TrafficRoadDrawer _roadDrawer;
        private TrafficLaneData _laneData;
        private TrafficLaneDrawer _laneDrawer;
        private TrafficConnectionCreator _connectionCreator;
        private TrafficConnectionEditorData _connectionData;
        private TrafficConnectionDrawer _connectionDrawer;
        private Road _clickedRoad;
        private float _scrollAdjustment;
        private int _nrOfRoads;
        private int _nrOfCars;
        private int _nrOfConnections;
        private int _clickedLane;
        private bool _drawAllConnections;
        private bool _showCustomizations;
        private bool _drawOutConnectors;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);

            _roadData = new TrafficRoadData();
            _laneData = new TrafficLaneData(_roadData);
            _connectionData = new TrafficConnectionEditorData(_roadData);

            _waypointCreator = new TrafficWaypointCreator();
            _connectionCreator = new TrafficConnectionCreator(_connectionData, _waypointCreator);

            _roadDrawer = new TrafficRoadDrawer(_roadData);
            _laneDrawer = new TrafficLaneDrawer(_laneData);
            _connectionDrawer = new TrafficConnectionDrawer(_connectionData);

            _nrOfCars = System.Enum.GetValues(typeof(VehicleTypes)).Length;
            _allowedCarIndex = new bool[_nrOfCars];
            for (int i = 0; i < _allowedCarIndex.Length; i++)
            {
                _allowedCarIndex[i] = true;
            }

            _connectionDrawer.onWaypointClicked += WaypointClicked;
            _drawOutConnectors = true;
            return this;
        }

        internal override void DrawInScene()
        {
            if(_roadData.HasErrors())
            {
                return;
            }

            _roadsOfInterest = _roadDrawer.ShowAllRoads(MoveTools.None, _editorSave.EditorColors.RoadColor, _editorSave.EditorColors.AnchorPointColor, _editorSave.EditorColors.ControlPointColor, _editorSave.EditorColors.LabelColor, _editorSave.ViewLabels);

            if (_roadsOfInterest.Count != _nrOfRoads)
            {
                _nrOfRoads = _roadsOfInterest.Count;
                SettingsWindowBase.TriggerRefreshWindowEvent();
            }

            for (int i = 0; i < _nrOfRoads; i++)
            {
                _laneDrawer.DrawAllLanes(_roadsOfInterest[i], _editorSave.ViewRoadWaypoints, _editorSave.viewRoadLaneChanges, _editorSave.ViewLabels, _editorSave.EditorColors.LaneColor, _editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.DisconnectedColor, _editorSave.EditorColors.LaneChangeColor, _editorSave.EditorColors.LabelColor);
            }

            _connectionsOfInterest = _connectionDrawer.ShowAllConnections(_roadsOfInterest, _editorSave.ViewLabels, _editorSave.EditorColors.ConnectorLaneColor, _editorSave.EditorColors.AnchorPointColor,
               _editorSave.EditorColors.RoadConnectorColor, _editorSave.EditorColors.SelectedRoadConnectorColor, _editorSave.EditorColors.DisconnectedColor, _editorSave.WaypointDistance, _editorSave.EditorColors.LabelColor, _editorSave.EditorColors.WaypointColor,
               _drawOutConnectors, _clickedRoad, _clickedLane);

            if (_connectionsOfInterest.Count != _nrOfConnections)
            {
                _nrOfConnections = _connectionsOfInterest.Count;
                SettingsWindowBase.TriggerRefreshWindowEvent();
            }

            base.DrawInScene();
        }


        protected override void TopPart()
        {
            base.TopPart();
            string drawButton = "Draw All Connections";
            if (_drawAllConnections == true)
            {
                drawButton = "Clear All";
            }

            if (GUILayout.Button(drawButton))
            {
                DrawButton();
            }

            EditorGUI.BeginChangeCheck();
            if (_showCustomizations == false)
            {
                _scrollAdjustment = _minValue;
                _showCustomizations = EditorGUILayout.Toggle("Change Colors ", _showCustomizations);
                _editorSave.ViewLabels = EditorGUILayout.Toggle("View Labels", _editorSave.ViewLabels);
                _editorSave.ViewRoadWaypoints = EditorGUILayout.Toggle("View Waypoints", _editorSave.ViewRoadWaypoints);
                _editorSave.viewRoadLaneChanges = EditorGUILayout.Toggle("View Lane Changes", _editorSave.viewRoadLaneChanges);
            }
            else
            {
                _scrollAdjustment = _maxValue;
                _showCustomizations = EditorGUILayout.Toggle("Change Colors ", _showCustomizations);
                EditorGUILayout.BeginHorizontal();
                _editorSave.ViewLabels = EditorGUILayout.Toggle("View Labels", _editorSave.ViewLabels, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.LabelColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LabelColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _editorSave.ViewRoadWaypoints = EditorGUILayout.Toggle("View Waypoints", _editorSave.ViewRoadWaypoints, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField(_editorSave.EditorColors.WaypointColor);
                _editorSave.EditorColors.DisconnectedColor = EditorGUILayout.ColorField(_editorSave.EditorColors.DisconnectedColor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _editorSave.viewRoadLaneChanges = EditorGUILayout.Toggle("View Lane Changes", _editorSave.viewRoadLaneChanges, GUILayout.Width(TOGGLE_WIDTH));
                _editorSave.EditorColors.LaneChangeColor = EditorGUILayout.ColorField(_editorSave.EditorColors.LaneChangeColor);
                EditorGUILayout.EndHorizontal();


                _editorSave.EditorColors.RoadColor = EditorGUILayout.ColorField("Road Color", _editorSave.EditorColors.RoadColor);
                _editorSave.EditorColors.LaneColor = EditorGUILayout.ColorField("Lane Color", _editorSave.EditorColors.LaneColor);
                _editorSave.EditorColors.ConnectorLaneColor = EditorGUILayout.ColorField("Connector Lane Color", _editorSave.EditorColors.ConnectorLaneColor);
                _editorSave.EditorColors.AnchorPointColor = EditorGUILayout.ColorField("Anchor Point Color", _editorSave.EditorColors.AnchorPointColor);
                _editorSave.EditorColors.RoadConnectorColor = EditorGUILayout.ColorField("Road Connector Color", _editorSave.EditorColors.RoadConnectorColor);
                _editorSave.EditorColors.SelectedRoadConnectorColor = EditorGUILayout.ColorField("Selected Connector Color", _editorSave.EditorColors.SelectedRoadConnectorColor);
            }
            EditorGUI.EndChangeCheck();

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
        }


        protected override void ScrollPart(float width, float height)
        {
            base.ScrollPart(width, height);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));

            if (_connectionsOfInterest != null)
            {
                for (int i = 0; i < _connectionsOfInterest.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _connectionsOfInterest[i].draw = EditorGUILayout.Toggle(_connectionsOfInterest[i].draw, GUILayout.Width(TOGGLE_DIMENSION));
                    EditorGUILayout.LabelField(_connectionsOfInterest[i].GetName());
                    Color oldColor = GUI.backgroundColor;
                    if (_connectionsOfInterest[i].drawWaypoints == true)
                    {
                        GUI.backgroundColor = Color.green;
                    }

                    if (GUILayout.Button("Waypoints", GUILayout.Width(BUTTON_DIMENSION)))
                    {
                        _connectionsOfInterest[i].drawWaypoints = !_connectionsOfInterest[i].drawWaypoints;
                    }
                    GUI.backgroundColor = oldColor;

                    if (GUILayout.Button("View", GUILayout.Width(BUTTON_DIMENSION)))
                    {
                        View(i);
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(BUTTON_DIMENSION)))
                    {
                        if (EditorUtility.DisplayDialog("Delete " + _connectionsOfInterest[i].name + "?", "Are you sure you want to delete " + _connectionsOfInterest[i].name + "? \nYou cannot undo this operation.", "Delete", "Cancel"))
                        {
                            _connectionCreator.DeleteConnection(_connectionsOfInterest[i]);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();
            GUILayout.EndScrollView();
            EditorGUILayout.Space();

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
        }


        protected override void BottomPart()
        {
            _editorSave.WaypointDistance = EditorGUILayout.FloatField("Waypoint distance ", _editorSave.WaypointDistance);
            if (_editorSave.WaypointDistance <= 0)
            {
                Debug.LogWarning("Waypoint distance needs to be >0. will be set to 1 by default");
                _editorSave.WaypointDistance = 1;
            }

            if (GUILayout.Button("Generate Selected Connections"))
            {
                GenerateSelectedConnections();
            }
            base.BottomPart();
        }


        private void WaypointClicked(Road road, int lane)
        {
            if (_drawOutConnectors == true)
            {
                _drawOutConnectors = false;
                _clickedRoad = road;
                _clickedLane = lane;
            }
            else
            {
                _drawOutConnectors = true;
                if (road != null)
                {
                    _connectionCreator.CreateConnection(road.transform.parent.GetComponent<ConnectionPool>(), _clickedRoad, _clickedLane, road, lane, _editorSave.WaypointDistance);
                }
                _clickedRoad = null;
                _clickedLane = -1;

            }
        }


        private void DrawButton()
        {
            _drawAllConnections = !_drawAllConnections;
            for (int i = 0; i < _connectionsOfInterest.Count; i++)
            {
                _connectionsOfInterest[i].draw = _drawAllConnections;
                if (_drawAllConnections == false)
                {
                    _connectionsOfInterest[i].drawWaypoints = false;
                }
            }
        }


        void GenerateSelectedConnections()
        {
            _connectionCreator.GenerateConnections(_connectionsOfInterest, _editorSave.WaypointDistance);
            SceneView.RepaintAll();
        }


        private void View(int curveIndex)
        {
            GleyUtilities.TeleportSceneCamera(_connectionsOfInterest[curveIndex].GetOutConnector().gameObject.transform.position);
        }


        internal override void DestroyWindow()
        {
            if (_connectionDrawer != null)
            {
                _connectionDrawer.onWaypointClicked -= WaypointClicked;
            }

            _roadDrawer?.OnDestroy();
            _laneDrawer?.OnDestroy();
            _connectionDrawer?.OnDestroy();

            base.DestroyWindow();
        }
    }
}
