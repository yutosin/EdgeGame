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
        private int listPos;
        public int ListPos
        {
            get { return (listPos); }

            set { listPos = value; }
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

        /// <WHYWONTYOUASSIGN>
        /// 
        /// For some reason this function is not attaching when spawning buttons don't know why
        /// See last line in AssignUnSetVariables, that is prob issue
        /// 
        /// </WHYGODWHY>
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
            button.ListPos = i;
            button.CSpawn = cSpawn;
            button.ButtonText.text = button.uses.ToString();
            button.ThisButton.onClick.AddListener(button.CreateProceduralCube);
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

        Vector3 buttonPos = new Vector3(buttonX, buttonY);
        for (int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i].useThisLevel)
            {
                if(buttonPos.x > xMax)
                {
                    buttonPos.x = buttonSpacing * 2;
                    buttonPos.y -= buttonSpacing;
                }
                GameObject newButton = Instantiate(buttonList[i].thisButtonObj) as GameObject;
                newButton.transform.SetParent(panelObj.transform, true);
                newButton.transform.position = buttonPos;
                buttonPos.x += buttonSpacing;
            }
        }

    }

}
