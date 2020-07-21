﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    private GameObject self;
    public MaterialSelectScript mScript;
    public Face selectedFace;
    public Material mat;
    public ParticleSystem particle;
    public Vector3 endPos;
    public float speed = 15f;

    private void Start()
    {
        self = this.gameObject;
        particle = GetComponent<ParticleSystem>();
        particle.Play();
        Material partRend = particle.GetComponent<Renderer>().material;
        partRend.renderQueue = 4000;
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        particle.transform.position = Vector3.MoveTowards(particle.transform.position, endPos, step);
        if (Vector3.Distance(particle.transform.position, endPos) < .001f)
        {
            particle.transform.position = endPos;
            Invoke("DestroySelf", particle.main.startLifetime.constant);
            if(selectedFace != null)
            {
                selectedFace._rend.material = mat;
                if(selectedFace.Ability == null)
                {
                    selectedFace.Ability = selectedFace.gameObject.AddComponent<ExtrudeFaceAbility>();

                    ExtrudeFaceAbility extrude = selectedFace.GetComponent<ExtrudeFaceAbility>();
                    extrude.SetStartingConditions(selectedFace);
                }
            }
        }
    }

    private void DestroySelf()
    {
        Destroy(self);
    }

    //This bit is giving me trouble, can't seem to edit particle system settings
    public void SetColorAndGradient()
    {
        Color setTo = mat.color;

        var overTime = particle.colorOverLifetime;

        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(setTo, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        overTime.color = grad;
    }

}