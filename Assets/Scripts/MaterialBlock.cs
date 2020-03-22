using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBlock : MonoBehaviour
{
    //The material property block we pass to the GPU
    private MaterialPropertyBlock property_block;
    //RGB = albedo, A = is highlighted or not
    private Color _Albedo = new Color(1.0f, 0, 0.8f, 0.0f);
    // R = radius, G = ambient component, B = metallicness, A = glossiness
    public Color _RadiusAndShading = new Color(0.07f, 0.7f, 1, 0.658f);

    public void Start()
    {

    }

    private void OnValidate()
    {
        SetPropertyBlock();
    }

    public void SetColor(Color color)
    {
        _Albedo.r = color.r;
        _Albedo.g = color.g;
        _Albedo.b = color.b;

        SetPropertyBlock();
    }

    public void SetHighlighted(bool is_highlighted)
    {
        _Albedo.a = (is_highlighted) ? 255 : 0;
        SetPropertyBlock();
    }

    public void SetRadius(float radius)
    {
        _RadiusAndShading.r = radius;
        SetPropertyBlock();
    }

    public void SetPropertyBlock()
    {
        if (property_block == null)
        {
            property_block = new MaterialPropertyBlock();
        }

        //Get a renderer component either of the own gameobject or of a child
        Renderer renderer = GetComponentInChildren<Renderer>();
        //set the color property
        property_block.SetColor("_Albedo", _Albedo);
        property_block.SetColor("_RadiusAndShading", _RadiusAndShading);
        //apply propertyBlock to renderer
        renderer.SetPropertyBlock(property_block);
    }

}
