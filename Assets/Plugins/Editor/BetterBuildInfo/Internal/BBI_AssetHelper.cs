// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    /// <summary>
    /// For Unity4 compability
    /// </summary>
    internal static class AssetHelper
    {
        public static T LoadAsset<T>(out T asset, string path) where T : UnityEngine.Object
        {
            return LoadAsset(out asset, path, false);
        }

        public static T LoadAsset<T>(out T asset, string path, bool throwIfNotFound) where T : UnityEngine.Object
        {
            asset = (T)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if ( throwIfNotFound && !asset )
            {
                throw new System.ArgumentOutOfRangeException("Asset not found: " + path);
            }
            return asset;
        }
        

        public static IEnumerable<T> FindAssetsOfType<T>()
        {
            var result = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                .SelectMany(x => UnityEditor.AssetDatabase.LoadAllAssetsAtPath(x).OfType<T>());

            return result;
        }
    }
}
