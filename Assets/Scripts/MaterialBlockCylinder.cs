using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBlockCylinder : MonoBehaviour
{
    /* The material property block we pass to the renderer */
    private MaterialPropertyBlock property_block;
    /* Property 1: RGB = albedo, A = radius */
    public Color _Albedo = new Color(0.5f, 0, 0, 0.015f);
    /* Property 2: R = height, G = ambient, B = metallic, A = glossiness */
    public Color _RadiusAndShading = new Color(0.07f, 0.7f, 0, 0);

    public void SetColor(Color color)
    {
        _Albedo.r = color.r;
        _Albedo.g = color.g;
        _Albedo.b = color.b;

        SetPropertyBlock();
    }

    public void SetRadius(float radius)
    {
        _Albedo.a = radius;
        SetPropertyBlock();
    }

    public void SetHeight(float height)
    {
        _RadiusAndShading.r = height;
        SetPropertyBlock();
    }

    public void SetAmbientComponent(float ambient)
    {
        _RadiusAndShading.g = ambient;
        SetPropertyBlock();
    }

    public void SetMetallic(float metallic)
    {
        _RadiusAndShading.b = metallic;
        SetPropertyBlock();
    }

    public void SetGlossiness(float glossy)
    {
        _RadiusAndShading.a = glossy;
        SetPropertyBlock();
    }

    public void SetPropertyBlock()
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
