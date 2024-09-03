using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal static class Customizations 
    {
        private const float _referenceDistance = 35;
        private const float _anchorSize = 0.5f;
        private const float _controlSize = 1;
        private const float _roadConnectorSize = 1;


        internal static float GetZoomPercentage(Vector3 cameraPoz, Vector3 objPoz)
        {
            float cameraDistace = Vector3.Distance(cameraPoz, objPoz);
            return  cameraDistace / _referenceDistance;
        }


        internal static float GetRoadConnectorSize(Vector3 camPoz, Vector3 objPoz)
        {
            return GetZoomPercentage(camPoz,objPoz) * _roadConnectorSize;
        }


        internal static float GetControlPointSize(Vector3 camPoz, Vector3 objPoz)
        {
            return GetZoomPercentage(camPoz,objPoz) * _controlSize;
        }


        internal static float GetAnchorPointSize(Vector3 camPoz, Vector3 objPoz)
        {
            return GetZoomPercentage(camPoz, objPoz) * _anchorSize;
        }
    }
}
