using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICylinder : MonoBehaviour
{
    private float radius_;
    private float height_;
    private float radius_correction_ = 2.2f;
    private float height_correction_ = 1.0f;
    private Matrix4x4 inverse_transform_;
    private Material mat_;


    void Start()
    {
        CalculateInverseTransform();

        mat_ = GetComponent<Renderer>().material;
        mat_.SetMatrix("_InverseTransform", inverse_transform_);

        SetRadius(0.02f);
        SetHeight(1f);
    }

    //Update is called once per frame
    void Update()
    {
        CalculateInverseTransform();
        GetComponent<Renderer>().material.SetMatrix("_InverseTransform", inverse_transform_);

        //SetRadius(0.3f);
        //SetHeight(1.3f);
    }

    public void SetRadius(float radius)
    {
        radius_ = radius;
        transform.localScale = new Vector3(radius_ * radius_correction_, height_ * height_correction_, radius_ * radius_correction_) ;
        mat_.SetFloat("_Radius", radius);
    }

    public void SetHeight(float height)
    {
        height_ = height;
        transform.localScale = new Vector3(radius_ * radius_correction_, height_ * height_correction_, radius_ * radius_correction_);
        mat_.SetFloat("_Height", height);
    }

    private void CalculateInverseTransform()
    {
        Vector3 O = transform.position;
        Vector3 Y = transform.up;
        Vector3 X = Vector3.Normalize(Vector3.Cross(Y, new Vector3(0, Y.z, Y.y)));
        Vector3 Z = Vector3.Normalize(Vector3.Cross(X, Y));

        inverse_transform_ = Matrix4x4.Inverse(new Matrix4x4(new Vector4(X.x, X.y, X.z, 0), new Vector4(Y.x, Y.y, Y.z, 0), new Vector4(Z.x, Z.y, Z.z, 0), new Vector4(O.x, O.y, O.z, 1)));
    }
}
