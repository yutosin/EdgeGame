using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobonautHandler : MonoBehaviour
{
    //Script should be put in empty gameobject. Player model should be a child of the object and added to the playerModel transform below.
    [Header("Set in Inspector")]
    public Transform playerModel;
    public float playerSpeed;
    public float turnSpeed;
    public float foreLean;
    public float sideLean;

    [Header ("Dynamic")]
    public Vector3[] targetPath;
    public bool commit;

    private int stage;
    private bool stageOn;
    private float dist;
    private float baseDist;
    private float vel;
    private float angle;
    private float multAngle;
    private float oldAngle;
    private float deltaAngle;
    private float eulerX;
    private float eulerY;
    private float eulerZ;
    private Vector3 pos;
    private Vector3 oldPos;
    private Vector3 dynPos;
    private Vector3 basePos;

    void Start()
    {
        commit = false;
        stage = 0;
        stageOn = false;
        dist = 0;
        baseDist = 0;
        angle = 0;
        multAngle = 0;
        oldAngle = 0;
        deltaAngle = 0;
        eulerX = 0;
        eulerY = 0;
        eulerZ = 0;
        
        pos = transform.position;
        oldPos = pos;
    }

    private void Update()
    {
        if (commit)
        {
            stage = 0;
            basePos = transform.position;
            stageOn = true;
            commit = false;
        }

        if (stageOn)
        {
            //Locomotion
            oldPos = pos;
            int clampedStage = Mathf.Clamp(stage, 0, targetPath.Length - 1);
            dist = Mathf.Sqrt(Mathf.Pow(targetPath[clampedStage].z - pos.z, 2) + Mathf.Pow(targetPath[clampedStage].x - pos.x, 2)); ;
            baseDist = Mathf.Sqrt(Mathf.Pow(targetPath[clampedStage].z - basePos.z, 2) + Mathf.Pow(targetPath[clampedStage].x - basePos.x, 2));
            angle = -Mathf.Atan2(targetPath[clampedStage].z - basePos.z, targetPath[clampedStage].x - basePos.x) * (180 / Mathf.PI) / 360 + 0.5f;
            dynPos = new Vector3(
                (-Mathf.Cos(angle * 2 * Mathf.PI)) * Mathf.Clamp(baseDist, 0, (playerSpeed / 100)),
                0,
                Mathf.Sin(angle * Mathf.PI * 2) * Mathf.Clamp(baseDist, 0, (playerSpeed / 100))
            );
            basePos += dynPos;
            pos = new Vector3(
                Mathf.LerpUnclamped(transform.position.x, basePos.x, playerSpeed / 200),
                Mathf.Lerp(transform.position.y, targetPath[clampedStage].y, 0.05f),
                Mathf.LerpUnclamped(transform.position.z, basePos.z, playerSpeed / 200)
            );
            transform.position = pos;
            vel = Mathf.Sqrt(Mathf.Pow(oldPos.z - pos.z, 2) + Mathf.Pow(oldPos.x - pos.x, 2));
            if (baseDist < playerSpeed / 2000 && baseDist > -(playerSpeed / 2000))
            {
                stage++;
                if (stage >= targetPath.Length)
                {
                    if (dist < playerSpeed / 8000 && dist > -(playerSpeed / 8000))
                    {
                        stageOn = false;
                        transform.position = targetPath[targetPath.Length - 1];
                    }
                }
            }

            //Rotation
            eulerY = (Mathf.Atan2(oldPos.x - pos.x, oldPos.z - pos.z) * 180 / Mathf.PI) + 180;
            oldAngle = playerModel.eulerAngles.y;
            deltaAngle = oldAngle - eulerY;
            if (deltaAngle > 180)
            {
                eulerY += 360;
                deltaAngle -= 360;
            }
            else if (deltaAngle < -180)
            {
                eulerY -= 360;
                deltaAngle += 360;
            }
            eulerX = vel * foreLean;
            eulerZ = sideLean * deltaAngle * vel * 0.1f;
            playerModel.eulerAngles = new Vector3(0, Mathf.Lerp(playerModel.eulerAngles.y, eulerY, turnSpeed / 50), 0);
            //Debug.Log(deltaAngle);
        }
    }
}
