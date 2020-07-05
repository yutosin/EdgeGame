using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAbility : MonoBehaviour, IFaceAbility
{
    public void InitializeAbility(Face face)
    {
        AbilityFace = face;
        GameManager.SharedInstance.playerAgent.OnActiveAbility = true;
        SetAgentPosition();
    }

    public Face AbilityFace { get; set; }
    public bool IsActing { get; set; }
    public bool IsInitialized { get; set; }
    public int AbilityTimes { get; set; }

    private Vector3 otherPos;
    private bool otherSet = false;
    public Vector3 OtherPos
    {
        get { return (otherPos); }

        set
        {
            otherPos = value;
            otherSet = true;
        }
    }
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
        if (otherSet)
        {
            GameManager.SharedInstance.playerAgent.OnActiveAbility = true;
            Invoke("Teleport", .1f);
            AbilityTimes--;
        }
    }

    //this is a seperate function only because the input seems to sometime register on same frame they teleport, this doing this twice
    private void Teleport() 
    {
        var playerTransform = GameManager.SharedInstance.playerAgent.transform;
        Vector3 tpPos = otherPos;
        tpPos.y = playerTransform.position.y + otherPos.y;
        playerTransform.position = otherPos;
        GameManager.SharedInstance.playerAgent.OnActiveAbility = false;
    }

    private void Start()
    {
        AbilityTimes = 1;
    }

}
