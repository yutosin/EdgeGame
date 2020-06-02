using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobonautMover : MonoBehaviour
{
    [Header("Placeholder Position")]
    public Vector3 setPosition;
    public bool commit;

    [Header("Player Variables")]
    //public Vector3 playerPosition;
    public Vector3 playerRotation;
    public float playerSpeed;
    public float moveSpeed;
    public float rotateSpeed;
    public float targetLean;

    [Header("Child Model")]
    public Transform playerChild;

    private Vector3 lastPosition; // Position on last tick, for calculating leaning.
    private Vector3 trackPosition;  // Position before last commit, for determining direction to turn towards.
    private float targetDirection;
    private float targetSpeed;
    private float rotTicker;
    private float posTicker;

    void Start()
    {
        if (playerChild == null) { playerChild = transform.GetChild(0); }
        lastPosition = transform.position;
        playerRotation = playerChild.transform.eulerAngles;
        commit = false;
        targetDirection = 0;
        posTicker = 0;
        rotTicker = 0;
    }
    void Update()
    {
        float posX = transform.position.x;
        float posZ = transform.position.z;
        float rotY = playerRotation.y;
        float rotZ = playerRotation.z;
        Vector3 deltaPosition = transform.position - lastPosition;
        lastPosition = transform.position;

        if (commit)
        {
            trackPosition = setPosition - transform.position;
            posTicker = 1;
            rotTicker = 1;
            commit = false;
        }

        if (Mathf.Abs(setPosition.x - transform.position.x) > 0.001f || Mathf.Abs(setPosition.z - transform.position.z) > 0.001f)
        {
            // Face East
            if (trackPosition.z > Mathf.Abs(trackPosition.x) && trackPosition.z > 0)
            {
                if (rotY > 280) { rotY -= 360; }
                else if (rotY > 260 && targetDirection == 270) { rotY += TurnWrapper(); }
                targetDirection = 90;
            }

            //Face West
            else if (-trackPosition.z > Mathf.Abs(trackPosition.x) && trackPosition.z < 0)
            {
                if (rotY < 80) { rotY += 360; }
                else if (rotY < 100 && targetDirection == 90) { rotY += TurnWrapper(); }
                targetDirection = 270;
            }

            //Face North
            else if (-trackPosition.x > Mathf.Abs(trackPosition.z) && trackPosition.x < 0)
            {
                if (rotY > 190) { rotY -= 360; }
                else if (rotY > 170 && targetDirection == 180) { rotY -= TurnWrapper(); }
                targetDirection = 0;
            }

            //Face South
            else if (trackPosition.x > Mathf.Abs(trackPosition.z) && trackPosition.x > 0)
            {
                if (!(rotY > 10 && rotY < 350) && targetDirection == 0) { rotY += TurnWrapper(); }
                targetDirection = 180;
            }

            //Player movement
            if (Mathf.Abs(targetDirection - rotY) < 90)
            {
                if (posTicker > 0)
                {
                    if (moveSpeed <= 0)
                    {
                        Debug.Log("Player movement speed must be greater than zero to work properly");
                        posTicker = 0;
                    }
                    else
                    {
                        posTicker++;
                        transform.position = new Vector3
                        (
                             Mathf.Lerp(posX, setPosition.x, posTicker * (moveSpeed / 10000)),
                             0,
                             Mathf.Lerp(posZ, setPosition.z, posTicker * (moveSpeed / 10000))
                        );
                    }
                }
            }
        }
        else
        {
            transform.position = setPosition;
            posTicker = 0;
        }

        //Player rotation & lean
        if (rotTicker > 0 || rotZ > 0.01f)
        {
            rotTicker++;
            if (rotateSpeed <= 0)
            {
                Debug.Log("Player rotation speed must be greater than zero to work properly");
                rotTicker = 0;
            }
            else
            {
                rotTicker++;
                if (Mathf.Abs(playerRotation.y - targetDirection) > 0.001 || rotZ > 0.001)
                {
                    float deltaFloat = Mathf.Abs(deltaPosition.x + deltaPosition.z) / 2;
                    float spin = Mathf.Lerp(rotY, targetDirection, rotTicker * (rotateSpeed / 10000));
                    playerRotation = new Vector3
                    (
                        0,
                        spin,
                        Mathf.Clamp((deltaFloat * targetLean * 5) - targetLean / 10, 0, targetLean) //Lean
                    ) ;
                }
                else
                {
                    playerRotation = new Vector3(0, targetDirection, 0);
                    rotTicker = 0;
                }
            }
            playerChild.eulerAngles = playerRotation;
        }
    }

    //Handles randomized turning left or right for when the player is turning to face the opposite direction
    float TurnWrapper()
    {
        int[] num = new int[2];
        num[0] = 0;
        if (playerRotation.y > 180) { num[1] = -360; }
        else { num[1] = 360; }
        return num[Random.Range(0, 2)];
    }
}