using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverTester : MonoBehaviour
{
    public Transform thing;
    private float countUp;
    private Vector3 targetPosition;
    void Start()
    {
        countUp = 0;
    }

    // Update is called once per frame
    void Update()
    {
        countUp += 0.002f;
        targetPosition.x = Mathf.Sin(countUp * Mathf.PI * 2) * 10;
        targetPosition.z = Mathf.Cos(countUp * Mathf.PI * 2) * 30;
        thing.position = targetPosition;
    }
}
