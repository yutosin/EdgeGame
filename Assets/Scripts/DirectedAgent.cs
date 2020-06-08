using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DirectedAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public RobonautMoverMod mover;

    // Use this for initialization
    void Awake () 
    {
        agent = GetComponent<NavMeshAgent>();
        mover = GetComponent<RobonautMoverMod>();
    }

    public void MoveToLocation(Vector3 targetPoint)
    {
        agent.destination = targetPoint;
        agent.isStopped = false;
    }
}
