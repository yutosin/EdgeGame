using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoScript : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject mainMenuCanvasObj;
    private bool mainMenu;

    private void Awake()
    {
        if (mainMenuCanvasObj == null)
        {
            mainMenu = false;
        }
        else
        {
            mainMenu = true;
        }
        videoPlayer.loopPointReached += CheckOver;
    }

    public void ButtonHit()
    {
        StartCoroutine(PlayVideo());
    }

    private IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(.1f);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        
        videoPlayer.Play();
        HidePanel();
    }

    private void CheckOver(UnityEngine.Video.VideoPlayer vp)
    {
        videoPlayer.Stop();
        ShowPanel();
    }

    private void HidePanel()
    {
        if (mainMenu)
        {
            mainMenuCanvasObj.SetActive(false);
        }
        else
        {
            GameManager.SharedInstance.uiManager.allUI.SetActive(false);
        }
    }

    private void ShowPanel()
    {
        if (mainMenu)
        {
            mainMenuCanvasObj.SetActive(true);
        }
        else
        {
            GameManager.SharedInstance.uiManager.allUI.SetActive(true);
        }
    }

}
