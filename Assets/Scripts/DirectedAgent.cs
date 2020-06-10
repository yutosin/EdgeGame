using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class DirectedAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public RobonautMoverMod mover;
    public Vector3 goalPoint;

    // Use this for initialization
    void Awake () 
    {
        agent = GetComponent<NavMeshAgent>();
        mover = GetComponent<RobonautMoverMod>();
    }

    private void Update()
    {
        if (Vector3.Distance(agent.nextPosition, goalPoint) <= .6f)
        {
            SceneManager.LoadScene("MainMenu");
        }
        // if (agent.destination == goalPoint)
        // {
        //     SceneManager.LoadScene("MainMenu");
        // }
    }

    public void MoveToLocation(Vector3 targetPoint)
    {
        agent.destination = targetPoint;
        agent.isStopped = false;
    }
}
