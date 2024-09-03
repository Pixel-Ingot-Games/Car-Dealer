using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Keeps track of time.
    /// </summary>
    public class TimeManager
    {
        private float _realtimeSinceStartup;

        public float RealTimeSinceStartup => _realtimeSinceStartup;


        public void UpdateTime()
        {
            _realtimeSinceStartup += Time.deltaTime;
        }
    }
}
