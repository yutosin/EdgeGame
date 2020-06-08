using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCube : MonoBehaviour
{
    public CubeMeshData data = new CubeMeshData();
    Dictionary<string, int[]> faces = new Dictionary<string, int[]>();
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        
    }

    private void Start()
    {
        CreateFacesForMoving();
        MakeCube();
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void MakeCube()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < 6; x++)
        {
            MakeFace(x);
        }
    }

    private void MakeFace(int dir)
    {
        vertices.AddRange(data.faceVertices(dir));
        int vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }


    //Create faces in dictionary
    private void CreateFacesForMoving()
    {
        faces.Add("XPlus", new int[4] { 1, 2, 4, 7 });
        faces.Add("XMinus", new int[4] { 0, 3, 5, 6 });
        faces.Add("YPlus", new int[4] { 0, 1, 4, 5 });
        faces.Add("YMinus", new int[4] { 2, 3, 6, 7 });
        faces.Add("ZPlus", new int[4] { 0, 1, 2, 3 });
        faces.Add("ZMinus", new int[4] { 4, 5, 6, 7 });

    }

    //A generalized version of the face move functions
    public void MoveFace(string faceToMove, float newPos)
    {
        if (!faces.ContainsKey(faceToMove))
        {
            Debug.LogError("Not a valid face");
        }

        else
        {
            Vector3 currentVPos;

            for (int i = 0; i < faces[faceToMove].Length; i++)
            {
                currentVPos = data.vertices[faces[faceToMove][i]];
                currentVPos.x = newPos;
                data.vertices[faces[faceToMove][i]] = currentVPos;
            }
            MakeCube();
            UpdateMesh();
        }

    }

}
