﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Continue on this script, left off here
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    //grid settings
    public float cellSize = 1;
    public Vector3 gridOffset;
    public int gridSize = 1;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    private void Start()
    {
        MakeProceduralGrid();
        UpdateMesh();
    }

    private void MakeProceduralGrid()
    {
        //set array sizes
        vertices = new Vector3[gridSize * gridSize * 4];
        triangles = new int[gridSize * gridSize * 6];

        //tracker integers
        int v = 0;
        int t = 0;

        //set vertex offest
        float vOffset = cellSize * .5f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 cellOffset = new Vector3(x * cellSize, 0, y * cellSize);

                //populate vertices and triangle arrays
                vertices[v] = new Vector3(-vOffset, 0, -vOffset) + cellOffset + gridOffset;
                vertices[v + 1] = new Vector3(-vOffset, 0, vOffset) + cellOffset + gridOffset;
                vertices[v + 2] = new Vector3(vOffset, 0, -vOffset) + cellOffset + gridOffset;
                vertices[v + 3] = new Vector3(vOffset, 0, vOffset) + cellOffset + gridOffset;

                triangles[t] = 0;
                triangles[t + 1] = triangles[t + 4] = v + 1;
                triangles[t + 2] = triangles[t + 3] = v + 2;
                triangles[t + 5] = v + 3;

                v += 4;
                t += 6;

            }
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

}
