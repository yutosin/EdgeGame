using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCube : MonoBehaviour
{
    [HideInInspector]
    public CubeMeshData data = new CubeMeshData();
    Dictionary<string, int[]> faces = new Dictionary<string, int[]>();

    Mesh mesh;
    [HideInInspector]
    public MeshRenderer rend;

    List<Vector3> vertices;
    List<int> triangles;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        rend = GetComponent<MeshRenderer>();
        FillFacesDictionary();

    }

    private void Start()
    {
        MakeCube();
        UpdateMesh();
    }

    //This whole thing needs to be cleaned up, not nearly as neat as it was in my head
    public void SetInitialPos(Vector3[] coords)
    {
        //Want to clear this method up, got to be a better way
        float[] minMaxX = minAndMaxX(coords);
        MoveFace("XMinus", minMaxX[0]);
        MoveFace("XPlus", minMaxX[1]);

        float[] minMaxY = minAndMaxY(coords);
        MoveFace("YMinus", minMaxY[0]);
        MoveFace("YPlus", minMaxY[1]);

        float[] minMaxZ = minAndMaxZ(coords);
        MoveFace("ZMinus", minMaxZ[0]);
        MoveFace("ZPlus", minMaxZ[1]);

        float adjust;

        //This set of if statements is so that verts don't overlap and look uggo
        if ((coords[0].x == coords[1].x) && (coords[0].x == coords[2].x) && (coords[0].x == coords[3].x))
        {
            adjust = coords[0].x + -0.01f;
            MoveFace("XPlus", adjust);
        }
        else if ((coords[0].y == coords[1].y) && (coords[0].y == coords[2].y) && (coords[0].y == coords[3].y))
        {
            adjust = coords[0].z + -0.01f;
            MoveFace("YPlus", adjust);
        }
        else
        {
            adjust = coords[0].z + -0.01f;
            MoveFace("ZPlus", adjust);
        }
    }

    //returns 2 floats each
    private float[] minAndMaxX(Vector3[] coords)
    {
        float[] minAndMax = new float[2];

        float min = 1000;
        float max = -1000;

        for (int i = 0; i < coords.Length; i++)
        {
            if(coords[i].x > max)
            {
                max = coords[i].x;
            }

            if (coords[i].x < min)
            {
                min = coords[i].x;
            }
        }

        minAndMax[0] = min;
        minAndMax[1] = max;

        return (minAndMax);
    }
    private float[] minAndMaxY(Vector3[] coords)
    {
        float[] minAndMax = new float[2];

        float min = 1000;
        float max = -1000;

        for (int i = 0; i < coords.Length; i++)
        {
            if (coords[i].y > max)
            {
                max = coords[i].y;
            }

            if (coords[i].y < min)
            {
                min = coords[i].y;
            }
        }

        minAndMax[0] = min;
        minAndMax[1] = max;

        return (minAndMax);
    }
    private float[] minAndMaxZ(Vector3[] coords)
    {
        float[] minAndMax = new float[2];

        float min = 1000;
        float max = -1000;

        for (int i = 0; i < coords.Length; i++)
        {
            if (coords[i].z > max)
            {
                max = coords[i].z;
            }

            if (coords[i].z < min)
            {
                min = coords[i].z;
            }
        }

        minAndMax[0] = min;
        minAndMax[1] = max;

        return (minAndMax);
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
    private void FillFacesDictionary()
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
            Debug.LogError("Not a valid face. The face you put was " + faceToMove);
        }

        else
        {
            Vector3 currentVPos;

            ////////////////
            ///GLARING ISSUE, THIS IS UGLY AND I NEED TO CLEAN IT
            ///GLARING ISSUE, THIS IS UGLY AND I NEED TO CLEAN IT
            ///GLARING ISSUE, THIS IS UGLY AND I NEED TO CLEAN IT
            ////////////////
            if ((faceToMove == "XPlus") || (faceToMove == "XMinus"))
            {
                for (int i = 0; i < faces[faceToMove].Length; i++)
                {
                    currentVPos = data.vertices[faces[faceToMove][i]];
                    //This little currentVpos.X punk is the reason I had to split this into 3 statements :/
                    currentVPos.x = newPos;
                    data.vertices[faces[faceToMove][i]] = currentVPos;
                }
            }
            else if ((faceToMove == "YPlus") || (faceToMove == "YMinus"))
            {
                for (int i = 0; i < faces[faceToMove].Length; i++)
                {
                    currentVPos = data.vertices[faces[faceToMove][i]];

                    currentVPos.y = newPos;
                    data.vertices[faces[faceToMove][i]] = currentVPos;
                }
            }
            else
            {
                for (int i = 0; i < faces[faceToMove].Length; i++)
                {
                    currentVPos = data.vertices[faces[faceToMove][i]];

                    currentVPos.z = newPos;
                    data.vertices[faces[faceToMove][i]] = currentVPos;
                }
            }

            MakeCube();
            UpdateMesh();
        }

    }

}
