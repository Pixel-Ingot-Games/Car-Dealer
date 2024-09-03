using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Access intersection data.
    /// </summary>
    internal class AllIntersectionsDataHandler
    {
        private readonly AllIntersectionsData _allIntersectionsData;


        internal AllIntersectionsDataHandler(AllIntersectionsData data)
        {
            _allIntersectionsData = data;
        }


        internal void SetTrafficLightsBehaviour(TrafficLightsBehaviour trafficLightsBehaviour)
        {
            for (int i = 0; i < _allIntersectionsData.TrafficLightsIntersections.Length; i++)
            {
                GetTrafficLightsIntersection(i).SetTrafficLightsBehaviour(trafficLightsBehaviour);
            }

            for (int i = 0; i < _allIntersectionsData.TrafficLightsCrossings.Length; i++)
            {
                GetTrafficLightsCrossing(i).SetTrafficLightsBehaviour(trafficLightsBehaviour);
            }
        }


        internal void SetRoadToGreen(string intersectionName, int roadIndex, bool doNotChangeAgain)
        {
            for (int i = 0; i < _allIntersectionsData.TrafficLightsIntersections.Length; i++)
            {
                if (GetTrafficLightsIntersection(i).GetName() == intersectionName)
                {
                    GetTrafficLightsIntersection(i).SetGreenRoad(roadIndex, doNotChangeAgain);
                    return;
                }
            }
            Debug.LogWarning($"{intersectionName} not found. Make sure it is a Traffic Lights Intersection.");
        }


        internal TrafficLightsColor GetTrafficLightsCrossingState(string crossingName)
        {
            for (int i = 0; i < _allIntersectionsData.TrafficLightsCrossings.Length; i++)
            {
                if (GetTrafficLightsCrossing(i).GetName() == crossingName)
                {
                    return GetTrafficLightsCrossing(i).GetCrossingState();
                }
            }
            Debug.LogWarning($"{crossingName} not found");
            return TrafficLightsColor.Red;
        }


        internal bool IsPriorityCrossingRed(string crossingName)
        {
            for (int i = 0; i < _allIntersectionsData.PriorityCrossings.Length; i++)
            {
                if (GetPriorityCrossing(i).GetName() == crossingName)
                {
                    return GetPriorityCrossing(i).GetPriorityCrossingState();
                }
            }
            Debug.LogWarning($"{crossingName} not found");
            return false;
        }


        internal void SetPriorityCrossingState(string crossingName, bool stop, bool stopUpdate)
        {
            for (int i = 0; i < _allIntersectionsData.PriorityCrossings.Length; i++)
            {
                if (GetPriorityCrossing(i).GetName() == crossingName)
                {
                    GetPriorityCrossing(i).SetPriorityCrossingState(stop, stopUpdate);
                }
            }
        }


        internal GenericIntersection[] GetAllIntersections()
        {
            return _allIntersectionsData.AllIntersections;
        }


        internal List<GenericIntersection> GetIntersections(List<int> intersectionIndexes)
        {
            List<GenericIntersection> result = new List<GenericIntersection>();
            for (int i = 0; i < intersectionIndexes.Count; i++)
            {
                result.Add(GetIntersection(intersectionIndexes[i]));
            }
            return result;
        }


        private GenericIntersection GetIntersection(int intersectionIndex)
        {
            return _allIntersectionsData.AllIntersections[intersectionIndex];
        }


        private TrafficLightsIntersection GetTrafficLightsIntersection(int intersectionIndex)
        {
            return _allIntersectionsData.TrafficLightsIntersections[intersectionIndex];
        }


        private TrafficLightsCrossing GetTrafficLightsCrossing(int intersectionIndex)
        {
            return _allIntersectionsData.TrafficLightsCrossings[intersectionIndex];
        }


        private PriorityCrossing GetPriorityCrossing(int intersectionIndex)
        {
            return _allIntersectionsData.PriorityCrossings[intersectionIndex];
        }
    }
}
