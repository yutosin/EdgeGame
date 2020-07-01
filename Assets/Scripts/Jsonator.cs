using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Jsonator : MonoBehaviour
{
    //Set the maximum allowed size of the grid.
    [Header("General Editor Settings")]
    public Vector3 gridSize;
    public float selectorCubeSize;
    public float selectorCubeDistance;

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

    // Model for displaying where the start position in the editor is.
    [Header("Player Model")]
    public Transform selectCubeModel;
    public Transform playerModel;
    public Transform endModel;

    [Header("Save Path")]
    public string path;

    [Header("UI Elements")]
    public Scrollbar silhouetteSwitch;
    public InputField saveName;
    public RectTransform fileContent;
    public RectTransform fileTemplate;
    public RectTransform matContent;
    public RectTransform matTemplate;
    public RectTransform matView;

    private Transform cubeXNeg;
    private Transform cubeXPos;
    private Transform cubeYNeg;
    private Transform cubeYPos;
    private Transform cubeZNeg;
    private Transform cubeZPos;
    private Transform startInd;
    private Transform finInd;
    private bool paintMode;
    private bool selectingStart;
    private bool silhouetteMode;
    private float silhouetteVal;
    private Material[] displayMaterials;
    private Material selectedMaterial;
    private Vector3 startPoint;
    private Vector3 goalPoint;
    private Vector3 selectCube;

    [SerializeField]
    private Cube[] cubeData;
    private Tile[] tileData;

    void Awake()
    {
        //Set up load and materials buttons.
        selectingStart = false;
        silhouetteMode = false;
        silhouetteVal = 0;
        OnRefreshButton();
        RectTransform[] matButton = new RectTransform[materials.Length];
        matContent.GetComponent<RectTransform>().sizeDelta = new Vector2(matTemplate.GetComponent<RectTransform>().rect.width * matButton.Length + (matButton.Length * 10) + 10, 0);
        displayMaterials = new Material[materials.Length];
        for (int m = 0; m < matButton.Length; m++)
        {
            displayMaterials[m] = new Material(materials[m]);
            displayMaterials[m].shader = Shader.Find("UI/Default");
            matButton[m] = Instantiate(matTemplate, matContent);
            matButton[m].GetComponent<RectTransform>().anchoredPosition = new Vector2(m * 110 + 60, 0);
            matButton[m].GetComponent<Image>().material = displayMaterials[m];
            matButton[m].name = m.ToString();
        }
        selectCube = new Vector3(1, 1, 1);
    }

    void Update()
    {
        //Externally disable level editor mode via GameManager.
        if (!GameManager.SharedInstance.InLevelEditor) return;

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
                        transform.GetChild(l).GetChild(c).GetComponent<Renderer>().material = silhouette;
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
                        string unsilMatParse = tileToScrub.name.Split('_')[2];
                        if (unsilMatParse == developerLeft.name) { tileToScrub.GetComponent<Renderer>().material = developerLeft; }
                        else if (unsilMatParse == developerTop.name) { tileToScrub.GetComponent<Renderer>().material = developerTop; }
                        else if (unsilMatParse == developerRight.name) { tileToScrub.GetComponent<Renderer>().material = developerRight; }
                        else
                        {
                            for (int m = 0; m < materials.Length; m++)
                            {
                                if (materials[m].name.Contains(unsilMatParse))
                                {
                                    tileToScrub.GetComponent<Renderer>().material = materials[m];
                                    break;
                                }

                            }
                        }
                    }
                }
            }
            silhouetteVal = silhouetteSwitch.value;
        }

        //Handle Mouse-based block addition and removal.
        bool leftMouse = Input.GetMouseButtonDown(0);
        bool rightMouse = Input.GetMouseButtonDown(1);

        if (leftMouse || rightMouse)
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
                        if (paintMode && !silhouetteMode)
                        {
                            hitRay.collider.GetComponentInParent<Renderer>().material = selectedMaterial;
                            string[] parseSplit = hitRay.collider.name.Split('_');
                            hitRay.collider.name = parseSplit[0] + "_" + parseSplit[1] + "_" + selectedMaterial.name.Replace(" (Instance)", "");
                        }
                        else if (selectingStart)
                        {
                            matView.GetComponentInChildren<Text>().text = "EDITING\nTILE";
                            Destroy(startInd.gameObject);
                            startInd.gameObject.name = "PlayerIndicator";
                            startPoint = new Vector3(rayPos.x, rayPos.y + 0.5f, rayPos.z);
                            startInd = Instantiate(playerModel);
                            startInd.transform.position = startPoint;
                            selectingStart = false;
                        }
                        else if (!paintMode)
                        {
                            if (hitRay.transform.parent.position + new Vector3(0.5f, 0.5f, 0.5f) == selectCube && hitRay.transform.name.Contains("Select"))
                            {
                                //Define adjacent voxels to the one clicked on.
                                if (hitRay.collider.name.Contains("LeftPositive")) rayPos.x++;
                                else if (hitRay.collider.name.Contains("LeftNegative")) rayPos.x--;
                                else if (hitRay.collider.name.Contains("TopPositive")) rayPos.y++;
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
                            matView.GetComponentInChildren<Text>().text = "EDITING\nTILE";
                            Destroy(finInd.gameObject);
                            goalPoint = new Vector3(rayPos.x, rayPos.y + 0.5f, rayPos.z);
                            finInd = Instantiate(endModel);
                            finInd.transform.position = goalPoint;
                            finInd.name = "GoalIndicator";
                            selectingStart = false;
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
                            }
                        }
                    }
                }
            }
        }
    }

    //Buttons
    public void OnNewButton()
    {
        //Clear out the old cubes.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
            if (startInd != null) { Destroy(startInd.gameObject); }
        }

        //Build a single cube at 1, 1, 1
        Cuber("Top", developerTop.name, "Right", developerRight.name, "Left", developerLeft.name, new Vector3(1, 1, 1));
    }

    public void OnSaveButton()
    {
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
                Tile inTile = new Tile();
                inTile.d = cuberString;
                inTile.m = cube.GetChild(c).name.Split('_')[2];
                tileData[c] = inTile;
            }
            Cube inCube = new Cube();
            inCube.position = new Vector3(cube.position.x, cube.position.y, cube.position.z);
            inCube.tileData = tileData;
            cubeData[s] = inCube;
        }

        //Save the data into a json file.
        Grid gridSave = new Grid();
        gridSave.cubeData = cubeData;
        gridSave.vertices = vertices.ToArray();
        if (startPoint != null)
        {
            gridSave.startPoint = startPoint;
            gridSave.goalPoint = goalPoint;
            string stringSave = JsonUtility.ToJson(gridSave, true);
            File.WriteAllText(path + saveName.text + ".json", stringSave);
        }
        else
        {
            Debug.Log("ERROR - player start position required.");
        }
        OnRefreshButton();
    }

    public void OnLoadButton(Button button)
    {
        //Clear out the old cubes.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
            if (startInd != null) Destroy(startInd.gameObject);
            if (finInd != null) Destroy(finInd.gameObject);
        }

        //Read and interpret the save file.
        string stringLoad = File.ReadAllText(path + button.transform.parent.name + ".json");
        Grid gridLoad = JsonUtility.FromJson<Grid>(stringLoad);
        startPoint = gridLoad.startPoint;
        startInd = Instantiate(playerModel);
        startInd.position = startPoint;
        startInd.gameObject.name = "PlayerIndicator";
        goalPoint = gridLoad.goalPoint;
        finInd = Instantiate(endModel);
        finInd.position = goalPoint;
        finInd.gameObject.name = "GoalIndicator";
        saveName.text = button.transform.parent.name;
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
    }

    public void OnRefreshButton()
    {
        //Clear old buttons.
        for (int d = 1; d < fileContent.transform.childCount; d++)
        {
            Destroy(fileContent.GetChild(d).gameObject);
        }

        //Build new buttons.
        RectTransform[] fileButton = new RectTransform[Directory.GetFiles(path).Length];
        fileContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, fileTemplate.GetComponent<RectTransform>().rect.height * fileButton.Length + (fileButton.Length * 10) + 10);
        for (int r = 0; r < fileButton.Length; r++)
        {
            fileButton[r] = Instantiate(fileTemplate, fileContent);
            fileButton[r].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -r * 50 - 30);
            string[] fileParse = Directory.GetFiles(path)[r].Split('/', '\\');
            string file = fileParse[fileParse.Length - 1].Replace(".json", "");
            fileButton[r].name = file;
            fileButton[r].GetComponentInChildren<Text>().text = file;
        }
    }

    public void OnMaterialButton(Button button)
    {
        paintMode = true;
        int matNum = int.Parse(button.name);
        matView.GetComponentInChildren<Text>().text = "";
        matView.GetComponent<Image>().material = displayMaterials[matNum];
        selectedMaterial = materials[matNum];
    }

    public void OnTileButton()
    {
        paintMode = false;
        selectingStart = false;
        matView.GetComponentInChildren<Text>().text = "EDITING\nTILE";
        matView.GetComponent<Image>().material = null;
    }

    public void OnStartpointButton()
    {
        if (selectingStart)
        {
            selectingStart = false;
            matView.GetComponentInChildren<Text>().text = "SETTING\nGOAL";
            matView.GetComponent<Image>().material = null;
            return;
        }
        selectingStart = true;
        matView.GetComponentInChildren<Text>().text = "SETTING\nSTART";
        matView.GetComponent<Image>().material = null;
    }

    public void OnDeleteButton(Button button)
    {
        File.Delete(path + button.transform.parent.name + ".json");
        OnRefreshButton();
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
        tile.AddComponent<MeshFilter>();
        MeshFilter tileF = tile.GetComponent<MeshFilter>();
        tile.AddComponent<MeshRenderer>();
        MeshRenderer tileR = tile.GetComponent<MeshRenderer>();
        tile.AddComponent<BoxCollider>();
        BoxCollider tileC = tile.GetComponent<BoxCollider>();
        if (!GameManager.SharedInstance.InLevelEditor)
            tileC.enabled = false;

        //Define and populate the variables and vectors.
        Material loadMat = null;
        if (mat != "")
        {
            string matParse;
            for (int m = 0; m < materials.Length; m++)
            {
                matParse = materials[m].name;
                if (matParse.Contains(mat))
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
        Collider[] clearOldSelect = Physics.OverlapBox(selectCube, new Vector3(1.5f, 1.5f, 1.5f));
        for (int c = 0; c < clearOldSelect.Length; c++)
        {
            if (clearOldSelect[c].name.Contains("Select")) Destroy(clearOldSelect[c].gameObject);
        }
        SelectBuilder("Select_LeftPositive", selectCubeModel, parent, new Vector3(selPos.x + dist, selPos.y, selPos.z), size);
        if (parent.position.x > 1) SelectBuilder("Select_LeftNegative", selectCubeModel, parent, new Vector3(selPos.x - dist, selPos.y, selPos.z), size);
        SelectBuilder("Select_TopPositive", selectCubeModel, parent, new Vector3(selPos.x, selPos.y + dist, selPos.z), size);
        if (parent.position.y > 1) SelectBuilder("Select_TopNegative", selectCubeModel, parent, new Vector3(selPos.x, selPos.y - dist, selPos.z), size);
        SelectBuilder("Select_RightPositive", selectCubeModel, parent, new Vector3(selPos.x, selPos.y, selPos.z + dist), size);
        if (parent.position.z > 1) SelectBuilder("Select_RightNegative", selectCubeModel, parent, new Vector3(selPos.x, selPos.y, selPos.z - dist), size);
        return selPos + new Vector3(0.5f, 0.5f, 0.5f);


    }

    //Build a selector cube at a specified position.
    void SelectBuilder(string name, Transform transform, Transform parent, Vector3 pos, float size)
    {
        Transform buildSelect = Instantiate(transform, pos, Quaternion.identity, parent);
        buildSelect.localScale = new Vector3(size * 2, size * 2, size * 2);
        buildSelect.name = name;
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
    void DeleteCube(Transform x, Transform y, Transform z, Vector3 rayPos, Vector3 delPos, int array)
    {
        if (delPos == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z))
        {
            z = transform.GetChild(array);
            Tiler("Left", z, "Left", "");
        }
        if (delPos == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z))
        {
            y = transform.GetChild(array);
            Tiler("Top", y, "Top", "");
        }
        if (delPos == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1))
        {
            x = transform.GetChild(array);
            Tiler("Right", x, "Right", "");
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

    //Load from .json as a playable level.
    public Grid LoadLevel(string levelName, bool silhouetted)
    {
        //Clear out the old cubes.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
        }

        //Check silhouetted and turn the mode on if true.
        if (silhouetted) { silhouetteMode = true; }
        else { silhouetteMode = false; }

        //Read and interpret the save file.
        //string stringLoad = File.ReadAllText(path + levelName + ".json");
        TextAsset stringLoad = Resources.Load(levelName) as TextAsset;
        Grid gridLoad = JsonUtility.FromJson<Grid>(stringLoad.text);
        int loadCount = gridLoad.cubeData.Length;
        Cube loadCube;
        for (int l = 0; l < loadCount; l++)
        {
            loadCube = gridLoad.cubeData[l];
            Vector3 loadPos = new Vector3(loadCube.position.x - 0.5f, loadCube.position.y -0.5f, loadCube.position.z -0.5f);
            string[] toCuber = new string[3] {"", "", ""};
            string[] toMatter = new string[3] {"", "", ""};
            for (int c = 0; c < loadCube.tileData.Length; c++)
            {
                string loadMat = loadCube.tileData[c].m;
                if (loadCube.tileData[c].d == "Top")
                {
                    toCuber[0] = "Top";
                    toMatter[0] = (loadMat == null) ? "developerTop" : loadMat;
                }
                else if (loadCube.tileData[c].d == "Right")
                {
                    toCuber[1] = "Right";
                    toMatter[1] = (loadMat == null) ? "developerRight" : loadMat;
                }
                else if (loadCube.tileData[c].d == "Left")
                {
                    toCuber[2] = "Left";
                    toMatter[2] = (loadMat == null) ? "developerLeft" : loadMat;
                }
            }

            //Build the new level.
            Cuber(toCuber[0], toMatter[0], toCuber[1], toMatter[1], toCuber[2], toMatter[2], new Vector3(loadPos.x + 1, loadPos.y + 1, loadPos.z + 1));
        }
        return gridLoad;
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
}