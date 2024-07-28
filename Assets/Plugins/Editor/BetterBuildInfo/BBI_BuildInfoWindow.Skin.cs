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
        [Serializable]
        private partial class Skin
        {
            [NonSerialized]
            public bool loaded = false;


            public readonly static float MaxSizeMB = 100.0f;

            public readonly GUIContent headerOpenLogContent = new GUIContent("Open Build Log...");
            public readonly GUIContent openReportContent = new GUIContent("Open Report...");
            public readonly GUIContent openReportRefenceContent = new GUIContent("Compare With...");
            public readonly GUIContent optionsContent = new GUIContent("Settings...");
            public readonly GUIContent openRecentContent = new GUIContent("Open Recent...");
            public readonly GUIContent openSettingsContent = new GUIContent("Open");
            public readonly GUIContent reloadSettingsContent = new GUIContent("Reload");
            public readonly GUIContent reloadSkinContent = new GUIContent("Reload Skin");
            public readonly GUIContent reviewContent = new GUIContent("Review");
            public readonly string openStorePageContent = "Open in Asset Store";
            public readonly string dismissContent = "Hide This Button";

            public readonly GUIContent categoriesContent = new GUIContent("Category");
            public readonly GUIContent scenesFilterContent = new GUIContent("Used By");

            public GUIStyle header;
            public GUIStyle pathStyle;
            public GUIStyle largeButtonStyle;
            public GUIStyle closeButtonStyle;

            public GUIStyle entry;
            public GUIStyle entryZeroSize;
            public GUIStyle category;
            public GUIStyle treeMapEntry;

            public GUIStyle row0;
            public GUIStyle row1;
            public GUIStyle selectedRow;

            public GUIStyle toolbarButtonLeftAlignedStyle;
            public GUIStyle labelRightAlignedStyle;
            public GUIStyle miniLabelRightAlignedStyle;
            public GUIStyle labelRightTopAlignedStyle;

            public GUIStyle inspectorBigStyle;
            public GUIStyle changelogStyle;

            public GUIStyle toolbarNoPaddingStyle;

            public GUIStyle centeredGreyMiniLabel;

            public GUIStyle richMenuItem;
            public GUIStyle richLabel;
            public GUIStyle richLabelRightAligned;

            public GUIStyle projectOverlayMask;

            public readonly GUILayoutOption infoColumnWidth = GUILayout.Width(120.0f);
            public readonly GUILayoutOption sizeColumnWidth = GUILayout.Width(70.0f);
            public readonly GUILayoutOption sizeDiffColumnWidth = GUILayout.Width(85.0f);
            public readonly GUILayoutOption bundledSizeColumnWidth = GUILayout.Width(100.0f);
            public readonly GUILayoutOption categoryColumnWidth = GUILayout.Width(120.0f);

            public readonly static string Cross = "\u2716";
            public readonly static string Star = "\u2600";
            public readonly static string UpArrow = "\u25B2";
            public readonly static string DownArrow = "\u25BC";
            public readonly static string MenuSafeSlash = " \u2044 ";
            public readonly static string NullWidthSpace = "\uFEFF";

            public Texture2D iconWindowContent;
            public Texture2D iconHeader;
            public Texture2D iconAssetMissing;
            public Texture2D iconWhiteCircle;

            public readonly Color sizeIncreasedColor = (Color)new Color32(255, 255, 168, 255);
            public readonly Color sizeDecreasedColor = (Color)new Color32(219, 255, 168, 255);
            public readonly Color newItemColor = (Color)new Color32(255, 168, 168, 255);
            public readonly Color deletedItemColor = (Color)new Color32(180, 255, 140, 255);

            public static readonly GUIContent[] ToolbarOptions = 
            {
                new GUIContent("Overview"),
                new GUIContent("Used Assets"),
                new GUIContent("Used Assets Map"),
                new GUIContent("Unused Assets")
            };

            public Color overlayUsedColor = Color.green;
            public Color overlayUnusedColor = Color.red;
            public Color overlaySpecialColor = Color.grey;
            public readonly GUIContent overlayUnusedContent = new GUIContent("Unused");
            public readonly GUIContent overlaySpecialContent = new GUIContent("Special");
            public readonly GUIContent overlayHasSpecialContent = new GUIContent("Has Special");
            public readonly GUIContent overlayEmptyContent = new GUIContent("Empty");

            public static readonly string EverythingContent = "Everything";
            public static readonly string NothingContent = "Nothing";

            public string changelog;
            public string tips;

            public static readonly GUIContent forceDisabledContent = new GUIContent(
                "<color=#aa0000><b>ForceEnabledFlag is set to false. Reports are <b>NOT</b> going to be generated.</b></color>\n" +
                "Most likely <b>BETTERBUILDINFO_FORCE_DISABLED</b> is defined in PlayerSettings.");

            public static readonly GUIContent forceEnabledContent = new GUIContent(
                "<color=#00aa00><b>ForceEnabledFlag is set to true. Reports are going to be generated, bypassing 'enabled' setting.</b></color>\n" +
                "Most likely <b>BETTERBUILDINFO_FORCE_ENABLED</b> is defined in PlayerSettings.");

            public static readonly GUIContent disabledStatusContent = new GUIContent(
                "The build analysis is <b><color=#aa0000>DISABLED</color></b>. " +
                "Reports are <b>NOT</b> going to be generated.");

            public static readonly GUIContent enabledStatusContent = new GUIContent(
                "The build analysis is <b><color=#00aa00>ENABLED</color></b>. " +
                "The tool will automatically generate a report on every successful build.");

            public static readonly GUIContent enabledAutoOpenContent = new GUIContent(
                "Auto-open reports is <b><color=#00aa00>ENABLED</color></b>. Generated reports are going to be opened automatically.");

            public static readonly GUIContent disabledAutoOpenContent = new GUIContent(
                "Auto-open reports is <b><color=#aa0000>DISABLED</color></b>. Generated reports are <b>NOT</b> going to be opened automatically.");

            public static readonly GUIContent enabledCollectAssetsDetailsContent = new GUIContent(
                "Collect Assets Details is <b><color=#00aa00>ENABLED</color></b>. Detailed information about assets (texture format etc.) is going to be collected.");

            public static readonly GUIContent disabledCollectAssetsDetailsContent = new GUIContent(
                "Collect Assets Details is <b><color=#aa0000>DISABLED</color></b>. Detailed information about assets (texture format etc.) is <b>NOT</b> going to be collected.");

            public static readonly GUIContent openSettingsInfoContent = new GUIContent(
                "To change these and more click on the <i>Settings...</i> button and then <i>Open</i>.");


            public void Reload()
            {
                var skin = GetCustomStyles();

                SetStyleOrThrow(ref header, "header", skin);
                SetStyleOrThrow(ref row0, "row0", skin);
                SetStyleOrThrow(ref row1, "row1", skin);
                SetStyleOrThrow(ref selectedRow, "selectedRow", skin);
                SetStyleOrThrow(ref entry, EditorGUIUtility.isProSkin ? "entryPro" : "entry", skin);
                SetStyleOrThrow(ref category, EditorGUIUtility.isProSkin ? "entryPro" : "entryWhite", skin);
                SetStyleOrThrow(ref treeMapEntry, "treeMapEntry", skin);
                SetStyleOrThrow(ref projectOverlayMask, "projectOverlayMask", skin);

                changelog = LoadTextOrThrow("BBI_Changelog.txt");
                tips = LoadTextOrThrow("BBI_Tips.txt");

                largeButtonStyle = GUIHelpers.GetStyleOrThrow("LargeButton");

                closeButtonStyle = GUIHelpers.GetStyle("WinBtnClose");
                if ( closeButtonStyle == null )
                {
                    closeButtonStyle = GUIHelpers.GetStyle("WinBtnCloseWin");
                    if (closeButtonStyle == null)
                    {
                        closeButtonStyle = EditorStyles.miniButton;
                    }
                }

                pathStyle = EditorStyles.largeLabel;

                entryZeroSize = new GUIStyle(entry);
                entryZeroSize.fontStyle = FontStyle.Italic;

                changelogStyle = new GUIStyle(EditorStyles.label);
                changelogStyle.richText = true;
                changelogStyle.wordWrap = true;

                toolbarButtonLeftAlignedStyle = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButtonLeftAlignedStyle.alignment = TextAnchor.MiddleLeft;

                labelRightAlignedStyle = new GUIStyle(EditorStyles.label);
                labelRightAlignedStyle.alignment = TextAnchor.MiddleRight;

                miniLabelRightAlignedStyle = new GUIStyle(EditorStyles.miniLabel);
                miniLabelRightAlignedStyle.alignment = TextAnchor.MiddleRight;

                labelRightTopAlignedStyle = new GUIStyle(EditorStyles.label);
                labelRightTopAlignedStyle.alignment = TextAnchor.UpperRight;

                inspectorBigStyle = new GUIStyle(GUIHelpers.GetStyleOrThrow("In BigTitle"));
                inspectorBigStyle.margin.top = 0;

                toolbarNoPaddingStyle =  new GUIStyle(EditorStyles.toolbar);
                toolbarNoPaddingStyle.padding.left = toolbarNoPaddingStyle.padding.right = 0;

                richMenuItem = new GUIStyle(GUIHelpers.GetStyleOrThrow("MenuItem"));
                richMenuItem.richText = true;
                richMenuItem.imagePosition = ImagePosition.ImageLeft;

                richLabel = new GUIStyle(EditorStyles.label);
                richLabel.richText = true;
                richLabel.clipping = TextClipping.Overflow;
                richLabel.padding = new RectOffset();

                richLabelRightAligned = new GUIStyle(richLabel);
                richLabelRightAligned.alignment = TextAnchor.MiddleRight;

                centeredGreyMiniLabel = EditorStyles.miniLabel;
                centeredGreyMiniLabel.normal.textColor = Color.grey;
                centeredGreyMiniLabel.alignment = TextAnchor.MiddleCenter;

                iconWindowContent = LoadTextureOrThrow("BBI_iconWindowContent.png");
                iconHeader = LoadTextureOrThrow("BBI_iconHeader.png");
                iconAssetMissing = LoadTextureOrThrow("BBI_iconAssetMissing.png");
                iconWhiteCircle = LoadTextureOrThrow("BBI_iconWhiteCircle.png");

                loaded = true;
            }

            public static float LineHeight
            {
                get { return EditorGUIUtility.singleLineHeight + 2; }
            }


            public static Texture2D LoadTextureOrThrow(string name)
            {
                var result = LoadTexture(name);
                if ( !result )
                {
                    throw new System.ArgumentOutOfRangeException(name);
                }
                return result;
            }

            public static string LoadTextOrThrow(string name)
            {
                var result = LoadText(name);
                if ( result == null )
                {
                    throw new System.ArgumentOutOfRangeException(name);
                }
                return result;
            }

            private static string NiceFormat(double value)
            {
                if (value <= 0)
                {
                    return "{0}";
                }

                var log = System.Math.Log10(value);
                if (log <= -2)
                {
                    return "{0:0.000}";
                }
                else if (log < 1)
                {
                    return "{0:0.00}";
                }
                else if (log < 2)
                {
                    return "{0:0.0}";
                }
                else
                {
                    return "{0:0}";
                }
            }

            public static string NicelyFormatted(double value)
            {
                return string.Format(NiceFormat(value), value);
            }

            public static string GetMenuOption(string label, int count, FileSize size)
            {
                return string.Format("{0} ({2}{3}{1})", label, count, size, "/");
            }

            public static string RichifyFileName(string path)
            {
                try
                {
                    var directory = ReliablePath.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        directory += '/';
                    }

                    return directory + "<b>" + ReliablePath.GetFileName(path) + "</b>";
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(string.Format("Error {0} for path {1}", ex, path));
                    return path;
                }
            }

            public Texture2D CreateCategoryIcon(Color categoryColor)
            {
                categoryColor.a = 1;

                if ( iconWhiteCircle == null )
                {
                    iconWhiteCircle = LoadTextureOrThrow("BBI_iconWhiteCircle.png");
                }

                var result = new Texture2D(iconWhiteCircle.width, iconWhiteCircle.height, TextureFormat.ARGB32, false);

                var pixels = iconWhiteCircle.GetPixels();
                for (int i = 0; i < pixels.Length; ++i)
                {
                    pixels[i] *= categoryColor;
                }

                result.SetPixels(pixels);
                result.Apply();
                
                return result;
            }

            private void SetStyleOrThrow(ref GUIStyle prop, string key, GUIStyle[] styles)
            {
                var result = styles.FirstOrDefault(x => x.name == key);
                if ( result == null )
                {
                    throw new System.ArgumentOutOfRangeException(key);
                }
                prop = result;
            }


        }
    }

    
}