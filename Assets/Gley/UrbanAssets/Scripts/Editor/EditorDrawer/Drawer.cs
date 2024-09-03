using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class Drawer
    {
        private readonly bool _showDebugMessages = false;
        private readonly EditorData _data;
        private readonly Quaternion _arrowSide1 = Quaternion.Euler(0, -25, 0);
        private readonly Quaternion _arrowSide2 = Quaternion.Euler(0, 25, 0);

        private Vector3 _direction;

        protected GUIStyle _style;
        protected bool _cameraMoved;


        protected Drawer(EditorData data)
        {
            if (_showDebugMessages)
            {
                Debug.Log("Initialize " + this);
            }
            _data = data;
            data.OnModified += Refresh;
            _style = new GUIStyle();
            SceneCameraTracker.OnSceneCameraMoved += CameraMoved;
            SceneCameraTracker.TriggerSceneCameraMovedEvent();
        }


        protected void DrawTriangle(Vector3 start, Vector3 end)
        {
            _direction = (start - end).normalized;
            Handles.DrawPolyLine(end, end + _arrowSide1 * _direction, end + _arrowSide2 * _direction, end);
        }


        private void Refresh()
        {
            SceneCameraTracker.TriggerSceneCameraMovedEvent();
            SceneView.RepaintAll();
        }


        private void CameraMoved()
        {
            _cameraMoved = true;
        }


        internal virtual void OnDestroy()
        {
            if (_showDebugMessages)
            {
                Debug.Log("OnDestroy " + this);
            }
            _data.OnModified -= Refresh;
            SceneCameraTracker.OnSceneCameraMoved -= CameraMoved;
        }
    }
}
