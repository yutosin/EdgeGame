using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAbility : MonoBehaviour, IFaceAbility
{
    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
    public int AbilityTimes { get; set; }
    private Vector3 _target;

    public GameObject cubePrefab;
    private GameObject cubeChild;
    private ProceduralCube cScript;

    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        Vector3 facePoint = AbilityFace.Parent.position;
        Vector3 target = new Vector3(facePoint.x, facePoint.y + 3, facePoint.z);
        _target = target;
        GameManager.SharedInstance.playerAgent.transform.parent = transform;
        GameManager.SharedInstance.playerAgent.OnActiveAbility = true;
        IsActing = true;
    }
    
    IEnumerator SetAgentPosition()
    {
        yield return new WaitForSeconds(0.01f);
        var playerTransform = GameManager.SharedInstance.playerAgent.transform;
        playerTransform.parent = null;
    }

    private IEnumerator DelayedScan()
    {
        yield return new WaitForFixedUpdate();
        AstarPath.active.Scan();
    }

    private void CubeSpawn()
    {
        cubePrefab = GameManager.SharedInstance.matSelect.cubePrefab;

        cubeChild = Instantiate(cubePrefab);
        cScript = cubeChild.GetComponent<ProceduralCube>();
        cScript.SetInitialPos(AbilityFace.Vertices, AbilityFace._rend.material);
    }

    private void RaiseCube()
    {
        cScript.MoveFace("YPlus", AbilityFace.Parent.position.y);
    }

    private void Start()
    {
        AbilityTimes = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsActing)
            return;
        float step = 3 * Time.deltaTime;
        AbilityFace.Parent.position =
            Vector3.MoveTowards(AbilityFace.Parent.position, _target, step);
        AbilityFace.Tiles[0].transform.position = AbilityFace.Parent.position;
        if (Vector3.Distance(AbilityFace.Parent.position, _target) < .001f)
        {
            cubeChild.transform.SetParent(AbilityFace.transform);
            IsActing = false;
            GameManager.SharedInstance.playerAgent.OnActiveAbility = false;
            AstarPath.active.Scan();
            StartCoroutine(SetAgentPosition());
            StartCoroutine(DelayedScan());
            AbilityTimes--;
        }
        if(cubeChild == null)
        {
            CubeSpawn();
        }
        RaiseCube();
    }
}
