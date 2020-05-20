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
    private List<int> _idConvert;

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
        _idConvert = ids;

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

    // Adds an edge to an undirected graph 
    public void addEdge(int src, int dest)
    {
        // Add an edge from src to dest. 
        adjListArray[src].Add(dest);

        // Since graph is undirected, add an edge from dest 
        // to src also 
        adjListArray[dest].Add(src);
    }

    void DFSUtil(int v, bool[] visited, List<int> validFace)
    {
        // Mark the current node as visited and print it 
        visited[v] = true;
        validFace.Add(_idConvert[v]);
        Debug.Log(v + " | ");

        // Recur for all the vertices 
        // adjacent to this vertex 
        foreach (int x in adjListArray[v])
        {
            if (!visited[x]) DFSUtil(x, visited, validFace);
        }

    }

    public List<List<int>> connectedComponents()
    {
        // Mark all the vertices as not visited 
        bool[] visited = new bool[V];
        List<List<int>> validFaces = new List<List<int>>();
        for (int v = 0; v < V; ++v)
        {
            if (!visited[v])
            {
                List<int> validFace = new List<int>(4);
                // print all reachable vertices 
                // from v 
                DFSUtil(v, visited, validFace);
                Debug.Log("");
                
                if (validFace.Count == 4)
                    validFaces.Add(validFace);
            }
        }

        return validFaces;
    }
}
