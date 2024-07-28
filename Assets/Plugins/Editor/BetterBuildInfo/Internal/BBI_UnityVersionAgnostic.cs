// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal static class UnityVersionAgnostic
    {
#if UNITY_4_7
        public static RegexOptions CompiledRegexOptions = RegexOptions.None;
        public const BuildTarget iOSBuildTarget = BuildTarget.iPhone;
#else
        private delegate AudioCompressionFormat GetSoundCompressionFormatDelegate(AudioClip clip);
        private static readonly GetSoundCompressionFormatDelegate GetSoundCompressionFormat;


        private static readonly PropertyInfo AddressableSettingsProperty;
        private static readonly MethodInfo FindAssetEntryMethod;
        private static readonly PropertyInfo AddressableAddressProperty;

        static UnityVersionAgnostic()
        {
            var audioUtilType = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil", false);
            if (audioUtilType != null)
            {
                audioUtilType.CreateMethodDelegate("GetTargetPlatformSoundCompressionFormat", BindingFlags.Public | BindingFlags.Static, out GetSoundCompressionFormat);
                if (GetSoundCompressionFormat == null)
                {
                    audioUtilType.CreateMethodDelegateOrThrow("GetSoundCompressionFormat", BindingFlags.Public | BindingFlags.Static, out GetSoundCompressionFormat);
                }
            }

            {
                var addressableEditor = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Unity.Addressables.Editor");

                if (addressableEditor != null)
                {
                    try
                    {
                        var settingsType = addressableEditor.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject", true);
                        AddressableSettingsProperty = settingsType.GetPropertyOrThrow("Settings");
                        FindAssetEntryMethod = AddressableSettingsProperty.PropertyType.GetMethodOrThrow("FindAssetEntry", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(string) });
                        AddressableAddressProperty = FindAssetEntryMethod.ReturnType.GetPropertyOrThrow("address");
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"Unable to initialize Addressables support. Please report this along with the Addressables package version. The error: {ex}");
                    }
                }
            }
        }

        public static RegexOptions CompiledRegexOptions = RegexOptions.Compiled;
        public const BuildTarget iOSBuildTarget = BuildTarget.iOS;

        private static readonly System.Version VersionThatPrintsAssetsAfterPostProcess = new System.Version(5, 6, 2);
