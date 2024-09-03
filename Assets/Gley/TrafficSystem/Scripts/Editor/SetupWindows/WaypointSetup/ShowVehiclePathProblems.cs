using Gley.UrbanSystem.Editor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class ShowVehiclePathProblems : ShowWaypointsTrafficBase
    {
        private readonly float _scrollAdjustment = 210;

        private bool _waypointsLoaded = false;


        internal override void DrawInScene()
        {
            _waypointsOfInterest = _trafficWaypointDrawer.ShowVehiclePathProblems(_editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.AgentColor);

            if (_waypointsLoaded == false)
            {
                SettingsWindowBase.TriggerRefreshWindowEvent();
                _waypointsLoaded = true;
            }
            base.DrawInScene();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            base.ScrollPart(width, height);
            GUILayout.EndScrollView();
        }
    }
}
