namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Each traffic participant outside Traffic System should implement this interface so the traffic cars could overtake it.
    /// </summary>
    public interface ITrafficParticipant
    {
        //returns the rb.velocity
        public float GetCurrentSpeedMS();
    }
}