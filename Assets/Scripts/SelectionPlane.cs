using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A script attached to an impsotor sphere that will implement a selection plane around this sphere */
public class SelectionPlane : MonoBehaviour
{
    [SerializeField] GameObject prefab_arrow_top;
    [SerializeField] GameObject prefab_arrow_top_right;
    [SerializeField] GameObject prefab_arrow_right;
    [SerializeField] GameObject prefab_arrow_bottom_right;
    [SerializeField] GameObject prefab_arrow_bottom;
    [SerializeField] GameObject prefab_arrow_bottom_left;
    [SerializeField] GameObject prefab_arrow_left;
    [SerializeField] GameObject prefab_arrow_top_left;

    /* Holds the transform matrix for the local coordinate system of this selection plane. 
     * For more information look at function CalculateInverseTransform()
     */
    Matrix4x4 ITM_;

    /* Holds the spheres contained within this selection plane */
    List<ISphere> spheres_ = new List<ISphere>();
    /* Holds the positions of the above spheres into the local coordinate system defined by the above ITM */
    List<Vector3> plane_positions_;

    /* A 3x3 array that will store sphere indices that point to spheres_ array, that 
     * will be used to store the closest selection
     */
    int[,] array_;
    Color[,] colors_;
    GameObject[,] arrows_;

    float drag_x_ = 0;
    float drag_y_ = 0;


    Color color_top = new Color(1, 0, 0);
    Color color_top_right = new Color(0.74f, 0.85f, 0.28f);
    Color color_right = new Color(0, 0.58f, 0.85f);
    Color color_bottom_right = new Color(1, 1, 0);
    Color color_bottom = new Color(0.52f, 0.27f, 0.6f);
    Color color_bottom_left = new Color(0.27f, 0.72f, 0.56f);
    Color color_left = new Color(0.96f, 0.56f, 0.12f);
    Color color_top_left = new Color(0, 0, 1);

