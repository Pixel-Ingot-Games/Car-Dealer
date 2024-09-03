using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class IntersectionSetupWindow : TrafficSetupWindow
    {
        private readonly float scrollAdjustment = 246;

        private PriorityIntersectionSettings[] _allPriorityIntersections;
        private TrafficLightsIntersectionSettings[] _allTrafficLightsIntersections;
        private TrafficLightsCrossingSettings[] _allTrafficLightsCrossings;
        private PriorityCrossingSettings[] _allPriorityCrossings;
        private IntersectionEditorData _intersectionData;
        private IntersectionDrawer _intersectionsDrawer;
        private IntersectionCreator _intersectionCreator;
        private int _nrOfPriorityIntersections;
        private int _nrOfTrafficLightsIntersections;
        private int _nrOfTrafficLightsCrossings;
        private int _nrOfPriorityCrossings;
        private bool _refresh;

        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _intersectionData = new IntersectionEditorData();
            _intersectionsDrawer = new IntersectionDrawer(_intersectionData);
            _intersectionCreator = new IntersectionCreator(_intersectionData);
            _intersectionsDrawer.onIntersectionClicked += IntersectionClicked;
            return this;
        }


        internal override void DrawInScene()
        {
            _refresh = false;
            _allPriorityIntersections = _intersectionsDrawer.DrawPriorityIntersections(true, _editorSave.EditorColors.IntersectionColor, _editorSave.EditorColors.StopWaypointsColor, _editorSave.EditorColors.ExitWaypointsColor, _editorSave.EditorColors.LabelColor);
            _allPriorityCrossings = _intersectionsDrawer.DrawPriorityCrossings(true, _editorSave.EditorColors.IntersectionColor, _editorSave.EditorColors.StopWaypointsColor);
            _allTrafficLightsIntersections = _intersectionsDrawer.DrawTrafficLightsIntersections(true, _editorSave.EditorColors.IntersectionColor, _editorSave.EditorColors.StopWaypointsColor, _editorSave.EditorColors.ExitWaypointsColor, _editorSave.EditorColors.LabelColor);
            _allTrafficLightsCrossings = _intersectionsDrawer.DrawTrafficLightsCrossings(true, _editorSave.EditorColors.IntersectionColor, _editorSave.EditorColors.StopWaypointsColor);

            if (_nrOfPriorityIntersections != _allPriorityIntersections.Length)
            {
                _nrOfPriorityIntersections = _allPriorityIntersections.Length;
                _refresh = true;
            }

            if (_nrOfPriorityCrossings != _allPriorityCrossings.Length)
            {
                _nrOfPriorityCrossings = _allPriorityCrossings.Length;
                _refresh = true;
            }

            if (_nrOfTrafficLightsIntersections != _allTrafficLightsIntersections.Length)
            {
                _nrOfTrafficLightsIntersections = _allTrafficLightsIntersections.Length;
                _refresh = true;
            }

            if (_nrOfTrafficLightsCrossings != _allTrafficLightsCrossings.Length)
            {
                _nrOfTrafficLightsCrossings = _allTrafficLightsCrossings.Length;
                _refresh = true;
            }

            if (_refresh)
            {
                SettingsWindowBase.TriggerRefreshWindowEvent();
            }

            base.DrawInScene();
        }


        protected override void TopPart()
        {
            base.TopPart();
            if (GUILayout.Button("Create Priority Intersection"))
            {
                IntersectionClicked(_intersectionCreator.Create<PriorityIntersectionSettings>());
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Create Priority Crossing"))
            {
                IntersectionClicked(_intersectionCreator.Create<PriorityCrossingSettings>());
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Create Traffic Lights Intersection"))
            {
                IntersectionClicked(_intersectionCreator.Create<TrafficLightsIntersectionSettings>());
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Create Traffic Lights Crossing"))
            {
                IntersectionClicked(_intersectionCreator.Create<TrafficLightsCrossingSettings>());
            }
            EditorGUILayout.Space();

            _editorSave.showAllIntersections = EditorGUILayout.Toggle("Show All Intersections", _editorSave.showAllIntersections);
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - scrollAdjustment));

            if (_editorSave.showAllIntersections)
            {
                _allPriorityCrossings = _intersectionData.GetPriorityCrossings();
                _allPriorityIntersections = _intersectionData.GetPriorityIntersections();
                _allTrafficLightsCrossings = _intersectionData.GetTrafficLightsCrossings();
                _allTrafficLightsIntersections = _intersectionData.GetTrafficLightsIntersections();
            }

            if (_allPriorityIntersections != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Priority Intersections");
                for (int i = 0; i < _allPriorityIntersections.Length; i++)
                {
                    DrawIntersectionButton(_allPriorityIntersections[i]);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (_allPriorityCrossings != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Priority Crossings");
                for (int i = 0; i < _allPriorityCrossings.Length; i++)
                {
                    DrawIntersectionButton(_allPriorityCrossings[i]);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (_allTrafficLightsIntersections != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Traffic Light Intersections");
                for (int i = 0; i < _allTrafficLightsIntersections.Length; i++)
                {
                    DrawIntersectionButton(_allTrafficLightsIntersections[i]);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (_allTrafficLightsCrossings != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Traffic Light Crossings");
                for (int i = 0; i < _allTrafficLightsCrossings.Length; i++)
                {
                    DrawIntersectionButton(_allTrafficLightsCrossings[i]);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            GUILayout.EndScrollView();
        }


        private void IntersectionClicked(GenericIntersectionSettings clickedIntersection)
        {
            SettingsWindow.SetSelectedIntersection(clickedIntersection);
            if (clickedIntersection.GetType().Equals(typeof(TrafficLightsIntersectionSettings)))
            {
                _window.SetActiveWindow(typeof(TrafficLightsIntersectionWindow), true);
            }
            if (clickedIntersection.GetType().Equals(typeof(PriorityIntersectionSettings)))
            {
                _window.SetActiveWindow(typeof(PriorityIntersectionWindow), true);
            }
            if (clickedIntersection.GetType().Equals(typeof(TrafficLightsCrossingSettings)))
            {
                _window.SetActiveWindow(typeof(TrafficLightsCrossingWindow), true);
            }
            if (clickedIntersection.GetType().Equals(typeof(PriorityCrossingSettings)))
            {
                _window.SetActiveWindow(typeof(PriorityCrossingWindow), true);
            }
        }


        private void DrawIntersectionButton(GenericIntersectionSettings intersection)
        {
            if (intersection == null)
            {
                return;
            }
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(intersection.name);
            if (GUILayout.Button("View", GUILayout.Width(BUTTON_DIMENSION)))
            {
                GleyUtilities.TeleportSceneCamera(intersection.transform.position, 10);
            }
            if (GUILayout.Button("Edit", GUILayout.Width(BUTTON_DIMENSION)))
            {
                IntersectionClicked(intersection);
            }
            if (GUILayout.Button("Delete", GUILayout.Width(BUTTON_DIMENSION)))
            {
                _intersectionCreator.DeleteIntersection(intersection);
            }
            EditorGUILayout.EndHorizontal();
        }



        internal override void DestroyWindow()
        {
            if (_intersectionsDrawer != null)
            {
                _intersectionsDrawer.onIntersectionClicked -= IntersectionClicked;
                _intersectionsDrawer.OnDestroy();
            }
            base.DestroyWindow();
        }
    }
}
