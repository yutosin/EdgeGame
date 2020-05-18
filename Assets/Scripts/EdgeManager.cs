using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeManager : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private MeshRenderer[] cubeRenderers;

    private Dictionary<string, (bool edgeActive, string[] faceIDs)> _edgeBase;
    private Dictionary<string, string[]> _faceBase;
    private List<TestPoint> _createdPoints; //mainly to keep track of visited points then deleted later

    private int nextPtID = 1;
    private int nextFaceID = 1;

    private void Start()
    {
        _edgeBase = new Dictionary<string, (bool edgeActive, string[] faceIDs)>();
        _faceBase = new Dictionary<string, string[]>();
        _createdPoints = new List<TestPoint>();

        foreach (var renderer in cubeRenderers)
        {
            SetUpFacesEdges(renderer);
        }

        _createdPoints = null;
    }

    public void GenerateLine(Vector3 p1, Vector3 p2, string pt1ID, string pt2ID)
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

        line.transform.parent = gameObject.transform;
        
        string edgeID = GenerateEdgeID(pt1ID, pt2ID);
        var edgeVals = (true, _edgeBase[edgeID].faceIDs);
        _edgeBase[edgeID] = edgeVals;
        
        CheckForFaces(_edgeBase[edgeID].faceIDs);
    }

    private string GenerateEdgeID(string pt1ID, string pt2ID)
    {
        int idCompare = string.Compare(pt1ID, pt2ID);
        string edgeID = (idCompare < 0) ? pt1ID + pt2ID : pt2ID + pt1ID;
        return edgeID;
    }
    
    public string GenerateEdgePoint(Vector3 pos, Transform parenTransform)
    {
        foreach (var testPoint in _createdPoints)
        {
            if (testPoint.gameObject.transform.position == pos)
                return testPoint.ptID;
        }
        
        GameObject edgePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        TestPoint tp = edgePoint.AddComponent<TestPoint>();
        _createdPoints.Add(tp);
        tp.ptID = "Pt" + nextPtID;
        nextPtID++;

        Renderer rend = edgePoint.GetComponent<Renderer>();
        //rend.material.shader = Shader.Find("Unlit/ColorZAlways");
        rend.enabled = false;
        
        SphereCollider sphereCollider = edgePoint.AddComponent<SphereCollider>();
        sphereCollider.radius = .15f;
        
        edgePoint.transform.position = pos;
        edgePoint.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        
        if (parenTransform == null)
            return tp.ptID;
        edgePoint.transform.parent = parenTransform;

        return tp.ptID;
    }

    private void SetUpFacesEdges(MeshRenderer renderer)
    {
        //TO DO: Handle overlapping points; it's throwing off the face detection
        Bounds meshBounds = renderer.bounds;
        Vector3 p1, p2, p3, p4, p5, p6, p7, p8;
        p1 = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        p2 = new Vector3(meshBounds.min.x , meshBounds.max.y, meshBounds.max.z);
        p3 = meshBounds.max;
        p4 = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        p5 = new Vector3(p1.x, p1.y - (meshBounds.extents.z * 2), p1.z);
        p6 = new Vector3(p2.x, p2.y - (meshBounds.extents.z * 2), p2.z);
        p7 = new Vector3(p3.x, p3.y - (meshBounds.extents.z * 2), p3.z);
        p8 = new Vector3(p4.x, p4.y - (meshBounds.extents.z * 2), p4.z);

        string p1ID = GenerateEdgePoint(p1, renderer.gameObject.transform);
        string p2ID = GenerateEdgePoint(p2, renderer.gameObject.transform);
        string p3ID = GenerateEdgePoint(p3, renderer.gameObject.transform);
        string p4ID = GenerateEdgePoint(p4, renderer.gameObject.transform);
        string p5ID = GenerateEdgePoint(p5, renderer.gameObject.transform);
        string p6ID = GenerateEdgePoint(p6, renderer.gameObject.transform);
        string p7ID = GenerateEdgePoint(p7, renderer.gameObject.transform);
        string p8ID = GenerateEdgePoint(p8, renderer.gameObject.transform);
        
        //Be aware: all of the edges and faces are created manually; some edges are shared by faces
        //Create face #1 (top) and edges
        string faceID = "face" + nextFaceID;
        //refactor edges later to add unique entries
        string edgeID1 = GenerateEdgeID(p1ID,p2ID);
        string edgeID2 = GenerateEdgeID(p2ID, p3ID);
        string edgeID3 = GenerateEdgeID(p3ID, p4ID);
        string edgeID4 = GenerateEdgeID(p1ID, p4ID);
        string edgeID5 = GenerateEdgeID(p5ID, p6ID);
        string edgeID6 = GenerateEdgeID(p6ID, p7ID);
        string edgeID7 = GenerateEdgeID(p7ID, p8ID);
        string edgeID8 = GenerateEdgeID(p5ID, p8ID);
        string edgeID9 = GenerateEdgeID(p1ID, p5ID);
        string edgeID10 = GenerateEdgeID(p2ID, p6ID);
        string edgeID11 = GenerateEdgeID(p3ID, p7ID);
        string edgeID12 = GenerateEdgeID(p4ID, p8ID);
        
        _edgeBase[edgeID1] = (false, new string[2]);
        _edgeBase[edgeID1].faceIDs[0] = faceID;
        
        _edgeBase[edgeID2] = (false, new string[2]);
        _edgeBase[edgeID2].faceIDs[0] = faceID;
        
        _edgeBase[edgeID3] = (false, new string[2]);
        _edgeBase[edgeID3].faceIDs[0] = faceID;
        
        _edgeBase[edgeID4] = (false, new string[2]);
        _edgeBase[edgeID4].faceIDs[0] = faceID;

        _faceBase[faceID] = new[] {edgeID1, edgeID2, edgeID3, edgeID4};

        nextFaceID++;
        
        //Create face #2 (top) and edges
        faceID = "face" + nextFaceID;
        
        _edgeBase[edgeID5] = (false, new string[2]);
        _edgeBase[edgeID5].faceIDs[0] = faceID;
        
        _edgeBase[edgeID6] = (false, new string[2]);
        _edgeBase[edgeID6].faceIDs[0] = faceID;
        
        _edgeBase[edgeID7] = (false, new string[2]);
        _edgeBase[edgeID7].faceIDs[0] = faceID;
        
        _edgeBase[edgeID8] = (false, new string[2]);
        _edgeBase[edgeID8].faceIDs[0] = faceID;

        _faceBase[faceID] = new[] {edgeID5, edgeID6, edgeID7, edgeID8};

        nextFaceID++;
        
        //Create face #3 (left) and edges
        faceID = "face" + nextFaceID;

        _edgeBase[edgeID4].faceIDs[1] = faceID;
        _edgeBase[edgeID8].faceIDs[1] = faceID;
        
        _edgeBase[edgeID9] = (false, new string[2]);
        _edgeBase[edgeID9].faceIDs[0] = faceID;
        
        _edgeBase[edgeID12] = (false, new string[2]);
        _edgeBase[edgeID12].faceIDs[0] = faceID;

        _faceBase[faceID] = new[] {edgeID9, edgeID8, edgeID12, edgeID4};

        nextFaceID++;
        
        //Create face #4 (right) and edges
        faceID = "face" + nextFaceID;

        _edgeBase[edgeID2].faceIDs[1] = faceID;
        _edgeBase[edgeID6].faceIDs[1] = faceID;
        
        _edgeBase[edgeID10] = (false, new string[2]);
        _edgeBase[edgeID10].faceIDs[0] = faceID;

        _edgeBase[edgeID11] = (false, new string[2]);
        _edgeBase[edgeID11].faceIDs[0] = faceID;

        _faceBase[faceID] = new[] {edgeID10, edgeID6, edgeID11, edgeID2};

        nextFaceID++;
        
        //Create face #5 (back) and edges
        faceID = "face" + nextFaceID;

        _edgeBase[edgeID1].faceIDs[1] = faceID;
        _edgeBase[edgeID5].faceIDs[1] = faceID;
        _edgeBase[edgeID9].faceIDs[1] = faceID;
        _edgeBase[edgeID10].faceIDs[1] = faceID;

        _faceBase[faceID] = new[] {edgeID1, edgeID10, edgeID5, edgeID9};

        nextFaceID++;
        
        //Create face #6 (front) and edges
        faceID = "face" + nextFaceID;

        _edgeBase[edgeID3].faceIDs[1] = faceID;
        _edgeBase[edgeID7].faceIDs[1] = faceID;
        _edgeBase[edgeID11].faceIDs[1] = faceID;
        _edgeBase[edgeID12].faceIDs[1] = faceID;

        _faceBase[faceID] = new[] {edgeID3, edgeID11, edgeID7, edgeID12};

        nextFaceID++;
    }

    private void CheckForFaces(string[] faceIDs)
    {
        for (int i = 0; i < faceIDs.Length; i++)
        {
            var edgesToCheck = _faceBase[faceIDs[i]];
            int activeEdges = 0;
            foreach (var edge in edgesToCheck)
            {
                if (_edgeBase[edge].edgeActive)
                    activeEdges++;
            }
            if (activeEdges >= 4)
                Debug.Log("valid face detected");
        }
    }
}
