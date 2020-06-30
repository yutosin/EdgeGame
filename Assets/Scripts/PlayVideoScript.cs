using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoScript : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private void Start()
    {
        videoPlayer.loopPointReached += CheckOver;
    }

    public IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(.5f);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        GameManager.SharedInstance.uiManager.allUI.SetActive(false);
        videoPlayer.Play();
    }

    private void CheckOver(UnityEngine.Video.VideoPlayer vp)
    {
        videoPlayer.Stop();
        GameManager.SharedInstance.uiManager.allUI.SetActive(true);
    }

}
