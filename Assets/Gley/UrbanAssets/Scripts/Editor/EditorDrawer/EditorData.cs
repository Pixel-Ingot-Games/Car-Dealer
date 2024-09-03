using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal abstract class EditorData 
    {
        private readonly bool _showDebugMessages = false;

        internal delegate void Modified();
        internal event Modified OnModified;
        internal void TriggerOnModifiedEvent()
        {
            if (_showDebugMessages)
            {
                Debug.Log("Modified " + this);
            }
            LoadAllData();
            OnModified?.Invoke();
        }


        protected abstract void LoadAllData();


        protected EditorData()
        {
            if (_showDebugMessages)
            {
                Debug.Log("EditorData " + this);
            }
        }
    }
}