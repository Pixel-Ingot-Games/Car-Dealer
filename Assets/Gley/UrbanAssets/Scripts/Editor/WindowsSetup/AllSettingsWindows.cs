using System.Linq;

namespace Gley.UrbanSystem.Editor
{
    internal class AllSettingsWindows
    {
        private WindowProperties[] _allWindows;


        internal void Initialize(WindowProperties[] allWindowsProperties)
        {
            _allWindows = allWindowsProperties;
        }


        internal WindowProperties GetWindowProperties(string className)
        {
            return _allWindows.First(cond => cond.ClassName == className);
        }


        internal string GetWindowName(string className)
        {
            return GetWindowProperties(className).Title;
        }
    }
}
