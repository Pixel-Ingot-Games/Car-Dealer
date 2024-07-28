// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    /// <summary>
    /// Lots of scope methods do live in Unity 5.2+, but they're awkard to use.
    /// </summary>
    internal static class GUIHelpers
    {
        public delegate float PowerSliderDelegate(Rect position, string label, float sliderValue, float min, float max, float power);
        public static readonly PowerSliderDelegate PowerSlider;

        public delegate void DisplayCustomMenuDelegate(Rect position, string[] options, int[] selected, EditorUtility.SelectMenuItemFunction callback, object userData);
        public static readonly DisplayCustomMenuDelegate DisplayCustomMenu;

        public delegate string ToolbarSearchFieldDelegate(int id, Rect position, string text, bool showWithPopupArrow);
        public static readonly ToolbarSearchFieldDelegate ToolbarSearchField;

        public delegate bool ButtonWithDropdownListDelegate(GUIContent content, string[] buttonNames, GenericMenu.MenuFunction2 callback, params GUILayoutOption[] options);
        public static readonly ButtonWithDropdownListDelegate ButtonWithDropdownList;


        static GUIHelpers()
        {
            typeof(EditorGUI).CreateMethodDelegateOrThrow("PowerSlider", BindingFlags.NonPublic | BindingFlags.Static, out PowerSlider);
            typeof(EditorUtility).CreateMethodDelegateOrThrow("DisplayCustomMenu", BindingFlags.NonPublic | BindingFlags.Static, out DisplayCustomMenu);
            typeof(EditorGUI).CreateMethodDelegateOrThrow("ToolbarSearchField", BindingFlags.NonPublic | BindingFlags.Static, out ToolbarSearchField);
            typeof(EditorGUI).CreateMethodDelegateOrThrow("ButtonWithDropdownList", BindingFlags.NonPublic | BindingFlags.Static, out ButtonWithDropdownList);
        }

        internal static IDisposable Vertical(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
            return DisposableAction.Create(EditorGUILayout.EndVertical);
        }

        internal static IDisposable Vertical(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            return DisposableAction.Create(EditorGUILayout.EndVertical);
        }

        internal static IDisposable Horizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            return DisposableAction.Create(EditorGUILayout.EndHorizontal);
        }

        internal static IDisposable Horizontal(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            return DisposableAction.Create(EditorGUILayout.EndHorizontal);
        }

        public static IDisposable Enabled(bool enabled)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && enabled;
            return DisposableAction.Create(() => GUI.enabled = wasEnabled);
        }

        public static IDisposable Area(Rect rect)
        {
            GUILayout.BeginArea(rect);
            return DisposableAction.Create(() => GUILayout.EndArea());
        }

        public static IDisposable LabelWidth(float width)
        {
            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
            return DisposableAction.Create(() => EditorGUIUtility.labelWidth = prevWidth);
        }

        public static IDisposable ContentColor(Color color)
        {
            var oldColor = GUI.contentColor;
            GUI.contentColor = color;
            return DisposableAction.Create(() => GUI.contentColor = oldColor);
        }

        public static IDisposable Color(Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            return DisposableAction.Create(() => GUI.color = oldColor);
        }

        public static IDisposable BackgroundColor(Color color)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            return DisposableAction.Create(() => GUI.backgroundColor = oldColor);
        }

        public static IDisposable ScrollView(ref Vector2 scroll)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            return DisposableAction.Create(EditorGUILayout.EndScrollView);
        }

        public static IDisposable ScrollView(ref Vector2 scroll, GUIStyle style, params GUILayoutOption[] options)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, style, options);
            return DisposableAction.Create(EditorGUILayout.EndScrollView);
        }

        public static IDisposable ScrollViewWindow(Vector2 scroll)
        {
            EditorGUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);
            return DisposableAction.Create(EditorGUILayout.EndScrollView);
        }

        public static string ToolbarSearchFieldLayout(string text, bool showWithPopupArrow, params GUILayoutOption[] options)
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUIHelpers.GetStyleOrThrow("ToolbarSeachTextField"), options);
            var controlId = GUIUtility.GetControlID("ToolbarSearchFieldLayout".GetHashCode(), FocusType.Passive, rect);
            return GUIHelpers.ToolbarSearchField(controlId, rect, text, showWithPopupArrow);
        }

        public static void FastScroll(ref Vector2 scroll, int itemsCount, float lineHeight, int? scrollToIndex, float visibleHeight,
            System.Action<int> a, params GUILayoutOption[] options)
        {
            if (scrollToIndex.HasValue)
            {
                var firstFullyVisibleIndex = Mathf.CeilToInt(scroll.y / lineHeight);
                var lastPossibleIndex = Mathf.FloorToInt((scroll.y + visibleHeight) / lineHeight);

                var selectedIndex = Mathf.Abs(scrollToIndex.Value);

                if (selectedIndex == lastPossibleIndex)
                {
                    scroll.y = (lastPossibleIndex + 1) * lineHeight - visibleHeight;
                }
                else if (selectedIndex < firstFullyVisibleIndex || selectedIndex > lastPossibleIndex)
                {
                    scroll.y = selectedIndex * lineHeight;
                    if (scrollToIndex.Value < 0)
                        scroll.y -= visibleHeight - lineHeight;
                }
            }

            scroll.y = Mathf.Max(Mathf.Min(scroll.y, itemsCount * lineHeight - visibleHeight), 0);

            using (ScrollView(ref scroll, new GUIStyle(), options))
            {
                var index = Mathf.FloorToInt(scroll.y / lineHeight);

                // fill for invisible elements
                GUILayout.Space(index * lineHeight);

                float heightLeft = visibleHeight;
                for ( ; index < itemsCount && heightLeft > 0.1f; index++, heightLeft -= lineHeight)
                {
                    using (Horizontal(GUILayout.Height(lineHeight)))
                    {
                        a(index);
                    }
                }

                // fill for invisible elements
                GUILayout.Space(Mathf.Max(0, (itemsCount - index) * lineHeight));
                GUILayout.FlexibleSpace();
            }
        }

        public static int StaticLabelLayout(GUIContent content, GUIStyle style, bool selected, params GUILayoutOption[] options)
        {
            var rect = GUILayoutUtility.GetRect(content, style, options);
            return StaticLabel(rect, content, style, selected);
        }


        public static int StaticLabel(Rect position, GUIContent content, GUIStyle style, bool selected)
        {
            EventType eventType = Event.current.type;
            switch (eventType)
            {
                case EventType.Repaint:
                    style.Draw(position, content, false, false, selected, false);
                    break;

                //case EventType.mouseDown:
                //    if ( position.Contains(Event.current.mousePosition) )
                //    {
                //        var result = Event.current.clickCount;
                //        Event.current.Use();
                //        return result;
                //    }
                //    break;

                default:
                    break;
            }

            return 0;
        }

        public static void HierarchyLikeLabel<T>(Rect position, GUIContent content, int controlId, GUIStyle style, bool selected, T context, System.Action<int, T> onClicked = null, float leftDrawOffset = 0, System.Action<KeyCode, T> onKeyDown = null)
        {
            // taken from reflected EditorGUI.indent

            EventType eventType = Event.current.type;
            switch (eventType)
            {
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == controlId)
                    {
                        if (onKeyDown != null)
                        {
                            onKeyDown(Event.current.keyCode, context);
                        }
                    }
                    break;

                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlId;

                        if (onClicked != null)
                        {
                            onClicked(Event.current.clickCount, context);
                        }

                        Event.current.Use();

                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    position.x += leftDrawOffset;
                    position.width -= leftDrawOffset;
                    style.Draw(position, content, controlId, selected);
                    break;

                default:
                    break;
            }
        }

        public static float Indent
        {
            get { return EditorGUI.indentLevel * 15.0f; }
        }

        public static GUIStyle GetStyleOrThrow(string styleName)
        {
            GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (guiStyle == null)
            {
                throw new System.ArgumentOutOfRangeException(styleName);
            }
            return guiStyle;
        }

        public static GUIStyle GetStyle(string styleName)
        {
            return GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }

        public static string GenerateDropdownLabel(BitArray selectedMask, string[] labels)
        {
            string content;
            List<int> selected = new List<int>();

            for (int i = 0; i < selectedMask.Length; ++i)
            {
                if (selectedMask[i])
                {
                    selected.Add(i);
                }
            }

            if (selected.Count == 0)
            {
                selected.Add(0);
                content = "Nothing";
            }
            else if (selected.Count == selectedMask.Length)
            {
                selected.Add(1);
                content = "Everything";
            }
            else if (selected.Count == 1)
            {
                content = labels[selected[0]];
            }
            else
            {
                content = "Mixed (" + string.Join(", ", selected.Select(x => labels[x]).ToArray()) + ")";
            }

            return content;
        }
    }
}
