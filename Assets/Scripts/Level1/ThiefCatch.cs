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

    public GameObject Lvl2EndScene;
    private int thiefCarOneHits = 0;  
    private int thiefCarTwoHits = 0;


    private int collide = 0;
    void Start()
    {
        EndCutScene.SetActive(false);
        Endcam.SetActive(false);
        Lvl2EndScene.SetActive(false);
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ThiefCar"))
        {
            collide++;
        }
        if (collide == 2)
        {
            EndCutScene.SetActive(true);
            Endcam.SetActive(true);
            police.SetActive(true);
            thief.SetActive(true);

        }
        //Level2 Functionality 

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("ThiefCar1"))
            {
                thiefCarOneHits++;


                if (thiefCarOneHits == 2)
                {
                    Debug.Log("ThiefCar1 hit twice.");
                    CheckBothCarsHitTwice();
                }
            }

            if (collision.gameObject.CompareTag("ThiefCar2"))
            {
                thiefCarTwoHits++;

                if (thiefCarTwoHits == 2)
                {
                    Debug.Log("ThiefCar2 hit twice.");
                    CheckBothCarsHitTwice();
                }
            }
        }


    }
    void CheckBothCarsHitTwice()
    {
        if (thiefCarOneHits == 2 && thiefCarTwoHits == 2)
        {
            
            
                Lvl2EndScene.SetActive(true);

            Debug.Log("Both Thief Cars hit twice. Triggering cutscene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
