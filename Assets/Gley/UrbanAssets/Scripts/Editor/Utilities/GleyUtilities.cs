using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class GleyUtilities
    {
        private static Camera _sceneCamera;

        internal static bool IsPointInViewWithValidation(Vector3 position)
        {
            if (!SetCamera())
            {
                return false;
            }
            return IsPointInView(position);
        }


        internal static bool IsPointInView(Vector3 position)
        {
            Vector3 screenPosition = _sceneCamera.WorldToViewportPoint(position);
            if (screenPosition.x > 1 || screenPosition.x < 0 || screenPosition.y > 1 || screenPosition.y < 0 || screenPosition.z < 0)
            {
                return false;
            }
            return true;
        }


        internal static bool SetCamera()
        {
            if (_sceneCamera == null)
            {
                if (SceneView.lastActiveSceneView == null)
                {
                    return false;
                }
                _sceneCamera = SceneView.lastActiveSceneView.camera;
            }
            return true;
        }


        internal static void TeleportSceneCamera(Vector3 cam_position, float height = 1)
        {
            var scene_view = SceneView.lastActiveSceneView;
            if (scene_view != null)
            {
                scene_view.Frame(new Bounds(cam_position, Vector3.one * height), false);
            }
        }


        internal static int GetFreeRoadNumber(Transform parent)
        {
            var numbers = new List<int>();
            for (int i = 0; i < parent.childCount; i++)
            {
                int.TryParse(parent.GetChild(i).name.Split('_')[1], out var number);
                numbers.Add(number);
            }
            return FindSmallestMissingNumber(numbers);
        }


        private static int FindSmallestMissingNumber(List<int> numbers)
        {
            numbers.Sort();

            if (numbers.Count == 0 || numbers[0] > 1)
            {
                return 1;
            }

            for (int i = 0; i < numbers.Count - 1; i++)
            {
                if (numbers[i + 1] - numbers[i] > 1)
                {
                    return numbers[i] + 1;
                }
            }

            return numbers[numbers.Count - 1] + 1;
        }
    }
}
