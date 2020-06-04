using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
//TODO: once again have to handle repeat faces not being generated; prolly easy but i'm tired
//To above: kinda handled this; now need to handle...breaking large face into subfaces and deleting the big face??
public class Graph
{
    // A user define class to represent a graph. 
    // A graph is an array of connected vertices lists. 
    // Size of array will be numVertices (number of vertices 
    // in graph) 
    int numVertices;
    List<int>[] connectedListArray;
    private List<int> _pointIds; //used to map graph ids to ids from points in world space
    //used to map pt ids to graph ids, specifically when generating an edge and pt ids are the only available id to pass into graph
    private Dictionary<int, int> _graphIdLookup;
    private List<int> _createdFaceIds;
    private List<List<int>> _faces;
    
    //Data structures for Johnson's Algorithm Implement
    private Stack<int> _cycleStack;
    private HashSet<int> _blocked;
    private Dictionary<int, HashSet<int>> _bSets;
    private List<List<int>> _cycles;
    
    // constructor 
    public Graph(int numVertices, List<int> ids)
    {
        this.numVertices = numVertices;
        _graphIdLookup = new Dictionary<int, int>(ids.Count);
        _pointIds = ids;

        // define the size of array as 
        // number of vertices 
        connectedListArray = new List<int>[numVertices];

        _createdFaceIds = new List<int>();
        _faces = new List<List<int>>();
        
        _cycleStack = new Stack<int>();
        _blocked = new HashSet<int>();
        _bSets = new Dictionary<int, HashSet<int>>();
        _cycles = new List<List<int>>();

        // Create a new list for each vertex 
        // such that connected nodes can be stored 
        for (int i = 0; i < numVertices; i++)
        {
            //since ids are sorted before add them, we basically map the world space vertex id to the 0 based index graphs
            //use so that all the operations are easier to deal with
            connectedListArray[i] = new List<int>();
            _graphIdLookup[ids[i]] = i;
        }
    }
    
    // Adds an edge to an undirected graph 
    public bool addEdge(int src, int dest, bool isSubEdge = false)
    {
        //pts in "world space" have no knowledge of a graphs internal vertex indexing system and pass in the world space
        //ids; we need to convert those ids to graph ids with our lookup table
        int convertedSrc = _graphIdLookup[src];
        int convertedDest= _graphIdLookup[dest];
        
        var srcConnectedList = connectedListArray[convertedSrc];
        var destConnectedList = connectedListArray[convertedDest];

        if (srcConnectedList.Contains(convertedDest) || destConnectedList.Contains(convertedSrc)) //prevent duplicate edges
            return false;
        
        // Add an edge from src to dest.
        srcConnectedList.Add(convertedDest);

        // Since graph is undirected, add an edge from dest 
        // to src also
        destConnectedList.Add(convertedSrc);

        findCycleUtil(convertedSrc);
        
        if (connectedListArray[convertedDest].Count > 2)
            findCycleUtil(convertedDest);
        
        DefineFaces();
        
        ClearState();
        
        return true;
    }

    private void findCycleUtil(int startIndex)
    {
        foreach (var x in connectedListArray[startIndex])
        {
            _blocked.Remove(x);
            GetVertexBSet(x).Clear();
        }
        
        findCyclesFromVertex(startIndex, startIndex, -1);
        
        foreach (var cycle in _cycles)
        {
            string cycleString = "";
            foreach (var vertex in cycle)
            {
                cycleString += vertex + " ";
            }
            Debug.Log(cycleString);
        }
        
        
    }

    private void ClearState()
    {
        _blocked.Clear();
        _cycleStack.Clear();
        _bSets.Clear();
        _cycles.Clear();
    }
    
    private void UnblockVertex(int vertex)
    {
        _blocked.Remove(vertex);
        var bSet = GetVertexBSet(vertex);
        foreach (int blockedVert in bSet)
        {
            if (_blocked.Contains(blockedVert))
                UnblockVertex(blockedVert);
        }
        bSet.Clear();
    }

    private HashSet<int> GetVertexBSet(int vertex)
    {
        if (_bSets.TryGetValue(vertex, out HashSet<int> bSet))
        {
            return bSet;
        }

        _bSets.Add(vertex, new HashSet<int>());
        return _bSets[vertex];
    }

    private bool findCyclesFromVertex(int startIndex, int vertexIndex, int parentIndex)
    {
        bool foundCycle = false;
        List<int> connectedList = connectedListArray[vertexIndex];
        if (connectedList.Count < 2)
            return false;
        _cycleStack.Push(vertexIndex);
        _blocked.Add(vertexIndex);

        // List<int> connectedList = connectedListArray[vertexIndex];
        // if (connectedList.Count < 2)
        //     return false;

        foreach (int x in connectedList)
        {
            if (x == startIndex && x != parentIndex)
            {
                List<int> cycle = new List<int>(_cycleStack);
                _cycles.Add(cycle);
                foundCycle = true;
            }
            else if (!_blocked.Contains(x))
            {
                bool gotCycle = findCyclesFromVertex(startIndex, x, vertexIndex);
                foundCycle = foundCycle || gotCycle;
            }
        }

        if (foundCycle)
        {
            UnblockVertex(vertexIndex);
        }
        else
        {
            foreach (int x in connectedList)
            {
                GetVertexBSet(x).Add(vertexIndex);
            }
        }

        _cycleStack.Pop();
        return foundCycle;
    }
    
    private void DefineFaces()
    {
        foreach (var cycle in _cycles)
        {
            List<int> face = new List<int>(4);
            int idSum = 0;
            foreach (var vertex in cycle)
            {
                face.Add(_pointIds[vertex]);
                idSum += vertex;
            }
            if (_createdFaceIds.Contains(idSum))
                continue;
            _createdFaceIds.Add(idSum);
            //face.Sort(); //don't sort; need to maintain the path order to properly determine corner vertices
            _faces.Add(face);
        }
    }

    public List<List<int>> FindFaces()
    {
        //Leving this around cause i might use the logic to handle sub-face vs large face issue
        // foreach (var cycle in _cycles)
        // {
        //     List<int> face = new List<int>(4);
        //     int idSum = 0;
        //     foreach (var vertex in cycle)
        //     {
        //         face.Add(_pointIds[vertex]);
        //         idSum += vertex;
        //     }
        //     if (_createdFaceIds.Contains(idSum))
        //         continue;
        //     _createdFaceIds.Add(idSum);
        //     _faces.Add(face);
        // }
        var faceCopy = new List<List<int>>(_faces);
        _faces.Clear();
        
        return faceCopy;
    }
}
