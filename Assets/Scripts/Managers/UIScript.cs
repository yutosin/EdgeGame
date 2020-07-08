using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public GameObject allUI;
    public GameObject InstructionsPanel;
    public GameObject FaceAbilitiesPanel;
    public GameObject MainPanel;
    public GameObject FeedbackPanel;
    public GameObject CreditsPanel;
    public Text FeedackText;
    public MaterialSelectScript mScript;
    public Button GamePlayModeButton;
    public Button DrawModeButton;
    
    // Start is called before the first frame update
    void Start()
    {
        FeedackText.text = "";
        FeedbackPanel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuALT");
    }

    public void ShowInstructionsPanel()
    {
        if (MainPanel && MainPanel.activeInHierarchy)
            MainPanel.SetActive(false);
        if (FaceAbilitiesPanel && FaceAbilitiesPanel.activeInHierarchy)
            FaceAbilitiesPanel.SetActive(false);
        InstructionsPanel.SetActive(true);
    }

    public void ShowCreditsPanel()
    {
        if (MainPanel && MainPanel.activeInHierarchy)
            MainPanel.SetActive(false);
        if (FaceAbilitiesPanel && FaceAbilitiesPanel.activeInHierarchy)
            FaceAbilitiesPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void ShowFaceAbilitiesPanel()
    {
        if (InstructionsPanel && InstructionsPanel.activeInHierarchy)
            InstructionsPanel.SetActive(false);
        FaceAbilitiesPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMain()
    {
        if (InstructionsPanel && InstructionsPanel.activeInHierarchy)
            InstructionsPanel.SetActive(false);
        if (CreditsPanel && CreditsPanel.activeInHierarchy)
            CreditsPanel.SetActive(false);
        if (FaceAbilitiesPanel && FaceAbilitiesPanel.activeInHierarchy)
            FaceAbilitiesPanel.SetActive(false);
        MainPanel.SetActive(true);
    }

    public void SwitchToPlayMode()
    {
        GameManager.SharedInstance.PlayMode = true;
        DrawModeButton.interactable = true;
        GamePlayModeButton.interactable = false;
        mScript.DeselectFaces();
    }

    public void SwitchToDrawMode()
    {
        GameManager.SharedInstance.PlayMode = false;
        DrawModeButton.interactable = false;
        GamePlayModeButton.interactable = true;
    }

    public void ReloadLevel()
    {
        GameManager.SharedInstance.levelManager.LoadLevel();
    }
}
