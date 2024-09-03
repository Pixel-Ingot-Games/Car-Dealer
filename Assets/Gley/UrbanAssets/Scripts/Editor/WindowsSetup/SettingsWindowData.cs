using System.Collections.Generic;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    public abstract class SettingsWindowData : ScriptableObject
    {
        public EditorColors EditorColors;
        public RoutesColors AgentRoutes;
        public RoutesColors PathFindingRoutes;
        public RoutesColors PriorityRoutes;

        public bool ShowConnections;
        public bool ShowPriority;
        public bool ShowVehicles;

        public bool ViewOtherRoads;
        public bool ViewRoadLanes;
        public bool ViewRoadWaypoints;
        public bool ViewLabels;

        public bool PathFindingEnabled;

        public float WaypointDistance;
        public float LaneWidth;

        public MoveTools MoveTool = MoveTools.Move2D;

        internal abstract SettingsWindowData Initialize();
    }

    [System.Serializable]
    public class EditorColors
    {
        public Color LabelColor = Color.white;

        public Color RoadColor = Color.green;
        public Color LaneColor = Color.blue;
        public Color LaneChangeColor = Color.magenta;
        public Color ConnectorLaneColor = Color.cyan;

        public Color AnchorPointColor = Color.white;
        public Color ControlPointColor = Color.red;
        public Color RoadConnectorColor = Color.cyan;
        public Color SelectedRoadConnectorColor = Color.green;

        public Color WaypointColor = Color.blue;
        public Color SelectedWaypointColor = Color.green;
        public Color DisconnectedColor = Color.red;
        public Color PrevWaypointColor = Color.yellow;

        public Color SpeedColor = Color.white;
        public Color AgentColor = Color.green;
        public Color PriorityColor = Color.green;

        public Color ComplexGiveWayColor = Color.black;

        public Color IntersectionColor = Color.green;
        public Color LightsColor = Color.cyan;
        public Color StopWaypointsColor = Color.red;
        public Color ExitWaypointsColor = Color.cyan;
    }

    [System.Serializable]
    public class RoutesColors
    {
        public List<Color> RoutesColor = new List<Color> { Color.white };
        public List<bool> Active = new List<bool> { true };
    }
}
