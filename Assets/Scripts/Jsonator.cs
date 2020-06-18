using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Jsonator : MonoBehaviour
{
    [Header("Set Materials")]
    public Material leftTile;
    public Material rightTile;
    public Material topTile;

    [Header("General")]
    public string path;

    [Header("Buttons")]
    public bool commitSave;
    public bool commitLoad;

    [SerializeField]
    private Cube[] cubeData;
    private Tile[] tileData;

    void Update()
    {
        if (commitSave)
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
                    Debug.Log(tileData[c].d);
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
            File.WriteAllText(path, stringSave);

            commitSave = false;
        }

        if (commitLoad)
        {
            string stringLoad = File.ReadAllText(path);
            Grid gridLoad = JsonUtility.FromJson<Grid>(stringLoad);
            int loadCount = gridLoad.cubeData.Length;
            Cube loadCube;
            for (int l = 0; l < loadCount; l++)
            {
                loadCube = gridLoad.cubeData[l];
                Vector3 loadPos = loadCube.position;
                string[] toCuber = new string[3] { "", "", "" };
                for (int c = 0; c < loadCube.tileData.Length; c++)
                {
                    if (loadCube.tileData[c].d == "Top") { toCuber[0] = "Top"; }
                    else if (loadCube.tileData[c].d == "Right") { toCuber[1] = "Right"; }
                    else if (loadCube.tileData[c].d == "Left") { toCuber[2] = "Left"; }
                }
                Cuber(toCuber[0], toCuber[1], toCuber[2], new Vector3(loadPos.x + 1, loadPos.y + 1, loadPos.z + 1));
            }
            commitLoad = false;
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
                    Transform cubeXNeg = null;
                    Transform cubeXPos = null;
                    Transform cubeYNeg = null;
                    Transform cubeYPos = null;
                    Transform cubeZPos = null;
                    Transform cubeZNeg = null;
                    Transform cubeParse;

                    //Left click, build the voxel.
                    if (leftMouse)
                    {
                        //Define adjacent voxels to the one clicked on.
                        string side = "";
                        if (hitRay.collider.name.Contains("Left")) { side = "Left"; }
                        else if (hitRay.collider.name.Contains("Top")) { side = "Top"; }
                        else if (hitRay.collider.name.Contains("Right")) { side = "Right"; }

                        if (side == "Left") { rayPos.x += 1; }
                        else if (side == "Top") { rayPos.y += 1; }
                        else if (side == "Right") { rayPos.z += 1; }

                        for (int c = 0; c < transform.childCount; c++)
                        {
                            cubeParse = transform.GetChild(c).transform;
                            if (cubeParse.position == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z)) { cubeXNeg = transform.GetChild(c); }
                            if (cubeParse.position == new Vector3(rayPos.x + 1, rayPos.y, rayPos.z)) { cubeXPos = transform.GetChild(c); }
                            if (cubeParse.position == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z)) { cubeYNeg = transform.GetChild(c); }
                            if (cubeParse.position == new Vector3(rayPos.x, rayPos.y + 1, rayPos.z)) { cubeYPos = transform.GetChild(c); }
                            if (cubeParse.position == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1)) { cubeZNeg = transform.GetChild(c); }
                            if (cubeParse.position == new Vector3(rayPos.x, rayPos.y, rayPos.z + 1)) { cubeZPos = transform.GetChild(c); }

                            //Curate faces of newly adjoined or exposed faces.
                            if (side == "Right")
                            {
                                for (int t = 0; t < transform.GetChild(c).childCount; t++)
                                {
                                    if (cubeXNeg != null)
                                    {
                                        if (cubeXNeg.GetChild(t).name.Contains("Left"))
                                        {
                                            Destroy(cubeXNeg.GetChild(t).gameObject);
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (side == "Top")
                            {
                                for (int t = 0; t < transform.GetChild(c).childCount; t++)
                                {
                                    if (cubeYNeg != null)
                                    {
                                        if (cubeYNeg.GetChild(t).name.Contains("Top"))
                                        {
                                            Destroy(cubeYNeg.GetChild(t).gameObject);
                                            break;
                                        }
                                    }
                                    else if (cubeXNeg != null)
                                    {
                                        if (cubeXNeg.GetChild(t).name.Contains("Left"))
                                        {
                                            Destroy(cubeYNeg.GetChild(t).gameObject);
                                        }
                                    }
                                }
                            }
                            else if (side == "Left")
                            {
                                for (int t = 0; t < transform.GetChild(c).childCount; t++)
                                {
                                    if (cubeZNeg != null)
                                    {
                                        if (cubeZNeg.GetChild(t).name.Contains("Right"))
                                        {
                                            Destroy(cubeZNeg.GetChild(t).gameObject);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        //Build the voxel.
                        Cuber((cubeYPos != null) ? "" : "Top", (cubeZPos != null) ? "" : "Right", (cubeXPos != null) ? "" : "Left", new Vector3(rayPos.x + 1, rayPos.y + 1, rayPos.z + 1));
                        Destroy(hitRay.transform.gameObject);
                    }

                    //Right click, remove voxel.
                    else if (rightMouse)
                    {
                        if (hitRay.collider.name.Contains("Left"))
                        {
                            Destroy(hitRay.transform.parent.gameObject);
                            for (int r = 0; r < transform.childCount; r++)
                            {
                                cubeParse = transform.GetChild(r).transform;
                                if (cubeParse.position == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z))
                                {
                                    cubeZPos = transform.GetChild(r);
                                    Tiler("Left", cubeZPos, "Left");
                                }
                                if (cubeParse.position == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1))
                                {
                                    cubeZNeg = transform.GetChild(r);
                                    Tiler("Right", cubeZNeg, "Right");
                                }
                                if (cubeParse.position == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z))
                                {
                                    cubeYPos = transform.GetChild(r);
                                    Tiler("Top", cubeYPos, "Top");
                                }
                            }
                        }
                        else if (hitRay.collider.name.Contains("Right"))
                        {
                            Destroy(hitRay.transform.parent.gameObject);
                            for (int r = 0; r < transform.childCount; r++)
                            {
                                cubeParse = transform.GetChild(r).transform;
                                if (cubeParse.position == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1))
                                {
                                    cubeXPos = transform.GetChild(r);
                                    Tiler("Right", cubeXPos, "Right");
                                }
                                if (cubeParse.position == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z))
                                {
                                    cubeXPos = transform.GetChild(r);
                                    Tiler("Left", cubeXPos, "Left");
                                }
                                if (cubeParse.position == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z))
                                {
                                    cubeYPos = transform.GetChild(r);
                                    Tiler("Top", cubeYPos, "Top");
                                }
                            }
                        }
                        else if (hitRay.collider.name.Contains("Top"))
                        {
                            Destroy(hitRay.transform.parent.gameObject);
                            for (int r = 0; r < transform.childCount; r++)
                            {
                                cubeParse = transform.GetChild(r).transform;
                                if (cubeParse.position == new Vector3(rayPos.x, rayPos.y, rayPos.z - 1))
                                {
                                    cubeZPos = transform.GetChild(r);
                                    Tiler("Right", cubeZPos, "Right");
                                }
                                if (cubeParse.position == new Vector3(rayPos.x - 1, rayPos.y, rayPos.z))
                                {
                                    cubeXPos = transform.GetChild(r);
                                    Tiler("Left", cubeXPos, "Left");
                                }
                                if (cubeParse.position == new Vector3(rayPos.x, rayPos.y - 1, rayPos.z))
                                {
                                    cubeYPos = transform.GetChild(r);
                                    Tiler("Top", cubeYPos, "Top");
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void Cuber(string top, string rgt, string lft, Vector3 pos)
    {
        GameObject cube = new GameObject("Cube " + pos.x + " - " + pos.y + " - " + pos.z);
        cube.transform.position = new Vector3(pos.x - 1, pos.y - 1, pos.z - 1);
        cube.transform.SetParent(this.transform);
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