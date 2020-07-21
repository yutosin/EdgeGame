using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobonautHandler : MonoBehaviour
{
    //Script should be put in empty gameobject. Player model should be a child of the object and added to the playerModel transform below.
    [Header("Set in Inspector")]
    public Transform playerModel;
    public float playerSpeed;
    public float turnSpeed;
    public float foreLean;
    public float sideLean;
    public float climbSpeed;
    public float swingOut; //How far the robonaut will position beyond the side of a block when climbing.

    [Header("Dynamic")]
    public Vector3[] targetPath;
    public bool commit;

    private int stage;
    private int clampedStage;
    private bool stageOn;
    private int climbOn;
    private float dist;
    private float vel;
    private float angle;
    private float targetAngle;
    private float oldAngle;
    private float deltaAngle;
    private float eulerY;
    private Vector3 pos;
    private Vector3 oldPos;
    private Vector3 dynPos;
    private Vector3 basePos;
    private Vector3 goHere;

    void Start()
    {
        commit = false;
        stage = 0;
        clampedStage = 0;
        stageOn = false;
        climbOn = 0;
        dist = 0;
        angle = 0;
        targetAngle = 0;
        oldAngle = 0;
        deltaAngle = 0;
        eulerY = 0;
        pos = transform.position;
        oldPos = pos;
        goHere = new Vector3(0, 0, 0);
    }

    private void FixedUpdate()
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
            if (GameManager.SharedInstance.playerAgent.OnActiveAbility)
            {
                stageOn = false;
                return;
            }

            if (GameManager.SharedInstance.playerAgent.LevelLoading)
            {
                stageOn = false;
                GameManager.SharedInstance.playerAgent.LevelLoading = false;
                return;
            }

            //Locomotion
            oldPos = pos;
            clampedStage = Mathf.Clamp(stage, 0, targetPath.Length - 1);
            if (climbOn == 0) goHere = targetPath[clampedStage];
            dist = Mathf.Sqrt(Mathf.Pow(goHere.z - pos.z, 2) + Mathf.Pow(goHere.y - pos.y, 2) + Mathf.Pow(goHere.x - pos.x, 2));
            angle = -Mathf.Atan2(goHere.z - basePos.z, goHere.x - basePos.x) * (180 / Mathf.PI) / 360 + 0.5f;
            dynPos = new Vector3(
                (-Mathf.Cos(angle * 2 * Mathf.PI)) * Mathf.Clamp(dist, 0, (playerSpeed / 100)),
                0,
                Mathf.Sin(angle * Mathf.PI * 2) * Mathf.Clamp(dist, 0, (playerSpeed / 100))
            );
            basePos += dynPos;
            if (climbOn == 2)
            {
                if (pos.y > targetPath[clampedStage].y) pos -= new Vector3(0, (climbSpeed / 100), 0);
                else if (pos.y < targetPath[clampedStage].y) pos += new Vector3(0, (climbSpeed / 100), 0);
            }
            else
            {
                pos = new Vector3(
                    Mathf.LerpUnclamped(transform.position.x, basePos.x, playerSpeed / 200),
                    Mathf.Lerp(transform.position.y, goHere.y, 0.05f),
                    Mathf.LerpUnclamped(transform.position.z, basePos.z, playerSpeed / 200)
                );
            }
            transform.position = pos;
            vel = Mathf.Sqrt(Mathf.Pow(oldPos.z - pos.z, 2) + Mathf.Pow(oldPos.x - pos.x, 2));
            if (climbOn == 0)
            {
                if (dist < playerSpeed / 12 && dist > -(playerSpeed / 12))
                {
                    if (!StageOverflow())
                    {
                        if (pos.y != targetPath[stage].y) climbOn = 1;
                        if (Mathf.Abs(targetPath[stage - 1].x - targetPath[stage].x) > Mathf.Abs(targetPath[stage - 1].z - targetPath[stage].z))
                        {
                            if (targetPath[stage].x > basePos.x)
                            {
                                //Go down-left (Positive X).
                                goHere = new Vector3(targetPath[stage - 1].x + 0.5f + ((targetPath[stage].y > pos.y) ? -swingOut : swingOut), targetPath[stage - 1].y, targetPath[stage - 1].z);
                            }
                            else
                            {
                                //Go up-right (Negative X).
                                goHere = new Vector3(targetPath[stage - 1].x - 0.5f - ((targetPath[stage].y > pos.y) ? -swingOut : swingOut), targetPath[stage - 1].y, targetPath[stage - 1].z);
                            }
                        }
                        else
                        {
                            if (targetPath[stage].z > basePos.z)
                            {
                                //Go down-right (Positive Z).
                                goHere = new Vector3(targetPath[stage - 1].x, targetPath[stage - 1].y, targetPath[stage - 1].z + 0.5f + ((targetPath[stage].y > pos.y) ? -swingOut : swingOut));
                            }
                            else
                            {
                                //Go up-left (Negative Z).
                                goHere = new Vector3(targetPath[stage - 1].x, targetPath[stage - 1].y, targetPath[stage - 1].z - 0.5f - ((targetPath[stage].y > pos.y) ? -swingOut : swingOut));
                            }
                        }
                        targetAngle = -Mathf.Atan2(targetPath[stage].z - basePos.z, targetPath[stage].x - basePos.x) * (180 / Mathf.PI) + 90;
                    }
                }
            }
            else if (climbOn == 1)
            {
                if (dist < (climbSpeed / 20) && dist > -(climbSpeed / 20))
                {
                    transform.position = goHere;
                    climbOn = 2;
                    goHere = new Vector3(goHere.x, targetPath[stage].y, goHere.z);
                }
            }
            else if (climbOn == 2)
            {
                if (pos.y < targetPath[clampedStage].y + 0.001f && pos.y > targetPath[clampedStage].y - 0.001f)
                {
                    goHere = targetPath[stage];
                    transform.position = new Vector3(transform.position.x, goHere.y, transform.position.z);
                    climbOn = 0;
                }
            }

            //Rotation
            if (climbOn == 1) eulerY = targetAngle;
            else if (climbOn == 2) eulerY = ((pos.y > targetPath[stage].y) ? targetAngle + 180 : targetAngle);
            else eulerY = (Mathf.Atan2(oldPos.x - pos.x, oldPos.z - pos.z) * 180 / Mathf.PI) + 180;
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
            if (dist > 0.1f) playerModel.eulerAngles = new Vector3(0, Mathf.Lerp(playerModel.eulerAngles.y, eulerY, turnSpeed / 50), 0);
        }
        AnimationController();
    }

    bool StageOverflow()
    {
        stage++;
        if (stage >= targetPath.Length)
        {
            if (dist < playerSpeed / 8000 && dist > -(playerSpeed / 8000))
            {
                stageOn = false;
                transform.position = targetPath[targetPath.Length - 1];
            }
            return true;
        }
        else return false;
    }

    private void AnimationController()
    {
        if (GameManager.SharedInstance)
            GetComponentInChildren<Animator>().SetBool("PlayMode", GameManager.SharedInstance.PlayMode);

        GetComponentInChildren<Animator>().SetFloat("MoveSpeed", vel);

        if (oldPos.y < pos.y)
            GetComponentInChildren<Animator>().SetInteger("ChangeY", 1);
        else if (oldPos.y < pos.y)
            GetComponentInChildren<Animator>().SetInteger("ChangeY", -1);
        else if (oldPos.y == pos.y)
            GetComponentInChildren<Animator>().SetInteger("ChangeY", 0);
    }
}
