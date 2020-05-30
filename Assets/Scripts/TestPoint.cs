using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class TestPoint : MonoBehaviour
{
    private Renderer _rend;
    
    public bool isActivePoint;
    public string ptID;
    public int listLoc;
    private static TestPoint _activePoint;

    [SerializeField]private List<TestPoint> _adjacentXPoints;
    [SerializeField]private List<TestPoint> _adjacentYPoints;
    [SerializeField]private List<TestPoint> _adjacentZPoints;

    private bool _isActiveSelectable;
    
    private void Start()
    {
        if (Physics.Linecast(GameManager.SharedInstance.MainCamera.transform.position,
            transform.position, out RaycastHit hitInfo))
        {
            if (hitInfo.collider.gameObject.name != gameObject.name)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        _rend = GetComponent<Renderer>();
        _rend.shadowCastingMode = ShadowCastingMode.Off;
        _rend.receiveShadows = false;

        _isActiveSelectable = false;

        _adjacentXPoints = new List<TestPoint>();
        _adjacentYPoints = new List<TestPoint>();
        _adjacentZPoints = new List<TestPoint>();

        FillAdjacentList(_adjacentXPoints, Vector3.right);
        FillAdjacentList(_adjacentYPoints, Vector3.up);
        FillAdjacentList(_adjacentZPoints, Vector3.forward);
    }

    private void FillAdjacentList(List<TestPoint> adjacentList, Vector3 axis)
    {
        Vector3 tpPos = transform.position;
        //Check for adjacent points in positive axis dir
        Vector3 overlapPos = new Vector3(tpPos.x + axis.x, tpPos.y + axis.y, tpPos.z + axis.z);
        Collider[] hitCollider = Physics.OverlapSphere(overlapPos, .25f);
        int i = 1;
        while (hitCollider.Length > 0)
        {
            bool foundTp = false;
            foreach (Collider collider in hitCollider)
            {
                TestPoint tp = collider.gameObject.GetComponent<TestPoint>();
                if (tp)
                {
                    adjacentList.Add(tp);
                    foundTp = true;
                    break;
                }
            }
            if (!foundTp)
                break;
            i++;
            overlapPos = new Vector3(tpPos.x + axis.x * i,
                tpPos.y + axis.y * i,
                tpPos.z + axis.z * i);
            hitCollider = Physics.OverlapSphere(overlapPos, .25f);
        }
        
        //Check for adjacent points in negative axis dir
        overlapPos = new Vector3(tpPos.x - axis.x, tpPos.y - axis.y, tpPos.z - axis.z);
        hitCollider = Physics.OverlapSphere(overlapPos, .25f);
        i = 1;
        while (hitCollider.Length > 0)
        {
            bool foundTp = false;
            foreach (Collider collider in hitCollider)
            {
                TestPoint tp = collider.gameObject.GetComponent<TestPoint>();
                if (tp)
                {
                    adjacentList.Add(tp);
                    foundTp = true;
                    break;
                }
            }
            if (!foundTp)
                break;
            i++;
            overlapPos = new Vector3(tpPos.x - axis.x * i,
                tpPos.y - axis.y * i,
                tpPos.z - axis.z * i);
            hitCollider = Physics.OverlapSphere(overlapPos, .25f);
        }
    }

    private void OnMouseEnter()
    {
        if (isActivePoint)
            return;
        if (_activePoint && !_isActiveSelectable)
            return;
        _rend.enabled = true;
    }
    
    private void OnMouseExit()
    {
        if (isActivePoint)
            return;
        if (_activePoint && _isActiveSelectable)
            return;
        _rend.enabled = false;
    }

    private void OnMouseDown()
    {
        Debug.Log(ptID);
        if (!_activePoint)
        {
            isActivePoint = true;
            _activePoint = this;
            ToggleSelectablePoints();
            _rend.enabled = true;
        }
        else if (_isActiveSelectable)
        {
            GameManager.SharedInstance.edgeManager.GenerateEdge(_activePoint, this);
            _activePoint.isActivePoint = false;
            _activePoint._rend.enabled = false;
            ToggleSelectablePoints();
            _activePoint = null;
            _rend.enabled = true;
        }
    }

    private void ToggleSelectablePoints()
    {
        foreach (var tp in _activePoint._adjacentXPoints)
        {
            if (!tp)
                continue;
            tp._rend.enabled = !tp._rend.enabled;
            tp._isActiveSelectable = !tp._isActiveSelectable;
        }
        foreach (var tp in _activePoint._adjacentYPoints)
        {
            if (!tp)
                continue;
            tp._rend.enabled = !tp._rend.enabled;
            tp._isActiveSelectable = !tp._isActiveSelectable;
        }
        foreach (var tp in _activePoint._adjacentZPoints)
        {
            if (!tp)
                continue;
            tp._rend.enabled = !tp._rend.enabled;
            tp._isActiveSelectable = !tp._isActiveSelectable;
        }
    }

    private void Update()
    {

    }
}
