using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAbility : MonoBehaviour, IBaseAbility
{
    public GameObject cubePrefab;
    private Material _newMat;
    private Vector3 _currentPos, _startingPos;
    private bool _raised;

    public void InitializeAbility(Face face)
    {
        
    }

    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
}
