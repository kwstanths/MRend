using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPlane : MonoBehaviour
{
    Matrix4x4 ITM_;

    List<ISphere> spheres_ = new List<ISphere>();
    List<Vector3> plane_positions_;

    int[,] array_;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = 2 * new Vector3(Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS);
    }

    // Update is called once per frame
    void Update() {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        CalculateInverseTransform();

        plane_positions_ = new List<Vector3>(spheres_.Count);
        foreach (ISphere s in spheres_) {
            Vector4 sphere_world_position = new Vector4(s.transform.position.x, s.transform.position.y, s.transform.position.z, 1);
            Vector4 sphere_plane_position = ITM_ * sphere_world_position;
            plane_positions_.Add(sphere_plane_position);
            s.SetCPKColor();
            s.SetHighlighted(false);
        }

        FillArray();

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                int s = array_[i, j];
                if (s == -1) continue;
                spheres_[s].SetHighlighted(true);
            }
        }

        int sphere_index = -1;
        if (Input.GetKey(KeyCode.RightArrow)) sphere_index = array_[2, 1];
        if (Input.GetKey(KeyCode.LeftArrow)) sphere_index = array_[0, 1];
        if (Input.GetKey(KeyCode.DownArrow)) sphere_index = array_[1, 0];
        if (Input.GetKey(KeyCode.UpArrow)) sphere_index = array_[1, 2];
        if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.UpArrow)) sphere_index = array_[2, 2];
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.DownArrow)) sphere_index = array_[0, 0];
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow)) sphere_index = array_[2, 0];
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow)) sphere_index = array_[0, 2];

        if (sphere_index != -1) spheres_[sphere_index].SetColor(Color.gray);
    }

    private void FillArray() {
        array_ = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                array_[i, j] = -1;
            }
        }

        float margin = Atoms.SELECTION_MODE_SPHERE_RADIUS;
        for (int s = 0; s < spheres_.Count; s++) {

            int i, j;
            float local_xy_angle = Mathf.Atan2(plane_positions_[s].y, plane_positions_[s].x);
            if (local_xy_angle >= -0.523 && local_xy_angle < 0.523) {
                /* Right */
                i = 2;
                j = 1;
            }else if (local_xy_angle >= 0.523 && local_xy_angle < 1.046) {
                /* Up right */
                i = 2;
                j = 2;
            }else if (local_xy_angle >= 1.046 && local_xy_angle < 2.093) {
                /* Up */
                i = 1;
                j = 2;
            }else if (local_xy_angle >= 2.093 && local_xy_angle < 2.616) {
                /* Up left */
                i = 0;
                j = 2;
            }else if (local_xy_angle >= 2.616 || local_xy_angle < -2.616) {
                /* Left */
                i = 0;
                j = 1;
            }else if (local_xy_angle >= -2.616 && local_xy_angle < -2.093) {
                /* Bottom left */
                i = 0;
                j = 0;
            }else if (local_xy_angle >= -2.093 && local_xy_angle < -1.046) {
                /* Bottom */
                i = 1;
                j = 0;
            } else {
                /* Bottom right */
                i = 2;
                j = 0;
            }

            if (array_[i, j] == -1) array_[i, j] = s;
            else if (Mathf.Abs(plane_positions_[s].z) < Mathf.Abs(plane_positions_[array_[i, j]].z)) array_[i, j] = s;
        }
    }

    private void OnDestroy() {
        foreach (ISphere s in spheres_) {
            s.SetHighlighted(false);
        }
    }

    public void AddSphere(ISphere s) {
        spheres_.Add(s);
    }

    void CalculateInverseTransform() {
        Vector3 O = transform.position;
        Vector3 X = Camera.main.transform.right;
        Vector3 Y = Camera.main.transform.up;
        Vector3 Z = Camera.main.transform.forward;

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
