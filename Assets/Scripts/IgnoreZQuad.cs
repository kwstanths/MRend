using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreZQuad : MonoBehaviour
{
    private UnityEngine.Rendering.CompareFunction always_pass = UnityEngine.Rendering.CompareFunction.Always;

    // Start is called before the first frame update
    void Start() {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material current_mat = renderer.material;
        Material updatedMaterial = new Material(current_mat);
        updatedMaterial.SetInt("unity_GUIZTestMode", (int)always_pass);
        renderer.material = updatedMaterial;
    }
}
