using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal abstract class SetupWindowBase : UnityEditor.Editor
    {
        protected const int BOTTOM_SPACE = 70;
        protected const int TOGGLE_DIMENSION = 15;
        protected const int BUTTON_DIMENSION = 70;
        protected const int SCROLL_SPACE = 20;
        protected const int TOGGLE_WIDTH = 168;

        protected Vector2 _scrollPosition = Vector2.zero;
        protected SettingsWindowBase _window;
        private string _fullClassName;
        private string _windowTitle;
        private string _tutorialLink;
        private bool _enabled;
        private bool _showBack;
        private bool _showTitle;
        private bool _showTop;
        private bool _showScroll;
        private bool _showBottom;
        private bool _blockClicks;


        internal virtual SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _window = window;
            _fullClassName = windowProperties.NameSpace + "." + windowProperties.ClassName;
            _windowTitle = windowProperties.Title;
            _showBack = windowProperties.ShowBack;
            _showTitle = windowProperties.ShowTitle;
            _showTop = windowProperties.ShowTop;
            _showScroll = windowProperties.ShowScroll;
            _showBottom = windowProperties.ShowBottom;
            _tutorialLink = windowProperties.TutorialLink;
            _enabled = true;
            _blockClicks = windowProperties.BlockClicks;

            return this;
        }


        internal bool GetBlockClicksState()
        {
            return _blockClicks;
        }


        internal string GetWindowTitle()
        {
            return _windowTitle;
        }


        internal bool DrawInWindow(float width, float height)
        {
            if (_showBack)
            {
                Navigation();
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_showTitle)
            {
                WindowTitle();
            }
            if (_showTop)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                TopPart();
                EditorGUILayout.EndVertical();
            }

            if (_showScroll)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                ScrollPart(width, height);
                EditorGUILayout.EndVertical();
            }

            if (_showBottom)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                BottomPart();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            return _enabled;
        }


        internal virtual void DrawInScene()
        {

        }


        internal virtual void MouseMove(Vector3 mousePosition)
        {

        }


        internal virtual void LeftClick(Vector3 mousePosition, bool clicked)
        {

        }


        internal virtual void RightClick(Vector3 mousePosition)
        {

        }


        internal virtual void UndoAction()
        {

        }


        internal virtual void DestroyWindow()
        {

        }


        internal string GetFullClassName()
        {
            return _fullClassName;
        }


        internal virtual void InspectorUpdate()
        {


        }


        protected virtual void WindowTitle()
        {
            GUIStyle style = EditorStyles.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField(GetWindowTitle(), style);
            style.fontStyle = FontStyle.Normal;
            style.alignment = TextAnchor.MiddleLeft;
        }


        protected virtual void TopPart()
        {

        }


        protected virtual void ScrollPart(float width, float height)
        {

        }


        protected virtual void BottomPart()
        {
            if (GUILayout.Button("View Tutorial"))
            {
                Application.OpenURL(_tutorialLink);
            }
        }


        private void Navigation()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("<< Back", GUILayout.Width(BUTTON_DIMENSION)))
            {
                CloseWindow();
            }
            EditorGUILayout.LabelField(_window.GetBackPath() + GetWindowTitle());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }


        private void CloseWindow()
        {
            _enabled = false;
        }
    }
}
