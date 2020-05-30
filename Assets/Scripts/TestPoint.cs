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
    private bool _notDestroyed;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
        _rend.shadowCastingMode = ShadowCastingMode.Off;
        _rend.receiveShadows = false;
        _notDestroyed = false;
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

    private void Update()
    {
        if (_notDestroyed)
            return;
        if (Physics.Linecast(GameManager.SharedInstance.MainCamera.transform.position,
            transform.position, out RaycastHit hitInfo))
        {
            if (hitInfo.collider.gameObject.name != gameObject.name)
            {
                Destroy(gameObject);
            }
        }
        else
            _notDestroyed = true;
    }
}
