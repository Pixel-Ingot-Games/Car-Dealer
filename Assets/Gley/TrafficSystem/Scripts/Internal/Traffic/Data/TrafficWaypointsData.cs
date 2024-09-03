using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores all available waypoints for current scene.
    /// </summary>
    public class TrafficWaypointsData : MonoBehaviour
    {
        [SerializeField] private TrafficWaypoint[] _allTrafficWaypoints;

        internal TrafficWaypoint[] AllTrafficWaypoints
        {
            get
            {
                return _allTrafficWaypoints;
            }
        }


        public void SetTrafficWaypoints(TrafficWaypoint[] waypoints)
        {
            _allTrafficWaypoints = waypoints;
        }


        public void AssignZipperGiveWay()
        {
            for (int i = 0; i < _allTrafficWaypoints.Length; i++)
            {
                if (_allTrafficWaypoints[i].ZipperGiveWay)
                {
                    var prevs = _allTrafficWaypoints[i].Prev;
                    for (int j = 0; j < prevs.Length; j++)
                    {
                        _allTrafficWaypoints[prevs[j]].GiveWay = true;
                    }
                }
            }
        }


        internal bool IsValid(out string error)
        {
            error = string.Empty;
            if (_allTrafficWaypoints == null)
            {
                error = TrafficSystemErrors.NullWaypointData;
                return false;
            }

            if (_allTrafficWaypoints.Length <= 0)
            {
                error = TrafficSystemErrors.NoWaypointsFound;
                return false;
            }

            return true;
        }
    }
}