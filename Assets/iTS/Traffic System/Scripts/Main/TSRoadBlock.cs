using UnityEngine;
using System.Collections;
using ITS.AI;
using ITS.Utils;

public class TSRoadBlock : MonoBehaviour {


	public TSTrafficLight.TSPointReference[] blockingPoints = new TSTrafficLight.TSPointReference[0];

	public TSMainManager manager;

	public float roadBlockAheadDistance = 40f;

	public float range = 10;
	private int myID;

	// Use this for initialization
	void Awake () {
		myID=TSUtils.GetUniqueID();
		if (manager ==null)
			manager = GameObject.FindObjectOfType<TSMainManager>();
	}

	void OnEnable()
	{
		if (manager !=null)
			BlockPoints();
	}

	void OnDisable()
	{
		if (manager !=null){
			UnBlockPoints();
		}
	}

	public void BlockPoints()
	{
		for (int i =0; i < blockingPoints.Length;i++)
		{
			SetPointReservationID(blockingPoints[i],myID);
		}
	}



	public void UnBlockPoints()
	{
		foreach (var point in blockingPoints)
		{
			UnReservePoint(point);
		}
	}

	private void UnReservePoint(TSTrafficLight.TSPointReference point)
	{
		if (point.connector == -1)
		{
			manager.lanes[point.lane].points[point.point].TryUnReservePoint(myID);
		}
		else
		{
			manager.lanes[point.lane].connectors[point.connector].points[point.point].TryUnReservePoint(myID);
			manager.lanes[point.lane].points[manager.lanes[point.lane].points.Length-1].TryUnReservePoint(myID);
		}
		
		SetRoadBlockAhead(point, false);
	}

	private void SetPointReservationID(TSTrafficLight.TSPointReference point, int reservationID)
	{
		if (point.connector == -1)
		{
			manager.lanes[point.lane].points[point.point].TryReservePoint(reservationID, null, true);
		}
		else
		{
			manager.lanes[point.lane].connectors[point.connector].points[point.point].TryReservePoint(reservationID, null, true);
			manager.lanes[point.lane].points[manager.lanes[point.lane].points.Length-1].TryReservePoint(reservationID, null, true);
		}
		SetRoadBlockAhead(point, (reservationID !=0));
	}

	private void SetRoadBlockAhead(TSTrafficLight.TSPointReference point,bool setRoadBlock)
	{
		float dist =0;
		int currentPoint = point.point;
		while (dist < roadBlockAheadDistance && currentPoint >=0)
		{
			manager.lanes[point.lane].points[currentPoint].roadBlockAhead = setRoadBlock;
			dist +=manager.lanes[point.lane].points[currentPoint].distanceToNextPoint;
			currentPoint--;
		}
	}


}
