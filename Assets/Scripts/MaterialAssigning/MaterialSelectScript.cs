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
        //Methods are so that they do not serialize and take up unnecessary space in inspector
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

        //This get assigned to OnClick when buttons spawn, but the updating of text seems to not be working
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

    public CubeSpawnHolder cSpawn;//So buttons can spawn cubes
    public GameObject panelObj;
    private RectTransform panelSpace;
    public float buttonSpacing;//Adjust in inspector as you please, sets space between buttons and edge of panel
    public List<MaterialButtons> buttonList;//Also set in inspector for prefab and in scene if you want different setups for levels
    

    private void Awake()
    {
        panelSpace = panelObj.GetComponent<RectTransform>();//I didn't like setting it manually in the inspector
        AssignUnSetVariables();//Got to make sure variables reliant on other variables actually get set
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
            MaterialButtons button = buttonList[i]; //making this iteration the button variable is just easier to read
            button.ThisButton = button.thisButtonObj.GetComponent<Button>(); //gets the actual button component to set interactible
            button.ButtonText = button.ThisButton.GetComponentInChildren<Text>();
            button.HoldingPanel = panelObj;//this is needed so that the function can hide the panel the button is nested in
            button.CSpawn = cSpawn;//this is so that the function can spawn cubes
            button.ButtonText.text = button.uses.ToString();
        }
    }

    //Sets positions dynamically
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
            //So that only selected buttons are instantiated
            if (buttonList[i].useThisLevel == true)
            {
                //When it runs out of horispace will reset and move down one
                if(buttonX > xMax)
                {
                    buttonX = buttonSpacing;
                    buttonY -= buttonSpacing;
                }
                GameObject buttonObj = Instantiate(buttonList[i].thisButtonObj) as GameObject;
                buttonObj.transform.SetParent(panelSpace.transform, true);

                Button button = buttonObj.GetComponent<Button>();
                //This is important, as it makes the class refer to the instantiated button rather than the prefab
                buttonList[i].ThisButton = button;
                button.interactable = true;
                //This assigns the function for the associated class to OnClick()
                button.onClick.AddListener(buttonList[i].CreateProceduralCube);

                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonX, buttonY);
                buttonX += buttonSpacing;
            }
        }
    }

}
