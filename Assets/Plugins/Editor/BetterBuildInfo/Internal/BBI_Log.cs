using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal static class Log
    {
        public static void Error(string format, params object[] args)
        {
            UnityEngine.Debug.LogError("BetterBuildInfo: " + string.Format(format, args));
        }

        public static void Debug(string format, params object[] args)
        {
            if (BuildInfoSettings.Instance.debugLogEnabled)
            {
                UnityEngine.Debug.Log("BetterBuildInfo: " + string.Format(format, args));
            }
        }

        public static void Debug(UnityEngine.Object context, string format, params object[] args)
        {
            if (BuildInfoSettings.Instance.debugLogEnabled)
            {
                UnityEngine.Debug.Log("BetterBuildInfo: " + string.Format(format, args), context);
            }
        }

        public static void Info(string format, params object[] args)
        {
            UnityEngine.Debug.Log("BetterBuildInfo: " + string.Format(format, args));
        }

        public static void Warning(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarning("BetterBuildInfo: " + string.Format(format, args));
        }

        public static void Warning(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarning("BetterBuildInfo: " + string.Format(format, args), context);
        }
    }
}
