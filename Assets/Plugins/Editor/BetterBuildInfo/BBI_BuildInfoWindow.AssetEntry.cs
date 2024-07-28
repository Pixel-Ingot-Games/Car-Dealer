// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;
using Better.BuildInfo.Internal;
using System.Reflection;

namespace Better.BuildInfo
{
    public partial class BuildInfoWindow
    {
        [Serializable]
        private class AssetEntry
        {
            public AssetInfo info;
            public int categoryIndex;
            public BitArray scenesAndBundlesMask;
            public FileSize size;
            public FileSize sizeDiff;


            [NonSerialized]
            public bool selected;

            [NonSerialized]
            private GUIContent m_richTextContent;
            [NonSerialized]
            private GUIContent m_shortTextContentWithTooltip;
            [NonSerialized]
            private GUIContent m_sizeContent;
            [NonSerialized]
            private GUIContent m_bundledSizeContent;
            [NonSerialized]
            private GUIContent m_sizeDiffContent;
            [NonSerialized]
            private GUIContent m_sizeWithDepsContent;
            [NonSerialized]
            private GUIContent m_totalSizeContent;


            [NonSerialized]
            private UnityEngine.Object m_asset;
            [NonSerialized]
            private Texture2D m_icon;
            [NonSerialized]
            private Texture2D m_thumbnail;

            [NonSerialized]
            private FileSize? m_bundledSize;

            [NonSerialized]
            private bool m_retreivedAsset;

            [NonSerialized]
            public int controlId;

            [NonSerialized]
            public List<AssetEntry> dependencies = new List<AssetEntry>();
            [NonSerialized]
            public List<AssetEntry> references = new List<AssetEntry>();
            [NonSerialized]
            public List<AssetEntry> scenes = new List<AssetEntry>();
            [NonSerialized]
            public List<AssetEntry> assetBundles = new List<AssetEntry>();

            public FileSize dependenciesSize
            {
                get { return FileSize.FromKiloBytes(dependencies.Sum(x => x.size.KiloBytes)); }
            }

            public FileSize totalSize
            {
                get { return size + dependenciesSize; }
            }

            public bool PreloadThumbnail()
            {
                if (m_thumbnail != null)
                    return false;

                m_thumbnail = CreatePreview(true);
                if (m_thumbnail == m_icon)
                    return false;

                return true;
            }

            public bool PreloadAsset()
            {
                if (m_retreivedAsset)
                    return false;

                if (!string.IsNullOrEmpty(info.spritePackerTag))
                {
                    m_asset = UnityVersionAgnostic.LoadSpriteAtlasPreview(info.spritePackerTag, info.spritePackerPage);
                }
                else
                {
                    m_asset = AssetDatabase.LoadMainAssetAtPath(info.path);
                }

                m_retreivedAsset = true;

                return true;
            }

            public bool hasAsset
            {
                get { return m_retreivedAsset && asset != null; }
            }

            public UnityEngine.Object asset
            {
                get
                {
                    PreloadAsset();
                    return m_asset;
                }
            }

            public Texture icon
            {
                get
                {
                    if (m_icon == null)
                    {
                        m_icon = CreatePreview(false);
                    }
                    return m_icon;
                }
            }

            public bool hasThumbnail
            {
                get { return m_thumbnail != null; }
            }

            public Texture thumbnail
            {
                get
                {
                    PreloadThumbnail();
                    return m_thumbnail;
                }
            }

            public GUIContent richTextContent
            {
                get
                {
                    if (m_richTextContent == null)
                    {
                        m_richTextContent = new GUIContent(CreateText(true, true));
                    }
                    return m_richTextContent;
                }
            }

            public GUIContent shortTextContentWithTooltip
            {
                get
                {
                    if (m_shortTextContentWithTooltip == null)
                    {
                        m_shortTextContentWithTooltip = new GUIContent(CreateText(false, false));
                        m_shortTextContentWithTooltip.tooltip = CreateText(false, true);
                    }
                    return m_shortTextContentWithTooltip;
                }
            }

            public GUIContent sizeContent
            {
                get
                {
                    if (m_sizeContent == null)
                    {
                        m_sizeContent = new GUIContent(size.ToString());
                    }
                    return m_sizeContent;
                }
            }

            public GUIContent bundledSizeContent
            {
                get
                {
                    if (m_bundledSizeContent == null)
                    {
                        m_bundledSizeContent = new GUIContent(bundledSize.ToString());
                    }
                    return m_bundledSizeContent;
                }
            }

