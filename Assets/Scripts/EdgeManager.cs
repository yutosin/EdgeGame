using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: refactor and refine this code
public class EdgeManager : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private MeshRenderer[] cubeRenderers;
    
    private Dictionary<string, Graph> _xGraphs;
    private Dictionary<string, Graph> _yGraphs;
    private Dictionary<string, Graph> _zGraphs;
    
    private List<Vector3> _points;

    private int nextPtID = 0;

    private void Start()
    {
        _points = new List<Vector3>(cubeRenderers.Length * 8);

        foreach (var renderer in cubeRenderers)
        {
            AddCubeVerticesToList(renderer);
        }
        
        Dictionary<string, List<int>> xAxisPartitions = new Dictionary<string, List<int>>();
        Dictionary<string, List<int>> yAxisPartitions = new Dictionary<string, List<int>>();
        Dictionary<string, List<int>> zAxisPartitions = new Dictionary<string, List<int>>();
        
        for (int i = 0; i < _points.Count; i++)
        {
            Vector3 point = _points[i];
            string xPartitionKey = point.x.ToString();
            string yPartitionKey = point.y.ToString();
            string zPartitionKey = point.z.ToString();
            if (xAxisPartitions.TryGetValue(xPartitionKey, out List<int> entry))
                entry.Add(i);
            else
            {
                xAxisPartitions[xPartitionKey] = new List<int>();
                xAxisPartitions[xPartitionKey].Add(i);
            }

            if (yAxisPartitions.TryGetValue(yPartitionKey, out List<int> entry2))
                entry2.Add(i);
            else
            {
                yAxisPartitions[yPartitionKey] = new List<int>();
                yAxisPartitions[yPartitionKey].Add(i);
            }
            
            if (zAxisPartitions.TryGetValue(zPartitionKey, out List<int> entry3))
                entry3.Add(i);
            else
            {
                zAxisPartitions[zPartitionKey] = new List<int>();
                zAxisPartitions[zPartitionKey].Add(i);
            }
        }
        
        _xGraphs = new Dictionary<string, Graph>(xAxisPartitions.Count);
        _yGraphs = new Dictionary<string, Graph>(yAxisPartitions.Count);
        _zGraphs = new Dictionary<string, Graph>(zAxisPartitions.Count);
        
        foreach (var xAxisPartition in xAxisPartitions)
        {
            List<int> partitionPoints = xAxisPartition.Value;
            partitionPoints.Sort();
            Graph xPartitionGraph = new Graph(xAxisPartition.Value.Count, partitionPoints);
            _xGraphs[xAxisPartition.Key] = xPartitionGraph;
            
        }
        
        foreach (var yAxisPartition in yAxisPartitions)
        {
            List<int> partitionPoints = yAxisPartition.Value;
            partitionPoints.Sort();
            Graph yPartitionGraph = new Graph(yAxisPartition.Value.Count, partitionPoints);
            _yGraphs[yAxisPartition.Key] = yPartitionGraph;
        }
        
        foreach (var zAxisPartition in zAxisPartitions)
        {
            List<int> partitionPoints = zAxisPartition.Value;
            partitionPoints.Sort();
            Graph zPartitionGraph = new Graph(zAxisPartition.Value.Count, partitionPoints);
            _zGraphs[zAxisPartition.Key] = zPartitionGraph;
        }
    }

    private void AddCubeVerticesToList(MeshRenderer renderer)
    {
        Bounds meshBounds = renderer.bounds;
        Vector3[] cubeVertices = new Vector3[8];
        cubeVertices[0] = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        cubeVertices[1] = new Vector3(meshBounds.min.x , meshBounds.max.y, meshBounds.max.z);
        cubeVertices[2] = meshBounds.max;
        cubeVertices[3] = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        cubeVertices[4] = new Vector3(cubeVertices[0].x, cubeVertices[0].y - (meshBounds.extents.z * 2), cubeVertices[0].z);
        cubeVertices[5] = new Vector3(cubeVertices[1].x, cubeVertices[0].y - (meshBounds.extents.z * 2), cubeVertices[1].z);
        cubeVertices[6] = new Vector3(cubeVertices[2].x, cubeVertices[0].y - (meshBounds.extents.z * 2), cubeVertices[2].z);
        cubeVertices[7] = new Vector3(cubeVertices[3].x, cubeVertices[0].y - (meshBounds.extents.z * 2), cubeVertices[3].z);

        foreach (Vector3 vertex in cubeVertices)
        {
            if(GenerateEdgePoint(vertex, renderer.gameObject.transform))
                _points.Add(vertex);
        }
    }
    
    public bool GenerateEdgePoint(Vector3 pos, Transform parenTransform)
    {
        foreach (var testPoint in _points)
        {
            if (testPoint == pos)
                return false;
        }
        
        GameObject edgePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        TestPoint tp = edgePoint.AddComponent<TestPoint>();
        tp.ptID = "Pt" + nextPtID;
        tp.listLoc = nextPtID;
        nextPtID++;

        Renderer rend = edgePoint.GetComponent<Renderer>();
        //rend.material.shader = Shader.Find("Unlit/ColorZAlways");
        rend.enabled = false;
        
        SphereCollider sphereCollider = edgePoint.AddComponent<SphereCollider>();
        sphereCollider.radius = .15f;
        
        edgePoint.transform.position = pos;
        edgePoint.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        
        if (parenTransform == null)
            return true;
        edgePoint.transform.parent = parenTransform;

        return true;
    }

    /*TODO: Longer edges that overlap multiple points should also create smaller segments e.g. pt1->pt3 would also
    create pt1->pt2 and pt->pt3*/
    public void GenerateEdge(Vector3 p1, Vector3 p2, int pt1ID, int pt2ID)
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
        float xDistance = Mathf.Abs(p2.x - p1.x);
        float yDistance = Mathf.Abs(p2.y - p1.y);
        float zDistance = Mathf.Abs(p2.z - p1.z);
        float scaleAmount;
        bool xAxisEdge = false, yAxisEdge = false, zAxisEdge = false;
        if (xDistance > 0)
        {
            yRot = 90;
            scaleAmount = xDistance;
            xAxisEdge = true;
        }
        else if (yDistance > 0)
        {
            xRot = 180;
            yRot = 90;
            scaleAmount = yDistance;
            yAxisEdge = true;
        }
        else
        {
            scaleAmount = zDistance;
            zAxisEdge = true;
        }

        GameObject line =
            Instantiate(linePrefab, midPoint, Quaternion.Euler(xRot, yRot, 0));
        Transform lineTransform = line.transform;

        lineTransform.parent = gameObject.transform;
        var localScale = lineTransform.localScale;
        lineTransform.localScale = new Vector3(localScale.x, 
            localScale.y * scaleAmount, 
            localScale.z);

        string xGraphKey = p1.x.ToString();
        string yGraphKey = p1.y.ToString();
        string zGraphKey = p1.z.ToString();
        
        if (xAxisEdge)
        {
            _yGraphs[yGraphKey].addEdge(pt1ID, pt2ID);
            _zGraphs[zGraphKey].addEdge(pt1ID, pt2ID);

            var yConnectedComponents = _yGraphs[yGraphKey].connectedComponents();
            var zConnectedComponents = _zGraphs[zGraphKey].connectedComponents();

            foreach (var faceVertices in yConnectedComponents)
            {
                //Vector3[] vertexVectors = new Vector3[4];
                List<Vector3> vertexVectors = new List<Vector3>(4);
                foreach (var vertex in faceVertices)
                {
                    vertexVectors.Add(_points[vertex]);
                }
                GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());
            }
            
            foreach (var faceVertices in zConnectedComponents)
            {
                //Vector3[] vertexVectors = new Vector3[4];
                List<Vector3> vertexVectors = new List<Vector3>(4);
                foreach (var vertex in faceVertices)
                {
                    vertexVectors.Add(_points[vertex]);
                }
                GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());
            }
        }
        else if (yAxisEdge)
        {
            _xGraphs[xGraphKey].addEdge(pt1ID, pt2ID);
            _zGraphs[zGraphKey].addEdge(pt1ID, pt2ID);
            
            var xConnectedComponents = _xGraphs[xGraphKey].connectedComponents();
            var zConnectedComponents = _zGraphs[zGraphKey].connectedComponents();
            
            foreach (var faceVertices in xConnectedComponents)
            {
                //Vector3[] vertexVectors = new Vector3[4];
                List<Vector3> vertexVectors = new List<Vector3>(4);
                foreach (var vertex in faceVertices)
                {
                    vertexVectors.Add(_points[vertex]);
                }
                GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());
            }
            
            foreach (var faceVertices in zConnectedComponents)
            {
                //Vector3[] vertexVectors = new Vector3[4];
                List<Vector3> vertexVectors = new List<Vector3>(4);
                foreach (var vertex in faceVertices)
                {
                    vertexVectors.Add(_points[vertex]);
                }
                GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());
            }
        }
        else if (zAxisEdge)
        {
            _xGraphs[xGraphKey].addEdge(pt1ID, pt2ID);
            _yGraphs[yGraphKey].addEdge(pt1ID, pt2ID);
            
            var xConnectedComponents = _xGraphs[xGraphKey].connectedComponents();
            var yConnectedComponents = _yGraphs[yGraphKey].connectedComponents();
            
            foreach (var faceVertices in xConnectedComponents)
            {
                //Vector3[] vertexVectors = new Vector3[4];
                List<Vector3> vertexVectors = new List<Vector3>(4);
                foreach (var vertex in faceVertices)
                {
                    vertexVectors.Add(_points[vertex]);
                }
                GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());
            }
            
            foreach (var faceVertices in yConnectedComponents)
            {
                //Vector3[] vertexVectors = new Vector3[4];
                List<Vector3> vertexVectors = new List<Vector3>(4);
                foreach (var vertex in faceVertices)
                {
                    vertexVectors.Add(_points[vertex]);
                }
                GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());
            }
        }
    }
    
    private void GenerateQuadWithQuadMeshTop(Vector3[] quadVertices, bool flipFirstPair = true)
    {
        GameObject newQuad = new GameObject();
        gameObject.transform.parent = gameObject.transform;
        MeshRenderer meshRenderer = newQuad.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/ColorZAlways"));
        meshRenderer.sharedMaterial.color = Color.gray;

        MeshFilter meshFilter = newQuad.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        if (flipFirstPair)
        {
            //Vector3[] flipped = new[] {quadVertices[1], quadVertices[0], quadVertices[2], quadVertices[3]};
            //TODO: figure out why this order works!! you flipped the first two then reversed the whole array..why?
            Vector3[] flipped = new[] {quadVertices[3], quadVertices[2], quadVertices[0], quadVertices[1]};
            mesh.vertices = flipped;
        }
        else
            mesh.vertices = quadVertices;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 0)
        };
        int[] indices = new int[]
        {
            0, 1, 2,3
        };
        mesh.uv = uv;
        mesh.SetIndices(indices, MeshTopology.Quads,0);
        mesh.RecalculateNormals();
        
        GameObject anchorPoint = new GameObject();
        anchorPoint.transform.position = mesh.bounds.center;
        newQuad.transform.parent = anchorPoint.transform;
        
        Vector3[] normals = mesh.normals;
        //Vector3[] newNormals = new Vector3[4];
        if (normals[0] == Vector3.back)
            anchorPoint.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (normals[0] == Vector3.down)
            anchorPoint.transform.rotation = Quaternion.Euler(0, 0, 180);
        else if (normals[0] == Vector3.left)
            anchorPoint.transform.rotation = Quaternion.Euler(0, 180, 0);

        //mesh.normals = newNormals;
        meshFilter.mesh = mesh;
    }
    private string GenerateEdgeID(string pt1ID, string pt2ID)
    {
        int idCompare = string.Compare(pt1ID, pt2ID);
        string edgeID = (idCompare < 0) ? pt1ID + pt2ID : pt2ID + pt1ID;
        return edgeID;
    }
}
