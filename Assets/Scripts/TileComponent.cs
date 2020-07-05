using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponent : MonoBehaviour
{
    public string matName;
    public string orientation;

    private Renderer _rend;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
    }

    public void RevealTile()
    {
        bool materialSet = false;
        foreach (Material mat in GameManager.SharedInstance.LevelLoader.materials)
        {
            if (matName == mat.name)
            {
                _rend.material = mat;
                materialSet = true;
            }
        }
        
        if (materialSet)
            return;

        if (matName == GameManager.SharedInstance.LevelLoader.developerTop.name)
            _rend.material = GameManager.SharedInstance.LevelLoader.developerTop;
        else if (matName == GameManager.SharedInstance.LevelLoader.developerRight.name)
            _rend.material = GameManager.SharedInstance.LevelLoader.developerRight;
        else if (matName == GameManager.SharedInstance.LevelLoader.developerLeft.name)
            _rend.material = GameManager.SharedInstance.LevelLoader.developerLeft;
        else
            Debug.Log("Tile material does not exist");
    }
}
