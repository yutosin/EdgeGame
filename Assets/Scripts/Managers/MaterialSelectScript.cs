///////////////////////////////////////////
///Work to do list:
///     Making a system that will flexibly assign different abilities, want to avoid switch statements
///     
///////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
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

        private Material material;

        public string abilityName;
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
            string needsFlatFeedback = "This ability cannot be placed on a wall";
            switch (abilityName) //Made this a switch statement for now, I don't like it though
            {
                case "Elevator":
                    if (IsFlatSurface())
                    {
                        AssignElevator();
                    }
                    else
                    {
                        _mScript.GiveFeedback(needsFlatFeedback);
                    }
                    break;

                case "Teleport": //My soul is condemed the the fires of hell for this if statement in a switch statement
                    if (IsFlatSurface())
                    {
                        AssignTP();
                    }
                    else
                    {
                        _mScript.GiveFeedback(needsFlatFeedback);
                    }
                    break;

                case "XMoving":
                    if (IsFlatSurface())
                    {
                        AssignMoving(true);
                    }
                    else
                    {
                        _mScript.GiveFeedback(needsFlatFeedback);
                    }
                    break;

                case "ZMoving":
                    if (IsFlatSurface())
                    {
                        AssignMoving(false);
                    }
                    else
                    {
                        _mScript.GiveFeedback(needsFlatFeedback);
                    }
                    break;

                case "Extrude":
                    if (!IsFlatSurface())
                    {
                        AssignExtrude();
                    }
                    else
                    {
                        string needsWallFeedback = "This ability must be placed on a wall";
                        _mScript.GiveFeedback(needsWallFeedback);
                    }
                    break;

                default:
                    Debug.LogError("That is not an ability");
                    break;
            }

        }

        private void AssignElevator()
        {
            face.Ability = face.gameObject.AddComponent<ElevatorAbility>();
            SetMaterial();
            UpdateUses();
        }

        private void AssignTP()
        {
            face.Ability = face.gameObject.AddComponent<TeleportAbility>();

            TeleportAbility tp = face.GetComponent<TeleportAbility>();
            tp.thisPos = tp.FindCenter(face);

            if (_mScript.tpFaces[0] == null)
            {
                _mScript.tpFaces[0] = tp;
            }
            else if (_mScript.tpFaces[1] == null)
            {
                _mScript.tpFaces[1] = tp;

                _mScript.tpFaces[0].OtherPos = _mScript.tpFaces[1].thisPos;
                _mScript.tpFaces[1].OtherPos = _mScript.tpFaces[0].thisPos;
            }

            SetMaterial();
            UpdateUses();
        }

        private void AssignMoving(bool isMotionX)
        {
            face.Ability = face.gameObject.AddComponent<LateralMovingAbility>();

            LateralMovingAbility XMove = face.GetComponent<LateralMovingAbility>();

            SetMaterial();
            UpdateUses();
            XMove.SetStartingConditions(face, isMotionX);
        }

        private void AssignExtrude()
        {
            face.Ability = face.gameObject.AddComponent<ExtrudeFaceAbility>();

            ExtrudeFaceAbility extrude = face.GetComponent<ExtrudeFaceAbility>();

            SetMaterial();
            UpdateUses();
            extrude.SetStartingConditions(face);
        }

        private void SetMaterial()
        {
            material = new Material(Shader.Find("Unlit/ColorZAlways"));
            material.color = ThisButton.GetComponent<Image>().color;
            material.renderQueue = 2005;

            face._rend.material = material;
        }

        private void UpdateUses()
        {
            uses--;
            ButtonText.text = uses.ToString();
            if (uses <= 0)
            {
                thisButton.interactable = false;
            }
            holdingPanel.SetActive(false);
        }

        private bool IsFlatSurface()
        {
            bool isFlat = true;
            float yValue = Mathf.Round(face.Vertices[0].y);
            
            for (int i = 1; i < face.Vertices.Length; i++)
            {
                if (yValue != Mathf.Round(face.Vertices[i].y))
                {
                    isFlat = false;
                    break;
                }
            }
            return (isFlat);
        }

    }

    public GameObject cubePrefab;
    public UIScript uiScript;
    public GameObject panelObj;
    public Button xButtonObj;
    private RectTransform panelSpace;
    public float buttonSpacing;//Adjust in inspector as you please, sets space between buttons and edge of panel
    public List<MaterialButtons> buttonList;//Also set in inspector for prefab and in scene if you want different setups for levels
    [HideInInspector]
    public TeleportAbility[] tpFaces = new TeleportAbility[2];

    //Made this a method so that when the value is changed the buttons reference the right face
    private Face _selectedFace, _lastFace;
    //This is a large method as it also handles what happens when trying to change the selected face
    //If this is bad coding practice let me know
    public Face SelectedFace
    {
        get { return (_selectedFace); }

        set
        {
            bool goodSize = IsSmallSquare(value);
            //I put this in front because for some reason it is able to select a new face and say it did not
            //This is likely due to overlapping faces, so we don't want an Else If statment after
            if (!goodSize)
            {
                string feedback = "You can only select faces that are 1x1 squares";
                GiveFeedback(feedback);
            }
            if (goodSize && !IsAbilityAssigned(value))
            {
                PutAwayFeedback();
                panelObj.SetActive(true);
                if((_selectedFace != null))
                {
                    _lastFace = _selectedFace;
                    if (!IsAbilityAssigned(_lastFace))
                    {
                        _lastFace._rend.material = value._defaultMat;
                    }
                }

                _selectedFace = value;
                _selectedFace._rend.material = value._selectedMat;
                for (int i = 0; i < buttonList.Count; i++)
                {
                    buttonList[i].Face = _selectedFace;
                }
            }
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
        RectTransform xButtonDimensions = xButtonObj.GetComponent<RectTransform>();

        buttonSpacing += buttonDimensions.sizeDelta.x;

        float buttonX = buttonSpacing;
        float buttonY = panelSpace.sizeDelta.y - buttonSpacing - xButtonDimensions.sizeDelta.y;
        float xMax = panelSpace.sizeDelta.x - buttonSpacing;

        for (int i = 0; i < buttonList.Count; i++)
        {
            GameObject buttonObj = buttonList[i].thisButtonObj;
            buttonObj.transform.SetParent(panelSpace.transform, true);
            //So that only selected buttons are instantiated
            if (buttonList[i].useThisLevel == true)
            {
                //When it runs out of horispace will reset and move down one
                if(buttonX > xMax)
                {
                    buttonX = buttonSpacing;
                    buttonY -= buttonSpacing;
                }

                Button button = buttonObj.GetComponent<Button>();
                //This is important, as it makes the class refer to the instantiated button rather than the prefab
                buttonList[i].ThisButton = button;
                button.interactable = true;
                buttonList[i].ButtonText = buttonList[i].ThisButton.GetComponentInChildren<Text>(); //Had to move the text grabbing here
                //This assigns the function for the associated class to OnClick()
                button.onClick.AddListener(buttonList[i].AssignColorAndAbility);

                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonX, buttonY);
                buttonX += buttonSpacing;
            }
            else
            {
                buttonObj.transform.position = new Vector2(10000f, 10000f);
            }
        }
    }

    private bool IsSmallSquare(Face face)
    {
        bool isSquare = true;
        float xValue = face.Vertices[0].x;
        float yValue = face.Vertices[0].y;
        float zValue = face.Vertices[0].z;
        float maxDistance = 1;

        for (int i = 1; i < face.Vertices.Length; i++)
        {
            float xGap = Mathf.Round(Mathf.Abs(xValue - face.Vertices[i].x));
            float yGap = Mathf.Round(Mathf.Abs(yValue - face.Vertices[i].y));
            float zGap = Mathf.Round(Mathf.Abs(zValue - face.Vertices[i].z));
            if ((xGap > maxDistance) || (yGap > maxDistance) || (zGap > maxDistance))
            {
                isSquare = false;
                break;
            }
        }

        return (isSquare);
    }

    private bool IsAbilityAssigned(Face face)
    {
        bool assigned = false;
        if(face.Ability != null)
        {
            assigned = true;
        }

        return (assigned);
    }

    public void DeselectFaces()
    {
        if(_lastFace != null)
        {
            if(_lastFace.Ability == null)
            {
                _lastFace._rend.material = _lastFace._defaultMat;
            }
            _lastFace = null;
        }

        if (_selectedFace != null)
        {
            if (_selectedFace.Ability == null)
            {
                _selectedFace._rend.material = _selectedFace._defaultMat;
            }
            _selectedFace = null;
        }
        panelObj.SetActive(false);
    }

    private void GiveFeedback(string feedback)
    {
        uiScript.FeedbackPanel.SetActive(true);
        uiScript.FeedackText.text = feedback;
        CancelInvoke("PutAwayFeedback");
        Invoke("PutAwayFeedback", 2.0f);
    }

    private void PutAwayFeedback()
    {
        uiScript.FeedackText.text = "";
        uiScript.FeedbackPanel.SetActive(false);
    }

}
