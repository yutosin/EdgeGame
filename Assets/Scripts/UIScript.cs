using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIScript : MonoBehaviour
{
    public GameObject InstructionsPanel;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("PrototypingScene");
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
