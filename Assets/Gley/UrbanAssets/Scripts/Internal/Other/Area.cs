using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    /// <summary>
    /// Store a circular area.
    /// </summary>
    [System.Serializable]
    public class Area
    {
        public Vector3 Center;
        public float Radius;
        [HideInInspector]
        public float SqrRadius;

        public Area(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
            SqrRadius = radius * radius;
        }

        public Area(Area area)
        {
            Center = area.Center;
            Radius = area.Radius;
            SqrRadius = Radius * Radius;
        }
    }
}
