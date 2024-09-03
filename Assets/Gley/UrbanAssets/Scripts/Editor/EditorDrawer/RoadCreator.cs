using Gley.UrbanSystem.Internal;
using UnityEditor;

namespace Gley.UrbanSystem.Editor
{
    internal abstract class RoadCreator<T, R, X> where T : RoadBase where R : GenericConnectionPool<X> where X : ConnectionCurveBase
    {
        protected RoadEditorData<T> _data;


        protected RoadCreator(RoadEditorData<T> data)
        {
            _data = data;
        }


        internal void DeleteCurrentRoad(RoadBase road)
        {
            Undo.DestroyObjectImmediate(road.gameObject);
            _data.TriggerOnModifiedEvent();
        }
    }
}