    void Start()
    {
        /* Set the scale based on the selection radius used */
        transform.localScale = 2 * new Vector3(Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS);

        /* Make the sphere transparent */
        ISphere s = GetComponent<ISphere>();
        s.SetTransparent(true);
        s.SetRadius(Atoms.SELECTION_MODE_SPHERE_RADIUS);
        s.SetColor(new Color(0.1f, 0.1f, 0.1f, 0));

        colors_ = new Color[3, 3];
        colors_[0, 0] = color_bottom_left;
        colors_[0, 1] = color_bottom;
        colors_[0, 2] = color_bottom_right;
        colors_[1, 0] = color_left;
        colors_[1, 2] = color_right;
        colors_[2, 0] = color_top_left;
        colors_[2, 1] = color_top;
        colors_[2, 2] = color_top_right;

        arrows_ = new GameObject[3, 3];
        arrows_[0, 0] = Instantiate(prefab_arrow_bottom_left, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[0, 1] = Instantiate(prefab_arrow_bottom, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[0, 2] = Instantiate(prefab_arrow_bottom_right, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[1, 0] = Instantiate(prefab_arrow_left, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[1, 2] = Instantiate(prefab_arrow_right, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[2, 0] = Instantiate(prefab_arrow_top_left, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[2, 1] = Instantiate(prefab_arrow_top, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[2, 2] = Instantiate(prefab_arrow_top_right, new Vector3(0, 0, 0), Quaternion.identity);
    }

    void Update() {
        CalculateInverseTransform();

        /* Calculate positions and reset colors and highlighting */
        plane_positions_ = new List<Vector3>(spheres_.Count);
        for (int i = 0; i < spheres_.Count; i++) {
            ISphere s = spheres_[i];
            Vector4 sphere_world_position = new Vector4(s.transform.position.x, s.transform.position.y, s.transform.position.z, 1);
            Vector4 sphere_plane_position = ITM_ * sphere_world_position;
            plane_positions_.Add(sphere_plane_position);
            s.SetCPKColor();
            s.SetHighlighted(false);
        }

        FillArray();

        /* Highlight the spheres that can be navigated to */
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                int s = array_[i, j];
                if (s == -1) continue;
                spheres_[s].SetHighlighted(true);
                spheres_[s].SetColor(colors_[j, i]);

                arrows_[j, i].SetActive(true);
                arrows_[j, i].transform.position = spheres_[s].transform.position +
                    Vector3.Normalize(Camera.main.transform.position - spheres_[s].transform.position) * 2.0f * AtomicRadii.ball_and_stick_radius;

                arrows_[j, i].transform.rotation = Quaternion.LookRotation(arrows_[j, i].transform.position - Camera.main.transform.position);
            }
        }

        /* Get the index for the selected sphere based on the control input */
        int sphere_index = GetDirectionInput();
        if (sphere_index != -1) {
            ISphere s = spheres_[sphere_index];

            /* If clicked as well, move the selection */
            if (Input.GetMouseButtonDown(0)) {
                MoveSelectionToSphere(s);
            } else {
                s.SetColor(Color.gray);
            }
        }

    }

    private void MoveSelectionToSphere(ISphere s) {
        /* Set new position */
        transform.position = s.transform.position;
        s.SetCPKColor();

        /* Clear spheres */
        ClearHighlightedAndSetCPKColor();
        spheres_.Clear();

        /* Get the new spheres within radius */
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, Atoms.SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ISphere sphere = c.gameObject.GetComponent<ISphere>();
            if (sphere == null || sphere == s) continue;

            AddSphere(sphere);
        }
    }

    private int GetDirectionInput() {
        int sphere_index = -1;
        if (Input.GetKey(KeyCode.RightArrow)) sphere_index = array_[2, 1];
        if (Input.GetKey(KeyCode.LeftArrow)) sphere_index = array_[0, 1];
        if (Input.GetKey(KeyCode.DownArrow)) sphere_index = array_[1, 0];
        if (Input.GetKey(KeyCode.UpArrow)) sphere_index = array_[1, 2];
        if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.UpArrow)) sphere_index = array_[2, 2];
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.DownArrow)) sphere_index = array_[0, 0];
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow)) sphere_index = array_[2, 0];
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow)) sphere_index = array_[0, 2];
        return sphere_index;

        //float x = Input.GetAxis("Mouse X");
        //float y = Input.GetAxis("Mouse Y");
        //if (x != 0 || y != 0) {
        //    drag_x_ = x;
        //    drag_y_ = y;
        //}
        //drag_x_ += x;
        //drag_y_ += y;
        //drag_x_ = Mathf.Clamp(drag_x_, -0.3f, 0.3f);
        //drag_y_ = Mathf.Clamp(drag_y_, -0.3f, 0.3f);
        //print(drag_x_ + " " + drag_y_);

        int i, j;
        float local_xy_angle = Mathf.Atan2(drag_y_, drag_x_);
        GetArrayIndex(out i, out j, local_xy_angle);

        return array_[i, j];
    }

    /* Fills the 3x3 array with the closest spheres */
    private void FillArray() {
        array_ = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                array_[i, j] = -1;
                if (i != 1 || j != 1) {
                    arrows_[i, j].SetActive(false);
                }
            }
        }

        /* Calculate the array position based on the angle defined by the local x,y coordinates */
        float margin = Atoms.SELECTION_MODE_SPHERE_RADIUS;
        for (int s = 0; s < spheres_.Count; s++) {
            int i = -1, j = -1;
            float local_xy_angle = Mathf.Atan2(plane_positions_[s].y, plane_positions_[s].x);
            GetArrayIndex(out i, out j, local_xy_angle);

            /* If array slot is empty, or this one is closer, then store its index */
            if (array_[i, j] == -1) array_[i, j] = s;
            else if (Mathf.Abs(plane_positions_[s].z) < Mathf.Abs(plane_positions_[array_[i, j]].z)) array_[i, j] = s;
        }
    }

    /* Given a angle in radians (-pi, pi] find the array index */
    private void GetArrayIndex(out int i, out int j, float local_xy_angle) {
        if (local_xy_angle >= -0.5234 && local_xy_angle < 0.5234) {
            /* Right */
            i = 2;
            j = 1;
        } else if (local_xy_angle > 0.523 && local_xy_angle < 1.0467) {
            /* Up right */
            i = 2;
            j = 2;
        } else if (local_xy_angle >= 1.046 && local_xy_angle < 2.0934) {
            /* Up */
            i = 1;
            j = 2;
        } else if (local_xy_angle >= 2.093 && local_xy_angle < 2.6167) {
            /* Up left */
            i = 0;
            j = 2;
        } else if (local_xy_angle >= 2.616 || local_xy_angle < -2.616) {
            /* Left */
            i = 0;
            j = 1;
        } else if (local_xy_angle >= -2.6167 && local_xy_angle < -2.093) {
            /* Bottom left */
            i = 0;
            j = 0;
        } else if (local_xy_angle >= -2.0934 && local_xy_angle < -1.046) {
            /* Bottom */
            i = 1;
            j = 0;
        } else if (local_xy_angle >= -1.0467 && local_xy_angle < -0.523) {
            /* Bottom right */
            i = 2;
            j = 0;
        } else {
            i = 1;
            j = 1;
            print("FATAL ERROR");
        }
    }

    private void OnDestroy() {
        ClearHighlightedAndSetCPKColor();
    }

    private void ClearHighlightedAndSetCPKColor() {
        foreach (ISphere s in spheres_) {
            s.SetHighlighted(false);
            s.SetCPKColor();
        }
    }

    public void AddSphere(ISphere s) {
        spheres_.Add(s);
    }

    private void CalculateInverseTransform() {
        /* New origin, X, Y, Z directions */
        Vector3 O = transform.position;
        Vector3 Z = Vector3.Normalize(transform.position - Camera.main.transform.position);
        Vector3 Y = Camera.main.transform.up;
        /* Unity is a left handed coordiante system */
        Vector3 X = -Vector3.Cross(Z, Y);

        Matrix4x4 temp = new Matrix4x4();
        temp.SetColumn(0, new Vector4(X.x, X.y, X.z, 0));
        temp.SetColumn(1, new Vector4(Y.x, Y.y, Y.z, 0));
        temp.SetColumn(2, new Vector4(Z.x, Z.y, Z.z, 0));
        temp.SetColumn(3, new Vector4(O.x, O.y, O.z, 1));
        ITM_ = Matrix4x4.Inverse(temp);
    }



    /* Not used */
    private float map_to_array(float in_a, float in_b, float in_t, float out_a, float out_b) {
        return out_a + ((out_b - out_a) / (in_b - in_a)) * (in_t - in_a);
    }
}
