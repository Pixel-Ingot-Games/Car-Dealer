using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    [System.Serializable]
    public class Waypoint
    {
        [SerializeField] private int[] _neighbors;
        [SerializeField] private int[] _prev;
        [SerializeField] private Vector3 _position;
        [SerializeField] private string _name;
        [SerializeField] private int _listIndex;
        [SerializeField] private bool _temporaryDisabled;

        public int[] Neighbors => _neighbors;
        public int[] Prev => _prev;
        public Vector3 Position => _position;
        public string Name => _name;
        public int ListIndex => _listIndex;
        public bool TemporaryDisabled
        {
            get
            {
                return _temporaryDisabled;
            }
            set
            {
                _temporaryDisabled = value;
            }
        }



        /// <summary>
        /// Constructor used to convert from editor waypoint to runtime waypoint 
        /// </summary>
        public Waypoint(string name, int listIndex, Vector3 position, int[] neighbors, int[] prev)
        {
            _name = name;
            _listIndex = listIndex;
            _position = position;
            _neighbors = neighbors;
            _prev = prev;
            _temporaryDisabled = false;
        }
    }
}
