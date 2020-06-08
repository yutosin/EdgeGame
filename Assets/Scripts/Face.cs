using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Face : MonoBehaviour
{
    private NavMeshPath path;
    RaycastHit m_HitInfo = new RaycastHit();
    // Start is called before the first frame update
    void Start()
    {
        path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 point = Vector3.zero;
        if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
            point = m_HitInfo.point;
            // GameManager.SharedInstance.playerAgent.agent.destination = m_HitInfo.point;
        if (NavMesh.CalculatePath(GameManager.SharedInstance.playerAgent.transform.position,
            m_HitInfo.point,
            NavMesh.AllAreas,
            path))
            Debug.Log("PATH!");
        Debug.Log(path.corners[0]);
        if (path.corners.Length < 2)
            return;
        Debug.Log(path.corners[1]);
        // Vector3 moverPoint = new Vector3(path.corners[1].x, 
        //     GameManager.SharedInstance.playerAgent.transform.position.y, 
        //     path.corners[1].z);
        
        List<Vector3> moverPoints = new List<Vector3>();
        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 moverPoint = new Vector3(path.corners[i].x, 
                GameManager.SharedInstance.playerAgent.transform.position.y, 
                path.corners[i].z);
            moverPoints.Add(moverPoint);
        }
        
        // GameManager.SharedInstance.playerAgent.mover.setPosition = moverPoint;
        GameManager.SharedInstance.playerAgent.mover.setPositions = moverPoints;
        GameManager.SharedInstance.playerAgent.mover.commit = true;
    }
}
