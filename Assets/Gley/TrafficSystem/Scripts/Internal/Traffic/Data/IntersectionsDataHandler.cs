namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Access intersection properties
    /// </summary>
    public class IntersectionsDataHandler
    {
        private readonly IntersectionsData IntersectionsData;
        

        public IntersectionsDataHandler(IntersectionsData data)
        {
            IntersectionsData = data;
        }
      

        public IntersectionDataType[] GetIntersectionData()
        {
            return IntersectionsData.AllIntersections;
        }

       

        public TrafficLightsCrossingData GetTrafficLightsCrossingData(int intersectionIndex)
        {
            return IntersectionsData.AllLightsCrossings[intersectionIndex];
        }


        public TrafficLightsIntersectionData GetTrafficLightsIntersectionData(int intersectionIndex)
        {
            return IntersectionsData.AllLightsIntersections[intersectionIndex];
        }


        public PriorityCrossingData GetPriorityCrossingData(int intersectionIndex)
        {
            return IntersectionsData.AllPriorityCrossings[intersectionIndex];
        }


        public PriorityIntersectionData GetPriorityIntersectionData(int intersectionIndex)
        {
            return IntersectionsData.AllPriorityIntersections[intersectionIndex];
        }
    }
}