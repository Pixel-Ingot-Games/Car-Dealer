using Gley.UrbanSystem.Editor;

namespace Gley.TrafficSystem.Editor
{
    internal class GridSetupWindow : GridSetupWindowBase
    {
        internal override void DrawInScene()
        {
            if (_viewGrid)
            {
                _gridDrawer.DrawGrid(true);
            }
            base.DrawInScene();
        }
    }
}
