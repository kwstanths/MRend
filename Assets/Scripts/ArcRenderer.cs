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
    /* The width of the line */
    public float width = 0.003f;

    private LineRenderer lr_;
    /* The angle in degrees */
    private float angle_deg_;

    // Start is called before the first frame update
    void Start()
    {
        lr_ = GetComponent<LineRenderer>();

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
        Vector3 text_pos = X_ * Radius_ + W_ * Radius_;
        temp.GetComponent<RectTransform>().localPosition = text_pos; 
    }

    private Vector3[] CalculateArc() {
        Vector3[] points = new Vector3[resolution_ + 1];

        X_ = Vector3.Normalize(X_);
        Vector3 Z_ = Vector3.Cross(X_, W_);
        Vector3 Y_ = Vector3.Cross(Z_, X_);

        Vector3 Origin = transform.position;
        float angle = Mathf.Acos(Vector3.Dot(X_, W_));

        angle_deg_ = angle * Mathf.Rad2Deg;

        for (int i = 0; i <= resolution_; i++) {
            float t = (float) i / (float) resolution_;
            float phi = t * angle;
            Vector3 P = Origin + Radius_ * Mathf.Cos(phi) * X_ + Radius_ * Mathf.Sin(phi) * Y_;
            points[i] = P;
        }
        return points;
    }

}
