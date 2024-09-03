using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    public class TrafficWindowNavigationData
    {
        private Road _selectedRoad;
        private WaypointSettings _selectedWaypoint;
        private GenericIntersectionSettings _selectedIntersection;
        private LayerMask _roadLayers;


        internal void InitializeData()
        {
            UpdateLayers();
            _selectedRoad = null;
        }


        internal Road GetSelectedRoad()
        {
            return _selectedRoad;
        }


        internal void SetSelectedRoad(Road road)
        {
            _selectedRoad = road;
        }


        internal WaypointSettings GetSelectedWaypoint()
        {
            return _selectedWaypoint;
        }


        internal void SetSelectedWaypoint( WaypointSettings waypoint)
        {
            _selectedWaypoint = waypoint;
        }


        internal GenericIntersectionSettings GetSelectedIntersection()
        {
            return _selectedIntersection;
        }


        internal void SetSelectedIntersection(GenericIntersectionSettings intersection)
        {
            _selectedIntersection = intersection;
        }


        internal void UpdateLayers()
        {
            _roadLayers = FileCreator.LoadOrCreateLayers<LayerSetup>(Internal.TrafficSystemConstants.layerPath).roadLayers;
        }


        internal LayerMask GetRoadLayers()
        {
            return _roadLayers;
        }
    }
}
