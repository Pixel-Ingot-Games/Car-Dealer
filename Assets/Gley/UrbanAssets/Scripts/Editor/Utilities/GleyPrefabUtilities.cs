using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    public class GleyPrefabUtilities : UnityEditor.Editor
    {
        private static string _prefabStage;


        internal static bool EditingInsidePrefab()
        {
            return (StageUtility.GetCurrentStageHandle() != StageUtility.GetMainStageHandle());
        }


        internal static bool PrefabChanged()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                if (_prefabStage != PrefabStageUtility.GetCurrentPrefabStage().assetPath)
                {
                    _prefabStage = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
                    return true;
                }
            }
            else
            {
                if (_prefabStage != "")
                {
                    _prefabStage = "";
                    return true;
                }
            }
            return false;
        }


        internal static GameObject GetScenePrefabRoot()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                return prefabStage.prefabContentsRoot;
            }
            return null;
        }


        internal static GameObject GetInstancePrefabRoot(GameObject go)
        {
            return PrefabUtility.GetOutermostPrefabInstanceRoot(go);
        }


        internal static bool IsInsidePrefab(GameObject go)
        {
            GameObject prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (prefab == null)
            {
                return false;
            }
            return true;
        }


        internal static void DeleteGameObjectFromPrefab(GameObject prefabRoot, string gameObjectName)
        {
            ApplyPrefab(prefabRoot, GetPrefabPath(prefabRoot));
            string path = GetPrefabPath(prefabRoot);
            GameObject prefab = LoadPrefab(path);
            DestroyTransform(prefab.transform.FindDeepChild(gameObjectName));
            SavePrefab(prefab, path);
        }


        internal static void ApplyPrefab(GameObject prefab, string path)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, path, InteractionMode.AutomatedAction);
        }


        internal static string GetPrefabPath(GameObject prefab)
        {
            GameObject parentObject = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            string path = AssetDatabase.GetAssetPath(parentObject);
            return path;
        }


        internal static void DestroyTransform(Transform transformToDestroy)
        {
            if (transformToDestroy != null)
            {
                if (IsInsidePrefab(transformToDestroy.gameObject))
                {
                    if (EditingInsidePrefab())
                    {
                        DeleteGameObjectFromPrefab(GetScenePrefabRoot(), transformToDestroy.name);
                    }
                    else
                    {
                        DeleteGameObjectFromPrefab(GetInstancePrefabRoot(transformToDestroy.gameObject), transformToDestroy.name);
                    }
                }
                else
                {
                    DestroyImmediate(transformToDestroy.gameObject);
                }
            }
        }


        internal static GameObject LoadPrefab(string path)
        {
            GameObject instantiatedObj = PrefabUtility.LoadPrefabContents(path); ;
            return instantiatedObj;
        }


        internal static void SavePrefab(GameObject prefab, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);
        }


        internal static void ClearAllChildObjects(Transform holder)
        {
            while (holder.childCount > 0)
            {
                if (IsInsidePrefab(holder.gameObject))
                {
                    if (EditingInsidePrefab())
                    {
                        DeleteGameObjectFromPrefab(GetScenePrefabRoot(), holder.GetChild(0).name);
                    }
                    else
                    {
                        DeleteGameObjectFromPrefab(GetInstancePrefabRoot(holder.gameObject), holder.GetChild(0).name);
                    }
                }
                else
                {
                    DestroyImmediate(holder.GetChild(0).gameObject);
                }
            }
        }


        internal static void ApplyPrefabInstance(GameObject roadHolder)
        {
            if (IsInsidePrefab(roadHolder))
            {
                if (!EditingInsidePrefab())
                {
                    GameObject prefabRoot = GetInstancePrefabRoot(roadHolder);
                    ApplyPrefab(prefabRoot, GetPrefabPath(PrefabUtility.GetOutermostPrefabInstanceRoot(prefabRoot)));
                }
            }
        }


        internal static void RevertToPrefab(Object componentToRevert)
        {
            PrefabUtility.RevertObjectOverride(componentToRevert, InteractionMode.AutomatedAction);
        }


        public static T[] GetAllComponents<T>() where T : MonoBehaviour
        {
            T[] _allWaypoints;

            if (!EditingInsidePrefab())
            {
                _allWaypoints = FindObjectsByType<T>(FindObjectsSortMode.None);
            }
            else
            {
                _allWaypoints = GetScenePrefabRoot().GetComponentsInChildren<T>();
            }
            return _allWaypoints;
        }
    }
}
