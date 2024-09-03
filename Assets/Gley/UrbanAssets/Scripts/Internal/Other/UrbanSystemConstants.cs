namespace Gley.UrbanSystem.Internal
{
    public static class UrbanSystemConstants
    {
        public const string PARENT = "Gley";
        public const string PACKAGE_NAME = "UrbanSystem";
        public const string PLAY_HOLDER = "PlayModeData";
        public const string EDITOR_HOLDER = "EditorData";

        public const string EDITOR_TAG = "EditorOnly";

        public const string OutWaypointEnding = "-Out";
        public const string InWaypointEnding = "-In";
        public const string LanesHolderName = "Lanes";
        public const string LaneNamePrefix = "Lane_";
        public const string WaypointNamePrefix = "Waypoint_";
        public const string ConnectionEdgeName = "CConnect";
        public const string ConnectionWaypointName = "Connector";
        public const string Connect = "Connect";

        public static string PlayHolder
        {
            get
            {
                return $"{PACKAGE_NAME}/{PLAY_HOLDER}";
            }
        }
    }
}