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

using BitArray = System.Collections.BitArray;

namespace Better.BuildInfo
{
    [InitializeOnLoad]
    public sealed partial class BuildInfoWindow : EditorWindow, ISerializationCallbackReceiver
    {

        public enum Mode
        {
            Overview,
            UsedAssetsGrid,
            UsedAssetsTreemap,
            UnusedAssets
        }


        public enum SortProperty
        {
            AssetPath,
            Category,
            Size,
            BundledSize,
            ReferenceSize,
            Dependencies,
            TotalSize,
            AssetsDetailsStart,
        }

        [Serializable]
        private class SortInfo
        {
            public SortProperty prop;
            public bool ascending;
        }


        private Mode m_mode;

        private System.Exception m_error;
        private string m_openFilePath = string.Empty;
        private string m_referenceFilePath = string.Empty;
        private BuildInfo m_buildInfo = null;
        private BuildInfo m_referenceBuildInfo = null;

        private Skin m_skin = new Skin();

        private System.Exception m_skinError;

        private List<SortInfo> m_sorts = new List<SortInfo>()
        {
            new SortInfo()
            {
                prop = SortProperty.Size,
                ascending = false
            }
        };

        private bool m_needsRefresh;

        [NonSerialized]
        private bool? m_needsSorting = true;
        [NonSerialized]
        private bool m_needsFiltering = true;
        [NonSerialized]
        private bool m_needsDataPostprocessing = true;
        [NonSerialized]
        private bool m_needsSelectionRefresh = true;


        private List<AssetEntry> m_assets = new List<AssetEntry>();
        private Dictionary<string, AssetEntry> m_assetsByPath = new Dictionary<string, AssetEntry>();
        private List<AssetEntry> m_filteredAssets = new List<AssetEntry>();

        [NonSerialized]
        private List<AssetEntry> m_visibleAssets = new List<AssetEntry>();
        [NonSerialized]
        private List<AssetEntry> m_selectionDependencies = new List<AssetEntry>();
        [NonSerialized]
        private List<AssetEntry> m_selectionReferences = new List<AssetEntry>();
        [NonSerialized]
        private List<AssetEntry> m_selectionScenes = new List<AssetEntry>();


        [NonSerialized]
        private PreviewResizer m_previewResizer;

        private FileSize m_filteredTotalSize;




        private string m_randomTip;

        static BuildInfoWindow()
        {
            EditorApplication.update += () =>
            {
                string pathToOpen = BuildInfoSettings.ReportToOpen;
                if (!string.IsNullOrEmpty(pathToOpen))
                {
                    BuildInfoSettings.ReportToOpen = null;

                    Log.Info("Opening report: {0}", pathToOpen);

                    var wnd = BuildInfoWindow.GetWindow<BuildInfoWindow>();
                    wnd.Show();
                    wnd.OpenFile(pathToOpen);
                }
            };
        }

        [Serializable]
        private class GroupsInfo
        {
            public string[] names = new string[0];

            public FileSize[] sizes = new FileSize[0];
            public int[] counts = new int[0];

            public GUIContent[] dropDownContents = new GUIContent[0];
            public Internal.BitArray selected = new Internal.BitArray();

            public int count;
        }

        [Serializable]
        private class CategoriesGroupInfo : GroupsInfo
        {
            public Color[] colors = new Color[0];
            public GUIContent[] gridLabels = new GUIContent[0];
            public GUIContent[] overviewLabels = new GUIContent[0];
            public FileSize[] sizeDiffs = new FileSize[0];
        }

        [Serializable]
        private class AssetsDetailsInfo : GroupsInfo
        {
            public bool[] isNumeric = new bool[0];
            public bool[] isSize = new bool[0];
        }


        [Serializable]
        private class ScenesAndBundlesGroupInfo : GroupsInfo
        {
            public bool hasBundles = false;
        }

        private CategoriesGroupInfo m_categories = new CategoriesGroupInfo();
        private ScenesAndBundlesGroupInfo m_scenesAndBundles = new ScenesAndBundlesGroupInfo();
        private AssetsDetailsInfo m_detailsInfo = new AssetsDetailsInfo();


        [Serializable]
        private class UIState
        {
            public float gridHeight = 1000;
            public float detailsListHeight = 100;

            public Vector2 startScreenScroll;
            public Vector2 gridScroll;
            public Vector2 dependenciesScroll;
            public Vector2 referencesScroll;
            public Vector2 scenesScroll;

            public float minSizeMB = 0;
            public float maxSizeMB = Skin.MaxSizeMB;

            public string assetsFilter = "";
            public bool exclusiveScenesFilter = false;

            public bool overviewAssetsFoldout;

            [NonSerialized]
            public Rect lastTreeMapRect;

            public Vector2 overviewScroll;

            public bool settingsFoldout;
            public string settingsSearch;

            public bool environmentFoldout;
            public string environmentSearch;

            public int? scrollToIndex = null;
            public ScrollMode scrollMode = ScrollMode.SelectionFirst;

            public int prevSelectedIndex = -1;
            public string directoryToRemoveFrom = "Assets/";
            public string removeFilter = "";
        }





        private UIState ui = new UIState();


        private class CategoryTreeMapEntry
        {
            public Color color;
            public Rect rect;
            public List<Rect> assetsRects;
            public List<AssetEntry> assets;
            public List<bool> smalls;
        }

        private List<CategoryTreeMapEntry> m_categoriesTree;

        private bool IsComparing
        {
            get { return !string.IsNullOrEmpty(m_referenceFilePath); }
        }


        private static void FilterAssets(List<AssetEntry> filteredAssets, List<AssetEntry> assets, FileSize minSize, FileSize? maxSize, Internal.BitArray categoriesMask, Internal.BitArray selectionMask, bool exclusiveSelection, string nameFilter, string[] propertiesNames)
        {
            List<WildcardTest> parts = null;
            List<WildcardTest>[] propertiesValues = null;

            if (!string.IsNullOrEmpty(nameFilter))
            {
                parts = new List<WildcardTest>();

                foreach ( var part in nameFilter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) )
                {
                    var colonIndex = part.IndexOf(':');
                    // ignore 0 on purpose
                    if ( colonIndex > 0 )
                    {
                        var propertyName = part.Substring(0, colonIndex);

                        var propertyIndex = Array.FindIndex(propertiesNames, x => string.Equals(propertyName, x, StringComparison.OrdinalIgnoreCase));
                        if ( propertyIndex < 0 )
                        {
                            // unknown property, clear all
                            return;
                        }

                        if ( propertiesValues == null )
                        {
                            propertiesValues = new List<WildcardTest>[propertiesNames.Length];
                        }

                        if ( propertiesValues[propertyIndex] == null )
                        {
                            propertiesValues[propertyIndex] = new List<WildcardTest>();
                        }

                        string pattern = "";
                        if ( colonIndex + 1 < part.Length )
                        {
                            pattern = part.Substring(colonIndex + 1);
                        }

                        bool fullMatch = false;
                        if (pattern.Length > 1 && pattern.StartsWith("\"") && pattern.EndsWith("\""))
                        {
                            pattern = pattern.Substring(1, pattern.Length - 2);
                            fullMatch = true;
                        }

                        propertiesValues[propertyIndex].Add(WildcardTest.Create(pattern, fullMatch));
                    }
                    else
                    {
                        parts.Add(WildcardTest.Create(part, fullMatch: false));
                    }
                }
            }

