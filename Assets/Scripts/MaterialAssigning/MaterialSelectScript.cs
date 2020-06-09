using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Stick this guy onto an empty gameobject and it will let you assign buttons to go along with materials for use in the level
public class MaterialSelectScript : MonoBehaviour
{
    //This class allows you to have multiple elements assigned to a list, I find it
    // easier to work with in the editor than creating sperate lists
    [System.Serializable]
    public class MaterialButtons
    {
        public GameObject thisButtonObj;
        private Button thisButton;
        public Button ThisButton
        {
            get { return (thisButton); }

            set { thisButton = value; }
        }

        public Material material;
        public int uses = 1;
        public bool useThisLevel = true;

        
        private Text buttonText;
        public Text ButtonText
        {
            get { return (buttonText); }

            set { buttonText = value; }
        }
        private CubeSpawnHolder cSpawn;
        public CubeSpawnHolder CSpawn
        {
            get { return (cSpawn); }

            set { cSpawn = value; }
        }
        private GameObject holdingPanel;
        public GameObject HoldingPanel
        {
            get { return (holdingPanel); }

            set { holdingPanel = value; }
        }

        public void CreateProceduralCube()
        {
            this.uses--;
            this.ButtonText.text = this.uses.ToString();
            this.CSpawn.whatMaterial = this.material;
            this.CSpawn.CreateACube();
            if(this.uses <= 0)
            {
                this.thisButton.interactable = false;
            }
            this.holdingPanel.SetActive(false);
        }

    }

    public CubeSpawnHolder cSpawn;
    public GameObject panelObj;
    private RectTransform panelSpace;
    public float buttonSpacing;
    public List<MaterialButtons> buttonList;
    

    private void Awake()
    {
        panelSpace = panelObj.GetComponent<RectTransform>();
        AssignUnSetVariables();
        SetButtonPositions();
    }

    private void Start()
    {
        //panelObj.SetActive(false);
        //for testing, normally it would start disabled and only show up when you select a face
        panelObj.SetActive(true);
    }

    //Had to make this because I could not in the class initialization itself
    private void AssignUnSetVariables()
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            MaterialButtons button = buttonList[i];
            button.ThisButton = button.thisButtonObj.GetComponent<Button>();
            button.ButtonText = button.ThisButton.GetComponentInChildren<Text>();
            button.HoldingPanel = panelObj;
            button.CSpawn = cSpawn;
            button.ButtonText.text = button.uses.ToString();
        }
    }

    //Wanted to have a function to dynamically place and space buttons used in this level
    //!!!!!!!!!!!
    //For some reason transform is still not relative to panel, need to look into this
    //!!!!!!!!!!!
    private void SetButtonPositions()
    {
        //This accounts for the size of the buttons in case we adjust button sizes
        RectTransform buttonDimensions = buttonList[0].ThisButton.GetComponent<RectTransform>();
        buttonSpacing += buttonDimensions.sizeDelta.x;

        float buttonX = buttonSpacing;
        float buttonY = panelSpace.sizeDelta.y - buttonSpacing;
        float xMax = panelSpace.sizeDelta.x - buttonSpacing;

        for (int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i].useThisLevel == true)
            {
                if(buttonX > xMax)
                {
                    buttonX = buttonSpacing;
                    buttonY -= buttonSpacing;
                    
                }
                GameObject buttonObj = Instantiate(buttonList[i].thisButtonObj) as GameObject;
                buttonObj.transform.SetParent(panelSpace.transform, true);
                buttonObj.transform.localPosition = Vector3.zero;

                Button button = buttonObj.GetComponent<Button>();
                buttonList[i].ThisButton = button;
                button.interactable = true;
                button.onClick.AddListener(buttonList[i].CreateProceduralCube);
                
                Debug.Log("Xpos is " + buttonX.ToString() + " and YPos is " + buttonY.ToString());

                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonX, buttonY);

                buttonX += buttonSpacing;
            }
        }

    }

}
