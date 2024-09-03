using System.Collections.Generic;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class NavigationRuntimeData
    {
        private readonly AllSettingsWindows _allSettingsWindows;

        private readonly List<string> _path;
        

        public NavigationRuntimeData(AllSettingsWindows allSettingsWindows)
        {
            _allSettingsWindows = allSettingsWindows;
            _path = new List<string>();
        }


        internal void AddWindow(string newWindow)
        {
            if (!_path.Contains(newWindow))
            {
                _path.Add(newWindow);
            }
            else
            {
                Debug.LogWarning("Trying to add same window twice: " + newWindow);
            }
        }


        internal string GetBackPath()
        {
            if (_path.Count == 0)
                return "";

            string result = "";
            for (int i = 0; i < _path.Count; i++)
            {
                result += _allSettingsWindows.GetWindowName(_path[i].Split('.')[3]) + " > ";
            }
            return result;
        }


        internal string RemoveLastWindow()
        {
            string lastWindow = _path[_path.Count - 1];

            _path.RemoveAt(_path.Count - 1);
            return lastWindow;
        }
    }
}
