using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBlock : MonoBehaviour
{
    //The color of the object
    public Color _Albedo;

    //The material property block we pass to the GPU
    private MaterialPropertyBlock property_block;

    // OnValidate is called in the editor after the component is edited
    void OnValidate() {
        //create propertyblock only if none exists
        SetColor(_Albedo);
    }

    public void SetColor(Color color) {
        if (color == _Albedo) return;
        _Albedo = color;

        if (property_block == null)
            property_block = new MaterialPropertyBlock();
        
        //Get a renderer component either of the own gameobject or of a child
        Renderer renderer = GetComponentInChildren<Renderer>();
        //set the color property
        property_block.SetColor("_Albedo", color);
        //apply propertyBlock to renderer
        renderer.SetPropertyBlock(property_block);
    }

}
