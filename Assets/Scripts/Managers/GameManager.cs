using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _sharedInstance = null;
    public static GameManager SharedInstance
    {
        get { return _sharedInstance; }
    }

    public Camera MainCamera;
    public LevelManager levelManager;
    public EdgeManager edgeManager;
    public DirectedAgent playerAgent;
    public GridGraph levelGraph;
    public MaterialSelectScript matSelect;
    public UIScript uiManager;
    public Jsonator LevelLoader;
    public InstructionScript InstructionScript;
    public bool PlayMode;
    public bool InLevelEditor;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (_sharedInstance != null)
        {
            Destroy(gameObject);
            return;
        }
		
        _sharedInstance = this;

        MainCamera = Camera.main;
        Physics.queriesHitTriggers = false;
    }

    // Update is called once per frame
    private void Start()
    {
        PlayMode = false;
        if (InLevelEditor)
            return;
        var gg = AstarPath.active.data.gridGraph;
        if (gg == null)
            return;
        levelGraph = gg;
    }

    private void BuildNodeLinks(GridNode gridNode)
    {
        GameObject linkObject = new GameObject("nodeLink");
        linkObject.transform.position = (Vector3)gridNode.position;
        int nodeX = gridNode.XCoordinateInGrid;
        int nodeZ = gridNode.ZCoordinateInGrid;

        for (int x = nodeX - 1; x < nodeX + 2; x++)
        {
            for (int z = nodeZ - 1; z < nodeZ + 2; z++)
            {
                GridNode subNode = levelGraph.GetNode(x, z) as GridNode;
                if (subNode == null)
                    continue;
                if (Mathf.Abs(gridNode.position.y - subNode.position.y) == 1)
                {
                    NodeLink2 nodeLink = linkObject.AddComponent<NodeLink2>();
                    GameObject endPoint = new GameObject("end");
                    endPoint.transform.position = (Vector3)subNode.position;
                    endPoint.transform.SetParent(linkObject.transform, true);
                    nodeLink.end = endPoint.transform;
                }
            }
        }
    }
}
