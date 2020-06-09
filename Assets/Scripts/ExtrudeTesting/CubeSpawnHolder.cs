using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnHolder : MonoBehaviour
{
    public Material whatMaterial;
    public GameObject cubePrefab;
    [HideInInspector]
    public List<GameObject> cubes = new List<GameObject>();
    [HideInInspector]
    public List<ProceduralCube> cScripts = new List<ProceduralCube>();



    private Vector3[] initalPos = new Vector3[4]
    { new Vector3 (1, 1, 1) , new Vector3 (1, -1, -1) ,
        new Vector3 (1, 1, -1) , new Vector3 (1, 1, -1) };

    public void CreateACube()
    {
        GameObject newCube = Instantiate(cubePrefab);
        ProceduralCube newScript = newCube.GetComponent<ProceduralCube>();
        cubes.Add(newCube);
        cScripts.Add(newScript);

        newScript.SetInitialPos(initalPos);
        newScript.rend.material = whatMaterial;
        //Debug.Log(toSet);

    }

    private void Start()
    {
        //This was just to test out assigning of materials
        //CreateACube(initalPos, defaultMaterial);
    }

}
