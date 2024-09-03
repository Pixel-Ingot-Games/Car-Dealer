namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Controls the sound volume.
    /// </summary>
    internal class SoundManager
    {
        private float _masterVolume;

        internal float MasterVolume => _masterVolume;


        internal SoundManager(float masterVolume)
        {
            _masterVolume = masterVolume;
        }


        /// <summary>
        /// Update engine volume of the vehicle
        /// </summary>
        /// <param name="volume"></param>
        internal void UpdateMasterVolume(float volume)
        {
            _masterVolume = volume;
        }
    }
}