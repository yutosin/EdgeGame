using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrudeFaceAbility : MonoBehaviour, IFaceAbility
{

    public void InitializeAbility(Face face)
    {
        MoveOutByOne();
    }

    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
    public int AbilityTimes { get; set; }

    public GameObject cubePrefab;
    private GameObject cubeChild;
    private ProceduralCube cScript;
    private Vector3 targetPos;
    private bool xMove;

    public void SetStartingConditions(Face face)
    {
        cubePrefab = GameManager.SharedInstance.matSelect.cubePrefab;

        AbilityFace = face;

        AbilityTimes = 1;

        xMove = xFacing();

        targetPos = AbilityFace.Parent.position;

        MoveOutByOne();
        //this is just to set the cube extruded as soon as you assign the color for testing purposes
        //comment out later

    }

    private bool xFacing()
    {
        bool isFacingX = true;

        Vector3[] verts = AbilityFace.Vertices;
        float xValue = verts[0].x;
        for (int i = 1; i < verts.Length; i++)
        {
            if(verts[i].x != xValue)
            {
                isFacingX = false;
                break;
            }
        }

        return (isFacingX);
    }

    private void MoveOutByOne()
    {
        if(cubeChild.transform.parent != null)
        {
            cubeChild.transform.parent = null;
        }
        if (xMove)
        {
            targetPos.x += 1;
        }
        else
        {
            targetPos.z += 1;
        }
        AbilityTimes--;
        IsActing = true;
    }

    private void ExtrudeFace()
    {
        if (cubeChild == null)
        {
            cubeChild = Instantiate(cubePrefab);
            cScript = cubeChild.GetComponent<ProceduralCube>();
            cScript.SetInitialPos(AbilityFace.Vertices, AbilityFace._rend.material);
        }

        float step = 4 * Time.deltaTime;
        AbilityFace.Parent.position =
            Vector3.MoveTowards(AbilityFace.Parent.position, targetPos, step);
        if (xMove)
        {
            cScript.MoveFace("XPlus", AbilityFace.Parent.position.x);
        }
        else
        {
            cScript.MoveFace("ZPlus", AbilityFace.Parent.position.z);
        }
        
        if (Vector3.Distance(AbilityFace.Parent.position, targetPos) < .005f)
        {
            cubeChild.transform.SetParent(AbilityFace.transform);
            AbilityFace.Parent.position = targetPos;
            IsActing = false;
            AstarPath.active.Scan();
        }

    }

    private void Update()
    {
        if (!IsActing)
            return;
        ExtrudeFace();
    }

}
