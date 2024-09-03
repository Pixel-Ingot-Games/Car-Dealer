using System.Collections.Generic;
using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    [System.Serializable]
    public class CellWaypointsData
    {
        [SerializeField] List<int> _waypoints;
        [SerializeField] List<SpawnWaypoint> _spawnWaypoints;
        [SerializeField] bool _hasWaypoints;
        [SerializeField] bool _hasSpawnWaypoints;

        public List<int> Waypoints => _waypoints;
        public List<SpawnWaypoint> SpawnWaypoints => _spawnWaypoints;
        public bool HasWaypoints
        {
            get
            {
                return _hasWaypoints;
            }
            set
            {
                _hasWaypoints = value;
            }
        }
        public bool HasSpawnWaypoints
        {
            get
            {
                return _hasSpawnWaypoints;
            }
            set
            {
                _hasSpawnWaypoints = value;
            }
        }


        public CellWaypointsData(List<int> waypoints, List<SpawnWaypoint> spawnWaypoints, bool hasWaypoints, bool hasSpawnWaypoints)
        {
            _waypoints = waypoints;
            _spawnWaypoints = spawnWaypoints;
            _hasWaypoints = hasWaypoints;
            _hasSpawnWaypoints = hasSpawnWaypoints;
        }
    }
}