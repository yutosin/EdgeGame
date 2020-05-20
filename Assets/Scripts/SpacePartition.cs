using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacePartition : MonoBehaviour
{
    private MeshRenderer _meshRenderer;

    private Dictionary<string, List<int>> _xAxisPartitions;
    private Dictionary<string, List<int>> _yAxisPartitions;
    private Dictionary<string, List<int>> _zAxisPartitions;

    private Dictionary<string, Graph> _xGraphs;
    private Dictionary<string, Graph> _yGraphs;
    private Dictionary<string, Graph> _zGraphs;
    
    void Start()
    {
        _xAxisPartitions = new Dictionary<string, List<int>>();
        _yAxisPartitions = new Dictionary<string, List<int>>();
        _zAxisPartitions = new Dictionary<string, List<int>>();
        
        _xGraphs = new Dictionary<string, Graph>();
        _yGraphs = new Dictionary<string, Graph>();
        _zGraphs = new Dictionary<string, Graph>();

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
        
        //TODO: Format strings to only have 2 decimal places
        for (int i = 0; i < points2.Length; i++)
        {
            Vector3 point = points2[i];
            if (_xAxisPartitions.TryGetValue(point.x.ToString(), out List<int> entry))
                entry.Add(i);
            else
            {
                _xAxisPartitions[point.x.ToString()] = new List<int>();
                _xAxisPartitions[point.x.ToString()].Add(i);
            }

            if (_yAxisPartitions.TryGetValue(point.y.ToString(), out List<int> entry2))
                entry2.Add(i);
            else
            {
                _yAxisPartitions[point.y.ToString()] = new List<int>();
                _yAxisPartitions[point.y.ToString()].Add(i);
            }
            
            if (_zAxisPartitions.TryGetValue(point.z.ToString(), out List<int> entry3))
                entry3.Add(i);
            else
            {
                _zAxisPartitions[point.z.ToString()] = new List<int>();
                _zAxisPartitions[point.z.ToString()].Add(i);
            }
        }

        foreach (var xAxisPartition in _xAxisPartitions)
        {
            List<int> partitionPoints = xAxisPartition.Value;
            partitionPoints.Sort();
            Graph xPartitionGraph = new Graph(xAxisPartition.Value.Count, partitionPoints);
            _xGraphs[xAxisPartition.Key] = xPartitionGraph;
            
        }
        
        foreach (var yAxisPartition in _yAxisPartitions)
        {
            List<int> partitionPoints = yAxisPartition.Value;
            partitionPoints.Sort();
            Graph yPartitionGraph = new Graph(yAxisPartition.Value.Count, partitionPoints);
            _yGraphs[yAxisPartition.Key] = yPartitionGraph;
        }
        
        foreach (var zAxisPartition in _zAxisPartitions)
        {
            List<int> partitionPoints = zAxisPartition.Value;
            partitionPoints.Sort();
            Graph zPartitionGraph = new Graph(zAxisPartition.Value.Count, partitionPoints);
            _zGraphs[zAxisPartition.Key] = zPartitionGraph;
        }
        
        //pts ordered to match zig-zag pattern from bottom left
        //Vector3[] points = new[] { p4, p3, p1, p2 };
        
        _yGraphs["1.5"].addEdge(0, 1);
        _yGraphs["1.5"].addEdge(1, 2);
        _yGraphs["1.5"].addEdge(2, 3);
        _yGraphs["1.5"].addEdge(0, 3);
        
        _zGraphs["1.5"].addEdge(0, 1);
        _zGraphs["1.5"].addEdge(1, 2);
        _zGraphs["1.5"].addEdge(2, 3);
        _zGraphs["1.5"].addEdge(0, 3);
        
        _xGraphs["1.5"].addEdge(0, 1);
        _xGraphs["1.5"].addEdge(1, 2);
        _xGraphs["1.5"].addEdge(2, 3);
        _xGraphs["1.5"].addEdge(0, 3);
        var test = _yGraphs["1.5"].connectedComponents();
        var test2 = _zGraphs["1.5"].connectedComponents();
        var test3 = _xGraphs["1.5"].connectedComponents();
        foreach (var faceVertices in test)
        {
            string testString = "";
            Vector3[] vertexVectors = new Vector3[4];
            foreach (var vertex in faceVertices)
            {
                testString += vertex + " ";
                vertexVectors[vertex] = points2[vertex];
            }
            // GenerateQuad(vertexVectors);
            GenerateQuadWithQuadMeshTop(vertexVectors);
            Debug.Log(testString);
        }

        foreach (var faceVertices in test2)
        {
            string testString = "";
            Vector3[] vertexVectors = new Vector3[4];
            for (int i = 0; i < faceVertices.Count; i++)
            {
                var vertex = faceVertices[i];
                testString += vertex + " ";
                vertexVectors[i] = points2[vertex];
            }
            //GenerateQuad(vertexVectors);
            GenerateQuadWithQuadMeshTop(vertexVectors, true);
            Debug.Log(testString);
        }
        
        foreach (var faceVertices in test3)
        {
            string testString = "";
            Vector3[] vertexVectors = new Vector3[4];
            for (int i = 0; i < faceVertices.Count; i++)
            {
                var vertex = faceVertices[i];
                testString += vertex + " ";
                vertexVectors[i] = points2[vertex];
            }
            //GenerateQuad(vertexVectors);
            GenerateQuadWithQuadMeshTop(vertexVectors, true);
            Debug.Log(testString);
        }
    }

    private void GenerateQuadWithQuadMeshTop(Vector3[] quadVertices, bool flipFirstPair = false)
    {
        GameObject newQuad = new GameObject();
        MeshRenderer meshRenderer = newQuad.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/ColorZAlways"));
        meshRenderer.sharedMaterial.color = Color.white;

        MeshFilter meshFilter = newQuad.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        if (flipFirstPair)
        {
            Vector3[] flipped = new[] {quadVertices[1], quadVertices[0], quadVertices[2], quadVertices[3]};
            mesh.vertices = flipped;
        }
        else
            mesh.vertices = quadVertices;

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

        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
