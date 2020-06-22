using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAbility : MonoBehaviour, IFaceAbility
{
    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        SetAgentPosition();
    }

    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
    public int AbilityTimes { get; set; }

    public Vector3 otherPos;
    public Vector3 thisPos;

    public Vector3 FindCenter(Face face)
    {
        Vector3[] verts = face.Vertices;
        Vector3 center = Vector3.zero;
        for (int i = 0; i < verts.Length; i++)
        {
            center += verts[i];
        }
        center = center / verts.Length;
        return (center);
    }

    private void SetAgentPosition()//this has been copied from the elevator ability, probably can do this another way since parenting is not needed
    {
        var playerTransform = GameManager.SharedInstance.playerAgent.transform;
        Vector3 tpPos = otherPos;
        tpPos.y = playerTransform.position.y + otherPos.y;
        playerTransform.position = otherPos;
    }

    private void Start()
    {
        AbilityTimes = 1;
    }

}
