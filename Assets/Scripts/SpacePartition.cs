using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacePartition : MonoBehaviour
{
    private MeshRenderer _meshRenderer;

    private Dictionary<int, List<int>> _xAxisPartitions;
    private Dictionary<int, List<int>> _yAxisPartitions;
    private Dictionary<int, List<int>> _zAxisPartitions;

    private Dictionary<int, Graph> _xGraphs;
    private Dictionary<int, Graph> _yGraphs;
    private Dictionary<int, Graph> _zGraphs;
    
    void Start()
    {
        //create top face graph
        //Graph faceGraph = new Graph(4);
        
        _xAxisPartitions = new Dictionary<int, List<int>>();
        _yAxisPartitions = new Dictionary<int, List<int>>();
        _zAxisPartitions = new Dictionary<int, List<int>>();
        
        _xGraphs = new Dictionary<int, Graph>();
        _yGraphs = new Dictionary<int, Graph>();
        _zGraphs = new Dictionary<int, Graph>();

        Vector3 p1, p2, p3, p4, p5, p6, p7, p8;
        _meshRenderer = GetComponent<MeshRenderer>();
        Bounds meshBounds = _meshRenderer.bounds;
        p1 = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        p2 = new Vector3(meshBounds.min.x , meshBounds.max.y, meshBounds.max.z);
        p3 = meshBounds.max;
        p4 = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z - (meshBounds.extents.z * 2));
        p5 = new Vector3(p1.x, p1.y - (meshBounds.extents.z * 2), p1.z);
        p6 = new Vector3(p2.x, p2.y - (meshBounds.extents.z * 2), p2.z);
        p7 = new Vector3(p3.x, p3.y - (meshBounds.extents.z * 2), p3.z);
        p8 = new Vector3(p4.x, p4.y - (meshBounds.extents.z * 2), p4.z);

        Vector3[] points = new[] { p1, p2, p3, p4 };
        Vector3[] points2 = new[] { p1, p2, p3, p4, p5, p6, p7, p8 };
        
        for (int i = 0; i < points2.Length; i++)
        {
            Vector3 point = points2[i];
            if (_xAxisPartitions.TryGetValue((int)point.x, out List<int> entry))
                entry.Add(i);
            else
            {
                _xAxisPartitions[(int)point.x] = new List<int>();
                _xAxisPartitions[(int)point.x].Add(i);
            }

            if (_yAxisPartitions.TryGetValue((int)point.y, out List<int> entry2))
                entry2.Add(i);
            else
            {
                _yAxisPartitions[(int)point.y] = new List<int>();
                _yAxisPartitions[(int)point.y].Add(i);
            }
            
            if (_zAxisPartitions.TryGetValue((int)point.z, out List<int> entry3))
                entry3.Add(i);
            else
            {
                _zAxisPartitions[(int)point.z] = new List<int>();
                _zAxisPartitions[(int)point.z].Add(i);
            }
        }

        foreach (var xAxisPartition in _xAxisPartitions)
        {
            List<int> partitionPoints = xAxisPartition.Value;
            partitionPoints.Sort();
            Graph xPartitionGraph = new Graph(xAxisPartition.Value.Count, partitionPoints[0]);
            _xGraphs[xAxisPartition.Key] = xPartitionGraph;
            
        }
        
        foreach (var yAxisPartition in _yAxisPartitions)
        {
            List<int> partitionPoints = yAxisPartition.Value;
            partitionPoints.Sort();
            Graph yPartitionGraph = new Graph(yAxisPartition.Value.Count, partitionPoints[0]);
            _yGraphs[yAxisPartition.Key] = yPartitionGraph;
        }
        
        foreach (var zAxisPartition in _zAxisPartitions)
        {
            List<int> partitionPoints = zAxisPartition.Value;
            partitionPoints.Sort();
            Graph zPartitionGraph = new Graph(zAxisPartition.Value.Count, partitionPoints[0]);
            _zGraphs[zAxisPartition.Key] = zPartitionGraph;
        }
        
        //pts ordered to match zig-zag pattern from bottom left
        //Vector3[] points = new[] { p4, p3, p1, p2 };
        
        _yGraphs[1].addEdge(0, 1);
        _yGraphs[1].addEdge(1, 2);
        _yGraphs[1].addEdge(2, 3);
        _yGraphs[1].addEdge(0, 3);
        var test = _yGraphs[1].connectedComponents();
        foreach (var faceVertices in test)
        {
            string testString = "";
            Vector3[] vertexVectors = new Vector3[4];
            foreach (var vertex in faceVertices)
            {
                testString += vertex + " ";
                vertexVectors[vertex] = points[vertex];
            }
            GenerateQuad(vertexVectors);
            Debug.Log(testString);
        }
    }

    private void GenerateQuad(Vector3[] quadVertices)
    {
        float width = 1;
        float height = 1;
        
        GameObject newQuad = new GameObject();
        MeshRenderer meshRenderer = newQuad.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/ColorZAlways"));
        meshRenderer.sharedMaterial.color = Color.white;

        MeshFilter meshFilter = newQuad.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0)
        };
        mesh.vertices = quadVertices;
        //mesh.vertices = vertices;
        
        //tris based on bottom edge l->r (0, 1) and top edge l->r (2, 3)
        // int[] tris = new int[6]
        // {
        //     // lower left triangle
        //     0, 2, 1,
        //     // upper right triangle
        //     2, 3, 1
        // };
        
        //tris based on clockwork face vertices top left to bottom left
        int[] tris = new int[6]
        {
            // lower left triangle
            3, 0, 2,
            // upper right triangle
            0, 1, 2
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;

        //newQuad.transform.rotation = Quaternion.Euler(0, 180, 0);
        //newQuad.transform.parent = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
