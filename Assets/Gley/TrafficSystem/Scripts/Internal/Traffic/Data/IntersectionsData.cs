using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores all intersection properties.
    /// </summary>
    public class IntersectionsData : MonoBehaviour
    {
        [SerializeField] IntersectionDataType[] _allIntersections;
        [SerializeField] PriorityIntersectionData[] _allPriorityIntersections;
        [SerializeField] TrafficLightsIntersectionData[] _allLightsIntersections;
        [SerializeField] TrafficLightsCrossingData[] _allLightsCrossings;
        [SerializeField] PriorityCrossingData[] _allPriorityCrossings;

        internal IntersectionDataType[] AllIntersections => _allIntersections;
        internal PriorityIntersectionData[] AllPriorityIntersections => _allPriorityIntersections;
        internal TrafficLightsIntersectionData[] AllLightsIntersections => _allLightsIntersections;
        internal TrafficLightsCrossingData[] AllLightsCrossings => _allLightsCrossings;
        internal PriorityCrossingData[] AllPriorityCrossings => _allPriorityCrossings;


        public void SetTrafficIntersectionData(IntersectionDataType[] allIntersections, TrafficLightsIntersectionData[] allLightsIntersections, PriorityIntersectionData[] allPriorityIntersections, TrafficLightsCrossingData[] allLightsCrossings, PriorityCrossingData[] allPriorityCrossings)
        {
            _allIntersections = allIntersections;
            _allLightsCrossings = allLightsCrossings;
            _allLightsIntersections = allLightsIntersections;
            _allPriorityIntersections = allPriorityIntersections;
            _allPriorityCrossings = allPriorityCrossings;

        }


        internal bool IsValid(out string error)
        {
            error = string.Empty;
            if (_allIntersections == null || _allLightsCrossings == null || _allLightsIntersections == null || _allPriorityCrossings == null || _allPriorityIntersections == null)
            {
                error = TrafficSystemErrors.NullIntersectionData;
                return false;
            }
            return true;
        }
    }
}
