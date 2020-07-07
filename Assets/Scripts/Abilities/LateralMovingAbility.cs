using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateralMovingAbility : MonoBehaviour, IFaceAbility
{
    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        GameManager.SharedInstance.playerAgent.transform.parent = transform;
        GameManager.SharedInstance.playerAgent.OnActiveAbility = true;
        IsActing = true;

        if (cubeChild == null)
        {
            cubeChild = Instantiate(cubePrefab);
            cScript = cubeChild.GetComponent<ProceduralCube>();
            cScript.SetInitialPos(AbilityFace.Vertices, AbilityFace._rend.material);
        }
    }

    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
    public int AbilityTimes { get; set; }

    public GameObject cubePrefab;
    private GameObject cubeChild;
    private ProceduralCube cScript;

    private Vector3 targetHeight, targetSideMotion, returnPos;
    private bool moved, heightSet, conditionsSet = false;

    public void SetStartingConditions(Face face, bool isMotionX)
    {
        if (conditionsSet)
            return;

        cubePrefab = GameManager.SharedInstance.matSelect.cubePrefab;

        AbilityFace = face;

        AbilityTimes = 1;

        targetHeight = AbilityFace.Parent.position;
        targetHeight.y += 1;
        returnPos = targetHeight;
        targetSideMotion = returnPos;
        if (isMotionX)
        {
            targetSideMotion.x -= 3;
        }
        else
        {
            targetSideMotion.z += 3;
        }
        
        //IsActing = true;
        conditionsSet = true;
    }

    private void Update()
    {
        if (!IsActing)
            return;
        if (!heightSet)
        {
            RaiseFace();
            return;
        }
        LateralMotion();

    }

    private void RaiseFace()
    {
        float step = 4 * Time.deltaTime;
        AbilityFace.Parent.position =
            Vector3.MoveTowards(AbilityFace.Parent.position, targetHeight, step);
        cScript.MoveFace("YPlus", AbilityFace.Parent.position.y);
        if (Vector3.Distance(AbilityFace.Parent.position, targetHeight) < .005f)
        {
            heightSet = true;
            cubeChild.transform.SetParent(AbilityFace.transform);
            AbilityFace.Parent.position = targetHeight;
            AstarPath.active.Scan();
        }
    }

    private void LateralMotion()
    {
        Vector3 target;
        if (!moved)
        { target = targetSideMotion; }
        else
        { target = returnPos; }

        float step = 2 * Time.deltaTime;
        AbilityFace.Parent.position =
            Vector3.MoveTowards(AbilityFace.Parent.position, target, step);
        AbilityFace.Tiles[0].transform.position = AbilityFace.Parent.position;
        if (Vector3.Distance(AbilityFace.Parent.position, target) < .001f)
        {
            IsActing = false;
            GameManager.SharedInstance.playerAgent.OnActiveAbility = false;
            AstarPath.active.Scan();
            StartCoroutine(SetAgentPosition());
            //Don't know if we want to limit this ability uses yet
            //AbilityTimes--;
        }
    }

    IEnumerator SetAgentPosition()
    {
        yield return new WaitForSeconds(0.01f);
        var playerTransform = GameManager.SharedInstance.playerAgent.transform;
        playerTransform.parent = null;
    }

}
