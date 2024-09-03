using UnityEngine;

namespace Gley.UrbanSystem.Internal
{
    public class DebugBounds : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(smr.bounds.center, smr.bounds.size);
            }
        }
    }
}