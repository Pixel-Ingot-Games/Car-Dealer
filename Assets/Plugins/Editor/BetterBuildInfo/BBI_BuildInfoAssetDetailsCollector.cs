// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Better.BuildInfo.Internal;
using System.IO.Compression;
using System.IO;

namespace Better.BuildInfo
{
    internal sealed class BuildInfoAssetDetailsCollector : IDisposable
    {
        private delegate void GetBatchingForPlatformDelegate(BuildTarget platform, out int staticBatching, out int dynamicBatching);
        private static readonly GetBatchingForPlatformDelegate GetBatchingForPlatform;

        private static readonly System.Type s_internalShaderChannelType;
        private static readonly MethodInfo s_hasChannelMethodInfo;

        private static readonly Func<Mesh, int> s_getPrimitiveCount;

        private readonly CompressedSizeWorker m_compressionWorker;
        private readonly Dictionary<string, long> m_compressedSizes;
        private readonly System.Diagnostics.Stopwatch m_compressionWorkerOverhead = new System.Diagnostics.Stopwatch();

        private readonly List<string> m_scriptsToLookReferencesTo = new List<string>();


        static BuildInfoAssetDetailsCollector()
        {
            GetBatchingForPlatform = typeof(PlayerSettings).CreateMethodDelegateOrThrow<GetBatchingForPlatformDelegate>("GetBatchingForPlatform", BindingFlags.NonPublic | BindingFlags.Static);

            s_internalShaderChannelType = typeof(Mesh).Assembly.GetType("UnityEngine.Mesh+InternalShaderChannel");
            if (s_internalShaderChannelType != null)
            {
                s_hasChannelMethodInfo = typeof(Mesh).GetMethodOrThrow("HasChannel", BindingFlags.Instance | BindingFlags.NonPublic, new[] { s_internalShaderChannelType });
            }

            var internalMeshUtilType = typeof(Editor).Assembly.GetType("UnityEditor.InternalMeshUtil", true);
            internalMeshUtilType.CreateMethodDelegateOrThrow("GetPrimitiveCount", BindingFlags.Public | BindingFlags.Static, out s_getPrimitiveCount);
        }

        public BuildInfoAssetDetailsCollector(bool collectCompressedSizes)
        {
            if (collectCompressedSizes)
            {
                m_compressionWorker = new CompressedSizeWorker();
                m_compressedSizes = new Dictionary<string, long>();
            }
        }

        public bool CollectForAsset(List<AssetProperty> props, UnityEngine.Object asset, string assetPath)
        {
            if (asset is Texture2D)
            {
                var importer = string.IsNullOrEmpty(assetPath) ? null : AssetImporter.GetAtPath(assetPath) as TextureImporter;
                CollectDetailsForTexture2D(props, (Texture2D)asset, importer);

                if (m_compressionWorker != null)
                {
                    BeginCompressionJob(asset, compressedSize =>
                    {
                        lock (m_compressedSizes)
                        {
                            m_compressedSizes.Add(assetPath, compressedSize);
                        }
                    });
                }
            }
            else if (asset is AudioClip)
            {
                CollectDetailsForAudioClip(props, (AudioClip)asset, assetPath);
            }
            else if (asset is GameObject)
            {
                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer is ModelImporter)
                {
                    CollectDetailsModelPrefab(props, (GameObject)asset, (ModelImporter)importer);
                }
                else
                {
                    CollectDetailsForPrefab(props, (GameObject)asset);
                }
            }
            else if (asset is Material)
            {
                CollectDetailsForMaterial(props, (Material)asset);
            }
            else if (asset is MonoScript)
            {
                CollectDetailsForMonoScript(props, (MonoScript)asset, assetPath);
            }

            return props.Count > 0;
        }

        private void CollectDetailsForMonoScript(List<AssetProperty> props, MonoScript asset, string assetPath)
        {
            string className = string.Empty;

            var classType = asset.GetClass();
            if (classType != null && classType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                className = classType.FullName;
                m_scriptsToLookReferencesTo.Add(assetPath);
            }

            props.Add(AssetProperty.Create("ScriptClass", className));
        }

