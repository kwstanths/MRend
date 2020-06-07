using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox
{
    public Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    public Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
    public Vector3 center;
    public Vector3 extents;

    public void AddPoint(Vector3 point) {
        min = Vector3.Min(min, point);
        max = Vector3.Max(max, point);
        center = (min + max) / 2;
        extents = (max - min) / 2;
    }
}
