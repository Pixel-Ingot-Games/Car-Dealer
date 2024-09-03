using System.Collections.Generic;
using UnityEngine;
namespace Gley.UrbanSystem.Internal
{
    public class WaypointSettingsBase : MonoBehaviour
    {
        public List<WaypointSettingsBase> neighbors;
        public List<WaypointSettingsBase> prev;

        //public bool stop;
        public bool draw = true;
        public bool inView;
        public Vector3 position;

        //priority
        public bool priorityLocked;
        public int priority;

        //events
        public bool triggerEvent;
        public string eventData;

        //path finding
        public List<int> distance;
        public bool penaltyLocked;
        public int penalty;

        public virtual void Initialize()
        {
            neighbors = new List<WaypointSettingsBase>();
            prev = new List<WaypointSettingsBase>();
        }

        public virtual void VerifyAssignments(bool showPrevsWarning)
        {
            if (neighbors == null)
            {
                neighbors = new List<WaypointSettingsBase>();
            }

            for (int j = neighbors.Count - 1; j >= 0; j--)
            {
                if (neighbors[j] == null)
                {
                    neighbors.RemoveAt(j);
                }
            }

            if (prev == null)
            {
                prev = new List<WaypointSettingsBase>();
            }

            for (int j = prev.Count - 1; j >= 0; j--)
            {
                if (prev[j] == null)
                {
                    prev.RemoveAt(j);
                }
            }

            if (distance == null)
            {
                distance = new List<int>();
            }

            if (priority <= 0)
            {
                priority = 1;
            }
        }

        public void SetPriorityForAllNeighbors(int newPriority)
        {
            Queue<WaypointSettingsBase> queue = new Queue<WaypointSettingsBase>();
            HashSet<WaypointSettingsBase> visited = new HashSet<WaypointSettingsBase>();

            // Start with the current waypoint
            queue.Enqueue(this);
            visited.Add(this);
            priorityLocked = false;

            while (queue.Count > 0)
            {
                WaypointSettingsBase current = queue.Dequeue();
                if (!current.priorityLocked)
                {
                    current.priority = newPriority;
                    // Enqueue all unvisited neighbors
                    foreach (WaypointSettingsBase neighbor in current.neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }
            if (priority != 0)
            {
                priorityLocked = true;
            }
            Debug.Log("Done");
        }


        public void SetPenaltyForAllNeighbors(int newPenalty)
        {
            Queue<WaypointSettingsBase> queue = new Queue<WaypointSettingsBase>();
            HashSet<WaypointSettingsBase> visited = new HashSet<WaypointSettingsBase>();

            // Start with the current waypoint
            queue.Enqueue(this);
            visited.Add(this);
            penaltyLocked = false;

            while (queue.Count > 0)
            {
                WaypointSettingsBase current = queue.Dequeue();
                if (!current.penaltyLocked)
                {
                    current.penalty = newPenalty;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(current);
#endif
                    // Enqueue all unvisited neighbors
                    foreach (WaypointSettingsBase neighbor in current.neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }
            if (penalty != 0)
            {
                penaltyLocked = true;
            }
            Debug.Log("Done");
        }
    }
}