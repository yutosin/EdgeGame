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
    public int pagePos = 0;
    private int maxPage;

    private void Start()
    {
        maxPage = panels.Length - 1;
        previousButtonObj.SetActive(false);
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
