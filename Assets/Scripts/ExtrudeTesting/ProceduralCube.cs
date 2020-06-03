using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCube : MonoBehaviour
{
    public CubeMeshData data = new CubeMeshData();
    Dictionary<string, int[]> faces;
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

    //These are to be shaved off if possible
    public void MoveXPlusFace(float newPos)
    {
        Vector3 currentVPos;

        for (int i = 0; i < faces["XPlus"].Length; i++)
        {
            currentVPos = data.vertices[faces["XPlus"][i]];
            currentVPos.x = newPos;
            data.vertices[faces["XPlus"][i]] = currentVPos;
        }
    }

    public void MoveXMinusFace(float newPos)
    {
        Vector3 currentVPos;

        for (int i = 0; i < faces["XMinus"].Length; i++)
        {
            currentVPos = data.vertices[faces["XMinus"][i]];
            currentVPos.x = newPos;
            data.vertices[faces["XMinus"][i]] = currentVPos;
        }
    }

    public void MoveZPlusFace(float newPos)
    {
        Vector3 currentVPos;

        for (int i = 0; i < faces["ZPlus"].Length; i++)
        {
            currentVPos = data.vertices[faces["ZPlus"][i]];
            currentVPos.x = newPos;
            data.vertices[faces["ZPlus"][i]] = currentVPos;
        }
    }

    public void MoveZMinusFace(float newPos)
    {
        Vector3 currentVPos;

        for (int i = 0; i < faces["ZMinus"].Length; i++)
        {
            currentVPos = data.vertices[faces["ZMinus"][i]];
            currentVPos.x = newPos;
            data.vertices[faces["ZMinus"][i]] = currentVPos;
        }
    }

    public void MoveyYPlusFace(float newPos)
    {
        Vector3 currentVPos;

        for (int i = 0; i < faces["YPlus"].Length; i++)
        {
            currentVPos = data.vertices[faces["YPlus"][i]];
            currentVPos.x = newPos;
            data.vertices[faces["YPlus"][i]] = currentVPos;
        }
    }

    public void MoveYMinusFace(float newPos)
    {
        Vector3 currentVPos;

        for (int i = 0; i < faces["YMinus"].Length; i++)
        {
            currentVPos = data.vertices[faces["YMinus"][i]];
            currentVPos.x = newPos;
            data.vertices[faces["YMinus"][i]] = currentVPos;
        }
    }

    //A generalized version of the face move functions
    public void MoveFace(string faceToMove, float newPos)
    {
        if((faceToMove != "XPlus") || (faceToMove != "XMinus") || 
            (faceToMove != "YPlus") || (faceToMove != "YMinus") || 
            (faceToMove != "ZPlus") || (faceToMove != "ZMinus"))
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
