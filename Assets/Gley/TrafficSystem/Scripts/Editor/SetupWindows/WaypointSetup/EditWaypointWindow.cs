using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class EditWaypointWindow : TrafficSetupWindow
    {
        protected struct VehicleDisplay
        {
            public Color color;
            public int vehicle;
            public bool active;
            public bool view;

            public VehicleDisplay(bool active, int vehicle, Color color)
            {
                this.active = active;
                this.vehicle = vehicle;
                this.color = color;
                view = false;
            }
        }


        protected enum ListToAdd
        {
            None,
            Neighbors,
            OtherLanes,
            GiveWayWaypoints
        }

        private readonly float _scrollAdjustment = 230;

        private VehicleDisplay[] _vehicleDisplay;
        private TrafficWaypointEditorData _trafficWaypointData;
        private TrafficWaypointDrawer _waypointDrawer;
        private WaypointSettings _selectedWaypoint;
        private WaypointSettings _clickedWaypoint;
        private ListToAdd _selectedList;
        private int _nrOfAllowedVehicles;
        private int _maxSpeed;
        private int _priority;
        private int _penalty;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);

            _selectedWaypoint = SettingsWindow.GetSelectedWaypoint();
            _vehicleDisplay = SetCarDisplay();
            _maxSpeed = _selectedWaypoint.maxSpeed;
            _priority = _selectedWaypoint.priority;
            _penalty = _selectedWaypoint.penalty;
            _trafficWaypointData = new TrafficWaypointEditorData();
            _waypointDrawer = new TrafficWaypointDrawer(_trafficWaypointData);
            _waypointDrawer.onWaypointClicked += WaypointClicked;

            return this;
        }


        internal override void DrawInScene()
        {
            base.DrawInScene();

            if (_selectedList != ListToAdd.None)
            {
                _waypointDrawer.DrawWaypointsForLink(_selectedWaypoint, _selectedWaypoint.neighbors, _selectedWaypoint.otherLanes, _editorSave.EditorColors.WaypointColor);
            }

            _waypointDrawer.DrawCurrentWaypoint(_selectedWaypoint, _editorSave.EditorColors.SelectedWaypointColor, _editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.LaneChangeColor, _editorSave.EditorColors.PrevWaypointColor, _editorSave.EditorColors.ComplexGiveWayColor);

            for (int i = 0; i < _vehicleDisplay.Length; i++)
            {
                if (_vehicleDisplay[i].view)
                {
                    _waypointDrawer.ShowWaypointsWithVehicle(_vehicleDisplay[i].vehicle, _vehicleDisplay[i].color);
                }
            }

            if (_clickedWaypoint)
            {
                _waypointDrawer.DrawSelectedWaypoint(_clickedWaypoint, _editorSave.EditorColors.SelectedRoadConnectorColor);
            }
        }


        protected override void TopPart()
        {
            base.TopPart();
            EditorGUI.BeginChangeCheck();
            _editorSave.EditorColors.SelectedWaypointColor = EditorGUILayout.ColorField("Selected Color ", _editorSave.EditorColors.SelectedWaypointColor);
            _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField("Neighbor Color ", _editorSave.EditorColors.WaypointColor);
            _editorSave.EditorColors.LaneChangeColor = EditorGUILayout.ColorField("Lane Change Color ", _editorSave.EditorColors.LaneChangeColor);
            _editorSave.EditorColors.PrevWaypointColor = EditorGUILayout.ColorField("Previous Color ", _editorSave.EditorColors.PrevWaypointColor);
            _editorSave.EditorColors.ComplexGiveWayColor = EditorGUILayout.ColorField("Required Free Waypoints ", _editorSave.EditorColors.ComplexGiveWayColor);

            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Select Waypoint"))
            {
                Selection.activeGameObject = _selectedWaypoint.gameObject;
            }

            base.TopPart();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            EditorGUI.BeginChangeCheck();
            if (_selectedList == ListToAdd.None)
            {
                _selectedWaypoint.giveWay = EditorGUILayout.Toggle(new GUIContent("Give Way", "Vehicle will stop when reaching this waypoint and check if next waypoint is free before continuing"), _selectedWaypoint.giveWay);

                EditorGUILayout.BeginHorizontal();
                _selectedWaypoint.complexGiveWay = EditorGUILayout.Toggle(new GUIContent("Complex Give Way", "Vehicle will stop when reaching this waypoint check if all selected waypoints are free before continue"), _selectedWaypoint.complexGiveWay);
                if (_selectedWaypoint.complexGiveWay)
                {
                    if (GUILayout.Button("Pick Required Free Waypoints"))
                    {
                        //PickFreeWaypoints();
                        _selectedList = ListToAdd.GiveWayWaypoints;
                    }
                }
                EditorGUILayout.EndHorizontal();

                _selectedWaypoint.zipperGiveWay = EditorGUILayout.Toggle(new GUIContent("Zipper Give Way", "Vehicles will stop before reaching this waypoint and continue randomly one at the time"), _selectedWaypoint.zipperGiveWay);
                _selectedWaypoint.triggerEvent = EditorGUILayout.Toggle(new GUIContent("Trigger Event", "If a vehicle reaches this, it will trigger an event"), _selectedWaypoint.triggerEvent);
                if (_selectedWaypoint.triggerEvent == true)
                {
                    _selectedWaypoint.eventData = EditorGUILayout.TextField(new GUIContent("Event Data", "This string will be sent as a parameter for the event"), _selectedWaypoint.eventData);
                }

                EditorGUILayout.BeginHorizontal();
                _maxSpeed = EditorGUILayout.IntField(new GUIContent("Max speed", "The maximum speed allowed in this waypoint"), _maxSpeed);
                if (GUILayout.Button("Set Speed"))
                {
                    _selectedWaypoint.SetSpeedForAllNeighbors(_maxSpeed);
                }
                EditorGUILayout.EndHorizontal();




                EditorGUILayout.BeginHorizontal();
                _priority = EditorGUILayout.IntField(new GUIContent("Spawn priority", "If the priority is higher, the vehicles will have higher chances to spawn on this waypoint"), _priority);
                if (GUILayout.Button("Set Priority"))
                {
                    _selectedWaypoint.SetPriorityForAllNeighbors(_priority);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _penalty = EditorGUILayout.IntField(new GUIContent("Waypoint penalty", "Used for path finding. If penalty is higher vehicles are less likely to pick this route"), _penalty);
                if (GUILayout.Button("Set Penalty "))
                {
                    _selectedWaypoint.SetPenaltyForAllNeighbors(_penalty);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent("Allowed vehicles: ", "Only the following vehicles can pass through this waypoint"), EditorStyles.boldLabel);
                EditorGUILayout.Space();

                for (int i = 0; i < _nrOfAllowedVehicles; i++)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    _vehicleDisplay[i].active = EditorGUILayout.Toggle(_vehicleDisplay[i].active, GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField(((VehicleTypes)i).ToString());
                    _vehicleDisplay[i].color = EditorGUILayout.ColorField(_vehicleDisplay[i].color, GUILayout.MaxWidth(80));
                    Color oldColor = GUI.backgroundColor;
                    if (_vehicleDisplay[i].view)
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    if (GUILayout.Button("View", GUILayout.MaxWidth(64)))
                    {
                        _vehicleDisplay[i].view = !_vehicleDisplay[i].view;
                    }
                    GUI.backgroundColor = oldColor;

                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Set"))
                {
                    SetCars();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
            MakeListOperations("Neighbors", "From this waypoint a moving agent can continue to the following ones", _selectedWaypoint.neighbors, ListToAdd.Neighbors);

            EditorGUILayout.Space();
            MakeListOperations("Other Lanes", "Connections to other lanes, used for overtaking", _selectedWaypoint.otherLanes, ListToAdd.OtherLanes);

            if (_selectedList == ListToAdd.GiveWayWaypoints)
            {
                EditorGUILayout.Space();
                MakeListOperations("Pick Required Free Waypoints", "Waypoints required to be free for Complex Give Way", _selectedWaypoint.giveWayList, ListToAdd.GiveWayWaypoints);
            }
            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }

            base.ScrollPart(width, height);
            GUILayout.EndScrollView();
        }


        private void SetCars()
        {
            List<VehicleTypes> result = new List<VehicleTypes>();
            for (int i = 0; i < _vehicleDisplay.Length; i++)
            {
                if (_vehicleDisplay[i].active)
                {
                    result.Add((VehicleTypes)_vehicleDisplay[i].vehicle);
                }
            }
            _selectedWaypoint.SetVehicleTypesForAllNeighbors(result);
        }


        private void DeleteWaypoint(WaypointSettingsBase waypoint, ListToAdd list)
        {
            switch (list)
            {
                case ListToAdd.Neighbors:
                    waypoint.prev.Remove(_selectedWaypoint);
                    _selectedWaypoint.neighbors.Remove(waypoint);
                    break;
                case ListToAdd.OtherLanes:
                    _selectedWaypoint.otherLanes.Remove((WaypointSettings)waypoint);
                    break;
                case ListToAdd.GiveWayWaypoints:
                    _selectedWaypoint.giveWayList.Remove((WaypointSettings)waypoint);
                    break;
            }
            _clickedWaypoint = null;
            SceneView.RepaintAll();
        }


        private void AddNeighbor(WaypointSettingsBase neighbor)
        {
            if (!_selectedWaypoint.neighbors.Contains(neighbor))
            {
                _selectedWaypoint.neighbors.Add(neighbor);
                neighbor.prev.Add(_selectedWaypoint);
            }
            else
            {
                neighbor.prev.Remove(_selectedWaypoint);
                _selectedWaypoint.neighbors.Remove(neighbor);
            }
        }


        private void AddOtherLanes(WaypointSettings waypoint)
        {
            if (!_selectedWaypoint.otherLanes.Contains(waypoint))
            {
                _selectedWaypoint.otherLanes.Add(waypoint);
            }
            else
            {
                _selectedWaypoint.otherLanes.Remove(waypoint);
            }
        }


        private void AddGiveWayWaypoints(WaypointSettings waypoint)
        {
            if (!_selectedWaypoint.giveWayList.Contains(waypoint))
            {
                _selectedWaypoint.giveWayList.Add(waypoint);
            }
            else
            {
                _selectedWaypoint.giveWayList.Remove(waypoint);
            }
        }


        private void WaypointClicked(WaypointSettings clickedWaypoint, bool leftClick)
        {
            if (leftClick)
            {
                if (_selectedList == ListToAdd.Neighbors)
                {
                    AddNeighbor(clickedWaypoint);
                }

                if (_selectedList == ListToAdd.OtherLanes)
                {
                    AddOtherLanes(clickedWaypoint);
                }

                if (_selectedList == ListToAdd.GiveWayWaypoints)
                {
                    AddGiveWayWaypoints(clickedWaypoint);
                }

                if (_selectedList == ListToAdd.None)
                {
                    _window.SetActiveWindow(typeof(EditWaypointWindow), false);
                }
            }
            SettingsWindowBase.TriggerRefreshWindowEvent();
        }


        private void MakeListOperations<T>(string title, string description, List<T> listToEdit, ListToAdd listType) where T : WaypointSettingsBase
        {
            //if (listType == ListToAdd.GiveWayWaypoints)
            //    return;
            if (_selectedList == listType || _selectedList == ListToAdd.None)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent(title, description), EditorStyles.boldLabel);
                EditorGUILayout.Space();
                for (int i = 0; i < listToEdit.Count; i++)
                {
                    if (listToEdit[i] == null)
                    {
                        listToEdit.RemoveAt(i);
                        i--;
                        continue;
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(listToEdit[i].name);
                    Color oldColor = GUI.backgroundColor;
                    if (listToEdit[i] == _clickedWaypoint)
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    if (GUILayout.Button("View", GUILayout.MaxWidth(64)))
                    {
                        if (listToEdit[i] == _clickedWaypoint)
                        {
                            _clickedWaypoint = null;
                        }
                        else
                        {
                            ViewWaypoint(listToEdit[i]);
                        }
                    }
                    GUI.backgroundColor = oldColor;
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(64)))
                    {
                        DeleteWaypoint(listToEdit[i], listType);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();
                if (_selectedList == ListToAdd.None)
                {
                    if (GUILayout.Button("Add/Remove " + title))
                    {
                        //baseWaypointDrawer.Initialize();
                        _selectedList = listType;
                    }
                }
                else
                {
                    if (GUILayout.Button("Done"))
                    {
                        _selectedList = ListToAdd.None;
                        SceneView.RepaintAll();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }
        }


        private VehicleDisplay[] SetCarDisplay()
        {
            _nrOfAllowedVehicles = System.Enum.GetValues(typeof(VehicleTypes)).Length;
            VehicleDisplay[] carDisplay = new VehicleDisplay[_nrOfAllowedVehicles];
            for (int i = 0; i < _nrOfAllowedVehicles; i++)
            {
                carDisplay[i] = new VehicleDisplay(_selectedWaypoint.allowedCars.Contains((VehicleTypes)i), i, Color.white);
            }
            return carDisplay;
        }


        private void ViewWaypoint(WaypointSettingsBase waypoint)
        {
            _clickedWaypoint = (WaypointSettings)waypoint;
            GleyUtilities.TeleportSceneCamera(waypoint.transform.position);
        }


        internal override void DestroyWindow()
        {
            if (_waypointDrawer != null)
            {
                _waypointDrawer.onWaypointClicked -= WaypointClicked;
                _waypointDrawer.OnDestroy();
            }
            if (_selectedWaypoint)
            {
                EditorUtility.SetDirty(_selectedWaypoint);
            }
            base.DestroyWindow();
        }
    }
}
