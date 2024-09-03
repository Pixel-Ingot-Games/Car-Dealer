using Gley.UrbanSystem.Internal;
using System.Collections.Generic;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Updates all intersections
    /// </summary>
    internal class IntersectionManager : IDestroyable
    {
        private List<GenericIntersection> _activeIntersections;


        internal IntersectionManager()
        {
            Assign();
#if GLEY_PEDESTRIAN_SYSTEM
            PedestrianSystem.Events.OnPedestrianRemoved += PedestrianRemoved;
#endif

            IntersectionEvents.onActiveIntersectionsChanged += SetActiveIntersection;
            _activeIntersections = new List<GenericIntersection>();
            SetActiveIntersection(_activeIntersections);
        }


        public void Assign()
        {
            DestroyableManager.Instance.Register(this);
        }


        /// <summary>
        /// Initialize all active intersections
        /// </summary>
        /// <param name="activeIntersections"></param>
        internal void SetActiveIntersection(List<GenericIntersection> activeIntersections)
        {
            for (int i = 0; i < activeIntersections.Count; i++)
            {
                if (_activeIntersections != null)
                {
                    if (!_activeIntersections.Contains(activeIntersections[i]))
                    {
                        activeIntersections[i].ResetIntersection();
                    }
                }
            }
           _activeIntersections = activeIntersections;
        }


        internal void RemoveVehicle(int index)
        {
            for (int i = 0; i < _activeIntersections.Count; i++)
            {
                _activeIntersections[i].RemoveVehicle(index);
            }
        }


        /// <summary>
        /// Called on every frame to update active intersection road status
        /// </summary>
        internal void UpdateIntersections(float realTimeSinceStartup)
        {
            for (int i = 0; i < _activeIntersections.Count; i++)
            {
                _activeIntersections[i].UpdateIntersection(realTimeSinceStartup);
            }
        }


        private void PedestrianRemoved(int pedestrianIndex)
        {
            for (int i = 0; i < _activeIntersections.Count; i++)
            {
                _activeIntersections[i].PedestrianPassed(pedestrianIndex);
            }
        }


        public void OnDestroy()
        {
#if GLEY_PEDESTRIAN_SYSTEM
            PedestrianSystem.Events.OnPedestrianRemoved -= PedestrianRemoved;
#endif
            IntersectionEvents.onActiveIntersectionsChanged -= SetActiveIntersection;
        }
    }
}
