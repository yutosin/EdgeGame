using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobonautHandler : MonoBehaviour
{
    [Header("Set In Inspector")]
    public Transform playerModel;
    public bool setPositionFeed;
    public float startSpin;
    public float height;
    public float spinRate;
    public float foreLean;
    public float sideLean;

    [Header("Dynamic Feed")]
    public Vector3 targetPosition;

    public Vector3 oldPosition;
    public float oldRotation;
    private float posX;
    private float curX;
    private float velX;
    private float posZ;
    private float curZ;
    private float velZ;
    private float eulerX;
    public float eulerY;
    private float eulerZ;
    private float spinFactor;
    public float velocity;
    private float deltaAngle;

    void Start()
    {
        oldRotation = playerModel.eulerAngles.y;
        posX = targetPosition.x;
        curX = transform.position.x;
        velX = 0;
        posZ = targetPosition.z;
        curZ = transform.position.z;
        velZ = 0;
        eulerX = 0;
        eulerY = oldRotation;
        eulerZ = 0;
        spinFactor = 0.2f;
        velocity = 0;
        deltaAngle = 0;
    }

    void Update()
    {
        //Position
        if (setPositionFeed)
        {
            posX = targetPosition.x;
            curX = transform.position.x;
            posZ = targetPosition.z;
            curZ = transform.position.z;
            oldPosition = transform.position;
            velocity = Mathf.Sqrt(Mathf.Pow(posZ - oldPosition.z, 2) + Mathf.Pow(posX - oldPosition.x, 2));
            velX = Mathf.Lerp(curX, posX, velocity * Mathf.Clamp((-Mathf.Abs(deltaAngle) + 90) / 90, 0, 1) / 100);
            velZ = Mathf.Lerp(curZ, posZ, velocity * Mathf.Clamp((-Mathf.Abs(deltaAngle) + 90) / 90, 0, 1) / 100);
            transform.position = new Vector3(velX, height, velZ);
            Rotator();
        }
        else
        {
            posX = transform.position.x;
            posZ = transform.position.z;
            velocity = Mathf.Sqrt(Mathf.Pow(posZ - oldPosition.z, 2) + Mathf.Pow(posX - oldPosition.x, 2));
            Rotator();
            oldPosition = transform.position;
        }
    }

    //Rotation
    void Rotator()
    {
        oldRotation = playerModel.eulerAngles.y;
        eulerY = (Mathf.Atan2(oldPosition.x - posX, oldPosition.z - posZ) * 180 / Mathf.PI) + startSpin;
        deltaAngle = oldRotation - eulerY;
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
        if (setPositionFeed)
        {
            if (Mathf.Abs(deltaAngle) > 5)
            {
                if (spinFactor < 1) { spinFactor += 0.01f; }
            }
            else
            {
                if (spinFactor > 0.05f) spinFactor -= 0.01f;
            }
        }
        else { spinFactor = 1; }
        eulerX = sideLean * deltaAngle * velocity * 0.1f;
        eulerZ = velocity * foreLean;
        playerModel.eulerAngles = new Vector3(eulerZ, Mathf.Lerp(oldRotation, eulerY, spinRate * spinFactor), eulerX);
    }
}