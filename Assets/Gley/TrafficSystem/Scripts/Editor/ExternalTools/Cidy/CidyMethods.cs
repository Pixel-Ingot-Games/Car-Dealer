#if GLEY_CIDY_TRAFFIC
using CiDy;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;

namespace Gley.TrafficSystem.Editor
{
    public class CidyMethods : UnityEditor.Editor
    {
        struct TrafficLight
        {
            public Transform lightObject;
            public int intersectionIndex;
            public int roadIndex;

            public TrafficLight(Transform lightObject, int intersectionIndex, int roadIndex)
            {
                this.lightObject = lightObject;
                this.intersectionIndex = intersectionIndex;
                this.roadIndex = roadIndex;
            }
        }

        private static List<GenericIntersectionSettings> allGleyIntersections;
        private static List<TrafficLight> trafficLights;
        private static List<Transform> allWaypoints;
        private static List<Transform> allConnectors;
        private static List<TempWaypoint> connectors;

        private static string CidyWaypointsHolder
        {
            get
            {
                return $"{TrafficSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/CiDyEditorWaypoints";
            }
        }

        private static string CidyIntersectionsHolder
        {
            get
            {
                return $"{TrafficSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/CiDyIntersections";
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

        internal static void ExtractWaypoints(IntersectionType intersectionType, float greenLightTime, float yellowLightTime, int maxSpeed, List<int> vehicleTypes, int waypointDistance)
        {
            DestroyImmediate(GameObject.Find(CidyWaypointsHolder));
            DestroyImmediate(GameObject.Find(CidyIntersectionsHolder));


            allWaypoints = new List<Transform>();
            allConnectors = new List<Transform>();
            allGleyIntersections = new List<GenericIntersectionSettings>();
            connectors = new List<TempWaypoint>();
            trafficLights = new List<TrafficLight>();

            Transform intersectionHolder = MonoBehaviourUtilities.GetOrCreateGameObject(CidyIntersectionsHolder, true).transform;
            Transform waypointsHolder = MonoBehaviourUtilities.GetOrCreateGameObject(CidyWaypointsHolder, true).transform;

            CiDyGraph graph = FindObjectOfType<CiDyGraph>();
            graph.BuildTrafficData();

            //extract road waypoints
            List<GameObject> roads = graph.roads;
            for (int i = 0; i < roads.Count; i++)
            {
                GameObject road = MonoBehaviourUtilities.CreateGameObject("Road_" + i, waypointsHolder, waypointsHolder.position, true);
                CiDyRoad cidyRoad = roads[i].GetComponent<CiDyRoad>();
                ExtractLaneWaypoints(cidyRoad.leftRoutes.routes, road, "Left", i, maxSpeed, cidyRoad.laneWidth, vehicleTypes);
                ExtractLaneWaypoints(cidyRoad.rightRoutes.routes, road, "Right", i, maxSpeed, cidyRoad.laneWidth, vehicleTypes);
            }

            //extract connectors
            List<CiDyNode> nodes = graph.masterGraph;
            for (int i = 0; i < nodes.Count; i++)
            {
                CiDyNode node = nodes[i];
                float laneWidth = node.connectedRoads[0].laneWidth;
                switch (node.type)
                {
                    case CiDyNode.IntersectionType.continuedSection:
                        GameObject lane = MonoBehaviourUtilities.CreateGameObject("Connector" + i, waypointsHolder, node.position, true);
                        for (int j = 0; j < node.leftRoutes.routes.Count; j++)
                        {
                            ExtractLaneConnectors(node.leftRoutes.routes[j], lane.transform, j, i, maxSpeed, laneWidth, -1, vehicleTypes);
                        }
                        for (int j = 0; j < node.rightRoutes.routes.Count; j++)
                        {
                            ExtractLaneConnectors(node.rightRoutes.routes[j], lane.transform, j, i, maxSpeed, laneWidth, -1, vehicleTypes);
                        }
                        break;
                    case CiDyNode.IntersectionType.culDeSac:
                        lane = MonoBehaviourUtilities.CreateGameObject("CulDeSac" + i, waypointsHolder, node.position, true);
                        for (int j = 0; j < node.leftRoutes.routes.Count; j++)
                        {
                            ExtractLaneConnectors(node.leftRoutes.routes[j], lane.transform, j, i, maxSpeed, laneWidth, -1, vehicleTypes);
                        }
                        for (int j = 0; j < node.rightRoutes.routes.Count; j++)
                        {
                            ExtractLaneConnectors(node.rightRoutes.routes[j], lane.transform, j, i, maxSpeed, laneWidth, -1, vehicleTypes);
                        }
                        break;
                    case CiDyNode.IntersectionType.tConnect:
                        lane = MonoBehaviourUtilities.CreateGameObject("Intersection" + i, waypointsHolder, node.position, true);
                        allGleyIntersections.Add(AddIntersection(intersectionHolder, intersectionType, greenLightTime, yellowLightTime, node.name, node.position));
                        for (int j = 0; j < node.intersectionRoutes.intersectionRoutes.Count; j++)
                        {
                            AddTrafficlights(new TrafficLight(node.intersectionRoutes.intersectionRoutes[j].light, allGleyIntersections.Count - 1, node.intersectionRoutes.intersectionRoutes[j].sequenceIndex));
                            ExtractLaneConnectors(node.intersectionRoutes.intersectionRoutes[j].route, lane.transform, j, node.intersectionRoutes.intersectionRoutes[j].sequenceIndex, maxSpeed, laneWidth, allGleyIntersections.Count - 1, vehicleTypes);
                        }
                        break;
                }

            }

            LinkAllWaypoints(waypointsHolder);

            LinkOvertakeLanes(waypointsHolder, waypointDistance);

            LinkConnectorsToRoadWaypoints();

            AssignIntersections(intersectionType);

            if (intersectionType == IntersectionType.TrafficLights)
            {
                AssignTrafficLights();
            }

            RemoveNonRequiredWaypoints();
        }


        private static void AddTrafficlights(TrafficLight trafficLight)
        {
            if (!trafficLights.Contains(trafficLight))
            {
                trafficLights.Add(trafficLight);
            }
        }


        private static void AssignTrafficLights()
        {
            for (int i = 0; i < allGleyIntersections.Count; i++)
            {
                TrafficLightsIntersectionSettings currentIntersection = (TrafficLightsIntersectionSettings)allGleyIntersections[i];

                if (currentIntersection.stopWaypoints != null)
                {
                    for (int j = 0; j < currentIntersection.stopWaypoints.Count; j++)
                    {
                        List<TrafficLight> currentRoadLights = trafficLights.Where(cond => cond.intersectionIndex == i && cond.roadIndex == j).ToList();
                        for (int k = 0; k < currentRoadLights.Count; k++)
                        {
                            Transform colorObject = currentRoadLights[k].lightObject.Find("RedLight");
                            if (colorObject != null)
                            {
                                EnableRenderer(colorObject.GetComponent<Renderer>());
                                currentIntersection.stopWaypoints[j].redLightObjects.Add(colorObject.gameObject);
                            }
                            colorObject = currentRoadLights[k].lightObject.Find("YellowLight");
                            if (colorObject != null)
                            {
                                EnableRenderer(colorObject.GetComponent<Renderer>());
                                currentIntersection.stopWaypoints[j].yellowLightObjects.Add(colorObject.gameObject);
                            }
                            colorObject = currentRoadLights[k].lightObject.Find("GreenLight");
                            if (colorObject != null)
                            {
                                EnableRenderer(colorObject.GetComponent<Renderer>());
                                currentIntersection.stopWaypoints[j].greenLightObjects.Add(colorObject.gameObject);
                            }
                        }
                    }
                }
            }
        }


        static void EnableRenderer(Renderer renderer)
        {
            if (renderer != null)
            {
                if (renderer.enabled == false)
                {
                    renderer.enabled = true;
                }
            }
        }


        private static void AssignIntersections(IntersectionType intersectionType)
        {
            for (int i = 0; i < connectors.Count; i++)
            {
                if (connectors[i].ListIndex != -1)
                {
                    switch (intersectionType)
                    {
                        case IntersectionType.Priority:
                            {
                                PriorityIntersectionSettings currentIntersection = (PriorityIntersectionSettings)allGleyIntersections[connectors[i].ListIndex];
                                if (connectors[i].Enter == true)
                                {
                                    AssignEnterWaypoints(currentIntersection.enterWaypoints, (WaypointSettings)allConnectors[i].GetComponent<WaypointSettings>().prev[0]);
                                }

                                if (connectors[i].Exit)
                                {
                                    if (currentIntersection.exitWaypoints == null)
                                    {
                                        currentIntersection.exitWaypoints = new List<WaypointSettings>();
                                    }
                                    WaypointSettings waypointToAdd = allConnectors[i].GetComponent<WaypointSettings>();
                                    if (!currentIntersection.exitWaypoints.Contains(waypointToAdd))
                                    {
                                        currentIntersection.exitWaypoints.Add(waypointToAdd);
                                    }
                                }
                            }
                            break;

                        case IntersectionType.TrafficLights:
                            {
                                TrafficLightsIntersectionSettings currentIntersection = (TrafficLightsIntersectionSettings)allGleyIntersections[connectors[i].ListIndex];
                                if (connectors[i].Enter == true)
                                {
                                    WaypointSettings waypoint = allConnectors[i].GetComponent<WaypointSettings>();
                                    if (waypoint.prev.Count > 0)
                                    {
                                        AssignEnterWaypoints(currentIntersection.stopWaypoints, (WaypointSettings)allConnectors[i].GetComponent<WaypointSettings>().prev[0]);
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


        private static GenericIntersectionSettings AddIntersection(Transform intersectionHolder, IntersectionType intersectionType, float greenLightTime, float yellowLightTime, string name, Vector3 position)
        {
            GameObject intersection = MonoBehaviourUtilities.CreateGameObject(name, intersectionHolder, position, true);
            GenericIntersectionSettings intersectionScript = null;
            switch (intersectionType)
            {
                case IntersectionType.Priority:
                    intersectionScript = intersection.AddComponent<PriorityIntersectionSettings>();
                    intersectionScript.position = position;
                    ((PriorityIntersectionSettings)intersectionScript).enterWaypoints = new List<IntersectionStopWaypointsSettings>();
                    break;
                case IntersectionType.TrafficLights:
                    intersectionScript = intersection.AddComponent<TrafficLightsIntersectionSettings>();
                    intersectionScript.position = position;
                    ((TrafficLightsIntersectionSettings)intersectionScript).stopWaypoints = new List<IntersectionStopWaypointsSettings>();
                    ((TrafficLightsIntersectionSettings)intersectionScript).greenLightTime = greenLightTime;
                    ((TrafficLightsIntersectionSettings)intersectionScript).yellowLightTime = yellowLightTime;
                    break;
                default:
                    Debug.LogWarning(intersectionType + " not supported");
                    break;
            }

            return intersectionScript;
        }


        private static void RemoveNonRequiredWaypoints()
        {
            for (int j = allWaypoints.Count - 1; j >= 0; j--)
            {
                if (allWaypoints[j].GetComponent<WaypointSettings>().neighbors.Count == 0)
                {
                    DestroyImmediate(allWaypoints[j].gameObject);
                }
            }
        }


        private static void LinkConnectorsToRoadWaypoints()
        {
            for (int i = 0; i < allConnectors.Count; i++)
            {
                if (allConnectors[i].name.Contains(UrbanSystemConstants.ConnectionEdgeName))
                {
                    bool found = false;
                    for (int j = 0; j < allWaypoints.Count; j++)
                    {
                        if (Vector3.Distance(allConnectors[i].position, allWaypoints[j].position) < 0.01f)
                        {
                            found = true;
                            WaypointSettings connectorScript = allConnectors[i].GetComponent<WaypointSettings>();
                            WaypointSettings waypointScript = allWaypoints[j].GetComponent<WaypointSettings>();

                            if (connectorScript.prev.Count == 0)
                            {
                                connectorScript.prev = waypointScript.prev;
                                waypointScript.prev[0].neighbors.Remove(waypointScript);
                                waypointScript.prev[0].neighbors.Add(connectorScript);
                                break;
                            }

                            if (connectorScript.neighbors.Count == 0)
                            {
                                connectorScript.neighbors = waypointScript.neighbors;
                                waypointScript.neighbors[0].prev.Add(connectorScript);
                                break;
                            }
                            found = false;
                        }

                    }
                    if (found == false)
                    {
                        Debug.Log("Not Found " + allConnectors[i].name, allConnectors[i]);
                    }
                }
            }
        }


        private static void ExtractLaneConnectors(CiDyRoute routeData, Transform node, int laneIndex, int roadIndex, int speedLimit, float laneWidth, int intersectionIndex, List<int> vehicleTypes)
        {
            Transform connectorsHolder = MonoBehaviourUtilities.CreateGameObject("Connectors_" + laneIndex, node, node.position, true).transform;
            List<Vector3> laneConnectors = routeData.waypoints;
            laneConnectors.AddRange(routeData.newRoutePoints);
            TrafficWaypointCreator waypointCreator = new TrafficWaypointCreator();

            for (int i = 0; i < laneConnectors.Count; i++)
            {
                var waypoint = new TempWaypoint();
                waypoint.ListIndex = -1;
                if (i == 0 || i == laneConnectors.Count - 1)
                {
                    waypoint.ListIndex = intersectionIndex;
                    waypoint.Name = "Road_" + roadIndex + "-" + UrbanSystemConstants.LaneNamePrefix + laneIndex + "-" + UrbanSystemConstants.ConnectionEdgeName + i;
                    if (i == 0)
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
                    waypoint.Name = "Road_" + roadIndex + "-" + UrbanSystemConstants.LaneNamePrefix + laneIndex + "-" + UrbanSystemConstants.ConnectionWaypointName + i;
                }
                waypoint.Position = laneConnectors[i];
                waypoint.MaxSpeed = speedLimit;
                waypoint.LaneWidth = laneWidth;
                connectors.Add(waypoint);
                allConnectors.Add(waypointCreator.CreateWaypoint(connectorsHolder, waypoint.Position, waypoint.Name, vehicleTypes, waypoint.MaxSpeed, waypoint.LaneWidth));
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


        private static void LinkAllWaypoints(Transform holder)
        {
            for (int i = 0; i < holder.childCount; i++)
            {
                for (int j = 0; j < holder.GetChild(i).childCount; j++)
                {
                    Transform laneHolder = holder.GetChild(i).GetChild(j);
                    LinkWaypoints(laneHolder);
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


        static void ExtractLaneWaypoints(List<CiDyRoute> lanes, GameObject lanesHolder, string side, int roadIndex, int maxSpeed, float laneWidth, List<int> vehicleTypes)
        {
            if (lanes.Count > 0)
            {
                TrafficWaypointCreator waypointCreator = new TrafficWaypointCreator();
                for (int i = 0; i < lanes.Count; i++)
                {
                    GameObject lane = MonoBehaviourUtilities.CreateGameObject("Lane_" + lanesHolder.transform.childCount + "_" + side, lanesHolder.transform, lanesHolder.transform.position, true);

                    List<Vector3> positions = lanes[i].waypoints;
                    positions.AddRange(lanes[i].newRoutePoints);
                    for (int k = 0; k < positions.Count; k++)
                    {
                        if (k > 0 && positions[k - 1] == positions[k])
                        {
                            continue;
                        }
                        var waypoint = new TempWaypoint();
                        waypoint.MaxSpeed = maxSpeed;
                        waypoint.Name = "Road_" + roadIndex + "-" + UrbanSystemConstants.LaneNamePrefix + (lanesHolder.transform.childCount - 1) + "-" + UrbanSystemConstants.WaypointNamePrefix + k;
                        waypoint.Position = positions[k];
                        waypoint.LaneWidth = laneWidth;

                        allWaypoints.Add(waypointCreator.CreateWaypoint(lane.transform, waypoint.Position, waypoint.Name, vehicleTypes, waypoint.MaxSpeed, waypoint.LaneWidth));
                    }
                }
            }
        }


        private static void LinkOvertakeLanes(Transform holder, int waypointDistance)
        {
            for (int i = 0; i < holder.childCount; i++)
            {
                if (holder.GetChild(i).name.Contains("Road"))
                {
                    for (int j = 0; j < holder.GetChild(i).childCount; j++)
                    {
                        Transform firstLane = holder.GetChild(i).GetChild(j);
                        int laneToLink = j - 1;
                        if (laneToLink >= 0)
                        {
                            LinkLanes(firstLane, holder.GetChild(i).GetChild(laneToLink), waypointDistance);
                        }
                        laneToLink = j + 1;
                        if (laneToLink < holder.GetChild(i).childCount)
                        {
                            LinkLanes(firstLane, holder.GetChild(i).GetChild(laneToLink), waypointDistance);
                        }
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
    }
}
#endif
