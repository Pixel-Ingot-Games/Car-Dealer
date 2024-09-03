using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    public class GridData : MonoBehaviour
    {
        [SerializeField] private int _gridCellSize;
        [SerializeField] private Vector3 _gridCorner;
        [SerializeField] private RowData[] _grid;

        public RowData[] Grid => _grid;
        public Vector3 GridCorner => _gridCorner;
        public int GridCellSize => _gridCellSize;


        public void SetGridData(RowData[] grid, Vector3 gridCorner, int gridCellSize)
        {
            _grid = grid;
            _gridCorner = gridCorner;
            _gridCellSize = gridCellSize;
        }


        public bool IsValid(out string error)
        {
            error = string.Empty;
            if (_grid == null)
            {
                error = UrbanSystemErrors.SceneDataIsNull;
                return false;
            }

            if (_grid.Length == 0)
            {
                error = UrbanSystemErrors.SceneGridIsNull;
                return false;
            }
            return true;
        }
    }
}
