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
    private Tile[] tileData;

    void Start()
    {
        Tile a1 = new Tile();
        a1.d = "Top";
        a1.p = new Vector3(1, 1, 1);
        Tile a2 = new Tile();
        a2.d = "Right";
        a2.p = new Vector3(1, 1, 1);
        Tile a3 = new Tile();
        a3.d = "Left";
        a3.p = new Vector3(1, 1, 1);
        tileData = new Tile[3];
        tileData[0] = a1;
        tileData[1] = a2;
        tileData[2] = a3;
    }

    void Update()
    {
        if (commitSave)
        {
            //Create a TileDataHolder object and assign it the tileData array
            TileDataHolder _tileDataHolder = new TileDataHolder();
            _tileDataHolder.tileData = tileData;
            
            //Convert class to json; this will also serialize the class' properties, in this case the tileData array
            //and since all the objects in the array are serializable there are also properly represented in json
            string tile = JsonUtility.ToJson(_tileDataHolder);
            File.WriteAllText(path, tile);
            commitSave = false;
        }
    }

    void Tiler(int dim, Vector3 pos, int num)
    {
        //Create the gameobject.
        GameObject tile = new GameObject("Tile_" + num);
        tile.transform.SetParent(this.transform);
        tile.transform.position = pos;
        tile.AddComponent<MeshFilter>();
        MeshFilter tileF = tile.GetComponent<MeshFilter>();
        tile.AddComponent<MeshRenderer>();
        MeshRenderer tileR = tile.GetComponent<MeshRenderer>();

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
            case (1):
                tileV[0] = new Vector3(0, -1, -1);
                tileV[1] = new Vector3(0, -1, 0);
                tileV[2] = new Vector3(0, 0, -1);
                tileV[3] = new Vector3(0, 0, 0);
                tileN[0] = Vector3.right;
                tileN[1] = Vector3.right;
                tileN[2] = Vector3.right;
                tileN[3] = Vector3.right;
                tileR.material = leftTile;
                break;
            case (2):
                tileV[0] = new Vector3(0, -1, 0);
                tileV[1] = new Vector3(-1, -1, 0);
                tileV[2] = new Vector3(0, 0, 0);
                tileV[3] = new Vector3(-1, 0, 0);
                tileN[0] = Vector3.forward;
                tileN[1] = Vector3.forward;
                tileN[2] = Vector3.forward;
                tileN[3] = Vector3.forward;
                tileR.material = rightTile;
                break;
            case (3):
                tileV[0] = new Vector3(0, 0, -1);
                tileV[1] = new Vector3(0, 0, 0);
                tileV[2] = new Vector3(-1, 0, -1);
                tileV[3] = new Vector3(-1, 0, 0);
                tileN[0] = Vector3.up;
                tileN[1] = Vector3.up;
                tileN[2] = Vector3.up;
                tileN[3] = Vector3.up;
                tileR.material = topTile;
                break;
            default:
                Debug.LogError("Tile " + num + " has failed to parse.");
                break;
        }

        //Build the mesh.
        if (dim == 1 || dim == 2 || dim == 3)
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
    public Vector3 p;
}

//Serializable class that will hold the tileData array that's created
[System.Serializable]
public class TileDataHolder
{
    public Tile[] tileData;
}