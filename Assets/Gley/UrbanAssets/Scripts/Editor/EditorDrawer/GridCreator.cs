using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class GridCreator
    {
        private readonly GridEditorData _gridData;

        internal GridCreator(GridEditorData gridData)
        {
            _gridData = gridData;
        }


        internal void GenerateGrid(int gridCellSize)
        {
            System.DateTime startTime = System.DateTime.Now;
            int nrOfColumns;
            int nrOfRows;
            Bounds b = new Bounds();
            foreach (Renderer r in MonoBehaviourUtilities.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                b.Encapsulate(r.bounds);
            }
            foreach (Terrain t in MonoBehaviourUtilities.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                b.Encapsulate(t.terrainData.bounds);
            }

            nrOfColumns = Mathf.CeilToInt(b.size.x / gridCellSize);
            nrOfRows = Mathf.CeilToInt(b.size.z / gridCellSize);
            if (nrOfRows == 0 || nrOfColumns == 0)
            {
                Debug.LogError(UrbanSystemErrors.EmptyScene);
                return;
            }
            Vector3 corner = new Vector3(b.center.x - b.size.x / 2 + gridCellSize / 2, 0, b.center.z - b.size.z / 2 + gridCellSize / 2);
            int nr = 0;

            RowData[] grid = new RowData[nrOfRows];
            for (int row = 0; row < nrOfRows; row++)
            {
                grid[row] = new RowData(new CellData[nrOfColumns]);
                for (int column = 0; column < nrOfColumns; column++)
                {
                    nr++;
                    grid[row].Row[column] = new CellData(new CellProperties(column, row, new Vector3(corner.x + column * gridCellSize, 0, corner.z + row * gridCellSize)),
                        new CellWaypointsData(new List<int>(), new List<SpawnWaypoint>(), false, false),
                        new CellWaypointsData(new List<int>(), new List<SpawnWaypoint>(), false, false),
                        new List<int>());
                }
            }
            var gridCorner = grid[0].Row[0].CellProperties.Center - new Vector3(gridCellSize / 2, 0, gridCellSize / 2);
            if (MonoBehaviourUtilities.TryGetSceneScript<GridData>(out var gridData))
            {
                gridData.Value.SetGridData(grid, gridCorner, gridCellSize);
                EditorUtility.SetDirty(gridData.Value);
                _gridData.TriggerOnModifiedEvent();
            }
            else
            {
                Debug.LogError(gridData.Error);
            }
            Debug.Log("Done generate grid in " + (System.DateTime.Now - startTime));
        }
    }
}
