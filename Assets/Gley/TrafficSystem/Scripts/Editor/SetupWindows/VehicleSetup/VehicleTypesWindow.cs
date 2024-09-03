using Gley.UrbanSystem.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class VehicleTypesWindow : SetupWindowBase
    {
        private readonly float _scrollAdjustment = 205;

        private List<string> _vehicleCategories = new List<string>();
        private string _errorText;
        

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _errorText = "";
            LoadVehicles();
            return base.Initialize(windowProperties, window);
        }


        protected override void TopPart()
        {
            base.TopPart();
            EditorGUILayout.LabelField("Vehicle types are used to limit vehicle movement.\n" +
                "You can use different vehicle types to restrict the access of different type of vehicles in some areas.");
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));

            for (int i = 0; i < _vehicleCategories.Count; i++)
            {
                GUILayout.BeginHorizontal();
                _vehicleCategories[i] = EditorGUILayout.TextField(_vehicleCategories[i]);
                _vehicleCategories[i] = Regex.Replace(_vehicleCategories[i], @"^[\d-]*\s*", "");
                _vehicleCategories[i] = _vehicleCategories[i].Replace(" ", "");
                _vehicleCategories[i] = _vehicleCategories[i].Trim();
                if (GUILayout.Button("Remove"))
                {
                    _vehicleCategories.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add vehicle category"))
            {
                _vehicleCategories.Add("");
            }

            GUILayout.EndScrollView();
        }


        protected override void BottomPart()
        {
            GUILayout.Label(_errorText);
            if (GUILayout.Button("Save"))
            {
                if (CheckForNull() == false)
                {
                    _errorText = "Success";
                    Save();
                }
            }
            EditorGUILayout.Space();
            base.BottomPart();
        }


        private void LoadVehicles()
        {
            var allCarTypes = Enum.GetValues(typeof(VehicleTypes)).Cast<VehicleTypes>();
            foreach (VehicleTypes car in allCarTypes)
            {
                _vehicleCategories.Add(car.ToString());
            }
        }


        private void Save()
        {
            FileCreator.CreateAgentTypesFile<VehicleTypes>(_vehicleCategories, Internal.TrafficSystemConstants.GLEY_TRAFFIC_SYSTEM, Internal.TrafficSystemConstants.trafficNamespace, Internal.TrafficSystemConstants.agentTypesPath);
        }


        private bool CheckForNull()
        {
            for (int i = 0; i < _vehicleCategories.Count - 1; i++)
            {
                for (int j = i + 1; j < _vehicleCategories.Count; j++)
                {
                    if (_vehicleCategories[i] == _vehicleCategories[j])
                    {
                        _errorText = _vehicleCategories[i] + " Already exists. No duplicates allowed";
                        return true;
                    }
                }
            }
            for (int i = 0; i < _vehicleCategories.Count; i++)
            {
                if (string.IsNullOrEmpty(_vehicleCategories[i]))
                {
                    _errorText = "Car category cannot be empty! Please fill all of them";
                    return true;
                }
            }
            return false;
        }
    }
}