#endif

        public static void SetWindowTitle(EditorWindow window, GUIContent titleContent)
        {
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER
            window.titleContent = titleContent;
#else
            var titleContentProperty = typeof(EditorWindow).GetProperty("titleContent");
            if ( titleContentProperty != null )
            {
                titleContentProperty.SetValue(window, titleContent, null);
            }
            else
            {
                window.title = titleContent.text;
            }
#endif
        }

        public static string ToHtmlStringRGB(Color color)
        {
#if UNITY_5_3_OR_NEWER
            return ColorUtility.ToHtmlStringRGB(color);
#else
            Color32 color32 = color;
            return string.Format("{0:X2}{1:X2}{2:X2}", color32.r, color32.g, color32.b);
#endif
        }

        public static bool HasStuffInRootForStandaloneBuild
        {
            get
            {
                return UnityEditorInternal.InternalEditorUtility.GetUnityVersion().Major >= 2018;
            }
        }

        public static bool AssetLogPrintedAfterPostProcessors
        {
            get
            {
#if UNITY_4_7
                return false;
#else
                var version = UnityEditorInternal.InternalEditorUtility.GetUnityVersion();
                var result = version >= VersionThatPrintsAssetsAfterPostProcess;
                return result;
#endif
            }
        }

        public static bool HasNestedPrefabs
        {
            get
            {
#if UNITY_2018_3_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }

        public static string CurrentScene
        {
            get
            {
#if UNITY_5_3_OR_NEWER
                return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
#else
                return EditorApplication.currentScene;
#endif
            }
        }

        public static GameObject[] GetSceneRoots()
        {
#if UNITY_5_4_OR_NEWER || UNITY_5_3_5
            return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects();
#else
            List<GameObject> result = new List<GameObject>();

            var prop = new HierarchyProperty(HierarchyType.GameObjects);
            var expanded = new int[0];
            while (prop.Next(expanded))
            {
                var go = prop.pptrValue as GameObject;
                if ( go )
                {
                    result.Add(go);
                }
            }

            return result.ToArray();
#endif
        }

        public static string OpenFilePanelWithFilters(string title, string lastDirectory, string[] filters, string fallbackFilter)
        {
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
            return EditorUtility.OpenFilePanelWithFilters(title, lastDirectory, filters);
#else
            return EditorUtility.OpenFilePanel(title, lastDirectory, fallbackFilter);
#endif
        }

        public static string GetAudioClipLoadType(AudioClip audioClip, string audioClipPath)
        {
#if UNITY_4_7
            var importer = AssetImporter.GetAtPath(audioClipPath) as AudioImporter;
            if ( importer != null )
            {
                return importer.loadType.ToString();
            }
            else
            {
                return string.Empty;
            }
#else
            return audioClip.loadType.ToString();
#endif
        }

        public static string GetAudioClipFormat(AudioClip audioClip, string audioClipPath)
        {
#if UNITY_4_7
            var importer = AssetImporter.GetAtPath(audioClipPath) as AudioImporter;
            if (importer != null)
            {
                return importer.format.ToString();
            }
            else
            {
                return string.Empty;
            }
#else
            return GetSoundCompressionFormat(audioClip).ToString();
#endif
        }

        public static UnityEngine.Object GetLatestBuildReport()
        {
            try
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditor.BuildReporting.BuildReport", false);
                if ( type == null )
                {
                    type = typeof(Editor).Assembly.GetType("UnityEditor.Build.Reporting.BuildReport", true);
                }

                var getLatestMethod = type.GetMethodOrThrow("GetLatestReport", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                return (UnityEngine.Object)getLatestMethod.Invoke(null, null);
            }
            catch (System.Exception ex)
            {
                throw new System.NotSupportedException("Make sure you're on Unity 5.4 or newer", ex);
            }
        }

        public static Texture2D GetDirectional(this LightmapData data)
        {
#if UNITY_5_5_OR_NEWER
            return data.lightmapDir;
#else
            return data.lightmapNear;
#endif
        }

        public static Texture2D GetLight(this LightmapData data)
        {
#if UNITY_5_6_OR_NEWER
            return data.lightmapColor;
#elif UNITY_5_5_OR_NEWER
            return data.lightmapLight;
#else
            return data.lightmapFar;
#endif
        }

        public static bool IsUsingLegacySpriteAtlases
        {
            get { return EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOn || EditorSettings.spritePackerMode == SpritePackerMode.BuildTimeOnly; }
        }

        public static bool IsUsingSpriteAtlases
        {
#if UNITY_2017_1_OR_NEWER
            get { return EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOnAtlas || EditorSettings.spritePackerMode == SpritePackerMode.BuildTimeOnlyAtlas; }
#else
            get { return false; }
#endif
        }

        public static bool IsOSXBuildTarget(BuildTarget buildTarget)
        {
#if UNITY_2017_3_OR_NEWER
            if (buildTarget == BuildTarget.StandaloneOSX)
                return true;
#else
            if (buildTarget == BuildTarget.StandaloneOSXIntel || buildTarget == BuildTarget.StandaloneOSXIntel64 || buildTarget == BuildTarget.StandaloneOSXUniversal)
                return true;
#endif
            return false;
        }

        public static Texture2D LoadSpriteAtlasPreview(string tag, int page)
        {
            Texture2D[] previews = null;

            if (UnityVersionAgnostic.IsUsingLegacySpriteAtlases)
            {
                previews = UnityEditor.Sprites.Packer.GetTexturesForAtlas(tag);
            }
            else if (UnityVersionAgnostic.IsUsingSpriteAtlases)
            {
                var spriteAtlas = UnityEditor.AssetDatabase.FindAssets("t:spriteatlas")
                    .Select(x => AssetDatabase.GUIDToAssetPath(x))
                    .Select(x => AssetDatabase.LoadMainAssetAtPath(x))
                    .FirstOrDefault(x => GetSpriteAtlasTag(x) == tag);

                if (spriteAtlas)
                {
                    previews = LoadSpriteAtlasTextues(spriteAtlas);
                }
            }

            if (previews != null && previews.Length > page)
            {
                return previews[page];
            }
            return null;
        }

        internal static Texture2D[] LoadSpriteAtlasTextues(Object atlas)
        {
#if UNITY_2017_1_OR_NEWER
            var type = typeof(Editor).Assembly.GetType("UnityEditor.U2D.SpriteAtlasExtensions", true);
            var getPreviewTextures = type.GetMethodOrThrow("GetPreviewTextures", BindingFlags.NonPublic | BindingFlags.Static, new[] { atlas.GetType() });
            if (getPreviewTextures == null)
                throw new System.ArgumentOutOfRangeException("Method not found: GetPreviewTextures");

            return (Texture2D[])getPreviewTextures.Invoke(null, new object[] { atlas });
#else
            throw new System.NotSupportedException();
#endif

        }

        internal static string GetSpriteAtlasTag(Object atlas)
        {
#if UNITY_2017_1_OR_NEWER
            return ((UnityEngine.U2D.SpriteAtlas)atlas).tag;
#else
            throw new System.NotSupportedException();
#endif
        }

#if UNITY_5_2 || UNITY_5_3_OR_NEWER
        public static readonly bool IsGetRawTextureDataSupported = true;
        public static byte[] GetRawTextureData(Texture2D texture)
        {
            return texture.GetRawTextureData();
        }
#else
        public static readonly bool IsGetRawTextureDataSupported = false;
        public static byte[] GetRawTextureData(Texture2D texture)
        {
            throw new System.NotSupportedException();
        }
#endif

        public static bool? IsMeshOptimized(this ModelImporter importer)
        {
#if UNITY_2019_1_OR_NEWER
            return null;
#else
            return importer.optimizeMesh;
#endif
        }

        public static bool? AreMeshVerticesOptimised(this ModelImporter importer)
        {
#if UNITY_2019_1_OR_NEWER
            return importer.optimizeMeshVertices;
#else
            return null;
#endif
        }

        public static bool? AreMeshPolygonsOptimised(this ModelImporter importer)
        {
#if UNITY_2019_1_OR_NEWER
            return importer.optimizeMeshPolygons;
#else
            return null;
#endif
        }

        public static string GetAddressableAddress(string assetPath)
        {
            if (AddressableSettingsProperty == null || FindAssetEntryMethod == null || AddressableAddressProperty == null)
                return string.Empty;

            var settings = AddressableSettingsProperty.GetValue(null);
            if (settings == null)
                return string.Empty;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var addressableEntry = FindAssetEntryMethod.Invoke(settings, new object[] { guid });
            if (addressableEntry == null)
                return string.Empty;

            string result = (string)AddressableAddressProperty.GetValue(addressableEntry);
            return result; 
        }
    }
}
