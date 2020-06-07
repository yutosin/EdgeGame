using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    public Vector3[] Vertices;
    public int FaceId;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Debug.Log("Face Id: " + FaceId);
        GameManager.SharedInstance.CubeSpawnHolder.CreateACube(Vertices, null);
    }
}
