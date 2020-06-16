using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobonautHandler_Mod : MonoBehaviour
{
    [Header("Set In Inspector")]
    public Transform playerModel;
    public float playerSpeed;
    public float spinSpeed;
    public float foreLean;
    public float sideLean;

    [Header("Dynamic Feed")]
    public Vector3[] targetPath;
    public bool commit;

    private Vector3 pos;
    private Vector3 oldPos;
    private int leg;
    private bool moving;

    private Vector3 dynamicPos;
    private float dist;
    private float vel;
    private float multVel;
    private float angle;
    private float oldAngle;
    private float deltaAngle;
    private float multAngle;

    private float eulerX;
    private float eulerY;
    private float eulerZ;

    void Start()
    {
        pos = transform.position;
        oldPos = pos;
        leg = 0;
        moving = false;

        dynamicPos = pos;
        dist = 0;
        vel = 0;
        multVel = 0;
        angle = 0;
        oldAngle = 0;
        deltaAngle = 0;
        multAngle = 0;

        eulerX = 0;
        eulerY = playerModel.eulerAngles.y;
        eulerZ = 0;
    }

    private void Update()
    {
        if (commit)
        {
            leg = 0;
            moving = true;
            commit = false;
        }
        if (moving)
        {
            //Position
            oldPos = pos;
            dist = Mathf.Sqrt(Mathf.Pow(targetPath[leg].z - dynamicPos.z, 2) + Mathf.Pow(targetPath[leg].x - dynamicPos.x, 2));
            angle = -Mathf.Atan2(targetPath[leg].z - pos.z, targetPath[leg].x - pos.x) * (180 / Mathf.PI) / 360 + 0.5f;
            oldAngle = playerModel.eulerAngles.y;
            dynamicPos += new Vector3(
                (-Mathf.Cos(angle * 2 * Mathf.PI)) * (playerSpeed / 100),
                targetPath[leg].y,
                Mathf.Sin(angle * Mathf.PI * 2) * (playerSpeed / 100)
            );
            transform.position = new Vector3(
                Mathf.LerpUnclamped(pos.x, dynamicPos.x, playerSpeed / 500 * multVel),
                targetPath[leg].y,
                Mathf.LerpUnclamped(pos.z, dynamicPos.z, playerSpeed / 500 * multVel)
            );
            pos = transform.position;
            vel = Mathf.Sqrt(Mathf.Pow(oldPos.z - pos.z, 2) + Mathf.Pow(oldPos.x - pos.x, 2));

            //Rotation
            eulerY = (Mathf.Atan2(oldPos.x - pos.x, oldPos.z - pos.z) * 180 / Mathf.PI) + 180;
            deltaAngle = oldAngle - eulerY;
            multVel = Mathf.Clamp(24 / Mathf.Abs(deltaAngle), 0.1f, 1);
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
            if (Mathf.Abs(deltaAngle) > 5)
            {
                if (multAngle < 1) { multAngle += 0.01f; }
            }
            else
            {
                if (multAngle > 0.05f) { multAngle -= 0.01f; }
            }
            eulerX = vel * foreLean;
            eulerZ = sideLean * deltaAngle * vel * 0.1f;
            playerModel.eulerAngles = new Vector3(eulerX, Mathf.Lerp(playerModel.eulerAngles.y, eulerY, spinSpeed / 100 * multAngle), eulerZ);
            Debug.Log(multVel);

            //Stop
            if (leg < targetPath.Length - 1)
            {
                if (dist < 1) { leg++; }
            }
            else if (eulerX < 0.1f && eulerZ < 0.1f && vel < 0.1f)
            {
                moving = false;
                playerModel.eulerAngles = new Vector3(0, playerModel.eulerAngles.y, 0);
                transform.position = targetPath[targetPath.Length - 1];
                leg = 0;
            }
        }
    }
}