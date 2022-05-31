using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcePower : MonoBehaviour
{
    public List<Renderer> visibleRenderers = new List<Renderer>();
    public float pullingDistance;
    public float pullingPower;
    public LayerMask mylayerMask;
    private void Update()
    {
        if (visibleRenderers.Count != 0)
        {
            foreach (Renderer renderer in visibleRenderers)
            {
                if(renderer != null)
                    PullObject(renderer.gameObject, 0.1f);
            }
        }
    }
    /* void FireRay()
     {
         RaycastHit hitData;
         bool hit = Physics.Raycast(transform.position, transform.forward,out hitData, pullingDistance,mylayerMask);
         if (hit)
         {
             PullObject(hitData.collider.gameObject);
         }
     }*/

    private void PullObject(GameObject gameobj,float mul)
    {
        if((mylayerMask.value & (1 << gameobj.layer)) > 0)
        {
            Rigidbody target = gameobj.GetComponentInParent<Rigidbody>();
            if (target != null)
            {
               // Debug.Log("HIT : " + target.gameObject + ", TAG " + target.tag);
                if (target.CompareTag("Grabbable"))
                {
                //    Debug.Log("Found graabble");
                    target.AddForce((transform.position - target.transform.position) * pullingPower*mul);
                }
                else
                {
                   // Debug.Log("Found normal");
                    target.AddForce((transform.position - target.transform.position) * pullingPower*mul);
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 direction = transform.forward * 10;
        Gizmos.DrawRay(transform.position, direction);
    }

    void OnEnable()
    {
        FindObjects();
    }

    // Find and store visible renderers to a list
    void FindObjects()
    {
        // Retrieve all renderers in scene
        Renderer[] sceneRenderers = FindObjectsOfType<Renderer>();

        // Store only visible renderers
        visibleRenderers.Clear();
        for (int i = 0; i < sceneRenderers.Length; i++)
            if (IsVisible(sceneRenderers[i]))
                visibleRenderers.Add(sceneRenderers[i]);

        // debug console
        string result = "Total Renderers = " + sceneRenderers.Length + ".  Visible Renderers = " + visibleRenderers.Count;
        foreach (Renderer renderer in visibleRenderers)
            PullObject(renderer.gameObject,1);
    }
    

    // Is the renderer within the camera frustrum?
    bool IsVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return (GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) ? true : false;
    }
}
