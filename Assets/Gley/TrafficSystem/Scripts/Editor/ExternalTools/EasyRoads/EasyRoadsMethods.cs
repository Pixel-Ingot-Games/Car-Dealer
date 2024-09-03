#if GLEY_EASYROADS_TRAFFIC
using EasyRoads3Dv3;
using System.Collections.Generic;
using UnityEngine;
using System;
using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;

namespace Gley.TrafficSystem.Editor
{
    public class EasyRoadsMethods : UnityEditor.Editor
    {
        private static List<GenericIntersectionSettings> _allGleyIntersections;
        private static List<TempWaypoint> _points;
        private static List<Transform> _waypointParents;
        private static List<TempWaypoint> _connectors;
        private static List<Transform> _connectionParents;
        private static List<Transform> _allWaypoints;
        private static List<Transform> _allConnectors;
        private static ERCrossings[] _allERIntersections;

        private static string EasyRoadsWaypointsHolder
        {
            get
            {
                return $"{TrafficSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/EasyRoadsEditorWaypoints";
            }
        }

        private static string EasyRoadsIntersectionsHolder
        {
            get
            {
                return $"{TrafficSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/EasyRoadsIntersections";
            }
        }

        class TempWaypoint
        {
            internal string Name { get; set; }
            internal Vector3 Position { get; set; }
            internal float LaneWidth { get; set; }
            internal int MaxSpeed { get; set; }
            internal int ListIndex { get; set; }
            internal bool Enter { get; set; }
            internal bool Exit { get; set; }
        }


        public static void ExtractWaypoints(IntersectionType intersectionType, float greenLightTime, float yellowLightTime, bool linkLanes, int waypointDistance, List<int> vehicleTypes)
        {
            //destroy existing roads
            DestroyImmediate(GameObject.Find(EasyRoadsWaypointsHolder));
            DestroyImmediate(GameObject.Find(EasyRoadsIntersectionsHolder));

            //create scene hierarchy
            ERRoadNetwork roadNetwork = new ERRoadNetwork();
            ERRoad[] roads = roadNetwork.GetRoadObjects();
            Debug.Log("Roads: " + roads.Length);

            _points = new List<TempWaypoint>();
            _waypointParents = new List<Transform>();
            _allWaypoints = new List<Transform>();
            _allConnectors = new List<Transform>();
            _connectors = new List<TempWaypoint>();
            _connectionParents = new List<Transform>();
            _allGleyIntersections = new List<GenericIntersectionSettings>();
            Transform intersectionHolder = MonoBehaviourUtilities.GetOrCreateGameObject(EasyRoadsIntersectionsHolder, true).transform;
            Transform waypointsHolder = MonoBehaviourUtilities.GetOrCreateGameObject(EasyRoadsWaypointsHolder, true).transform;

            AddIntersections(intersectionHolder, intersectionType, greenLightTime, yellowLightTime);

            //extract information from EasyRoads
            for (int i = 0; i < roads.Length; i++)
            {
                if (!roads[i].roadScript.isSideObject)
                {
                    GameObject road = MonoBehaviourUtilities.CreateGameObject("Road_" + i, waypointsHolder, Vector3.zero, true);
                    GameObject lanesHolder = MonoBehaviourUtilities.CreateGameObject("Lanes", road.transform, Vector3.zero, true);
                    Transform connectorsHolder = MonoBehaviourUtilities.CreateGameObject("Connectors", road.transform, Vector3.zero, true).transform;

                    if (roads[i].GetLaneCount() > 0)
                    {
                        float laneWidth = roads[i].GetWidth() / roads[i].GetLaneCount();
                        ExtractLaneWaypoints(roads[i].GetLeftLaneCount(), lanesHolder, roads[i], ERLaneDirection.Left, i, laneWidth);
                        ExtractLaneWaypoints(roads[i].GetRightLaneCount(), lanesHolder, roads[i], ERLaneDirection.Right, i, laneWidth);
                        ExtractConnectors(roads[i].GetLaneCount(), roads[i], connectorsHolder, i, laneWidth);
                    }
                    else
                    {
                        Debug.LogError("No lane data found for " + roads[i].gameObject + ". Make sure this road hat at least one lane inside Lane Info tab.", roads[i].gameObject);
                    }
                }
            }

            //convert extracted information to waypoints
            CreateTrafficWaypoints(vehicleTypes);

            LinkAllWaypoints(waypointsHolder);

            if (linkLanes)
            {
                LinkOvertakeLanes(waypointsHolder, waypointDistance);
            }

            CreateConnectorWaypoints(vehicleTypes);

            LinkAllConnectors(waypointsHolder);

            LinkConnectorsToRoadWaypoints();

            AssignIntersections(intersectionType);

            RemoveNonRequiredWaypoints();

            Debug.Log("total waypoints generated " + _allWaypoints.Count);

            Debug.Log("Done generating waypoints for Easy Roads");
        }


