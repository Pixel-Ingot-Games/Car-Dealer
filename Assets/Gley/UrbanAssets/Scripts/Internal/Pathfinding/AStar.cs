using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    internal class AStar
    {
        /// <summary>
        /// Find a path between 2 waypoints.
        /// Uses A* algorithm.
        /// </summary>
        internal List<int> FindPath(int startNodeIndex, int targetNodeIndex, int pedestraianTypes, PathFindingWaypoint[] allPathFindingWaypoints)
        {
            var startNode = allPathFindingWaypoints[startNodeIndex];
            var targetNode = allPathFindingWaypoints[targetNodeIndex];
            Heap<PathFindingWaypoint> openSet = new Heap<PathFindingWaypoint>(allPathFindingWaypoints.Length);
            HashSet<PathFindingWaypoint> closedSet = new HashSet<PathFindingWaypoint>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathFindingWaypoint currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode, allPathFindingWaypoints);
                }

                for (int i = 0; i < currentNode.Neighbours.Length; i++)
                {
                    PathFindingWaypoint neighbour = allPathFindingWaypoints[currentNode.Neighbours[i]];
                    if (!neighbour.AllowedAgents.Contains(pedestraianTypes) || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + currentNode.MovementPenalty[i];
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode.ListIndex;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
            return null;
        }


        private List<int> RetracePath(PathFindingWaypoint startNode, PathFindingWaypoint endNode, PathFindingWaypoint[] allPathFindingWaypoints)
        {
            List<int> path = new List<int>();
            PathFindingWaypoint currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.ListIndex);
                currentNode = allPathFindingWaypoints[currentNode.Parent];
            }
            path.Reverse();
            return path;
        }


        /// <summary>
        /// Approximate distance between 2 waypoints.
        /// </summary>
        private int GetDistance(PathFindingWaypoint nodeA, PathFindingWaypoint nodeB)
        {
            float dstX = Mathf.Abs(nodeA.WorldPosition.x - nodeB.WorldPosition.x);
            float dstY = Mathf.Abs(nodeA.WorldPosition.z - nodeB.WorldPosition.z);

            if (dstX > dstY)
                return (int)(14 * dstY + 10 * (dstX - dstY));
            return (int)(14 * dstX + 10 * (dstY - dstX));
        }
    }
}
