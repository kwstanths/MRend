﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class RenderOnTopText : MonoBehaviour
{
    private UnityEngine.Rendering.CompareFunction always_pass = UnityEngine.Rendering.CompareFunction.Always;

    // Start is called before the first frame update
    void Start()
    {
        Text image = GetComponent<Text>();
        Material existingGlobalMat = image.materialForRendering;
        Material updatedMaterial = new Material(existingGlobalMat);
        updatedMaterial.SetInt("unity_GUIZTestMode", (int)always_pass);
        image.material = updatedMaterial;
        image.material.renderQueue = (int)RenderQueue.Transparent + 10;
    }


}
