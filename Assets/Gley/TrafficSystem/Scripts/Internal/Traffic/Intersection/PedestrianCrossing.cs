namespace Gley.TrafficSystem.Internal
{
    // Helping class for crossing the road.
    internal class PedestrianCrossing
    {
        internal int PedestrianIndex;
        internal int Road;
        internal bool Crossing;

        internal PedestrianCrossing(int pedestrianIndex, int road)
        {
            PedestrianIndex = pedestrianIndex;
            Road = road;
            Crossing = false;
        }
    }
}