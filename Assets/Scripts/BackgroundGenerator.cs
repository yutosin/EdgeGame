using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    public int mode;
    public Transform cube;
    public Material cubeMat;
    public Color cubeCol;

    [Header("Set Cloud Parameters")]
    public Vector3 cloudMin;
    public Vector3 cloudMax;

    void Awake()
    {
        if (mode == 0) Debug.Log("Blank!");
        else if (mode == 1) Cloudmaker(cube);
        else if (mode == 2) Debug.Log("Flying Islands!");
        else if (mode == 3) Debug.Log("Caverns!");
        else if (mode == 4) Debug.Log("Forests!");
    }

    void FixedUpdate()
    {
    }

    void Cloudmaker(Transform cube)
    {
        int nimbocount = Random.Range(2, 13);
        GameObject cloud = new GameObject("Cloud");
        cloud.transform.SetParent(transform);
        for (int n = 0; n < nimbocount; n++)
        {
            Transform piece = Instantiate(cube, cloud.transform);
            piece.localScale = new Vector3(Random.Range(cloudMin.x, cloudMax.x), Random.Range(cloudMin.y, cloudMax.y), Random.Range(cloudMin.z, cloudMax.z));
            piece.position = new Vector3(Random.Range(cloudMin.x, cloudMax.x), Random.Range(cloudMin.y, cloudMax.y), Random.Range(cloudMin.z, cloudMax.z));
            piece.GetComponent<Renderer>().material = cubeMat;
            piece.GetComponent<Renderer>().material.color = cubeCol;
        }
    }
}