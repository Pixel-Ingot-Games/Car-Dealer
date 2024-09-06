
using System.Collections.Generic;
using System.Linq;
#if CiDy_PRESENT && CiDy
using CiDy;
#endif
using ITS.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CiDyLaneCreator : BaseInstallableModule
{
    #if CiDy_PRESENT
    [SerializeField] private float tolerance = 0.4f;
    [SerializeField] private TSMainManager tsMainManager;
    [SerializeField] private CiDyGraph ciDyGraph;

    public void CreateRoad(TSMainManager mainManager)
    {
        tsMainManager = mainManager;
        tsMainManager.Clear();
        ciDyGraph = GameObject.FindObjectOfType<CiDyGraph>();

        if (ciDyGraph == null){ return; }

        ciDyGraph.BuildTrafficData();
        CreateLanes();
        
        foreach (var ciDyNode in ciDyGraph.masterGraph)
        {
            CreateCulDeSacAndConsecutiveRoadsConnexions(ciDyNode);

            CreateJunctionsAndTrafficLights(ciDyNode);
        }
        
        tsMainManager.ProcessJunctions(false, 0f);
    }

    private void CreateCulDeSacAndConsecutiveRoadsConnexions(CiDyNode ciDyNode)
    {
        foreach (var route in ciDyNode.leftRoutes.routes)
        {
            CreateConnectorFromRoute(route);
        }

        foreach (var route in ciDyNode.rightRoutes.routes)
        {
            CreateConnectorFromRoute(route);
        }
    }

    private void CreateJunctionsAndTrafficLights(CiDyNode ciDyNode)
    {
        List<TSTrafficLight> trafficLights = new List<TSTrafficLight>();

        foreach (var intersectionRoute in ciDyNode.intersectionRoutes.intersectionRoutes)
        {
            CreateConnectorFromRoute(intersectionRoute.route);
            var trafficLight = AddTrafficLight(intersectionRoute);
            if (trafficLights.Contains(trafficLight) == false)
            {
                trafficLights.Add(trafficLight);
            }
        }

        if (ciDyNode.connectedRoads.Count > 1)
        {
            TSTrafficLightGroupManager.AutoSyncTrafficLights(trafficLights.ToArray(), 15f, 1f);
        }
    }

    private void CreateLanes()
    {
        foreach (var roadGO in ciDyGraph.roads)
        {
            var road = roadGO.GetComponent<CiDyRoad>();
            foreach (var route in road.leftRoutes.routes)
            {
                tsMainManager.AddLane<TSLaneInfo>(route.waypoints.ToArray(), tolerance);
            }

            foreach (var route in road.rightRoutes.routes)
            {
                tsMainManager.AddLane<TSLaneInfo>(route.waypoints.ToArray(), tolerance);
            }
        }
    }

    private TSTrafficLight AddTrafficLight(CiDyIntersectionRoute intersectionRoute)
    {
        var trafficLight = intersectionRoute.light.gameObject.GetComponent<TSTrafficLight>();
        if (trafficLight == null)
        {
            trafficLight = intersectionRoute.light.gameObject.AddComponent<TSTrafficLight>();            
        }
        
        trafficLight.lights = new List<TSTrafficLight.TSLight>();
        AddLights(trafficLight);
        AddConnectorsToTrafficLight(intersectionRoute, trafficLight);
        trafficLight.manager = tsMainManager;
        return trafficLight;
    }

    private void AddConnectorsToTrafficLight(CiDyIntersectionRoute intersectionRoute, TSTrafficLight trafficLight)
    {
        var laneInfo = tsMainManager.lanes.FindNearestLastPoint(intersectionRoute.finalRoutePoint);
        var laneIndex = tsMainManager.lanes.FindIndex(laneInfo);
        trafficLight.pointsNormalLight.Clear();
        
        for (int i = 0; i < laneInfo.connectors.Length; i++)
        {
            trafficLight.pointsNormalLight.Add(new TSTrafficLight.TSPointReference()
            {
                connector = i,
                lane = laneIndex,
                point = 0
            });    
        }
    }
    
    private static void AddLights(TSTrafficLight trafficLight)
    {
        var lightGameObject = trafficLight.transform.Find("GreenLight").gameObject;
        trafficLight.AddLight(lightGameObject, TSTrafficLight.LightType.Green, 15);
        var yellowLightGameObject = trafficLight.transform.Find("YellowLight").gameObject;
        trafficLight.AddLight(yellowLightGameObject, TSTrafficLight.LightType.Yellow, 1);
        var redLightGameObject = trafficLight.transform.Find("RedLight").gameObject;
        trafficLight.AddLight(redLightGameObject, TSTrafficLight.LightType.Red, 15);
    }

    private void CreateConnectorFromRoute(CiDyRoute route)
    {
        var laneFromPoint = route.waypoints.First();
        var laneToPoint = route.waypoints.Last();
        var laneFrom = tsMainManager.lanes.FindNearestLastPoint(laneFromPoint);
        var laneTo = tsMainManager.lanes.FindNearestFirstPoint(laneToPoint);
        tsMainManager.AddConnector<TSLaneConnector>(laneFrom, laneTo, route.waypoints.ToArray());
    }
    #endif

#if UNITY_EDITOR && CiDy_PRESENT
    private bool foldOut = true;

    public override void OnGUI(TSMainManager mainManager)
    {
        tolerance = EditorGUILayout.FloatField(new GUIContent("Tolerance", "Higher values makes it generate fewer lane points and would be less accurate (the lane shape)"), tolerance);
        if (GUILayout.Button("Create Roads"))
        {
            CreateRoad(mainManager);    
        }
    }

    public override void OnGUI() { }
#endif

    public override string Name => "CiDy";
    public override string Description => "Auto creates the lanes and lane connectors from an CiDy instance";
    public override string Define => "CiDy_PRESENT";
    public override bool Installed
    {
        get
        {
#if CiDy_PRESENT
            return true;
#else
            return false;
#endif
        }
    }

    public override bool Detected
    {
        get
        {
#if CiDy
            return TSUtils.TypeExists("CiDyGraph");
#else
            return false;
#endif      
        }
    }
}