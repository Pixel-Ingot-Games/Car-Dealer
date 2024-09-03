using System.Collections.Generic;
using System.Linq;

namespace Gley.TrafficSystem.Internal
{
    /// <summary>
    /// Stores all active drive actions for a vehicle.
    /// </summary>
    internal readonly struct ActiveActions
    {
        private readonly List<DriveAction> _activeActions;

        internal readonly List<DriveAction> CurrentActiveActions => _activeActions;


        public ActiveActions(List<DriveAction> activeActions)
        {
            _activeActions = activeActions;
        }


        internal readonly void Add(DriveAction newAction)
        {
            if (!_activeActions.Contains(newAction))
            {
                _activeActions.Add(newAction);
            }
        }


        internal readonly void Remove(DriveActions actionType)
        {
            _activeActions.RemoveAll(cond => cond.ActionType == actionType);
        }


        internal readonly void RemoveAll(DriveActions[] movingActions)
        {
            _activeActions.RemoveAll(cond => movingActions.Contains(cond.ActionType));
        }


        internal readonly void Insert(int position, DriveAction newAction)
        {
            _activeActions.Insert(position, newAction);
        }


        internal readonly bool Contains(DriveActions actionType)
        {
            return _activeActions.Any(cond => cond.ActionType == actionType);
        }
    }


    internal readonly struct DriveAction
    {
        private readonly DriveActions _actionType;
        private readonly RoadSide _side;

        internal readonly RoadSide Side => _side;
        internal readonly DriveActions ActionType => _actionType;


        public DriveAction(DriveActions actionType, RoadSide side)
        {
            _actionType = actionType;
            _side = side;
        }
    }
}