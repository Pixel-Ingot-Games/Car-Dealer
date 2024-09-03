using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class IntersectionDrawer : Drawer
    {
        private readonly int _roadWaypointDimension = 1;
#if GLEY_PEDESTRIAN_SYSTEM
        private readonly float _pedestrianWaypointDimension = 0.5f;
#endif

        private PriorityIntersectionSettings[] _priorityIntersectionsInView;
        private PriorityCrossingSettings[] _priorityCrossingsInView;
        private TrafficLightsIntersectionSettings[] _trafficLightsIntersectionsInView;
        private TrafficLightsCrossingSettings[] _trafficLightsCrossingsInView;
        private IntersectionEditorData _intersectionData;
        private GUIStyle _centeredStyle;

        public delegate void IntersectionClicked(GenericIntersectionSettings clickedIntersection);
        public event IntersectionClicked onIntersectionClicked;
        private void TriggetIntersectionClickedEvent(GenericIntersectionSettings clickedIntersection)
        {
            SettingsWindow.SetSelectedIntersection(clickedIntersection);
            if (onIntersectionClicked != null)
            {
                onIntersectionClicked(clickedIntersection);
            }
        }


        internal IntersectionDrawer (IntersectionEditorData intersectionData):base(intersectionData)
        {
            _centeredStyle = new GUIStyle();
            _centeredStyle.alignment = TextAnchor.UpperRight;

            _centeredStyle.fontStyle = FontStyle.Bold;
            _intersectionData = intersectionData;
        }


        internal PriorityIntersectionSettings[] DrawPriorityIntersections(bool showLabel, Color color, Color stopWaypointsColor, Color exitWaypointsColor, Color labelColor)
        {
            _style.normal.textColor = color;
            _centeredStyle.normal.textColor = labelColor;
            var intersections = _intersectionData.GetPriorityIntersections();
            UpdateInViewProperty();
            int nr = 0;
            for (int i = 0; i < intersections.Length; i++)
            {
                if (intersections[i].inView)
                {
                    nr++;
                    Handles.color = color;
                    DrawIntersection(intersections[i], showLabel);
                    Handles.color = stopWaypointsColor;
                    DrawStopWaypoints(intersections[i].enterWaypoints, false);
                    Handles.color = exitWaypointsColor;
                    DrawExitWaypoints(intersections[i].exitWaypoints);
                }
            }
            if (_priorityIntersectionsInView == null || nr != _priorityIntersectionsInView.Length)
            {
                _priorityIntersectionsInView = UpdateSelectedIntersections(intersections);
            }
            return _priorityIntersectionsInView;
        }


        internal TrafficLightsIntersectionSettings[] DrawTrafficLightsIntersections(bool showLabel, Color color, Color stopWaypointsColor, Color exitWaypointsColor, Color labelColor)
        {
            _style.normal.textColor = color;
            _centeredStyle.normal.textColor = labelColor;
            var intersections = _intersectionData.GetTrafficLightsIntersections();
            UpdateInViewProperty();
            int nr = 0;
            for (int i = 0; i < intersections.Length; i++)
            {
                if (intersections[i].inView)
                {
                    nr++;
                    Handles.color = color;
                    DrawIntersection(intersections[i], showLabel);
                    Handles.color = stopWaypointsColor;
                    DrawStopWaypoints(intersections[i].stopWaypoints, false);
                    Handles.color = exitWaypointsColor;
                    DrawExitWaypoints(intersections[i].exitWaypoints);
                }
            }
            if (_trafficLightsIntersectionsInView == null || nr != _trafficLightsIntersectionsInView.Length)
            {
                _trafficLightsIntersectionsInView = UpdateSelectedIntersections(intersections);
            }
            return _trafficLightsIntersectionsInView;
        }


        internal PriorityCrossingSettings[] DrawPriorityCrossings(bool showLabel, Color color, Color stopWaypointsColor)
        {
            _style.normal.textColor = color;
            var intersections = _intersectionData.GetPriorityCrossings();
            UpdateInViewProperty();
            int nr = 0;
            for (int i = 0; i < intersections.Length; i++)
            {
                if (intersections[i].inView)
                {
                    nr++;
                    Handles.color = color;
                    DrawIntersection(intersections[i], showLabel);
                    Handles.color = stopWaypointsColor;
                    DrawStopWaypoints(intersections[i].enterWaypoints, false);
                }
            }
            if (_priorityCrossingsInView == null || nr != _priorityCrossingsInView.Length)
            {
                _priorityCrossingsInView = UpdateSelectedIntersections(intersections);
            }
            return _priorityCrossingsInView;
        }


        internal TrafficLightsCrossingSettings[] DrawTrafficLightsCrossings(bool showLabel, Color color, Color stopWaypointsColor)
        {
            _style.normal.textColor = color;
            var intersections = _intersectionData.GetTrafficLightsCrossings();
            UpdateInViewProperty();
            int nr = 0;
            for (int i = 0; i < intersections.Length; i++)
            {
                if (intersections[i].inView)
                {
                    nr++;
                    Handles.color = color;
                    DrawIntersection(intersections[i], showLabel);
                    Handles.color = stopWaypointsColor;
                    DrawStopWaypoints(intersections[i].stopWaypoints, false);
                }
            }
            if (_trafficLightsCrossingsInView == null || nr != _trafficLightsCrossingsInView.Length)
            {
                _trafficLightsCrossingsInView = UpdateSelectedIntersections(intersections);
            }
            return _trafficLightsCrossingsInView;
        }


        internal void DrawExitWaypoints(GenericIntersectionSettings intersection, Color waypointColor)
        {
            DrawListWaypoints(intersection.GetExitWaypoints(), waypointColor, -1, default, _roadWaypointDimension);
        }


        internal void DrawStopWaypoints(GenericIntersectionSettings intersection, int road, Color waypointColor, Color labelColor)
        {

            if (road == int.MaxValue)
            {
                var waypoints = intersection.GetAssignedWaypoints();
                for (int i = 0; i < waypoints.Count; i++)
                {
                    DrawListWaypoints(waypoints[i].roadWaypoints, waypointColor, i + 1, labelColor, _roadWaypointDimension);
                }
            }
            else
            {
                DrawListWaypoints(intersection.GetStopWaypoints(road), waypointColor, road + 1, labelColor, _roadWaypointDimension);
            }
        }


        internal void DrawPedestrianWaypoints(GenericIntersectionSettings intersection, int road, Color waypointColor)
        {
#if GLEY_PEDESTRIAN_SYSTEM
            if (road == int.MaxValue)
            {
                DrawListWaypoints(intersection.GetPedestrianWaypoints(), waypointColor, -1, default, _pedestrianWaypointDimension);
            }
            else
            {
                DrawListWaypoints(intersection.GetPedestrianWaypoints(road), waypointColor, -1, default, _pedestrianWaypointDimension);
            }
#endif
        }


        internal void DrawDirectionWaypoints(GenericIntersectionSettings intersection, Color waypointColor)
        {
#if GLEY_PEDESTRIAN_SYSTEM
            DrawListWaypoints(intersection.GetDirectionWaypoints(), waypointColor, -1, default, _pedestrianWaypointDimension);
#endif
        }


        private void DrawListWaypoints<T>(List<T> waypointList, Color waypointColor, int road, Color textColor, float size) where T : WaypointSettingsBase
        {
            for (int i = 0; i < waypointList.Count; i++)
            {
                if (waypointList[i].draw)
                {
                    DrawIntersectionWaypoint(waypointList[i], waypointColor, road, textColor, size);
                }
            }
        }


        private void DrawIntersectionWaypoint(WaypointSettingsBase waypoint, Color waypointColor, int road, Color labelColor, float size)
        {
            if (waypoint != null)
            {
                Handles.color = waypointColor;
                Handles.DrawSolidDisc(waypoint.transform.position, Vector3.up, size);
                if (road != -1)
                {
                    _centeredStyle.normal.textColor = labelColor;
                    Handles.Label(waypoint.transform.position, road.ToString(), _centeredStyle);
                }
            }
        }


        private T[] UpdateSelectedIntersections<T>(T[] allIntersections) where T : GenericIntersectionSettings
        {
            var selectedIntersections = new List<T>();
            for (int i = 0; i < allIntersections.Length; i++)
            {
                if (allIntersections[i].inView)
                {
                    selectedIntersections.Add(allIntersections[i]);
                }
            }
            return selectedIntersections.ToArray();
        }


        private void UpdateInViewProperty()
        {
            GleyUtilities.SetCamera();
            if (_cameraMoved)
            {
                _cameraMoved = false;
                var allIntersections = _intersectionData.GetAllIntersections();
                for (int i = 0; i < allIntersections.Length; i++)
                {
                    if (GleyUtilities.IsPointInView(allIntersections[i].position))
                    {
                        allIntersections[i].inView = true;
                    }
                    else
                    {
                        allIntersections[i].inView = false;
                    }
                }
            }
        }


        private void DrawIntersection(GenericIntersectionSettings intersection, bool showLabel)
        {
            if (Handles.Button(intersection.position, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), 1f, 1f, Handles.DotHandleCap))
            {
                TriggetIntersectionClickedEvent(intersection);
            }
            if (showLabel)
            {
                Handles.Label(intersection.transform.position, "\n" + intersection.name, _style);
            }
        }


        private void DrawStopWaypoints(List<IntersectionStopWaypointsSettings> stopWaypoints, bool showLabel)
        {
            for (int i = 0; i < stopWaypoints.Count; i++)
            {
                for (int j = 0; j < stopWaypoints[i].roadWaypoints.Count; j++)
                {
                    Handles.DrawSolidDisc(stopWaypoints[i].roadWaypoints[j].transform.position, Vector3.up, _roadWaypointDimension);
                    if (showLabel)
                    {
                        Handles.Label(stopWaypoints[i].roadWaypoints[j].transform.position, (j + 1).ToString(), _centeredStyle);
                    }
                }
            }
        }


        private void DrawExitWaypoints(List<WaypointSettings> exitWaypoints)
        {
            for (int i = 0; i < exitWaypoints.Count; i++)
            {
                Handles.DrawSolidDisc(exitWaypoints[i].transform.position, Vector3.up, _roadWaypointDimension);
            }
        }
    }
}
