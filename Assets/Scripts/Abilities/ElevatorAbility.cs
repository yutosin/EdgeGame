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
    private Vector3[] faceVertices;

    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        Vector3 facePoint = AbilityFace.Parent.position;
        Vector3 target = new Vector3(facePoint.x, facePoint.y + 3, facePoint.z);
        _target = target;
        GameManager.SharedInstance.playerAgent.transform.parent = transform.parent;
        GameManager.SharedInstance.playerAgent.agent.enabled = false;
        IsActing = true;
    }
    
    IEnumerator SetAgentPosition()
    {
        yield return new WaitForSeconds(0.01f);
        var playerTransform = GameManager.SharedInstance.playerAgent.transform;
        playerTransform.parent = null;
        GameManager.SharedInstance.playerAgent.agent.enabled = true;
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
        GameManager.SharedInstance.playerAgent.agent.nextPosition = AbilityFace.Parent.position;
        if (Vector3.Distance(AbilityFace.Parent.position, _target) < .001f)
        {
            IsActing = false;
            var meshSurface = GameManager.SharedInstance.edgeManager._meshSurface;
            meshSurface.UpdateNavMesh(meshSurface.navMeshData);
            StartCoroutine(SetAgentPosition());
            AbilityTimes--;
        }
    }
}
