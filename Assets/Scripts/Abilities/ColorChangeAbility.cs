using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeAbility : MonoBehaviour, IFaceAbility
{
    private Material _newMat;
    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        _newMat = new Material(Shader.Find("Unlit/Color"));
        _newMat.color = Color.cyan;
        AbilityFace._rend.material = _newMat;
    }

    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
    public int AbilityTimes { get; set; }
}
