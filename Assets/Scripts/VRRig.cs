using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;


    public void Map()
    {
      //  if (rb == null)
      //  {
            rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
            rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
       // }
    }

}

public class VRRig : MonoBehaviour
{
    public VRMap head;
    public VRMap lefthand;
    public VRMap righthand;
    public Transform headConstraint;
    public Vector3 headBodyOffset;
    // Start is called before the first frame update
    void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = headConstraint.position+headBodyOffset;
        transform.forward = Vector3.ProjectOnPlane( headConstraint.up,Vector3.up).normalized;

    }

    private void FixedUpdate()
    {
        head.Map();
        righthand.Map();
        lefthand.Map();
    }
}
