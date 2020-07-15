using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Jsonator : MonoBehaviour
{
    [Header("MainCamera")]
    public Camera mainCamera;

    //Set the maximum allowed size of the grid.
    [Header("General Editor")]
    public RectTransform canvas;
    public RectTransform wholePanel;
    public Vector3 gridSize;
    public Transform backgroundGenerator;
    public RectTransform errorBar;

    public Transform selectCubeModel;
    public float selectorCubeSize;
    public float selectorCubeDistance;
    public Color selectorCubeColor;

    public Transform startModel;
    public float startModelSize;
    public Color startModelColor;

    public Transform goalModel;
    public float goalModelSize;
    public Color goalModelColor;

    //The general list of materials to be added with the inspector.
    [Header("Paint Materials")]
    public Material[] materials;

    //Switch to this material in Silhouette Mode.
    [Header("Silhouette Material")]
    public Material silhouette;

    // Tiles that will show up if there is an error loading the set material.
    [Header("Developer Materials")]
    public Material developerTop;
    public Material developerRight;
    public Material developerLeft;

    [Header("Save Path")]
    public string path;

    [Header("UI Elements")]
    public Slider modeSet;
    public Scrollbar silhouetteSwitch;
    public Scrollbar HSVSwitch;
    public Scrollbar abilityElevator;
    public Scrollbar abilityTeleport;
    public Scrollbar abilityMoveX;
    public Scrollbar abilityMoveZ;
    public Scrollbar abilityExtrude;
    public Text elevatorCount;
    public Text teleportCount;
    public Text moveXCount;
    public Text moveZCount;
    public Text extrudeCount;
    public InputField saveName;
    public RectTransform fileContent;
    public RectTransform fileTemplate;
    public RectTransform matContent;
    public RectTransform matTemplate;
    public RectTransform matView;
    public RectTransform BackgroundColorDisplay;
    public RectTransform cloudColorDisplay;
    public RectTransform SilhouetteColorDisplay;
    public Slider BGColorR;
    public Slider BGColorG;
    public Slider BGColorB;
    public Slider CloudColorR;
    public Slider CloudColorG;
    public Slider CloudColorB;
    public Slider SilhouetteColorR;
    public Slider SilhouetteColorG;
    public Slider SilhouetteColorB;

    private Transform backgroundInstance;
    private Transform cubeXNeg;
    private Transform cubeXPos;
    private Transform cubeYNeg;
    private Transform cubeYPos;
    private Transform cubeZNeg;
    private Transform cubeZPos;
    private Transform startInd;
    private Transform goalInd;
    private bool paintMode;
    private bool selectingStart;
    private bool silhouetteMode;
    private bool HSVMode;
    private bool checkColor;
    private bool checkBackground;
    private int modeCheck;
    private float silhouetteVal;
    private float HSVVal;
    private float abElevVal;
    private float abTeleVal;
    private float abMovXVal;
    private float abMovZVal;
    private float abExtrVal;
    private Material[] displayMaterials;
    private Material selectedMaterial;
    private Vector3 camPos;
    private Vector3 saveCamPos;
    private Vector3 startPoint;
    private Vector3 goalPoint;
    private Vector3 selectCube;
    private Color backgroundColor;
    private Color cloudColor;
    private Color silhouetteColor;

    [SerializeField]
    private Cube[] cubeData;
    private Tile[] tileData;
    private Grid _currentLevel;

    void Start()
    {
        //General setup.
        selectingStart = false;
        silhouetteMode = false;
        HSVMode = false;
        checkColor = false;
        BackgroundBuilder(0);
        silhouetteVal = 0;
        HSVVal = 0;
        if (!GameManager.SharedInstance.InLevelEditor) return;

        //Set sliders.
        modeSet.value = 0;
        abilityElevator.value = 0;
        abilityTeleport.value = 0;
        abilityMoveX.value = 0;
        abilityMoveZ.value = 0;
        abilityExtrude.value = 0;
        SetColor(new Color(0.5f, 0.55f, 0.6f), BGColorR, BGColorG, BGColorB);
        SetColor(new Color(0, 0, 0), CloudColorR, CloudColorG, CloudColorB);
        SetColor(new Color(0, 0, 0), SilhouetteColorR, SilhouetteColorG, SilhouetteColorB);

        //Set up Materials array.
        RectTransform[] matButton = new RectTransform[materials.Length];
        matContent.GetComponent<RectTransform>().sizeDelta = new Vector2(matTemplate.rect.width * matButton.Length + (matButton.Length * 10) + 10, 0);
        displayMaterials = new Material[materials.Length];
        for (int m = 0; m < matButton.Length; m++)
        {
            displayMaterials[m] = new Material(materials[m]);
            displayMaterials[m].shader = Shader.Find("UI/Unlit/Detail");
            matButton[m] = Instantiate(matTemplate, matContent);
            matButton[m].gameObject.SetActive(true);
            matButton[m].GetComponent<RectTransform>().anchoredPosition = new Vector2(m * (matTemplate.rect.width + 10) + (matTemplate.rect.width / 2 + 10), 0);
            matButton[m].name = m.ToString();
            matButton[m].GetComponent<Image>().material = displayMaterials[m];
        }

        //Populate start settings.
        selectCube = new Vector3(1, 1, 1);
        camPos = new Vector3(0, 0, 0);
        modeCheck = 0;
        abElevVal = 0;
        abTeleVal = 0;
        abMovXVal = 0;
        abMovZVal = 0;
        abExtrVal = 0;

        //Initiate.
        OnRefreshButton();
        OnNewButton();
        if (PlayerPrefs.GetString("editorLevel", "") != "")
        {
            LoadEditorLevel(PlayerPrefs.GetString("editorLevel"));
            PlayerPrefs.SetString("editorLevel", "");
        }
    }

    void Update()
    {
        //Externally disable level editor mode via GameManager.
        if (!GameManager.SharedInstance.InLevelEditor)
            return;

        //Manage background settings
        if (checkBackground)
        {
            if (backgroundInstance != null) Destroy(backgroundInstance.gameObject);
            BackgroundBuilder(modeSet.value);
            if (modeCheck == 1 || modeCheck == 2) CloudColorer(backgroundInstance, cloudColor);
            checkBackground = false;
        }

        //Manage transition between RGB and HSV color modes.
        if (HSVVal != HSVSwitch.value)
        {
            HSVMode = (HSVSwitch.value == 1) ? true : false;
            HSVVal = (HSVVal == 1) ? 0 : 1;
            if (HSVMode)
            {
                RGBToHSV(backgroundColor, BGColorR, BGColorG, BGColorB);
                RGBToHSV(cloudColor, CloudColorR, CloudColorG, CloudColorB);
                RGBToHSV(silhouetteColor, SilhouetteColorR, SilhouetteColorG, SilhouetteColorB);
            }
            else
            {
                HSVToRGB(BGColorR, BGColorG, BGColorB);
                HSVToRGB(CloudColorR, CloudColorG, CloudColorB);
                HSVToRGB(SilhouetteColorR, SilhouetteColorG, SilhouetteColorB);
            }
        }

        //Apply colors
        if (checkColor)
        {
            if (HSVMode)
            {
                backgroundColor = Color.HSVToRGB(BGColorR.value, BGColorG.value, BGColorB.value);
                cloudColor = Color.HSVToRGB(CloudColorR.value, CloudColorG.value, CloudColorB.value);
                silhouetteColor = Color.HSVToRGB(SilhouetteColorR.value, SilhouetteColorG.value, SilhouetteColorB.value);
            }
            else
            {
                backgroundColor = new Color(BGColorR.value, BGColorG.value, BGColorB.value, 1);
                cloudColor = new Color(CloudColorR.value, CloudColorG.value, CloudColorB.value, 1);
                silhouetteColor = new Color(SilhouetteColorR.value, SilhouetteColorG.value, SilhouetteColorB.value, 1);
            }
            BackgroundColorDisplay.GetComponent<Image>().color = new Color(backgroundColor.r * 0.9f, backgroundColor.g * 0.9f, backgroundColor.b * 0.9f);
            cloudColorDisplay.GetComponent<Image>().color = new Color(cloudColor.r * 0.9f, cloudColor.g * 0.9f, cloudColor.b * 0.9f);
            SilhouetteColorDisplay.GetComponent<Image>().color = new Color(silhouetteColor.r * 0.9f, silhouetteColor.g * 0.9f, silhouetteColor.b * 0.9f);
            mainCamera.backgroundColor = backgroundColor;
            if (modeCheck == 1 || modeCheck == 2 || modeCheck == 4) CloudColorer(backgroundInstance, cloudColor);
            if (backgroundInstance != null) backgroundInstance.GetComponent<BackgroundGenerator>().cloudColor = cloudColor;
            silhouette.color = silhouetteColor;
            checkColor = false;
        }

        //Manage ability settings
        abElevVal = abilityElevator.value * 4;
        abTeleVal = abilityTeleport.value * 2;
        abMovXVal = abilityMoveX.value * 4;
        abMovZVal = abilityMoveZ.value * 4;
        abExtrVal = abilityExtrude.value * 4;
        elevatorCount.text = ((int)abElevVal).ToString();
        teleportCount.text = ((int)abTeleVal).ToString();
        moveXCount.text = ((int)abMovXVal).ToString();
        moveZCount.text = ((int)abMovZVal).ToString();
        extrudeCount.text = ((int)abExtrVal).ToString();

        //Silhouette mode determination
        if (silhouetteVal != silhouetteSwitch.value)
        {
            silhouetteMode = (silhouetteSwitch.value == 1) ? true : false;
            if (silhouetteMode)
            {
                for (int l = 0; l < transform.childCount; l++)
                {
                    for (int c = 0; c < transform.GetChild(l).childCount; c++)
                    {
                        Transform child = transform.GetChild(l).GetChild(c);
                        if (child.name.Contains("Tile")) child.GetComponent<Renderer>().material = silhouette;
                    }
                }
            }
            else
            {
                for (int l = 0; l < transform.childCount; l++)
                {
                    for (int c = 0; c < transform.GetChild(l).childCount; c++)
                    {
                        Transform tileToScrub = transform.GetChild(l).GetChild(c);
                        string[] unsilMatParse = tileToScrub.name.Split('_');
                        if (unsilMatParse[0] == "Tile")
                        {
                            if (unsilMatParse[2] == developerLeft.name) tileToScrub.GetComponent<Renderer>().material = developerLeft;
                            else if (unsilMatParse[2] == developerTop.name) tileToScrub.GetComponent<Renderer>().material = developerTop;
                            else if (unsilMatParse[2] == developerRight.name) tileToScrub.GetComponent<Renderer>().material = developerRight;
                            else
                            {
                                for (int m = 0; m < materials.Length; m++)
                                {
                                    if (materials[m].name.Contains(unsilMatParse[2]))
                                    {
                                        tileToScrub.GetComponent<Renderer>().material = materials[m];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            silhouetteVal = silhouetteSwitch.value;
        }

        //Hide lower panels when spacebar is down.
        if (Input.GetKeyDown(KeyCode.Space)) wholePanel.gameObject.SetActive(false);
        else if (Input.GetKeyUp(KeyCode.Space)) wholePanel.gameObject.SetActive(true);

        //Handle all operations with mouse.
        bool leftMouse = Input.GetMouseButtonDown(0);
        bool rightMouse = Input.GetMouseButtonDown(1);

        //Pan and zoom.
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + -Input.mouseScrollDelta.y,3,22);
        if (Input.GetMouseButton(2) || (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0)))
        {
            float mX = Input.mousePosition.x * 0.02f;
            float mY = Input.mousePosition.y * 0.02f;
            if (Input.GetMouseButtonDown(2) || (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))) camPos = new Vector3(mainCamera.transform.position.x - (mX + mY), mainCamera.transform.position.y, mainCamera.transform.position.z - (mY - mX));
            mainCamera.transform.position = new Vector3(Mathf.Clamp(mX + camPos.x + mY, 15, 25 + gridSize.x), camPos.y, Mathf.Clamp(mY + camPos.z - mX, 15, 25 + gridSize.z));
        }

        //Do things with tiles and UI.
        else if (leftMouse || rightMouse)
        {
            RaycastHit hitRay;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitRay))
            {
                Vector3 rayPos = hitRay.transform.parent.gameObject.transform.position;
                if (hitRay.collider != null)
                {
                    //Left click, build the voxel.
                    if (leftMouse)
                    {
                        if (selectingStart)
                        {
                            if (hitRay.collider.name.Split('_')[1] == "Top" && goalPoint != new Vector3(rayPos.x, rayPos.y + 0.5f, rayPos.z))
                            {
                                if (startInd != null) Destroy(startInd.gameObject);
                                startPoint = new Vector3(rayPos.x, rayPos.y + 0.5f, rayPos.z);
                                startInd = Instantiate(startModel);
                                startInd.transform.position = startPoint;
                                startInd.transform.localScale = new Vector3(startModelSize, startModelSize, startModelSize);
                                startInd.GetComponent<Renderer>().material.SetColor("_Color", startModelColor);
                                startInd.name = "StartIndicator";
                            }
                        }
                        else if (paintMode && !silhouetteMode)
                        {
                            if (hitRay.collider.name.Contains("Tile"))
                            {
                                if (selectedMaterial != null)
                                {
                                    hitRay.collider.GetComponentInParent<Renderer>().material = selectedMaterial;
                                    string[] parseSplit = hitRay.collider.name.Split('_');
                                    hitRay.collider.name = parseSplit[0] + "_" + parseSplit[1] + "_" + selectedMaterial.name.Replace(" (Instance)", "");
                                }
                            }
                        }
                        else if (!paintMode)
                        {
                            if (hitRay.transform.parent.position + new Vector3(0.5f, 0.5f, 0.5f) == selectCube && hitRay.transform.name.Contains("Select"))
                            {
                                //Define adjacent voxels to the one clicked on.
                                if (hitRay.collider.name.Contains("LeftPositive")) rayPos.x++;
                                else if (hitRay.collider.name.Contains("LeftNegative")) rayPos.x--;
                                else if (hitRay.collider.name.Contains("TopPositive"))
                                {
                                    if (hitRay.collider.transform.parent.position == new Vector3(startPoint.x, startPoint.y - 0.5f, startPoint.z)) Destroy(startInd.gameObject);
                                    if (hitRay.collider.transform.parent.position == new Vector3(goalPoint.x, goalPoint.y - 0.5f, goalPoint.z)) Destroy(goalInd.gameObject);
                                    rayPos.y++;
                                }
                                else if (hitRay.collider.name.Contains("TopNegative")) rayPos.y--;
                                else if (hitRay.collider.name.Contains("RightPositive")) rayPos.z++;
                                else if (hitRay.collider.name.Contains("RightNegative")) rayPos.z--;
                                if (rayPos.x >= 0 && rayPos.y >= 0 && rayPos.z >= 0)
                                {
                                    //Curate faces the newly created voxel will be adjacent to.
                                    bool[] backFace = new bool[3] { false, false, false };
                                    for (int c = 0; c < transform.childCount; c++)
                                    {
                                        CubeParse(true, transform.GetChild(c).transform, rayPos, c);
                                        if (cubeXPos != null) backFace[0] = true;
                                        if (cubeYPos != null) backFace[1] = true;
                                        if (cubeZPos != null) backFace[2] = true;
                                        int tc = transform.GetChild(c).childCount;
                                        for (int t = 0; t < tc; t++) if (CurateCube(cubeXNeg, "Left", t) || CurateCube(cubeYNeg, "Top", t) || CurateCube(cubeZNeg, "Right", t)) tc--;
                                        CubeParse(false, null, new Vector3(0, 0, 0), 0);
                                    }

                                    //Build the new voxel.
                                    Transform newCube = Cuber(
                                        backFace[1] ? "" : "Top", "",
                                        backFace[2] ? "" : "Right", "",
                                        backFace[0] ? "" : "Left", "",
                                        new Vector3(rayPos.x + 0.5f, rayPos.y + 0.5f, rayPos.z + 0.5f)
                                    );
                                    selectCube = Selector(newCube, rayPos, selectorCubeSize, selectorCubeDistance);
                                }
                            }
                            else selectCube = Selector(hitRay.transform.parent, rayPos, selectorCubeSize, selectorCubeDistance);
                        }
                    }

                    //Right click.
                    else if (rightMouse)
                    {
                        if (selectingStart)
                        {
                            if (hitRay.collider.name.Split('_')[1] == "Top" && startPoint != new Vector3(rayPos.x, rayPos.y + 0.5f, rayPos.z))
                            {
                                if (goalInd != null) Destroy(goalInd.gameObject);
                                goalPoint = new Vector3(rayPos.x, rayPos.y + 0.5f, rayPos.z);
                                goalInd = Instantiate(goalModel);
                                goalInd.transform.position = goalPoint;
                                goalInd.transform.localScale = new Vector3(goalModelSize, goalModelSize, goalModelSize);
                                goalInd.GetComponent<Renderer>().material.SetColor("_Color", goalModelColor);
                                goalInd.name = "GoalIndicator";
                            }
                        }
                        else if (paintMode && !silhouetteMode)
                        {
                            string[] parseSplit = hitRay.collider.name.Split('_');
                            if (hitRay.collider.name.Split('_')[1] == "Left")
                            {
                                hitRay.collider.GetComponentInParent<Renderer>().material = developerLeft;
                                hitRay.collider.name = parseSplit[0] + "_" + parseSplit[1] + "_" + developerLeft.name;
                            }
                            else if (hitRay.transform.name.Split('_')[1] == "Top")
                            {
                                hitRay.collider.GetComponentInParent<Renderer>().material = developerTop;
                                hitRay.collider.name = parseSplit[0] + "_" + parseSplit[1] + "_" + developerTop.name;
                            }
                            else if (hitRay.transform.name.Split('_')[1] == "Right")
                            {
                                hitRay.collider.GetComponentInParent<Renderer>().material = developerRight;
                                hitRay.collider.name = parseSplit[0] + "_" + parseSplit[1] + "_" + developerRight.name;
                            }
                        }
                        else
                        {
                            //Remove voxel.
                            if (hitRay.collider.name.Contains("Left") || hitRay.collider.name.Contains("Top") || hitRay.collider.name.Contains("Right"))
                            {
                                Destroy(hitRay.transform.parent.gameObject);
                                for (int r = 0; r < transform.childCount; r++)
                                {
                                    DeleteCube(cubeXPos, cubeYPos, cubeZPos, rayPos, transform.GetChild(r).transform.position, r);
                                }
                                if (hitRay.collider.transform.parent.position == new Vector3(startPoint.x, startPoint.y - 0.5f, startPoint.z)) Destroy(startInd.gameObject);
                                if (hitRay.collider.transform.parent.position == new Vector3(goalPoint.x, goalPoint.y - 0.5f, goalPoint.z)) Destroy(goalInd.gameObject);
                            }
                        }
                    }
                }
            }
            else SelectDeleter(selectCube);
        }
    }

    //Buttons
    public void OnNewButton()
    {
        //Clear out the old stuff.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
            if (startInd != null) { Destroy(startInd.gameObject); }
        }
        saveName.text = "";

        //Reset colors.
        if (HSVMode)
        {
            RGBToHSV(new Color(0.5f, 0.55f, 0.6f), BGColorR, BGColorG, BGColorB);
            RGBToHSV(new Color(0, 0, 0), CloudColorR, CloudColorG, CloudColorB);
            RGBToHSV(new Color(0, 0, 0), SilhouetteColorR, SilhouetteColorG, SilhouetteColorB);
        }
        else
        {
            SetColor(new Color(0.5f, 0.55f, 0.6f), BGColorR, BGColorG, BGColorB);
            SetColor(new Color(0, 0, 0), CloudColorR, CloudColorG, CloudColorB);
            SetColor(new Color(0, 0, 0), SilhouetteColorR, SilhouetteColorG, SilhouetteColorB);
        }

        //Reset background mode.
        modeSet.value = 0;

        //Place the camera.
        mainCamera.transform.position = new Vector3(34, 25, 34);
        saveCamPos = mainCamera.transform.position;
        mainCamera.orthographicSize = 8;

        //Build a single cube at 1, 1, 1
        Cuber("Top", developerTop.name, "Right", developerRight.name, "Left", developerLeft.name, new Vector3(19, 1, 19));
    }

    public void OnSaveButton()
    {
        if (saveName.text != "")
        {
            SelectDeleter(selectCube);

            //Check and read all tiles in the field.
            int saveCount = transform.childCount;
            cubeData = new Cube[saveCount];
            List<Vector3Int> vertices = new List<Vector3Int>(saveCount * 4);
            for (int s = 0; s < saveCount; s++)
            {
                Transform cube = transform.GetChild(s);
                AddTileVerticesToList(cube, vertices);
                int tileCount = cube.childCount;
                tileData = new Tile[tileCount];
                for (int c = 0; c < tileCount; c++)
                {
                    string saveParse = cube.GetChild(c).name.Split('_')[1];
                    string cuberString;
                    if (saveParse == "Top" || saveParse == "Right" || saveParse == "Left") cuberString = saveParse;
                    else cuberString = "";
                    Tile inTile = new Tile { d = cuberString };
                    if (cube.GetChild(c).name.Contains("Tile")) inTile.m = cube.GetChild(c).name.Split('_')[2];
                    tileData[c] = inTile;
                }
                Cube inCube = new Cube();
                inCube.position = new Vector3(cube.position.x, cube.position.y, cube.position.z);
                inCube.tileData = tileData;
                cubeData[s] = inCube;
            }
            //Save the data into a json file.
            Grid gridSave = new Grid
            {
                cubeData = cubeData,
                vertices = vertices.ToArray(),
                backgroundColor = new Vector3(backgroundColor.r, backgroundColor.g, backgroundColor.b),
                cloudColor = new Vector3(cloudColor.r, cloudColor.g, cloudColor.b),
                silhouetteColor = new Vector3(silhouetteColor.r, silhouetteColor.g, silhouetteColor.b),
                cameraPosition = saveCamPos,
                backgroundMode = modeCheck,
                abilityElevator = (int)(abilityElevator.value * (abilityElevator.numberOfSteps - 1)),
                abilityTeleport = (int)(abilityTeleport.value * (abilityTeleport.numberOfSteps)),
                abilityMoveX = (int)(abilityMoveX.value * (abilityMoveX.numberOfSteps - 1)),
                abilityMoveZ = (int)(abilityMoveZ.value * (abilityMoveZ.numberOfSteps - 1)),
                abilityExtrude = (int)(abilityExtrude.value * (abilityExtrude.numberOfSteps - 1))
            };
            if (startInd != null && goalInd != null)
            {
                if (startPoint != goalPoint)
                {
                    gridSave.startPoint = startPoint;
                    gridSave.goalPoint = goalPoint;
                    string stringSave = JsonUtility.ToJson(gridSave, true);
                    if (Application.isEditor)
                        File.WriteAllText(path + saveName.text + ".json", stringSave);
                    else
                        File.WriteAllText(Application.persistentDataPath + "/USER" + saveName.text + ".json", stringSave);
                    ErrorBar(3, "Level " + saveName.text + " has been saved.");
                }
                else ErrorBar(4, "THE STARTPOINT AND GOALPOINT MUST NOT BE IN THE SAME PLACE TO SAVE.");
            }
            else
            {
                if (startInd == null && goalInd == null) ErrorBar(4, "A SET STARTPOINT AND GOALPOINT ARE REQUIRED TO SAVE.");
                else if (startInd == null) ErrorBar(4, "A SET STARTPOINT IS REQUIRED TO SAVE.");
                else if (goalInd == null) ErrorBar(4, "A SET GOALPOINT IS REQUIRED TO SAVE.");
            }
            _currentLevel = gridSave;
            OnRefreshButton();
        }
        else ErrorBar(3, "SAVE MUST HAVE A NAME IN THE INPUT FIELD UNDER THE SAVE BUTTON.");
    }

    public void OnLoadButton(Button button)
    {
        //Clear out the old cubes.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
            if (startInd != null) Destroy(startInd.gameObject);
            if (goalInd != null) Destroy(goalInd.gameObject);
        }
        SelectDeleter(selectCube);
        selectCube = new Vector3(0, 0, 0);

        //Read and interpret the save file.
        LoadEditorLevel(button.transform.parent.name);
    }

    private void LoadEditorLevel(string levelName)
    {
        string stringLoad;
        
        if (File.Exists(path + "USER" + levelName + ".json"))
            stringLoad = File.ReadAllText(path + "USER" + levelName + ".json");
        else if (File.Exists(Application.persistentDataPath + "/USER" + levelName + ".json"))
            stringLoad = File.ReadAllText(Application.persistentDataPath + "/USER" + levelName + ".json");
        else if (Application.isEditor)
            stringLoad = File.ReadAllText(path + levelName + ".json");
        else
            return;
        Grid gridLoad = JsonUtility.FromJson<Grid>(stringLoad);
        _currentLevel = gridLoad;
        startPoint = gridLoad.startPoint;
        startInd = Instantiate(startModel);
        startInd.transform.position = startPoint;
        startInd.transform.localScale = new Vector3(startModelSize, startModelSize, startModelSize);
        startInd.GetComponent<Renderer>().material.SetColor("_Color", startModelColor);
        startInd.name = "StartIndicator";
        goalPoint = gridLoad.goalPoint;
        goalInd = Instantiate(goalModel);
        goalInd.position = goalPoint;
        goalInd.transform.localScale = new Vector3(goalModelSize, goalModelSize, goalModelSize);
        goalInd.GetComponent<Renderer>().material.SetColor("_Color", goalModelColor);
        goalInd.gameObject.name = "GoalIndicator";
        backgroundColor = new Color(gridLoad.backgroundColor.x, gridLoad.backgroundColor.y, gridLoad.backgroundColor.z);
        cloudColor = new Color(gridLoad.cloudColor.x, gridLoad.cloudColor.y, gridLoad.cloudColor.z);
        silhouetteColor = new Color(gridLoad.silhouetteColor.x, gridLoad.silhouetteColor.y, gridLoad.silhouetteColor.z);
        modeSet.value = gridLoad.backgroundMode;
        abilityElevator.value = (float)gridLoad.abilityElevator / (abilityElevator.numberOfSteps - 1);
        abilityTeleport.value = (float)gridLoad.abilityTeleport / (abilityTeleport.numberOfSteps - 1);
        abilityMoveX.value = (float)gridLoad.abilityMoveX / (abilityMoveX.numberOfSteps - 1);
        abilityMoveZ.value = (float)gridLoad.abilityMoveZ / (abilityMoveZ.numberOfSteps - 1);
        abilityExtrude.value = (float)gridLoad.abilityExtrude / (abilityExtrude.numberOfSteps - 1);
        if (HSVMode)
        {
            RGBToHSV(backgroundColor, BGColorR, BGColorG, BGColorB);
            RGBToHSV(cloudColor, CloudColorR, CloudColorG, CloudColorB);
            RGBToHSV(silhouetteColor, SilhouetteColorR, SilhouetteColorG, SilhouetteColorB);
        }
        else
        {
            BGColorR.value = backgroundColor.r;
            BGColorG.value = backgroundColor.g;
            BGColorB.value = backgroundColor.b;
            CloudColorR.value = cloudColor.r;
            CloudColorG.value = cloudColor.g;
            CloudColorB.value = cloudColor.b;
            SilhouetteColorR.value = silhouetteColor.r;
            SilhouetteColorG.value = silhouetteColor.g;
            SilhouetteColorB.value = silhouetteColor.b;
        }

        //Place the camera.
        mainCamera.transform.position = new Vector3(gridLoad.cameraPosition.x, 25, gridLoad.cameraPosition.y);
        mainCamera.orthographicSize = gridLoad.cameraPosition.z;
        saveCamPos = gridLoad.cameraPosition;

        //Build the new level.
        saveName.text = levelName;
        int loadCount = gridLoad.cubeData.Length;
        Cube loadCube;
        for (int l = 0; l < loadCount; l++)
        {
            loadCube = gridLoad.cubeData[l];
            Vector3 loadPos = new Vector3(loadCube.position.x - 0.5f, loadCube.position.y - 0.5f, loadCube.position.z - 0.5f);
            string[] toCuber = new string[3] { "", "", "" };
            string[] toMatter = new string[3] { "", "", "" };
            for (int c = 0; c < loadCube.tileData.Length; c++)
            {
                if (loadCube.tileData[c].d == "Top")
                {
                    toCuber[0] = "Top";
                    toMatter[0] = loadCube.tileData[c].m;
                }
                else if (loadCube.tileData[c].d == "Right")
                {
                    toCuber[1] = "Right";
                    toMatter[1] = loadCube.tileData[c].m;
                }
                else if (loadCube.tileData[c].d == "Left")
                {
                    toCuber[2] = "Left";
                    toMatter[2] = loadCube.tileData[c].m;
                }
            }
            Cuber(toCuber[0], toMatter[0], toCuber[1], toMatter[1], toCuber[2], toMatter[2], new Vector3(loadPos.x + 1, loadPos.y + 1, loadPos.z + 1));
        }
    }

    public void OnRefreshButton()
    {
        //Clear old buttons.
        for (int d = 0; d < fileContent.transform.childCount; d++)
        {
            Destroy(fileContent.GetChild(d).gameObject);
        }

        //Build new buttons.
        var directoryFiles = (Application.isEditor)
            ? Directory.GetFiles(path)
            : Directory.GetFiles(Application.persistentDataPath);
        RectTransform[] fileButton = new RectTransform[directoryFiles.Length];
        int fileCount = 0;
        for (int r = 0; r < fileButton.Length; r++)
        {
            string[] fileParse = directoryFiles[r].Split('/', '\\');
            if (!fileParse[fileParse.Length - 1].Contains(".meta"))
            {
                string file = fileParse[fileParse.Length - 1].Replace(".json", "");
                bool userPrefix = file.Substring(0, 4) == "USER";
                if (!Application.isEditor && !userPrefix)
                    continue;
                if (userPrefix)
                    file = file.Remove(0, 4);
                fileButton[r] = Instantiate(fileTemplate, fileContent);
                fileButton[r].gameObject.SetActive(true);
                fileButton[r].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -fileCount * 50 - 30);
                fileButton[r].name = file;
                fileButton[r].GetComponentInChildren<Text>().text = file;
                fileCount++;
            }
        }
        fileContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, fileTemplate.GetComponent<RectTransform>().rect.height * fileCount + (fileCount * 10) + 10);
    }

    public void OnMaterialButton(Button button)
    {
        SelectDeleter(selectCube);
        selectingStart = false;
        int matNum = int.Parse(button.name);
        Image btnImage = button.GetComponent<Image>();
        matView.GetComponent<Image>().material = btnImage.material;
        selectedMaterial = materials[matNum];
    }

    public void OnTileButton()
    {
        selectingStart = false;
        matView.GetComponent<Image>().material = null;
    }

    public void StartChecker(bool check)
    {
        if (check) SelectDeleter(selectCube);
        selectingStart = check;
    }

    public void OnDeleteButton(Button button)
    {
        if (button.GetComponentInChildren<Text>().text == "DELETE")
        {
            button.GetComponentInChildren<Text>().text = "CONFIRM";
            button.GetComponent<Image>().color = new Color(1, 0.08f, 0);
        }
        else
        {
            if (File.Exists(Application.persistentDataPath + "/USER" + button.transform.parent.name + ".json"))
                File.Delete(Application.persistentDataPath + "/USER" + button.transform.parent.name + ".json");
            else if (File.Exists(path + "USER" + button.transform.parent.name + ".json"))
                File.Delete(path + "USER" + button.transform.parent.name + ".json");
            else if (File.Exists(path + button.transform.parent.name + ".json"))
                File.Delete(path + button.transform.parent.name + ".json");
            ErrorBar(4, "File " + button.transform.parent.name + " has been deleted.");
            OnRefreshButton();
        }
    }

    public void ValueChecker()
    {
        checkColor = true;
    }

    public void PaintChecker(bool check)
    {
        paintMode = check;
        if (check) SelectDeleter(selectCube);
    }

    public void BackgroundChecker()
    {
        checkBackground = true;
    }

    public void OnCameraButton(bool mode)
    {
        if (mode)
        {
            saveCamPos = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.z, mainCamera.orthographicSize);
            ErrorBar(4, "START CAMERA POSITION HAS BEEN SET AT CURRENT VIEW.");
        }
        else
        {
            mainCamera.transform.position = new Vector3(saveCamPos.x, 25, saveCamPos.y);
            mainCamera.orthographicSize = saveCamPos.z;
            ErrorBar(4, "VIEW HAS BEEN SET TO STAGED CAMERA.");
        }
    }

    //Build a cube on the grid.
    Transform Cuber(string top, string topMat, string rgt, string rgtMat, string lft, string lftMat, Vector3 pos)
    {
        GameObject cube = new GameObject("Cube " + pos.x + " - " + pos.y + " - " + pos.z);
        cube.transform.SetParent(transform);
        cube.transform.position = new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f);
        cube.AddComponent<BoxCollider>().transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        if (top != "") Tiler("Top", cube.transform, "Top", topMat);
        if (rgt != "") Tiler("Right", cube.transform, "Right", rgtMat);
        if (lft != "") Tiler("Left", cube.transform, "Left", lftMat);
        return cube.transform;
    }

    //Add a tile to a cube.
    void Tiler(string dim, Transform par, string name, string mat)
    {
        //Create the gameobject.
        GameObject tile = new GameObject();
        tile.transform.SetParent(par);
        tile.layer = LayerMask.NameToLayer("Tile");
        tile.AddComponent<MeshFilter>();
        MeshFilter tileF = tile.GetComponent<MeshFilter>();
        tile.AddComponent<MeshRenderer>();
        MeshRenderer tileR = tile.GetComponent<MeshRenderer>();
        tile.AddComponent<BoxCollider>();
        BoxCollider tileC = tile.GetComponent<BoxCollider>();
        if (!GameManager.SharedInstance.InLevelEditor)
        {
            tileC.isTrigger = true;
            TileComponent tc = tile.AddComponent<TileComponent>();
            tc.orientation = dim;
            tc.matName = mat;
        }

        //Define and populate the variables and vectors.
        Material loadMat = null;
        if (mat != "")
        {
            string matParse;
            for (int m = 0; m < materials.Length; m++)
            {
                matParse = materials[m].name;
                if (matParse == mat)
                {
                    loadMat = materials[m];
                    break;
                }
            }
        }
        Vector3[] tileV = new Vector3[4];
        Vector3[] tileN = new Vector3[4];
        Vector2[] tileUV = new Vector2[4];
        int[] tileT = new int[6] { 0, 2, 1, 2, 3, 1 };
        tileUV[0] = new Vector2(0, 0);
        tileUV[1] = new Vector2(1, 0);
        tileUV[2] = new Vector2(0, 1);
        tileUV[3] = new Vector2(1, 1);
        switch (dim)
        {
            case ("Top"):
                tile.transform.position = new Vector3(par.position.x, par.position.y + 0.5f, par.position.z);
                tileV[0] = new Vector3(0.5f, 0, -0.5f);
                tileV[1] = new Vector3(0.5f, 0, 0.5f);
                tileV[2] = new Vector3(-0.5f, 0, -0.5f);
                tileV[3] = new Vector3(-0.5f, 0, 0.5f);
                tileN[0] = Vector3.up;
                tileN[1] = Vector3.up;
                tileN[2] = Vector3.up;
                tileN[3] = Vector3.up;
                tileC.size = new Vector3(1, 0, 1);
                tileC.center = new Vector3(0, 0, 0);
                tileR.material = (loadMat != null) ? loadMat : developerTop;
                tile.name = "Tile_" + name + "_" + tileR.material.name.Replace(" (Instance)", "");
                break;
            case ("Right"):
                tile.transform.position = new Vector3(par.position.x, par.position.y, par.position.z + 0.5f);
                tileV[0] = new Vector3(0.5f, -0.5f, 0);
                tileV[1] = new Vector3(-0.5f, -0.5f, 0);
                tileV[2] = new Vector3(0.5f, 0.5f, 0);
                tileV[3] = new Vector3(-0.5f, 0.5f, 0);
                tileN[0] = Vector3.forward;
                tileN[1] = Vector3.forward;
                tileN[2] = Vector3.forward;
                tileN[3] = Vector3.forward;
                tileC.size = new Vector3(1, 1, 0);
                tileC.center = new Vector3(0, 0, 0);
                tileR.material = (loadMat != null) ? loadMat : developerRight;
                tile.name = "Tile_" + name + "_" + tileR.material.name.Replace(" (Instance)", "");
                break;
            case ("Left"):
                tile.transform.position = new Vector3(par.position.x + 0.5f, par.position.y, par.position.z);
                tileV[0] = new Vector3(0, -0.5f, -0.5f);
                tileV[1] = new Vector3(0, -0.5f, 0.5f);
                tileV[2] = new Vector3(0, 0.5f, -0.5f);
                tileV[3] = new Vector3(0, 0.5f, 0.5f);
                tileN[0] = Vector3.right;
                tileN[1] = Vector3.right;
                tileN[2] = Vector3.right;
                tileN[3] = Vector3.right;
                tileC.size = new Vector3(0, 1, 1);
                tileC.center = new Vector3(0, 0, 0);
                tileR.material = (loadMat != null) ? loadMat : developerLeft;
                tile.name = "Tile_" + name + "_" + tileR.material.name.Replace(" (Instance)", "");
                break;
            default:
                Debug.LogError("Tile " + name + " has failed to parse.");
                break;
        }

        //Build the mesh.
        if (dim == "Top" || dim == "Right" || dim == "Left")
        {
            tileR.material = silhouetteMode ? silhouette : tileR.material;
            tileF.mesh.vertices = tileV;
            tileF.mesh.normals = tileN;
            tileF.mesh.uv = tileUV;
            tileF.mesh.triangles = tileT;
        }
    }

    //Set up selection array.
    Vector3 Selector(Transform parent, Vector3 selPos, float size, float dist)
    {
        SelectDeleter(selectCube);
        if (parent.position.x < gridSize.x) SelectBuilder("Select_LeftPositive", selectCubeModel, parent, new Vector3(selPos.x + dist, selPos.y, selPos.z), size);
        if (parent.position.x > 1) SelectBuilder("Select_LeftNegative", selectCubeModel, parent, new Vector3(selPos.x - dist, selPos.y, selPos.z), size);
        if (parent.position.y < gridSize.y) SelectBuilder("Select_TopPositive", selectCubeModel, parent, new Vector3(selPos.x, selPos.y + dist, selPos.z), size);
        if (parent.position.y > 1) SelectBuilder("Select_TopNegative", selectCubeModel, parent, new Vector3(selPos.x, selPos.y - dist, selPos.z), size);
        if (parent.position.z < gridSize.z) SelectBuilder("Select_RightPositive", selectCubeModel, parent, new Vector3(selPos.x, selPos.y, selPos.z + dist), size);
        if (parent.position.z > 1) SelectBuilder("Select_RightNegative", selectCubeModel, parent, new Vector3(selPos.x, selPos.y, selPos.z - dist), size);
        return selPos + new Vector3(0.5f, 0.5f, 0.5f);


    }

    //Build a selector cube at a specified position.
    void SelectBuilder(string name, Transform transform, Transform parent, Vector3 pos, float size)
    {
        Transform buildSelect = Instantiate(transform, pos, Quaternion.identity, parent);
        buildSelect.localScale = new Vector3(size * 2, size * 2, size * 2);
        buildSelect.GetComponent<Renderer>().material.SetColor("_Color",selectorCubeColor);
        buildSelect.name = name;
    }

    //Delete the selection array.
    void SelectDeleter(Vector3 selPos)
    {
        Collider[] clearOldSelect = Physics.OverlapBox(selPos, new Vector3(1.5f, 1.5f, 1.5f));
        for (int c = 0; c < clearOldSelect.Length; c++)
        {
            if (clearOldSelect[c].name.Contains("Select")) Destroy(clearOldSelect[c].gameObject);
        }
        selectCube = new Vector3(0, 0, 0);
    }

    //Cube Operations
    void CubeParse(bool mode, Transform target, Vector3 rayPos, int arrayIn)
    {
        if (mode)
        {
            if (target.position == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z)) cubeXNeg = transform.GetChild(arrayIn);
            else if (target.position == new Vector3(rayPos.x + 1, rayPos.y, rayPos.z)) cubeXPos = transform.GetChild(arrayIn);
            else if (target.position == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z)) cubeYNeg = transform.GetChild(arrayIn);
            else if (target.position == new Vector3(rayPos.x, rayPos.y + 1, rayPos.z)) cubeYPos = transform.GetChild(arrayIn);
            else if (target.position == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1)) cubeZNeg = transform.GetChild(arrayIn);
            else if (target.position == new Vector3(rayPos.x, rayPos.y, rayPos.z + 1)) cubeZPos = transform.GetChild(arrayIn);
        }
        else
        {
            cubeXNeg = null;
            cubeXPos = null;
            cubeYNeg = null;
            cubeYPos = null;
            cubeZNeg = null;
            cubeZPos = null;
        }
    }

    //Manage cube adjacencies, delete faces that would be merged with another cube.
    bool CurateCube(Transform cube, string dim, int array)
    {
        if (cube != null)
        {
            if (cube.GetChild(array) != null)
            {
                if (cube.GetChild(array).name.Contains(dim))
                {
                    Destroy(cube.GetChild(array).gameObject);
                    return true;
                }
            }
        }
        return false;
    }

    //Delte cube, fill holes in other adjacent cubes.
    void DeleteCube(Transform tx, Transform ty, Transform tz, Vector3 rayPos, Vector3 delPos, int array)
    {
        if (delPos == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z))
        {
            tz = transform.GetChild(array);
            Tiler("Left", tz, "Left", "");
        }
        if (delPos == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z))
        {
            ty = transform.GetChild(array);
            Tiler("Top", ty, "Top", "");
        }
        if (delPos == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1))
        {
            tx = transform.GetChild(array);
            Tiler("Right", tx, "Right", "");
        }
    }

    //Set up vertices for level.
    private void AddTileVerticesToList(Transform cube, List<Vector3Int> verticesList)
    {
        foreach (Transform tileTransform in cube)
        {
            MeshFilter tileMeshFilter = tileTransform.GetComponent<MeshFilter>();
            if (!tileMeshFilter)
                continue;
            Mesh tileMesh = tileMeshFilter.mesh;
            foreach (var meshVertex in tileMesh.vertices)
            {
                Vector3 worldSpaceVertex = tileTransform.TransformPoint(meshVertex);
                Vector3Int meshVertexInt = Vector3Int.RoundToInt(worldSpaceVertex);
                if (verticesList.Exists(v => v == meshVertexInt))
                    continue;
                Vector3 dir = GameManager.SharedInstance.MainCamera.transform.position - worldSpaceVertex;
                if (Physics.Raycast(worldSpaceVertex, dir, out RaycastHit hitInfo))
                {
                    if (hitInfo.transform.parent != tileTransform.parent)
                        continue;
                }
                verticesList.Add(meshVertexInt);
            }
        }
    }
    
    public Grid LoadLevel(string levelName, bool silhouetted = true)
    {
        //Clear out the old cubes.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
            if (startInd != null) Destroy(startInd.gameObject);
            if (goalInd != null) Destroy(goalInd.gameObject);
        }

        silhouetteMode = silhouetted;
        
        //Read and interpret the save file.
        string stringLoad;
        if (File.Exists(Application.persistentDataPath + "/USER" + levelName + ".json"))
            stringLoad = File.ReadAllText(Application.persistentDataPath + "/USER" + levelName + ".json");
        else if (Application.isEditor && Resources.Load("USER" + levelName))
        {
            TextAsset resourceLevel = Resources.Load("USER" + levelName) as TextAsset;
            stringLoad = resourceLevel.text;
        }
        else
        {
            TextAsset resourceLevel = Resources.Load(levelName) as TextAsset;
            stringLoad = resourceLevel.text;
        }
        //TextAsset stringLoad = Resources.Load(levelName) as TextAsset;
        Grid gridLoad = JsonUtility.FromJson<Grid>(stringLoad);
        startPoint = gridLoad.startPoint;
        goalPoint = gridLoad.goalPoint;
        backgroundColor = new Color(gridLoad.backgroundColor.x, gridLoad.backgroundColor.y, gridLoad.backgroundColor.z);
        cloudColor = new Color(gridLoad.cloudColor.x, gridLoad.cloudColor.y, gridLoad.cloudColor.z);
        silhouetteColor = new Color(gridLoad.silhouetteColor.x, gridLoad.silhouetteColor.y, gridLoad.silhouetteColor.z);
        
        int loadCount = gridLoad.cubeData.Length;
        Cube loadCube;
        for (int l = 0; l < loadCount; l++)
        {
            loadCube = gridLoad.cubeData[l];
            Vector3 loadPos = new Vector3(loadCube.position.x - 0.5f, loadCube.position.y - 0.5f, loadCube.position.z - 0.5f);
            string[] toCuber = new string[3] { "", "", "" };
            string[] toMatter = new string[3] { "", "", "" };
            for (int c = 0; c < loadCube.tileData.Length; c++)
            {
                if (loadCube.tileData[c].d == "Top")
                {
                    toCuber[0] = "Top";
                    toMatter[0] = loadCube.tileData[c].m;
                }
                else if (loadCube.tileData[c].d == "Right")
                {
                    toCuber[1] = "Right";
                    toMatter[1] = loadCube.tileData[c].m;
                }
                else if (loadCube.tileData[c].d == "Left")
                {
                    toCuber[2] = "Left";
                    toMatter[2] = loadCube.tileData[c].m;
                }
            }
            //Build the new level.
            Cuber(toCuber[0], toMatter[0], toCuber[1], toMatter[1], toCuber[2], toMatter[2], new Vector3(loadPos.x + 1, loadPos.y + 1, loadPos.z + 1));
        }
        
        if (backgroundInstance != null) Destroy(backgroundInstance.gameObject);
        BackgroundBuilder(gridLoad.backgroundMode);
        if (gridLoad.backgroundMode == 1 || gridLoad.backgroundMode == 2)
            CloudColorer(backgroundInstance, cloudColor);
        
        mainCamera.transform.position = new Vector3(gridLoad.cameraPosition.x, 20.5f, gridLoad.cameraPosition.y);
        mainCamera.orthographicSize = gridLoad.cameraPosition.z;
        mainCamera.backgroundColor = backgroundColor;

        GameManager.SharedInstance.EdgeCamera.orthographicSize = gridLoad.cameraPosition.z;
        GameManager.SharedInstance.PlayerCamera.orthographicSize = gridLoad.cameraPosition.z;

        return gridLoad;
    }

    public void TestLevel()
    {
        OnSaveButton();
        PlayerPrefs.SetString("editorLevel", _currentLevel.levelName);
        PlayerPrefs.SetInt("loadEditorLevel", 1);
        SceneManager.LoadScene("EditorGameScene");
    }

    //Convert from RGB to HSV
    void RGBToHSV(Color color, Slider R, Slider G, Slider B)
    {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        R.value = H;
        G.value = S;
        B.value = V;
    }

    //Convert from HSV to RGB
    void HSVToRGB(Slider R, Slider G, Slider B)
    {
        Color color = Color.HSVToRGB(R.value, G.value, B.value);
        R.value = color.r;
        G.value = color.g;
        B.value = color.b;
    }

    //Set a given color to a set of sliders.
    void SetColor(Color color, Slider r, Slider g, Slider b)
    {
        r.value = color.r;
        g.value = color.g;
        b.value = color.b;
    }

    //Color the clouds.
    void CloudColorer(Transform gen, Color color)
    {
        if (modeCheck == 1 || modeCheck == 2)
        {
            for (int c = 0; c < gen.childCount; c++)
            {
                for (int p = 0; p < gen.GetChild(c).childCount; p++)
                {
                    gen.GetChild(c).GetChild(p).GetComponent<Renderer>().material.color = new Color(color.r, color.g, color.b, 0.2f);
                }
            }
        }
        else if (modeCheck == 4)
        {
            for (int c = 0; c < gen.childCount; c++)
            {
                gen.GetChild(c).GetComponent<Renderer>().material.color = new Color(color.r, color.g, color.b, 0.2f);
            }
        }
    }

    //Checks and applies ability settings.
    float ScrollParse(float abilityVal, Scrollbar ability)
    {
        if (abilityVal != ability.value)
        {
            float abMult = ability.value * (ability.numberOfSteps - 1);
            return abMult;
        }
        return abilityVal;
    }

    //Trigger the error bar.
    void ErrorBar(float dur, string content)
    {
        errorBar.GetComponent<ErrorBar>().content = content;
        errorBar.GetComponent<ErrorBar>().duration = dur;
        errorBar.GetComponent<ErrorBar>().trigger = true;
    }

    //Set up the background.
    void BackgroundBuilder(float mode)
    {
        backgroundInstance = backgroundGenerator;
        backgroundInstance.position = new Vector3(-100, -150, -100);
        backgroundInstance.GetComponent<BackgroundGenerator>().cloudColor = cloudColor;
        backgroundInstance.GetComponent<BackgroundGenerator>().mode = (int)mode;
        modeCheck = (int)mode;
        if (mode == 3) backgroundInstance = Instantiate(backgroundInstance, mainCamera.transform);
        else backgroundInstance = Instantiate(backgroundInstance);
    }
}

[System.Serializable]
public class Tile
{
    public string d;
    public string m;
}

[System.Serializable]
public class Cube
{
    public Vector3 position;
    public Tile[] tileData;
}
[System.Serializable]
public class Grid
{
    public Cube[] cubeData;
    public Vector3Int[] vertices;
    public Vector3 startPoint;
    public Vector3 goalPoint;
    public Vector3 backgroundColor;
    public Vector3 cloudColor;
    public Vector3 silhouetteColor;
    public Vector3 cameraPosition;
    public int backgroundMode;
    public int abilityElevator;
    public int abilityTeleport;
    public int abilityMoveX;
    public int abilityMoveZ;
    public int abilityExtrude;
    public string levelName;
}