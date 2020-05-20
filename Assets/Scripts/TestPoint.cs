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
    private static TestPoint _activePoint;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
    }

    private void OnMouseOver()
    {
        if (isActivePoint)
            return;
        _rend.enabled = true;
        //_rend.material.color = Color.white;
    }
    
    private void OnMouseExit()
    {
        if (isActivePoint)
            return;
        _rend.enabled = false;
        //_rend.material.color = new Color(1, 1, 1, 0);
    }

    private void OnMouseDown()
    {
        if (!_activePoint)
        {
            isActivePoint = true;
            _activePoint = this;
        }
        else
        {
            GameManager.SharedInstance.edgeManager.GenerateLine(_activePoint.transform.position, 
                transform.position, ptID, _activePoint.ptID);
            _activePoint.isActivePoint = false;
            _activePoint.gameObject.GetComponent<Renderer>().enabled = false;
            _activePoint = null;
            return;

        }
        _rend.enabled = true;
    }
}
