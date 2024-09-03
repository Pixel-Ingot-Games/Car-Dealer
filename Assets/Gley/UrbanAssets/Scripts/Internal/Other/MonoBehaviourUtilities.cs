using System.Collections;
using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    public class MonoBehaviourUtilities : MonoBehaviour
    {
        private static GameObject _parent;


        public static T GetOrCreateSceneInstance<T>(string path, bool addEditorTag) where T : MonoBehaviour
        {
            T[] allScripts = FindObjectsByType<T>();
            if (allScripts.Length > 1)
            {
                Debug.LogError($"Multiple components of type {typeof(T)} exists in scene. Just one is required, delete extra components before continuing.");
                for (int i = 0; i < allScripts.Length; i++)
                {
                    Debug.LogWarning($"{typeof(T)} component exists on: {allScripts[i].name}", allScripts[i]);
                }
            }

            if (allScripts.Length == 0)
            {
                T component = GetOrCreateGameObject(path, addEditorTag).AddComponent<T>();
                return component;
            }
            return allScripts[0];
        }


        public static T GetOrCreateObjectScript<T>(string holderName, bool addEditorTag) where T : MonoBehaviour
        {
            return GetOrCreateObjectScript<T>(GetOrCreateGameObject(holderName, addEditorTag));
        }


        public static T GetOrCreateObjectScript<T>(GameObject holder) where T : MonoBehaviour
        {
            if(TryGetObjectScript<T>(holder,out var result))
            {
                return result.Value;
            }
            else
            {
                return holder.AddComponent<T>();
            }
        }

       
        public static GameObject GetOrCreateGameObject(string path, bool addEditorTag)
        {
            if (_parent == null)
            {
                _parent = GameObject.Find(UrbanSystemConstants.PARENT);
                if (_parent == null)
                {
                    _parent = new GameObject(UrbanSystemConstants.PARENT);
                    Debug.Log(_parent.name + " was generated");
                }
            }

            var pathObjects = path.Split("/");
            Transform parent = _parent.transform;
            for (int i = 0; i < pathObjects.Length; i++)
            {
                parent = FindOrCreate(parent, pathObjects[i], addEditorTag);
            }

            return parent.gameObject;
        }


        public static GameObject CreateGameObject(string name, Transform parent, Vector3 position, bool addEditorTag)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            if (addEditorTag)
            {
                go.tag = UrbanSystemConstants.EDITOR_TAG;
            }
            return go;
        }


        public static bool TryGetSceneScript<T>(out ExecutionResult<T> result) where T : MonoBehaviour
        {
            T[] allScripts = FindObjectsOfType<T>();
            string error = null;
            T value = default(T);

            if (allScripts.Length == 0)
            {
                error = UrbanSystemErrors.TypeNotFound<T>();
                result = new ExecutionResult<T>(value, error);
                return false;
            }

            if (allScripts.Length > 1)
            {
                error = UrbanSystemErrors.MultipleComponentsOfTypeFound<T>();
                foreach (var script in allScripts)
                {
                    Debug.LogWarning(UrbanSystemErrors.ComponentAlreadyExistsOn<T>(script.name), script);
                }
                result = new ExecutionResult<T>(value, error);
                return false;
            }

            value = allScripts[0];
            result = new ExecutionResult<T>(value, error);
            return true;
        }


        public static bool TryGetObjectScript<T>(string holderName, out ExecutionResult<T> result) where T : MonoBehaviour
        {
            if (TryGetObject(holderName, out var go))
            {
                return TryGetObjectScript(go.Value, out result);
            }
            else
            {
                result = new ExecutionResult<T>(default, go.Error);
                return false;
            }
        }


        public static bool TryGetObjectScript<T>(GameObject holder, out ExecutionResult<T> result) where T : MonoBehaviour
        {
            T script = holder.GetComponent<T>();

            if (script == null)
            {
                result = new ExecutionResult<T>(default, UrbanSystemErrors.TypeNotFound<T>());
                return false;
            }

            result = new ExecutionResult<T>(script, null);
            return true;
        }


        public static bool TryGetObject(string path, out ExecutionResult<GameObject> result)
        {
            var obj = GameObject.Find(path);
            if (obj == null)
            {
                result = new ExecutionResult<GameObject>(null, UrbanSystemErrors.ObjectNotFound(path));
                return false;
            }

            result = new ExecutionResult<GameObject>(obj, null);
            return true;
        }


        public static T[] FindObjectsByType<T>() where T : MonoBehaviour
        {
            return FindObjectsByType<T>(FindObjectsSortMode.None);
        }

        private static Transform FindOrCreate(Transform parent, string gameObjectName, bool editor)
        {
            var go = parent.transform.Find(gameObjectName);
            if (go == null)
            {
                go = CreateGameObject(gameObjectName, parent, parent.position, editor).transform;
                Debug.Log(go.name + " was generated");
            }
            return go;
        }


    }
}
