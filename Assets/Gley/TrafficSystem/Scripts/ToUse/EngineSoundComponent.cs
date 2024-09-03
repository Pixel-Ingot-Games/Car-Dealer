using UnityEngine;

namespace Gley.TrafficSystem
{
    /// <summary>
    /// Add this component on the Vehicle prefab if you need engine sound for your vehicle
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EngineSoundComponent : MonoBehaviour
    {
        [Tooltip("Pitch used when vehicle is stationary")]
        public float minPitch = 0.6f;
        [Tooltip("Pitch used when vehicle is at max speed")]
        public float maxPitch = 1;
        [Tooltip("Volume used when vehicle is stationary")]
        public float minVolume = 0.5f;
        [Tooltip("Volume used when vehicle is at max speed")]
        public float maxVolume = 1;

        private AudioSource _audioSource;


        /// <summary>
        /// Initialize the sound component if required
        /// </summary>
        public void Initialize()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;      
        }


        /// <summary>
        /// Play sound is vehicle is enabled
        /// </summary>
        /// <param name="masterVolume"></param>
        public void Play(float masterVolume)
        {
            _audioSource.volume = masterVolume;
            _audioSource.Play();
        }


        /// <summary>
        /// Stop volume when vehicle is disabled
        /// </summary>
        public void Stop()
        {
            _audioSource.Stop();
        }


        /// <summary>
        /// Update engine sound based on speed
        /// </summary>
        /// <param name="velocity">current vehicle speed</param>
        /// <param name="maxVelocity">max vehicle speed</param>
        /// <param name="masterVolume">master volume</param>
        public void UpdateEngineSound(float velocity, float maxVelocity, float masterVolume)
        {
            float percent = velocity / maxVelocity;
            _audioSource.volume = (minVolume + (maxVolume - minVolume) * percent) * masterVolume;

            float pitch = minPitch + (maxPitch - minPitch) * percent;
            _audioSource.pitch = pitch;
        }
    }
}