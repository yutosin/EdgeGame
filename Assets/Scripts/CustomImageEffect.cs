using System;
using UnityEngine;
 
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CustomImageEffect : MonoBehaviour {
 
    public Material material;
    public Camera cam;
 
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest, material);
    }

    private void Update()
    {
        if (cam.depthTextureMode != DepthTextureMode.DepthNormals)
            cam.depthTextureMode = DepthTextureMode.DepthNormals;
    }
}