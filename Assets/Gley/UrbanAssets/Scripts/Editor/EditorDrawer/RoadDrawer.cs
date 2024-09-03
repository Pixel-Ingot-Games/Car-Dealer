using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class RoadDrawer<T,R> : Drawer where T : RoadEditorData<R> where R : RoadBase
    {
        private readonly T _roadData;
        private readonly float _segmentSelectDistanceThreshold = 1f;

        private List<R> _selectedRoads = new();


        protected RoadDrawer(EditorData roadData) : base(roadData)
        {
            _roadData = (T)roadData;
        }


        internal List<R> ShowAllRoads(MoveTools moveTool, Color roadColor, Color anchorColor, Color controlColor, Color textColor, bool showLabel)
        {
            var allRoads = _roadData.GetAllRoads();
            UpdateInViewProperty(allRoads);

            int nr = 0;
            for (int i = 0; i < allRoads.Length; i++)
            {
                if (allRoads[i].inView)
                {
                    nr++;
                    if (allRoads[i].draw)
                    {
                        if (!allRoads[i].skip)
                        {
                            DrawPath(allRoads[i], moveTool, roadColor, anchorColor, controlColor, textColor, showLabel);
                        }
                    }
                }
            }
            if (nr != _selectedRoads.Count)
            {
                UpdateSelectedRoads(allRoads);
            }
            return _selectedRoads;
        }


        internal void DrawPath(RoadBase road, MoveTools moveTool, Color roadColor, Color anchorColor, Color controlColor, Color textColor, bool showLabel)
        {
            Path path = road.path;
            // Draw path.
            for (int i = 0; i < path.NumSegments; i++)
            {
                Vector3[] points = path.GetPointsInSegment(i, road.positionOffset);
                if (moveTool != MoveTools.None)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1], points[0]);
                    Handles.DrawLine(points[2], points[3]);
                }
                Handles.DrawBezier(points[0], points[3], points[1], points[2], roadColor, null, 2);
            }
            if (showLabel)
            {
                _style.normal.textColor = textColor;
                Handles.Label(road.startPosition, road.gameObject.name, _style);
                if (!path.IsClosed)
                {
                    Handles.Label(road.endPosition, road.gameObject.name, _style);
                }
            }
            // Draw points.
            for (int i = 0; i < path.NumPoints; i++)
            {
                float handleSize;
                Color handleColor;
                if (i % 3 == 0)
                {
                    handleSize = Customizations.GetControlPointSize(SceneView.lastActiveSceneView.camera.transform.position, path.GetPoint(i, road.positionOffset));
                    handleColor = controlColor;
                }
                else
                {
                    handleSize = Customizations.GetAnchorPointSize(SceneView.lastActiveSceneView.camera.transform.position, path.GetPoint(i, road.positionOffset));
                    handleColor = anchorColor;
                }
                Handles.color = handleColor;
                Vector3 newPos = path[i];

                switch (moveTool)
                {
                    case MoveTools.None:
                        break;
                    case MoveTools.Move3D:
                        newPos = Handles.PositionHandle(path.GetPoint(i, road.positionOffset), Quaternion.identity);
                        Handles.SphereHandleCap(0, path.GetPoint(i, road.positionOffset), Quaternion.identity, handleSize, EventType.Repaint);
                        break;
                    case MoveTools.Move2D:
                        newPos = DrawHandle(path, i, road, handleSize);
                        newPos.y = path.GetPoint(i, road.positionOffset).y;
                        break;
                }
                if (path[i] != newPos)
                {
                    Undo.RecordObject(road, "Move point");
                    path.MovePoint(i, newPos - road.positionOffset);
                }
            }
        }


        internal void SetDrawProperty(bool drawAllRoads)
        {
            var allRoads = _roadData.GetAllRoads();
            for (int i = 0; i < allRoads.Length; i++)
            {
                allRoads[i].draw = drawAllRoads;
            }
        }


        internal void SelectSegmentIndex(R road, Vector3 mousePosition)
        {
            float minDstToSegment = _segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < road.path.NumSegments; i++)
            {
                Vector3[] points = road.path.GetPointsInSegment(i, road.positionOffset);
                float dst = HandleUtility.DistancePointBezier(mousePosition, points[0], points[3], points[1], points[2]);
                if (dst < minDstToSegment)
                {
                    minDstToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != road.selectedSegmentIndex)
            {
                road.selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }


        internal void AddPathPoint(Vector3 mousePosition, RoadBase road)
        {
            if (road.selectedSegmentIndex == -1)
            {
                Undo.RecordObject(road, "Add segment");
                road.path.AddSegment(mousePosition);
            }
            else
            {
                Undo.RecordObject(road, "Split segment");
                road.path.SplitSegment(mousePosition, road.selectedSegmentIndex);
            }
        }


        internal void Delete(RoadBase road, Vector3 mousePosition)
        {
            float minDstToAnchor = 1 * .5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < road.path.NumPoints; i += 3)
            {
                float dst = Vector2.Distance(mousePosition, road.path[i]);
                if (dst < minDstToAnchor)
                {
                    minDstToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(road, "Delete segment");
                road.path.DeleteSegment(closestAnchorIndex);
            }
        }


        protected virtual Vector3 DrawHandle(Path path, int i, RoadBase road, float handleSize)
        {
#if UNITY_2019 || UNITY_2020 || UNITY_2021
            return Handles.FreeMoveHandle(path.GetPoint(i, road.positionOffset), Quaternion.identity, handleSize, Vector2.zero, Handles.SphereHandleCap);
#else
            return Handles.FreeMoveHandle(path.GetPoint(i, road.positionOffset), handleSize, Vector2.zero, Handles.SphereHandleCap);
#endif
        }


        private void UpdateSelectedRoads(RoadBase[] allRoads)
        {
            _selectedRoads = new List<R>();
            for (int i = 0; i < allRoads.Length; i++)
            {
                if (allRoads[i].inView && !allRoads[i].skip)
                {
                    _selectedRoads.Add((R)allRoads[i]);
                }
            }
        }


        private void UpdateInViewProperty(RoadBase[] allRoads)
        {
            GleyUtilities.SetCamera();
            if (_cameraMoved)
            {
                _cameraMoved = false;
                for (int i = 0; i < allRoads.Length; i++)
                {
                    if (GleyUtilities.IsPointInView(allRoads[i].startPosition) || GleyUtilities.IsPointInView(allRoads[i].endPosition))
                    {
                        allRoads[i].inView = true;
                    }
                    else
                    {
                        allRoads[i].inView = false;
                    }
                }
            }
        }
    }
}
