// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Better.BuildInfo.Internal;
using System.IO;
using System.Reflection;

namespace Better.BuildInfo
{
    public partial class BuildInfoWindow
    {
        private partial class Skin
        {
            public readonly string titleContent = "Better Build Info PRO {0}";
            public readonly string storeUrl = "https://assetstore.unity.com/packages/tools/utilities/better-build-info-pro-report-tool-72579";

            public bool NeedsReload()
            {
                return false;
            }

            public static Texture2D LoadTexture(string name)
            {
                var path = GetResourcePath(name);
                Texture2D asset;
                return AssetHelper.LoadAsset(out asset, path);
            }

            public static string LoadText(string name)
            {
                var path = GetResourcePath(name);
                TextAsset asset;
                AssetHelper.LoadAsset(out asset, path);
                return asset != null ? asset.text : null;
            }


            private GUIStyle[] GetCustomStyles()
            {
                GUISkin skin = null;
                AssetHelper.LoadAsset(out skin, GetResourcePath("BBI_Skin.guiskin"), true);
                return skin.customStyles;
            }

            private static string GetResourcePath(string name)
            {
                return ReliablePath.Combine(BuildInfoPaths.SkinDirectory, name).Replace('\\', '/');
            }
        }
    }

    
}