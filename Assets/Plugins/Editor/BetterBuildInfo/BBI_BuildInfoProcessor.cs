// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Text;

using Stopwatch = System.Diagnostics.Stopwatch;
using Better.BuildInfo.Internal;
using UnitySprite = UnityEngine.Sprite;
using UnityEngine.SceneManagement;

namespace Better.BuildInfo
{
    [InitializeOnLoad]
    public sealed partial class BuildInfoProcessor : IDisposable
    {
        private Stopwatch buildTimer = new Stopwatch();
        private Stopwatch toolOverheadTimer = new Stopwatch();

        private Dictionary<string, AssetInfo> assetsUsedByScenes = new Dictionary<string, AssetInfo>();
        private List<string> processedScenes = new List<string>();
        private List<string> processedScenesNonAltered = new List<string>();
        private List<AssetProperty[]> scenesDetails = new List<AssetProperty[]>();

        private BuildInfoAssetDetailsCollector detailsCollector = null;

        public void Dispose()
        {
            buildTimer.Stop();
            toolOverheadTimer.Stop();
            if (detailsCollector != null)
                detailsCollector.Dispose();
        }

        private static void WarnIfDisabled()
        {
            if (!BetterBuildInfo.IsEnabled)
            {
                Log.Warning("Better Build Info is not enabled. This operation will have no effect.");
            }
        }


        private void PostProcessScene(Scene scene, string reliableScenePath, Dictionary<int, string> preexistingPrefabInstances)
        {
            Log.Debug("Processing scene: {0} ({1})", scene.name, scene.path);

            if (!buildTimer.IsRunning)
            {
                buildTimer.Start();
                BuildInfoSettings.ReportToOpen = null;
            }

            if (BuildInfoSettings.Instance.collectAssetsDetails && detailsCollector == null)
            {
                bool checkCompressedSize = false;
                if (UnityVersionAgnostic.IsGetRawTextureDataSupported)
                {
                    checkCompressedSize = BuildInfoSettings.Instance.checkAssetsCompressedSize;
                }
                detailsCollector = new BuildInfoAssetDetailsCollector(checkCompressedSize);
            }

            try
            {
                toolOverheadTimer.Start();

                var sceneIndex = processedScenes.Count;

                if (processedScenesNonAltered.Contains(scene.path))
                {
                    Log.Debug("Scene {0} already processed, ignoring", scene.path);
                }
                else
                {
                    AssetProperty[] details;
                    CollectUsedAssets(scene, reliableScenePath, assetsUsedByScenes, preexistingPrefabInstances, detailsCollector, out details);
                    processedScenes.Add(reliableScenePath);
                    scenesDetails.Add(details);
                    processedScenesNonAltered.Add(scene.path);
                }
            }
            finally
            {
                toolOverheadTimer.Stop();
            }
        }

