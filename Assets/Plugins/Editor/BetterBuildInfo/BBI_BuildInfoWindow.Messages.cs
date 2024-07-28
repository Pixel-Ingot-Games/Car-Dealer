// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using Better.BuildInfo.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo
{
    public partial class BuildInfoWindow
    {
        private List<string> m_prevSelectedAssets = new List<string>();
        private Texture2D m_icon;
        private AssetsDirectoriesInfo m_assetsDirectoriesInfo = new AssetsDirectoriesInfo();



        [MenuItem("Window/Better/Better Build Info")]
        public static void ShowMenuItem()
        {
            var window = EditorWindow.GetWindow<BuildInfoWindow>();
            window.Show();
        }

        [MenuItem("Window/Better/Better Build Info (New Instance)")]
        public static void ShowNewInstanceMenuItem()
        {
            var window = EditorWindow.CreateInstance<BuildInfoWindow>();
            window.Show();
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void OnEnable()
        {
            if ( m_icon == null )
            {
                m_icon = Skin.LoadTexture("BBI_iconWindowContent.png");
            }

            UnityVersionAgnostic.SetWindowTitle(this, new GUIContent("Build Info", m_icon));
            ResetProjectWindowItemCallback();
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void OnFocus()
        {
            ResetProjectWindowItemCallback();
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void OnDestroy()
        {
            EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemCallback;
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void OnGUI()
        {
            if ( !EnsureSkin() )
            {
                return;
            }

            try
            {
                using ( GUIHelpers.Horizontal(m_skin.inspectorBigStyle, GUILayout.Height(45.0f)) )
                {
                    {
                        var r = GUILayoutUtility.GetRect(32, 32, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                        GUI.DrawTexture(r, m_skin.iconHeader);
                    }

                    using ( GUIHelpers.Vertical() )
                    {
                        using ( GUIHelpers.Horizontal() )
                        {
                            if ( string.IsNullOrEmpty(m_openFilePath) )
                            {
                                EditorGUILayout.LabelField(string.Format(m_skin.titleContent, BetterBuildInfo.Version), m_skin.pathStyle, GUILayout.MinWidth(1.0f));
                            }
                            else
                            {
                                EditorGUILayout.LabelField(m_openFilePath, m_skin.pathStyle, GUILayout.MinWidth(1.0f));
                            }

                        }

                        using ( GUIHelpers.Horizontal() )
                        {
                            var recentReports = BuildInfoSettings.RecentReports;
                            var recentReportsOptions = recentReports
                                .Select(x => x.Replace("/", Skin.MenuSafeSlash)).ToArray();

                            if (GUIHelpers.ButtonWithDropdownList(m_skin.openReportContent, recentReportsOptions, 
                                x => OpenFile(recentReports[(int)x]), GUILayout.Width(130)))
                            {
                                OpenFile();
                            }

                            using (GUIHelpers.Enabled(m_buildInfo != null))
                            {
                                if (GUIHelpers.ButtonWithDropdownList(m_skin.openReportRefenceContent, recentReportsOptions,
                                    x => OpenReferenceFile(recentReports[(int)x]), GUILayout.Width(130)))
                                {
                                    OpenReferenceFile();
                                }
                            }

                            {
                                var rect = GUILayoutUtility.GetRect(m_skin.optionsContent, GUI.skin.button, GUILayout.Width(130));
                                if ( GUI.Button(rect, m_skin.optionsContent, GUI.skin.button) )
                                {
                                    GUIContent[] options = new[]
                                    {
                                            m_skin.openSettingsContent,
                                            m_skin.reloadSettingsContent,
                                    };

                                    EditorUtility.DisplayCustomMenu(rect, options, -1,
                                        (userData, opts, selected) =>
                                        {
                                            if ( selected == 0 )
                                            {
                                                EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow")).Show(true);
                                                BuildInfoSettings.EnsureAsset();
                                                EditorGUIUtility.PingObject(Settings);
                                                Selection.activeObject = Settings;
                                            }
                                            else if ( selected == 1 )
                                            {
                                                m_error = null;
                                                m_needsRefresh = true;
                                            }
                                        },
                                        null);
                                }
                            }

                            if (!EditorPrefs.GetBool("BBI_LeaveReviewDismissed", false))
                            {
                                if (GUIHelpers.ButtonWithDropdownList(m_skin.reviewContent, new[] { m_skin.openStorePageContent, m_skin.dismissContent }, x =>
                                {
                                    if ((int)x == 0)
                                    {
                                        Help.BrowseURL(m_skin.storeUrl);
                                    }
                                    else
                                    {
                                        EditorPrefs.SetBool("BBI_LeaveReviewDismissed", true);
                                    }
                                }, GUILayout.Width(130)
                                ))
                                {
                                    Help.BrowseURL(m_skin.storeUrl);
                                }
                            }

                            GUILayout.FlexibleSpace();

                            {
                                if ( GUILayout.Button("support@dmprog.pl", EditorStyles.miniLabel) )
                                {
                                    Help.BrowseURL("mailto:support@dmprog.pl?subject=BetterBuildInfo");
                                }
                            }

                        }
                    }
                }

                if ( string.IsNullOrEmpty(m_openFilePath) )
                {
                    DoStartScreenGUI();
                }
                else
                {
                    DoBuildInfoGUI();
                }
            }
            catch ( UnityEngine.ExitGUIException )
            {
                // don't log it, just rethrow
                throw;
            }
            catch ( System.Exception ex )
            {
                Debug.LogException(ex);
                m_error = ex;
            }

            if ( m_error != null )
            {
                try
                {
                    // disabled the error message, for now
                    // EditorGUILayout.HelpBox("There was an error: " + m_error.Message + "\nCheck console for details.", MessageType.Error);
                }
                catch ( System.Exception ) { }
            }
        }

        private bool TryRefreshSelection()
        {
            if ( m_needsSelectionRefresh )
            {
                var selectedEntries = new HashSet<AssetEntry>(m_assets.Where(x => x.selected));

                m_selectionDependencies.Clear();
                m_selectionDependencies.AddRange(selectedEntries.SelectMany(x => x.dependencies).Distinct()
                    .Where(x => !selectedEntries.Contains(x)).OrderBy(x => x.info.path));

                m_selectionReferences.Clear();
                m_selectionReferences.AddRange(selectedEntries.SelectMany(x => x.references).Distinct()
                    .Where(x => !selectedEntries.Contains(x)).OrderBy(x => x.info.path));

                m_selectionScenes.Clear();
                m_selectionScenes.AddRange(selectedEntries.SelectMany(x => x.scenes).Distinct().OrderBy(x => x.info.path));
                m_selectionScenes.AddRange(selectedEntries.SelectMany(x => x.assetBundles).Distinct().OrderBy(x => x.info.path));

                m_needsSelectionRefresh = false;

                if (Settings.syncSelection)
                {
                    Selection.objects = selectedEntries.Select(x => x.asset).Where(x => x != null).ToArray();
                }

                return true;
            }

            return false;
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void OnInspectorUpdate()
        {
            PreloadVisibleAssetsThumbnails(15);
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void Update()
        {
            try
            {
                if ( m_needsRefresh )
                {
                    try
                    {
                        if ( m_buildInfo != null )
                        {
                            m_assets = Refresh(m_buildInfo, m_referenceBuildInfo);
                            m_assetsByPath = m_assets.ToDictionary(x => x.info.path);

                            m_needsDataPostprocessing = true;
                        }
                    }
                    finally
                    {
                        m_needsRefresh = false;
                        m_needsSorting = true;
                    }
                }

                if ( m_needsDataPostprocessing )
                {
                    foreach ( var asset in m_assets )
                    {
                        asset.dependencies.Clear();
                        asset.references.Clear();
                        asset.scenes.Clear();
                    }

                    int controlId = 20000000;
                    foreach ( var asset in m_assets )
                    {
                        asset.controlId = controlId++;

                        foreach ( var dep in asset.info.dependencies )
                        {
                            AssetEntry info;
                            if ( m_assetsByPath.TryGetValue(dep, out info) )
                            {
                                asset.dependencies.Add(info);
                                info.references.Add(asset);
                            }
                        }

                        foreach ( var sceneName in asset.info.scenes )
                        {
                            AssetEntry scene;
                            if ( m_assetsByPath.TryGetValue(sceneName, out scene) )
                            {
                                asset.scenes.Add(scene);
                            }
                        }

                        foreach (var assetBundle in asset.info.assetBundles)
                        {
                            AssetEntry bundle;
                            if (m_assetsByPath.TryGetValue(assetBundle.name, out bundle))
                            {
                                asset.assetBundles.Add(bundle);
                            }
                        }
                    }

                    m_needsDataPostprocessing = false;
                    m_needsSelectionRefresh = true;
                }

                if ( m_needsSorting != false )
                {
                    if ( m_needsSorting == true )
                    {
                        Sort(m_assets, m_sorts);
                    }
                    else
                    {
                        // just reverse
                        m_assets.Reverse();
                    }

                    m_needsSorting = false;
                    m_needsFiltering = true;
                }

                if ( m_needsFiltering )
                {
                    var minSize = FileSize.FromKiloBytes(ui.minSizeMB * 1024.0f);

                    FileSize? maxSize = null;
                    if ( !Utils.NearlyEqual(ui.maxSizeMB, Skin.MaxSizeMB) )
                    {
                        maxSize = FileSize.FromKiloBytes(ui.maxSizeMB * 1024.0f);
                    }

                    m_categoriesTree = null;
                    m_filteredAssets.Clear();

                    FilterAssets(m_filteredAssets, m_assets, minSize, maxSize, m_categories.selected, m_scenesAndBundles.selected, ui.exclusiveScenesFilter, ui.assetsFilter, m_detailsInfo.names);

                    m_filteredTotalSize = m_filteredAssets.SumFileSize(x => x.size);

                    m_needsFiltering = false;
                    ui.prevSelectedIndex = -1;
                }

                TryRefreshSelection();
            }
            catch ( System.Exception ex )
            {
                Debug.LogException(ex);
                m_error = ex;
            }
        }

        [System.Reflection.Obfuscation(Exclude = true)]
        void OnSelectionChange()
        {
            if (!Settings.syncSelection)
            {
                return;
            }

            var assets = new List<string>(Selection.assetGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x)));
            assets.Sort();

            bool hadChanges = false;

            if ( !assets.SequenceEqual(m_prevSelectedAssets) )
            {
                foreach ( var removed in m_prevSelectedAssets.Where(x => assets.BinarySearch(x) < 0) )
                {
                    AssetEntry entry;
                    if ( m_assetsByPath.TryGetValue(removed, out entry) )
                    {
                        if ( entry.selected )
                        {
                            entry.selected = false;
                            hadChanges = true;
                        }
                    }
                }

                foreach ( var added in assets.Where(x => m_prevSelectedAssets.BinarySearch(x) < 0) )
                {
                    AssetEntry entry;
                    if ( m_assetsByPath.TryGetValue(added, out entry) )
                    {
                        if ( ui.scrollToIndex == null )
                        {
                            int index = m_filteredAssets.IndexOf(entry);
                            if ( index >= 0 )
                            {
                                ui.scrollToIndex = index;
                            }
                        }

                        if ( !entry.selected )
                        {
                            entry.selected = true;
                            hadChanges = true;
                        }
                    }
                }
            }

            m_prevSelectedAssets = assets;

            if ( hadChanges )
            {
                m_needsSelectionRefresh = true;
                TryRefreshSelection();

                ui.prevSelectedIndex = -1;
                Repaint();
            }
        }

        private void ProjectWindowItemCallback(string guid, Rect selectionRect)
        {
            if (!m_skin.loaded)
                return;

            if (m_assetsDirectoriesInfo.IsEmpty)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
                return;

            int usedCount, assetsCount, editorAssetsCount;
            FileSize size;

            if (Directory.Exists(path))
            {
                path = path.TrimEnd('/', '\\');

                if (m_assetsDirectoriesInfo.GetDirectoryInfo(path, out assetsCount, out usedCount, out editorAssetsCount, out size))
                {
                    Color backgroundColor;
                    GUIContent content;

                    if (assetsCount == 0)
                    {
                        if (editorAssetsCount == 0)
                        {
                            backgroundColor = m_skin.overlayUnusedColor;
                            content = m_skin.overlayEmptyContent;
                        }
                        else
                        {
                            backgroundColor = m_skin.overlaySpecialColor;
                            content = m_skin.overlayHasSpecialContent;
                        }
                    }
                    else
                    {
                        backgroundColor = Color.Lerp(m_skin.overlayUnusedColor, m_skin.overlayUsedColor, (float)usedCount / assetsCount);
                        content = new GUIContent(string.Format("{0}/{1} | {2}", usedCount, assetsCount, size));
                    }

                    using (GUIHelpers.BackgroundColor(backgroundColor))
                    {
                        GUIHelpers.StaticLabel(selectionRect, content, m_skin.projectOverlayMask, false);
                    }
                }
                else if (AssetsDirectoriesInfo.IsInSpecialEditorDirectory(path))
                {
                    using (GUIHelpers.BackgroundColor(m_skin.overlaySpecialColor))
                    {
                        GUIHelpers.StaticLabel(selectionRect, m_skin.overlaySpecialContent, m_skin.projectOverlayMask, false);
                    }
                }
            }
            else if (m_assetsByPath != null)
            {
                AssetEntry asset;
                if (m_assetsByPath.TryGetValue(path, out asset))
                {
                    using (GUIHelpers.BackgroundColor(m_skin.overlayUsedColor))
                    {
                        GUIHelpers.StaticLabel(selectionRect, asset.sizeContent, m_skin.projectOverlayMask, false);
                    }
                }
                else
                {
                    var directory = ReliablePath.GetDirectoryName(path);
                    if (m_assetsDirectoriesInfo.GetDirectoryInfo(directory, out assetsCount, out usedCount, out editorAssetsCount, out size))
                    {
                        using (GUIHelpers.BackgroundColor(m_skin.overlayUnusedColor))
                        {
                            GUIHelpers.StaticLabel(selectionRect, m_skin.overlayUnusedContent, m_skin.projectOverlayMask, false);
                        }
                    }
                    else if (AssetsDirectoriesInfo.IsInSpecialEditorDirectory(path))
                    {
                        using (GUIHelpers.BackgroundColor(m_skin.overlaySpecialColor))
                        {
                            GUIHelpers.StaticLabel(selectionRect, m_skin.overlaySpecialContent, m_skin.projectOverlayMask, false);
                        }
                    }
                }
            }
        }

        private static bool IsValidAsset(string assetPath)
        {
            if ( !assetPath.StartsWith("Assets") )
                return false;

            if ( assetPath.Contains("/Editor/") )
                return false;

            if ( assetPath.StartsWith("Assets/StreamingAssets") )
                return false;

            return true;
        }

        private static bool IsDirectory(string assetPath)
        {
            if ( Directory.Exists(assetPath) )
                return true;
            return false;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_assetsByPath = m_assets.ToDictionary(x => x.info.path);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        private bool EnsureSkin()
        {
            if ( m_skin.loaded )
            {
                if ( m_skin.NeedsReload() )
                {
                    m_skin = new Skin();
                }
            }

            if ( !m_skin.loaded && m_skinError == null )
            {
                try
                {
                    m_skin.Reload();
                }
                catch ( System.Exception ex )
                {
                    m_skinError = ex;
                    Debug.LogException(ex);
                }
            }

            if ( m_skinError != null )
            {
                EditorGUILayout.HelpBox("There's been an error loading the skin (see console for details).\n" +
                    "Make sure BetterBuildInfo is located in " + BuildInfoPaths.Base + " and then reopen this window.", MessageType.Error);

                return false;
            }

            return true;
        }

        private void OnAssetEntryKeyUp(KeyCode key, AssetEntry context)
        {
            var obj = context.asset;
            int entryIndex = m_filteredAssets.IndexOf(context);

            int delta = 0;

            if ( key == KeyCode.UpArrow )
            {
                delta = -1;
            }
            else if ( key == KeyCode.DownArrow )
            {
                delta = 1;
            }
            else if ( key == KeyCode.PageUp )
            {
                delta = -Mathf.FloorToInt(ui.gridHeight / Skin.LineHeight);
            }
            else if ( key == KeyCode.PageDown )
            {
                delta = +Mathf.FloorToInt(ui.gridHeight / Skin.LineHeight);
            }
            else if ( key == KeyCode.Home )
            {
                delta = -m_filteredAssets.Count;
            }
            else if (key == KeyCode.End)
            {
                delta = +m_filteredAssets.Count;
            }

            var newIndex = Mathf.Clamp(entryIndex + delta, 0, m_filteredAssets.Count - 1);

            if (newIndex == entryIndex)
                return;

            var scrollMode = delta > 0 ? ScrollMode.SelectionLast : ScrollMode.SelectionFirst;

            if (Event.current.shift || EditorGUI.actionKey)
            {
                if (ui.prevSelectedIndex != -1)
                {
                    SelectRange(Mathf.Min(ui.prevSelectedIndex, newIndex), Mathf.Max(ui.prevSelectedIndex, newIndex), scrollTo: newIndex, scrollMode: scrollMode, needsRefresh: false);
                }
            }
            else
            {
                // we'll refresh onKeyUp
                SelectRange(newIndex, newIndex, scrollTo: newIndex, scrollMode: scrollMode, needsRefresh: false);
                ui.prevSelectedIndex = newIndex;
            }

            Event.current.Use();
            GUIUtility.keyboardControl = m_filteredAssets[newIndex].controlId;
            Repaint();
        }

        private void OnAssetEntryClicked(int clickCount, AssetEntry context)
        {
            OnAssetEntryClickedEx(clickCount, context, false, !Settings.doubleClickSelect);
        }

        private void OnAssetEntryClickedEx(int clickCount, AssetEntry context, bool enableShiftSelection, bool singleClickSelect)
        {
            var obj = context.asset;
            int entryIndex = m_filteredAssets.IndexOf(context);
            bool changed = false;

            try
            {
                if ( EditorGUI.actionKey )
                {
                    if ( entryIndex < 0 )
                    {
                        return;
                    }

                    changed = true;
                    context.selected = !context.selected;

                    if ( context.selected )
                    {
                        ui.scrollToIndex = entryIndex;
                        ui.prevSelectedIndex = entryIndex;
                        if (obj && !Selection.Contains(obj) && Settings.syncSelection)
                        {
                            Selection.objects = Selection.objects.Concat(Enumerable.Repeat(obj, 1)).ToArray();
                        }
                    }
                    else
                    {
                        ui.prevSelectedIndex = -1;
                        if (obj && Selection.Contains(obj) && Settings.syncSelection)
                        {
                            Selection.objects = Selection.objects.Where(x => x != obj).ToArray();
                        }
                    }
                }
                else if ( enableShiftSelection && Event.current.shift )
                {
                    int startIndex, endIndex;
                    if (HandleShiftSelection(entryIndex, out startIndex, out endIndex))
                    {
                        SelectRange(startIndex, endIndex, scrollTo: entryIndex);
                        changed = true;
                    }
                }
                else
                {
                    if (obj && clickCount == 1 && Settings.syncSelection)
                    {
                        EditorGUIUtility.PingObject(obj);
                    }

                    if ( singleClickSelect ? clickCount == 1 : clickCount == 2 )
                    {
                        SelectRange(entryIndex, entryIndex, scrollTo: entryIndex, scrollMode: ScrollMode.SelectionFirst);
                        
                        ui.prevSelectedIndex = entryIndex;
                        changed = true;
                    }
                }
            }
            finally
            {
                if ( changed )
                {
                    m_needsSelectionRefresh = true;
                }
            }
        }

        private bool HandleShiftSelection(int entryIndex, out int startIndex, out int endIndex)
        {
            startIndex = endIndex = -1;

            if (entryIndex < 0)
                return false;

            if (entryIndex == ui.prevSelectedIndex)
            {
                return false;
            }
            else
            {
                var firstIndex = m_filteredAssets.TakeWhile(x => !x.selected).Count();

                if (firstIndex < 0)
                {
                    return false;
                }
                else
                {
                    var lastIndex = m_filteredAssets.Count - Enumerable.Reverse(m_filteredAssets).TakeWhile(x => !x.selected).Count() - 1;

                    if (entryIndex > lastIndex)
                    {
                        startIndex = firstIndex;
                        endIndex = entryIndex;
                    }
                    else if (entryIndex >= firstIndex && entryIndex < lastIndex)
                    {
                        if (ui.prevSelectedIndex > entryIndex)
                        {
                            startIndex = entryIndex;
                            endIndex = lastIndex;
                        }
                        else
                        {
                            startIndex = firstIndex;
                            endIndex = entryIndex;
                        }
                    }
                    else
                    {
                        startIndex = entryIndex;
                        endIndex = lastIndex;
                    }

                    return true;
                }
            }
        }

        private enum ScrollMode
        {
            SelectionFirst,
            SelectionLast
        }


        private void SelectRange(int startIndex, int endIndex, int? scrollTo = null, ScrollMode scrollMode = ScrollMode.SelectionFirst, bool needsRefresh = true)
        {
            foreach (var otherAsset in m_assets)
            {
                otherAsset.selected = false;
            }

            if ( startIndex >= 0 && endIndex >= 0 )
            {
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    m_filteredAssets[i].selected = true;
                }

                if (scrollTo != null )
                {
                    ui.scrollToIndex = scrollTo.Value;
                    ui.scrollMode = scrollMode;
                }
            }

            if (needsRefresh)
                m_needsSelectionRefresh = true;
        }

        private void ResetProjectWindowItemCallback()
        {
            EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemCallback;
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemCallback;
        }
    }
}
