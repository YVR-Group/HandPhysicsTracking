using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public Vector3 spawnPos;
    public GameObject deathParticle;
    public bool autoRespawn = false;
    public bool isDestroyOnEverything = false;
    // Start is called before the first frame update
    void Start()
    {
        spawnPos = this.transform.position; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Sword") || isDestroyOnEverything)
        {
            if(autoRespawn)
            Instantiate(this.gameObject, spawnPos, Quaternion.identity);
            GameObject a = Instantiate(deathParticle, transform.position,Quaternion.identity);
            a.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            Destroy(gameObject);
        }
    }
}
