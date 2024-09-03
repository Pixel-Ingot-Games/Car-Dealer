using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficConnectionEditorData : EditorData
    {
        private TrafficRoadData _roadData;
        private ConnectionPool[] _allConnectionPools;
        private ConnectionCurve[] _allConnections;
        private Dictionary<ConnectionCurve, WaypointSettings[]> _connectionWaypoints;
        private Dictionary<ConnectionCurve, ConnectionPool> _pools;


        internal TrafficConnectionEditorData(TrafficRoadData roadData)
        {
            _roadData = roadData;
            LoadAllData();
        }


        internal ConnectionCurve[] GetAllConnections()
        {
            return _allConnections;
        }


        internal ConnectionPool[] GetAllConnectionPools()
        {
            return _allConnectionPools;
        }


        internal WaypointSettings[] GetWaypoints(ConnectionCurve connection)
        {
            _connectionWaypoints.TryGetValue(connection, out var waypoints);
            return waypoints;
        }


        internal ConnectionPool GetConnectionPool(ConnectionCurve connection)
        {
            _pools.TryGetValue(connection, out var pool);
            return pool;
        }


        protected override void LoadAllData()
        {
            var tempConnections = new List<ConnectionCurve>();
            var connectionPools = new List<ConnectionPool>();
            _connectionWaypoints = new Dictionary<ConnectionCurve, WaypointSettings[]>();
            _pools = new Dictionary<ConnectionCurve, ConnectionPool>();
            var allRoads = _roadData.GetAllRoads();
            for (int i = 0; i < allRoads.Length; i++)
            {
                if (allRoads[i].isInsidePrefab && !GleyPrefabUtilities.EditingInsidePrefab())
                {
                    continue;
                }
                ConnectionPool connectionsScript = allRoads[i].transform.parent.GetComponent<ConnectionPool>();
                if (connectionsScript == null)
                {
                    Debug.Log(allRoads[i].name, allRoads[i].transform.parent);
                    continue;
                }
                if (!connectionPools.Contains(connectionsScript))
                {
                    connectionPools.Add(connectionsScript);
                }
            }

            //verify
            for (int i = 0; i < connectionPools.Count; i++)
            {
                connectionPools[i].VerifyAssignments();
                var connectionCurves = connectionPools[i].GetAllConnections();
                for (int j = connectionCurves.Count - 1; j >= 0; j--)
                {
                    if (connectionCurves[j].VerifyAssignments() == false)
                    {
                        if (connectionCurves[j].holder)
                        {
                            GleyPrefabUtilities.DestroyImmediate(connectionCurves[j].holder.gameObject);
                        }
                        connectionCurves.RemoveAt(j);
                    }
                    else
                    {
                        connectionCurves[j].inPosition = connectionCurves[j].GetInConnector().transform.position;
                        connectionCurves[j].outPosition = connectionCurves[j].GetOutConnector().transform.position;

                        //add waypoints
                        var waypoints = new List<WaypointSettings>();
                        Transform waypointsHolder = connectionCurves[j].holder;
                        for (int k = 0; k < waypointsHolder.childCount; k++)
                        {
                            var waypointScript = waypointsHolder.GetChild(k).GetComponent<WaypointSettings>();
                            if (waypointScript != null)
                            {
                                //check for null assignments assigned and remove them
                                waypointScript.VerifyAssignments(false);
                                waypointScript.position = waypointScript.transform.position;
                                waypoints.Add(waypointScript);
                            }
                        }
                        tempConnections.Add(connectionCurves[j]);
                        _connectionWaypoints.Add(connectionCurves[j], waypoints.ToArray());
                        _pools.Add(connectionCurves[j], connectionPools[i]);
                    }
                }
            }

            _allConnectionPools = connectionPools.ToArray();
            _allConnections = tempConnections.ToArray();
        }
    }
}
