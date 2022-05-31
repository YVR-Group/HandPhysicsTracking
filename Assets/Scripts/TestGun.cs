using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGun : MonoBehaviour
{
    bool isGrabbing;
    public GameObject shotParticle;
    public Transform shotOffset;
    public GameObject bullet;
    public float force;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGrabbing(bool res)
    {
        isGrabbing = res;
    }

    public void Shot()
    {
        if (isGrabbing)
        {
            GameObject a = Instantiate(shotParticle, shotOffset);
            a.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            a.transform.localPosition = Vector3.zero;
            GameObject b = Instantiate(bullet, shotOffset.transform.position, Quaternion.identity);
            b.GetComponent<Rigidbody>().AddForce((shotOffset.transform.forward) * force);
        }
    }
}
