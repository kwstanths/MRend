using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICylinderBench : MonoBehaviour
{
    /* Radius and height for the impostor cylinder */
    public float radius_ = AtomicRadii.ball_and_stick_bond_radius;
    public float height_ = 0.5f;

    /* Correction applied to the above properties */
    private float radius_correction_ = 2.2f;

    /* A reference to the material property block of that impostor cylinder */
    private MaterialBlockCylinder material_block_;

    void Start() {
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

    public void SetRadius(float radius) {
        /* Set radius in material property block */
        radius_ = radius;
        transform.localScale = new Vector3(radius_ * radius_correction_, height_, radius_ * radius_correction_);
        material_block_.SetRadius(radius);
    }

    public void SetHeight(float height) {
        /* Set height in material property block */
        height_ = height;
        transform.localScale = new Vector3(radius_ * radius_correction_, height_, radius_ * radius_correction_);
    }
}
