using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    /// <summary>
    /// Checks if a vehicle is viewed by the camera
    /// </summary>
    public class VisibilityScript : MonoBehaviour
    {
        private bool _readyToRemove;
        private bool _neverBeenVisible;


        /// <summary>
        /// Reset component
        /// </summary>
        public void Reset()
        {
            _neverBeenVisible = true;
            _readyToRemove = false;
        }


        /// <summary>
        /// Unity method automatically triggered
        /// </summary>
        private void OnBecameVisible()
        {
            _neverBeenVisible = false;
            _readyToRemove = false;
        }


        /// <summary>
        /// Unity method automatically triggered
        /// </summary>
        private void OnBecameInvisible()
        {
            _readyToRemove = true;
        }


        /// <summary>
        /// Check if a vehicle is visible
        /// </summary>
        /// <returns>true it is not in view</returns>
        public bool IsNotInView()
        {
            if (_neverBeenVisible == true)
            {
                return true;
            }
            return _readyToRemove;
        }
    }
}
