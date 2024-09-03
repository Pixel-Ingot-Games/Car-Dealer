namespace Gley.UrbanSystem.Internal
{
    public static class UrbanSystemErrors
    {
        public static string SceneDataIsNull => "Scene data is null.";
        public static string SceneGridIsNull => "Scene grid is not set up correctly.";
        public static string EmptyScene => "The scene seems empty. Please add some geometry inside the scene before setting up anything.";
        public static string NoPathFindingWaypoints => "Path Finding Waypoints not found.";
        public static string NullPathFindingData => "Path Finding data is null.";
        public static string TypeNotFound<T>()
        {
            return $"{typeof(T)} could not be found.";
        }

        public static string MultipleComponentsOfTypeFound<T>()
        {
            return $"Multiple components of type {typeof(T)} exist in the scene.";
        }

        public static string ComponentAlreadyExistsOn<T>(string name)
        {
            return $"{typeof(T)} component exists on: {name}";
        }

        internal static string ObjectNotFound(string path)
        {
            return $"Object {path} not found";
        }

        public static string NoPrevs(string name)
        {
            return $"Waypoint {name} has no prevs. Please regenerate this road or connection if this is not intended.";
        }
    }
}