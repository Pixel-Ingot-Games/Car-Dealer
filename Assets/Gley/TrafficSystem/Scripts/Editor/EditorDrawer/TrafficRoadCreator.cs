using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficRoadCreator : RoadCreator<Road, ConnectionPool, ConnectionCurve>
    {
        public TrafficRoadCreator(RoadEditorData<Road> data) : base(data)
        {
        }

        internal Road Create(int nrOfLanes, float laneWidth, float waypointDistance, string prefix, Vector3 firstClick, Vector3 secondClick, int globalMaxSpeed, int nrOfAgents, bool leftSideTraffic, int otherLaneLinkDistance)
        {
            Transform roadParent = MonoBehaviourUtilities.GetOrCreateSceneInstance<ConnectionPool>(TrafficSystemConstants.EditorWaypointsHolder, true).transform;
            int roadNumber = GleyUtilities.GetFreeRoadNumber(roadParent);
            GameObject roadHolder = MonoBehaviourUtilities.CreateGameObject(prefix + "_" + roadNumber, roadParent, firstClick, true);
            roadHolder.transform.SetSiblingIndex(roadNumber);
            var road = roadHolder.AddComponent<Road>();
            road.SetDefaults(nrOfLanes, laneWidth, waypointDistance, otherLaneLinkDistance);
            road.CreatePath(firstClick, secondClick);
            road.SetRoadProperties(globalMaxSpeed, nrOfAgents, leftSideTraffic);
            road.justCreated = true;
            EditorUtility.SetDirty(road);
            AssetDatabase.SaveAssets();
            _data.TriggerOnModifiedEvent();
            return road;
        }
    }
}
