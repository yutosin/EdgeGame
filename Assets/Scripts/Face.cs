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

    private bool _activatedAbility;
    private Vector3 _target;
    private Renderer _rend;
    private bool _selected;
    private static Face _selectedFace;
    private static Material _defaultMat;
    private static Material _selectedMat;
    private static Material _abilityMat;

    public bool hasAbility;

    // Start is called before the first frame update
    void Start()
    {
        path = new NavMeshPath();
        _rend = GetComponent<Renderer>();
        _activatedAbility = false;
        hasAbility = false;
        
        _defaultMat = new Material(Shader.Find("Unlit/ColorZAlways"));
        _defaultMat.color = Color.gray;
        _defaultMat.renderQueue = 2001;
        
        _selectedMat = new Material(Shader.Find("Unlit/ColorZAlways"));
        Color selectColor = new Color();
        if (ColorUtility.TryParseHtmlString("#0979AD", out selectColor))
            _selectedMat.color = selectColor;
        else
            _selectedMat.color = Color.blue;
        _selectedMat.renderQueue = 2005;
        
        _abilityMat = new Material(Shader.Find("Unlit/ColorZAlways"));
        Color abilityColor = new Color();
        if (ColorUtility.TryParseHtmlString("#AD3911", out abilityColor))
            _abilityMat.color = abilityColor;
        else
            _abilityMat.color = Color.red;
        _abilityMat.renderQueue = 2005;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.SharedInstance.PlayMode)
        {
            if (!hasAbility)
                return;
            Vector3 facePoint = gameObject.transform.parent.position;
            Vector3 agentPoint = GameManager.SharedInstance.playerAgent.agent.destination;

            if (Vector3.Distance(facePoint, agentPoint) <= 1.0f && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("hit");
                Debug.Log(facePoint);
                Debug.Log(agentPoint);
                _activatedAbility = true;
                Vector3 target = new Vector3(facePoint.x, facePoint.y + 3, facePoint.z);
                _target = target;
                GameManager.SharedInstance.playerAgent.transform.parent = transform.parent;
                GameManager.SharedInstance.playerAgent.agent.enabled = false;
            }

            if (!_activatedAbility)
                return;
            float step = 3 * Time.deltaTime;
            gameObject.transform.parent.position =
                Vector3.MoveTowards(gameObject.transform.parent.position, _target, step);
            GameManager.SharedInstance.playerAgent.agent.nextPosition = gameObject.transform.parent.position;
            if (Vector3.Distance(gameObject.transform.parent.position, _target) < .001f)
            {
                _activatedAbility = false;
                var meshSurface = GameManager.SharedInstance.edgeManager._meshSurface;
                meshSurface.UpdateNavMesh(meshSurface.navMeshData);
                StartCoroutine(SetAgentPosition());
            }
        }
        else
        {
            if (_selectedFace == this && Input.GetKeyDown(KeyCode.E))
            {
                hasAbility = true;
                _selectedFace._rend.material = _abilityMat;
                _selectedFace = null;
            }
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.SharedInstance.PlayMode)
        {
            var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
                GameManager.SharedInstance.playerAgent.agent.destination = m_HitInfo.point;
            if (gameObject.CompareTag("Finish"))
                GameManager.SharedInstance.playerAgent.goalPoint = m_HitInfo.point;
            //point = m_HitInfo.point;

            if (NavMesh.CalculatePath(GameManager.SharedInstance.playerAgent.transform.position,
                m_HitInfo.point,
                NavMesh.AllAreas,
                path))
                Debug.Log("PATH!");

            if (path.corners.Length < 2)
                return;

            List<Vector3> moverPoints = new List<Vector3>();
            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 moverPoint = new Vector3(path.corners[i].x,
                    GameManager.SharedInstance.playerAgent.transform.position.y,
                    path.corners[i].z);
                moverPoints.Add(moverPoint);
            }

            // GameManager.SharedInstance.playerAgent.mover.setPositions = moverPoints;
            // GameManager.SharedInstance.playerAgent.mover.commit = true;
        }
        else
        {
            if (_selectedFace && !_selectedFace.hasAbility)
            {
                _selectedFace._rend.material = _defaultMat;
            }
            else if (_selectedFace && _selectedFace.hasAbility)
            {
                _selectedFace._rend.material = _abilityMat;
            }
            var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;
            hits = Physics.RaycastAll(ray.origin, ray.direction);
            float lowestHitDistance = 0;
            Face lastFace = null;
            for (int i = 0; i < hits.Length; i++)
            {
                m_HitInfo = hits[i];
                var face = m_HitInfo.collider.gameObject.GetComponent<Face>();
                if (!face)
                    continue;
                float hitDistance = Vector3.Distance(m_HitInfo.point, face.gameObject.transform.parent.position);
                if (lowestHitDistance == 0 || hitDistance < lowestHitDistance)
                    lowestHitDistance = hitDistance;
                else if (hitDistance > lowestHitDistance)
                    continue;
                if (lastFace)
                    lastFace._rend.material = _defaultMat;
                face._rend.material = _selectedMat;
                lastFace = face;
                _selectedFace = face;

            }
        }
    }

    private void OnMouseEnter()
    {

    }

    private void OnMouseExit()
    {

    }

    IEnumerator SetAgentPosition()
    {
        yield return new WaitForSeconds(0.01f);
        var playerTransform = GameManager.SharedInstance.playerAgent.transform;
        playerTransform.parent = null;
        GameManager.SharedInstance.playerAgent.agent.enabled = true;
    }
}
