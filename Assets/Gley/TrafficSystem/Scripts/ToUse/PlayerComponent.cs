using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.TrafficSystem
{

    //TODO optimize this.
    public class PlayerComponent : MonoBehaviour, ITrafficParticipant
    {
        private Rigidbody _rb;

#if GLEY_TRAFFIC_SYSTEM
        private List<TrafficWaypoint> _allWaypoints;
        private List<Vector2Int> _cellNeighbors;
       
        private Transform _myTransform;
        private GridDataHandler _gridDataHandler;
        private CellData _currentCell;
        private WaypointManager _waypointManager;
        private TrafficWaypointsDataHandler _trafficWaypointsDataHandler;
        private TrafficWaypoint _proposedTarget;
        private TrafficWaypoint _currentTarget;
        private bool _initialized;
        private bool _targetChanged;

        public delegate void LaneChange();
        public event LaneChange OnLaneChange;

        void TriggetChangeDrivingStateEvent(int fromWaypointIndex, int toWaypointIndex)
        {
            if (OnLaneChange != null)
            {
                if (!_waypointManager.HaveCommonNeighbors(fromWaypointIndex, toWaypointIndex))
                {
                    if (!_trafficWaypointsDataHandler.GetName(fromWaypointIndex).Contains(Gley.UrbanSystem.Internal.UrbanSystemConstants.Connect) && !_trafficWaypointsDataHandler.GetName(toWaypointIndex).Contains(Gley.UrbanSystem.Internal.UrbanSystemConstants.Connect))
                    {
                        OnLaneChange();
                    }
                }
            }
        }


        private void Start()
        {
            StartCoroutine(Initialize());
        }


        IEnumerator Initialize()
        {
            while (!TrafficManager.Instance.IsInitialized())
            {
                yield return null;
            }
            _rb = GetComponent<Rigidbody>();
            _myTransform = transform;
            _gridDataHandler = TrafficManager.Instance.GridDataHandler;
            _waypointManager = TrafficManager.Instance.WaypointManager;
            _trafficWaypointsDataHandler = TrafficManager.Instance.TrafficWaypointsDataHandler;
            _waypointManager.RegisterPlayer(GetInstanceID(), -1);
            _initialized = true;
        }


        // Update is called once per frame
        void Update()
        {
            if (_initialized)
            {
                CellData cell = _gridDataHandler.GetCell(_myTransform.position);
                if (cell != _currentCell)
                {
                    _currentCell = cell;
                    _cellNeighbors = _gridDataHandler.GetCellNeighbors(cell.CellProperties.Row, cell.CellProperties.Column, 1, false);
                    _allWaypoints = new List<TrafficWaypoint>();
                    for (int i = 0; i < _cellNeighbors.Count; i++)
                    {
                        List<int> cellWaypoints = _gridDataHandler.GetAllWaypoints(_cellNeighbors[i]);
                        for (int j = 0; j < cellWaypoints.Count; j++)
                        {
                            _allWaypoints.Add(_trafficWaypointsDataHandler.GetWaypointFromIndex(cellWaypoints[j]));
                        }
                    }
                }

                float oldDistance = Mathf.Infinity;
                for (int i = 0; i < _allWaypoints.Count; i++)
                {
                    float newDistance = Vector3.SqrMagnitude(_myTransform.position - _allWaypoints[i].Position);
                    if (newDistance < oldDistance)
                    {
                        if (CheckOrientation(_allWaypoints[i]))
                        {
                            oldDistance = newDistance;
                        }
                    }
                }
                if (_currentTarget != _proposedTarget)
                {
                    _targetChanged = false;
                    if (_currentTarget != null)
                    {
                        if (_currentTarget.Neighbors.Contains(_proposedTarget.ListIndex))
                        {
                            _targetChanged = true;
                        }
                        else
                        {
                            float angle1 = Vector3.SignedAngle(_myTransform.forward, _proposedTarget.Position - _myTransform.position, Vector3.up);
                            float angle2 = Vector3.SignedAngle(_myTransform.forward, _currentTarget.Position - _myTransform.position, Vector3.up);                          
                            if (Mathf.Abs(angle1) < Mathf.Abs(angle2))
                            {
                                _targetChanged = true;
                                TriggetChangeDrivingStateEvent(_currentTarget.ListIndex, _proposedTarget.ListIndex);
                            }
                            else
                            {
                                float distance1 = Vector3.SqrMagnitude(_myTransform.position - _proposedTarget.Position);
                                float distance2 = Vector3.SqrMagnitude(_myTransform.position - _currentTarget.Position);
                                if (distance1 * distance1 < distance2)
                                {
                                    _targetChanged = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        _targetChanged = true;
                    }

                    if (_targetChanged)
                    {
                        _currentTarget = _proposedTarget;
                        _waypointManager.UpdatePlayerWaypoint(GetInstanceID(), _proposedTarget.ListIndex);
                    }
                }
            }
        }


        private bool CheckOrientation(TrafficWaypoint waypoint)
        {
            if (waypoint.Neighbors.Length < 1)
                return false;

            TrafficWaypoint neighbor = _trafficWaypointsDataHandler.GetWaypointFromIndex(waypoint.Neighbors[0]);
            float angle = Vector3.SignedAngle(_myTransform.forward, neighbor.Position - waypoint.Position, Vector3.up);
            if (Math.Abs(angle) < 90)
            {
                _proposedTarget = neighbor;
                return true;
            }
            return false;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (_initialized)
                {
                    if (TrafficManager.Instance.DebugManager.IsDebugWaypointsEnabled())
                    {
                        if (_currentTarget != null)
                        {
                            Gizmos.color = Color.green;
                            Vector3 position = _currentTarget.Position;
                            Gizmos.DrawSphere(position, 1);
                        }
                    }
                }
            }
        }
#endif
#endif

        public float GetCurrentSpeedMS()
        {
            return _rb.velocity.magnitude;
        }
    }

}
