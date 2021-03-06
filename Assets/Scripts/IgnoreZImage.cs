﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

/* A script applied on a Unity GameObject that contains an image, and we want to ingore ZTest when rendering */
public class IgnoreZImage : MonoBehaviour
{
    public int transparent_queue_addition_ = 3;
    public bool do_z_test_ = true;

    private UnityEngine.Rendering.CompareFunction always_pass = UnityEngine.Rendering.CompareFunction.Always;
    private UnityEngine.Rendering.CompareFunction lequal = UnityEngine.Rendering.CompareFunction.LessEqual;

    public void Start() {
        DoZTest(do_z_test_);
    }

    public void DoZTest(bool active) {
        if (!active) {
            Image image = GetComponent<Image>();
            Material existingGlobalMat = image.materialForRendering;
            Material updatedMaterial = new Material(existingGlobalMat);
            updatedMaterial.SetInt("unity_GUIZTestMode", (int)always_pass);
            image.material = updatedMaterial;
            image.material.renderQueue = (int)RenderQueue.Transparent + transparent_queue_addition_;
        } else {
            Image image = GetComponent<Image>();
            Material existingGlobalMat = image.materialForRendering;
            Material updatedMaterial = new Material(existingGlobalMat);
            updatedMaterial.SetInt("unity_GUIZTestMode", (int)lequal);
            image.material = updatedMaterial;
            image.material.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
