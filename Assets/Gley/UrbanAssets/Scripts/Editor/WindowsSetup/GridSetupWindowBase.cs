using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal abstract class GridSetupWindowBase : SetupWindowBase
    {
        private GridEditorData _gridData;
        private GridCreator _gridCreator;
        private Color _oldColor;
        private int _gridCellSize;

        protected GridDrawer _gridDrawer;
        protected bool _viewGrid;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _gridData = new GridEditorData();
            _gridCreator = new GridCreator(_gridData);
            _gridDrawer = new GridDrawer(_gridData);
            _gridCellSize = _gridData.GetGridCellSize();
            return base.Initialize(windowProperties, window);
        }


        protected override void TopPart()
        {
            base.TopPart();
            EditorGUILayout.LabelField("The grid is used to improve the performance. Moving agents are generated in the cells adjacent to player cell.\n\n" +
                "The cell size should be smaller if your player speed is low and should increase if your speed is high.\n\n" +
                "You can experiment with this settings until you get the result you want.");
        }


        protected override void ScrollPart(float width, float height)
        {
            _gridCellSize = EditorGUILayout.IntField("Grid Cell Size: ", _gridCellSize);
            if (GUILayout.Button("Regenerate Grid"))
            {
                _gridCreator.GenerateGrid(_gridCellSize);
            }
            EditorGUILayout.Space();

            _oldColor = GUI.backgroundColor;
            if (_viewGrid == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("View Grid"))
            {
                _viewGrid = !_viewGrid;
                SceneView.RepaintAll();
            }
            GUI.backgroundColor = _oldColor;
            base.ScrollPart(width, height);
        }


        internal override void DestroyWindow()
        { 
            _gridDrawer?.OnDestroy();
        }
    }
}
