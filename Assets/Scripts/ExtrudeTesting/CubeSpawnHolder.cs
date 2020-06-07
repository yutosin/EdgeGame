using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnHolder : MonoBehaviour
{
    //remove later, this is for testing
    public Material defaultMaterial;

    [HideInInspector]
    public List<GameObject> cubes = new List<GameObject>();
    [HideInInspector]
    public List<ProceduralCube> cScripts = new List<ProceduralCube>();

    public GameObject cubePrefab;

    public void CreateACube(Vector3[] arrays, Material toSet)
    {
        GameObject newCube = Instantiate(cubePrefab);
        ProceduralCube newScript = newCube.GetComponent<ProceduralCube>();
        cubes.Add(newCube);
        cScripts.Add(newScript);

        newScript.SetInitialPos(arrays);
        if (!toSet)
        {
            newScript.rend.material = defaultMaterial;
            return;
        }

        newScript.rend.material = toSet;
        //Debug.Log(toSet);

    }

    /////////////////////////
    ///Below is simply for testing, will be removed later
    ///Below is simply for testing, will be removed later
    ///Below is simply for testing, will be removed later
    /////////////////////////

    private void Start()
    {
        Vector3[] sample = new Vector3[4];
        sample[0] = new Vector3 (1, 1, 1);
        sample[1] = new Vector3(1, -1, -1);
        sample[2] = new Vector3(1, 1, -1);
        sample[3] = new Vector3(1, -1, 1);
        CreateACube(sample, defaultMaterial);
    }

}
