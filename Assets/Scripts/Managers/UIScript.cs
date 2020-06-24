using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public GameObject InstructionsPanel;
    public GameObject FeedbackPanel;
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
        SceneManager.LoadScene("PrototypingScene_Restructure");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowInstructionsPanel()
    {
        InstructionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMain()
    {
        InstructionsPanel.SetActive(false);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
