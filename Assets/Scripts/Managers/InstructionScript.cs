using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InstructionScript : MonoBehaviour
{
    public GameObject uiPanel;
    public GameObject previousButtonObj;
    public GameObject nextButtonObj;
    public GameObject[] panels;
    public VideoClass[] videos;
    private bool _playVideo;
    public bool PlayVideo
    {
        get { return (_playVideo); }

        set
        {
            _playVideo = value;

            if(value == true)
            {
                videos[_vPos].videoPlayer.Play();
            }
            else
            {
                videos[_vPos].videoPlayer.Stop();
            }
            videos[_vPos].projectTo.SetActive(value);
        }
    }
    private int _vPos = 0;
    public int VPos
    {
        get { return(_vPos); }

        set
        {
            videos[_vPos].videoPlayer.Stop();
            _vPos = value;
            if(_vPos < videos.Length)
            {
                videos[_vPos].videoPlayer.Play();
            }
            else
            {
                videos[_vPos].projectTo.SetActive(false);
            }
        }
    }
    public int pagePos = 0;
    private int maxPage;

    private void Start()
    {
        maxPage = panels.Length - 1;
        previousButtonObj.SetActive(false);
        videos[_vPos].projectTo.SetActive(false);
        if (videos.Length > 0)
        {
            videos[VPos].videoPlayer.Play();
            StartCoroutine(WaitTilPrepared());
        }
        StartingValuesSet();
    }

    private IEnumerator WaitTilPrepared()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(.5f);
        while (!videos[VPos].videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }

        videos[_vPos].projectTo.SetActive(true);
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
        if (videos[_vPos].projectTo.activeInHierarchy)
            videos[_vPos].projectTo.SetActive(false);
    }

    public void ReturnToGame()
    {
        uiPanel.SetActive(false);
        videos[_vPos].projectTo.SetActive(true);
    }

    public void AccessDleayCoroutine(VideoClass vC)
    {
        StartCoroutine(vC.WaitTilPrepared());
    }

    private void StartingValuesSet()
    {
        for (int i = 0; i < videos.Length; i++)
        {
            videos[i].iScript = this;
            videos[i].StartingValues();
            RawImage raw = videos[i].projectTo.GetComponent<RawImage>();

        }
    }

    [System.Serializable]
    public class VideoClass
    {
        private InstructionScript _iScript;
        public InstructionScript iScript

        {
            get { return (_iScript); }

            set { _iScript = value; }
        }

        public string VideoName;
        public VideoPlayer videoPlayer;
        public GameObject projectTo, playButtonObj, stopButtonObj;
        private RawImage raw;
        private Button _playButton, _stopButton;
        public Button playButton
        {
            get { return (_playButton); }

            set { _playButton = value; }
        }
        public Button stopButton
        {
            get { return (_stopButton); }

            set { _stopButton = value; }
        }

        public void StartingValues()
        {
            raw = projectTo.GetComponent<RawImage>();
            playButton = playButtonObj.GetComponent<Button>();
            stopButton = stopButtonObj.GetComponent<Button>();

            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, VideoName + ".mp4");
            videoPlayer.loopPointReached += OnVideoEnd;

            playButton.onClick.AddListener(PlayVideo);
            stopButton.onClick.AddListener(StopVideo);
            videoPlayer.Prepare();
            StopVideo();
            Color transparent = raw.color;
            transparent.a = 0;
            raw.color = transparent;
        }

        private void PlayVideo()
        {
            videoPlayer.Play();
            iScript.AccessDleayCoroutine(this);
            playButtonObj.SetActive(false);
            stopButtonObj.SetActive(true);
            Color filledColor = raw.color;
            filledColor.a = 1;
            raw.color = filledColor;
        }

        private void StopVideo()
        {
            videoPlayer.Stop();
            playButtonObj.SetActive(true);
            stopButtonObj.SetActive(false);
            projectTo.SetActive(false);
        }

        private void OnVideoEnd(UnityEngine.Video.VideoPlayer vp)
        {
            StopVideo();
        }

        public IEnumerator WaitTilPrepared()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(.1f);
            while (!videoPlayer.isPrepared)
            {
                yield return waitForSeconds;
                break;
            }
            projectTo.SetActive(true);
        }

    }

}
