using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
//TODO: once again have to handle repeat faces not being generated; prolly easy but i'm tired
//TODO: not necessarily in this class, but we need to break down large edges into sub edges that bi-connect (1->2->3/3->2->1)
//TODO: use vertex struct and pass in "x/y" values to do corner detection
//TODO: consider overlapping edge/sub-edge possibility
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
    
    //BFFaceFinder uses a modified breadth first traversal approach where once a vertex is selected, we add the connected
    //vertices to a queue data structure (first item in is the first item out) then inspect these connected vertices
    //to see what they're connected to. Then we add those inspected vertices to the queue and repeat the same process until
    //all vertices are traversed. But for our purposes we modify this to check if the connected vertices of the first
    //inspected vertex share a connected vertex meaning they create a closed loop or a face.
    private bool BFFaceFinder(int v, bool[] graphVisited, int firstVertex, List<int> faceVertices)
    {
        Queue<int> bfQueue = new Queue<int>();
        //add first vertex to front of the queue
        bfQueue.Enqueue(v);
        
        while (bfQueue.Count > 0)
        {
            int vertex = bfQueue.Peek(); //peek returns first item in queue w/o removing it
            bfQueue.Dequeue();
            
            var connectedVertexList = connectedListArray[vertex];
            //if a vertex doesn't have at least to vertices connected to it, there's no way it can form a square/closed loop so we jet out
            if (connectedVertexList.Count < 2)
            {
                graphVisited[vertex] = true;
                return false;
            }
            
            //there are cases where a vertex may be part of more than one face meaning it's probably connected to at least
            //4 vertices. we don't want to make those types of vertices as visited because we need to return to it
            // and determine if its other connections are a valid face/closed loop. so we only set it to visited for
            //two connections indicating only one face.
            graphVisited[vertex] = connectedVertexList.Count == 2;
            faceVertices.Add(_pointIds[vertex]);
            
            //loop through connected vertices and add valid vertices to queue
            foreach (int x in connectedVertexList)
            {
                if (x == firstVertex)
                    continue;
                //same logic as earlier; vertices w/o two connections are dead ends so skip em and mark visited
                if (connectedListArray[x].Count < 2)
                {
                    graphVisited[x] = true;
                    continue;
                }
                if (graphVisited[x])
                    continue;

                //this checks if the connected vertices of the first vertex share a connected vertex; this works because
                //consider pt1 and pt2 both connected to pt0 with both their list of connected vertices containing pt3
                //when pt1 is visited first, it goes through it's list of connected vertices and adds pt3 to the queue
                //then pt2 is inspected and before we enqueue its vertices we check to see if the queue already contains
                //it meaning both pt1 and pt2 connect to pt3 and pt0 making a face
                if (bfQueue.Contains(x))
                {
                    faceVertices.Add(_pointIds[x]);
                    graphVisited[x] = connectedListArray[x].Count == 2;
                    return true;
                }
                    
                bfQueue.Enqueue(x);
            }
        }

        return false;
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
        _cycleStack.Push(vertexIndex);
        _blocked.Add(vertexIndex);

        List<int> connectedList = connectedListArray[vertexIndex];
        if (connectedList.Count < 2)
            return false;

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

    //TODO: Don't return duplicates or just dont generate faces w/same points, whichever is easier
    // public List<List<int>> FindFaces()
    // {
    //     // Mark all the vertices as not visited 
    //     bool[] visitedVertices = new bool[numVertices];
    //     //whenever a face is found in the graph (4 points that connect to each other cyclically) it's added to the
    //     //faces list; face is a list on ints corresponding to vertex (point) ids
    //     List<List<int>> validFaces = new List<List<int>>();
    //     
    //     //we loop through all the vertices in the graph but thanks to the visitedVertices array and some other checks
    //     //we avoid making unnecessary trips along a vertex's connected vertices
    //     for (int vertex = 0; vertex < numVertices; ++vertex)
    //     {
    //         if (!visitedVertices[vertex])
    //         {
    //             List<int> faceVertices = new List<int>(4);
    //             //starting from vertex 0, find all faces in the graph through modified breadth first graph traversal
    //             bool validFace = BFFaceFinder(vertex, visitedVertices, vertex, faceVertices);
    //             int idSum = 0;
    //             foreach (var faceVertex in faceVertices)
    //                 idSum += faceVertex;
    //             if (faceVertices.Count == 4 && validFace && !_createdFaceIds.Contains(idSum))
    //             {
    //                 _createdFaceIds.Add(idSum);
    //                 validFaces.Add(faceVertices);
    //             }
    //         }
    //     }
    //
    //     return validFaces;
    // }
}
