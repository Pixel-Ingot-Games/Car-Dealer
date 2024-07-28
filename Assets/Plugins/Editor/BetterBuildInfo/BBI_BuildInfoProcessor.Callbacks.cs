#if UNITY_2018_3_OR_NEWER

using Better.BuildInfo.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Better.BuildInfo
{
    public partial class BuildInfoProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport, IProcessSceneWithReport
    {
        private static BuildInfoProcessor activeInstance = null;
        private Dictionary<int, string> preexistingPrefabInstances = new Dictionary<int, string>();

        int IOrderedCallback.callbackOrder => int.MaxValue;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            if ( activeInstance != null )
            {
                activeInstance.CleanUpIfBuildFailed();
                Debug.Assert(activeInstance == null);
            }

            if (!BetterBuildInfo.IsEnabled)
                return;
            if (BuildInfoSettings.Instance.useLegacyCallbacks)
                return;

            activeInstance = this;

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.update += CleanUpIfBuildFailed;
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            if (this != activeInstance)
                return; // was not listening

            Debug.Assert(!BuildInfoSettings.Instance.useLegacyCallbacks);

            try
            {
                CleanUp();

                if (report.summary.result != BuildResult.Succeeded && report.summary.result != BuildResult.Unknown)
                {
                    Log.Warning("Build has not succeeded (result: {0}), not going to generate a report", report.summary.result);
                }
                else
                {
                    PostProcessBuild(report.summary.platform, report.summary.outputPath, scenes =>
                    {
                        return BuildReportReader.CollectUsedAssets(report);
                    });
                }
            }
            finally
            {
                Dispose();
            }
        }

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null)
                return; // not a build
            if (this != activeInstance)
                return;

            Debug.Assert(!BuildInfoSettings.Instance.useLegacyCallbacks);
            PostProcessScene(scene, scene.path, preexistingPrefabInstances);
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!BuildPipeline.isBuildingPlayer)
                return;
            if (this != activeInstance)
                return;

            Debug.Assert(!BuildInfoSettings.Instance.useLegacyCallbacks);
            preexistingPrefabInstances.Clear();

            // collect all scene objects
            var allGameObjects = EditorUtility.CollectDeepHierarchy(scene.GetRootGameObjects())
                .OfType<GameObject>()
                .Where(x => PrefabUtility.GetPrefabInstanceStatus(x) == PrefabInstanceStatus.Connected);

            // store all the prefabs please
            foreach (var gameObject in allGameObjects)
            {
                var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Log.Debug("Prefab not found for {0}, how come?", gameObject);
                }
                else
                {
                    preexistingPrefabInstances.Add(gameObject.GetInstanceID(), prefabPath);
                }
            }
        }

        private void CleanUpIfBuildFailed()
        {
            Debug.Assert(activeInstance == this);
            Log.Warning("The build seems to have failed or been interrupted");
            CleanUp();
            activeInstance = null;
        }


        private void CleanUp()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorApplication.update -= CleanUpIfBuildFailed;
        }
    }
}
#endif
