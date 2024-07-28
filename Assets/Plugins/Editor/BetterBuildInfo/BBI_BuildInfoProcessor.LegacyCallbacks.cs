using Better.BuildInfo.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Better.BuildInfo
{
    public sealed partial class BuildInfoProcessor
    {
        private static BuildInfoProcessor s_buildSession = null;
        private static string[] expectedScenesNames = null;
        private bool deferAnalysis = false;
        private BuildTarget buildTarget;
        private string buildPath;

        static BuildInfoProcessor()
        {
            EditorApplication.update += () =>
            {
                if (s_buildSession != null)
                {
                    OnBuildEnded();
                }
            };
        }

        private static string EditorLogPath
        {
            get
            {
                // first try to get it from the command line
                var args = System.Environment.GetCommandLineArgs();
                var logFileSwitchIndex = Array.FindIndex(args, x => string.Equals(x, "-logFile", StringComparison.OrdinalIgnoreCase));
                if (logFileSwitchIndex >= 0 && logFileSwitchIndex < args.Length - 1)
                {
                    return args[logFileSwitchIndex + 1];
                }

                switch (SystemInfo.operatingSystemFamily)
                {
                    case OperatingSystemFamily.Windows:
                        return Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Unity\Editor\Editor.log");
                    case OperatingSystemFamily.MacOSX:
                        return ReliablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Logs/Unity/Editor.log");
                    case OperatingSystemFamily.Linux:
                        return ReliablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "unity3d/Editor.log");
                    default:
                        // fallback to the old code
                        Log.Warning("Unknown operatingSystem ({0}), falling back to Environment.OSVersion", SystemInfo.operatingSystem);
                        switch (Environment.OSVersion.Platform)
                        {
                            case PlatformID.Win32NT:
                                return Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Unity\Editor\Editor.log");
                            case PlatformID.MacOSX:
                            case PlatformID.Unix:
                                return ReliablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Logs/Unity/Editor.log");
                            default:
                                throw new NotSupportedException("Platform " + Environment.OSVersion.Platform + " not supported");
                        }
                }
            }
        }

        internal static void OnBuildEnded()
        {
            expectedScenesNames = null;
            if (s_buildSession == null)
                throw new System.InvalidOperationException("Build was not in progress");

            try
            {
                using (s_buildSession)
                {
                    if (s_buildSession.deferAnalysis)
                    {
                        s_buildSession.PostProcessBuild(s_buildSession.buildTarget, s_buildSession.buildPath, scenes =>
                        {
                            return BuildLogParser.GetLastBuildAssetsSizes(EditorLogPath, scenes);
                        });
                    }
                    else if (s_buildSession.buildTimer.IsRunning)
                    {
                        Log.Warning("The build seems to have failed or been interrupted");
                    }
                }
            }
            finally
            {
                s_buildSession = null;
            }
        }

        [UnityEditor.Callbacks.PostProcessScene(int.MaxValue)]
        public static void OnPostprocessScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!BetterBuildInfo.IsEnabled)
            {
                return;
            }

            if (!BuildInfoSettings.Instance.useLegacyCallbacks)
            {
                if (!UnityVersionAgnostic.HasNestedPrefabs)
                    Log.Warning("To use new callbacks update Unity to 2018.3 or newer.");
                return;
            }

            if (s_buildSession == null)
            {
                s_buildSession = new BuildInfoProcessor();
            }

            var scene = EditorSceneManager.GetActiveScene();
            var sceneIndex = s_buildSession.processedScenes.Count;
            string scenePath = scene.path;

            if (expectedScenesNames != null)
            {
                if (expectedScenesNames.Length > sceneIndex)
                {
                    scenePath = expectedScenesNames[sceneIndex];
                }
                else
                {
                    Log.Warning("SetExpectedScenesPaths was not provided with enough paths for the build (current scene no: {0} ({1})). " +
                               "Carrying on, but bad things may happen.", sceneIndex, scenePath);
                }
            }
            else if (string.IsNullOrEmpty(scenePath) || scenePath.ToLower().StartsWith("temp/"))
            {
                // oopsie
                var guessName = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).Skip(sceneIndex).FirstOrDefault();
                if (guessName != null)
                {
                    Log.Warning("Detected a temp scene ({0}), guessed it's really {1} based on editor build settings.\n" +
                               "This happens if a scene to be included in a build is opened. If you call BuildPlayer method from script " +
                               "consider calling Better.BuildInfoProcessor.SetExpectedScenesPaths first.", scenePath, guessName);

                    scenePath = guessName;
                }
                else
                {
                    Log.Warning("Detected a temp scene ({0}), but unable to guess it's real name.\n" +
                               "This happens if a scene to be included in a build is opened and you are using BuildPlayer method from script; " +
                               "consider calling Better.BuildInfoProcessor.SetExpectedScenesPaths first."
                               , guessName);
                }
            }

            s_buildSession.PostProcessScene(scene, scenePath, null);
        }

        [UnityEditor.Callbacks.PostProcessBuild(int.MaxValue)]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (!BetterBuildInfo.IsEnabled)
            {
                return;
            }

            if (!BuildInfoSettings.Instance.useLegacyCallbacks)
            {
                return;
            }

            if (s_buildSession == null)
            {
                Log.Error("Somehow the tool hasn't recognized that the build is in progress");
                return;
            }

            if (UnityVersionAgnostic.AssetLogPrintedAfterPostProcessors)
            {
                Log.Debug("This Unity version prints assets usage *after* post processors are run (possibly a bug); deferring the analysis to the first editor update after the build.");
                s_buildSession.buildTarget = target;
                s_buildSession.buildPath = path;
                s_buildSession.deferAnalysis = true;
            }
            else
            {
                try
                {
                    using (s_buildSession)
                    {
                        s_buildSession.PostProcessBuild(target, path, scenes =>
                        {
                            return BuildLogParser.GetLastBuildAssetsSizes(EditorLogPath, scenes);
                        });
                    }
                }
                finally
                {
                    s_buildSession = null;
                }
            }

            return;
        }

        public static void SetExpectedScenesPaths(string[] paths)
        {
            WarnIfDisabled();
            expectedScenesNames = paths;
        }

    }
}