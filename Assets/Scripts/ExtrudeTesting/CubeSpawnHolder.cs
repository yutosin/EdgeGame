using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnHolder : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> cubes = new List<GameObject>();
    [HideInInspector]
    public List<ProceduralCube> cScripts = new List<ProceduralCube>();

    public GameObject cubePrefab;

}
