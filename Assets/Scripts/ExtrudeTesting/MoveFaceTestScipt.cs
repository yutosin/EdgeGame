using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFaceTestScipt : MonoBehaviour
{

    public ProceduralCube cube;
    public float moveSpeed = 1;
    public float pos1 = 1;
    public float pos2 = -1;

    private void Update()
    {
        if(pos1 < 5)
        {
            pos1 += moveSpeed * Time.deltaTime;
            cube.MoveFace("XPlus", pos1);
            
        }
        if (pos1 >= 5 && pos2 < 4)
        {
            pos2 += moveSpeed * Time.deltaTime;
            cube.MoveFace("XMinus", pos2);

        }
    }
}
