namespace Gley.TrafficSystem.Internal
{
    public static class TrafficSystemConstants
    {
        public const string PACKAGE_NAME = "TrafficSystem";
        public const string GLEY_TRAFFIC_SYSTEM = "GLEY_TRAFFIC_SYSTEM";
        public const string GLEY_CIDY_TRAFFIC = "GLEY_CIDY_TRAFFIC";
        public const string GLEY_EASYROADS_TRAFFIC = "GLEY_EASYROADS_TRAFFIC";

        public const string TrafficHolderName = "TrafficHolder";
        public const string layerSetupData = "LayerSetupData";
        public const string layerPath = "Assets/Gley/TrafficSystem/Resources/LayerSetupData.asset";
        public const string trafficNamespaceEditor = "Gley.TrafficSystem.Editor";
        public const string trafficNamespace = "Gley.TrafficSystem";
        public const string windowSettingsPath = "Assets/Gley/TrafficSystem/EditorSave/SettingsWindowData.asset";
        public const string agentTypesPath = "/Gley/TrafficSystem/Scripts/ToUse";
        public const string DebugOptionsPath = "EditorSave";
        public const string DebugOptionsName = "DebugOptions";
        public const string roadName = "Road";

        public const int INVALID_WAYPOINT_INDEX = -1;
        public const int INVALID_VEHICLE_INDEX = -1;

        public static string EditorWaypointsHolder
        {
            get
            {
                return $"{PACKAGE_NAME}/{UrbanSystem.Internal.UrbanSystemConstants.EDITOR_HOLDER}/EditorWaypoints";
            }
        }

        public static string EditorConnectionsHolder
        {
            get
            {
                return $"{PACKAGE_NAME}/{UrbanSystem.Internal.UrbanSystemConstants.EDITOR_HOLDER}/EditorConnections";
            }
        }

        public static string EditorIntersectionsHolder
        {
            get
            {
                return $"{PACKAGE_NAME}/{UrbanSystem.Internal.UrbanSystemConstants.EDITOR_HOLDER}/Intersections";
            }
        }

        public static string PlayHolder
        {
            get
            {
                return $"{PACKAGE_NAME}/{UrbanSystem.Internal.UrbanSystemConstants.PLAY_HOLDER}";
            }
        }
    }
}
