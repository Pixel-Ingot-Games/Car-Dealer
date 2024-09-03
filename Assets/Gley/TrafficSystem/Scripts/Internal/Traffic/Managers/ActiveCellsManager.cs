#if GLEY_TRAFFIC_SYSTEM
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Activates the grid cell near the player.
    /// Active vehicles are used to spawn vehicles and only intersections from these cells will work.
    /// </summary>
    internal class ActiveCellsManager
    {
        private readonly GridDataHandler _gridDataHandler;

        private List<Vector2Int> _activeCells;
        private List<Vector2Int> _currentCells;


        internal ActiveCellsManager(NativeArray<float3> activeCameraPositions, GridDataHandler gridDataHandler, int level)
        {
            _gridDataHandler = gridDataHandler;
            _currentCells = new List<Vector2Int>();
            for (int i = 0; i < activeCameraPositions.Length; i++)
            {
                _currentCells.Add(new Vector2Int());
            }

            UpdateActiveCells(activeCameraPositions, level);
        }


        /// <summary>
        /// Update the active cells.
        /// </summary>
        internal void UpdateGrid(int level, NativeArray<float3> activeCameraPositions)
        {
            UpdateActiveCells(activeCameraPositions, level);
        }


        /// <summary>
        /// Update active cells based on player position
        /// </summary>
        /// <param name="activeCameraPositions">position to check</param>
        private void UpdateActiveCells(NativeArray<float3> activeCameraPositions, int level)
        {
            if (_currentCells.Count != activeCameraPositions.Length)
            {
                _currentCells = new List<Vector2Int>();
                for (int i = 0; i < activeCameraPositions.Length; i++)
                {
                    _currentCells.Add(new Vector2Int());
                }
            }

            bool changed = false;
            for (int i = 0; i < activeCameraPositions.Length; i++)
            {
                Vector2Int temp = _gridDataHandler.GetCellIndex(activeCameraPositions[i]);
                if (_currentCells[i] != temp)
                {
                    _currentCells[i] = temp;
                    changed = true;
                }
            }

            if (changed)
            {
                _activeCells = new List<Vector2Int>();
                for (int i = 0; i < activeCameraPositions.Length; i++)
                {
                    _activeCells.AddRange(_gridDataHandler.GetCellNeighbors(_currentCells[i].x, _currentCells[i].y, level, false));
                }
                GridEvents.TriggerActiveGridCellsChangedEvent(_gridDataHandler.GetCells(_activeCells));
            }
        }
    }
}
#endif