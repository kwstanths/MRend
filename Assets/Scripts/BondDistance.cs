using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondDistance : MonoBehaviour
{
    public ISphere atom1_;
    public ISphere atom2_;
    public float width = 0.003f;

    private LineRenderer lr_;

    Matrix4x4 local_to_world_;

    // Start is called before the first frame update
    void Start()
    {
        lr_ = GetComponent<LineRenderer>();

        local_to_world_ = transform.localToWorldMatrix;

        Vector3 text_direction = Vector3.Normalize(Vector3.Cross(atom1_.transform.position - atom2_.transform.position, transform.position - Camera.main.transform.position));
        
        /* Calculate and set the arc points */
        Vector3[] points = new Vector3[] { atom1_.transform.position, transform.position + text_direction * 0.0f, atom2_.transform.position };
        lr_.positionCount = 3;
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
        
    }
}
