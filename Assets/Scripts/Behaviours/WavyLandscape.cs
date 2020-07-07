using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavyLandscape : MonoBehaviour
{
    public GameObject cube;
    public int xSize;
    public int ySize;
    // Start is called before the first frame update
    void Awake()
    {
        //Cube[,] cubes = new Cube[xSize, ySize];
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                Transform newCube = cube.transform;
                newCube.localPosition = new Vector3(x - (xSize / 2), 0, y - (ySize / 2));
                Instantiate(cube, transform);
            }
        }
    }
}