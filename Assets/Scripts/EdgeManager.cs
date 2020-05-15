using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeManager : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    
    public void GenerateLine(Vector3 p1, Vector3 p2, Transform parenTransform = null)
    {
        float xRot = 90;
        float yRot = 0;
        //use the mid point of the two points to position the prefab since the anchor is the center of the quad aka the
        //center of our line
        Vector3 midPoint = Vector3.zero;

        midPoint = (p1 + p2) / 2;
        
        //To make quads always visible to the (isometric) camera there are some specific rotation angles needed. When a
        //line is moving only along the x-axis it's y rot needs to be 90 and when a line is moving along the y-axis only
        //it's y-rot is 90 and it's x-rot is 180. These will be standard values since (for now) we're not rotating the
        //camera.
        if (Mathf.Abs(p2.x - p1.x) > 0)
            yRot = 90;
        else if (Mathf.Abs(p2.y - p1.y) > 0)
        {
            xRot = 180;
            yRot = 90;
        }

        GameObject line =
            Instantiate(linePrefab, midPoint, Quaternion.Euler(xRot, yRot, 0));
        if (parenTransform == null)
            return;
        line.transform.parent = parenTransform;
    }
    
    public void GenerateEdgePoint(Vector3 pos, Transform parenTransform)
    {
        GameObject edgePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        edgePoint.AddComponent<TestPoint>();

        Renderer rend = edgePoint.GetComponent<Renderer>();
        rend.enabled = false;
        
        SphereCollider sphereCollider = edgePoint.AddComponent<SphereCollider>();
        sphereCollider.radius = .15f;
        
        edgePoint.transform.position = pos;
        edgePoint.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        
        if (parenTransform == null)
            return;
        edgePoint.transform.parent = parenTransform;
    }
}
