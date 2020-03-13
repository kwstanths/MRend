using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBlock : MonoBehaviour
{
    //The color of the object
    [Tooltip("Set the default color of the object, The alpha component is if the object is highlighted or not")]
    public Color _Albedo = new Color(0.16f, 0.17f, 0.62f, 0.0f);

    //The material property block we pass to the GPU
    private MaterialPropertyBlock property_block;

    // OnValidate is called in the editor after the component is edited
    void OnValidate()
    {
        //create propertyblock only if none exists
        SetPropertyBlock();
    }

    public void SetHighlighted(bool is_highlighted)
    {
        _Albedo.a = (is_highlighted) ? 255 : 0;
        SetPropertyBlock();
    }

    public void SetPropertyBlock()
    {
        if (property_block == null)
            property_block = new MaterialPropertyBlock();

        //Get a renderer component either of the own gameobject or of a child
        Renderer renderer = GetComponentInChildren<Renderer>();
        //set the color property
        property_block.SetColor("_Albedo", _Albedo);
        //apply propertyBlock to renderer
        renderer.SetPropertyBlock(property_block);
    }

}
