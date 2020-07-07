using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    [Header("General")]
    public int mode;
    public Transform cube;
    public Material cubeMat;
    public GameObject warpStars;

    [Header("Set Cloudmode Parameters")]
    public Vector3 cloudMin;
    public Vector3 cloudMax;
    public Color cloudColor;
    public float speed;

    private Transform subject;
    private GameObject warpStarsInst;
    private int lockMode;
    private float time;

    void Awake()
    {
        lockMode = mode;
        time = 0;
        if (lockMode == 0)
        {
            Destroy(gameObject);
        }
        else if (lockMode == 1)
        {
            name = "Background_CloudsX";
            for (int c = 0; c < 40; c++)
            {
                Cloudmaker(cube, transform.position + new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 60)));
            }
        }
        else if (lockMode == 2)
        {
            name = "Background_CloudsY";
            for (int c = 0; c < 40; c++)
            {
                Cloudmaker(cube, transform.position + new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 60)));
            }
        }
        else if (lockMode == 3)
        {
            name = "Background_Warp";
            warpStarsInst = Instantiate(warpStars, transform);
            warpStarsInst.transform.position = new Vector3(0, -40, 0);
            warpStarsInst.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.color = new Color(cloudColor.r, cloudColor.g, cloudColor.b, 1);
        }
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (lockMode == 0)
        {
        }
        else if (lockMode == 1)
        {
            for (int c = 0; c < transform.childCount; c++)
            {
                subject = transform.GetChild(c);
                subject.position -= new Vector3(speed * 0.1f / subject.childCount, 0, 0);
                if (subject.transform.position.x < transform.position.x - 40 || subject.transform.position.z < transform.position.z - 40) Destroy(subject.gameObject);
            }
            if (time >= 3)
            {
                Cloudmaker(cube, transform.position + new Vector3(60, 0, Random.Range(-40, 60)));
                time = 0;
            }
        }
        else if (lockMode == 2)
        {
            for (int c = 0; c < transform.childCount; c++)
            {
                subject = transform.GetChild(c);
                subject.position -= new Vector3(0, 0, speed * 0.1f / subject.childCount);
                if (subject.transform.position.x < transform.position.x - 40 || subject.transform.position.z < transform.position.z - 40) Destroy(subject.gameObject);
            }
            if (time >= 3)
            {
                Cloudmaker(cube, transform.position + new Vector3(Random.Range(-40, 60), 0, 60));
                time = 0;
            }
        }
        else if (lockMode == 3)
        {
            warpStarsInst.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.color = new Color(cloudColor.r, cloudColor.g, cloudColor.b, 1);
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
            piece.GetComponent<Renderer>().material.color = new Color(cloudColor.r, cloudColor.g, cloudColor.b, 0.2f);
            piece.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Transparent Colored");
        }
    }
}