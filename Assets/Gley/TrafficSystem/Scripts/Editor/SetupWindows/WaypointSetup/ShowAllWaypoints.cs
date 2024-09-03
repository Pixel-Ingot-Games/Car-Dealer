namespace Gley.TrafficSystem.Editor
{
    internal class ShowAllWaypoints : ShowWaypointsTrafficBase
    {
        internal override void DrawInScene()
        {
            _trafficWaypointDrawer.ShowAllWaypoints(_editorSave.EditorColors.WaypointColor, _editorSave.ShowConnections, _editorSave.showSpeed, _editorSave.EditorColors.SpeedColor, _editorSave.ShowVehicles, _editorSave.EditorColors.AgentColor, _editorSave.showOtherLanes, _editorSave.EditorColors.LaneChangeColor, _editorSave.ShowPriority, _editorSave.EditorColors.PriorityColor, false);
            base.DrawInScene();
        }
    }
}
