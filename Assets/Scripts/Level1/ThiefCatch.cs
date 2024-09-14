using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefCatch : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject EndCutScene;
    public GameObject Endcam;
    public GameObject police;
    public GameObject thief;
    private int collide = 0;
    void Start()
    {
        EndCutScene.SetActive(false);
        Endcam.SetActive(false);
    }
    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("ThiefCar"))
        {
            collide++;
        }
        if(collide==2)
        {
            EndCutScene.SetActive(true);
            Endcam.SetActive(true);
            police.SetActive(true);
            thief.SetActive(true);

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
