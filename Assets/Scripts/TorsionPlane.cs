using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorsionPlane : MonoBehaviour
{
    public Vector3 pos1_;
    public Vector3 pos2_;
    public Vector3 pos3_;

    public float horizontal_addition_;
    public float vertical_addition_;

    private Mesh mesh_;

    private Vector3 PA_, PB_, PC_, PD_;
    private Vector3 y_axis_;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 direction = Vector3.Normalize(pos2_ - pos1_);
        PA_ = pos1_ - (horizontal_addition_ / 2) * direction;
        PB_ = pos2_ + (horizontal_addition_ / 2) * direction;

        float projection = Vector3.Dot(pos3_ - pos1_, direction);
        Vector3 point = pos1_ + projection * direction;
        Vector3 perp = pos3_ - point;
        y_axis_ = Vector3.Normalize(perp);

        PC_ = PB_ + perp + vertical_addition_ * perp;
        PD_ = PA_ + perp + vertical_addition_ * perp;

        FillMesh(PA_, PB_, PC_, PD_);

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh_;
    }

    void FillMesh(Vector3 PA, Vector3 PB, Vector3 PC, Vector3 PD) {
        /* Spawn the mesh */
        mesh_ = new Mesh();

        Vector3[] vertices = new Vector3[4] { PA, PB, PC, PD };
        mesh_.vertices = vertices;

        int[] tris = new int[]
        {
            0, 1, 2,
            0, 2, 1,
            0, 2, 3,
            0, 3, 2
        };
        mesh_.triangles = tris;
    }

    public void GetAxisPoints(out Vector3 A, out Vector3 B) {
        A = PA_;
        B = PB_;
    }

    public Vector3 GetAxisPerpendicular() {
        return y_axis_;
    }

}
