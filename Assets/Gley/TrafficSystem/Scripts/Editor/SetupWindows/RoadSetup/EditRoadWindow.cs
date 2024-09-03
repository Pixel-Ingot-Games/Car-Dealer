using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class EditRoadWindow : TrafficSetupWindow
    {
        private readonly float maxValue = 480;
        private readonly float minValue = 340;

        private bool[] allowedCarIndex;

        private TrafficRoadData _roadData;
        private TrafficLaneData _laneData;
        private TrafficConnectionEditorData _connectionData;
        private TrafficRoadDrawer _roadDrawer;
        private TrafficLaneDrawer _laneDrawer;
        private TrafficWaypointCreator _waypointCreator;
        private TrafficLaneCreator _laneCreator;
        private TrafficConnectionCreator _connectionCreator;
        private Road _selectedRoad;
        private MoveTools _moveTool;
        private float _scrollAdjustment;
        private int _nrOfCars;
        private bool _showCustomizations;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);

            _roadData = new TrafficRoadData();
            _laneData = new TrafficLaneData(_roadData);
            _connectionData = new TrafficConnectionEditorData(_roadData);

            _roadDrawer = new TrafficRoadDrawer(_roadData);
            _laneDrawer = new TrafficLaneDrawer(_laneData);

            _waypointCreator = new TrafficWaypointCreator();
            _laneCreator = new TrafficLaneCreator(_laneData, _waypointCreator);
            _connectionCreator = new TrafficConnectionCreator(_connectionData, _waypointCreator);

            _selectedRoad = SettingsWindow.GetSelectedRoad();
            _selectedRoad.justCreated = false;
            _moveTool = _editorSave.MoveTool;
            _nrOfCars = System.Enum.GetValues(typeof(VehicleTypes)).Length;
            allowedCarIndex = new bool[_nrOfCars];
            for (int i = 0; i < allowedCarIndex.Length; i++)
            {
                if (_editorSave.globalCarList.Contains((VehicleTypes)i))
                {
                    allowedCarIndex[i] = true;
                }
            }
            return this;
        }


        internal override void DrawInScene()
        {
            if (_selectedRoad == null)
            {
                Debug.LogWarning("No road selected");
                return;
            }

            _roadDrawer.DrawPath(_selectedRoad, _moveTool,_editorSave.EditorColors.RoadColor, _editorSave.EditorColors.AnchorPointColor, _editorSave.EditorColors.ControlPointColor, _editorSave.EditorColors.LabelColor, true);
            _laneDrawer.DrawAllLanes(_selectedRoad, _editorSave.ViewRoadWaypoints, _editorSave.viewRoadLaneChanges, _editorSave.ViewLabels, _editorSave.EditorColors.LaneColor, _editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.DisconnectedColor, _editorSave.EditorColors.LaneChangeColor, _editorSave.EditorColors.LabelColor);

            base.DrawInScene();
        }

        protected override void TopPart()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Press SHIFT + Left Click to add a road point");
            EditorGUILayout.LabelField("Press SHIFT + Right Click to remove a road point");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            _editorSave.ViewRoadWaypoints = EditorGUILayout.Toggle("View Waypoints", _editorSave.ViewRoadWaypoints);
            _editorSave.viewRoadLaneChanges = EditorGUILayout.Toggle("View Lane Changes", _editorSave.viewRoadLaneChanges);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            _selectedRoad.nrOfLanes = EditorGUILayout.IntField("Nr of lanes", _selectedRoad.nrOfLanes);
            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                _selectedRoad.UpdateLaneNumber(_editorSave.maxSpeed, System.Enum.GetValues(typeof(VehicleTypes)).Length);
            }

            _selectedRoad.laneWidth = EditorGUILayout.FloatField("Lane width (m)", _selectedRoad.laneWidth);
            _selectedRoad.waypointDistance = EditorGUILayout.FloatField("Waypoint distance ", _selectedRoad.waypointDistance);

            EditorGUI.BeginChangeCheck();
            _moveTool = (MoveTools)EditorGUILayout.EnumPopup("Select move tool ", _moveTool);
            _showCustomizations = EditorGUILayout.Toggle("Change Colors ", _showCustomizations);
            if (_showCustomizations == true)
            {
                _scrollAdjustment = maxValue;
                _editorSave.EditorColors.RoadColor = EditorGUILayout.ColorField("Road Color", _editorSave.EditorColors.RoadColor);
                _editorSave.EditorColors.LaneColor = EditorGUILayout.ColorField("Lane Color", _editorSave.EditorColors.LaneColor);
                _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField("Waypoint Color", _editorSave.EditorColors.WaypointColor);
                _editorSave.EditorColors.DisconnectedColor = EditorGUILayout.ColorField("Disconnected Color", _editorSave.EditorColors.DisconnectedColor);
                _editorSave.EditorColors.LaneChangeColor = EditorGUILayout.ColorField("Lane Change Color", _editorSave.EditorColors.LaneChangeColor);
                _editorSave.EditorColors.ControlPointColor = EditorGUILayout.ColorField("Control Point Color", _editorSave.EditorColors.ControlPointColor);
                _editorSave.EditorColors.AnchorPointColor = EditorGUILayout.ColorField("Anchor Point Color", _editorSave.EditorColors.AnchorPointColor);
            }
            else
            {
                _scrollAdjustment = minValue;
            }
            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }

            base.TopPart();
        }


        protected override void ScrollPart(float width, float height)
        {
            if (_selectedRoad == null)
            {
                Debug.LogWarning("No road selected");
                return;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Global Lane Settings", EditorStyles.boldLabel);
            GUILayout.Label("Allowed Vehicle Types:");
            for (int i = 0; i < _nrOfCars; i++)
            {
                allowedCarIndex[i] = EditorGUILayout.Toggle(((VehicleTypes)i).ToString(), allowedCarIndex[i]);
            }
            if (GUILayout.Button("Apply Global Vehicle Settings"))
            {
                ApplyGlobalCarSettings();
            }

            EditorGUILayout.BeginHorizontal();
            _editorSave.maxSpeed = EditorGUILayout.IntField("Global Max Speed", _editorSave.maxSpeed);
            if (GUILayout.Button("Apply Speed"))
            {
                SetSpeedOnLanes(_selectedRoad, _editorSave.maxSpeed);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Apply All Settings"))
            {
                ApplyGlobalCarSettings();
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Individual Lane Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (_selectedRoad.lanes != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                for (int i = 0; i < _selectedRoad.lanes.Count; i++)
                {
                    if (_selectedRoad.lanes[i].laneDirection == true)
                    {
                        DrawLaneButton(i);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                for (int i = 0; i < _selectedRoad.lanes.Count; i++)
                {
                    if (_selectedRoad.lanes[i].laneDirection == false)
                    {
                        DrawLaneButton(i);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            GUILayout.EndScrollView();

            base.ScrollPart(width, height);
        }


        protected override void BottomPart()
        {
            if (_selectedRoad == null)
            {
                Debug.LogWarning("No road selected");
                return;
            }

            if (GUILayout.Button("Generate waypoints"))
            {
                _editorSave.ViewRoadWaypoints = true;

                if (_selectedRoad.nrOfLanes <= 0)
                {
                    Debug.LogError("Nr of lanes has to be >0");
                    return;
                }

                if (_selectedRoad.waypointDistance <= 0)
                {
                    Debug.LogError("Waypoint distance needs to be >0");
                    return;
                }

                if (_selectedRoad.laneWidth <= 0)
                {
                    Debug.LogError("Lane width has to be >0");
                    return;
                }
                _connectionCreator.DeleteConnectionsWithThisRoad(_selectedRoad);
                _laneCreator.GenerateWaypoints(_selectedRoad, _window.GetGroundLayer());

                EditorUtility.SetDirty(_selectedRoad);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.BeginHorizontal();
            _selectedRoad.otherLaneLinkDistance = EditorGUILayout.IntField("Link distance", (_selectedRoad).otherLaneLinkDistance);
            if (_selectedRoad.otherLaneLinkDistance < 1)
            {
                _selectedRoad.otherLaneLinkDistance = 1;
            }
            if (GUILayout.Button("Link other lanes"))
            {
                _editorSave.ViewRoadWaypoints = true;
                _editorSave.viewRoadLaneChanges = true;
                _laneCreator.LinkOtherLanes(_selectedRoad);
                EditorUtility.SetDirty(_selectedRoad);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Unlink other lanes"))
            {
                _laneCreator.UnLinckOtherLanes((Road)_selectedRoad);
                EditorUtility.SetDirty(_selectedRoad);
                AssetDatabase.SaveAssets();
            }

            base.BottomPart();
        }


        internal override void MouseMove(Vector3 mousePosition)
        {
            if (_selectedRoad == null)
            {
                Debug.LogWarning("No road selected");
                return;
            }
            base.MouseMove(mousePosition);
            _roadDrawer.SelectSegmentIndex(_selectedRoad, mousePosition);
        }


        internal override void LeftClick(Vector3 mousePosition, bool clicked)
        {
            if (_selectedRoad == null)
            {
                Debug.LogWarning("No road selected");
                return;
            }

            _roadDrawer.AddPathPoint(mousePosition, _selectedRoad);
            base.LeftClick(mousePosition, clicked);
        }


        internal override void RightClick(Vector3 mousePosition)
        {
            if (_selectedRoad == null)
            {
                Debug.LogWarning("No road selected");
                return;
            }
            _roadDrawer.Delete(_selectedRoad, mousePosition);
            base.RightClick(mousePosition);
        }


        private void ApplyGlobalCarSettings()
        {
            SetSpeedOnLanes(_selectedRoad, _editorSave.maxSpeed);
            for (int i = 0; i < _selectedRoad.lanes.Count; i++)
            {
                for (int j = 0; j < allowedCarIndex.Length; j++)
                {
                    _selectedRoad.lanes[i].allowedCars[j] = allowedCarIndex[j];
                }
            }
        }


        private void SetSpeedOnLanes(Road selectedRoad, int maxSpeed)
        {
            for (int i = 0; i < selectedRoad.lanes.Count; i++)
            {
                selectedRoad.lanes[i].laneSpeed = maxSpeed;
            }
        }


        private void DrawLaneButton(int currentLane)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            _selectedRoad.lanes[currentLane].laneSpeed = EditorGUILayout.IntField("Lane " + currentLane + ", Lane Speed:", _selectedRoad.lanes[currentLane].laneSpeed);
            string buttonLebel = "<--";
            if (_selectedRoad.lanes[currentLane].laneDirection == false)
            {
                buttonLebel = "-->";
            }
            if (GUILayout.Button(buttonLebel))
            {
                _selectedRoad.lanes[currentLane].laneDirection = !_selectedRoad.lanes[currentLane].laneDirection;
                _connectionCreator.DeleteConnectionsWithThisLane(_selectedRoad, currentLane);
                _laneCreator.SwitchLaneDirection(_selectedRoad, currentLane);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Allowed vehicle types on this lane:");
            for (int i = 0; i < _nrOfCars; i++)
            {
                if (i >= _selectedRoad.lanes[currentLane].allowedCars.Length)
                {
                    _selectedRoad.lanes[currentLane].UpdateAllowedCars(_nrOfCars);
                }
                _selectedRoad.lanes[currentLane].allowedCars[i] = EditorGUILayout.Toggle(((VehicleTypes)i).ToString(), _selectedRoad.lanes[currentLane].allowedCars[i]);
            }
            EditorGUILayout.EndVertical();
        }


        internal override void DestroyWindow()
        {
            _editorSave.MoveTool = _moveTool;
            _editorSave.globalCarList = new List<VehicleTypes>();
            for (int i = 0; i < allowedCarIndex.Length; i++)
            {
                if (allowedCarIndex[i] == true)
                {
                    _editorSave.globalCarList.Add((VehicleTypes)i);
                }
            }
            _editorSave.nrOfLanes = _selectedRoad.nrOfLanes;
            _editorSave.LaneWidth = _selectedRoad.laneWidth;
            _editorSave.WaypointDistance = _selectedRoad.waypointDistance;
            _editorSave.otherLaneLinkDistance = _selectedRoad.otherLaneLinkDistance;
            _roadDrawer?.OnDestroy();
            _laneDrawer?.OnDestroy();

            base.DestroyWindow();
        }
    }
}
