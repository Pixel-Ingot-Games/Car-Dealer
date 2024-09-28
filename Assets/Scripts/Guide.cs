using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
      public ParticleSystem particles;
    private int times = 0;
    public GameObject c1;
    public GameObject c2;
    public GameObject c3;
    public GameObject c4;


    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PoliceCar"))
            {
            times++;  
            Destroy(gameObject);  
            Debug.Log("Checkpoints reached: " + times);

            if (times == 1)
            {
                Debug.Log("Player reached 2nd checkpoint, removing blocks around thief car.");
                c1.SetActive(false);
                c2.SetActive(false);
                c3.SetActive(false);
                c4.SetActive(false);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
