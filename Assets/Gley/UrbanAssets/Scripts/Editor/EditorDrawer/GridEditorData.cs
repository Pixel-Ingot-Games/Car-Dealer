using Gley.UrbanSystem.Internal;

namespace Gley.UrbanSystem.Editor
{
    internal class GridEditorData : EditorData
    {
        GridData _gridData;

        internal GridEditorData()
        {
            LoadAllData();
        }


        internal int GetGridCellSize()
        {
            if (_gridData.GridCellSize == 0)
            {
                return 50;
            }
            return _gridData.GridCellSize;
        }


        internal RowData[] GetGrid()
        {
            return _gridData.Grid;
        }


        protected override void LoadAllData()
        {
            _gridData = MonoBehaviourUtilities.GetOrCreateObjectScript<GridData>(UrbanSystemConstants.PlayHolder, false);
        }
    }
}