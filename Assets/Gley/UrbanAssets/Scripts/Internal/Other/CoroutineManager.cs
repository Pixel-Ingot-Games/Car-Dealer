using UnityEngine;
using System.Collections;

namespace Gley.UrbanSystem.Internal
{
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager instance;

        private void Awake()
        {
            // Ensure there's only one instance of CoroutineManager
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void StartStaticCoroutine(IEnumerator coroutine)
        {
            if (instance != null)
            {
                instance.StartCoroutine(coroutine);
            }
            else
            {
                Debug.LogError("CoroutineManager instance is not initialized.");
            }
        }
    }
}