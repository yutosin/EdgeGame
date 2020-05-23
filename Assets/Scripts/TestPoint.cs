using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestPoint : MonoBehaviour
{
    private Renderer _rend;
    
    public bool isActivePoint;
    public string ptID;
    public int listLoc;
    private static TestPoint _activePoint;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
    }

    private void OnMouseEnter()
    {
        if (isActivePoint)
            return;
        _rend.enabled = true;
    }
    
    private void OnMouseExit()
    {
        if (isActivePoint)
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
        }
        else
        {
            GameManager.SharedInstance.edgeManager.GenerateEdge(_activePoint, this);
            _activePoint.isActivePoint = false;
            _activePoint.gameObject.GetComponent<Renderer>().enabled = false;
            _activePoint = null;
            return;

        }
        _rend.enabled = true;
    }
}
