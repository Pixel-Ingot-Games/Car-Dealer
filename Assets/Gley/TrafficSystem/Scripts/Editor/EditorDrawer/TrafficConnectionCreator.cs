using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class TrafficConnectionCreator
    {
        private TrafficConnectionEditorData _connectionData;
        private TrafficWaypointCreator _waypointCreator;


        internal TrafficConnectionCreator (TrafficConnectionEditorData connectionData, TrafficWaypointCreator waypointCreator)
        {
            _connectionData = connectionData;
            _waypointCreator = waypointCreator;
        }


        internal void CreateConnection(ConnectionPool connectionPool, Road fromRoad, int fromIndex, Road toRoad, int toIndex, float waypointDistance)
        {
            Vector3 offset = Vector3.zero;
            if (!GleyPrefabUtilities.EditingInsidePrefab())
            {
                if (GleyPrefabUtilities.IsInsidePrefab(fromRoad.gameObject) && Gley.UrbanSystem.Editor.GleyPrefabUtilities.GetInstancePrefabRoot(fromRoad.gameObject) == Gley.UrbanSystem.Editor.GleyPrefabUtilities.GetInstancePrefabRoot(toRoad.gameObject))
                {
                    //connectionPool = GetOrCreateConnectionPool();
                    offset = fromRoad.positionOffset;
                }
                else
                {
                    //connectionPool = GetOrCreateConnectionPool();
                    offset = fromRoad.positionOffset;
                }
            }

            Path newConnection = new Path(fromRoad.lanes[fromIndex].laneEdges.outConnector.transform.position - offset, toRoad.lanes[toIndex].laneEdges.inConnector.transform.position - offset);
            string name = fromRoad.name + "_" + fromIndex + "->" + toRoad.name + "_" + toIndex;
            var parent = MonoBehaviourUtilities.GetOrCreateGameObject(TrafficSystemConstants.EditorConnectionsHolder, true);
            GameObject connectorsHolder = MonoBehaviourUtilities.CreateGameObject(name, parent.transform, fromRoad.startPosition, true);
            var curve = new ConnectionCurve(newConnection, fromRoad, fromIndex, toRoad, toIndex, true, connectorsHolder.transform);

            connectionPool.AddConnection(curve);
            GenerateConnectionWaypoints(curve, waypointDistance);

            _connectionData.TriggerOnModifiedEvent();

            EditorUtility.SetDirty(connectionPool);
            AssetDatabase.SaveAssets();
        }


        internal void DeleteConnection(ConnectionCurve connectingCurve)
        {
            RemoveConnectionHolder(connectingCurve.holder);
            var connectionPools = _connectionData.GetAllConnectionPools();
            for (int i = 0; i < connectionPools.Length; i++)
            {
                if (connectionPools[i].ContainsConnection(connectingCurve))
                {
                    connectionPools[i].RemoveConnection(connectingCurve);
                    EditorUtility.SetDirty(connectionPools[i]);
                }
            }
            _connectionData.TriggerOnModifiedEvent();
            AssetDatabase.SaveAssets();
        }


        internal void GenerateConnections(List<ConnectionCurve> connectionCurves, float waypointDistance)
        {
            for (int i = 0; i < connectionCurves.Count; i++)
            {
                if (connectionCurves[i].draw)
                {
                    GenerateConnectionWaypoints(connectionCurves[i], waypointDistance);
                }
            }

            _connectionData.TriggerOnModifiedEvent();
        }


        internal void DeleteConnectionsWithThisRoad(Road road)
        {
            var connections = _connectionData.GetAllConnections();
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].ContainsRoad(road))
                {
                    DeleteConnection(connections[i]);
                }
            }
        }


        internal void DeleteConnectionsWithThisLane(Road road, int laneNumber)
        {
            var connections = _connectionData.GetAllConnections();
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].ContainsLane(road, laneNumber))
                {
                    DeleteConnection(connections[i]);
                }
            }
        }


        private void GenerateConnectionWaypoints(ConnectionCurve connection, float waypointDistance)
        {
            RemoveConnectionWaipoints(connection.GetHolder());
            string roadName = connection.fromRoad.name;
            List<int> allowedCars = connection.GetOutConnector().allowedCars.Cast<int>().ToList();
            int maxSpeed = connection.GetOutConnector().maxSpeed;
            float laneWidth = connection.GetOutConnector().laneWidth;

            Path curve = connection.GetCurve();

            Vector3[] p = curve.GetPointsInSegment(0, connection.GetOffset());
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]);
            float nrOfWaypoints = estimatedCurveLength / waypointDistance;
            if (nrOfWaypoints < 1.5f)
            {
                nrOfWaypoints = 1.5f;
            }
            float step = 1 / nrOfWaypoints;
            float t = 0;
            int nr = 0;
            List<Transform> connectorWaypoints = new List<Transform>();
            while (t < 1)
            {
                t += step;
                if (t < 1)
                {
                    string waypointName = roadName + "-" + UrbanSystem.Internal.UrbanSystemConstants.LaneNamePrefix + connection.fromIndex + "-" + UrbanSystem.Internal.UrbanSystemConstants.ConnectionWaypointName + (++nr);
                    connectorWaypoints.Add(_waypointCreator.CreateWaypoint(connection.GetHolder(), BezierCurve.CalculateCubicBezierPoint(t, p[0], p[1], p[2], p[3]), waypointName, allowedCars, maxSpeed, laneWidth));
                }
            }

            WaypointSettingsBase currentWaypoint;
            WaypointSettingsBase connectedWaypoint;

            //set names
            connectorWaypoints[0].name = roadName + "-" + UrbanSystem.Internal.UrbanSystemConstants.LaneNamePrefix + connection.fromIndex + "-" + UrbanSystem.Internal.UrbanSystemConstants.ConnectionEdgeName + nr;
            connectorWaypoints[connectorWaypoints.Count - 1].name = roadName + "-" + UrbanSystem.Internal.UrbanSystemConstants.LaneNamePrefix + connection.fromIndex + "-" + UrbanSystem.Internal.UrbanSystemConstants.ConnectionEdgeName + (connectorWaypoints.Count - 1);

            //link middle waypoints
            for (int j = 0; j < connectorWaypoints.Count - 1; j++)
            {
                currentWaypoint = connectorWaypoints[j].GetComponent<WaypointSettingsBase>();
                connectedWaypoint = connectorWaypoints[j + 1].GetComponent<WaypointSettingsBase>();
                currentWaypoint.neighbors.Add(connectedWaypoint);
                connectedWaypoint.prev.Add(currentWaypoint);
            }

            //link first waypoint
            connectedWaypoint = connectorWaypoints[0].GetComponent<WaypointSettingsBase>();
            currentWaypoint = connection.GetOutConnector();
            currentWaypoint.neighbors.Add(connectedWaypoint);
            connectedWaypoint.prev.Add(currentWaypoint);
            EditorUtility.SetDirty(currentWaypoint);
            EditorUtility.SetDirty(connectedWaypoint);

            //link last waypoint
            connectedWaypoint = connection.GetInConnector();
            currentWaypoint = connectorWaypoints[connectorWaypoints.Count - 1].GetComponent<WaypointSettingsBase>();
            currentWaypoint.neighbors.Add(connectedWaypoint);
            connectedWaypoint.prev.Add(currentWaypoint);
            EditorUtility.SetDirty(currentWaypoint);
            EditorUtility.SetDirty(connectedWaypoint);
            AssetDatabase.SaveAssets();
        }


        private void RemoveConnectionHolder(Transform holder)
        {
            RemoveConnectionWaipoints(holder);
            GleyPrefabUtilities.DestroyTransform(holder);
        }


        private void RemoveConnectionWaipoints(Transform holder)
        {
            if (holder)
            {
                for (int i = holder.childCount - 1; i >= 0; i--)
                {
                    WaypointSettingsBase waypoint = holder.GetChild(i).GetComponent<WaypointSettingsBase>();
                    for (int j = 0; j < waypoint.neighbors.Count; j++)
                    {
                        if (waypoint.neighbors[j] != null)
                        {
                            waypoint.neighbors[j].prev.Remove(waypoint);
                        }
                        else
                        {
                            Debug.LogError(waypoint.name + " has null neighbors", waypoint);
                        }
                    }
                    for (int j = 0; j < waypoint.prev.Count; j++)
                    {
                        if (waypoint.prev[j] != null)
                        {
                            waypoint.prev[j].neighbors.Remove(waypoint);
                        }
                        else
                        {
                            Debug.LogError(waypoint.name + " has null prevs", waypoint);
                        }
                    }
                    GleyPrefabUtilities.DestroyImmediate(waypoint.gameObject);
                }
            }
        }
    }
}