using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineGenerator : MonoBehaviour
{
    public GameObject linePrefab;
    private MeshRenderer _meshRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        Bounds meshBounds = _meshRenderer.bounds;
        Vector3 p1, p2, p3, p4, p5, p6, p7, p8;
        p1 = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        p2 = new Vector3(meshBounds.min.x , meshBounds.max.y, meshBounds.max.z);
        p3 = meshBounds.max;
        p4 = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        p5 = new Vector3(p1.x, p1.y - (meshBounds.extents.z * 2), p1.z);
        p6 = new Vector3(p2.x, p2.y - (meshBounds.extents.z * 2), p2.z);
        p7 = new Vector3(p3.x, p3.y - (meshBounds.extents.z * 2), p3.z);
        p8 = new Vector3(p4.x, p4.y - (meshBounds.extents.z * 2), p4.z);
        
        GenerateLine(p1, p2);
        GenerateLine(p2, p3);
        GenerateLine(p3, p4);
        GenerateLine(p5, p6);
        GenerateLine(p6, p7);
        GenerateLine(p7, p8);
        GenerateLine(p5, p8);
        GenerateLine(p1, p5);
        GenerateLine(p2, p6);
        GenerateLine(p3, p7);
    }
    
    void GenerateLine(Vector3 p1, Vector3 p2)
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
        else if (Math.Abs(p2.y - p1.y) > 0)
        {
            xRot = 180;
            yRot = 90;
        }

        GameObject line =
            Instantiate(linePrefab, midPoint, Quaternion.Euler(xRot, yRot, 0));
        line.transform.parent = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