            foreach (var asset in assets)
            {
                if (!categoriesMask[asset.categoryIndex])
                    continue;

                if (!selectionMask.AndAny(asset.scenesAndBundlesMask))
                    continue;

                if (exclusiveSelection)
                {
                    if (asset.scenesAndBundlesMask.AndAnyNeg(selectionMask))
                        continue;
                }

                if (asset.size < minSize)
                    continue;

                if (maxSize.HasValue && asset.size > maxSize.Value)
                    continue;

                if (parts != null)
                {
                    int i;
                    for (i = 0; i < parts.Count; ++i)
                    {
                        if (!parts[i].IsMatch(asset.info.path))
                            break;
                    }

                    if ( i != parts.Count )
                        continue;
                }

                if ( propertiesValues != null )
                {
                    int i = 0;
                    for (i = 0; i < propertiesValues.Length; ++i)
                    {
                        var values = propertiesValues[i];
                        if ( values == null )
                            continue;

                        var assetValue = asset.GetPropertyValue(i);
                        int valueIndex;
                        for ( valueIndex = 0; valueIndex < values.Count; ++valueIndex )
                        {
                            var val = values[valueIndex];
                            if (val.IsMatch(assetValue))
                                break;
                        }

                        // one has to match value
                        if ( valueIndex == values.Count )
                            break;
                    }

                    if ( i != propertiesValues.Length )
                        continue;
                }

                filteredAssets.Add(asset);
            }
        }

        private Comparison<AssetEntry> GetComparison(SortProperty sortProperty)
        {
            switch (sortProperty)
            {
                case SortProperty.AssetPath:
                    return (x, y) => x.info.path.CompareTo(y.info.path);

                case SortProperty.Size:
                    return (x, y) => x.size.CompareTo(y.size);

                case SortProperty.BundledSize:
                    return (x, y) => x.bundledSize.CompareTo(y.bundledSize);

                case SortProperty.ReferenceSize:
                    return (x, y) => x.sizeDiff.CompareTo(y.sizeDiff);

                case SortProperty.Category:
                    return (x, y) => x.categoryIndex - y.categoryIndex;

                case SortProperty.Dependencies:
                    return (x, y) => x.dependenciesSize.CompareTo(y.dependenciesSize);

                case SortProperty.TotalSize:
                    return (x, y) => x.totalSize.CompareTo(y.totalSize);

                default:
                    var propertyIndex = ((int)sortProperty - (int)SortProperty.AssetsDetailsStart);
                    if (m_detailsInfo.isNumeric[propertyIndex])
                    {
                        return (x, y) =>
                        {
                            var astr = x.GetPropertyValue(propertyIndex);
                            var bstr = y.GetPropertyValue(propertyIndex);

                            if (string.IsNullOrEmpty(astr))
                            {
                                if (string.IsNullOrEmpty(bstr))
                                {
                                    return 0;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(bstr))
                                {
                                    return 1;
                                }
                                else
                                {
                                    double a, b;
                                    if ( Utils.TryParseInvariant(astr, out a) && Utils.TryParseInvariant(bstr, out b) )
                                    {
                                        return a.CompareTo(b);
                                    }
                                    else
                                    {
                                        return 0;
                                    }
                                }
                            }
                        };
                    }
                    else
                    {
                        return (x, y) =>
                        {
                            var a = x.GetPropertyValue(propertyIndex);
                            var b = y.GetPropertyValue(propertyIndex);
                            return string.Compare(a, b);
                        };
                    }
            }
        }

        private void Sort(List<AssetEntry> assets, IList<SortInfo> sortInfos)
        {
            if ( sortInfos.Count == 1 )
            {
                var comparison = GetComparison(sortInfos[0].prop);

                assets.Sort(comparison);
                if (!sortInfos[0].ascending)
                {
                    assets.Reverse();
                }
            }
            else
            {
                var comparisons = sortInfos.Select(x =>
                {
                    var comparison = GetComparison(x.prop);
                    if (!x.ascending)
                    {
                        var oldComparison = comparison;
                        comparison = (xx, yy) => -oldComparison(xx, yy);
                    }
                    return comparison;
                }).ToList();

                Comparison<AssetEntry> compoundComparison = (x, y) =>
                {
                    for (int i = 0; i < comparisons.Count; ++i)
                    {
                        var res = comparisons[i](x, y);
                        if (res != 0)
                            return res;
                    }
                    return 0;
                };

                assets.Sort(compoundComparison);
            }

            //InsertionSort(assets, comparison);
        }


