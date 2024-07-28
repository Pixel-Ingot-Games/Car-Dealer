using Better.BuildInfo.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnitySprite = UnityEngine.Sprite;

namespace Better.BuildInfo.Internal
{
    internal static class BuildInfoProcessorUtils
    {
        public const string CompressedSizeKey = "CompressedSize";

        private static readonly Func<Texture, int> GetStorageMemorySize =
            typeof(Editor).Assembly.GetType("UnityEditor.TextureUtil")
            .CreateMethodDelegateOrThrow<Func<Texture, int>>("GetStorageMemorySize", BindingFlags.Public | BindingFlags.Static);

        public static readonly Func<UnityEngine.Object> GetLightmapSettings =
            typeof(LightmapEditorSettings)
            .CreateMethodDelegateOrThrow<Func<UnityEngine.Object>>("GetLightmapSettings", BindingFlags.NonPublic | BindingFlags.Static);

        private class AssetInfoPathComparer : IComparer<AssetInfo>
        {
            public int Compare(AssetInfo x, AssetInfo y)
            {
                return x.path.CompareTo(y.path);
            }
        }


        public static readonly IComparer<AssetInfo> PathComparer = new AssetInfoPathComparer();

        public static string GenerateAtlasPagePath(string tag, int pageNo, int count)
        {
            return "Sprite Atlas " + tag + " [" + (pageNo + 1) + " of " + count + "]";
        }

        private static IEnumerable<AssetInfo> GetAtlasAssetPages(AssetInfo atlasInfo, BuildInfoAssetDetailsCollector collector)
        {
            // add dependencies to atlas textures manually
            var atlas = AssetDatabase.LoadMainAssetAtPath(atlasInfo.path);
            if (atlas)
            {
                var tag = UnityVersionAgnostic.GetSpriteAtlasTag(atlas);
                var previewTextures = UnityVersionAgnostic.LoadSpriteAtlasTextues(atlas);
                if (previewTextures != null && tag != null)
                {
                    int pageNo = 0;
                    foreach (var texture in previewTextures)
                    {
                        var textureInfo = new AssetInfo()
                        {
                            path = GenerateAtlasPagePath(tag, pageNo, previewTextures.Length),
                            spritePackerPage = pageNo,
                            spritePackerTag = tag,
                            size = GetStorageMemorySize(texture),
                            scenes = atlasInfo.scenes.ToList(),
                        };

                        if (collector != null)
                        {
                            Log.Debug("Collecting details for asset: {0}", textureInfo.path);
                            List<AssetProperty> details = new List<AssetProperty>();
                            collector.CollectForAsset(details, texture, textureInfo.path);
                            textureInfo.details = details.ToArray();
                        }

                        yield return textureInfo;
                        ++pageNo;
                    }
                }
                else
                {
                    Log.Warning("No textures found for atlas {0}", atlas);
                }
            }
        }

        public static void DiscoverDependenciesAndMissingAtlases(List<AssetInfo> assetsInfo, Dictionary<string, AssetInfo> assetsUsedByScenes, BuildInfoAssetDetailsCollector collector)
        {
            using (ProfileSection.Create("Resolving dependencies"))
            {
                Dictionary<string, AssetInfo> discoveredAtlases = new Dictionary<string, AssetInfo>();

                Action<UnitySprite, string> legacySpriteAtlasHandler = null;

                if (UnityVersionAgnostic.IsUsingLegacySpriteAtlases)
                {
                    legacySpriteAtlasHandler = CreateLegacyAtlasHandler((atlasPageName, atlasPage) =>
                    {
                        AssetInfo entry;
                        if (assetsUsedByScenes.TryGetValue(atlasPageName, out entry))
                            return entry;
                        else if (discoveredAtlases.TryGetValue(atlasPageName, out entry))
                            return entry;

                        entry = new AssetInfo()
                        {
                            path = atlasPageName
                        };
                        discoveredAtlases.Add(atlasPageName, entry);

                        if (collector != null)
                        {
                            List<AssetProperty> details = new List<AssetProperty>();
                            Log.Debug("Collecting details for asset: {0}", atlasPageName);
                            collector.CollectForAsset(details, atlasPage, atlasPageName);
                            entry.details = details.ToArray();
                        }

                        return entry;
                    });
                }

                // now resolve dependencies and references
                foreach (var assetInfo in assetsInfo)
                {
                    Log.Debug("Collecting dependencies for asset: {0}", assetInfo.path);

                    var dependencies = AssetDatabase.GetDependencies(new[] { assetInfo.path });

                    bool isPossiblyLeakingLegacyAtlasses =
                        legacySpriteAtlasHandler != null &&
                        assetInfo.scenes.Count == 0 &&
                        (assetInfo.path.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase) >= 0) &&
                        (assetInfo.path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) || assetInfo.path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase));

