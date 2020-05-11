using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorsionAngle : MonoBehaviour
{
    public Vector3 pos1_;
    public Vector3 pos2_;
    public Vector3 pos3_;
    public Vector3 pos4_;

    [SerializeField] GameObject prefab_arc = null;

    Vector3 normal1_, normal2_;
    Vector3 dir1_, dir2_;

    private LineRenderer lr_;
    TorsionPlane plane1_;

    // Start is called before the first frame update
    void Start()
    {
        normal1_ = Vector3.Normalize(GetPlaneNormal(pos1_, pos2_, pos3_));
        normal2_ = Vector3.Normalize(GetPlaneNormal(pos2_, pos3_, pos4_));
        float angle = Mathf.Acos(Vector3.Dot(normal1_, normal2_));
        float sign = Mathf.Sign(Vector3.Dot(normal2_, pos1_ - pos2_));

        Transform p1 = transform.GetChild(0);
        plane1_ = p1.GetComponent<TorsionPlane>();
        plane1_.pos1_ = pos3_;
        plane1_.pos2_ = pos2_;
        plane1_.pos3_ = pos1_;

        Transform p2 = transform.GetChild(1);
        TorsionPlane plane2 = p2.GetComponent<TorsionPlane>();
        plane2.pos1_ = pos2_;
        plane2.pos2_ = pos3_;
        plane2.pos3_ = pos4_;

        lr_ = GetComponent<LineRenderer>();
        lr_.positionCount = 0;
        lr_.startWidth = 0.003f;
        lr_.endWidth = 0.003f;

        GameObject temp = Instantiate(prefab_arc, (pos2_ + pos3_) / 2, Quaternion.identity);
        ArcRenderer arc = temp.GetComponent<ArcRenderer>();

        dir1_ = Vector3.Normalize(-Vector3.Cross(normal1_, Vector3.Normalize(pos2_ - pos3_)));
        dir2_ = Vector3.Normalize(-Vector3.Cross(normal2_, Vector3.Normalize(pos2_ - pos3_)));

        arc.X_ = dir1_;
        arc.W_ = dir2_;
        arc.Radius_ = 0.065f;
        arc.angle_positive_ = sign > 0;
    }

    private void Update() {
        if (lr_.positionCount == 0) {
            Vector3 A, B;
            plane1_.GetAxisPoints(out A, out B);

            lr_.positionCount = 2;
            Vector3[] points = new Vector3[] { A, B };
            lr_.SetPositions(points);
        }
    }

    Vector3 GetPlaneNormal(Vector3 t1, Vector3 t2, Vector3 t3) {
        float A = (t2[1] - t1[1]) * (t3[2] - t1[2]) - (t3[1] - t1[1]) * (t2[2] - t1[2]);
        float B = (t2[2] - t1[2]) * (t3[0] - t1[0]) - (t3[2] - t1[2]) * (t2[0] - t1[0]);
        float C = (t2[0] - t1[0]) * (t3[1] - t1[1]) - (t3[0] - t1[0]) * (t2[1] - t1[1]);

        return new Vector3(A, B, C);
    }

}
