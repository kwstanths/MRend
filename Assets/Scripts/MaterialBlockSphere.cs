using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBlockSphere : MonoBehaviour
{
    /* The default ambient component of the sphere */
    public static float SPHERE_AMBIENT = 0.7f;

    /* The material property block we pass to the renderer */
    private MaterialPropertyBlock property_block;
    /* Property 1: RGB = albedo, A = is highlighted or not */
    public Color _Albedo = new Color(1.0f, 0, 0.8f, 0.0f);
    /* Property 2: R = radius, G = ambient component, B = metallicness, A = glossiness */
    public Color _RadiusAndShading = new Color(0.07f, SPHERE_AMBIENT, 0, 0);

    public void SetColor(Color color)
    {
        _Albedo.r = color.r;
        _Albedo.g = color.g;
        _Albedo.b = color.b;

        SetPropertyBlock();
    }

    public void SetHighlighted(float is_highlighted)
    {
        /* If it's the same then return */
        //if (is_highlighted !=_Albedo.a) return;

        _Albedo.a = is_highlighted;
        SetPropertyBlock();
    }

    public void SetRadius(float radius)
    {
        _RadiusAndShading.r = radius;
        SetPropertyBlock();
    }

    public void SetAmbientComponent(float ambient)
    {
        _RadiusAndShading.g = ambient;
        SetPropertyBlock();
    }

    public void SetMetalicness(float metallic)
    {
        _RadiusAndShading.b = metallic;
        SetPropertyBlock();
    }

    public void SetGlossiness(float glossy)
    {
        _RadiusAndShading.a = glossy;
        SetPropertyBlock();
    }

    private void SetPropertyBlock()
    {
        if (property_block == null)
        {
            property_block = new MaterialPropertyBlock();
        }

        /* Get the renderer component either of the own gameobject or of a child */
        Renderer renderer = GetComponentInChildren<Renderer>();
        /* Set the color properties */
        property_block.SetColor("_Albedo", _Albedo);
        property_block.SetColor("_RadiusAndShading", _RadiusAndShading);
        /* Set to the renderer */
        renderer.SetPropertyBlock(property_block);
    }

}
