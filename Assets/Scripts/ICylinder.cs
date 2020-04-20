using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * A script attached on an impostor cylinder that holds information like, radius, height, etc.
 */
public class ICylinder : MonoBehaviour
{
    /* Radius and height for the impostor cylinder */
    public float radius_ = 0.03f;
    public float height_ = 0.5f;
    
    /* Correction applied to the above properties */
    private float radius_correction_ = 2.2f;
    private float height_correction_ = 1.0f;

    /* A reference to the material property block of that impostor cylinder */
    private MaterialBlockCylinder material_block_;

    void Start()
    {
        /* Get and set the cube mesh instance that will be used as the base mesh */
        /* A single mesh used for all cylinders to not break instancing */
        Mesh mesh = CubeCreator.Instance.GetCubeMesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        
        /* Initialize the material property block */
        material_block_ = GetComponent<MaterialBlockCylinder>();

        /* Set radius and height */
        SetRadius(radius_);
        SetHeight(height_);
    }

    public void SetRadius(float radius)
    {
        /* Set radius in material property block */
        radius_ = radius;
        transform.localScale = new Vector3(radius_ * radius_correction_, height_ * height_correction_, radius_ * radius_correction_);
        material_block_.SetRadius(radius);

        /* Set collision radius */
        float scaling = transform.localScale.x;
        GetComponent<CapsuleCollider>().radius = radius_ / scaling;
    }

    public void SetHeight(float height)
    {
        /* Set height in material property block */
        height_ = height;
        transform.localScale = new Vector3(radius_ * radius_correction_, height_ * height_correction_, radius_ * radius_correction_);
        material_block_.SetHeight(height);

        /* Set height in collision */
        float scaling = transform.localScale.y;
        GetComponent<CapsuleCollider>().height = height/ scaling;
    }

    /* Calculate a transform for that coordinate space defined by this cylinder, currently NOT USED */
    private void CalculateInverseTransform()
    {
        Vector3 O = transform.position;
        Vector3 Y = transform.up;
        Vector3 X = Vector3.Normalize(Vector3.Cross(Y, new Vector3(0, Y.z, Y.y)));
        Vector3 Z = Vector3.Normalize(Vector3.Cross(X, Y));

        Matrix4x4 inverse_transform;
        inverse_transform = Matrix4x4.Inverse(new Matrix4x4(new Vector4(X.x, X.y, X.z, 0), new Vector4(Y.x, Y.y, Y.z, 0), new Vector4(Z.x, Z.y, Z.z, 0), new Vector4(O.x, O.y, O.z, 1)));
    }
}
