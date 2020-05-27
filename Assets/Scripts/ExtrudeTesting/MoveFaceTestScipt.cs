using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFaceTestScipt : MonoBehaviour
{

    public ProceduralCube cube;
    public float moveSpeed = 1;
    public float pos = 1;

    private void Update()
    {
        if(pos < 5)
        {
            pos += moveSpeed * Time.deltaTime;
            cube.MoveFace("z+", pos);
        }
    }

}
