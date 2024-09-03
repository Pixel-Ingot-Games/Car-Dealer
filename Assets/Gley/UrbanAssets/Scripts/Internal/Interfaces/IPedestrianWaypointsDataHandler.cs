namespace Gley.UrbanSystem.Internal
{
    public interface IPedestrianWaypointsDataHandler
    {
        void SetIntersection(int[] pedestrianWaypointIndexes, IIntersection intersection);
    }
}