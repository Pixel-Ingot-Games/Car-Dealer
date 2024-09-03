using System;
using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal abstract class SettingsWindowBase : EditorWindow
    {
        private static bool _firstAnchorClicked;

        private WindowProperties[] _allWindowsData;
        private AllSettingsWindows _allSettingsWindows;
        private NavigationRuntimeData _backData;
        private SceneCameraTracker _sceneCameraTracker;
        private SettingsWindowBase _window;
        private Type _defaultWindow;
        private RaycastHit _hitInfo;
        private bool _playState;
        private bool _blockClicks;
        private bool _canClick;
        private bool _initialized;

        protected SetupWindowBase _activeSetupWindow;

        internal delegate void RefreshWindow();
        internal static RefreshWindow OnRefreshWindow;
        internal static void TriggerRefreshWindowEvent()
        {
            OnRefreshWindow?.Invoke();
        }

        internal abstract LayerMask GetGroundLayer();
        protected abstract void Reinitialize();
        protected abstract void MouseMove(Vector3 point);


        #region Initialization
        protected void Init(SettingsWindowBase window, Type defaultWindowType, WindowProperties[] allWindowsProperties, AllSettingsWindows allSettingsWindows)
        {
            _allSettingsWindows = allSettingsWindows;
            _initialized = false;
            _window = window;
            _allWindowsData = allWindowsProperties;
            _defaultWindow = defaultWindowType;
            _sceneCameraTracker = new SceneCameraTracker();
            ResetToHomeScreen(_defaultWindow, false);
        }


        protected virtual void OnEnable()
        {
            OnRefreshWindow -= Refresh;
            OnRefreshWindow += Refresh;
            SceneView.duringSceneGui += OnScene;
        }


        protected virtual void OnDisable()
        {
            OnRefreshWindow -= Refresh;
            BlockClicks(false);
            SceneView.duringSceneGui -= OnScene;
        }


        protected virtual void OnFocus()
        {
            ResetToHomeScreen(_defaultWindow, false);
        }


        protected virtual void ResetToHomeScreen(Type defaultWindow, bool now)
        {
            if (defaultWindow == null || _allSettingsWindows == null || _allWindowsData == null)
            {
                _initialized = false;
                Reinitialize();
            }
            else
            {
                if (!now)
                {
                    if (_initialized == true)
                        return;
                }
                _initialized = true;
                _playState = Application.isPlaying;
                _allSettingsWindows.Initialize(_allWindowsData);
                _backData = new NavigationRuntimeData(_allSettingsWindows);
                SetActiveWindow(defaultWindow, false);
                SceneView.RepaintAll();
            }
        }
        #endregion


        #region WindowNavigation
        internal void SetActiveWindow(Type windowType, bool addCurrent)
        {
            if (windowType == null)
            {
                return;
            }

            if (_activeSetupWindow != null)
            {
                _activeSetupWindow.DestroyWindow();
            }

            if (addCurrent)
            {
                _backData.AddWindow(_activeSetupWindow.GetFullClassName());
            }
            _activeSetupWindow = ((SetupWindowBase)CreateInstance(windowType)).Initialize(_allSettingsWindows.GetWindowProperties(windowType.Name), this);
            BlockClicks(_activeSetupWindow.GetBlockClicksState());
            if (_window)
            {
                _window.Repaint();
            }
        }


        internal string GetBackPath()
        {
            return _backData.GetBackPath();
        }


        private void Back()
        {
            SetActiveWindow(Type.GetType(_backData.RemoveLastWindow()), false);
        }
        #endregion


        #region WindowGUI
        protected virtual void OnGUI()
        {
            if (_playState != Application.isPlaying)
            {
                ResetToHomeScreen(_defaultWindow, true);
            }
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.Space();

            if (_activeSetupWindow == null)
            {
                if (_defaultWindow == null)
                {
                    return;
                }
                ResetToHomeScreen(_defaultWindow, false);
            }

            if (_activeSetupWindow.DrawInWindow(position.width, position.height) == false)
            {
                Back();
            }
        }


        private void Refresh()
        {
            if (_window)
            {
                _window.Repaint();
            }
        }
        #endregion


        #region SceneDisplay
        protected virtual void OnScene(SceneView obj)
        {
            if (_playState != Application.isPlaying)
            {
                ResetToHomeScreen(_defaultWindow, true);
            }

            if (GleyPrefabUtilities.PrefabChanged())
            {
                ResetToHomeScreen(_defaultWindow, true);
            }

            if (_blockClicks == false)
                return;

            Color handlesColor = Handles.color;
            Matrix4x4 handlesMatrix = Handles.matrix;
            Draw();
            Input();
            _sceneCameraTracker.MoveCheck();
            Handles.color = handlesColor;
            Handles.matrix = handlesMatrix;
        }


        internal void BlockClicks(bool block)
        {
            if (_window)
            {
                _window._blockClicks = block;
            }
        }


        private void Input()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Z)
            {
                UndoAction();
            }

            if (e.type == EventType.MouseMove)
            {
                Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (GleyPrefabUtilities.EditingInsidePrefab())
                {
                    if (GleyPrefabUtilities.GetScenePrefabRoot().scene.GetPhysicsScene().Raycast(worldRay.origin, worldRay.direction, out _hitInfo, Mathf.Infinity, GetGroundLayer()))
                    {
                        _canClick = true;
                    }
                    else
                    {
                        _canClick = false;
                    }
                }
                else
                {
                    if (Physics.Raycast(worldRay, out _hitInfo, Mathf.Infinity, GetGroundLayer()))
                    {
                        _canClick = true;
                    }
                    else
                    {
                        _canClick = false;
                    }
                }
                MouseMove(_hitInfo.point);
            }

            if (_canClick)
            {
#if GLEY_PEDESTRIAN_SYSTEM
                if (e.type == EventType.Layout && GUIUtility.hotControl == Gley.PedestrianSystem.Internal.PedestrianSystemConstants.PATH_ID)
                {
                    //click on the first gizmo
                    if (e.shift)
                    {
                        _firstAnchorClicked = true;
                        LeftClick(_hitInfo.point, _firstAnchorClicked);
                    }
                }
                else
                {
                    _firstAnchorClicked = false;
                }
#endif

                if (e.type == EventType.MouseDown && e.shift)
                {
                    if (e.button == 0)
                    {
                        LeftClick(_hitInfo.point, _firstAnchorClicked);
                        e.Use();
                    }
                    if (e.button == 1)
                    {
                        RightClick(_hitInfo.point);
                        e.Use();
                    }
                }
            }
        }


        private void LeftClick(Vector3 point, bool clicked)
        {
            if (_activeSetupWindow != null)
            {
                _activeSetupWindow.LeftClick(point, clicked);
            }
        }


        private void RightClick(Vector3 point)
        {
            if (_activeSetupWindow != null)
            {
                _activeSetupWindow.RightClick(point);
            }
        }


        private void UndoAction()
        {
            if (_activeSetupWindow != null)
            {
                _activeSetupWindow.UndoAction();
            }
        }


        private void Draw()
        {
            _activeSetupWindow.DrawInScene();
        }


        #endregion


        private void OnDestroy()
        {
            if (_activeSetupWindow != null)
            {
                _activeSetupWindow.DestroyWindow();
            }
        }
    }
}
