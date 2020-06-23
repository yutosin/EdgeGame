using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMovingAbility : MonoBehaviour, IFaceAbility
{
    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        GameManager.SharedInstance.playerAgent.transform.parent = transform;
        IsActing = true;
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

    public void SetStartingConditions(Face face)
    {
        if (conditionsSet)
            return;

        AbilityFace = face;

        AbilityTimes = 1;

        targetHeight.y = AbilityFace.Parent.position.y + 1;
        returnPos = targetHeight;
        targetSideMotion = returnPos;
        targetSideMotion.x += 3;

        cubeChild = Instantiate(cubeChild);
        cubeChild.transform.parent.SetParent(AbilityFace.transform);
        cubeChild.transform.localPosition = Vector3.zero;
        cScript = cubeChild.GetComponent<ProceduralCube>();


        cScript.SetInitialPos(AbilityFace.Vertices, AbilityFace._rend.material);
        IsActing = true;
        conditionsSet = true;
    }

    private void Update()
    {
        if (!heightSet)
        {
            RaiseFace();
        }
        if (!IsActing)
            return;
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
            AbilityFace.Parent.position = targetHeight;
            AstarPath.active.Scan();
        }
    }

    private void LateralMotion()
    {
        Vector3 target = Vector3.zero;
        if (!moved)
        { target = targetSideMotion; }
        else
        { target = returnPos; }

        float step = 2 * Time.deltaTime;
        AbilityFace.Parent.position =
            Vector3.MoveTowards(AbilityFace.Parent.position, target, step);
        if (Vector3.Distance(AbilityFace.Parent.position, target) < .001f)
        {
            IsActing = true;
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
