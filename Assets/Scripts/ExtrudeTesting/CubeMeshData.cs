using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMeshData
{
    //holds sample vertices to make cubes with
    public Vector3[] vertices =
    {
        new Vector3(.1f, .1f, .1f),
        new Vector3(-.1f, .1f, .1f),
        new Vector3(-.1f, -.1f, .1f),
        new Vector3(.1f, -.1f, .1f),
        new Vector3(-.1f, .1f, -.1f),
        new Vector3(.1f, .1f, -.1f),
        new Vector3(.1f, -.1f, -.1f),
        new Vector3(-.1f, -.1f, -.1f)
    };

    //present triangles set up for making cube meshes
    public int[][] faceTriangles =
    {
        new int[]{0, 1, 2, 3},
        new int[]{5, 0, 3, 6},
        new int[]{4, 5, 6, 7},
        new int[]{1, 4, 7, 2},
        new int[]{5, 4, 1, 0},
        new int[]{3, 2, 7, 6},
    };

    //to get verticies from face triangles
    public Vector3[] faceVertices(int dir)
    {
        Vector3[] fv = new Vector3[4];
        for(int x = 0; x < fv.Length; x++)
        {
            fv[x] = vertices[faceTriangles[dir][x]];
        }
        return fv;
    }

}
