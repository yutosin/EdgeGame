using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class DirectedAgent : MonoBehaviour
{
    private NavMeshPath _path;
    private RaycastHit _hitInfo;
    private Vector3 _goalPoint;
    private bool _hasJump;
    private Vector3 _jumpPoint;
    private Seeker _seeker;
    
    public NavMeshAgent agent;
    public RobonautMoverMod mover;
    public Transform targetPosition;

    // Use this for initialization
    void Awake () 
    {
        agent = GetComponent<NavMeshAgent>();
        mover = GetComponent<RobonautMoverMod>();
        _seeker = GetComponent<Seeker>();
        _path = new NavMeshPath();
        _hitInfo = new RaycastHit();
    }

    private void Start()
    {
        // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
        // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
        // takes to calculate the path. Usually it is called the next frame.
        _seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
    }

    public void OnPathComplete (Path p) {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        Debug.Log(p.vectorPath[p.vectorPath.Count - 1]);
    }

    private void Update()
    {
        if (Vector3.Distance(agent.nextPosition, _goalPoint) <= .6f)
        {
            SceneManager.LoadScene("MainMenu");
        }
        // if (_path.corners.Length == 0)
        //     return;
        // Vector3 adjustedNextPos = new Vector3(agent.nextPosition.x, agent.nextPosition.y - .5f, agent.nextPosition.z);
        // if (Vector3.Distance(adjustedNextPos, _path.corners[_path.corners.Length - 1]) <= .1f && _hasJump)
        // {
        //     agent.Warp(_jumpPoint);
        //     Debug.Log("close");
        //     _hasJump = false;
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

        if (NavMesh.CalculatePath(GameManager.SharedInstance.playerAgent.transform.position,
            _hitInfo.point,
            NavMesh.AllAreas,
            _path))
            Debug.Log("PATH!");

        if (_path.corners.Length < 2)
            return;
        
        // Vector3 lastPathPoint = _path.corners[_path.corners.Length - 1];
        // float verticalDiff = Mathf.Abs(agent.destination.y - lastPathPoint.y);
        // if (Mathf.Abs(1f - verticalDiff) > .01f)
        //     return;
        // if (Vector3.Distance(agent.destination, lastPathPoint) <= 1.5f)
        // {
        //     _jumpPoint = _hitInfo.point;
        //     _hasJump = true;
        // }

        List<Vector3> moverPoints = new List<Vector3>();
        for (int i = 1; i < _path.corners.Length; i++)
        {
            Vector3 moverPoint = new Vector3(_path.corners[i].x,
                GameManager.SharedInstance.playerAgent.transform.position.y,
                _path.corners[i].z);
            moverPoints.Add(moverPoint);
        }

        // GameManager.SharedInstance.playerAgent.mover.setPositions = moverPoints;
        // GameManager.SharedInstance.playerAgent.mover.commit = true;
    }

    public void MoveToLocation(Vector3 targetPoint)
    {
        agent.destination = targetPoint;
        agent.isStopped = false;
    }
}
