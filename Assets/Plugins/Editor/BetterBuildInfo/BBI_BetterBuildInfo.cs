using Better.BuildInfo;
using Better.BuildInfo.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

public static class BetterBuildInfo
{
    public static readonly string Version = "1.9.4";

    [Obsolete("Obsolete, use ForceEnabledFlag instead")]
    public static bool ForceDisabled
    {
        get { return ForceEnabledFlag == false; }
        set
        {
            if (value)
                ForceEnabledFlag = false;
            else
                ForceEnabledFlag = null;
        }
    }

    /// <summary>
    /// Setting to true or false enables/disables the tool, ignoring enabled flag in the settings.
    /// </summary>
    public static bool? ForceEnabledFlag =
#if BETTERBUILDINFO_FORCE_DISABLED
        false;
#elif BETTERBUILDINFO_FORCE_ENABLED
        true;
#else
        null;
#endif

    public static bool IsCloudBuild =
#if UNITY_CLOUD_BUILD
        true;
#else
        false;
#endif

    public static bool IsEnabled
    {
        get
        {
            if (ForceEnabledFlag.HasValue)
                return ForceEnabledFlag.Value;

            return BuildInfoSettings.Instance.enabled;
        }
    }
    
    /// <summary>
    /// Call with the same paths you pass to BuildPipeline.BuildPlayer immediately before calling it.
    /// </summary>
    /// <param name="paths"></param>
    public static void SetExpectedScenesPaths(string[] paths)
    {
        if (!BuildInfoSettings.Instance.useLegacyCallbacks)
        {
            Log.Warning("If using new callbacks, this is no longer needed.");
            return;
        }

        BuildInfoProcessor.SetExpectedScenesPaths(paths);
    }

    /// <summary>
    /// Call it immediately after BuildPipeline.BuildPlayer, if running in batchmode with Unity 5.6.2 or newer.
    /// </summary>
    public static void NotifyBuildEnded()
    {
        if (!BuildInfoSettings.Instance.useLegacyCallbacks)
        {
            Log.Warning("If using new callbacks, this is no longer needed.");
            return;
        }
        
        if (UnityVersionAgnostic.AssetLogPrintedAfterPostProcessors)
        {
            BuildInfoProcessor.OnBuildEnded();
        }
    }

    /// <summary>
    /// Call it immediately after BuildPipeline.BuildPlayer, if running in batchmode with Unity 5.6.2 or newer.
    /// </summary>
    [Obsolete("Use NotifyBuildEnded instead.")]
    public static void NotifyCloudBuildEnded()
    {
        if (!BuildInfoSettings.Instance.useLegacyCallbacks)
        {
            Log.Warning("If using new callbacks, this is no longer needed.");
            return;
        }

        if (!IsEnabled)
            return;

        var settings = BuildInfoSettings.Instance;
        var originalOutputPath = settings.outputPath;
        var autoOpenReportAfterBuild = settings.autoOpenReportAfterBuild;
        
        try
        {
            settings.outputPath = settings.cloudBuildOutputPath;
            settings.autoOpenReportAfterBuild = false;
            BuildInfoProcessor.OnBuildEnded();
        }
        finally
        {
            settings.outputPath = originalOutputPath;
            settings.autoOpenReportAfterBuild = autoOpenReportAfterBuild;
        }
    }
}
