using System.Collections;
using System.Collections.Generic;
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

    bool DFSUtil(int v, bool[] visited, List<int> facePoints, int firstV)
    {
        // Mark the current node as visited and print it 
        visited[v] = true;
        facePoints.Add(_pointIds[v]);
        //Debug.Log(v + " | ");

        // Recur for all the vertices 
        // adjacent to this vertex 
        foreach (int x in adjListArray[v])
        {
            if (!visited[x])
                return DFSUtil(x, visited, facePoints, firstV);
            if (x == firstV)
                return true;
        }

        return false;
    }
    
    //TODO: Don't return duplicates or just dont generate faces w/same points, whichever is easier
    public List<List<int>> connectedComponents()
    {
        // Mark all the vertices as not visited 
        bool[] visited = new bool[V];
        List<List<int>> validFaces = new List<List<int>>();
        for (int v = 0; v < V; ++v)
        {
            if (!visited[v])
            {
                List<int> facePoints = new List<int>(4);
                // print all reachable vertices 
                // from v 
                bool validFace = DFSUtil(v, visited, facePoints, v);
                //Debug.Log("");
                
                if (facePoints.Count == 4 && validFace)
                    validFaces.Add(facePoints);
            }
        }

        return validFaces;
    }
}
