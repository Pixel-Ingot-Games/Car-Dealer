using Gley.UrbanSystem.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal abstract class RoadEditorData<T> : EditorData where T : RoadBase
    {
        private bool _hasErrors;

        protected T[] _allRoads;


        protected RoadEditorData()
        {
            LoadAllData();
        }


        internal abstract T[] GetAllRoads();


        internal bool HasErrors()
        {
            return _hasErrors;
        }


        protected override void LoadAllData()
        {
            List<T> tempRoads;
            if (GleyPrefabUtilities.EditingInsidePrefab())
            {
                GameObject prefabRoot = GleyPrefabUtilities.GetScenePrefabRoot();
                tempRoads = prefabRoot.GetComponentsInChildren<T>().ToList();
                for (int i = 0; i < tempRoads.Count; i++)
                {
                    tempRoads[i].positionOffset = prefabRoot.transform.position;
                    tempRoads[i].rotationOffset = prefabRoot.transform.localEulerAngles;
                }
            }
            else
            {
                tempRoads = GleyPrefabUtilities.GetAllComponents<T>().ToList();
                for (int i = 0; i < tempRoads.Count; i++)
                {
                    tempRoads[i].isInsidePrefab = GleyPrefabUtilities.IsInsidePrefab(tempRoads[i].gameObject);
                    if (tempRoads[i].isInsidePrefab)
                    {
                        tempRoads[i].positionOffset = GleyPrefabUtilities.GetInstancePrefabRoot(tempRoads[i].gameObject).transform.position;
                        tempRoads[i].rotationOffset = GleyPrefabUtilities.GetInstancePrefabRoot(tempRoads[i].gameObject).transform.localEulerAngles;
                    }
                }
            }

            // Verifications.
            for (int i = tempRoads.Count - 1; i >= 0; i--)
            {
                if (tempRoads[i].path == null || tempRoads[i].path.NumPoints < 4)
                {
                    GleyPrefabUtilities.DestroyImmediate(tempRoads[i].gameObject);
                    tempRoads.RemoveAt(i);
                    continue;
                }

                if (!tempRoads[i].VerifyAssignments())
                {
                    _hasErrors = true;
                }

                tempRoads[i].startPosition = tempRoads[i].path[0] + tempRoads[i].positionOffset;
                tempRoads[i].endPosition = tempRoads[i].path[tempRoads[i].path.NumPoints - 1] + tempRoads[i].positionOffset;
            }

            _allRoads = tempRoads.ToArray();
        }
    }
}
