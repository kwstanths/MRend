using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArcRenderer : MonoBehaviour
{
    /* Spawn an arc for an angle that is defined by two direction vectors X, W, based on a given radius
      
            ^
       ->  /
       X  /
         /
        O------->
            ->
            W
    */
    public Vector3 X_;
    public Vector3 W_;
    public float Radius_;
    /* Number of points connected with a line to use to draw the angle */
    public int resolution_ = 30;
    /* The width of the lines */
    public float width = 0.003f;

    private LineRenderer lr_;
    /* The angle in degrees */
    private float angle_deg_;
    /* Holds the positions of the arc points in local space */
    Vector3[] local_points_;
    /* Holds the previous transform matrix from local to world space */
    Matrix4x4 local_to_world_;
    
    // Start is called before the first frame update
    void Start()
    {
        lr_ = GetComponent<LineRenderer>();

        local_to_world_ = transform.localToWorldMatrix;

        X_ = Vector3.Normalize(X_);
        W_ = Vector3.Normalize(W_);

        /* Calculate and set the arc points */
        Vector3[] points = CalculateArc();
        lr_.positionCount = resolution_ + 1;
        lr_.SetPositions(points);
        lr_.startWidth = width;
        lr_.endWidth = width;

        /* Set the angle text using one decimal */
        AngleText temp = GetComponentInChildren<AngleText>();
        temp.angle_degrees_ = angle_deg_.ToString("F1");

        /* Calculate the position of the text using the direction vectors and the radius */
        /* Multiply by a factor to extend the text a bit outside of the angle */
        Vector3 text_pos = 1.2f * (X_ * Radius_ + W_ * Radius_);
        temp.GetComponent<RectTransform>().localPosition = text_pos;

    }

    private void Update() {
        /* 
         * If the local to world matrix is not the one used when we initially 
         * calculated the points, i.e. somone moved the object around, then 
         * apply the transform to world space again
         */
        if (local_to_world_ != transform.localToWorldMatrix) {
            local_to_world_ = transform.localToWorldMatrix;
            Vector3[] positions = new Vector3[resolution_ + 1];
            for (int i = 0; i <= resolution_; i++) {
                positions[i] = TransformToWorld(local_points_[i]);
            }
            lr_.SetPositions(positions);
        }


    }

    private Vector3[] CalculateArc() {
        local_points_ = new Vector3[resolution_ + 1];
        Vector3[] world_points = new Vector3[resolution_ + 1];

        Vector3 Z_ = Vector3.Cross(X_, W_);
        Vector3 Y_ = Vector3.Cross(Z_, X_);

        float angle = Mathf.Acos(Vector3.Dot(X_, W_));
        angle_deg_ = angle * Mathf.Rad2Deg;

        for (int i = 0; i <= resolution_; i++) {
            float t = (float) i / (float) resolution_;
            float phi = t * angle;
            Vector3 P = Radius_ * Mathf.Cos(phi) * X_ + Radius_ * Mathf.Sin(phi) * Y_;
            local_points_[i] = P;
            world_points[i] = TransformToWorld(P);
        }
        return world_points;
    }

    Vector3 TransformToWorld(Vector3 P) {
        Vector4 temp = local_to_world_ * new Vector4(P.x, P.y, P.z, 1);
        return new Vector3(temp.x, temp.y, temp.z);
    }

}
