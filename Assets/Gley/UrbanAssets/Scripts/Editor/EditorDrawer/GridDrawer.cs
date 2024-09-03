using Gley.UrbanSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class GridDrawer : Drawer
    {
        private readonly GridEditorData _gridData;
        private Vector3 _cellSize;

        internal GridDrawer(GridEditorData gridData) : base(gridData)
        {
            _gridData = gridData;
            DataModified();
            _gridData.OnModified += DataModified;
        }


        internal void DrawGrid(bool traffic)
        {
            var grid = _gridData.GetGrid();
            int columnLength = grid.Length;
            if (columnLength <= 0)
                return;
            int rowLength = grid[0].Row.Length;

            UpdateInViewPropertyForGrid(grid, columnLength, rowLength);

            bool green = false;
            Handles.color = Color.white;
            for (int i = 0; i < columnLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    if (grid[i].Row[j].TrafficWaypointsData.HasWaypoints || grid[i].Row[j].PedestrianWaypointsData.HasWaypoints)
                    {
                        if (green == false)
                        {
                            green = true;
                            Handles.color = Color.green;
                        }
                    }
                    else
                    {
                        if (green == true)
                        {
                            green = false;
                            Handles.color = Color.white;
                        }
                    }

                    if (grid[i].Row[j].InView)
                    {
                        Handles.DrawWireCube(grid[i].Row[j].CellProperties.Center, _cellSize);
                    }
                }
            }
        }


        private void DataModified()
        {
            _cellSize = new Vector3(_gridData.GetGridCellSize(), 0, _gridData.GetGridCellSize());
        }


        private void UpdateInViewPropertyForGrid(RowData[] grid, int columnLength, int rowLength)
        {
            GleyUtilities.SetCamera();
            if (_cameraMoved)
            {
                _cameraMoved = false;
                for (int i = 0; i < columnLength; i++)
                {
                    for (int j = 0; j < rowLength; j++)
                    {
                        if (GleyUtilities.IsPointInView(grid[i].Row[j].CellProperties.Center))
                        {
                            grid[i].Row[j].InView = true;
                        }
                        else
                        {
                            grid[i].Row[j].InView = false;
                        }
                    }
                }
            }
        }


        internal override void OnDestroy()
        {
            base.OnDestroy();
            _gridData.OnModified -= DataModified;
        }
    }
}