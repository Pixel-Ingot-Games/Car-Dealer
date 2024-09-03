using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    /// <summary>
    /// Stores path finding waypoints.
    /// </summary>
    public class PathFindingData : MonoBehaviour
    {
        [SerializeField] private PathFindingWaypoint[] _allPathFindingWaypoints;


        internal PathFindingWaypoint[] AllPathFindingWaypoints => _allPathFindingWaypoints;


        public void SetPathFindingWaypoints(PathFindingWaypoint[] waypoints)
        {
            _allPathFindingWaypoints = waypoints;
        }


        internal bool IsValid(out string error)
        {
            error = string.Empty;
            if (_allPathFindingWaypoints == null)
            {
                error= UrbanSystemErrors.NullPathFindingData;
                return false;
            }

            if (_allPathFindingWaypoints.Length <= 0)
            {
                error = UrbanSystemErrors.NoPathFindingWaypoints;
                return false;
            }
            return true;
        }
    }
}