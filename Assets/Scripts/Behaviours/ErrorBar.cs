using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorBar : MonoBehaviour
{
    [Header("Set")]
    public bool trigger;
    public float scrollSpeed;
    public float duration;
    public string content;

    private bool localTrigger;
    private float startPos;
    private float height;
    private float top;
    private float time;

    void Awake()
    {
        trigger = false;
        height = transform.GetComponent<RectTransform>().rect.height;
        top = 0;
        time = 0;
        startPos = transform.GetComponent<RectTransform>().anchoredPosition.y;
    }

    private void FixedUpdate()
    {
        if (trigger)
        {
            trigger = false;
            time = 0;
            localTrigger = true;
        }
        if (localTrigger)
        {
            if (time == 0) transform.GetComponentInChildren<Text>().text = content;
            time += Time.deltaTime;
            if (top < height && time < duration) top += scrollSpeed;
            else if (time >= duration)
            {
                if (top > 0) top -= scrollSpeed;
                else
                {
                    localTrigger = false;
                    time = 0;
                }
            }
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, startPos + top);
        }
    }
}
