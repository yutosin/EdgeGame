using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineGenerator : MonoBehaviour
{
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
        
//        GameManager.SharedInstance.edgeManager.GenerateLine(p1, p2, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p2, p3, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p3, p4, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p5, p6, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p6, p7, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p7, p8, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p5, p8, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p1, p5, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p2, p6, gameObject.transform);
//        GameManager.SharedInstance.edgeManager.GenerateLine(p3, p7, gameObject.transform);
        
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p1, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p2, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p3, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p4, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p5, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p6, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p7, gameObject.transform);
        GameManager.SharedInstance.edgeManager.GenerateEdgePoint(p8, gameObject.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
