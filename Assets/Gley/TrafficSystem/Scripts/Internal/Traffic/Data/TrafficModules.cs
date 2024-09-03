using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores the enabled optional traffic system modules.
    /// </summary>
    public class TrafficModules : MonoBehaviour
    {
        [SerializeField] private bool _pathFinding;

        public bool PathFinding => _pathFinding;

        public void SetModules(bool enablePathFinding)
        {
            _pathFinding = enablePathFinding;
        }
    }
}