        private void PostProcessBuild(BuildTarget target, string path, Func<Dictionary<string, long>, Dictionary<string, long>> assetsGetter)
        {
            toolOverheadTimer.Start();

            try
            {
                EditorUtility.DisplayProgressBar("Better Build Info", "Analyzing build...", 1.0f);

                List<AssetInfo> infos = new List<AssetInfo>();
                Dictionary<string, long> scenesSizesFromLog = new Dictionary<string, long>();

                {
                    Dictionary<string, long> assetsFromLog = assetsGetter(scenesSizesFromLog);
                    infos = GetAssetsAndSizes(assetsUsedByScenes, assetsFromLog);
                }

                BuildInfoProcessorUtils.DiscoverDependenciesAndMissingAtlases(infos, assetsUsedByScenes, detailsCollector);

                if ( detailsCollector != null )
                {
                    BuildInfoProcessorUtils.FinishCollectingDetails(infos, detailsCollector);
                }

                BuildArtifactsInfo artifactsInfo = null;
                try
                { 
                    artifactsInfo = BuildArtifactsInfo.Create(target, path);
                }
                catch (Exception ex)
                {
                    Log.Warning("Unable to obtain build artifacts info: {0}", ex);
                }

                BuildInfoProcessorUtils.RefreshScenesInfo(scenesSizesFromLog, infos, artifactsInfo, processedScenes, scenesDetails, detailsCollector);

                if (artifactsInfo != null)
                {
                    BuildInfoProcessorUtils.RefreshModulesInfo(infos, artifactsInfo);
                    BuildInfoProcessorUtils.RefreshStreamingAssetsInfo(infos, artifactsInfo);
                }

                // sort infos based on paths (easier diffs)
                infos.Sort(BuildInfoProcessorUtils.PathComparer);

                List<AssetBundleInfo> assetBundles = new List<AssetBundleInfo>();
                if (artifactsInfo != null)
                {
                    BuildInfoProcessorUtils.RefreshOtherArtifacts(infos, artifactsInfo);

                    if (BuildInfoSettings.Instance.checkAssetBundles)
                    {
                        EditorUtility.DisplayProgressBar("Better Build Info", "Analyzing asset bundles...", 1.0f);

                        var tests = BuildInfoSettings.Instance.assetBundleFilters.Select(x => WildcardTest.Create(x)).ToList();

                        var streamingAssetBundles = tests
                            .SelectMany(x => artifactsInfo.streamingAssets.Keys.Where(xx => x.IsMatch(xx)))
                            .Distinct()
                            .ToList();

                        var assetsFromBundles = BuildInfoProcessorUtils.AnalyzeAssetBundle(streamingAssetBundles.Select(x =>
                            new BuildInfoProcessorUtils.BundleOpenInfo()
                            {
                                name = "Assets/StreamingAssets/" + x,
                                copyTo = targetPath => artifactsInfo.copyStreamingAsset(x, targetPath),
                            }), out assetBundles);

                        foreach (var asset in assetsFromBundles)
                        {
                            var foundIndex = infos.BinarySearch(asset, BuildInfoProcessorUtils.PathComparer);
                            if (foundIndex <= 0)
                            {
                                asset.assetBundleOnly = true;
                                infos.Insert(~foundIndex, asset);
                            }
                            else
                            {
                                // need to merge
                                var existing = infos[foundIndex];
                                Debug.Assert(existing.assetBundles == null);

                                existing.assetBundles = asset.assetBundles;
                                existing.dependencies = existing.dependencies.Concat(asset.dependencies).Distinct().ToList();
                                existing.dependencies.Sort();
                            }
                        }
                    }
                }

                if (detailsCollector != null)
                {
                    EditorUtility.DisplayProgressBar("Better Build Info", "Analyzing build... (compressed sizes)", 1.0f);
                    BuildInfoProcessorUtils.FinishCalculatingCompressedSizes(infos, detailsCollector);
                    BuildInfoProcessorUtils.CalculateScriptReferences(infos, detailsCollector);
                }

                var settings = BuildInfoProcessorUtils.GetPlayerSettings(typeof(PlayerSettings))
                    .Concat(BuildInfoProcessorUtils.GetPlayerSettings(typeof(EditorUserBuildSettings)));

#if UNITY_ANDROID
                settings = settings.Concat(BuildInfoProcessorUtils.GetPlayerSettings(typeof(PlayerSettings.Android)));
#elif UNITY_IOS
                settings = settings.Concat(BuildInfoProcessorUtils.GetPlayerSettings(typeof(PlayerSettings.iOS)));
#endif

                var buildInfo = new BuildInfo()
                {
                    dateUTC = DateTime.UtcNow.Ticks,
                    buildTarget = target.ToString(),
                    projectPath = Environment.CurrentDirectory.Replace('\\', '/'),
                    outputPath = path,
                    unityVersion = Application.unityVersion,

                    buildTime = buildTimer.ElapsedMilliseconds / 1000.0f,
                    overheadTime = toolOverheadTimer.ElapsedMilliseconds / 1000.0f,

                    assets = infos,
                    scenes = processedScenes.Distinct().ToList(),
                    buildSettings = settings.GroupBy(x => x.name).OrderBy(x => x.Key).Select(x => x.FirstOrDefault()).ToList(),

                    assetBundles = assetBundles.OrderBy(x => x.path).ToList(),

                    environmentVariables = System.Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().Select(x => new BuildSetting()
                    {
                        name = x.Key.ToString(),
                        value = x.Value.ToString()
                    }).OrderBy(x => x.name).ToList(),
                };

                if (artifactsInfo != null)
                {
                    buildInfo.totalSize = artifactsInfo.totalSize.uncompressed;
                    buildInfo.compressedSize = artifactsInfo.totalSize.compressed;
                    buildInfo.streamingAssetsSize = artifactsInfo.streamingAssets.Values.Sum(x => x.uncompressed);
                    buildInfo.runtimeSize = artifactsInfo.runtimeSize.uncompressed;
                    buildInfo.compressedRuntimeSize = artifactsInfo.runtimeSize.compressed;
                };

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(BuildInfo));

                var outputPath = string.Empty;

                try
                {
                    outputPath = BuildInfoSettings.Instance.GetOutputPath(DateTime.Now, target, path);

                    var dirName = ReliablePath.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    using (var writer = File.CreateText(outputPath))
                    {
                        serializer.Serialize(writer, buildInfo);
                    }

                    Log.Info("Generated report at: {0}", outputPath);

                    if (BuildInfoSettings.Instance.autoOpenReportAfterBuild)
                    {
                        BuildInfoSettings.ReportToOpen = outputPath;
                    }
                }
                catch (System.Exception ex)
                {
                    var tempPath = ReliablePath.GetTempPath() + "BBI_" + Guid.NewGuid().ToString() + ".bbi";
                    Log.Error("Error saving report at {0}, saving it temporarily at {1}. Copy it manually to a target location. Error details: {2}", outputPath, tempPath, ex);

                    using (var writer = new StreamWriter(tempPath))
                    {
                        serializer.Serialize(writer, buildInfo);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Unexpected error: {0}", ex);
            }
            finally
            {
                if (!BuildInfoSettings.Instance.autoOpenReportAfterBuild)
                {
                    BuildInfoSettings.ReportToOpen = null;
                }

                EditorUtility.ClearProgressBar();
            }
        }

        private static void CollectUsedAssets(Scene scene, string sceneName, Dictionary<string, AssetInfo> assets, Dictionary<int, string> preexistingPrefabInstances, BuildInfoAssetDetailsCollector collector, out AssetProperty[] sceneDetails)
        {
            List<AssetProperty> details = new List<AssetProperty>();
            Func<string, UnityEngine.Object, AssetInfo> touchEntry = (assetPath, asset) =>
            {
                AssetInfo entry;
                if (!assets.TryGetValue(assetPath, out entry))
                {
                    entry = new AssetInfo()
                    {
                        path = assetPath,
                    };

                    assets.Add(assetPath, entry);
                }

                if (collector != null && entry.details == null)
                {
                    bool isMainAsset = true;
                    if (!AssetDatabase.IsMainAsset(asset) && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
                    {
                        isMainAsset = false;
                    }

                    if (isMainAsset)
                    {
                        details.Clear();

                        Log.Debug("Collecting details for asset: {0}", assetPath);
                        collector.CollectForAsset(details, asset, assetPath);
                        entry.details = details.ToArray();
                    }
                    else
                    {
                        Log.Debug("Not a main asset: {0} {1}", asset.name, AssetDatabase.GetAssetPath(asset));
                    }
                }

                if (!string.IsNullOrEmpty(sceneName))
                {
                    int sceneIndex = entry.scenes.BinarySearch(sceneName);
                    if (sceneIndex < 0)
                    {
                        entry.scenes.Insert(~sceneIndex, sceneName);
                    }
                }
                return entry;
            };

            var legacySpriteHandler = UnityVersionAgnostic.IsUsingLegacySpriteAtlases ? BuildInfoProcessorUtils.CreateLegacyAtlasHandler(touchEntry) : null;

            // include inactive ones too
            var sceneRoots = scene.GetRootGameObjects();

            sceneDetails = null;
            if (collector != null)
            {
                Log.Debug("Collecting scene details: {0}", sceneName);
                sceneDetails = collector.CollectForCurrentScene(sceneRoots);
            }

            Log.Debug("Processing scene objects for scene: {0}", sceneName);

            IEnumerable<UnityEngine.Object> objects = EditorUtility.CollectDependencies(sceneRoots).Where(x => x);

            foreach (var obj in objects)
            {
                string assetPath;
                var dep = obj;

                if ( !EditorUtility.IsPersistent(dep) )
                {
                    if (dep is GameObject)
                    {
                        // hopefully this will work some day :(
                        // if (PrefabUtility.GetPrefabInstanceStatus(dep) == PrefabInstanceStatus.Connected)
                        // {
                        //     
                        //     assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(dep);
                        // }
                        if (preexistingPrefabInstances != null)
                        {
                            // well, let's see if the workaround worked
                            preexistingPrefabInstances.TryGetValue(dep.GetInstanceID(), out assetPath);
                        }
                        else
                        {
                            assetPath = null;
                        }

                        if (string.IsNullOrEmpty(assetPath))
                            continue;

                        dep = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                        if (dep == null)
                            continue;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    assetPath = AssetDatabase.GetAssetPath(dep);
                }

                if (string.IsNullOrEmpty(assetPath))
                {
                    Log.Debug(dep, "empty path: name: {0}, scene: {1}", dep.name, sceneName);
                    continue;
                }

                touchEntry(assetPath, dep);

                if (legacySpriteHandler != null && dep is UnitySprite)
                {
                    legacySpriteHandler((UnitySprite)dep, assetPath);
                }
            }

            // add lightmaps
            Log.Debug("Processing lightmaps for scene: {0}", sceneName);
            foreach (var data in UnityEngine.LightmapSettings.lightmaps)
            {
                if (data.GetDirectional())
                {
                    touchEntry(AssetDatabase.GetAssetPath(data.GetDirectional()), data.GetDirectional());
                }
                if (data.GetLight())
                {
                    touchEntry(AssetDatabase.GetAssetPath(data.GetLight()), data.GetLight());
                }
            }

            // now check lightmap settings
            var lightmapSettings = BuildInfoProcessorUtils.GetLightmapSettings();

            for (var prop = new SerializedObject(lightmapSettings).GetIterator(); prop.Next(true);)
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var obj = prop.objectReferenceValue;
                    if (obj && EditorUtility.IsPersistent(obj))
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        touchEntry(path, obj);
                    }
                }
            }
        }

        internal static List<AssetInfo> GetAssetsAndSizes(Dictionary<string, AssetInfo> assetsUsedByScenes, Dictionary<string, long> assets)
        {
            List<AssetInfo> assetsInfo = new List<AssetInfo>(assetsUsedByScenes.Values);

            foreach (var asset in assets)
            {
                AssetInfo info;
                if (!assetsUsedByScenes.TryGetValue(asset.Key, out info))
                {
                    info = new AssetInfo()
                    {
                        path = asset.Key
                    };
                    assetsInfo.Add(info);
                }

                if (info.size != 0)
                {
                    Log.Warning("How come asset {0} already has a size of {1}?", asset.Key, info.size);
                }

                info.size = asset.Value;
            }

            return assetsInfo;
        }
    }
}
