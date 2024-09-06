using System;
using System.Linq;
#if EASYROADS_PRESENT
using EasyRoads3Dv3;
#endif
using ITS.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class EasyRoadsLaneCreator : BaseInstallableModule
{
#if EASYROADS_PRESENT
    [SerializeField] private bool reverse;
    [SerializeField] private float laneWidth = 2f;
    [SerializeField] private float tolerance = 0.4f;
    [SerializeField] private TSMainManager tsMainManager;
    
    [EditorButton]
    [ContextMenu("Create Road")]
    private void CreateRoad(TSMainManager mainManager)
    {
        tsMainManager = mainManager;
        tsMainManager.Clear();
        var roads = GetRoads();
        CreateLanes(roads);
        CreateRoadConnexions(roads);
        tsMainManager.ProcessJunctions(false, 0f);
    }

    private void CreateRoadConnexions(ERRoad[] roads)
    {
        foreach (ERRoad road in roads)
        {
            var laneCount = road.GetLaneCount();
            for (int laneIndex = 0; laneIndex < laneCount; laneIndex++)
            {
                var connection = road.GetConnectionAtEnd(out var connectionIndex);
                AddConnection(connection, connectionIndex, laneIndex);
                connection = road.GetConnectionAtStart(out connectionIndex);
                AddConnection(connection, connectionIndex, laneIndex);
            }
        }
    }

    private void CreateLanes(ERRoad[] roads)
    {
        foreach (ERRoad road in roads)
        {
            var laneCount = road.GetLaneCount();
            for (int laneIndex = 0; laneIndex < laneCount; laneIndex++)
            {
                var laneData = road.GetLaneData(laneIndex);
                var points = (reverse ? laneData.points.Reverse() : laneData.points).ToArray();
                tsMainManager.AddLane<TSLaneInfo>(points, tolerance);
                tsMainManager.lanes.Last().laneWidth = laneWidth;
            }
        }
    }

    private static ERRoad[] GetRoads()
    {
        var roadNetwork = new ERRoadNetwork();
        //var trafficDirection = roadNetwork.GetTrafficDirection(); Future implementation on EasyRoads

        var roads = roadNetwork.GetRoads();
        return roads;
    }

    private void AddConnection(ERConnection connection, int connectionIndex, int laneIndex)
    {
        var data = connection.GetLaneData(connectionIndex, laneIndex);

        foreach (var laneConnector in data)
        {
            var laneFromPoint = reverse ? laneConnector.points.Last() : laneConnector.points.First();
            var laneToPoint = reverse ? laneConnector.points.First() : laneConnector.points.Last();
            var laneFrom = tsMainManager.lanes.FindNearestLastPoint(laneFromPoint);
            var laneTo = tsMainManager.lanes.FindNearestFirstPoint(laneToPoint);
            var points = (reverse ? laneConnector.points.Reverse() : laneConnector.points).ToArray();
            tsMainManager.AddConnector<TSLaneConnector>(laneFrom, laneTo, points);
        }
    }

#endif

#if UNITY_EDITOR && EASYROADS_PRESENT

    public override void OnGUI(TSMainManager mainManager)
    {
        reverse = EditorGUILayout.Toggle("Reverse lanes", reverse);
        laneWidth = EditorGUILayout.FloatField("Lane Width", laneWidth);
        tolerance = EditorGUILayout.FloatField(new GUIContent("Tolerance", "Higher values makes it generate fewer lane points and would be less accurate (the lane shape)"), tolerance);
        if (GUILayout.Button("Create Roads"))
        {
            CreateRoad(mainManager);
        }
    }

    public override void OnGUI()
    {
    }
#endif

    public override string Name => "EasyRoads";
    public override string Description => "Auto creates the lanes and lane connectors from an easy roads instance";
    public override string Define => "EASYROADS_PRESENT";

    public override bool Installed
    {
        get
        {
#if EASYROADS_PRESENT
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
#if UNITY_EDITOR
            return TSUtils.TypeExists("ERRoadNetwork") && TSUtils.MethodExists("ERRoad", "GetLaneCount");
            
#else
            return false;
#endif
        }
    }
}