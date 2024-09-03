using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    public class GridDataHandler
    {
        private readonly GridData _gridData;


        public GridDataHandler(GridData data)
        {
            _gridData = data;
        }


        public int GetCellSize()
        {
            return _gridData.GridCellSize;
        }


        private RowData[] GetGrid()
        {
            return _gridData.Grid;
        }


        /// <summary>
        /// Get all specified neighbors for the specified depth
        /// </summary>
        /// <param name="row">current row</param>
        /// <param name="column">current column</param>
        /// <param name="depth">how far the cells should be</param>
        /// <param name="justEdgeCells">ignore middle cells</param>
        /// <returns>Returns the neighbors of the given cells</returns>
        internal List<Vector2Int> GetCellNeighbors(int row, int column, int depth, bool justEdgeCells)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            int rowMinimum = row - depth;
            if (rowMinimum < 0)
            {
                rowMinimum = 0;
            }

            int rowMaximum = row + depth;
            if (rowMaximum >= GetGridLength())
            {
                rowMaximum = GetGridLength() - 1;
            }


            int columnMinimum = column - depth;
            if (columnMinimum < 0)
            {
                columnMinimum = 0;
            }

            int columnMaximum = column + depth;
            if (columnMaximum >= GetRowLength(row))
            {
                columnMaximum = GetRowLength(row) - 1;
            }

            for (int i = rowMinimum; i <= rowMaximum; i++)
            {
                for (int j = columnMinimum; j <= columnMaximum; j++)
                {
                    if (justEdgeCells)
                    {
                        if (i == row + depth || i == row - depth || j == column + depth || j == column - depth)
                        {
                            result.Add(new Vector2Int(i, j));
                        }
                    }
                    else
                    {
                        result.Add(new Vector2Int(i, j));
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Convert position to Grid cell.
        /// </summary>
        public CellData GetCell(Vector3 position)
        {
            return GetCell(position.x, position.z);
        }


        /// <summary>
        /// Convert indexes to Grid cell.
        /// </summary>
        public CellData GetCell(float xPoz, float zPoz)
        {
            int rowIndex = Mathf.FloorToInt(Mathf.Abs((GetGridCorner().z - zPoz) / GetCellSize()));
            int columnIndex = Mathf.FloorToInt(Mathf.Abs((GetGridCorner().x - xPoz) / GetCellSize()));
            return GetGrid()[rowIndex].Row[columnIndex];
        }


        public Vector3 GetGridCorner()
        {
            return _gridData.GridCorner;
        }


        public CellData GetCell(Vector2Int cellIndex)
        {
            return GetGrid()[cellIndex.x].Row[cellIndex.y];
        }


        public int GetGridLength()
        {
            return GetGrid().Length;
        }


        public int GetRowLength(int row)
        {
            return GetGrid()[row].Row.Length;
        }


        public Vector2Int GetCellIndex(Vector3 position)
        {
            int rowIndex = Mathf.FloorToInt(Mathf.Abs((GetGridCorner().z - position.z) / GetCellSize()));
            int columnIndex = Mathf.FloorToInt(Mathf.Abs((GetGridCorner().x - position.x) / GetCellSize()));
            return new Vector2Int(GetGrid()[rowIndex].Row[columnIndex].CellProperties.Row, GetGrid()[rowIndex].Row[columnIndex].CellProperties.Column);
        }


        public List<SpawnWaypoint> GetPedestrianSpawnWaypointsForCell(Vector2Int cellIndex, int agentType)
        {
            return GetGrid()[cellIndex.x].Row[cellIndex.y].PedestrianWaypointsData.SpawnWaypoints.Where(cond1 => cond1.AllowedAgents.Contains(agentType)).ToList();
        }


        internal List<int> GetPedestrianWaypointsAroundPosition(Vector3 position)
        {
            List<int> possibleWaypoints = GetCell(position.x, position.z).PedestrianWaypointsData.Waypoints;
            if (possibleWaypoints.Count == 0)
            {
                var gridCell = GetCell(position.x, position.z);
                List<Vector2Int> cells = GetCellNeighbors(gridCell.CellProperties.Row, gridCell.CellProperties.Column, 1, true);
                foreach (Vector2Int cell in cells)
                {
                    possibleWaypoints.AddRange(GetCell(cell).PedestrianWaypointsData.Waypoints);
                }
            }
            return possibleWaypoints;
        }


        internal List<SpawnWaypoint> GetPedestrianSpawnWaypoipointsAroundPosition(Vector3 position, int agentType)
        {
            var possibleWaypoints = GetCell(position.x, position.z).PedestrianWaypointsData.SpawnWaypoints.Where(cond1 => cond1.AllowedAgents.Contains(agentType)).ToList();

            if(possibleWaypoints.Count==0)
            {
                var gridCell = GetCell(position.x, position.z);
                List<Vector2Int> cells = GetCellNeighbors(gridCell.CellProperties.Row, gridCell.CellProperties.Column, 1, true);
                foreach (Vector2Int cell in cells)
                {
                    possibleWaypoints.AddRange(GetCell(cell).PedestrianWaypointsData.SpawnWaypoints.Where(cond1 => cond1.AllowedAgents.Contains(agentType)).ToList());
                }
            }

            return possibleWaypoints;
        }

        public List<SpawnWaypoint> GetAllPedestrianSpawnWaypoints()
        {
            var result = new List<SpawnWaypoint>();
            for (int i = 0; i < GetGrid().Length; i++)
            {
                for (int j = 0; j < GetGrid()[i].Row.Length; j++)
                {
                    result.AddRange(GetGrid()[i].Row[j].PedestrianWaypointsData.SpawnWaypoints);
                }
            }
            return result;
        }

        public List<SpawnWaypoint> GetAllTrafficSpawnWaypoints()
        {
            var result = new List<SpawnWaypoint>();
            for (int i = 0; i < GetGrid().Length; i++)
            {
                for (int j = 0; j < GetGrid()[i].Row.Length; j++)
                {
                    result.AddRange(GetGrid()[i].Row[j].TrafficWaypointsData.SpawnWaypoints);
                }
            }
            return result;
        }

        public List<int> GetAllPedestrianPlayModeWaypoints()
        {
            var result = new List<int>();
            for (int i = 0; i < GetGrid().Length; i++)
            {
                for (int j = 0; j < GetGrid()[i].Row.Length; j++)
                {
                    result.AddRange(GetGrid()[i].Row[j].PedestrianWaypointsData.Waypoints);
                }
            }
            return result;
        }


        public List<int> GetAllTrafficPlayModeWaypoints()
        {
            var result = new List<int>();
            for (int i = 0; i < GetGrid().Length; i++)
            {
                for (int j = 0; j < GetGrid()[i].Row.Length; j++)
                {
                    result.AddRange(GetGrid()[i].Row[j].TrafficWaypointsData.Waypoints);
                }
            }
            return result;
        }


        public List<SpawnWaypoint> GetTrafficSpawnWaypointsForCell(Vector2Int cellIndex, int agentType)
        {
            List<SpawnWaypoint> spawnWaypoints = GetGrid()[cellIndex.x].Row[cellIndex.y].TrafficWaypointsData.SpawnWaypoints;

            return GetGrid()[cellIndex.x].Row[cellIndex.y].TrafficWaypointsData.SpawnWaypoints.Where(cond1 => cond1.AllowedAgents.Contains(agentType)).ToList();
        }

        internal List<SpawnWaypoint> GetTrafficSpawnWaypoipointsAroundPosition(Vector3 position, int agentType)
        {
            var possibleWaypoints = GetCell(position.x, position.z).TrafficWaypointsData.SpawnWaypoints.Where(cond1 => cond1.AllowedAgents.Contains(agentType)).ToList();

            if (possibleWaypoints.Count == 0)
            {
                var gridCell = GetCell(position.x, position.z);
                List<Vector2Int> cells = GetCellNeighbors(gridCell.CellProperties.Row, gridCell.CellProperties.Column, 1, true);
                foreach (Vector2Int cell in cells)
                {
                    possibleWaypoints.AddRange(GetCell(cell).TrafficWaypointsData.SpawnWaypoints.Where(cond1 => cond1.AllowedAgents.Contains(agentType)).ToList());
                }
            }

            return possibleWaypoints;
        }


        public List<int> GetAllWaypoints(Vector2Int cellIndex)
        {
            return GetGrid()[cellIndex.x].Row[cellIndex.y].TrafficWaypointsData.Waypoints;
        }


        public bool HasPedestrianSpawnWaypoints(Vector2Int cellIndex)
        {
            return GetGrid()[cellIndex.x].Row[cellIndex.y].PedestrianWaypointsData.HasSpawnWaypoints;
        }


        public bool HasTrafficSpawnWaypoints(Vector2Int cellIndex)
        {
            return GetGrid()[cellIndex.x].Row[cellIndex.y].TrafficWaypointsData.HasSpawnWaypoints;
        }


        internal CellData[] GetCells(List<Vector2Int> activeCells)
        {
            CellData[] cellDatas = new CellData[activeCells.Count];
            for (int i = 0; i < activeCells.Count; i++)
            {
                cellDatas[i] = GetCell(activeCells[i]);
            }
            return cellDatas;
        }


        internal Vector3 GetCellPosition(Vector2Int vector2Int)
        {
            return GetCell(vector2Int).CellProperties.Center;
        }


        internal List<int> GetPedestrianWaypointsInArea(Area area)
        {
            List<int> result = new List<int>();
            CellData cell = GetCell(area.Center);
            List<Vector2Int> neighbors = GetCellNeighbors(cell.CellProperties.Row, cell.CellProperties.Column, Mathf.CeilToInt(area.Radius * 2 / GetCellSize()), false);
            for (int i = neighbors.Count - 1; i >= 0; i--)
            {
                cell = GetCell(neighbors[i]);
                result.AddRange(cell.PedestrianWaypointsData.Waypoints);
            }
            return result;
        }


        internal List<int> GetTrafficWaypointsAroundPosition(Vector3 position)
        {
            List<int> possibleWaypoints = GetCell(position.x, position.z).TrafficWaypointsData.Waypoints;
            if (possibleWaypoints.Count == 0)
            {
                var gridCell = GetCell(position.x, position.z);
                List<Vector2Int> cells = GetCellNeighbors(gridCell.CellProperties.Row, gridCell.CellProperties.Column, 1, true);
                foreach (Vector2Int cell in cells)
                {
                    possibleWaypoints.AddRange(GetCell(cell).TrafficWaypointsData.Waypoints);
                }
            }
            return possibleWaypoints;
        }


        public void AddTrafficWaypoint(CellData cellData, int waypointIndex)
        {
            cellData.TrafficWaypointsData.Waypoints.Add(waypointIndex);
            cellData.TrafficWaypointsData.HasWaypoints = true;
        }


        public void AddTrafficSpawnWaypoint(CellData cellData, int waypointIndex, int[] allowedVehicles, int priority)
        {
            cellData.TrafficWaypointsData.SpawnWaypoints.Add(new SpawnWaypoint(waypointIndex, allowedVehicles, priority));
            cellData.TrafficWaypointsData.HasSpawnWaypoints = true;
        }


        public void AddPedestrianWaypoint(CellData cellData, int waypointIndex)
        {
            cellData.PedestrianWaypointsData.Waypoints.Add(waypointIndex);
            cellData.PedestrianWaypointsData.HasWaypoints = true;
        }


        public void AddPedestrianSpawnWaypoint(CellData cellData, int waypointIndex, int[] allowedPedestrians, int priority)
        {
            cellData.PedestrianWaypointsData.SpawnWaypoints.Add(new SpawnWaypoint(waypointIndex, allowedPedestrians, priority));
            cellData.PedestrianWaypointsData.HasSpawnWaypoints = true;
        }


        public void AddIntersection(CellData cellData, int intersectionIndex)
        {
            cellData.IntersectionsInCell.Add(intersectionIndex);
        }
    }
}