        public AssetProperty[] CollectForCurrentScene(GameObject[] sceneRoots)
        {
            var allObjects = EditorUtility.CollectDeepHierarchy(sceneRoots);
            var gameObjectCount = allObjects.Count(x => x is GameObject);
            var componentsCount = allObjects.Count(x => x is Component);

            List<AssetProperty> props = new List<AssetProperty>();
            props.Add(AssetProperty.Create("GameObjects", gameObjectCount));
            props.Add(AssetProperty.Create("Components", componentsCount));

            int staticBatching, dynamicBatching;
            GetBatchingForPlatform(EditorUserBuildSettings.activeBuildTarget, out staticBatching, out dynamicBatching);

            if (staticBatching != 0)
            {
                HashSet<Mesh> staticMeshes = new HashSet<Mesh>();
                foreach (var mf in allObjects.OfType<MeshFilter>())
                {
                    var sharedMesh = mf.sharedMesh;
                    if (sharedMesh == null)
                        continue;

                    var meshRenderer = mf.GetComponent<MeshRenderer>();
                    if (meshRenderer == null)
                        continue;

                    if (EditorUtility.IsPersistent(sharedMesh))
                        continue;

                    if (sharedMesh.name.StartsWith("Combined Mesh (root: "))
                    {
                        staticMeshes.Add(sharedMesh);
                    }
                }

                var combinedMeshVertirces = staticMeshes.Sum(x => x.vertexCount);
                var primitives = staticMeshes.Sum(x => GetPrimitiveCount(x));

                props.Add(AssetProperty.Create("Vertices", combinedMeshVertirces));
                props.Add(AssetProperty.Create("Triangles", primitives));

                if (IsGetUsedChannelsSupported)
                {
                    var channels = staticMeshes.SelectMany(x => GetUsedChannelsSanitized(x)).Distinct().OrderBy(x => x).ToArray();
                    props.Add(AssetProperty.Create("VertexFormat", string.Join(",", channels.ToArray())));
                }

                var lightmapsCount = LightmapSettings.lightmaps.Select(x => (x.GetLight() != null ? 1 : 0) + (x.GetDirectional() != null ? 1 : 0)).Sum();
                props.Add(AssetProperty.Create("LightmapTextures", lightmapsCount));
            }

            return props.ToArray();
        }

        private static void CollectDetailsForPrefab(List<AssetProperty> props, GameObject prefab)
        {
            var hierarchy = EditorUtility.CollectDeepHierarchy(new[] { prefab });
            props.Add(AssetProperty.Create("GameObjects", hierarchy.Count(x => x is GameObject)));
            props.Add(AssetProperty.Create("Components", hierarchy.Count(x => x is Component)));
        }

        private static void CollectDetailsModelPrefab(List<AssetProperty> props, GameObject prefab, ModelImporter importer)
        {
            int totalVertices = 0;
            int totalBlendShapes = 0;
            int totalPrimitives = 0;

            HashSet<string> channels = null;
            if (IsGetUsedChannelsSupported)
            {
                channels = new HashSet<string>();
            }

            foreach (var mesh in AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).OfType<Mesh>())
            {
                totalVertices += mesh.vertexCount;
                totalBlendShapes += mesh.blendShapeCount;
                totalPrimitives += GetPrimitiveCount(mesh);

                if (channels != null)
                {
                    foreach (var channel in GetUsedChannelsSanitized(mesh))
                    {
                        channels.Add(channel);
                    }
                }
            }

            if (importer.IsMeshOptimized() != null)
                props.Add(AssetProperty.Create("Optimize", importer.IsMeshOptimized()));
            if (importer.AreMeshVerticesOptimised() != null)
                props.Add(AssetProperty.Create("OptimizeVertices", importer.AreMeshVerticesOptimised()));
            if (importer.AreMeshPolygonsOptimised() != null)
                props.Add(AssetProperty.Create("OptimizePolygons", importer.AreMeshPolygonsOptimised()));

            props.Add(AssetProperty.Create("Vertices", totalVertices));
            props.Add(AssetProperty.Create("Triangles", totalPrimitives));
            props.Add(AssetProperty.Create("BlendShapes", totalBlendShapes));
            props.Add(AssetProperty.Create("Animations", importer.clipAnimations.Length));
            props.Add(AssetProperty.Create("Readable", importer.isReadable));

