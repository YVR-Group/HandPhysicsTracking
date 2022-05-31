using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHand : MonoBehaviour
{
    public Transform targetHand;
    private Rigidbody rb;
    [SerializeField]  Vector3 trackingPositionOffset;
    [SerializeField] Vector3 trackingRotationOffset;
    [SerializeField] GameObject ghost;
    [SerializeField] float maxDistance;
    [SerializeField] float handVelocity;
    private Collider[] _allPhysicBones;
    bool _physicEnabled;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _allPhysicBones = GetComponentsInChildren<Collider>();
        _physicEnabled = true;
       
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, targetHand.position);
        if (distance > maxDistance)
        {
            ghost.SetActive(true);
        }
        else
        {
            ghost.SetActive(false);
        }
    }
    private void FixedUpdate()
    {
        /* rb.velocity = (targetHand.TransformPoint(trackingPositionOffset) - transform.position) * 0.5f / Time.fixedDeltaTime;
         Quaternion targetROt = targetHand.rotation * Quaternion.Euler(trackingRotationOffset);
         Quaternion rotationDifference = targetROt * Quaternion.Inverse(transform.rotation);
         rotationDifference.ToAngleAxis(out float AngleInDegree, out Vector3 rotationAxis);

         Vector3 rotationDifferenceInDegree = AngleInDegree * rotationAxis * 0.5f;
         rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);*/
        // Move using Velocity
        Vector3 positionDelta = targetHand.position - transform.position;
        rb.velocity = Vector3.MoveTowards(rb.velocity, (positionDelta * handVelocity) * Time.fixedDeltaTime, 5f);

        // Rotate using angular velocity
        float angle;
        Vector3 axis;
        Quaternion rotationDelta = targetHand.rotation * Quaternion.Inverse(transform.rotation);
        rotationDelta.ToAngleAxis(out angle, out axis);

        // Fix rotation angle
        if (angle > 180)
        {
            angle -= 360;
        }

        if (angle != 0)
        {
            Vector3 angularTarget = angle * axis;
            angularTarget = (angularTarget * 60f) * Time.fixedDeltaTime;
            rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, angularTarget, 20f);
        }
    }

    public void DisablePhysics()
    {
        if (_physicEnabled)
        {
            Debug.Log("Disable physics");
             for (int i = 0; i < _allPhysicBones.Length; i++)
             {
                 _allPhysicBones[i].enabled = false;
             }
            _physicEnabled = false;
        }
    }
    public void EnablePhysics()
    {
        if (!_physicEnabled)
        {
            Debug.Log("Enable physics");
             for (int i = 0; i < _allPhysicBones.Length; i++)
              {
                  _allPhysicBones[i].enabled = true ;
              }
            _physicEnabled = true;
        }
    }
}
