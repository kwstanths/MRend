using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondDistance : MonoBehaviour
{
    public ISphere atom1_;
    public ISphere atom2_;
    public float width = 0.003f;

    private LineRenderer lr_;
    private Vector3[] local_points_ = new Vector3[2];

    Matrix4x4 local_to_world_;

    // Start is called before the first frame update
    void Start()
    {
        lr_ = GetComponent<LineRenderer>();

        local_to_world_ = transform.localToWorldMatrix;

        Vector3 text_direction = Vector3.Normalize(Vector3.Cross(atom1_.transform.position - atom2_.transform.position, transform.position - Camera.main.transform.position));

        /* Calculate and set the arc points */
        local_points_[0] = atom1_.transform.position - transform.position;
        local_points_[1] = atom2_.transform.position - transform.position;

        Vector3[] points = new Vector3[] { TransformToWorld(local_points_[0]), TransformToWorld(local_points_[1]) };
        lr_.positionCount = 2;
        lr_.SetPositions(points);
        lr_.startWidth = width;
        lr_.endWidth = width;

        AngleText temp = GetComponentInChildren<AngleText>();
        temp.angle_degrees_ = Vector3.Distance(atom1_.transform.position, atom2_.transform.position).ToString("F3");

        Vector3 text_pos = text_direction * 0.08f;
        temp.GetComponent<RectTransform>().localPosition = text_pos;
    }

    // Update is called once per frame
    void Update()
    {
        /* 
        * If the local to world matrix is not the one used when we initially 
        * calculated the points, i.e. somone moved the object around, then 
        * apply the transform to world space again
        */
        if (local_to_world_ != transform.localToWorldMatrix) {
            local_to_world_ = transform.localToWorldMatrix;
            Vector3[] points = new Vector3[] { TransformToWorld(local_points_[0]), TransformToWorld(local_points_[1]) };
            lr_.SetPositions(points);
        }

        /* Debug label position */
        //Debug.DrawLine(transform.position, transform.position + 0.2f * Vector3.Normalize(transform.position - Camera.main.transform.position), new Color(255,0,0));
        //Debug.DrawLine(transform.position, transform.position + 0.2f * Vector3.Normalize(atom1_.transform.position - atom2_.transform.position), new Color(0, 0, 255));
    }

    Vector3 TransformToWorld(Vector3 P) {
        Vector4 temp = local_to_world_ * new Vector4(P.x, P.y, P.z, 1);
        return new Vector3(temp.x, temp.y, temp.z);
    }
}