        private static void RemoveNonRequiredWaypoints()
        {
            for (int j = _allWaypoints.Count - 1; j >= 0; j--)
            {
                if (_allWaypoints[j].GetComponent<WaypointSettings>().neighbors.Count == 0)
                {
                    DestroyImmediate(_allWaypoints[j].gameObject);
                }
            }
        }


        private static void AssignIntersections(IntersectionType intersectionType)
        {
            for (int i = 0; i < _connectors.Count; i++)
            {
                if (_connectors[i].ListIndex != -1)
                {
                    switch (intersectionType)
                    {

                        case IntersectionType.Priority:
                            {
                                PriorityIntersectionSettings currentIntersection = (PriorityIntersectionSettings)_allGleyIntersections[_connectors[i].ListIndex];
                                if (_connectors[i].Enter == true)
                                {
                                    WaypointSettings waypointToAdd = _allConnectors[i].GetComponent<WaypointSettings>();
                                    if (waypointToAdd.prev.Count > 0)
                                    {
                                        waypointToAdd = (WaypointSettings)waypointToAdd.prev[0];
                                        AssignEnterWaypoints(currentIntersection.enterWaypoints, waypointToAdd);
                                    }
                                    else
                                    {
                                        Debug.Log(waypointToAdd.name + " has no previous waypoints", waypointToAdd);
                                    }
                                }

                                if (_connectors[i].Exit)
                                {
                                    if (currentIntersection.exitWaypoints == null)
                                    {
                                        currentIntersection.exitWaypoints = new List<WaypointSettings>();
                                    }
                                    WaypointSettings waypointToAdd = _allConnectors[i].GetComponent<WaypointSettings>();
                                    if (waypointToAdd.neighbors.Count > 0)
                                    {
                                        waypointToAdd = (WaypointSettings)waypointToAdd.neighbors[0];
                                        if (!currentIntersection.exitWaypoints.Contains(waypointToAdd))
                                        {
                                            currentIntersection.exitWaypoints.Add(waypointToAdd);
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log(waypointToAdd.name + " has no neighbors.", waypointToAdd);
                                    }
                                }
                            }
                            break;
                        case IntersectionType.TrafficLights:
                            {
                                TrafficLightsIntersectionSettings currentIntersection = (TrafficLightsIntersectionSettings)_allGleyIntersections[_connectors[i].ListIndex];
                                if (_connectors[i].Enter == true)
                                {
                                    WaypointSettings waypoint = _allConnectors[i].GetComponent<WaypointSettings>();
                                    if (waypoint.prev.Count > 0)
                                    {
                                        AssignEnterWaypoints(currentIntersection.stopWaypoints, (WaypointSettings)_allConnectors[i].GetComponent<WaypointSettings>().prev[0]);
                                    }
                                    else
                                    {
                                        Debug.Log(waypoint.name + " is not properly linked", waypoint);
                                    }
                                }
                            }
                            break;
                        default:
                            Debug.LogWarning($"{intersectionType} is not supported");
                            break;
                    }
                }
            }
        }


        private static void AssignEnterWaypoints(List<IntersectionStopWaypointsSettings> enterWaypoints, WaypointSettings waypointToAdd)
        {
            string roadName = waypointToAdd.name.Split('-')[0];
            int index = -1;

            for (int j = 0; j < enterWaypoints.Count; j++)
            {
                if (enterWaypoints[j].roadWaypoints.Count > 0)
                {
                    if (enterWaypoints[j].roadWaypoints[0].name.Contains(roadName))
                    {
                        index = j;
                    }
                }
            }
            if (index == -1)
            {
                enterWaypoints.Add(new IntersectionStopWaypointsSettings());
                index = enterWaypoints.Count - 1;
                enterWaypoints[index].roadWaypoints = new List<WaypointSettings>();
            }

            if (!enterWaypoints[index].roadWaypoints.Contains(waypointToAdd))
            {
                enterWaypoints[index].roadWaypoints.Add(waypointToAdd);
            }
        }


        private static void LinkConnectorsToRoadWaypoints()
        {
            for (int i = 0; i < _allConnectors.Count; i++)
            {
                if (_allConnectors[i].name.Contains(UrbanSystemConstants.ConnectionEdgeName))
                {
                    for (int j = 0; j < _allWaypoints.Count; j++)
                    {
                        if (Vector3.Distance(_allConnectors[i].position, _allWaypoints[j].position) < 0.5f)
                        {
                            WaypointSettings connectorScript = _allConnectors[i].GetComponent<WaypointSettings>();
                            WaypointSettings waypointScript = _allWaypoints[j].GetComponent<WaypointSettings>();

                            if (connectorScript.prev.Count == 0)
                            {
                                connectorScript.prev = waypointScript.prev;
                                waypointScript.prev[0].neighbors.Remove(waypointScript);
                                waypointScript.prev[0].neighbors.Add(connectorScript);

                            }

                            if (connectorScript.neighbors.Count == 0)
                            {
                                connectorScript.neighbors = waypointScript.neighbors;
                                if (waypointScript.neighbors.Count > 0)
                                {
                                    waypointScript.neighbors[0].prev.Add(connectorScript);
                                }
                                //else
                                //{
                                //    Debug.Log(waypointScript.name + " has no neighbors", waypointScript);
                                //}
                            }
                            break;
                        }
                    }
                }
            }
        }


        private static void CreateConnectorWaypoints(List<int> vehicleTypes)
        {
            TrafficWaypointCreator waypointCreator = new TrafficWaypointCreator();

            for (int i = 0; i < _connectors.Count; i++)
            {
                _allConnectors.Add(waypointCreator.CreateWaypoint(_connectionParents[i], _connectors[i].Position, _connectors[i].Name, vehicleTypes, _connectors[i].MaxSpeed, _connectors[i].LaneWidth));
            }
        }


        private static void CreateTrafficWaypoints(List<int> vehicleTypes)
        {
            TrafficWaypointCreator waypointCreator = new TrafficWaypointCreator();
            for (int i = 0; i < _points.Count; i++)
            {
                _allWaypoints.Add(waypointCreator.CreateWaypoint(_waypointParents[i], _points[i].Position, _points[i].Name, vehicleTypes, _points[i].MaxSpeed, _points[i].LaneWidth));
            }
        }


        private static void AddIntersections(Transform intersectionHolder, IntersectionType intersectionType, float greenLightTime, float yellowLightTime)
        {
            _allERIntersections = FindObjectsOfType<ERCrossings>();
            for (int i = 0; i < _allERIntersections.Length; i++)
            {
                var intersection = MonoBehaviourUtilities.CreateGameObject(_allERIntersections[i].name, intersectionHolder, _allERIntersections[i].gameObject.transform.position, true);
                GenericIntersectionSettings intersectionScript = null;
                switch (intersectionType)
                {
                    case IntersectionType.Priority:
                        intersectionScript = intersection.AddComponent<PriorityIntersectionSettings>();
                        ((PriorityIntersectionSettings)intersectionScript).enterWaypoints = new List<IntersectionStopWaypointsSettings>();
                        break;
                    case IntersectionType.TrafficLights:
                        intersectionScript = intersection.AddComponent<TrafficLightsIntersectionSettings>();
                        ((TrafficLightsIntersectionSettings)intersectionScript).stopWaypoints = new List<IntersectionStopWaypointsSettings>();
                        ((TrafficLightsIntersectionSettings)intersectionScript).greenLightTime = greenLightTime;
                        ((TrafficLightsIntersectionSettings)intersectionScript).yellowLightTime = yellowLightTime;
                        break;
                    default:
                        Debug.LogWarning(intersectionType + " not supported");
                        break;
                }

                _allGleyIntersections.Add(intersectionScript);
            }
        }


        private static void LinkAllConnectors(Transform holder)
        {
            for (int r = 0; r < holder.childCount; r++)
            {
                for (int i = 0; i < holder.GetChild(r).GetChild(1).childCount; i++)
                {
                    Transform laneHolder = holder.GetChild(r).GetChild(1).GetChild(i);
                    LinkWaypoints(laneHolder);
                }
            }
        }


        private static void LinkAllWaypoints(Transform holder)
        {
            for (int r = 0; r < holder.childCount; r++)
            {
                for (int i = 0; i < holder.GetChild(r).GetChild(0).childCount; i++)
                {
                    Transform laneHolder = holder.GetChild(r).GetChild(0).GetChild(i);
                    LinkWaypoints(laneHolder);
                }
            }
        }


        private static void LinkOvertakeLanes(Transform holder, int waypointDistance)
        {
            for (int i = 0; i < holder.childCount; i++)
            {
                for (int j = 0; j < holder.GetChild(i).GetChild(0).childCount; j++)
                {
                    Transform firstLane = holder.GetChild(i).GetChild(0).GetChild(j);
                    int laneToLink = j - 1;
                    if (laneToLink >= 0)
                    {
                        LinkLanes(firstLane, holder.GetChild(i).GetChild(0).GetChild(laneToLink), waypointDistance);
                    }
                    laneToLink = j + 1;
                    if (laneToLink < holder.GetChild(i).GetChild(0).childCount)
                    {
                        LinkLanes(firstLane, holder.GetChild(i).GetChild(0).GetChild(laneToLink), waypointDistance);
                    }
                }
            }
        }


        private static void LinkLanes(Transform firstLane, Transform secondLane, int waypointDistance)
        {
            if (secondLane.name.Split('_')[2] == firstLane.name.Split('_')[2])
            {
                LinkLaneWaypoints(firstLane, secondLane, waypointDistance);
            }
        }


        private static void LinkLaneWaypoints(Transform currentLane, Transform otherLane, int waypointDistance)
        {
            for (int i = 0; i < currentLane.childCount; i++)
            {
                int otherLaneIndex = i + waypointDistance;
                if (otherLaneIndex < currentLane.childCount - 1)
                {
                    WaypointSettings currentLaneWaypoint = currentLane.GetChild(i).GetComponent<WaypointSettings>();
                    WaypointSettings otherLaneWaypoint = otherLane.GetChild(otherLaneIndex).GetComponent<WaypointSettings>();
                    currentLaneWaypoint.otherLanes.Add(otherLaneWaypoint);
                }
            }
        }


        private static void LinkWaypoints(Transform laneHolder)
        {
            WaypointSettings previousWaypoint = laneHolder.GetChild(0).GetComponent<WaypointSettings>();
            for (int j = 1; j < laneHolder.childCount; j++)
            {
                string waypointName = laneHolder.GetChild(j).name;
                WaypointSettings waypointScript = laneHolder.GetChild(j).GetComponent<WaypointSettings>();
                if (previousWaypoint != null)
                {
                    previousWaypoint.neighbors.Add(waypointScript);
                    waypointScript.prev.Add(previousWaypoint);
                }
                if (!waypointName.Contains("Output"))
                {
                    previousWaypoint = waypointScript;
                }
                else
                {
                    previousWaypoint = null;
                }
            }
        }


        static void ExtractLaneWaypoints(int lanes, GameObject lanesHolder, ERRoad road, ERLaneDirection side, int r, float laneWidth)
        {
            if (lanes > 0)
            {
                for (int i = 0; i < lanes; i++)
                {
                    Vector3[] positions = road.GetLanePoints(i, side);
                    if (positions != null)
                    {
                        GameObject lane = MonoBehaviourUtilities.CreateGameObject("Lane_" + lanesHolder.transform.childCount + "_" + side, lanesHolder.transform, Vector3.zero, true);

                        for (int j = 0; j < positions.Length; j++)
                        {
                            var waypoint = new TempWaypoint();
                            waypoint.Name = "Road_" + r + "-" + UrbanSystemConstants.LaneNamePrefix + (lanesHolder.transform.childCount - 1) + "-" + UrbanSystemConstants.WaypointNamePrefix + j;
                            waypoint.Position = positions[j];
                            waypoint.MaxSpeed = (int)road.GetSpeedLimit();
                            waypoint.LaneWidth = laneWidth;
                            _points.Add(waypoint);
                            _waypointParents.Add(lane.transform);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No lane points found for " + road.gameObject.name + ", make sure Generate Lane Data is enabled from AI traffic", road.gameObject);
                    }
                }
            }
        }


        private static void ExtractConnectors(int lanes, ERRoad road, Transform lanesHolder, int roadIndex, float laneWidth)
        {
            bool endConnectorsFound = true;
            bool connectorsFound = true;
            GameObject connectorGameobect = null;
            for (int i = 0; i < lanes; i++)
            {
                int connectionIndex;
                ERConnection conn = road.GetConnectionAtEnd(out connectionIndex);
                if (conn != null)
                {
                    ERLaneConnector[] laneConnectors = conn.GetLaneData(connectionIndex, i);
                    if (laneConnectors != null)
                    {
                        ExtractLaneConnectors(conn, laneConnectors, lanesHolder, i, roadIndex, (int)road.GetSpeedLimit(), laneWidth);
                    }
                    else
                    {
                        connectorsFound = false;
                        connectorGameobect = conn.gameObject;
                    }
                }
                else
                {
                    endConnectorsFound = false;
                }

                conn = road.GetConnectionAtStart(out connectionIndex);
                if (conn != null)
                {
                    ERLaneConnector[] laneConnectors = conn.GetLaneData(connectionIndex, i);
                    if (laneConnectors != null)
                    {
                        ExtractLaneConnectors(conn, laneConnectors, lanesHolder, i, roadIndex, (int)road.GetSpeedLimit(), laneWidth);
                    }
                    else
                    {
                        connectorsFound = false;
                        connectorGameobect = conn.gameObject;
                    }
                }
                else
                {
                    endConnectorsFound = false;
                }
            }

            if (endConnectorsFound == false)
            {
                Debug.LogWarning(road.gameObject + " is not connected to anything ", road.gameObject);
            }

            if (connectorsFound == false)
            {
                Debug.LogWarning("No waypoint connectors found for " + connectorGameobect + ". You should connect it manually.", connectorGameobect);
            }
        }


        private static void ExtractLaneConnectors(ERConnection conn, ERLaneConnector[] laneConnectors, Transform lanesHolder, int laneIndex, int roadIndex, int speedLimit, float laneWidth)
        {
            if (laneConnectors != null)
            {
                for (int j = 0; j < laneConnectors.Length; j++)
                {
                    GameObject lane = MonoBehaviourUtilities.CreateGameObject("Connector" + j, lanesHolder, Vector3.zero, true);
                    for (int k = 0; k < laneConnectors[j].points.Length; k++)
                    {
                        var waypoint = new TempWaypoint();
                        waypoint.ListIndex = -1;
                        if (k == 0 || k == laneConnectors[j].points.Length - 1)
                        {
                            waypoint.Name = "Road_" + roadIndex + "-" + UrbanSystemConstants.LaneNamePrefix + j + "-" + UrbanSystemConstants.ConnectionEdgeName + k;
                            waypoint.ListIndex = Array.FindIndex(_allERIntersections, cond => cond.gameObject == conn.gameObject);
                            if (k == 0)
                            {
                                waypoint.Enter = true;
                            }
                            else
                            {
                                waypoint.Exit = true;
                            }
                        }
                        else
                        {
                            waypoint.Name = "Road_" + roadIndex + "-" + UrbanSystemConstants.LaneNamePrefix + j + "-" + UrbanSystemConstants.ConnectionWaypointName + k;
                        }

                        waypoint.Position = laneConnectors[j].points[k];
                        waypoint.MaxSpeed = speedLimit;
                        waypoint.LaneWidth = laneWidth;
                        _connectors.Add(waypoint);
                        _connectionParents.Add(lane.transform);
                    }
                }
            }
        }
    }
}
#endif