        private void DoStartScreenGUI()
        {
            using (GUIHelpers.ScrollView(ref ui.startScreenScroll))
            {
                EditorGUILayout.LabelField(string.Format("Welcome to Better Build Info {0}!", BetterBuildInfo.Version), EditorStyles.boldLabel);

                if ( BetterBuildInfo.ForceEnabledFlag == false )
                {
                    GUILayout.Label(Skin.forceDisabledContent, m_skin.changelogStyle);
                }
                else if (BetterBuildInfo.ForceEnabledFlag == true)
                {
                    GUILayout.Label(Skin.forceEnabledContent, m_skin.changelogStyle);
                    GUILayout.Label(Settings.autoOpenReportAfterBuild ? Skin.enabledAutoOpenContent : Skin.disabledAutoOpenContent, m_skin.changelogStyle);
                    GUILayout.Label(Settings.collectAssetsDetails ? Skin.enabledCollectAssetsDetailsContent : Skin.disabledCollectAssetsDetailsContent, m_skin.changelogStyle);
                    GUILayout.Label(Skin.openSettingsInfoContent, m_skin.changelogStyle);
                }
                else 
                {
                    GUILayout.Label(Settings.enabled ? Skin.enabledStatusContent : Skin.disabledStatusContent, m_skin.changelogStyle);
                    GUILayout.Label(Settings.autoOpenReportAfterBuild ? Skin.enabledAutoOpenContent : Skin.disabledAutoOpenContent, m_skin.changelogStyle);
                    GUILayout.Label(Settings.collectAssetsDetails ? Skin.enabledCollectAssetsDetailsContent : Skin.disabledCollectAssetsDetailsContent, m_skin.changelogStyle);
                    GUILayout.Label(Skin.openSettingsInfoContent, m_skin.changelogStyle);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Random Tip", EditorStyles.boldLabel);
                using (GUIHelpers.Horizontal(GUI.skin.box))
                {
                    if (string.IsNullOrEmpty(m_randomTip))
                    {
                        var tips = m_skin.tips.Split(new[] { "***" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tips.Length == 0)
                        {
                            m_randomTip = m_skin.tips;
                        }
                        else
                        {
                            m_randomTip = tips[UnityEngine.Random.Range(0, tips.Length)].Trim();
                        }
                    }

                    GUILayout.Label(m_randomTip, m_skin.changelogStyle);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Changelog", EditorStyles.boldLabel);
                using (GUIHelpers.Horizontal(GUI.skin.box))
                {
                    GUILayout.Label(m_skin.changelog, m_skin.changelogStyle);
                }
            }
        }



        private void DoBuildInfoGUI()
        {
            using (GUIHelpers.Horizontal())
            {
                GUILayout.Space(38f);
                m_mode = (Mode)GUILayout.Toolbar((int)m_mode, Skin.ToolbarOptions, GUILayout.Width(position.width - 76f));
            }

            if (m_mode == Mode.Overview)
            {
                DoOverviewGUI();
            }
            else if ( m_mode == Mode.UnusedAssets )
            {
                DoUnusedAssetsGUI();
            }
            else if ( m_mode == Mode.UsedAssetsGrid || m_mode == Mode.UsedAssetsTreemap )
            {
                var minWidth = GUILayout.MinWidth(150.0f);

                using (GUIHelpers.Vertical())
                using (GUIHelpers.LabelWidth(60))
                {
                    GUILayout.Space(3.0f);

                    using (GUIHelpers.Horizontal())
                    {
                        GUILayout.Space(10.0f);

                        DoFilterControlGUI(m_categories, m_skin.categoriesContent, () =>
                        {
                            using (GUIHelpers.Vertical(m_skin.header))
                            {
                                EditorGUILayout.LabelField("TIP: ctrl+click for multiple selection.", EditorStyles.wordWrappedMiniLabel);
                            }
                        }, minWidth);
                        GUILayout.Space(10.0f);

                        DoFilterControlGUI(m_scenesAndBundles, m_skin.scenesFilterContent, () =>
                            {
                                using (GUIHelpers.Vertical(m_skin.header))
                                {
                                    EditorGUI.BeginChangeCheck();

                                    ui.exclusiveScenesFilter = EditorGUILayout.ToggleLeft("Exclusive Filter", ui.exclusiveScenesFilter, EditorStyles.wordWrappedMiniLabel);
                                    if (ui.exclusiveScenesFilter)
                                    {
                                        EditorGUILayout.LabelField("Assets that are referenced on selected scenes *exclusively* are shown.", EditorStyles.wordWrappedMiniLabel);
                                    }
                                    else
                                    {
                                        EditorGUILayout.LabelField("Assets that are referenced by any of selected scenes are shown.", EditorStyles.wordWrappedMiniLabel);
                                    }
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        m_needsFiltering = true;
                                        Repaint();
                                    }

                                    GUILayout.Space(5);
                                }
                            }, minWidth);
                        GUILayout.Space(10.0f);

                        {
                            EditorGUI.BeginChangeCheck();

                            var rect = EditorGUILayout.GetControlRect(false, minWidth);
                            DoMinMaxPowerSlider(rect, ref ui.minSizeMB, ref ui.maxSizeMB, 0.0f, Skin.MaxSizeMB, 4.0f, 60.0f);

                            if (EditorGUI.EndChangeCheck())
                            {
                                m_needsFiltering = true;
                            }
                        }

                        GUILayout.Space(10.0f);

                        {
                            EditorGUI.BeginChangeCheck();

                            ui.assetsFilter = GUIHelpers.ToolbarSearchFieldLayout(ui.assetsFilter, false, GUILayout.MinWidth(50.0f));

                            if (EditorGUI.EndChangeCheck())
                            {
                                m_needsFiltering = true;
                            }
                        }

                        GUILayout.Space(10.0f);
                    }
                }

                var selectedAssets = m_assets.Where(x => x.selected).ToList();

                int selectedCount = selectedAssets.Count;
                var selectedSize = selectedAssets.SumFileSize(x => x.size);

                EditorGUILayout.Space();

                if (m_mode == Mode.UsedAssetsGrid)
                {
                    AssetsGridGUI();
                }
                else if (m_mode == Mode.UsedAssetsTreemap)
                {
                    AssetsTreemapGUI();
                }

                if (m_previewResizer == null)
                {
                    m_previewResizer = new PreviewResizer();
                    m_previewResizer.Init("BetterMergeDetails10");
                }


                using (GUIHelpers.Horizontal(EditorStyles.toolbar, GUILayout.Height(20f)))
                {
                    {
                        GUILayout.Label(
                            string.Format("Selected <b>Size: {0}, Dependencies: {2}, Count: {1}</b>",
                                selectedSize.ToString(),
                                selectedCount,
                                m_selectionDependencies.SumFileSize(x => x.size).ToString()),
                            m_skin.richLabel);
                    }

                    GUILayout.FlexibleSpace();

                    {
                        GUILayout.Label(string.Format("Visible <b>Size: {0}, Count: {1}</b>",
                                m_filteredTotalSize,
                                m_filteredAssets.Count),
                            m_skin.richLabelRightAligned);
                    }
                }

                float detailsHeight = m_previewResizer.ResizeHandle(position, 100, 250f, 20f);

                if (detailsHeight == 0.0f)
                    return;

                GUILayout.Space(detailsHeight);

                var panelRect = new Rect(0, position.height - detailsHeight, position.width / 3, detailsHeight);

                float h = 0;

                h = Mathf.Max(h, BottomPanel(panelRect, ref ui.dependenciesScroll, "Selection's Dependencies", m_selectionDependencies));

                panelRect.x += panelRect.width;
                h = Mathf.Max(h, BottomPanel(panelRect, ref ui.referencesScroll, "Assets Referencing Selection", m_selectionReferences));

                panelRect.x += panelRect.width;
                h = Mathf.Max(h, BottomPanel(panelRect, ref ui.scenesScroll, "Scenes & Bundles Referencing Selection", m_selectionScenes));

                if (Event.current.type == EventType.Repaint)
                {
                    if (h > 1)
                    {
                        if (h != ui.detailsListHeight)
                        {
                            ui.detailsListHeight = h;
                            Repaint();
                        }
                    }
                }
            }
        }

        private void DoUnusedAssetsGUI()
        {
            EditorGUILayout.LabelField("Unused Assets feature is experimental.", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Unused Assets works as an overlay for the <b>Project window</b>. " +
                "This feature needs a bit of precomputing, so just press <b>Refresh</b> button to enable it or refresh, if you add/remove/move assets.", m_skin.changelogStyle);

            if (GUILayout.Button("Refresh Overlay"))
            {
                EditorUtility.DisplayProgressBar("Better Build Info", "Preparing unused assets overlay", 0);
                try
                {
                    m_assetsDirectoriesInfo = new AssetsDirectoriesInfo();
                    m_assetsDirectoriesInfo.Refresh(
                        AssetDatabase.FindAssets(string.Empty).Select(x => AssetDatabase.GUIDToAssetPath(x)).Distinct(),
                        m_buildInfo.assets
                    );

                    EditorUtility.FocusProjectWindow();
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            if (GUILayout.Button("Clear Overlay"))
            {
                m_assetsDirectoriesInfo = new AssetsDirectoriesInfo();
                EditorUtility.FocusProjectWindow();
            }
        }

        private void DrawSizeGUI(GUIContent label, FileSize uncompressed, FileSize compressed, FileSize? relativeUncompressed = null, FileSize? relativeCompressed = null, FileSize? uncompressedReference = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string text;

                if (relativeUncompressed.HasValue)
                {
                    text = string.Format("{0} ({1}%)", uncompressed, Skin.NicelyFormatted(100.0 * uncompressed.KiloBytes / relativeUncompressed.Value.KiloBytes));
                }
                else
                {
                    text = uncompressed.ToString();
                }

                if (compressed > FileSize.Zero)
                {
                    text += " / compressed: ";
                    if (relativeCompressed.HasValue)
                    {
                        text += string.Format("{0} ({1}%)", compressed, Skin.NicelyFormatted(100.0 * compressed.KiloBytes / relativeCompressed.Value.KiloBytes));
                    }
                    else
                    {
                        text += compressed.ToString();
                    }
                }

                if (uncompressedReference != null)
                {
                    var diff = uncompressed - uncompressedReference.Value;
                    var format = " <color=#{0}>{1}{2}</color>";
                    if (diff != FileSize.Zero)
                    {
                        text += string.Format(format,
                            UnityVersionAgnostic.ToHtmlStringRGB(diff > FileSize.Zero ? m_skin.sizeIncreasedColor : m_skin.deletedItemColor),
                            diff > FileSize.Zero ? Skin.UpArrow : Skin.DownArrow,
                            diff.Abs());
                    }
                }

                if (label == null)
                {
                    EditorGUILayout.LabelField(text, m_skin.richLabel);
                }
                else
                {
                    EditorGUILayout.LabelField(label, new GUIContent(text), m_skin.richLabel);
                }
            }
        }

        private void DrawSizeGUI(string label, FileSize uncompressed, FileSize compressed, FileSize? relativeUncompressed = null, FileSize? relativeCompressed = null, FileSize? uncompressedReference = null)
        {
            DrawSizeGUI(label == null ? null : new GUIContent(label), uncompressed, compressed, relativeUncompressed, relativeCompressed, uncompressedReference);
        }

        private static FileSize GetUnaccountedSize(BuildInfo report, FileSize assetsSize)
        {
            return FileSize.FromBytes(Math.Max(0, report.totalSize - report.runtimeSize - report.streamingAssetsSize - assetsSize.Bytes));
        }

        private void DoOverviewGUI()
        {
            using (GUIHelpers.ScrollView(ref ui.overviewScroll))
            {
                if (!string.IsNullOrEmpty(m_referenceFilePath))
                {
                    EditorGUILayout.LabelField("Comparing With", m_referenceFilePath, EditorStyles.boldLabel);
                    EditorGUILayout.Space();
                }

                var date = new DateTime(m_buildInfo.dateUTC, DateTimeKind.Utc).ToLocalTime();
                EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Build Date", date.ToShortDateString() + " " + date.ToShortTimeString());
                EditorGUILayout.LabelField("Build Target", m_buildInfo.buildTarget);
                EditorGUILayout.LabelField("Project Path", m_buildInfo.projectPath);
                EditorGUILayout.LabelField("Output Path", m_buildInfo.outputPath);
                EditorGUILayout.LabelField("Unity Version", m_buildInfo.unityVersion);

                EditorGUILayout.LabelField("Build Time", string.Format("{0} (overhead: {1})",
                    TimeSpan.FromSeconds(m_buildInfo.buildTime),
                    TimeSpan.FromSeconds(m_buildInfo.overheadTime)));

                EditorGUILayout.Toggle("Assets' Details", m_detailsInfo.names.Any());

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Size Overview", EditorStyles.boldLabel);

                var allAssetsSize = m_categories.sizes.SumFileSize(x => x);
                var allAssetsSizeDiff = m_categories.sizeDiffs.SumFileSize(x => x);

                FileSize? unaccountedReference = null;
                FileSize? totalSizeReference = null;
                FileSize? runtimeSizeReference = null;
                FileSize? streamingAssetsSizeReference = null;

                if (IsComparing)
                {
                    unaccountedReference = GetUnaccountedSize(m_referenceBuildInfo, allAssetsSize - allAssetsSizeDiff);
                    totalSizeReference = m_referenceBuildInfo.totalSize;
                    runtimeSizeReference = m_referenceBuildInfo.runtimeSize;
                    streamingAssetsSizeReference = m_referenceBuildInfo.streamingAssetsSize;
                }

                DrawSizeGUI("Total", m_buildInfo.totalSize, m_buildInfo.compressedSize, 
                    uncompressedReference: totalSizeReference);

                DrawSizeGUI("Runtime", m_buildInfo.runtimeSize, m_buildInfo.compressedRuntimeSize, m_buildInfo.totalSize, m_buildInfo.compressedSize,
                    uncompressedReference: runtimeSizeReference);

                DrawSizeGUI("Streaming Assets", m_buildInfo.streamingAssetsSize, 0, m_buildInfo.totalSize,
                    uncompressedReference: streamingAssetsSizeReference);

                DrawSizeGUI("Unaccounted", GetUnaccountedSize(m_buildInfo, allAssetsSize), 0, m_buildInfo.totalSize,
                    uncompressedReference: unaccountedReference);


                using (GUIHelpers.Horizontal())
                {
                    var r = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    ui.overviewAssetsFoldout = EditorGUI.Foldout(r, ui.overviewAssetsFoldout, "Assets");

                    DrawSizeGUI((string)null, allAssetsSize, 0, m_buildInfo.totalSize, uncompressedReference: allAssetsSize - allAssetsSizeDiff);
                }

                if (ui.overviewAssetsFoldout)
                {
                    var sortedIndices = m_categories.sizes.Select((size, index) => new { size, index })
                        .OrderByDescending(x => x.size)
                        .Select(x => x.index);

                    foreach (var index in sortedIndices)
                    {
                        if (m_categories.counts[index] <= 0)
                            continue;

                        EditorGUI.indentLevel++;

                        var categorySize = m_categories.sizes[index];
                        DrawSizeGUI(m_categories.overviewLabels[index], categorySize, 0,
                            relativeUncompressed: m_buildInfo.totalSize,
                            uncompressedReference: categorySize - m_categories.sizeDiffs[index]);

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Detailed Build Settings", EditorStyles.boldLabel);
                ui.settingsFoldout = EditorGUILayout.Foldout(ui.settingsFoldout, "Build Settings");
                if (ui.settingsFoldout)
                {
                    using (GUIHelpers.Vertical(GUI.skin.box))
                    {
                        ui.settingsSearch = GUIHelpers.ToolbarSearchFieldLayout(ui.settingsSearch, false);

                        string[] parts = null;
                        if (!string.IsNullOrEmpty(ui.settingsSearch))
                        {
                            parts = ui.settingsSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        }

                        foreach (var setting in m_buildInfo.buildSettings)
                        {
                            if (parts != null &&
                                !parts.All(x => setting.name.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0 || setting.value.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
                                continue;

                            EditorGUILayout.LabelField(setting.name, setting.value);
                        }
                    }
                }

                ui.environmentFoldout = EditorGUILayout.Foldout(ui.environmentFoldout, "Environment Variables");
                if (ui.environmentFoldout)
                {
                    using (GUIHelpers.Vertical(GUI.skin.box))
                    {
                        ui.environmentSearch = GUIHelpers.ToolbarSearchFieldLayout(ui.environmentSearch, false);

                        string[] parts = null;
                        if (!string.IsNullOrEmpty(ui.environmentSearch))
                        {
                            parts = ui.environmentSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        }

                        foreach (var setting in m_buildInfo.environmentVariables)
                        {
                            if (parts != null &&
                                !parts.All(x => setting.name.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0 || setting.value.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
                                continue;

                            EditorGUILayout.LabelField(setting.name, setting.value);
                        }
                    }
                }
            }
        }

        private void DrawEntryWithIcon(AssetEntry entry, Rect rect, Action<int, AssetEntry> onClick = null, Action<KeyCode, AssetEntry> onKeyDown = null)
        {
            var textureRect = rect;
            textureRect.width = 18;

            Texture icon;
            if ( entry.hasThumbnail || entry.hasAsset )
            {
                icon = entry.thumbnail;
            }
            else
            {
                icon = entry.icon;
            }
            
            GUI.DrawTexture(textureRect, icon ?? m_skin.iconAssetMissing, ScaleMode.ScaleToFit);

            GUIHelpers.HierarchyLikeLabel(rect, entry.richTextContent, entry.controlId, entry.size == FileSize.Zero ? m_skin.entryZeroSize : m_skin.entry, false,
                entry, leftDrawOffset: 18f, onClicked: onClick, onKeyDown: onKeyDown);
        }

        private float BottomPanel(Rect position, ref Vector2 scroll, string title, IList<AssetEntry> entries)
        {
            using (GUIHelpers.Area(position))
            using (GUIHelpers.Vertical(GUI.skin.box))
            {
                EditorGUILayout.LabelField(title, m_skin.centeredGreyMiniLabel);

                GUIHelpers.FastScroll(ref scroll, entries.Count, Skin.LineHeight, null, ui.detailsListHeight, index =>
                {
                    var rect = GUILayoutUtility.GetRect(1, Skin.LineHeight, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
                    DrawEntryWithIcon(entries[index], rect, onClick: OnAssetEntryClicked);
                });

                if (Event.current.type == EventType.Repaint)
                {
                    var r = GUILayoutUtility.GetLastRect();
                    return r.height;
                } 
                else
                {
                    return 0.0f;
                }
            }
        }

        private void AssetsTreemapGUI()
        {
            // eat up all the space there is
            GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            bool refreshTreeMap = m_categoriesTree == null;

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();

                if (rect != ui.lastTreeMapRect)
                {
                    // need to refresh the treemap
                    refreshTreeMap = true;
                }

                ui.lastTreeMapRect = rect;
            }

            if (ui.lastTreeMapRect.width == 0 || ui.lastTreeMapRect.height == 0)
            {
                return;
            }

            if (refreshTreeMap)
            {
                // first, get all used categories

                var grouped = new List<List<AssetEntry>>();
                for (int i = 0; i < m_categories.count; ++i)
                {
                    grouped.Add(new List<AssetEntry>());
                }

                foreach (var asset in m_filteredAssets)
                {
                    grouped[asset.categoryIndex].Add(asset);
                }

                var rects = TreeMap.CalcMap(grouped.Select(x => x.SumFileSize(xx => xx.size).KiloBytes), ui.lastTreeMapRect);

                m_categoriesTree = new List<CategoryTreeMapEntry>();

                for (int i = 0; i < rects.Length; ++i)
                {
                    var output = rects[i];

                    var entry = new CategoryTreeMapEntry()
                    {
                        assets = grouped[i],
                        color = m_categories.colors[i],
                        rect = output,
                    };

                    entry.assetsRects = TreeMap.CalcMap(entry.assets.Select(x => x.size.KiloBytes), output)
                        .ToList();

                    entry.smalls = entry.assetsRects.Select(x => x.width * x.height < 35.0f).ToList();

                    m_categoriesTree.Add(entry);
                }
            }


            foreach (var treeEntry in m_categoriesTree)
            {
                // determine the size of grouping entry for small assets
                Rect? smallRect = null;
                int smallCount = 0;

                for (int i = 0; i < treeEntry.assetsRects.Count; ++i)
                {
                    if (treeEntry.smalls[i])
                    {
                        var r = treeEntry.assetsRects[i];
                        ++smallCount;
                        if (r.width > 0 && r.height > 0)
                        {
                            if (smallRect == null)
                            {
                                smallRect = r;
                            }
                            else
                            {
                                smallRect = smallRect.Value.Union(r);
                            }
                        }
                    }
                }



                if (smallRect.HasValue)
                {
                    using (GUIHelpers.BackgroundColor(treeEntry.color * 0.8f))
                    {
                        var guiContent = new GUIContent("remaining " + smallCount + " assets");
                        guiContent.tooltip = guiContent.text;

                        GUIHelpers.HierarchyLikeLabel<UnityEngine.Object>(smallRect.Value, guiContent, GUIUtility.GetControlID(FocusType.Passive),
                            m_skin.treeMapEntry, false, null, null);
                    }
                }

                using (GUIHelpers.BackgroundColor(treeEntry.color))
                {
                    for (int i = 0; i < treeEntry.assetsRects.Count; ++i)
                    {
                        GUI.backgroundColor = treeEntry.color;

                        if (!treeEntry.smalls[i])
                        {
                            var assetEntry = treeEntry.assets[i];

                            GUIHelpers.HierarchyLikeLabel(treeEntry.assetsRects[i], assetEntry.shortTextContentWithTooltip, assetEntry.controlId,
                                m_skin.treeMapEntry, assetEntry.selected, assetEntry, onClicked: OnAssetEntryClicked);
                        }
                    }
                }
            }

        }

        private void AssetsGridGUI()
        {
            // using ( GUIHelpers.BackgroundColor(new Color(0.8f, 0.8f, 0.8f)))

            var columnWidths = m_detailsInfo.names
                .Select(x => BuildInfoSettings.GetColumnWidth(x))
                .Select(x => Mathf.Max(1.0f, x))
                .ToArray();

            bool nullifyDragProperty = Event.current.type == EventType.MouseUp;

            using ( GUIHelpers.ScrollViewWindow(new Vector2(ui.gridScroll.x, 0)) )
            using ( GUIHelpers.Horizontal(m_skin.toolbarNoPaddingStyle) )
            {
                float dummy = 0;
                HandleColumnLabel("Category", SortProperty.Category, false, ref dummy, m_skin.categoryColumnWidth);
                HandleColumnLabel("Asset Path", SortProperty.AssetPath, false, ref dummy, GUILayout.MinWidth(50));
                HandleColumnLabel("Size", SortProperty.Size, false, ref dummy, m_skin.sizeColumnWidth);

                if ( m_scenesAndBundles.hasBundles )
                {
                    HandleColumnLabel("BundledSize", SortProperty.BundledSize, false, ref dummy, m_skin.bundledSizeColumnWidth);
                }

                if (IsComparing)
                {
                    HandleColumnLabel("Difference", SortProperty.ReferenceSize, false, ref dummy, m_skin.sizeDiffColumnWidth);
                }

                if (Settings.showTotalSizeColumn)
                {
                    HandleColumnLabel("Total", SortProperty.TotalSize, false, ref dummy, m_skin.sizeColumnWidth);
                }

                if ( m_detailsInfo.names.Any() )
                {
                    for ( int i = 0; i < m_detailsInfo.selected.Length; ++i )
                    {
                        if ( !m_detailsInfo.selected[i] )
                            continue;

                        {
                            float width = columnWidths[i];
                            if ( HandleColumnLabel(m_detailsInfo.names[i], SortProperty.AssetsDetailsStart + i, true, ref width, GUILayout.Width(width)) )
                            {
                                m_sorts.RemoveAll(x => x.prop == SortProperty.AssetsDetailsStart + i);
                                m_needsSorting = true;
                                m_detailsInfo.selected[i] = false;
                                Repaint();
                            }
                            if ( width != columnWidths[i] )
                            {
                                BuildInfoSettings.SetColumnWidth(m_detailsInfo.names[i], width);
                                Repaint();
                            }
                        }
                    }

                    HandleDetailsColumnsButton(GUILayout.Width(16.0f));
                }
                else
                {
                    GUILayout.Space(16.0f);
                }
            }

            if ( nullifyDragProperty )
            {
                m_dragProperty = null;
            }

            m_visibleAssets.Clear();

            int? scrollToIndex = ui.scrollToIndex;
            if ( scrollToIndex.HasValue && ui.scrollMode == ScrollMode.SelectionLast )
            {
                scrollToIndex = -ui.scrollToIndex;
            }

            ui.scrollToIndex = null;

            if ( Event.current.type == EventType.KeyUp )
            {
                // a dirty workaround for key-based scrolling
                m_needsSelectionRefresh = true;
            }

            GUIHelpers.FastScroll(ref ui.gridScroll, m_filteredAssets.Count, Skin.LineHeight, scrollToIndex, ui.gridHeight, index =>
            {
                GUIStyle style;

                var asset = m_filteredAssets[index];
                m_visibleAssets.Add(asset);

                if (asset.selected)
                {
                    style = m_skin.selectedRow;
                }
                else
                {
                    style = index % 2 == 1 ? m_skin.row1 : m_skin.row0;
                } 

                using (GUIHelpers.Horizontal(style, GUILayout.Height(Skin.LineHeight)))
                {
                    int clickCount = 0;

                    using (GUIHelpers.ContentColor(m_categories.colors[asset.categoryIndex]))
                    {
                        clickCount += GUIHelpers.StaticLabelLayout(m_categories.gridLabels[asset.categoryIndex], m_skin.category, asset.selected, m_skin.categoryColumnWidth);
                    }

                    using (GUIHelpers.ContentColor(asset.size == FileSize.Zero ? Color.gray : Color.white))
                    {
                        var rect = GUILayoutUtility.GetRect(1, Skin.LineHeight, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
                        DrawEntryWithIcon(asset, rect,
                            onClick: (cc, context) => OnAssetEntryClickedEx(cc, context, true, !Settings.doubleClickSelect),
                            onKeyDown: (key, context) => OnAssetEntryKeyUp(key, context));
                    }

                    clickCount += GUIHelpers.StaticLabelLayout(asset.sizeContent, m_skin.entry, asset.selected, m_skin.sizeColumnWidth);

                    if (m_scenesAndBundles.hasBundles)
                    {
                        clickCount += GUIHelpers.StaticLabelLayout(asset.bundledSizeContent, m_skin.entry, asset.selected, m_skin.bundledSizeColumnWidth);
                    }

                    if (IsComparing)
                    {
                        Color color;
                        if (asset.size == FileSize.Zero)
                            color = m_skin.deletedItemColor;
                        else if (asset.size == asset.sizeDiff)
                            color = m_skin.newItemColor;
                        else if (asset.sizeDiff > FileSize.Zero)
                            color = m_skin.sizeIncreasedColor;
                        else
                            color = m_skin.sizeDecreasedColor;

                        using (GUIHelpers.ContentColor(color))
                        {
                            clickCount += GUIHelpers.StaticLabelLayout(asset.sizeDiffContent, m_skin.category, asset.selected, m_skin.sizeDiffColumnWidth);
                        }
                    }

                    if (Settings.showTotalSizeColumn)
                    {
                        clickCount += GUIHelpers.StaticLabelLayout(asset.totalSizeContent, m_skin.entry, asset.selected, m_skin.sizeColumnWidth);
                    }



                    for (int i = 0; i < m_detailsInfo.selected.Length; ++i)
                    {
                        if (!m_detailsInfo.selected[i])
                            continue;

                        GUIContent propertyContent = GUIContent.none;
                        var propertyValue = asset.GetPropertyValue(i);

                        if (!string.IsNullOrEmpty(propertyValue) )
                        {
                            long parsedSize;
                            if ( m_detailsInfo.isSize[i] && long.TryParse(propertyValue, out parsedSize) )
                            {
                                propertyValue = FileSize.FromBytes(parsedSize).ToString();
                            }
                            propertyContent = new GUIContent(propertyValue);
                        }

                        clickCount += GUIHelpers.StaticLabelLayout(propertyContent, m_skin.entry, asset.selected, GUILayout.Width(columnWidths[i]));
                    }

                    if ( clickCount > 0 )
                    {
                        // full row select -- there's something wrong with it for now
                        //OnAssetEntryClickedEx(clickCount, asset, true, !Settings.doubleClickSelect);
                    }

                    if ( ui.gridHeight > m_filteredAssets.Count * Skin.LineHeight )
                    {
                        GUILayout.Space(15.0f);
                    }
                }
            });

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                if (rect.height > 1)
                {
                    if ( rect.height != ui.gridHeight )
                    {
                        ui.gridHeight = rect.height;
                        Repaint();
                    }
                }
            }
        }



       

        private void DoFilterControlGUI(Rect rect, GroupsInfo groupInfo, System.Action footer)
        {
            var content = GUIHelpers.GenerateDropdownLabel(groupInfo.selected, groupInfo.names);
           
            if ( GUI.Button(rect, content, EditorStyles.popup) )
            {
                var popupContent = MenuPopupWindowContent.Create(m_skin.richMenuItem, Enumerable.Range(0, groupInfo.count),
                    i => groupInfo.dropDownContents[i],
                    i => groupInfo.selected[i],
                    (i, v) =>
                    {
                        groupInfo.selected[i] = v;
                        m_needsFiltering = true;
                        Repaint();
                    }, footer);

                PopupWindow.Show(rect, popupContent);
            }
        }

        private Rect DoFilterControlGUI(GroupsInfo groupInfo, GUIContent label, System.Action footer, params GUILayoutOption[] options)
        {
            Rect rect;
            if (label != null)
            {
                rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, options);
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            else
            {
                rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
            }

            DoFilterControlGUI(rect, groupInfo, footer);

            return rect;
        }







        private void DoMinMaxPowerSlider(Rect rect, ref float minValue, ref float maxValue, float minLimit, float maxLimit, float power, float labelWidth)
        {
            const float margin = 5.0f;

            var r = rect;
            r.width = labelWidth;

            EditorGUI.LabelField(r, Skin.NicelyFormatted(minValue) + " MB", m_skin.labelRightAlignedStyle);

            var start = Mathf.Pow(minLimit, 1f / power);
            var end = Mathf.Pow(maxLimit, 1f / power);
            var minSize = Mathf.Pow(minValue, 1f / power);
            var maxSize = Mathf.Pow(maxValue, 1f / power);

            r = rect;
            r.x += labelWidth + margin;
            r.width -= 2 * labelWidth + 2 * margin;
            EditorGUI.MinMaxSlider(r, ref minSize, ref maxSize, start, end);

            minValue = Mathf.Pow(minSize, power);
            maxValue = Mathf.Pow(maxSize, power);

            r = rect;
            r.x = r.x + r.width - labelWidth;
            r.width = labelWidth;

            bool nearMax = Utils.NearlyEqual(maxValue, maxLimit, 0.0001f);

            if (nearMax)
            {
                EditorGUI.LabelField(r, string.Format("> {0:0}", maxValue) + " MB");
            }
            else
            {
                EditorGUI.LabelField(r, Skin.NicelyFormatted(maxValue) + " MB");
            }
        }

        private void ModifySettings(Action<BuildInfoSettings> change)
        {
            BuildInfoSettings.EnsureAsset();
            change(Settings);
            EditorUtility.SetDirty(Settings);
        }

        private BuildInfoSettings Settings
        {
            get { return BuildInfoSettings.Instance; }
        }

        public void OpenReferenceFile(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = UnityVersionAgnostic.OpenFilePanelWithFilters("Open Build Info", BuildInfoSettings.LastDirectory, new[] { "BBI XML report", "bbi,xml", "All files", "*" }, "xml");
            }

            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                m_referenceFilePath = string.Empty;
                m_referenceBuildInfo = null;
                m_needsRefresh = true;

                using (var file = System.IO.File.OpenRead(path))
                {
                    var serializer = new XmlSerializer(typeof(BuildInfo));
                    m_referenceBuildInfo = (BuildInfo)serializer.Deserialize(file);
                    
                }

                m_referenceFilePath = path;
                m_error = null;

                ModifySettings(x => x.AddRecent(Utils.PoorMansRelativePath(path)));
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                m_error = ex;
            }
        }

        public void OpenFile(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = UnityVersionAgnostic.OpenFilePanelWithFilters("Open Build Info", BuildInfoSettings.LastDirectory, new[] { "BBI XML report", "bbi,xml", "All files", "*" }, "xml");
            }

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    BuildInfoSettings.LastDirectory = ReliablePath.GetDirectoryName(path);
                }
                catch (System.Exception) { }
                

                m_needsRefresh = true;
                m_buildInfo = null;
                m_openFilePath = string.Empty;
                m_referenceFilePath = string.Empty;
                m_referenceBuildInfo = null;

                try
                {
                    using (var file = System.IO.File.OpenRead(path))
                    {
                        var serializer = new XmlSerializer(typeof(BuildInfo));
                        m_buildInfo = (BuildInfo)serializer.Deserialize(file);

                        // hacky: trim build timings
                        m_buildInfo.buildTime = Mathf.Round(m_buildInfo.buildTime);
                        m_buildInfo.overheadTime = Mathf.Round(m_buildInfo.overheadTime);

                        // we need extra info to be sorted and without duplicates
                        foreach (var entry in m_buildInfo.assets)
                        {
                            entry.details = BuildInfoProcessorUtils.CleanUpAssetsDetails(entry.details, entry.path);
                        }

                        m_openFilePath = Utils.PoorMansRelativePath(path);
                        m_error = null;

                        ModifySettings(x => x.AddRecent(m_openFilePath));
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    m_error = ex;
                }
            }
        }

        private SortProperty? m_dragProperty = null;

        private static Rect SuffixRect(Rect parent, float width, float height, float padding)
        {
            Rect result = new Rect();
            result.x = Mathf.Round(parent.xMax - width - padding);
            result.y = Mathf.Round(parent.y + (parent.height - height) / 2);
            result.width = width;
            result.height = height;
            return result;
        }

        private bool HandleColumnLabel(string name, SortProperty property, bool closeable, ref float width, params GUILayoutOption[] options)
        {
            var sortIndex = m_sorts.FindIndex(x => x.prop == property);
            SortInfo sortInfo = sortIndex >= 0 ? m_sorts[sortIndex] : null;

            string label;
            if (sortInfo != null)
            {
                label = name + " " + (sortInfo.ascending ? Skin.UpArrow : Skin.DownArrow);
                if ( m_sorts.Count > 1 )
                {
                    label += sortIndex;
                }
            }
            else
            {
                label = name + "    ";
            }

            var rect = EditorGUILayout.GetControlRect(false, 16, m_skin.toolbarButtonLeftAlignedStyle, options);

            Rect closeButtonRect = new Rect();
            Rect sortIconRect = rect;

            float xMax = rect.xMax;

            if (closeable)
            {
                closeButtonRect = SuffixRect(rect, 13, 13, 2);
                xMax = closeButtonRect.xMin;
                sortIconRect.xMax -= 12;
            }

            var resizeRect = rect;
            resizeRect.width = 7;
            resizeRect.x -= 2;

            var close = false;

            if ( closeable )
            {
                EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);

                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if ( resizeRect.Contains(Event.current.mousePosition) )
                        {
                            m_dragProperty = property;
                        }
                        break;

                    case EventType.MouseDrag:
                        if ( m_dragProperty == property )
                        {
                            var delta = Event.current.delta;
                            width -= delta.x;
                        }
                        break;

                    case EventType.MouseUp:
                        if ( m_dragProperty == null && closeButtonRect.Contains(Event.current.mousePosition) )
                        {
                            close = true;
                        }
                        break;
                }
            }

            var style = new GUIStyle(m_skin.toolbarButtonLeftAlignedStyle);
            var originalPadding = style.padding;
            try
            {
                var newPadding = originalPadding;
                newPadding.right = Mathf.Max(newPadding.right, Mathf.CeilToInt(rect.xMax - xMax));
                style.padding = newPadding;

                if (close == true || m_dragProperty == property)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        style.Draw(rect, label, false, false, false, false);
                    }
                }
                else if (GUI.Button(rect, label, style))
                {
                    if (Event.current.control)
                    {
                        if (sortInfo != null)
                        {
                            sortInfo.ascending = !sortInfo.ascending;
                        }
                        else
                        {
                            m_sorts.Add(new SortInfo()
                            {
                                prop = property,
                                ascending = true
                            });
                        }
                        m_needsSorting = true;
                    }
                    else
                    {
                        if (m_sorts.Count != 1)
                        {
                            sortInfo = null;
                        }

                        if (sortInfo != null)
                        {
                            sortInfo.ascending = !sortInfo.ascending;
                            m_needsSorting = null;
                        }
                        else
                        {
                            m_sorts.Clear();
                            m_sorts.Add(new SortInfo()
                            {
                                prop = property,
                                ascending = true
                            });
                            m_needsSorting = true;
                        }
                    }
                }

                if (closeable && Event.current.type == EventType.Repaint)
                {
                    m_skin.closeButtonStyle.Draw(closeButtonRect, GUIContent.none, false, false, false, false);
                }
            }
            finally
            {
                style.padding = originalPadding;
            }


            return close;
        }

        

        private List<AssetEntry> Refresh(BuildInfo info, BuildInfo referenceInfo)
        {
            List<AssetEntry> result = new List<AssetEntry>();

            try
            {
                EditorUtility.DisplayProgressBar("Better Build Info", "Refresh", 0);

                // prepare
                foreach (var assetInfo in info.assets)
                {
                    var assetEntry = new AssetEntry
                    {
                        info = assetInfo,
                        size = FileSize.FromBytes(assetInfo.size),
                    };

                    result.Add(assetEntry);
                }

                if (referenceInfo != null)
                {
                    var referenceSizes = referenceInfo.assets.ToDictionary(x => x.path, x => FileSize.FromBytes(x.size));

                    foreach (var assetEntry in result)
                    {
                        FileSize referenceSize;
                        if (referenceSizes.TryGetValue(assetEntry.info.path, out referenceSize))
                        {
                            referenceSizes.Remove(assetEntry.info.path);
                        }
                        assetEntry.sizeDiff = assetEntry.size - referenceSize;
                    }

                    foreach (var missingEntry in referenceSizes)
                    {
                        result.Add(new AssetEntry()
                        {
                            info = new AssetInfo()
                            {
                                path = missingEntry.Key,
                            },
                            sizeDiff = -missingEntry.Value
                        });
                    }
                }

                m_sorts.RemoveAll(x => x.prop >= SortProperty.AssetsDetailsStart);
                m_categories = RefreshCategories(result);
                m_scenesAndBundles = RefreshScenesAndBundles(info, result);
                m_detailsInfo = RefreshDetailsInfo(result);
                m_assetsDirectoriesInfo = new AssetsDirectoriesInfo();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return result;
        }


        private ScenesAndBundlesGroupInfo RefreshScenesAndBundles(BuildInfo info, List<AssetEntry> entries)
        {
            var sortedScenes = info.scenes.OrderBy(x => x).ToArray();
            var sortedBundles = info.assetBundles.Select(x => x.path).ToArray();

            var sizes = new FileSize[sortedScenes.Length + sortedBundles.Length + 1];
            var counts = new int[sortedScenes.Length + sortedBundles.Length + 1];

            EditorUtility.DisplayProgressBar("Better Build Info", "Processing scenes & bundles...", 0);

            foreach (var assetEntry in entries)
            {
                // set the scenes mask
                assetEntry.scenesAndBundlesMask = new Internal.BitArray(sortedScenes.Length + sortedBundles.Length + 1, false);

                if (assetEntry.info.scenes.Count == 0 && !assetEntry.info.assetBundleOnly)
                {
                    // unreferenced
                    assetEntry.scenesAndBundlesMask[sortedScenes.Length] = true;
                    sizes[sortedScenes.Length] += assetEntry.size;
                    ++counts[sortedScenes.Length];
                }
                else
                {
                    foreach (var index in assetEntry.info.scenes.Select(x => Array.IndexOf(sortedScenes, x)))
                    {
                        if ( index < 0 )
                        {
                            throw new System.InvalidOperationException("Scene not found");
                        }
                        assetEntry.scenesAndBundlesMask[index] = true;
                        sizes[index] += assetEntry.size;
                        ++counts[index];
                    }
                }

                assetEntry.scenesAndBundlesMask[sortedScenes.Length] = !assetEntry.info.scenes.Any() && !assetEntry.info.assetBundleOnly;

                foreach (var index in assetEntry.info.assetBundles.Select(x => Array.IndexOf(sortedBundles, x.name)))
                {
                    if (index < 0)
                    {
                        throw new System.InvalidOperationException("Scene not found");
                    }
                    var effectiveIndex = index + sortedScenes.Length + 1;
                    assetEntry.scenesAndBundlesMask[effectiveIndex] = true;
                    sizes[effectiveIndex] += assetEntry.size;
                    ++counts[effectiveIndex];
                }
            }

            var sanitizedScenes = sortedScenes.Select(x =>
            {
                if (x.StartsWith("Assets/"))
                {
                    return x.Substring("Assets/".Length);
                }
                return x;
            });
            var sanitizedBundles = sortedBundles.Select(x =>
            {
                if (x.StartsWith("Assets/"))
                {
                    return x.Substring("Assets/".Length);
                }
                return x;
            });

            var groupInfo = new ScenesAndBundlesGroupInfo();
            var options = new List<string>();

            options.AddRange(sanitizedScenes);
            options.Add("Not Referenced");
            options.AddRange(sanitizedBundles);

            // as we have limited space for combo, place only scene names there
            groupInfo.names = options.Select(x => ReliablePath.GetFileName(x)).ToArray();

            groupInfo.selected = new Internal.BitArray(groupInfo.names.Length, true);

            options = options.Select(x => Skin.RichifyFileName(x)).ToList();

            // append menu entries with size / count
            for (int i = 0; i < options.Count; ++i)
            {
                options[i] = Skin.GetMenuOption(options[i], counts[i], sizes[i]);
            }

            groupInfo.sizes = sizes;
            groupInfo.counts = counts;
            groupInfo.dropDownContents = options.Select(x => new GUIContent(x)).ToArray();
            groupInfo.count = options.Count;
            groupInfo.hasBundles = sortedBundles.Length > 0;

            return groupInfo;
        }



        private CategoriesGroupInfo RefreshCategories(List<AssetEntry> entries)
        {
            EditorUtility.DisplayProgressBar("Better Build Info", "Preprocessing categories...", 0);

            var categoriesProcessed = Settings.categories.Select(category =>
            {
                return new
                {
                    name = category.name,
                    pathTests = category.filters.Select(x => WildcardTest.Create(x)).ToList(),
                };
            }).ToArray();

            // categories with same names may repeat...
            var uniqueSortedCategories = categoriesProcessed.Select(x => x.name).Distinct().OrderBy(x => x).ToArray();
            var counts = new int[uniqueSortedCategories.Length + 1];
            var sizes = new FileSize[uniqueSortedCategories.Length + 1];
            var sizeDiffs = new FileSize[uniqueSortedCategories.Length + 1];

            EditorUtility.DisplayProgressBar("Better Build Info", "Processing categories...", 0);

            for (int i = 0; i < entries.Count; ++i)
            {
                EditorUtility.DisplayProgressBar("Better Build Info", "Processing categories...", (float)i / entries.Count);

                var assetEntry = entries[i];

                var assetPath = assetEntry.info.path;
                assetEntry.categoryIndex = uniqueSortedCategories.Length;

                foreach (var category in categoriesProcessed)
                {
                    bool pathMatch = category.pathTests.Any(x => x.IsMatch(assetPath));

                    if (!pathMatch)
                        continue;

                    assetEntry.categoryIndex = Array.IndexOf(uniqueSortedCategories, category.name);
                    break;
                }

                ++counts[assetEntry.categoryIndex];
                sizes[assetEntry.categoryIndex] += assetEntry.size;
                sizeDiffs[assetEntry.categoryIndex] += assetEntry.sizeDiff;
            }

            // now mask groups
            List<string> options = new List<string>();
            options.AddRange(uniqueSortedCategories);
            options.Add("Unknown");

            List<Color> colors = new List<Color>();
            foreach (var categoryName in uniqueSortedCategories)
            {
                colors.Add(Settings.categories.First(x => x.name == categoryName).color);
            }
            colors.Add(Color.gray);

            var groupInfo = new CategoriesGroupInfo();
            groupInfo.names = options.ToArray();
            groupInfo.colors = colors.ToArray();
            groupInfo.gridLabels = groupInfo.names.Select(x => new GUIContent(x)).ToArray();
            groupInfo.selected = new Internal.BitArray(options.Count, true);

            groupInfo.overviewLabels = options.Select((x, i) => new GUIContent(x, m_skin.CreateCategoryIcon(colors[i]))).ToArray();

            // append menu entries with size / count
            for (int i = 0; i < options.Count; ++i)
            {
                options[i] = Skin.GetMenuOption(options[i], counts[i], sizes[i]);
            }

            groupInfo.sizes = sizes;
            groupInfo.counts = counts;
            groupInfo.dropDownContents = options.Select((x, i) => new GUIContent(x, m_skin.CreateCategoryIcon(colors[i]))).ToArray();
            groupInfo.count = options.Count;
            groupInfo.sizeDiffs = sizeDiffs;

            return groupInfo;
        }
        
        private void PreloadVisibleAssetsThumbnails(int timeoutMS)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var needsRepaint = false;

            foreach ( var source in m_visibleAssets )
            {
                needsRepaint |= source.PreloadThumbnail();
                if ( timer.ElapsedMilliseconds > timeoutMS )
                {
                    break;
                }
            }

            if ( needsRepaint )
            {
                Repaint();
            }
        }

        private AssetsDetailsInfo RefreshDetailsInfo(List<AssetEntry> entries)
        {
            var infoNames = new List<string>();
            var counts = new List<int>();
            var sizes = new List<FileSize>();
            var isNumeric = new List<bool>();
            var isSize = new List<bool>();

            EditorUtility.DisplayProgressBar("Better Build Info", "Processing extra info...", 0);

            foreach (var entry in entries)
            {
                if ( entry.info.details == null )
                    continue;

                foreach (var info in entry.info.details )
                {
                    var index = infoNames.BinarySearch(info.name);
                    if ( index < 0 )
                    {
                        index = ~index;
                        infoNames.Insert(index, info.name);
                        counts.Insert(index, 1);
                        sizes.Insert(index, entry.size);
                        isNumeric.Insert(index, true);
                        isSize.Insert(index, info.name.EndsWith("Size", StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        ++counts[index];
                        sizes[index] += entry.size;
                    }

                    double dummy;
                    if (isNumeric[index] && !Utils.TryParseInvariant(info.value, out dummy))
                    {
                        isNumeric[index] = false;
                        isSize[index] = false;
                    }
                }
            }

            foreach ( var entry in entries )
            {
                if ( entry.info.details == null || entry.info.details.Length == 0 )
                {
                    entry.detailsValues = null;
                }
                else
                {
                    entry.detailsValues = new string[infoNames.Count];

                    foreach ( var info in entry.info.details )
                    {
                        var index = infoNames.BinarySearch(info.name);
                        if (index < 0)
                        {
                            throw new System.InvalidOperationException("Extra info not found");
                        }

                        entry.detailsValues[index] = info.value;
                    }
                }
            }

            var groupInfo = new AssetsDetailsInfo();
            var options = new List<string>();

            options.AddRange(infoNames);

            // as we have limited space for combo, place only scene names there
            groupInfo.names = options.ToArray();

            groupInfo.selected = new Internal.BitArray(groupInfo.names.Length, false);
            {
                int compressedSizeIndex = options.IndexOf(BuildInfoProcessorUtils.CompressedSizeKey);
                if (compressedSizeIndex >= 0)
                    groupInfo.selected[compressedSizeIndex] = true;
            }

            options = options.Select(x => Skin.RichifyFileName(x)).ToList();

            // append menu entries with size / count
            for ( int i = 0; i < options.Count; ++i )
            {
                options[i] = Skin.GetMenuOption(options[i], counts[i], sizes[i]);
            }

            groupInfo.sizes = sizes.ToArray();
            groupInfo.counts = counts.ToArray();
            groupInfo.dropDownContents = options.Select(x => new GUIContent(x)).ToArray();
            groupInfo.count = options.Count;
            groupInfo.isNumeric = isNumeric.ToArray();
            groupInfo.isSize = isSize.ToArray();

            return groupInfo;
        }

        
        private void HandleDetailsColumnsButton(params GUILayoutOption[] options)
        {
            var style = m_skin.toolbarButtonLeftAlignedStyle;
            style = new GUIStyle(style);
            style.fontStyle = FontStyle.Bold;
            var rect = EditorGUILayout.GetControlRect(false, 16, style, options);

            using ( GUIHelpers.ContentColor(Color.green) )
            {
                if ( GUI.Button(rect, "+", style) )
                {
                    var popupContent = MenuPopupWindowContent.Create(m_skin.richMenuItem, Enumerable.Range(0, m_detailsInfo.count),
                        i => m_detailsInfo.dropDownContents[i],
                        i => m_detailsInfo.selected[i],
                        (i, v) =>
                        {
                            m_detailsInfo.selected[i] = v;
                            Repaint();
                        },
                        () =>
                        {
                            using ( GUIHelpers.Vertical(m_skin.header) )
                            {
                                EditorGUILayout.LabelField("TIP: To change the width of these columns, just drag left edge of a column", EditorStyles.wordWrappedMiniLabel);
                            }
                        },
                        true);

                    PopupWindow.Show(rect, popupContent);
                }
            }
        }

        private static long KiloToBytes(float kb)
        {
            return (long)(kb * 1024);
        }

        private static float BytesToKilo(long bytes)
        {
            return bytes / 1024.0f;
        }
    }


}