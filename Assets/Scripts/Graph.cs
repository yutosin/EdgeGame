using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Graph
{
    // A user define class to represent a graph. 
    // A graph is an array of adjacency lists. 
    // Size of array will be V (number of vertices 
    // in graph) 
    int V;
    List<int>[] adjListArray;
    private List<int> _pointIds; //used to map graph ids to ids from points in world space
    //used to map pt ids to graph ids, specifically when generating an edge and pt ids are the only available id to pass into graph
    private Dictionary<int, int> _graphIdLookup;

    // constructor 
    public Graph(int V)
    {
        this.V = V;

        // define the size of array as 
        // number of vertices 
        adjListArray = new List<int>[V];

        // Create a new list for each vertex 
        // such that adjacent nodes can be stored 

        for (int i = 0; i < V; i++)
        {
            adjListArray[i] = new List<int>();
        }
    }
    
    public Graph(int V, List<int> ids)
    {
        this.V = V;
        _graphIdLookup = new Dictionary<int, int>(ids.Count);
        _pointIds = ids;

        // define the size of array as 
        // number of vertices 
        adjListArray = new List<int>[V];

        // Create a new list for each vertex 
        // such that adjacent nodes can be stored 

        for (int i = 0; i < V; i++)
        {
            adjListArray[i] = new List<int>();
            _graphIdLookup[ids[i]] = i;
        }
    }

    //TODO: don't add duplicate edges
    // Adds an edge to an undirected graph 
    public void addEdge(int src, int dest)
    {
        int convertedSrc = _graphIdLookup[src];
        int convertedDest= _graphIdLookup[dest];
        
        // Add an edge from src to dest. 
        adjListArray[convertedSrc].Add(convertedDest);

        // Since graph is undirected, add an edge from dest 
        // to src also 
        adjListArray[convertedDest].Add(convertedSrc);
    }

    bool DFSUtil(int firstV, int v, bool[] visited, List<int> facePoints, bool validFace)
    {
        // Mark the current node as visited and print it 
        visited[v] = true;
        var vertexAdjList = adjListArray[v];
        if (vertexAdjList.Count >= 2)
            facePoints.Add(_pointIds[v]);
        //Debug.Log(v + " | ");

        // Recur for all the vertices 
        // adjacent to this vertex 
        foreach (int x in vertexAdjList)
        {
            if (!visited[x])
                DFSUtil(firstV,x, visited, facePoints, validFace);
            if (facePoints.Count >= 4 && x == firstV)
                validFace = true;
        }

        return validFace;
    }

    private bool BFFaceFinder(int v, bool[] graphVisited, int firstV, List<int> facePoints, bool[] inFace)
    {
        List<int> bfQueue = new List<int>(); //using as a queue

        bfQueue.Add(v);
        while (bfQueue.Count > 0)
        {
            int vertex = bfQueue[0];
            bfQueue.RemoveAt(0);
            var subVertexAdjList = adjListArray[vertex];
            if (subVertexAdjList.Count < 2)
            {
                graphVisited[vertex] = true;
                return false;
            }
            
            if (subVertexAdjList.Count > 2)
            {
                facePoints.Add(_pointIds[vertex]);
            }
            else
            {
                graphVisited[vertex] = true;
                facePoints.Add(_pointIds[vertex]);
            }

            foreach (int x in subVertexAdjList)
            {
                if (x == firstV)
                    continue;
                if (adjListArray[x].Count < 2)
                {
                    graphVisited[x] = true;
                    continue;
                }
                if (graphVisited[x])
                    continue;
                
                if (adjListArray[x].Count >= 2)
                {
                    if (bfQueue.Contains(x))
                    {
                        facePoints.Add(_pointIds[x]);
                        graphVisited[x] = adjListArray[x].Count == 2;
                        return true;
                    }

                    bfQueue.Add(x);
                }

                // if (graphVisited[x] && bfQueue.Contains(x))
                // {
                //     return true;
                // }
            }
        }

        return false;
    }
    
    //TODO: Don't return duplicates or just dont generate faces w/same points, whichever is easier
    public List<List<int>> connectedComponents()
    {
        // Mark all the vertices as not visited 
        bool[] visited = new bool[V];
        bool[] inFace = new bool[V];
        List<List<int>> validFaces = new List<List<int>>();
        for (int v = 0; v < V; ++v)
        {
            if (!visited[v])
            {
                List<int> facePoints = new List<int>(4);
                // print all reachable vertices 
                // from v 
                //DFSUtil(v, v, visited, facePoints, false);
                bool validFace = BFFaceFinder(v, visited, v, facePoints, inFace);
                //Debug.Log("");
                if (facePoints.Count == 4 && validFace)
                    validFaces.Add(facePoints);
            }
        }

        return validFaces;
    }
}
