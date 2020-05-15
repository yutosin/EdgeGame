using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPoint : MonoBehaviour
{
    private Renderer _rend;
    
    public bool isActivePoint;
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
    }
    
    private void OnMouseExit()
    {
        if (isActivePoint)
            return;
        _rend.enabled = false;
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
            GameManager.SharedInstance.edgeManager.GenerateLine(_activePoint.transform.position, transform.position);
            _activePoint.isActivePoint = false;
            _activePoint.gameObject.GetComponent<Renderer>().enabled = false;
            _activePoint = null;
            return;

        }
        _rend.enabled = true;
    }
}
