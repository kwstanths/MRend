using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreZQuad : MonoBehaviour
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
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Material current_mat = renderer.material;
            Material updatedMaterial = new Material(current_mat);
            updatedMaterial.SetInt("unity_GUIZTestMode", (int)always_pass);
            renderer.material = updatedMaterial;
        } else {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Material current_mat = renderer.material;
            Material updatedMaterial = new Material(current_mat);
            updatedMaterial.SetInt("unity_GUIZTestMode", (int)lequal);
            renderer.material = updatedMaterial;
        }
    }

}
