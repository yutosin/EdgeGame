using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Face : MonoBehaviour
{
    private NavMeshPath path;
    RaycastHit m_HitInfo = new RaycastHit();
    
    private Vector3 _target;
    public Renderer _rend;
    private bool _selected;
    private static Face _selectedFace;
    public Material _defaultMat;
    public Material _selectedMat;
    
    public IFaceAbility Ability;
    public Transform Parent;
    public Vector3[] Vertices;
    public int FaceId;
    public List<TileComponent> Tiles;

    private MaterialSelectScript _mScript;

    // Start is called before the first frame update
    void Start()
    {
        path = new NavMeshPath();
        if (Tiles.Count > 0)
            _rend = Tiles[0].GetComponent<Renderer>();
        else
            _rend = GetComponent<Renderer>(); 
        Parent = gameObject.transform.parent;
        
        //Leaving this around just in case...
        // _defaultMat = new Material(Shader.Find("Unlit/ColorZAlways"));
        // _defaultMat.color = Color.gray;
        // _defaultMat.renderQueue = 2001;

        _defaultMat = _rend.material;

        _selectedMat = new Material(Shader.Find("Unlit/ColorZAlways"));
        Color selectColor = new Color();
        if (ColorUtility.TryParseHtmlString("#0979AD", out selectColor))
            _selectedMat.color = selectColor;
        else
            _selectedMat.color = Color.blue;
        _selectedMat.renderQueue = 2005;

        _mScript = GameObject.Find("GameManager").GetComponent<MaterialSelectScript>();
        /*if(Vertices.Length == 0)
        {
            FillEmptyVerts();
        }*/
    }

    private void FillEmptyVerts()
    {
        MeshFilter meshRend = GetComponent<MeshFilter>();

        Vertices = meshRend.mesh.vertices;
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i] = transform.TransformPoint(Vertices[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.SharedInstance.PlayMode)
        {
            if (Ability == null)
                return;
            Vector3 facePoint = gameObject.transform.parent.position;
            Vector3 agentPoint = GameManager.SharedInstance.playerAgent.transform.position;
            
            if (Vector3.Distance(facePoint, agentPoint) <= 0.5f && Input.GetKeyDown(KeyCode.E))
            {
                if (Ability.AbilityTimes <= 0)
                    return;
                Ability.InitializeAbility(this);
            }
        }
    }

    private void PlayModeMouseDown()
    {
        GameManager.SharedInstance.playerAgent.HandleFacePointSelect();
    }

    private void DrawModeMouseDown()
    {
        var ray = GameManager.SharedInstance.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray.origin, ray.direction);
        float lowestHitDistance = 0;
        Face lastFace = null;
        for (int i = 0; i < hits.Length; i++)
        {
            m_HitInfo = hits[i];
            var face = m_HitInfo.collider.gameObject.GetComponent<Face>();
            if (!face)
                continue;
            float hitDistance = Vector3.Distance(m_HitInfo.point, face.gameObject.transform.parent.position);
            if (lowestHitDistance == 0 || hitDistance < lowestHitDistance)
                lowestHitDistance = hitDistance;
            else if (hitDistance > lowestHitDistance)
                continue;
            if (lastFace && (lastFace.Ability != null))
                continue;
            lastFace = face;
            _selectedFace = face;
            _mScript.SelectedFace = _selectedFace;
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.SharedInstance.PlayMode)
        {
            PlayModeMouseDown();
        }
        else
        {
            DrawModeMouseDown();
        }
    }
}
