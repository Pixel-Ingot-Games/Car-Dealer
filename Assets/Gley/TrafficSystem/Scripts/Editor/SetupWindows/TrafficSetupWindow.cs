using Gley.UrbanSystem.Editor;
using UnityEditor;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficSetupWindow : SetupWindowBase
    {
        protected TrafficSettingsWindowData _editorSave;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _editorSave = new SettingsLoader(Internal.TrafficSystemConstants.windowSettingsPath).LoadSettingsAsset<TrafficSettingsWindowData>();
            return this;
        }


        internal override void DestroyWindow()
        {
            EditorUtility.SetDirty(_editorSave);
            base.DestroyWindow();
        }
    }
}
