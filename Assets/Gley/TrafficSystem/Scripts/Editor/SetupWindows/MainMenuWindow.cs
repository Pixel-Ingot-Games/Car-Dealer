using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;
#if GLEY_PEDESTRIAN_SYSTEM
using Gley.PedestrianSystem.Editor;
#endif

namespace Gley.TrafficSystem.Editor
{
    internal class MainMenuWindow : SetupWindowBase
    {
        private const string SAVING = "GleyTrafficSaving";
        private const string STEP = "GleyTrafficStep";

        private readonly int _scrollAdjustment = 140;

        private int _step;
        private bool _saving;

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _saving = EditorPrefs.GetBool(SAVING);
            _step = EditorPrefs.GetInt(STEP);
            return base.Initialize(windowProperties, window);
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));

            EditorGUILayout.Space();

            if (GUILayout.Button("Import Required Packages"))
            {
                _window.SetActiveWindow(typeof(ImportPackagesWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Scene Setup"))
            {
                _window.SetActiveWindow(typeof(SceneSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Road Setup"))
            {
                _window.SetActiveWindow(typeof(RoadSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Intersection Setup"))
            {
                _window.SetActiveWindow(typeof(IntersectionSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Waypoint Setup"))
            {
                _window.SetActiveWindow(typeof(WaypointSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Speed Routes Setup"))
            {
                _window.SetActiveWindow(typeof(SpeedRoutesSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Vehicle Routes Setup"))
            {
                _window.SetActiveWindow(typeof(VehicleRoutesSetupWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Waypoint Priority Setup"))
            {
                _window.SetActiveWindow(typeof(WaypointPriorityWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Path Finding"))
            {
                _window.SetActiveWindow(typeof(PathFindingWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("External Tools"))
            {
                _window.SetActiveWindow(typeof(ExternalToolsWindow), true);
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Switch driving direction (Beta)"))
            {
                SwitchWaypointDirection.SwitchAll();
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Debug"))
            {
                _window.SetActiveWindow(typeof(DebugWindow), true);
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndScrollView();
            base.ScrollPart(width, height);
        }


        protected override void BottomPart()
        {
            if (GUILayout.Button("Apply Settings"))
            {
                if (FileCreator.LoadOrCreateLayers<LayerSetup>(Internal.TrafficSystemConstants.layerPath).edited == false)
                {
                    Debug.LogWarning("Layers are not configured. Go to Tools -> Gley -> Traffic System->Scene Setup -> Layer Setup");
                }

                if (FindFirstObjectByType<VehicleComponent>() != null)
                {
                    Debug.LogError("Failed: Please remove VehicleComponent from the following objects:");
                    var objects = FindObjectsByType<VehicleComponent>(FindObjectsSortMode.None);
                    foreach (var obj in objects)
                    {
                        Debug.Log(obj.name, obj);
                    }
                    return;
                }

                _step = 0;
                SaveSettings();
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Disable Traffic System"))
            {
                Common.PreprocessorDirective.AddToCurrent(Internal.TrafficSystemConstants.GLEY_TRAFFIC_SYSTEM, true);
            }
            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL("https://gley.gitbook.io/mobile-traffic-system/quick-start");
            }

            base.BottomPart();
        }


        private void SaveSettings()
        {
            Debug.Log($"Saving {_step + 1}/4");
            switch (_step)
            {
                case 0:
                    if (!File.Exists($"{Application.dataPath}{Internal.TrafficSystemConstants.agentTypesPath}/VehicleTypes.cs"))
                    {
                        FileCreator.CreateAgentTypesFile<VehicleTypes>(null, Internal.TrafficSystemConstants.GLEY_TRAFFIC_SYSTEM, Internal.TrafficSystemConstants.trafficNamespace, Internal.TrafficSystemConstants.agentTypesPath);
                    }
                    _saving = true;
                    EditorPrefs.SetBool(SAVING, _saving);
                    _step++;
                    EditorPrefs.SetInt(STEP, _step);
                    break;
                case 1:
                    Common.PreprocessorDirective.AddToCurrent(Internal.TrafficSystemConstants.GLEY_TRAFFIC_SYSTEM, false);
                    _saving = true;
                    EditorPrefs.SetBool(SAVING, _saving);
                    _step++;
                    EditorPrefs.SetInt(STEP, _step);
                    break;
                case 2:
                    ApplyTrafficSettings();
                    _saving = true;
                    EditorPrefs.SetBool(SAVING, _saving);
                    _step++;
                    EditorPrefs.SetInt(STEP, _step);
                    break;
                default:
                    Debug.Log("Save Done");
                    break;
            }
        }


        internal override void InspectorUpdate()
        {
            if (_saving)
            {
                if (EditorApplication.isCompiling == false)
                {
                    _saving = false;
                    EditorPrefs.SetBool(SAVING, false);
                    SaveSettings();
                }
            }
        }


        private void ApplyTrafficSettings()
        {
            var gridData = new GridEditorData();
            var gridCreator = new GridCreator(gridData);
            gridCreator.GenerateGrid(gridData.GetGridCellSize());

            var trafficWaypointsConverter = new TrafficWaypointsConverter();
            trafficWaypointsConverter.ConvertWaypoints();

#if GLEY_PEDESTRIAN_SYSTEM
            var pedestrianWaypointsConverter = new PedestrianWaypointsConverter();
            pedestrianWaypointsConverter.ConvertWaypoints();
#endif

            var intersectionConverter = new IntersectionConverter();
            intersectionConverter.ConvertAllIntersections();
        }
    }
}
