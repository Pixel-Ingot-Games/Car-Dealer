#if UNITY_2018_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnitySprite = UnityEngine.Sprite;

namespace Better.BuildInfo.Internal
{
    public static class BuildReportReader
    {
        private const string SpriteAtlasPrefix = "Built-in Texture2D: ";

        public static Dictionary<string, long> CollectUsedAssets(BuildReport buildReport)
        {
            var serializedObject = new SerializedObject(buildReport);
            var appendices = serializedObject.FindPropertyOrThrow("m_Appendices");
            var assets = new Dictionary<string, long>();

            for (int i = 0; i < appendices.arraySize; i++)
            {
                var appendix = appendices.GetArrayElementAtIndex(i);

                var appendixObj = appendix.objectReferenceValue;
                if (appendixObj?.GetType() != typeof(UnityEngine.Object))
                {
                    // 2019_3 fix
                    if (appendixObj?.GetType().Name.Equals("PackedAssets") != true)
                    {
                        continue;
                    }
                    
                }

                var appendixSO = new SerializedObject(appendixObj);

                var contents = appendixSO.FindProperty("m_Contents");
                if (contents == null)
                    continue;

                for (int j = 0; j < contents.arraySize; j++)
                {
                    var assetEntry = contents.GetArrayElementAtIndex(j);

                    var assetPathProp = assetEntry.FindPropertyRelative("buildTimeAssetPath");
                    var assetSizeProp = assetEntry.FindPropertyRelative("packedSize");

                    if (assetPathProp == null || assetSizeProp == null)
                        continue;

                    var assetPath = assetPathProp.stringValue;
                    var assetSize = assetSizeProp.intValue;

                    if (string.IsNullOrEmpty(assetPath))
                        continue;

                    if (assetPath.StartsWith(SpriteAtlasPrefix))
                    {
                        // ignore this, on Android size seems ok, but on PC & iOS it's complete bollocks
                        // 2017_3_1: for new sprites atlases, this entry "merges" by resolution, which is utterly useless
                        continue;
                    }

                    long existingSize;
                    if (assets.TryGetValue(assetPath, out existingSize))
                    {
                        assets[assetPath] += assetSize;
                    }
                    else
                    {
                        assets.Add(assetPath, assetSize);
                    }
                }
            }

            return assets;
        }
    }
}
#endif