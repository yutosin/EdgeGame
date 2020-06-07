using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _sharedInstance = null;
    public static GameManager SharedInstance
    {
        get { return _sharedInstance; }
    }

    public EdgeManager edgeManager;
    public CubeSpawnHolder CubeSpawnHolder;
    public Camera MainCamera;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (_sharedInstance != null)
        {
            Destroy(gameObject);
            return;
        }
		
        _sharedInstance = this;

        MainCamera = Camera.main;
    }

    // Update is called once per frame
    private void Start()
    {
        
    }
}
