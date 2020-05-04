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

    Vector3 middle, dir1, dir2;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 n1 = Vector3.Normalize(GetPlaneNormal(pos1_, pos2_, pos3_));
        Vector3 n2 = Vector3.Normalize(GetPlaneNormal(pos2_, pos3_, pos4_));
        float angle = Mathf.Acos(Vector3.Dot(n1, n2));
        float sign = Mathf.Sign(Vector3.Dot(n1, pos1_ - pos2_));
        print( sign * angle * Mathf.Rad2Deg);

        middle = (pos2_ + pos3_) / 2;

        GameObject temp = Instantiate(prefab_arc, middle, Quaternion.identity);
        ArcRenderer arc = temp.GetComponent<ArcRenderer>();

        dir1 = -Vector3.Cross(n1, pos3_ - pos2_);
        dir2 = -Vector3.Cross(n2, pos2_ - pos3_);
        arc.X_ = dir1;
        arc.W_ = dir2;
        arc.Radius_ = 0.05f;

        Transform p1 = transform.GetChild(0);
        Transform p2 = transform.GetChild(1);

        transform.position = middle;   
        p1.rotation = Quaternion.LookRotation(n1, dir1);
        p2.rotation = Quaternion.LookRotation(n2, dir2);
    }

    private void Update() {
        Debug.DrawLine(middle, middle + 10 * dir1);
        Debug.DrawLine(middle, middle + 10 * dir2);
    }

    Vector3 GetPlaneNormal(Vector3 t1, Vector3 t2, Vector3 t3) {
        float A = (t2[1] - t1[1]) * (t3[2] - t1[2]) - (t3[1] - t1[1]) * (t2[2] - t1[2]);
        float B = (t2[2] - t1[2]) * (t3[0] - t1[0]) - (t3[2] - t1[2]) * (t2[0] - t1[0]);
        float C = (t2[0] - t1[0]) * (t3[1] - t1[1]) - (t3[0] - t1[0]) * (t2[1] - t1[1]);

        return new Vector3(A, B, C);
    }

}
