using System.Collections.Generic;
using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    [System.Serializable]
    public class CellData
    {
        [SerializeField] CellProperties _cellProperties;
        [SerializeField] CellWaypointsData _trafficWaypointsData;
        [SerializeField] CellWaypointsData _pedestrianWaypointsData;
        [SerializeField] List<int> _intersectionsInCell;
        [SerializeField] private bool _inView;

        public CellProperties CellProperties => _cellProperties;
        public CellWaypointsData TrafficWaypointsData => _trafficWaypointsData;
        public CellWaypointsData PedestrianWaypointsData => _pedestrianWaypointsData;
        public List<int> IntersectionsInCell => _intersectionsInCell;

        public bool InView
        {
            get
            {
                return _inView;
            }
            set
            {
                _inView = value;
            }
        }


        public CellData(CellProperties cellProperties, CellWaypointsData trafficWaypointsData, CellWaypointsData pedestrianWaypointsData, List<int> intersectionsInCell)
        {
            _cellProperties = cellProperties;
            _trafficWaypointsData = trafficWaypointsData;
            _pedestrianWaypointsData = pedestrianWaypointsData;
            _intersectionsInCell = intersectionsInCell;
        }
    }
}
