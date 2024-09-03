using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores all properties of a play mode waypoint.
    /// </summary>
    [System.Serializable]
    public class TrafficWaypoint : Waypoint
    {
        private List<IIntersection> _associatedIntersections;

        [SerializeField] private VehicleTypes[] _allowedVehicles;
        [SerializeField] private int[] _giveWayList;
        [SerializeField] private int[] _otherLanes;
        [SerializeField] private string _eventData;
        [SerializeField] private float _laneWidth;
        [SerializeField] private int _maxSpeed;
        [SerializeField] private bool _giveWay;
        [SerializeField] private bool _complexGiveWay;
        [SerializeField] private bool _zipperGiveWay;
        [SerializeField] private bool _triggerEvent;
        [SerializeField] private bool _enter;
        [SerializeField] private bool _exit;
        [SerializeField] private bool _stop;

        public int[] GiveWayList => _giveWayList;
        public VehicleTypes[] AllowedVehicles => _allowedVehicles;
        public int[] OtherLanes => _otherLanes;
        public int MaxSpeed => _maxSpeed;
        public float LaneWidth => _laneWidth;
        public bool ComplexGiveWay => _complexGiveWay;
        public bool ZipperGiveWay => _zipperGiveWay;
        public bool Enter => _enter;
        public bool Exit => _exit;
        public List<IIntersection> AssociatedIntersections => _associatedIntersections;

        public bool Stop
        {
            get
            {
                return _stop;
            }
            set
            {
                _stop = value;
            }
        }
        public bool GiveWay
        {
            get
            {
                return _giveWay;
            }
            set
            {
                _giveWay = value;
            }
        }
        public bool TriggerEvent
        {
            get
            {
                return _triggerEvent;
            }
            set
            {
                _triggerEvent = value;
            }
        }
        public string EventData
        {
            get
            {
                return _eventData;
            }
            set
            {
                _eventData = value;
            }
        }


        public TrafficWaypoint(string name, int listIndex, Vector3 position, List<VehicleTypes> allowedVehicles, int[] neighbors, int[] prev, int[] otherLanes, int maxSpeed, bool giveWay,
            bool complexGiveWay, bool zipperGiveWay, bool triggerEvent, float laneWidth, string eventData, int[] giveWayList)
            : base(name, listIndex, position, neighbors, prev)
        {
            _maxSpeed = maxSpeed;
            _giveWay = giveWay;
            _complexGiveWay = complexGiveWay;
            _zipperGiveWay = zipperGiveWay;
            _giveWayList = giveWayList;
            _laneWidth = laneWidth;
            _otherLanes = otherLanes;
            _enter = false;
            _exit = false;
            _stop = false;
            _triggerEvent = triggerEvent;
            _eventData = eventData;
            _allowedVehicles = allowedVehicles.ToArray();
        }


        /// <summary>
        /// Initializes current waypoint properties
        /// Used by intersections
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="giveWay"></param>
        /// <param name="stop"></param>
        /// <param name="enter"></param>
        /// <param name="exit"></param>
        public void SetIntersection(IIntersection intersection, bool giveWay, bool stop, bool enter, bool exit)
        {
            if(_associatedIntersections==null)
            {
                _associatedIntersections=new List<IIntersection>();
            }
            if(!_associatedIntersections.Contains(intersection))
            {
                _associatedIntersections.Add(intersection);
            }
            _stop = stop;
            _giveWay = giveWay;
            _exit = exit;
            _enter = enter;
        }
    }
}