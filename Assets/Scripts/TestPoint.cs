﻿using System;
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
    [SerializeField]private bool _onPoint;
    private IEnumerator coroutine;
    
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
        _onPoint = false;

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
    
    private void OnMouseOver()
    {
        if (isActivePoint)
            return;
        if (_activePoint && !_isActiveSelectable)
            return;
        _rend.enabled = true;
        _onPoint = true;
    }
    
    private void OnMouseExit()
    {
        if (isActivePoint)
            return;
        if (_activePoint && !_isActiveSelectable)
            return;
        _rend.enabled = false;
        _onPoint = false;
    }

    private void OnMouseDown()
    {
        Debug.Log(ptID);
        if (!_activePoint)
        {
            isActivePoint = true;
            _activePoint = this;
            CleanAdjacentPointLists();
            ToggleSelectablePoints(false);
            coroutine = PreviewSelectablePoints();
            StartCoroutine(coroutine);
            _rend.enabled = true;
        }
        else if (_isActiveSelectable)
        {
            GameManager.SharedInstance.edgeManager.GenerateEdge(_activePoint, this);
            _activePoint.isActivePoint = false;
            _activePoint._rend.enabled = false;
            StopCoroutine(_activePoint.coroutine);
            ToggleSelectablePoints(true);
            _activePoint._onPoint = false;
            _activePoint = null;
            _rend.enabled = true;
        }
    }

    private void CleanAdjacentPointLists()
    {
        _activePoint._adjacentXPoints.RemoveAll(i => !i);
        _activePoint._adjacentYPoints.RemoveAll(i => !i);
        _activePoint._adjacentZPoints.RemoveAll(i => !i);
    }

    private void ToggleSelectablePoints(bool disableRenderers)
    {
        foreach (var tp in _activePoint._adjacentXPoints)
        {
            tp._isActiveSelectable = !tp._isActiveSelectable;
            if (tp == this)
                continue;
            if (disableRenderers)
                tp._rend.enabled = false;
        }
        foreach (var tp in _activePoint._adjacentYPoints)
        {
            tp._isActiveSelectable = !tp._isActiveSelectable;
            if (tp == this)
                continue;
            if (disableRenderers)
                tp._rend.enabled = false;
        }
        foreach (var tp in _activePoint._adjacentZPoints)
        {
            tp._isActiveSelectable = !tp._isActiveSelectable;
            if (tp == this)
                continue;
            if (disableRenderers)
                tp._rend.enabled = false;
        }
    }

    private IEnumerator PreviewSelectablePoints()
    {
        int xIndex = 0;
        int yIndex = 0;
        int zIndex = 0;

        List<TestPoint> adjXPoints = _activePoint._adjacentXPoints;
        List<TestPoint> adjYPoints = _activePoint._adjacentYPoints;
        List<TestPoint> adjZPoints = _activePoint._adjacentZPoints;
        
        while (_activePoint)
        {
            yield return new WaitForSeconds(.4f);
            if (!_activePoint)
                break;
            if (adjXPoints.Count > 0)
            {
                if (xIndex > 0 && !adjXPoints[xIndex - 1]._onPoint)
                    adjXPoints[xIndex - 1]._rend.enabled = false;
                else if (xIndex == 0 && !adjXPoints[adjXPoints.Count - 1]._onPoint)
                    adjXPoints[adjXPoints.Count - 1]._rend.enabled = false;
                adjXPoints[xIndex]._rend.enabled = true;
                xIndex++;
                xIndex %= adjXPoints.Count;
            }

            if (adjYPoints.Count > 0)
            {
                if (yIndex > 0 && !adjYPoints[yIndex - 1]._onPoint)
                    adjYPoints[yIndex - 1]._rend.enabled = false;
                else if (yIndex == 0 && !adjYPoints[adjYPoints.Count - 1]._onPoint)
                    adjYPoints[adjYPoints.Count - 1]._rend.enabled = false;
                adjYPoints[yIndex]._rend.enabled = true;
                yIndex++;
                yIndex %= adjYPoints.Count;
            }

            if (adjZPoints.Count > 0)
            {
                if (zIndex > 0 && !adjZPoints[zIndex - 1]._onPoint)
                    adjZPoints[zIndex - 1]._rend.enabled = false;
                else if (zIndex == 0 && !adjZPoints[adjZPoints.Count - 1]._onPoint)
                    adjZPoints[adjZPoints.Count - 1]._rend.enabled = false;
                adjZPoints[zIndex]._rend.enabled = true;
                zIndex++;
                zIndex %= adjZPoints.Count;
            }
        }
        
        // foreach (var tp in adjXPoints)
        // {
        //     tp._rend.enabled = false;
        // }
        // foreach (var tp in adjYPoints)
        // {
        //     tp._rend.enabled = false;
        // }
        // foreach (var tp in adjZPoints)
        // {
        //     tp._rend.enabled = false;
        // }
    }

    private void Update()
    {

    }
}
