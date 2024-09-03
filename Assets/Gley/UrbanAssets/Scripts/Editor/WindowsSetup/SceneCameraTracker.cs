using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class SceneCameraTracker
    {
        private Vector3 _oldPivot;
        private double _time;
        private float _oldCameraDistance;
        private bool _moved;


        internal delegate void SceneCameraMoved();
        internal static event SceneCameraMoved OnSceneCameraMoved;
        internal static void TriggerSceneCameraMovedEvent()
        {
            OnSceneCameraMoved?.Invoke();
        }


        internal void MoveCheck()
        {
            if (_moved)
            {
                if (EditorApplication.timeSinceStartup - _time > 0.2)
                {
                    TriggerSceneCameraMovedEvent();
                    _moved = false;
                }
            }

            if (SceneView.lastActiveSceneView != null)
            {
                if (_oldPivot != SceneView.lastActiveSceneView.pivot || _oldCameraDistance != SceneView.lastActiveSceneView.cameraDistance)
                {
                    _oldPivot = SceneView.lastActiveSceneView.pivot;
                    _oldCameraDistance = SceneView.lastActiveSceneView.cameraDistance;
                    _time = EditorApplication.timeSinceStartup;
                    _moved = true;
                }
            }
        }
    }
}