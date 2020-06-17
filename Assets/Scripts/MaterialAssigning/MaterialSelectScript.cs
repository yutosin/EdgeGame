///////////////////////////////////////////
///Work to do list:
///     Material and ability assigning seems to not be working, not sure why
///     Making a system that will flexibly assign different abilities, want to avoid switch statements
///////////////////////////////////////////

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

        private Face face;
        public Face Face
        {
            get { return (face); }

            set { face = value; }
        }
        private Text buttonText;
        public Text ButtonText
        {
            get { return (buttonText); }

            set { buttonText = value; }
        }
        private GameObject holdingPanel;
        public GameObject HoldingPanel
        {
            get { return (holdingPanel); }

            set { holdingPanel = value; }
        }
        //I hate having this here, but reference the comment above AssignAbility() to see why I have it here
        private MaterialSelectScript _mScript;
        public MaterialSelectScript MScript
        {
            get { return (_mScript); }

            set { _mScript = value; }
        }

        public void AssignColorAndAbility()
        {
            uses--;
            ButtonText.text = uses.ToString();
            if(uses <= 0)
            {
                thisButton.interactable = false;
            }
            //Attach a script to be made here to the face selected that will have the associated ability
            face._rend.material = material; //Change this later to work with attached face script
            MScript.AssignAbility();

            holdingPanel.SetActive(false);
        }

    }

    public GameObject panelObj;
    private RectTransform panelSpace;
    public float buttonSpacing;//Adjust in inspector as you please, sets space between buttons and edge of panel
    public List<MaterialButtons> buttonList;//Also set in inspector for prefab and in scene if you want different setups for levels

    //Made this a method so that when the value is changed the buttons reference the right face
    private Face _selectedFace;
    public Face SelectedFace
    {
        get { return (_selectedFace); }
        set
        {
            _selectedFace = value;
            for (int i = 0; i < buttonList.Count; i++)
            {
                buttonList[i].Face = _selectedFace;
            }
            //Debug.Log("A face has been selected");
        }
    }

    private void Awake()
    {
        panelSpace = panelObj.GetComponent<RectTransform>();//I didn't like setting it manually in the inspector
        AssignUnSetVariables();//Got to make sure variables reliant on other variables actually get set
        SetButtonPositions();
    }

    private void Start()
    {
        panelObj.SetActive(false);
        //for testing, normally it would start disabled and only show up when you select a face
        //panelObj.SetActive(true);
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
            button.MScript = this;
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
                button.onClick.AddListener(buttonList[i].AssignColorAndAbility);

                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonX, buttonY);
                buttonX += buttonSpacing;
            }
        }
    }

    //This is just for testing, I want this handled within the MaterialButtons Class
    //Also for some reason this does not work when placed in the MaterialButtons Class, need to find out why
    private void AssignAbility()
    {
        _selectedFace.Ability = gameObject.AddComponent<ElevatorAbility>();
    }

}
