// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using Better.BuildInfo.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo
{
    internal class MenuPopupWindowContent : PopupWindowContent
    {
        private readonly GUIContent m_nothingContent = new GUIContent("Nothing");
        private readonly GUIContent m_everythingContent = new GUIContent("Everything");

        private GUIContent[] m_labels;

        public const float ItemHeight = 16f;

        private GUIStyle m_itemStyle;

        private object[] m_options;
        private Func<object, bool> m_getSelected;
        private Action<object, bool> m_setSelected;
        private Action m_footer;

        private float m_perfectWidth;
        private float m_perfectHeight;

        private int m_hoverIndex = -1;

        private Vector2 scroll;
        private bool m_multipleSelect;

        private int ActualOptionsCount
        {
            get { return m_options.Length + 2; }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(Mathf.Max(m_perfectWidth, 200), m_perfectHeight);
        }

        public static MenuPopupWindowContent Create<T>(GUIStyle itemStyle, IEnumerable<T> source, Func<T, GUIContent> getLabel, Func<T, bool> getSelected, Action<T, bool> setSelected, System.Action footer, bool multipleSelect = false)
        {
            var result = new MenuPopupWindowContent(itemStyle, source.Cast<object>(), x => getLabel((T)x), x => getSelected((T)x), (x, v) => setSelected((T)x, v), footer, multipleSelect);
            return result;
        }

        public MenuPopupWindowContent(GUIStyle itemStyle, IEnumerable<object> source, Func<object, GUIContent> getLabel, Func<object, bool> getSelected, Action<object, bool> setSelected, Action footer, bool multipleSelect)
        {
            m_multipleSelect = multipleSelect;
            m_itemStyle = itemStyle;


            m_options = source.ToArray();

            m_labels = source.Select(x => getLabel(x)).ToArray();

            m_getSelected = getSelected;
            m_setSelected = setSelected;
            m_footer = footer;

            if (m_labels.Any())
            {
                m_perfectWidth = m_labels.Max(x => m_itemStyle.CalcSize(x).x) + 25.0f;
            }

            m_perfectHeight = ItemHeight * (m_options.Length + 3) + (footer != null ? 50 : 0);
        }


        public override void OnGUI(Rect rect)
        {
            Event current = Event.current;

            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                this.editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            using (GUIHelpers.Area(rect))
            {
                using (GUIHelpers.Vertical())
                {
                    using (GUIHelpers.ScrollView(ref scroll))
                    {
                        DrawList();
                    }

                    if (m_footer != null)
                    {
                        m_footer();
                    }
                }
            }
        }

        private bool DoOption(int index, GUIContent content, bool isSelected)
        {
            var position = GetLayoutRectForEntry(content);

            var current = Event.current;
            EventType type = current.type;
            switch (type)
            {
                case EventType.MouseDown:
                    if (current.button == 0 && position.Contains(current.mousePosition))
                    {
                        current.Use();
                        if (!current.control && !m_multipleSelect)
                        {
                            editorWindow.Close();
                        }
                        return true;
                    }
                    break;
                case EventType.MouseMove:
                    if (position.Contains(current.mousePosition))
                    {
                        m_hoverIndex = index;
                        current.Use();
                    }
                    break;
                case EventType.Repaint:
                    {
                        var isHover = index == this.m_hoverIndex;
                        m_itemStyle.Draw(position, content, isHover, isSelected, isSelected, false);
                    }
                    break;
            }
            return false;
        }

        private Rect GetLayoutRectForEntry(GUIContent label)
        {
            return EditorGUILayout.GetControlRect(false, ItemHeight, m_itemStyle, GUILayout.ExpandWidth(true), GUILayout.MinWidth(m_itemStyle.CalcSize(label).x));
        }

        private void DrawList()
        {
            var selectionState = m_options.Select(m_getSelected).ToArray();

            // first nothing
            if (DoOption(0, m_nothingContent, selectionState.All(x => !x)))
            {
                for (int i = 0; i < m_options.Length; ++i)
                {
                    if (selectionState[i])
                    {
                        m_setSelected(m_options[i], false);
                    }
                }
            }

            // then everything
            if (DoOption(1, m_everythingContent, selectionState.All(x => x)))
            {
                for (int i = 0; i < m_options.Length; ++i)
                {
                    if (!selectionState[i])
                    {
                        m_setSelected(m_options[i], true);
                    }
                }
            }

            EditorGUILayout.Space();

            // and now the rest
            for (int i = 0; i < m_options.Length; ++i)
            {
                //position = EditorGUILayout.GetControlRect()
                if (DoOption(i + 2, m_labels[i], selectionState[i]))
                {
                    if (Event.current.control || m_multipleSelect)
                    {
                        //if (selectionState.Count(x => x) == 1 && selectionState[i])
                        //{
                        //    // can't untoggle the one and only
                        //}
                        //else
                        {
                            m_setSelected(m_options[i], !selectionState[i]);
                        }
                    }
                    else
                    {
                        // deselect every other
                        for (int j = 0; j < m_options.Length; ++j)
                        {
                            if (i != j && selectionState[j])
                                m_setSelected(m_options[j], false);
                        }
                        m_setSelected(m_options[i], true);
                    }
                }
            }


        }
    }
}
