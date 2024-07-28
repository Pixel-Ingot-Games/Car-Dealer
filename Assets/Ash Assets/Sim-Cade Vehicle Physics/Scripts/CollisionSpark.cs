using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ashsvp
{
    public class CollisionSpark : MonoBehaviour
    {
        public GameObject CollisionSpark_prefab;
        public float Collision_impulse = 7000f;
        public AudioClip collision_sfx;

        private AudioSource CollisionAudioSource;


        private void Start()
        {
            CollisionAudioSource = gameObject.AddComponent<AudioSource>();
            CollisionAudioSource.spatialBlend = 1f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.impulse.magnitude > Collision_impulse)
            {
                GameObject spark = Instantiate(CollisionSpark_prefab, collision.contacts[0].point, Quaternion.identity);
                CollisionAudioSource.PlayOneShot(collision_sfx);
            }
        }

    }

}
