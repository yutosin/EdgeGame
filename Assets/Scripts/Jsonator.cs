using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


/*
TO DO:

    - Create Material system: Player selects Material Mode and a material from drop down, left clicks on a tile to add material, right-clicks to clear it.
    - Setup vector-nodes.
*/

public class Jsonator : MonoBehaviour
{
    [Header("Set Materials")]
    public Material leftTile;
    public Material rightTile;
    public Material topTile;

    [Header("General")]
    public string path;

    [Header("Buttons")]
    public InputField saveName;
    public RectTransform viewportContent;
    public Button buttonTemplate;

    private Transform cubeXNeg;
    private Transform cubeXPos;
    private Transform cubeYNeg;
    private Transform cubeYPos;
    private Transform cubeZNeg;
    private Transform cubeZPos;

    [SerializeField]
    private Cube[] cubeData;
    private Tile[] tileData;

    private void Awake()
    {
        OnRefreshButton();
    }



    void Update()
    {
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
                    //CubeParse(false, null, new Vector3(0,0,0));

                    //Left click, build the voxel.
                    if (leftMouse)
                    {
                        //Define adjacent voxels to the one clicked on.
                        if (hitRay.collider.name.Contains("Left")) { rayPos.x++; }
                        if (hitRay.collider.name.Contains("Top")) { rayPos.y++; }
                        if (hitRay.collider.name.Contains("Right")) { rayPos.z++; }

                        //Curate faces the newly created voxel will be adjacent to.
                        bool[] backFace = new bool[3] { false, false, false };
                        for (int c = 0; c < transform.childCount; c++)
                        {
                            CubeParse(true, transform.GetChild(c).transform, rayPos, c);
                            if (cubeXPos != null) { backFace[0] = true; }
                            if (cubeYPos != null) { backFace[1] = true; }
                            if (cubeZPos != null) { backFace[2] = true; }
                            int tc = transform.GetChild(c).childCount;
                            for (int t = 0; t < tc; t++)
                            {
                                if (CurateCube(cubeXNeg, "Left", t) || CurateCube(cubeYNeg, "Top", t) || CurateCube(cubeZNeg, "Right", t)) { tc--; }
                            }
                            CubeParse(false, null, new Vector3(0, 0, 0), 0);
                        }

                        //Build the new voxel.
                        Cuber(
                            backFace[1] ? "" : "Top",
                            backFace[2] ? "" : "Right",
                            backFace[0] ? "" : "Left",
                            new Vector3Int(Mathf.RoundToInt(rayPos.x + 1), Mathf.RoundToInt(rayPos.y + 1), Mathf.RoundToInt(rayPos.z + 1)));
                        Destroy(hitRay.transform.gameObject);
                    }

                    //Right click, remove voxel.
                    else if (rightMouse)
                    {
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

    void Cuber(string top, string rgt, string lft, Vector3Int pos)
    {
        GameObject cube = new GameObject("Cube " + pos.x + " - " + pos.y + " - " + pos.z);
        cube.transform.SetParent(transform);
        cube.transform.position = new Vector3Int(pos.x - 1, pos.y - 1, pos.z - 1);
        if (top != "") { Tiler("Top", cube.transform, "Top"); }
        if (rgt != "") { Tiler("Right", cube.transform, "Right"); }
        if (lft != "") { Tiler("Left", cube.transform, "Left"); }
    }

    void Tiler(string dim, Transform par, string name)
    {
        //Create the gameobject.
        GameObject tile = new GameObject("Tile_" + name);
        tile.transform.SetParent(par);
        tile.transform.position = new Vector3(par.position.x - 1, par.position.y - 1, par.position.z - 1);
        tile.AddComponent<MeshFilter>();
        MeshFilter tileF = tile.GetComponent<MeshFilter>();
        tile.AddComponent<MeshRenderer>();
        MeshRenderer tileR = tile.GetComponent<MeshRenderer>();
        tile.AddComponent<BoxCollider>();
        BoxCollider tileC = tile.GetComponent<BoxCollider>();

        //Define and populate the variables and vectors.
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
                tileV[0] = new Vector3(0, 0, -1);
                tileV[1] = new Vector3(0, 0, 0);
                tileV[2] = new Vector3(-1, 0, -1);
                tileV[3] = new Vector3(-1, 0, 0);
                tileN[0] = Vector3.up;
                tileN[1] = Vector3.up;
                tileN[2] = Vector3.up;
                tileN[3] = Vector3.up;
                tileC.size = new Vector3(1, 0, 1);
                tileC.center = new Vector3(-0.5f, 0, -0.5f);
                tileR.material = topTile;
                break;
            case ("Right"):
                tileV[0] = new Vector3(0, -1, 0);
                tileV[1] = new Vector3(-1, -1, 0);
                tileV[2] = new Vector3(0, 0, 0);
                tileV[3] = new Vector3(-1, 0, 0);
                tileN[0] = Vector3.forward;
                tileN[1] = Vector3.forward;
                tileN[2] = Vector3.forward;
                tileN[3] = Vector3.forward;
                tileC.size = new Vector3(1, 1, 0);
                tileC.center = new Vector3(-0.5f, -0.5f, 0);
                tileR.material = rightTile;
                break;
            case ("Left"):
                tileV[0] = new Vector3(0, -1, -1);
                tileV[1] = new Vector3(0, -1, 0);
                tileV[2] = new Vector3(0, 0, -1);
                tileV[3] = new Vector3(0, 0, 0);
                tileN[0] = Vector3.right;
                tileN[1] = Vector3.right;
                tileN[2] = Vector3.right;
                tileN[3] = Vector3.right;
                tileC.size = new Vector3(0, 1, 1);
                tileC.center = new Vector3(0, -0.5f, -0.5f);
                tileR.material = leftTile;
                break;
            default:
                Debug.LogError("Tile " + name + " has failed to parse.");
                break;
        }

        //Build the mesh.
        if (dim == "Top" || dim == "Right" || dim == "Left")
        {
            tileF.mesh.vertices = tileV;
            tileF.mesh.normals = tileN;
            tileF.mesh.uv = tileUV;
            tileF.mesh.triangles = tileT;
        }
    }

    //Buttons
    public void OnSaveButton()
    {
        //Check and read all tiles in the field.
        int saveCount = transform.childCount;
        cubeData = new Cube[saveCount];
        for (int s = 0; s < saveCount; s++)
        {
            Transform cube = transform.GetChild(s);
            int tileCount = cube.childCount;
            tileData = new Tile[tileCount];
            for (int c = 0; c < tileCount; c++)
            {
                string saveParse = cube.GetChild(c).name.Split('_')[1];
                string cuberString;
                if (saveParse == "Top" || saveParse == "Right" || saveParse == "Left")
                {
                    cuberString = saveParse;
                }
                else { cuberString = " "; }
                Tile inTile = new Tile();
                inTile.d = cuberString;
                tileData[c] = inTile;
            }
            Cube inCube = new Cube();
            inCube.position = cube.transform.position;
            inCube.tileData = tileData;
            cubeData[s] = inCube;
        }

        //Save the data into a json file.
        Grid gridSave = new Grid();
        gridSave.cubeData = cubeData;
        string stringSave = JsonUtility.ToJson(gridSave, true);
        File.WriteAllText(path + "\\"+ saveName.text + ".json", stringSave);
    }

    public void OnLoadButton(Button button)
    {
        //Clear out the old cubes.
        for (int d = 0; d < transform.childCount; d++)
        {
            Destroy(transform.GetChild(d).gameObject);
        }

        //Read and interpret the save file.
        string stringLoad = File.ReadAllText(path + "\\" + button.name + ".json");
        Grid gridLoad = JsonUtility.FromJson<Grid>(stringLoad);
        int loadCount = gridLoad.cubeData.Length;
        Cube loadCube;
        for (int l = 0; l < loadCount; l++)
        {
            loadCube = gridLoad.cubeData[l];
            Vector3Int loadPos = new Vector3Int((int)loadCube.position.x, (int)loadCube.position.y, (int)loadCube.position.z);
            string[] toCuber = new string[3] { "", "", "" };
            for (int c = 0; c < loadCube.tileData.Length; c++)
            {
                if (loadCube.tileData[c].d == "Top") { toCuber[0] = "Top"; }
                else if (loadCube.tileData[c].d == "Right") { toCuber[1] = "Right"; }
                else if (loadCube.tileData[c].d == "Left") { toCuber[2] = "Left"; }
            }

            //Build the new level.
            Cuber(toCuber[0], toCuber[1], toCuber[2], new Vector3Int(Mathf.RoundToInt(loadPos.x + 1), Mathf.RoundToInt(loadPos.y + 1), Mathf.RoundToInt(loadPos.z + 1)));
        }
    }

    public void OnRefreshButton()
    {
        //Clear old buttons.
        for (int d = 1; d < viewportContent.transform.childCount; d++)
        {
            Destroy(viewportContent.GetChild(d).gameObject);
        }

        //Build new buttons.
        Button[] fileButton = new Button[Directory.GetFiles(path).Length];
        viewportContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, buttonTemplate.GetComponent<RectTransform>().rect.height * fileButton.Length + (fileButton.Length * 10) + 10);
        for (int r = 0; r < fileButton.Length; r++)
        {
            fileButton[r] = Instantiate(buttonTemplate, viewportContent);
            fileButton[r].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -r * 50 - 30);
            string file = Directory.GetFiles(path)[r].Replace("./Resources\\", "").Replace(".json", "");
            fileButton[r].name = file;
            fileButton[r].GetComponentInChildren<Text>().text = Directory.GetFiles(path)[r].Replace("./Resources\\", "").Replace(".json", "");
        }
    }

    void CubeParse(bool mode, Transform target, Vector3 rayPos, int arrayIn)
    {
        if (mode)
        {
            if (target.position == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z)) { cubeXNeg = transform.GetChild(arrayIn); }
            else if (target.position == new Vector3(rayPos.x + 1, rayPos.y, rayPos.z)) { cubeXPos = transform.GetChild(arrayIn); }
            else if (target.position == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z)) { cubeYNeg = transform.GetChild(arrayIn); }
            else if (target.position == new Vector3(rayPos.x, rayPos.y + 1, rayPos.z)) { cubeYPos = transform.GetChild(arrayIn); }
            else if (target.position == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1)) { cubeZNeg = transform.GetChild(arrayIn); }
            else if (target.position == new Vector3(rayPos.x, rayPos.y, rayPos.z + 1)) { cubeZPos = transform.GetChild(arrayIn); }
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

    void DeleteCube(Transform x, Transform y, Transform z, Vector3 rayPos, Vector3 delPos, int array)
    {
        if (delPos == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z))
        {
            z = transform.GetChild(array);
            Tiler("Left", z, "Left");
        }
        if (delPos == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z))
        {
            y = transform.GetChild(array);
            Tiler("Top", y, "Top");
        }
        if (delPos == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1))
        {
            x = transform.GetChild(array);
            Tiler("Right", x, "Right");
        }
    }
}

[System.Serializable]
public class Tile
{
    public string d;
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
}