            if (channels != null)
            {
                props.Add(AssetProperty.Create("VertexFormat", string.Join(",", channels.OrderBy(x => x).ToArray())));
            }
        }

        private static void CollectDetailsForTexture2D(List<AssetProperty> props, Texture2D texture, TextureImporter importer)
        {
            props.Add(AssetProperty.Create("Format", texture.format));
            props.Add(AssetProperty.Create("Mipmaps", texture.mipmapCount));
            props.Add(AssetProperty.Create("Width", texture.width));
            props.Add(AssetProperty.Create("Height", texture.height));
            props.Add(AssetProperty.Create("NPOT", !Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height)));
            if ( importer != null )
            {
                props.Add(AssetProperty.Create("Type", importer.textureType));
                props.Add(AssetProperty.Create("CrunchedCompression", importer.crunchedCompression));
                props.Add(AssetProperty.Create("Readable", importer.isReadable));
            }
        }

        private static void CollectDetailsForAudioClip(List<AssetProperty> props, AudioClip audioClip, string path)
        {
            props.Add(AssetProperty.Create("Frequency", audioClip.frequency));
            props.Add(AssetProperty.Create("Length", audioClip.length));
            props.Add(AssetProperty.Create("Channels", audioClip.channels));
            props.Add(AssetProperty.Create("LoadType", UnityVersionAgnostic.GetAudioClipLoadType(audioClip, path)));
            props.Add(AssetProperty.Create("Format", UnityVersionAgnostic.GetAudioClipFormat(audioClip, path)));
            props.Add(AssetProperty.Create("LoadInBackground", audioClip.loadInBackground));
            props.Add(AssetProperty.Create("Preload", audioClip.preloadAudioData));
        }

        private static void CollectDetailsForMaterial(List<AssetProperty> props, Material material)
        {
            var keywords = material.shaderKeywords;
            props.Add(AssetProperty.Create("KeywordsCount", keywords.Length));
            props.Add(AssetProperty.Create("Keywords", string.Join(", ", keywords)));
        }

        internal static int GetPrimitiveCount(Mesh mesh)
        {
            return s_getPrimitiveCount(mesh);
        }

        internal static bool IsGetUsedChannelsSupported
        {
            get { return s_internalShaderChannelType != null && s_hasChannelMethodInfo != null; }
        }

        internal static object[] GetUsedChannels(Mesh mesh)
        {
            List<object> result = new List<object>();
            foreach (var val in System.Enum.GetValues(s_internalShaderChannelType))
            {
                if ((bool)s_hasChannelMethodInfo.Invoke(mesh, new[] { val }))
                {
                    result.Add(val);
                }
            }
            return result.ToArray();
        }

        internal static string[] GetUsedChannelsSanitized(Mesh mesh)
        {
            var channels = GetUsedChannels(mesh);

            return channels
                .Select(x => x.ToString().ToLower())
                .Where(x => x != "vertex")
                .Select(x => x.Replace("texcoord", "uv"))
                .ToArray();
        }

        public Dictionary<string, long> AcquireCalculatedCompressedSizes(int timeout)
        {
            if (m_compressedSizes == null)
            {
                // return empty
                return new Dictionary<string, long>();
            }

            if (m_compressionWorker != null)
            {
                if (!m_compressionWorker.Join(timeout))
                {
                    Log.Warning("Compression worker didn't make it in time: " + timeout);
                }
            }

            Log.Info("Compression worker overhead: {0}", m_compressionWorkerOverhead.Elapsed);

            lock (m_compressedSizes)
            {
                var result = new Dictionary<string, long>(m_compressedSizes);
                m_compressedSizes.Clear();
                return result;
            }
        }

        public List<string> AcquireValidScripts()
        {
            var result = m_scriptsToLookReferencesTo.ToList();
            m_scriptsToLookReferencesTo.Clear();
            return result;
        }

        public void Dispose()
        {
            if (m_compressionWorker != null)
            {
                const int timeout = 1000;
                if (!m_compressionWorker.Join(timeout))
                {
                    Log.Warning("Compression worker didn't make it in time: " + timeout);
                }
            }
        }

        private void BeginCompressionJob(UnityEngine.Object asset, Action<long> onCompleted)
        {
            m_compressionWorkerOverhead.Start();
            try
            {
                string assetName = asset.ToString();
                if (m_compressionWorker.BeginWaitIfBusy(asset, size =>
                    {
                        Log.Debug("CompressedSizeWorker: completed for {0}, size {1}", assetName, size);
                        onCompleted(size);
                    }))
                {
                    Log.Debug("CompressedSizeWorker: running for {0}", asset);
                }
                else
                {
                    Log.Warning("CompressedSizeWorker: unable to get compressed size for {0})", asset);
                }
            }
            finally
            {
                m_compressionWorkerOverhead.Stop();
            }
        }
    }
}
