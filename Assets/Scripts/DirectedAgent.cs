using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class DirectedAgent : MonoBehaviour
{
    private RaycastHit _hitInfo;
    public Vector3 goalPoint;
    private Seeker _seeker;
    
    public RobonautHandler mover;
    public bool OnActiveAbility;
    public bool LevelLoading;

    // Use this for initialization
    void Awake () 
    {
        mover = GetComponent<RobonautHandler>();
        _seeker = GetComponent<Seeker>();
        _hitInfo = new RaycastHit();
        goalPoint = new Vector3(999, 999, 999);
        OnActiveAbility = false;
        LevelLoading = false;
    }

    public void OnPathComplete (Path p) {
        //Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        if (p.error)
        {
            Debug.Log(p.errorLog);
            return;
        }

        mover.targetPath = p.vectorPath.ToArray();
        mover.commit = true;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, goalPoint) <= .6f)
        {
            //SceneManager.LoadScene("MainMenu");
            LevelLoading = true;
            GameManager.SharedInstance.levelManager.NextLevel();
        }
    }

    public void HandleFacePointSelect()
    {
        var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out _hitInfo))
        {
            var nearestGoal = GameManager.SharedInstance.levelGraph.GetNearest(_hitInfo.point, NNConstraint.Default);
            var nearestNodePosTarget = (Vector3)nearestGoal.node.position;
            
            // var nearestStart = GameManager.SharedInstance.levelGraph.GetNearest(transform.position, NNConstraint.Default);
            // var nearestNodePosStart= (Vector3)nearestStart.node.position;
            _seeker.StartPath(transform.position, nearestNodePosTarget, OnPathComplete);
        }

        if (_hitInfo.collider.gameObject.CompareTag("Finish"))
            GameManager.SharedInstance.playerAgent.goalPoint = new Vector3(_hitInfo.point.x, _hitInfo.point.y + 0.5f, _hitInfo.point.z);
    }
}
