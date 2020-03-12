using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISphere : MonoBehaviour
{
    public Atom atom_;

    private MaterialBlock material_block_;

    // Start is called before the first frame update
    void Start()
    {
        material_block_ = GetComponent<MaterialBlock>();
    }

    public void SetColor(Color color) {
        material_block_.SetColor(color);
    }

    //public void SetRadius(float radius) {
    //    GetComponent<Material>().SetFloat("_Radius", radius);

    //    float scaling = transform.localScale.x;
    //    GetComponent<SphereCollider>().radius = radius / scaling;
    //}
}
