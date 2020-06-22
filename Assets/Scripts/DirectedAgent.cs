using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectedAgent : MonoBehaviour
{
    private RaycastHit _hitInfo;
    private Vector3 _goalPoint;
    private Seeker _seeker;
    
    public RobonautHandler mover;

    // Use this for initialization
    void Awake () 
    {
        mover = GetComponent<RobonautHandler>();
        _seeker = GetComponent<Seeker>();
        _hitInfo = new RaycastHit();
        _goalPoint = new Vector3(999, 999, 999);
    }

    public void OnPathComplete (Path p) {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        mover.targetPath = p.vectorPath.ToArray();
        mover.commit = true;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _goalPoint) <= .6f)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void HandleFacePointSelect()
    {
        var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out _hitInfo))
        {
            var nearest = GameManager.SharedInstance.levelGraph.GetNearest(_hitInfo.point, NNConstraint.Default);
            var nearestNodePos = nearest.clampedPosition;
            _seeker.StartPath(transform.position, nearestNodePos, OnPathComplete);
        }

        if (_hitInfo.collider.gameObject.CompareTag("Finish"))
            GameManager.SharedInstance.playerAgent._goalPoint = new Vector3(_hitInfo.point.x, _hitInfo.point.y + 0.5f, _hitInfo.point.z);
    }
}
