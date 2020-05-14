using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineRendering : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private LineRenderer _lineRenderer;

    public Material lineMat;
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _meshRenderer = GetComponent<MeshRenderer>();
        Bounds meshBounds = _meshRenderer.bounds;

        Vector3 maxPoint = _meshRenderer.bounds.max;
        Vector3 maxPoint2 = new Vector3(maxPoint.x - (meshBounds.extents.x * 2), maxPoint.y, maxPoint.z);
        Vector3 maxPoint3 = new Vector3(maxPoint.x, maxPoint.y, maxPoint.z - (meshBounds.extents.z * 2));
        Vector3 maxPoint4 = new Vector3(maxPoint3.x - (meshBounds.extents.x * 2), maxPoint.y, maxPoint3.z);
        Vector3 maxPoint5 = new Vector3(maxPoint4.x, maxPoint.y, maxPoint4.z + (meshBounds.extents.x * 2));
        
        List<Vector3> pos = new List<Vector3>();
        pos.Add(maxPoint4);
        pos.Add(maxPoint2);
        pos.Add(maxPoint);
        pos.Add(maxPoint3);
        _lineRenderer.material = lineMat;
        _lineRenderer.startWidth = .05f;
        _lineRenderer.endWidth = .05f;
        _lineRenderer.positionCount = pos.Count;
        _lineRenderer.SetPositions(pos.ToArray());
        _lineRenderer.useWorldSpace = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
