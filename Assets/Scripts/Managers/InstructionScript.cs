using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionScript : MonoBehaviour
{
    public GameObject uiPanel;
    public GameObject previousButtonObj;
    public GameObject nextButtonObj;
    public GameObject[] panels;
    public PlayVideoScript[] miniVideos;
    public GameObject videoHolder;
    private bool _playVideo;
    public bool PlayVideo
    {
        get { return (_playVideo); }

        set
        {
            _playVideo = value;
            
            if(value == true)
            {
                miniVideos[_vPos].videoPlayer.Play();
            }
            else
            {
                miniVideos[_vPos].videoPlayer.Stop();
            }
            videoHolder.SetActive(value);
        }
    }
    private int _vPos = 0;
    public int VPos
    {
        get { return(_vPos); }

        set
        {
            miniVideos[_vPos].endLoop = true;
            miniVideos[_vPos].videoPlayer.Stop();
            _vPos = value;
            if(_vPos <= miniVideos.Length)
            {
                miniVideos[_vPos].videoPlayer.Play();
            }
            else
            {
                videoHolder.SetActive(false);
            }
        }
    }
    public int pagePos = 0;
    private int maxPage;

    private void Start()
    {
        maxPage = panels.Length - 1;
        previousButtonObj.SetActive(false);
        videoHolder.SetActive(false);
        if(miniVideos.Length > 0)
        {
            miniVideos[VPos].videoPlayer.Play();
            StartCoroutine(WaitTilPrepared());
        }
    }

    private IEnumerator WaitTilPrepared()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(.5f);
        while (!miniVideos[VPos].videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }

        videoHolder.SetActive(true);
    }

    public void PreviousButtonHit()
    {
        pagePos--;
        if (nextButtonObj.activeSelf == false)
        {
            nextButtonObj.SetActive(true);
        }
        if (pagePos <= 0)
        {
            pagePos = 0;
            previousButtonObj.SetActive(false);
        }
        ShowInstructionPage();
    }

    public void NextButtonHit()
    {
        pagePos++;
        if (previousButtonObj.activeSelf == false)
        {
            previousButtonObj.SetActive(true);
        }
        if (pagePos >= maxPage)
        {
            pagePos = maxPage;
            nextButtonObj.SetActive(false);
        }
        ShowInstructionPage();
    }

    private void ShowInstructionPage()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        panels[pagePos].SetActive(true);
    }

    public void PullUpInstructions()
    {
        uiPanel.SetActive(true);
    }

    public void ReturnToGame()
    {
        uiPanel.SetActive(false);
    }

}