                    // new sprites need to load their data manually
                    if (UnityVersionAgnostic.IsUsingSpriteAtlases && assetInfo.path.EndsWith(".spriteatlas"))
                    {
                        foreach (var pageInfo in GetAtlasAssetPages(assetInfo, collector))
                        {
                            if (discoveredAtlases.ContainsKey(pageInfo.path))
                            {
                                Log.Warning("Atlas already discovered: {0}", pageInfo.path);
                            }
                            else
                            {
                                discoveredAtlases.Add(pageInfo.path, pageInfo);
                                assetInfo.dependencies.Add(pageInfo.path);
                            }
                        }

                        assetInfo.dependencies.Sort();
                    }

                    bool mayHaveAnySpritesFromAtlases = false;

                    foreach (var dependency in dependencies)
                    {
                        if (dependency == assetInfo.path)
                            continue;

                        int dependencyIndex = assetInfo.dependencies.BinarySearch(dependency);
                        if (dependencyIndex < 0)
                        {
                            assetInfo.dependencies.Insert(~dependencyIndex, dependency);
                        }

                        if (!isPossiblyLeakingLegacyAtlasses)
                            continue;

                        var textureImporter = AssetImporter.GetAtPath(dependency) as TextureImporter;
                        if (textureImporter != null && textureImporter.qualifiesForSpritePacking)
                        {
                            // oh noes
                            mayHaveAnySpritesFromAtlases = true;
                        }
                    }

                    if (mayHaveAnySpritesFromAtlases)
                    {
                        Log.Debug("Asset {0} may be leaking some texture atlases, going to do a slow check", assetInfo.path);

                        // sad panda
                        var asset = AssetDatabase.LoadMainAssetAtPath(assetInfo.path);
                        if (!asset)
                        {
                            Log.Warning("Unable to do a slow texture atlas check for {0}", assetInfo.path);
                        }
                        else
                        {
                            var assetDependencies = EditorUtility.CollectDependencies(new[] { asset })
                                .OfType<UnitySprite>();

                            foreach (var sprite in assetDependencies)
                            {
                                var spriteAssetPath = AssetDatabase.GetAssetPath(sprite);
                                if (!string.IsNullOrEmpty(spriteAssetPath))
                                {
                                    legacySpriteAtlasHandler(sprite, spriteAssetPath);
                                }
                            }
                        }
                    }
                }