            public GUIContent sizeDiffContent
            {
                get
                {
                    if (m_sizeDiffContent == null)
                    {
                        if (sizeDiff == FileSize.Zero)
                        {
                            m_sizeDiffContent = new GUIContent();
                        }
                        else
                        {
                            var diff = sizeDiff.Abs().ToString();
                            var prefix = "";
                            if (size == FileSize.Zero)
                            {
                                prefix = Skin.NullWidthSpace + Skin.Cross;
                            }
                            else if ( size == sizeDiff )
                            {
                                prefix = Skin.NullWidthSpace + Skin.Star;
                            }
                            else if (sizeDiff > FileSize.Zero)
                            {
                                prefix = Skin.NullWidthSpace + Skin.UpArrow;
                            }
                            else
                            {
                                prefix = Skin.NullWidthSpace + Skin.DownArrow;
                            }

                            m_sizeDiffContent = new GUIContent(prefix + diff);
                        }
                    }
                    return m_sizeDiffContent;
                }
            }

            public GUIContent dependenciesSizeContent
            {
                get
                {
                    if (m_sizeWithDepsContent == null)
                    {
                        m_sizeWithDepsContent = new GUIContent(dependenciesSize.ToString());
                    }
                    return m_sizeWithDepsContent;
                }
            }

            public GUIContent totalSizeContent
            {
                get
                {
                    if (m_totalSizeContent == null)
                    {
                        m_totalSizeContent = new GUIContent((dependenciesSize + size).ToString());
                    }
                    return m_totalSizeContent;
                }
            }

            public FileSize bundledSize
            {
                get
                {
                    if (m_bundledSize == null)
                    {
                        m_bundledSize = FileSize.FromBytes(info.assetBundles.Select(x => x.size).Sum());
                    }
                    return m_bundledSize.Value;
                }
            }

            private string CreateText(bool rich, bool fullPath)
            {
                var path = info.path;

                if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring("Assets/".Length);
                }

                if (!fullPath)
                {
                    path = ReliablePath.GetFileName(path);
                }

                if (rich)
                {
                    return Skin.RichifyFileName(path);
                }
                else
                {
                    return path;
                }
            }

            private Texture2D CreatePreview(bool thumbnail = true)
            {
                Texture2D result = null;

                if (!string.IsNullOrEmpty(info.spritePackerTag))
                {
                    if (thumbnail)
                    {
                        if (asset != null)
                        {
                            result = AssetPreview.GetMiniThumbnail(asset);
                        }
                    }
                    if (!result)
                    {
                        result = AssetPreview.GetMiniTypeThumbnail(typeof(Texture2D));
                    }
                }
                else
                {
                    string extension = ReliablePath.GetExtension(info.path).ToLower();

                    switch (extension)
                    {
                        case ".ai":
                        case ".apng":
                        case ".png":
                        case ".bmp":
                        case ".cdr":
                        case ".dib":
                        case ".eps":
                        case ".exif":
                        case ".gif":
                        case ".ico":
                        case ".icon":
                        case ".j":
                        case ".j2c":
                        case ".j2k":
                        case ".jas":
                        case ".jiff":
                        case ".jng":
                        case ".jp2":
                        case ".jpc":
                        case ".jpe":
                        case ".jpeg":
                        case ".jpf":
                        case ".jpg":
                        case ".jpw":
                        case ".jpx":
                        case ".jtf":
                        case ".mac":
                        case ".omf":
                        case ".qif":
                        case ".qti":
                        case ".qtif":
                        case ".tex":
                        case ".tfw":
                        case ".tga":
                        case ".tif":
                        case ".tiff":
                        case ".wmf":
                        case ".psd":
                        case ".exr":
                        case ".shadervariants":
                        case ".asset":
                            if (thumbnail)
                            {
                                if (asset != null)
                                {
                                    result = AssetPreview.GetMiniThumbnail(asset);
                                }
                            }
                            break;

                        case ".cubemap":
                            result = AssetPreview.GetMiniTypeThumbnail(typeof(Cubemap));
                            break;

                        case ".xml":
                        case ".txt":
                        case ".bytes":
                            result = AssetPreview.GetMiniTypeThumbnail(typeof(TextAsset));
                            break;
                    }


                    if (result == null)
                    {
                        result = UnityEditorInternal.InternalEditorUtility.GetIconForFile(extension);
                    }
                }

                return result;
            }


            public string[] detailsValues;

            public string GetPropertyValue(int index)
            {
                if (detailsValues == null || detailsValues.Length == 0)
                    return null;

                return detailsValues[index];
            }
        }
    }
}