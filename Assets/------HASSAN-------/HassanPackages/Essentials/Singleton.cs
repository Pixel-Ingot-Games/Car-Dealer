
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
            // Check if an instance already exists in the scene
            _instance = FindObjectOfType<T>();

            if (_instance != null) return _instance;
            // If not found, create a new GameObject with the script attached
            var singletonObject = new GameObject(typeof(T).Name);
            _instance = singletonObject.AddComponent<T>();

            // Ensure the GameObject isn't destroyed when loading new scenes
            DontDestroyOnLoad(singletonObject);

            return _instance;
        }
    }

}
