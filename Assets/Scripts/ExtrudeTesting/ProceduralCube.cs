using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCube : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    private void Start()
    {
        MakeCube();
        UpdateMesh();
    }

    private void UpdateMesh()
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
        vertices.AddRange(CubeMeshData.faceVertices(dir));
        int vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }

    //Trying to clean up these switches
    public void MoveFace(string faceDir, float newPos)
    {
        Vector3 currentVPos;
        int[] arrayPos = new int[4];
        switch (faceDir)
        {
            case "x+":
                arrayPos[0] = 0;
                arrayPos[1] = 3;
                arrayPos[2] = 5;
                arrayPos[3] = 6;

                for (int i = 0; i < arrayPos.Length; i++)
                {
                    currentVPos = vertices[arrayPos[i]];
                    currentVPos.x = newPos;
                    vertices[arrayPos[i]] = currentVPos;
                }

                break;
            case "x-":
                arrayPos[0] = 1;
                arrayPos[1] = 2;
                arrayPos[2] = 4;
                arrayPos[3] = 7;

                for (int i = 0; i < arrayPos.Length; i++)
                {
                    currentVPos = vertices[arrayPos[i]];
                    currentVPos.x = newPos;
                    vertices[arrayPos[i]] = currentVPos;
                }
                break;
            case "z+":
                arrayPos[0] = 0;
                arrayPos[1] = 1;
                arrayPos[2] = 2;
                arrayPos[3] = 3;

                for (int i = 0; i < arrayPos.Length; i++)
                {
                    currentVPos = vertices[arrayPos[i]];
                    currentVPos.z = newPos;
                    vertices[arrayPos[i]] = currentVPos;
                }
                break;
            case "z-":
                arrayPos[0] = 4;
                arrayPos[1] = 5;
                arrayPos[2] = 6;
                arrayPos[3] = 7;

                for (int i = 0; i < arrayPos.Length; i++)
                {
                    currentVPos = vertices[arrayPos[i]];
                    currentVPos.z = newPos;
                    vertices[arrayPos[i]] = currentVPos;
                }
                break;
            case "y+":
                arrayPos[0] = 0;
                arrayPos[1] = 1;
                arrayPos[2] = 4;
                arrayPos[3] = 5;

                for (int i = 0; i < arrayPos.Length; i++)
                {
                    currentVPos = vertices[arrayPos[i]];
                    currentVPos.y = newPos;
                    vertices[arrayPos[i]] = currentVPos;
                }
                break;
            case "y-":
                arrayPos[0] = 2;
                arrayPos[1] = 3;
                arrayPos[2] = 6;
                arrayPos[3] = 7;

                for (int i = 0; i < arrayPos.Length; i++)
                {
                    currentVPos = vertices[arrayPos[i]];
                    currentVPos.y = newPos;
                    vertices[arrayPos[i]] = currentVPos;
                }
                break;
            default :
                Debug.Log("Invalid faceDir string");
                break;
        }

        UpdateMesh();

    }

}
