using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject verticesHolder;
    [SerializeField] private GameObject facesHolder;
    [SerializeField] private GameObject combinedLevel;
    [SerializeField] private MeshRenderer[] cubeRenderers;
    [SerializeField] private bool combineCubes;
    
    private Dictionary<string, Graph> _xGraphs;
    private Dictionary<string, Graph> _yGraphs;
    private Dictionary<string, Graph> _zGraphs;

    public NavMeshSurface _meshSurface;

    public Material FloorMat;  //floor material creation
    public Material WallMat1;

    private bool _navMeshBuilt = false;

    private List<GameObject> _cubeObjects;
    
    private List<Vector3> _points;

    private int _nextPtID;
    private int _nextFaceId;

    [SerializeField]private Jsonator _levelLoader;

    // [SerializeField]private int[] _initTPLocs;
    // [SerializeField]private Dictionary<int, EdgeVertex> _initTPs;

    private void Start()
    {
        _points = new List<Vector3>(cubeRenderers.Length * 8);
        // _cubeObjects = new List<GameObject>(cubeRenderers.Length);
        _meshSurface = facesHolder.GetComponent<NavMeshSurface>();
        // _initTPLocs = new[] {69, 67, 8, 54, 68, 65};
        // _initTPs = new Dictionary<int, EdgeVertex>();

        // foreach (var renderer in cubeRenderers)
        // {
        //     //AddCubeVerticesToList(renderer);
        //     _cubeObjects.Add(renderer.gameObject);
        // }

        Grid loadedLevel = _levelLoader.LoadLevel("castle");
        if (loadedLevel.vertices == null)
            return;
        foreach (var vertex in loadedLevel.vertices)
        {
            GenerateSelectableVertex(vertex);
        }
        
        CreatePartitionsAndGraphRepresentations();

        GameManager.SharedInstance.playerAgent.transform.position = loadedLevel.startPoint;

        // if (combineCubes)
        //     CombineCubesInLevel();

        // GenerateEdge(_initTPs[68], _initTPs[54]);
        // GenerateEdge(_initTPs[54], _initTPs[8]);
        // GenerateEdge(_initTPs[8], _initTPs[65]);
        // GenerateEdge(_initTPs[65], _initTPs[67]);
        // GenerateEdge(_initTPs[67], _initTPs[69]);
        // GenerateEdge(_initTPs[68], _initTPs[65]);
        // GenerateEdge(_initTPs[69], _initTPs[68]);

    }

    // private void AddCubeVerticesToList(MeshRenderer renderer)
    // {
    //     Bounds meshBounds = renderer.bounds;
    //     Vector3[] cubeVertices = new Vector3[8];
    //     cubeVertices[0] = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
    //     cubeVertices[1] = new Vector3(meshBounds.min.x , meshBounds.max.y, meshBounds.max.z);
    //     cubeVertices[2] = meshBounds.max;
    //     cubeVertices[3] = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
    //     cubeVertices[4] = new Vector3(cubeVertices[0].x, cubeVertices[0].y - (meshBounds.extents.y * 2), cubeVertices[0].z);
    //     cubeVertices[5] = new Vector3(cubeVertices[1].x, cubeVertices[0].y - (meshBounds.extents.y * 2), cubeVertices[1].z);
    //     cubeVertices[6] = new Vector3(cubeVertices[2].x, cubeVertices[0].y - (meshBounds.extents.y * 2), cubeVertices[2].z);
    //     cubeVertices[7] = new Vector3(cubeVertices[3].x, cubeVertices[0].y - (meshBounds.extents.y * 2), cubeVertices[3].z);
    //
    //     foreach (Vector3 vertex in cubeVertices)
    //     {
    //         if(GenerateSelectableVertex(vertex))
    //             _points.Add(vertex);
    //     }
    // }

    private void CreatePartitionsAndGraphRepresentations()
    {
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
        
        ConvertPartitionsToGraphs(xAxisPartitions, _xGraphs);
        ConvertPartitionsToGraphs(yAxisPartitions, _yGraphs);
        ConvertPartitionsToGraphs(zAxisPartitions, _zGraphs);
    }

    private void ConvertPartitionsToGraphs(Dictionary<string, List<int>> partitions, Dictionary<string, Graph> graphs)
    {
        foreach (var partition in partitions)
        {
            List<int> partitionPoints = partition.Value;
            partitionPoints.Sort();
            Graph zPartitionGraph = new Graph(partition.Value.Count, partitionPoints);
            graphs[partition.Key] = zPartitionGraph;
        }
    }
    
    private void GenerateSelectableVertex(Vector3 pos)
    {
        GameObject edgePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(edgePoint.GetComponent<SphereCollider>());
        EdgeVertex tp = edgePoint.AddComponent<EdgeVertex>();
        tp.ptID = "Pt" + _nextPtID;
        edgePoint.name = tp.ptID;
        tp.listLoc = _nextPtID;
        
        // if (Array.Exists(_initTPLocs, i => _nextPtID == i ))
        //     _initTPs.Add(_nextPtID, tp);
        
        _nextPtID++;

        Renderer rend = edgePoint.GetComponent<Renderer>();
        rend.material.shader = Shader.Find("Unlit/ColorZAlways");
        rend.material.renderQueue = 2350;
        rend.enabled = false;
        
        SphereCollider sphereCollider = edgePoint.AddComponent<SphereCollider>();
        sphereCollider.radius = .75f;
        
        edgePoint.transform.position = pos;
        edgePoint.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        
        edgePoint.transform.parent = verticesHolder.transform;

        _points.Add(pos);
    }
    
    //TODO: properly use the addEdge function to make use of the bool and avoid creating edge game object
    public void GenerateEdge(EdgeVertex tp1, EdgeVertex tp2)
    {
        Vector3 p1 = tp1.transform.position;
        Vector3 p2 = tp2.transform.position;
        int pt1ID = tp1.listLoc;
        int pt2ID = tp2.listLoc;

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
        Vector3 distance = p2 - p1;
        float scaleAmount;
        bool xAxisEdge = false, yAxisEdge = false, zAxisEdge = false;
        if (Mathf.Abs(distance.x) > 0)
        {
            yRot = 90;
            scaleAmount = Mathf.Abs(distance.x);
            xAxisEdge = true;
        }
        else if (Mathf.Abs(distance.y) > 0)
        {
            xRot = 180;
            yRot = 90;
            scaleAmount = Mathf.Abs(distance.y);
            yAxisEdge = true;
        }
        else
        {
            scaleAmount = Mathf.Abs(distance.z);
            zAxisEdge = true;
        }

        string xGraphKey = p1.x.ToString();
        string yGraphKey = p1.y.ToString();
        string zGraphKey = p1.z.ToString();
        
        if (xAxisEdge)
        {
            EdgeUtil(_yGraphs[yGraphKey], tp1, tp2, scaleAmount);
            EdgeUtil(_zGraphs[zGraphKey], tp1, tp2, scaleAmount);
            
            FaceGenUtil(_yGraphs[yGraphKey]);
            FaceGenUtil(_zGraphs[zGraphKey]);
        }
        else if (yAxisEdge)
        {
            EdgeUtil(_xGraphs[xGraphKey], tp1, tp2, scaleAmount);
            EdgeUtil(_zGraphs[zGraphKey], tp1, tp2, scaleAmount);
            
            FaceGenUtil(_xGraphs[xGraphKey]);
            FaceGenUtil(_zGraphs[zGraphKey]);
        }
        else if (zAxisEdge)
        {
            EdgeUtil(_xGraphs[xGraphKey], tp1, tp2, scaleAmount);
            EdgeUtil(_yGraphs[yGraphKey], tp1, tp2, scaleAmount);
            
            FaceGenUtil(_xGraphs[xGraphKey]);
            FaceGenUtil(_yGraphs[yGraphKey]);
        }
        
        /*TODO: optimization idea; check overlap sphere for midpoint, if edge already present (e.g. larger overlapping edge)
        or just the same edge, don't instantiate*/
        GameObject line =
            Instantiate(linePrefab, midPoint, Quaternion.Euler(xRot, yRot, 0));
        Transform lineTransform = line.transform;

        lineTransform.parent = gameObject.transform;
        var localScale = lineTransform.localScale;
        lineTransform.localScale = new Vector3(localScale.x, 
            localScale.y * scaleAmount, 
            localScale.z);
    }

    private void EdgeUtil(Graph graph, EdgeVertex tp1, EdgeVertex tp2, float scaleAmount)
    {
        var edgeSubVerts = FindEdgeSubVertices(tp1, tp2, scaleAmount);
        
        //FormEdgesFromSubVerts
        for (int i = 0; i < edgeSubVerts.Count - 1; i++)
        {
            graph.addEdge(edgeSubVerts[i], edgeSubVerts[i + 1]);
        }
        
        //TODO: figure out early exit for no edge being drawn
    }

    private List<int> FindEdgeSubVertices(EdgeVertex p1, EdgeVertex p2, float scaleAmount)
    {
        List<int> subEdgeVertices = new List<int>();
        subEdgeVertices.Add(p1.listLoc);
        Vector3 pt1 = p1.transform.position;
        Vector3 pt2 = p2.transform.position;
        Vector3 distance = (pt2 - pt1).normalized;

        for (int i = 1; i < scaleAmount; i++)
        {
            Vector3 overlapPos = new Vector3(pt1.x + (1 * distance.x * i), 
                pt1.y + (1 * distance.y * i), 
                pt1.z + (1 * distance.z * i));

            Collider[] hitCollider = Physics.OverlapSphere(overlapPos, .25f);
            foreach (Collider collider in hitCollider)
            {
                EdgeVertex tp = collider.gameObject.GetComponent<EdgeVertex>();
                if (tp)
                {
                    subEdgeVertices.Add(tp.listLoc);
                    break;
                }
            }
        }
        
        subEdgeVertices.Add(p2.listLoc);
        return subEdgeVertices;
    }

    private void FaceGenUtil(Graph graph)
    {
        var graphFaces = graph.FindFaces();

        foreach (var faceVertices in graphFaces)
        {
            faceVertices.Add(faceVertices[0]);
            List<Vector3> vertexVectors = new List<Vector3>(4);
            List<int> vertexIds = new List<int>(4);
            Vector3 startHeading = _points[faceVertices[1]] - _points[faceVertices[0]];
            float startHeadingMagnitude = startHeading.magnitude;
            Vector3 direction = startHeading / startHeadingMagnitude;
            for (int i = 0; i < faceVertices.Count - 1; i++)
            {
                Vector3 heading = _points[faceVertices[i + 1]] - _points[faceVertices[i]];
                float headingMagnitude = heading.magnitude;
                Vector3 tempDirection = heading / headingMagnitude;

                if (tempDirection != direction)
                {
                    vertexIds.Add(faceVertices[i]);
                    direction = tempDirection;
                }
            }
            faceVertices.RemoveAt(faceVertices.Count - 1);
            if (vertexIds.Count == 3)
                vertexIds.Insert(0, faceVertices[0]);
            else if (vertexIds.Count > 4)
                continue;

            foreach (var vertexId in vertexIds)
            {
                vertexVectors.Add(_points[vertexId]);
            }
            
            SortVerticesForQuad(ref vertexVectors);
            
            GenerateQuadWithQuadMeshTop(vertexVectors.ToArray());

            if (!_navMeshBuilt)
            {
                _meshSurface.BuildNavMesh();
                _navMeshBuilt = true;
            }
            else
                _meshSurface.UpdateNavMesh(_meshSurface.navMeshData);
        }
    }
    
    private void SortVerticesForQuad(ref List<Vector3> vertices)
    {
        float centroidX = (vertices[0].x + vertices[1].x + vertices[2].x + vertices[3].x) / 4;
        float centroidY = (vertices[0].y + vertices[1].y + vertices[2].y + vertices[3].y) / 4;
        float centroidZ = (vertices[0].z + vertices[1].z + vertices[2].z + vertices[3].z) / 4;
        Vector3 centroid = new Vector3(centroidX, centroidY, centroidZ);
        
        Vector3 axis = Vector3.zero;
        
        if (centroidX == vertices[0].x)
            axis = Vector3.right;
        else if (centroidY == vertices[0].y)
            axis = Vector3.up;
        else
            axis = Vector3.forward;
        
        float[] angles = new float[4];

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 offset = vertices[i] - centroid;
            float angle = Vector3.SignedAngle(centroid, offset, axis);
            angles[i] = angle;
        }
        
        for (int i = 0; i < angles.Length - 1; ++i)
        {
            for (int j = 0; j < angles.Length - i - 1; ++j)
            {
                if (angles[j] < angles[j + 1])
                {
                    Vector3 temp = vertices[j];
                    vertices[j] = vertices[j + 1];
                    vertices[j + 1] = temp;
                    
                    float tempAngle = angles[j];
                    angles[j] = angles[j + 1];
                    angles[j + 1] = tempAngle;
                }
            }
        }
    }
    
    private void GenerateQuadWithQuadMeshTop(Vector3[] quadVertices)
    {
        GameObject newQuad = new GameObject();
        Face face = newQuad.AddComponent<Face>();
        face.Vertices = quadVertices;
        face.FaceId = _nextFaceId;
        newQuad.name = "Face " + _nextFaceId;
        _nextFaceId++;
        
        gameObject.transform.parent = gameObject.transform;
        MeshRenderer meshRenderer = newQuad.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = WallMat1;
        // meshRenderer.sharedMaterial.color = Color.gray;

        MeshFilter meshFilter = newQuad.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        
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
        anchorPoint.transform.parent = facesHolder.transform;
        
        Vector3[] normals = mesh.normals;
        if (normals[0] == Vector3.back)
            anchorPoint.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (normals[0] == Vector3.down)
            anchorPoint.transform.rotation = Quaternion.Euler(0, 0, 180);
        else if (normals[0] == Vector3.left)
            anchorPoint.transform.rotation = Quaternion.Euler(0, 180, 0);
        
        meshFilter.mesh = mesh;

        //Might not even need colliders on these...but if we do probably should just use box collider
        BoxCollider collider = newQuad.AddComponent<BoxCollider>();
    }

    // private void CombineCubesInLevel()
    // {
    //     GameObject cubeHolder = new GameObject();
    //     cubeHolder.name = "LevelCombinedMesh";
    //     cubeHolder.transform.position = Vector3.zero;
    //     MeshFilter levelMeshFileter = cubeHolder.transform.gameObject.AddComponent<MeshFilter>();
    //     MeshRenderer meshRenderer = cubeHolder.AddComponent<MeshRenderer>();
    //     meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Color"));
    //     meshRenderer.sharedMaterial.color = Color.black;
    //     
    //     List<CombineInstance> combines = new List<CombineInstance>(_cubeObjects.Count);
    //     
    //     foreach (var cube in _cubeObjects)
    //     {
    //         MeshFilter[] meshFilters = cube.GetComponents<MeshFilter>();
    //
    //         int i = 0;
    //         while (i < meshFilters.Length)
    //         {
    //             CombineInstance cubeCombine = new CombineInstance();
    //             cubeCombine.mesh = meshFilters[i].sharedMesh;
    //             cubeCombine.transform = meshFilters[i].transform.localToWorldMatrix;
    //             Destroy(meshFilters[i].gameObject);
    //             combines.Add(cubeCombine);
    //             i++;
    //         }
    //     }
    //     
    //     levelMeshFileter.mesh = new Mesh();
    //     levelMeshFileter.mesh.CombineMeshes(combines.ToArray(), true,true);
    //     levelMeshFileter.mesh.RecalculateBounds();
    //     levelMeshFileter.mesh.RecalculateNormals();
    //     levelMeshFileter.mesh.Optimize();
    //     cubeHolder.SetActive(true);
    //
    //     MeshCollider cubeColl = cubeHolder.AddComponent<MeshCollider>();
    //     cubeColl.sharedMesh = levelMeshFileter.mesh;
    //
    //     // NavMeshSurface surface = cubeHolder.AddComponent<NavMeshSurface>();
    //     // surface.BuildNavMesh();
    //     //
    //     // combinedLevel = cubeHolder;
    //     // cubeColl.convex = true;
    //     // cubeColl.isTrigger = true;
    // }
    // private string GenerateEdgeID(string pt1ID, string pt2ID)
    // {
    //     int idCompare = string.Compare(pt1ID, pt2ID);
    //     string edgeID = (idCompare < 0) ? pt1ID + pt2ID : pt2ID + pt1ID;
    //     return edgeID;
    // }
}
