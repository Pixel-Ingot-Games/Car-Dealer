using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class IntersectionCreator
    {
        const string intersectionPrefix = "Intersection_";
        private IntersectionEditorData _intersectionData;

        internal IntersectionCreator(IntersectionEditorData intersectionData)
        {
            _intersectionData = intersectionData;
        }


        internal T Create<T>() where T : GenericIntersectionSettings
        {
            GameObject intersection = CreateIntersectionObject();
            return (T)intersection.AddComponent<T>().Initialize();
        }


        internal void DeleteIntersection(GenericIntersectionSettings intersection)
        {
            GleyPrefabUtilities.DestroyImmediate(intersection.gameObject);
            _intersectionData.TriggerOnModifiedEvent();
        }


        private GameObject CreateIntersectionObject()
        {
            Transform intersectionParent = MonoBehaviourUtilities.GetOrCreateGameObject(TrafficSystemConstants.EditorIntersectionsHolder, true).transform;
            Vector3 poz = SceneView.lastActiveSceneView.camera.transform.position;
            poz.y = 0;
            return MonoBehaviourUtilities.CreateGameObject(intersectionPrefix + GleyUtilities.GetFreeRoadNumber(intersectionParent), intersectionParent, poz, true);
        }
    }
}