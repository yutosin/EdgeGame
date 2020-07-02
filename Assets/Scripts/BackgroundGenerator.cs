using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    [Header("General")]
    public int mode;
    public Transform cube;
    public Material cubeMat;

    [Header("Set Cloudmode Parameters")]
    public Vector3 cloudMin;
    public Vector3 cloudMax;
    public float speed;
    public int direction;

    private Transform subject;
    private int lockMode;
    private float time;

    void Awake()
    {
        lockMode = mode;
        time = 0;
        if (lockMode == 1)
        {
            for (int c = 0; c < 40; c++)
            {
                Cloudmaker(cube, transform.position + new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 60)));
            }
        }
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (lockMode == 1)
        {
            for (int c = 0; c < transform.childCount; c++)
            {
                subject = transform.GetChild(c);
                subject.position -= new Vector3((direction == 0) ? speed * 0.1f / subject.childCount: 0, 0, (direction == 1) ? speed * 0.1f / subject.childCount: 0);
                if (subject.transform.position.x < transform.position.x - 40 || subject.transform.position.z < transform.position.z - 40) Destroy(subject.gameObject);
            }
            if (time >= 3)
            {
                Cloudmaker(cube, transform.position + new Vector3((direction == 1) ? Random.Range(-40, 60) : 60, 0, (direction == 0) ? Random.Range(-40, 60) : 60));
                time = 0;
            }
        }
    }

    void Cloudmaker(Transform sourceCube, Vector3 start)
    {
        int nimbocount = Random.Range(2, 13);
        GameObject cloud = new GameObject("Cloud");
        cloud.transform.position = start;
        cloud.transform.SetParent(transform);
        for (int n = 0; n < nimbocount; n++)
        {
            Transform piece = Instantiate(sourceCube, cloud.transform);
            piece.localScale = new Vector3(Random.Range(cloudMin.x, cloudMax.x), Random.Range(cloudMin.y, cloudMax.y), Random.Range(cloudMin.z, cloudMax.z));
            piece.localPosition = new Vector3(Random.Range(cloudMin.x, cloudMax.x), Random.Range(cloudMin.y, cloudMax.y), Random.Range(cloudMin.z, cloudMax.z));
            piece.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        }
    }
}