                assetsInfo.AddRange(discoveredAtlases.Values);
            }
        }

        public class BundleOpenInfo
        {
            public string name;
            public Action<string> copyTo;
        }

        private static ILookup<string, UnityEngine.Object> GetSpriteToAtlas()
        {
            var spriteAtlases = UnityEditor.AssetDatabase.FindAssets("t:spriteatlas")
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(x))
                .ToList();

            Dictionary<SpriteAtlas, string[]> atlasToSprites = new Dictionary<SpriteAtlas, string[]>();
            foreach (var atlas in spriteAtlases)
            {
                var sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);
                atlasToSprites.Add(atlas, sprites.Select(x => x.texture).Select(x => AssetDatabase.GetAssetPath(x)).Distinct().ToArray());
            }

            var spriteToAtlas = atlasToSprites.SelectMany(x => x.Value.Select(s => new { sprite = s, atlas = x.Key }))
                .ToLookup(x => x.sprite, x => (UnityEngine.Object)x.atlas);

            return spriteToAtlas;
        }

        internal static Dictionary<string, UnityEngine.Object[]> GetActualDependencies(string assetPath, string assetName, YamlClassId classId)
        {
            var dependencies = AssetDatabase.GetDependencies(assetPath);
            var dict = dependencies.ToDictionary(x => x, x => AssetDatabase.LoadAllAssetsAtPath(x));

            if (classId == YamlClassId.Sprite)
            {
                var spriteAtlases = UnityEditor.AssetDatabase.FindAssets("t:spriteatlas")
                    .Select(x => AssetDatabase.GUIDToAssetPath(x))
                    .Select(x => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(x))
                    .ToList();

                Dictionary<SpriteAtlas, string[]> atlasToSprites = new Dictionary<SpriteAtlas, string[]>();
                foreach (var atlas in spriteAtlases)
                {
                    var sprites = new Sprite[atlas.spriteCount];
                    atlas.GetSprites(sprites);
                    atlasToSprites.Add(atlas, sprites.Select(x => x.texture).Select(x => AssetDatabase.GetAssetPath(x)).Distinct().ToArray());
                }

                var spriteToAtlas = atlasToSprites.SelectMany(x => x.Value.Select(s => new { sprite = s, atlas = x.Key }))
                    .ToLookup(x => x.sprite, x => x.atlas);

                HashSet<string> extraDependencies = new HashSet<string>();

                foreach (var atlas in spriteToAtlas[assetPath])
                {
                    extraDependencies.Add(AssetDatabase.GetAssetPath(atlas));
                }

                foreach (var dep in extraDependencies)
                {
                    if (!dict.ContainsKey(dep))
                    {
                        dict.Add(dep, AssetDatabase.LoadAllAssetsAtPath(dep));
                    }
                }
            }

            return dict;
        }

        internal static List<AssetInfo> AnalyzeAssetBundle(IEnumerable<BundleOpenInfo> streams, out List<AssetBundleInfo> bundles)
        {
            var parser = new AssetBundleParser();
            var editorPath = ReliablePath.Combine(EditorApplication.applicationContentsPath, "Tools");
            var discoveredBundles = new List<AssetBundleInfo>();
            var settings = BuildInfoSettings.Instance;

            var allAssets = new Dictionary<AssetBundleParser.AssetId, List<AssetBundleParser.Asset>>();

            // first, parse all the bundles

            Parallel.ForEach(streams,
                () => new List<AssetBundleParser.AssetBundle>(),
                (source, state, list) =>
                {
                    var bundleName = source.name;
                    var copyTo = source.copyTo;
                    try
                    {
                        Log.Debug("Analyzing asset bundle: {0}", bundleName);
                        list.Add(parser.Parse(bundleName, copyTo, editorPath));
                    } catch {}
                    return list;
                },
                list =>
                {
                    lock (allAssets)
                    {
                        foreach (var bundle in list)
                        {
                            IEnumerable<AssetBundleParser.Asset> assetsToAdd = bundle.assets;
                            if ( bundle.isSceneBundle )
                            {
                                Log.Warning("{0} is a scene bundle, referenced assets are not going to get identified", bundle.name);
                                assetsToAdd = bundle.assets.Where(x => !string.IsNullOrEmpty(x.assetPath));
                            }

                            foreach (var asset in assetsToAdd)
                            {
                                // duplicates will happen in case of variants
                                List<AssetBundleParser.Asset> instances;
                                if (!allAssets.TryGetValue(asset.id, out instances))
                                {
                                    instances = new List<AssetBundleParser.Asset>();
                                    allAssets.Add(asset.id, instances);
                                }
                                instances.Add(asset);
                            }
                            discoveredBundles.Add(new AssetBundleInfo()
                            {
                                path = bundle.name,
                                size = bundle.compressedSize,
                            });
                        }
                    }
                }
            );

            // now that we've got all the assets from all the bundles, try to resolve the unexported ones;
            // first, build reverse dependency graph

            var identifiedAssets = new Queue<AssetBundleParser.Asset>();
            foreach (var asset in allAssets.Values.SelectMany(x => x))
            {
                // an unknown asset, ignore for now
                if (string.IsNullOrEmpty(asset.assetPath))
                    continue;

                // paths in bundles are all lowercase, lookup the asset in the database to get proper case
                var guid = AssetDatabase.AssetPathToGUID(asset.assetPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.Equals(path, asset.assetPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        asset.assetPath = path;
                    }
                }

                identifiedAssets.Enqueue(asset);
            }

            var builtInAssets = new[] { "Library/unity default resources", "Resources/unity_builtin_extra" }
                .ToDictionary(x => x, x => AssetDatabase.LoadAllAssetsAtPath(x), StringComparer.InvariantCultureIgnoreCase);

            // traverse references and guess where implicit assets come from
            var spriteToAtlasLookup = new Lazy<ILookup<string, UnityEngine.Object>>(GetSpriteToAtlas);
            var guessedAssets = new HashSet<AssetBundleParser.Asset>();
            while (identifiedAssets.Count > 0)
            {
                var asset = identifiedAssets.Dequeue();
                
                // try to decode unidentified references
                Dictionary<string, UnityEngine.Object[]> allDependencies = null;
                List<AssetBundleParser.Asset> spriteAtlasPages = null;

                foreach (var referenceId in asset.references)
                {
                    List<AssetBundleParser.Asset> references;
                    if (!allAssets.TryGetValue(referenceId, out references))
                    {
                        if (!builtInAssets.ContainsKey(referenceId.archivePath))
                        {
                            Log.Warning("Unknown reference in {0}: {1}", asset.assetPath, referenceId);
                        }
                        continue;
                    }

                    foreach (var reference in references)
                    {
                        if (!string.IsNullOrEmpty(reference.assetPath))
                            continue;

                        if (allDependencies == null)
                        {
                            Log.Debug("Loading all dependencies for {0}", asset.assetPath);

                            allDependencies = AssetDatabase.GetDependencies(asset.assetPath)
                                .ToDictionary(x => x, x => AssetDatabase.LoadAllAssetsAtPath(x));

                            if (asset.classID == YamlClassId.Sprite)
                            {
                                // sprites seem to have implict reference to their atlas
                                foreach (var atlas in spriteToAtlasLookup.Value[asset.assetPath])
                                {
                                    var atlasPath = AssetDatabase.GetAssetPath(atlas);
                                    if (!string.IsNullOrEmpty(atlasPath) && !allDependencies.ContainsKey(atlasPath))
                                    {
                                        allDependencies.Add(atlasPath, AssetDatabase.LoadAllAssetsAtPath(atlasPath));
                                    }
                                }
                            }
                        }

                        var referenceType = reference.classID.ToType();

                        // now to find matching asset
                        string candidateAssetPath = null;

                        foreach (var source in new[] { allDependencies, builtInAssets })
                        {
                            foreach (var kv in source)
                            {
                                foreach (var obj in kv.Value)
                                {
                                    if (obj == null)
                                    {
                                        continue;
                                    }
                                    if (obj.name == reference.name)
                                    {
                                        // does type match?
                                        if (referenceType == null || referenceType == obj.GetType())
                                        {
                                            if (candidateAssetPath == null)
                                            {
                                                candidateAssetPath = kv.Key;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Log.Warning("Does not match: " + referenceType + " vs " + obj.GetType());
                                        }
                                    }
                                }
                            }
                        }

                        if (candidateAssetPath == null)
                        {
                            // fallbacks for some specialised cases
                            if (asset.classID == YamlClassId.SpriteAtlas && reference.classID == YamlClassId.Texture2D && reference.name.Contains("-" + asset.name + "-") && reference.bundle == asset.bundle)
                            {
                                // will resolve this guy later
                                if (spriteAtlasPages == null)
                                {
                                    spriteAtlasPages = new List<AssetBundleParser.Asset>();
                                }
                                spriteAtlasPages.Add(reference);
                                continue;
                            }
                            else
                            {
                                Log.Warning("UnGuessed asset path for {0} ({2}): {1}", referenceId, reference.name, reference.classID);
                            }
                        }
                        else
                        {
                            Log.Debug("Guessed asset path for {0} ({2}): {1}", referenceId, candidateAssetPath, reference.classID);
                            reference.assetPath = candidateAssetPath;
                            identifiedAssets.Enqueue(reference);
                            guessedAssets.Add(reference);
                        }
                    }

                }

                if (spriteAtlasPages != null)
                {
                    for (int i = 0; i < spriteAtlasPages.Count; ++i)
                    {
                        var page = spriteAtlasPages[i];
                        page.assetPath = GenerateAtlasPagePath(asset.name, i, spriteAtlasPages.Count);
                        Log.Debug("Guessed asset path for {0} ({2}): {1}", page.id, page.assetPath, page.classID);
                        identifiedAssets.Enqueue(page);
                        guessedAssets.Add(page);
                    }
                }
            }

            var infos = new List<AssetInfo>();

            // now that refs have been fixed (where possible), group by paths
            foreach (var group in allAssets.Values.SelectMany(x => x).GroupBy(x => x.assetPath))
            {
                if (string.IsNullOrEmpty(group.Key))
                {
                    Log.Warning("Following assets were not identified:\n{0}",
                        string.Join("\n", group.Select(x => string.Format("{0} {1} {2} {3}", x.id, x.classID, x.name, x.bundle.name))));
                    continue;
                }

                var info = new AssetInfo()
                {
                    path = group.Key,
                    assetBundles = new List<AssetAssetBundleEntry>(),
                };

                foreach (var asset in group)
                {
                    var bundleEntry = new AssetAssetBundleEntry()
                    {
                        implicitCopy = guessedAssets.Contains(asset),
                        size = asset.estimatedSize,
                        name = asset.bundle.name
                    };
                    info.assetBundles.Add(bundleEntry);
                }

                info.dependencies = group.SelectMany(asset =>
                    {
                        List<string> result = new List<string>();
                        foreach (var referenceId in asset.references)
                        {
                            List<AssetBundleParser.Asset> references;
                            if ( allAssets.TryGetValue(referenceId, out references))
                            {
                                // we don't want variants to cross reference each other, so if there's a reference
                                // living in the same bundle, forget everything else
                                var sameBundleReference = references.SingleOrDefault(x => x.bundle == asset.bundle);
                                if ( sameBundleReference != null)
                                {
                                    result.Add(sameBundleReference.assetPath);
                                }
                                else
                                {
                                    foreach (var reference in references)
                                    {
                                        result.Add(reference.assetPath);
                                    }
                                }
                            }
                        }
                        return result;
                    })
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Where(x => x != group.Key)
                    .Distinct()
                    .ToList();

                infos.Add(info);
            }

            bundles = discoveredBundles;
            return infos;
        }
        public static Action<UnitySprite, string> CreateLegacyAtlasHandler(Func<string, UnityEngine.Object, AssetInfo> getOrCreateEntry)
        {
            Func<string, Texture2D[]> getAtlasTexturesCached;
            {
                var cache = new Dictionary<string, Texture2D[]>();
                getAtlasTexturesCached = (tag) =>
                {
                    Texture2D[] pages;
                    if (!cache.TryGetValue(tag, out pages))
                    {
                        pages = UnityEditor.Sprites.Packer.GetTexturesForAtlas(tag);
                    }
                    return pages;
                };
            }

            Dictionary<UnitySprite, AssetInfo> spriteToPageShortCircut = new Dictionary<UnitySprite, AssetInfo>();

            return (sprite, spriteParentPath) =>
            {
                AssetInfo pageEntry;

                if (!spriteToPageShortCircut.TryGetValue(sprite, out pageEntry))
                {
                    if (string.IsNullOrEmpty(spriteParentPath))
                    {
                        spriteToPageShortCircut.Add(sprite, null);
                        return;
                    }

                    var importer = (TextureImporter)AssetImporter.GetAtPath(spriteParentPath);
                    if (!importer || !importer.qualifiesForSpritePacking)
                    {
                        spriteToPageShortCircut.Add(sprite, null);
                        return;
                    }

                    string atlasTag;
                    Texture2D atlasTexture;
                    UnityEditor.Sprites.Packer.GetAtlasDataForSprite(sprite, out atlasTag, out atlasTexture);

                    if (!atlasTexture)
                    {
                        spriteToPageShortCircut.Add(sprite, null);
                        return;
                    }

                    var atlasTextures = getAtlasTexturesCached(atlasTag);
                    var pageNumber = Array.IndexOf(atlasTextures, atlasTexture);
                    if (pageNumber < 0)
                    {
                        Log.Error("Unable to find texture {0} in atlas {1}", atlasTexture, atlasTag);
                        spriteToPageShortCircut.Add(sprite, null);
                        return;
                    }

                    var uniqueName = "Sprite Atlas " + atlasTag + " [" + (pageNumber + 1) + " of " + atlasTextures.Length + "]";
                    pageEntry = getOrCreateEntry(uniqueName, atlasTextures[pageNumber]);
                    if (pageEntry == null)
                    {
                        spriteToPageShortCircut.Add(sprite, null);
                        return;
                    }

                    spriteToPageShortCircut.Add(sprite, pageEntry);
                    if (string.IsNullOrEmpty(pageEntry.spritePackerTag))
                    {
                        pageEntry.spritePackerPage = pageNumber;
                        pageEntry.spritePackerTag = atlasTag;
                        pageEntry.size = GetStorageMemorySize(atlasTextures[pageNumber]);
                    }
                }

                if (pageEntry != null)
                {
                    var dependencyIndex = pageEntry.dependencies.BinarySearch(spriteParentPath);
                    if (dependencyIndex < 0)
                    {
                        pageEntry.dependencies.Insert(~dependencyIndex, spriteParentPath);
                    }
                }
            };
        }

        public static void FinishCollectingDetails(List<AssetInfo> assetsInfo, BuildInfoAssetDetailsCollector collector)
        {
            using (ProfileSection.Create("Collecting Assets' Details"))
            {
                var details = new List<AssetProperty>();
                foreach (var assetInfo in assetsInfo)
                {
                    if (assetInfo.details != null)
                    {
                        continue;
                    }

                    Log.Debug("Load main asset at: {0}", assetInfo.path);
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetInfo.path);

                    if (mainAsset != null)
                    {
                        details.Clear();

                        Log.Debug("Collecting details for asset: {0}", assetInfo.path);
                        if (collector.CollectForAsset(details, mainAsset, assetInfo.path))
                        {
                            assetInfo.details = details.ToArray();
                        }
                    }
                }
            }

            using (ProfileSection.Create("Cleaning Up Assets' Details"))
            {
                // sanitize and sort stuff
                foreach (var assetInfo in assetsInfo)
                {
                    assetInfo.details = BuildInfoProcessorUtils.CleanUpAssetsDetails(assetInfo.details, assetInfo.path);
                }
            }
        }

        public static AssetProperty[] CleanUpAssetsDetails(AssetProperty[] details, string assetPath)
        {
            if (details == null || details.Length == 0)
                return details;

            Array.Sort(details, (a, b) => a.name.CompareTo(b.name));

            for (int i = 1; i < details.Length; ++i)
            {
                if (details[i - 1].name == details[i].name)
                {
                    Log.Warning("Removing duplicate asset property {0}, keeping {1}, discarding {2}", details[i].name, details[i - 1].value, details[i].value, assetPath);
                    ArrayUtility.RemoveAt(ref details, i);
                    --i;
                }
            }

            return details;
        }

        public static IEnumerable<BuildSetting> GetPlayerSettings(Type type)
        {
            var forbiddenProperties = new[]
            {
                "keystorePass",
                "keyaliasPass"
            };

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                // don't yield password!
                if (Array.IndexOf(forbiddenProperties, prop.Name) >= 0)
                    continue;

                string value;
                if (IsSameOrSubclass(typeof(UnityEngine.Object), prop.PropertyType))
                {
                    // output path
                    var unityValue = (UnityEngine.Object)prop.GetValue(null, null);
                    if (unityValue)
                    {
                        value = AssetDatabase.GetAssetPath(unityValue);
                    }
                    else
                    {
                        value = string.Empty;
                    }
                }
                else
                {
                    var val = prop.GetValue(null, null);
                    if (val != null)
                    {
                        if (prop.PropertyType.IsArray)
                        {
                            value = string.Join(";",
                                ((System.Collections.IEnumerable)val)
                                .Cast<object>()
                                .Select(x => (x ?? string.Empty).ToString())
                                .ToArray()
                            );
                        }
                        else
                        {
                            value = val.ToString();
                        }
                    }
                    else
                    {
                        value = string.Empty;
                    }
                }

                yield return new BuildSetting()
                {
                    name = prop.Name,
                    value = value
                };
            }
        }

        private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        private static long UpdateCompressedSize(AssetInfo assetInfo, long compressedSize, bool addSize = true)
        {
            if (compressedSize <= 0)
                return 0;

            long currentSize;
            GetCustomProperty(assetInfo, CompressedSizeKey, long.TryParse, out currentSize);

            if (addSize)
                compressedSize += currentSize;

            SetCustomProperty(assetInfo, CompressedSizeKey, compressedSize);

            return currentSize;
        }

        private delegate bool ParseDelegate<T>(string str, out T value);

        private static bool GetCustomProperty<T>(AssetInfo assetInfo, string key, ParseDelegate<T> conversion, out T value)
        {
            value = default(T);
            if (assetInfo.details == null)
                return false;

            var existing = assetInfo.details.FirstOrDefault(x => x.name == key);
            if (existing != null)
            {
                return conversion(existing.value, out value);
            }
            else
            {
                return false;
            }
        }

        private static void SetCustomProperty<T>(AssetInfo assetInfo, string key, T value)
        {
            if (assetInfo.details == null)
                assetInfo.details = new AssetProperty[0];

            var existing = assetInfo.details.FirstOrDefault(x => x.name == key);
            if (existing != null)
            {
                existing.value = value.ToString();
            }
            else
            {
                ArrayUtility.Add(ref assetInfo.details, AssetProperty.Create(key, value));
            }
        }

        public static void FinishCalculatingCompressedSizes(List<AssetInfo> infos, BuildInfoAssetDetailsCollector collector)
        {
            var compressedSizes = collector.AcquireCalculatedCompressedSizes(15000);
            foreach (var kv in compressedSizes)
            {
                var index = FindInfoIndexByPath(infos, kv.Key);
                if (index >= 0)
                {
                    long prevSize = UpdateCompressedSize(infos[index], kv.Value, addSize: false);
                    if (prevSize != 0 && prevSize != kv.Value)
                    {
                        Log.Debug("Calculated and actual compressed size are different: {0} vs {1}, using the former", prevSize, kv.Value);
                        UpdateCompressedSize(infos[index], prevSize, addSize: false);
                    }
                }
                else
                {
                    Log.Warning("Calculated compressed size for {0}, but not found in resulting assets.", kv.Key);
                }
            }
        }

        public static void CalculateScriptReferences(List<AssetInfo> infos, BuildInfoAssetDetailsCollector collector)
        {
            // count asset references
            var scripts = collector.AcquireValidScripts().ToDictionary(x => x, x => 0);
            foreach (var info in infos)
            {
                foreach (var dep in info.dependencies)
                {
                    int count;
                    if (scripts.TryGetValue(dep, out count))
                    {
                        scripts[dep] = ++count;
                    }
                }
            }

            foreach (var script in scripts)
            {
                var scriptInfoIndex = FindInfoIndexByPath(infos, script.Key);
                if (scriptInfoIndex < 0)
                    throw new InvalidOperationException("Unable to find script's index:  " + script.Key);

                var scriptInfo = infos[scriptInfoIndex];
                SetCustomProperty(scriptInfo, "ScriptReferences", scriptInfo.scenes.Count + script.Value);
            }
        }

        public static void RefreshOtherArtifacts(List<AssetInfo> sortedInfos, BuildArtifactsInfo artifactsInfo)
        {
            System.Action<string, long, long> forceUpdateAsset = (assetPath, uncompressed, compressed) =>
            {
                if (uncompressed == 0)
                    return;

                int index = FindInfoIndexByPath(sortedInfos, assetPath);
                if (index < 0)
                {
                    // "unity default resources" seems to move around between releases... so override here, just in case
                    if (assetPath.EndsWith("unity default resources"))
                    {
                        var overrideInfo = new AssetInfo() { path = "Library/unity default resources" };
                        var overrideIndex = sortedInfos.BinarySearch(overrideInfo, PathComparer);
                        if (overrideIndex >= 0)
                        {
                            index = overrideIndex;
                        }
                    }
                }

                AssetInfo info;
                if (index >= 0)
                {
                    info = sortedInfos[index];
                }
                else
                {
                    Log.Debug("Unknown asset: {0}, adding from build artifacts with zero dependencies", assetPath);
                    info = new AssetInfo() { path = assetPath };
                    sortedInfos.Insert(~index, info);
                }

                if (uncompressed > 0)
                {
                    Log.Debug("Overriding {0} size from {1} to {2} based on build artifacts", assetPath, info.size, uncompressed);
                    info.size = uncompressed; // questionable
                }

                long prevCompressedSize = UpdateCompressedSize(info, compressed);
                if (prevCompressedSize > 0)
                {
                    Log.Debug("Already had compressed size for {0}: {1} vs {2}", assetPath, prevCompressedSize, compressed);
                }
            };

            foreach (var res in artifactsInfo.unityResources)
            {
                forceUpdateAsset(res.Key, res.Value.uncompressed, res.Value.compressed);
            }

            foreach (var asset in artifactsInfo.otherAssets)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(asset.Key);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                forceUpdateAsset(assetPath, asset.Value.uncompressed, asset.Value.compressed);
            }
        }

        public static void RefreshScenesInfo(Dictionary<string, long> scenesSizes, List<AssetInfo> infos, BuildArtifactsInfo artifactsInfo, List<string> processedScenes, List<AssetProperty[]> scenesDetails, BuildInfoAssetDetailsCollector collector)
        {
            Dictionary<string, AssetInfo> scenesInfo = new Dictionary<string, AssetInfo>();

            // if there are missing scenes... add them
            foreach (var scenePath in processedScenes.Distinct())
            {
                var info = new AssetInfo()
                {
                    path = scenePath,
                    scenes = { scenePath }
                };

                long size;
                scenesSizes.TryGetValue(scenePath, out size);
                info.size = size;

                scenesInfo.Add(scenePath, info);
            }

            if (collector != null)
            {
                foreach (var sceneInfo in scenesInfo.Values)
                {
                    var index = processedScenes.FindIndex(x => string.Compare(x, sceneInfo.path, true) == 0);
                    if (index >= 0)
                        sceneInfo.details = CleanUpAssetsDetails(scenesDetails[index], sceneInfo.path);
                    else
                        Log.Warning("Unable to find details for level {0}", sceneInfo.path);
                }
            }

            infos.AddRange(scenesInfo.Values);


            if (artifactsInfo != null)
            {
                for (int i = 0; i < artifactsInfo.sceneSizes.Count; ++i)
                {
                    if (i >= processedScenes.Count)
                    {
#if !UNITY_2017_1_OR_NEWER
                        Log.Warning("Detected more scenes in the build than been notified of ({0} vs {1})", artifactsInfo.sceneSizes.Count, processedScenes.Count);
#endif
                        break;
                    }

                    if (artifactsInfo.sceneSizes[i].uncompressed == 0)
                    {
                        Log.Warning("No size for {0}", processedScenes[i]);
                    }

                    AssetInfo sceneInfo = scenesInfo[processedScenes[i]];

                    sceneInfo.size = System.Math.Max(artifactsInfo.sceneSizes[i].uncompressed, sceneInfo.size);
                    UpdateCompressedSize(sceneInfo, artifactsInfo.sceneSizes[i].compressed);
                }
            }
        }

        public static void RefreshStreamingAssetsInfo(List<AssetInfo> infos, BuildArtifactsInfo artifactsInfo)
        {
            var suspects = infos.Where(x => x.path.StartsWith("Assets/StreamingAssets/", StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.path);

            foreach (var kv in artifactsInfo.streamingAssets)
            {
                var fixedPath = "Assets/StreamingAssets/" + kv.Key;
                if (suspects.ContainsKey(fixedPath))
                {
                    Log.Error("Asset {0} seems to be a streaming asset, ignoring the actual streaming asset", fixedPath);
                }
                else
                {
                    var entry = new AssetInfo()
                    {
                        path = fixedPath,
                        size = kv.Value.uncompressed
                    };
                    UpdateCompressedSize(entry, kv.Value.compressed);
                    infos.Add(entry);
                }
            }
        }

        public static void RefreshModulesInfo(List<AssetInfo> infos, BuildArtifactsInfo artifactsInfo)
        {
            if (artifactsInfo.managedModules.Count <= 0)
                return;

            var modulesSuspects = infos.Where(x => x.path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                    .ToLookup(x => ReliablePath.GetFileName(x.path));

            // add modules
            foreach (var kv in artifactsInfo.managedModules)
            {
                var existingEntries = modulesSuspects[kv.Key].ToList();
                if (existingEntries.Any())
                {
                    foreach (var entry in existingEntries)
                    {
                        entry.size += kv.Value.uncompressed;
                        UpdateCompressedSize(entry, kv.Value.compressed);
                    }
                }
                else
                {
                    var entry = new AssetInfo()
                    {
                        path = kv.Key,
                        size = kv.Value.uncompressed,
                    };
                    UpdateCompressedSize(entry, kv.Value.compressed);
                    infos.Add(entry);
                }
            }
        }

        private static int FindInfoIndexByPath(List<AssetInfo> infos, string path)
        {
            if (infos == null)
            {
                throw new System.ArgumentNullException("infos");
            }
            int lo = 0;
            int hi = infos.Count - 1;
            while (lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order = infos[i].path.CompareTo(path);

                if (order == 0)
                {
                    return i;
                }

                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

    }
}
