using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class DirectedAgent : MonoBehaviour
{
    private RaycastHit _hitInfo;
    private Vector3 _goalPoint;
    private bool _hasJump;
    private Vector3 _jumpPoint;
    private Seeker _seeker;
    
    public RobonautHandeler2 mover;
    public Transform targetPosition;

    // Use this for initialization
    void Awake () 
    {
        mover = GetComponent<RobonautHandeler2>();
        _seeker = GetComponent<Seeker>();
        _hitInfo = new RaycastHit();
    }

    private void Start()
    {
        // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        //_seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
    }

    public void OnPathComplete (Path p) {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        Debug.Log(p.vectorPath[p.vectorPath.Count - 1]);
        mover.targetPath = p.vectorPath.ToArray();
        mover.commit = true;
    }

    private void Update()
    {
        // if (Vector3.Distance(agent.nextPosition, _goalPoint) <= .6f)
        // {
        //     SceneManager.LoadScene("MainMenu");
        // }
    }

    public void HandleFacePointSelect()
    {
        var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out _hitInfo))
        {
            //GameManager.SharedInstance.playerAgent.agent.destination = _hitInfo.point;
            var nearest = GameManager.SharedInstance.levelGraph.GetNearest(_hitInfo.point, NNConstraint.Default);
            var nearestNodePos = nearest.clampedPosition;
            _seeker.StartPath(transform.position, nearestNodePos, OnPathComplete);
        }

        if (_hitInfo.collider.gameObject.CompareTag("Finish"))
            GameManager.SharedInstance.playerAgent._goalPoint = _hitInfo.point;
    }
}
