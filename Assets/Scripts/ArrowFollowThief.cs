using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowFollowThief : MonoBehaviour
{
    public Transform ThiefCar;
    public RectTransform arrow;
    public Camera FollowCam;
    public Vector3 offset=new Vector3(0,1,0);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 thiefScreenPosition =FollowCam.WorldToScreenPoint(ThiefCar.position+offset);
        arrow.position = thiefScreenPosition;
    }
}
