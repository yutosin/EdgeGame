using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnHolder : MonoBehaviour
{
    //The material is just a default
    public Material whatMaterial;
    public GameObject cubePrefab;
    //Accessible lists by other scripts to keep track of cubes spawned
    [HideInInspector]
    public List<GameObject> cubes = new List<GameObject>();
    [HideInInspector]
    public List<ProceduralCube> cScripts = new List<ProceduralCube>();
    //This is a default placeholder, can and should be changed before the cube is actually spawned in
    [HideInInspector]
    public Vector3[] initalPos = new Vector3[4]
    { new Vector3 (1, 1, 1) , new Vector3 (1, -1, -1) ,
        new Vector3 (1, 1, -1) , new Vector3 (1, 1, -1) };

    public void CreateACube()
    {
        GameObject newCube = Instantiate(cubePrefab);
        ProceduralCube newScript = newCube.GetComponent<ProceduralCube>();
        cubes.Add(newCube);
        cScripts.Add(newScript);

        newScript.SetInitialPos(initalPos, whatMaterial);
    }

    private void Start()
    {
        //This was just to test out assigning of materials
        //CreateACube(initalPos, defaultMaterial);
    } 

}
