using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavyLandscape : MonoBehaviour
{
    public Material mat;
    public GameObject cube;
    public int xSize;
    public int ySize;
    public int sizeFactor;
    public int spreadFactor;

    void Awake()
    {
        for (int y = 0; y < ySize * spreadFactor; y += spreadFactor)
        {
            for (int x = 0; x < xSize * spreadFactor; x += spreadFactor)
            {
                float height = Mathf.Pow((float)x / (xSize / 2), 2) + Mathf.Pow((float)y / (ySize / 2), 2) + Random.Range(1,8);
                Transform cubeToParse = cube.transform;
                cubeToParse.position = new Vector3(transform.position.x - (x - (xSize / 2) + 1), transform.position.y + height + 0.5f, transform.position.z - (y - (ySize / 2) + 1));
                cubeToParse.localScale = new Vector3(sizeFactor, height * 2 + 1, sizeFactor);
                Instantiate(cubeToParse, transform);
            }
        }
    }
}