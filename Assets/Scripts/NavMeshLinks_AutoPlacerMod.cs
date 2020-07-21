using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif


namespace NavmeshLinksGenerator
{

    public class NavMeshLinks_AutoPlacerMod : GraphModifier
    {
        #region Variables
        
        public bool generateOnPostScan = true;
        public Transform linkPrefab;
        
        private List<NodeLink2> nodeLinks = new List<NodeLink2>();

        #endregion
        
        #region LinkGen

        public void Generate()
        {
            if (linkPrefab == null) return;
            
            var gg = AstarPath.active.data.gridGraph;
            if (gg == null)
                return;
            ClearLinks();
            gg.GetNodes(node => {
                if (!node.Walkable)
                    return;
                var gn = node as GridNode;
                BuildNodeLinks(gn, gg);
            });

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }
        
        private void BuildNodeLinks(GridNode gridNode, GridGraph levelGraph)
        {
            int nodeX = gridNode.XCoordinateInGrid;
            int nodeZ = gridNode.ZCoordinateInGrid;

            for (int x = nodeX - 1; x < nodeX + 2; x++)
            {
                for (int z = nodeZ - 1; z < nodeZ + 2; z++)
                {
                    GridNode subNode = levelGraph.GetNode(x, z) as GridNode;
                    if (subNode == null)
                        continue;
                    if (!subNode.Walkable)
                        continue;
                    if (Mathf.Abs(gridNode.position.x - subNode.position.x) > 0 && Mathf.Abs(gridNode.position.z - subNode.position.z) > 0)
                        continue;
                    if (Mathf.Abs(gridNode.position.y - subNode.position.y) == 1000)
                    {
                        Transform linkObject = Instantiate(linkPrefab, (Vector3)gridNode.position, Quaternion.identity);

                        NodeLink2 nodeLink = linkObject.GetComponent<NodeLink2>();
                        GameObject endPoint = new GameObject("end");
                        endPoint.transform.position = (Vector3)subNode.position;
                        endPoint.transform.SetParent(linkObject, true);
                        nodeLink.end = endPoint.transform;

                        linkObject.SetParent(transform);

                        nodeLinks.Add(nodeLink);
                    }
                }
            }
        }

        public override void OnPostScan()
        {
            if (generateOnPostScan)
                Generate();

            if (nodeLinks == null)
                return;
            
            foreach (NodeLink2 nodeLink in nodeLinks)
            {
                nodeLink.OnPostScan();
            }
        }

        public override void OnGraphsPostUpdate()
        {
            if (nodeLinks == null)
                return;
            
            foreach (NodeLink2 nodeLink in nodeLinks)
            {
                nodeLink.OnGraphsPostUpdate();
            }
        }

        public void ClearLinks()
        {
            List<NodeLink2> navMeshLinkList = GetComponentsInChildren<NodeLink2>().ToList();
            while (navMeshLinkList.Count > 0)
            {
                GameObject obj = navMeshLinkList[0].gameObject;
                if (obj != null) DestroyImmediate(obj);
                navMeshLinkList.RemoveAt(0);
            }
            
            nodeLinks.Clear();
        }

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(NavMeshLinks_AutoPlacerMod))]
    [CanEditMultipleObjects]
    public class NavMeshLinks_AutoPlacer_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinks_AutoPlacerMod) targ).Generate();
                }
            }

            if (GUILayout.Button("ClearLinks"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinks_AutoPlacerMod) targ).ClearLinks();
                }
            }
        }
    }

#endif
}