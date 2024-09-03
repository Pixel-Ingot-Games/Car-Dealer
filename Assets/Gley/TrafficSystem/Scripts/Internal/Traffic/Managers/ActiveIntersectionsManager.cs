using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Controls the update of active intersections.
    /// </summary>
    internal class ActiveIntersectionsManager : IDestroyable
    {
        private readonly AllIntersectionsDataHandler _allIntersectionsHandler;

        private List<GenericIntersection> _activeIntersections;


        internal ActiveIntersectionsManager(AllIntersectionsDataHandler trafficIntersectionsDataHandler)
        {
            _allIntersectionsHandler = trafficIntersectionsDataHandler;
            _activeIntersections = new List<GenericIntersection>();
            Assign();
            GridEvents.OnActiveGridCellsChanged += UpdateActiveIntersections;
        }


        public void Assign()
        {
            DestroyableManager.Instance.Register(this);
        }


        /// <summary>
        /// Create a list of active intersections
        /// </summary>
        private void UpdateActiveIntersections(CellData[] activeCells)
        {
            List<int> intersectionIndexes = new List<int>();
            for (int i = 0; i < activeCells.Length; i++)
            {
                intersectionIndexes.AddRange(activeCells[i].IntersectionsInCell.Except(intersectionIndexes));
            }

            List<GenericIntersection> result = _allIntersectionsHandler.GetIntersections(intersectionIndexes);

            if (_activeIntersections.Count == result.Count && _activeIntersections.All(result.Contains))
            {

            }
            else
            {
                _activeIntersections = result;
                IntersectionEvents.TriggetActiveIntersectionsChangedEvent(_activeIntersections);
            }
        }


        public void OnDestroy()
        {
            GridEvents.OnActiveGridCellsChanged -= UpdateActiveIntersections;
        }
    }
}
