using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class DebugWindow : SetupWindowBase
    {
        DebugSettings _save;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _save = DebugOptions.LoadOrCreateDebugSettings();
            return base.Initialize(windowProperties, window);
        }


        protected override void TopPart()
        {
            _save.debug = EditorGUILayout.Toggle("Debug Vehicle Actions", _save.debug);
            if (_save.debug == false)
            {
                _save.debugSpeed = false;

            }
            _save.debugSpeed = EditorGUILayout.Toggle("Debug Vehicle Speed", _save.debugSpeed);
            if (_save.debugSpeed == true)
            {
                _save.debug = true;
            }

            _save.debugIntersections = EditorGUILayout.Toggle("Debug Intersections", _save.debugIntersections);
            _save.debugWaypoints = EditorGUILayout.Toggle("Debug Waypoints", _save.debugWaypoints);
            _save.debugDisabledWaypoints = EditorGUILayout.Toggle("Disabled Waypoints", _save.debugDisabledWaypoints);
            _save.drawBodyForces = EditorGUILayout.Toggle("Draw Body Force", _save.drawBodyForces);
            _save.debugDensity = EditorGUILayout.Toggle("Debug Density", _save.debugDensity);
            _save.debugPathFinding = EditorGUILayout.Toggle("Debug Path Finding", _save.debugPathFinding);
            _save.DebugSpawnWaypoints = EditorGUILayout.Toggle("Spawn Waypoints", _save.DebugSpawnWaypoints);
            _save.DebugGiveWay = EditorGUILayout.Toggle("Debug Give Way", _save.DebugGiveWay);
            _save.DebugPlayModeWaypoints = EditorGUILayout.Toggle("Play Mode Waypoints", _save.DebugPlayModeWaypoints);

            if (_save.debugPathFinding == true)
            {
                _save.debug = true;
            }

            if (_save.DebugPlayModeWaypoints == true)
            {
                _save.ShowIndex = EditorGUILayout.Toggle("Show Index", _save.ShowIndex);
                _save.ShowPosition = EditorGUILayout.Toggle("Show Position", _save.ShowPosition);
            }

            base.TopPart();
        }

        protected override void ScrollPart(float width, float height)
        {
            base.ScrollPart(width, height);
            EditorGUILayout.Space();
            if (GUILayout.Button("Remove Unused Scene Objects"))
            {
                new VersionUpdater().DeleteSceneComponents();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Remove Unused Project Scripts"))
            {
                new VersionUpdater().DeleteProjectScripts();
            }
            EditorGUILayout.Space();
        }

        internal override void DestroyWindow()
        {
            base.DestroyWindow();
            EditorUtility.SetDirty(_save);
        }
    }
}
