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

    [SerializeField] GameObject prefab_atom_info_box_ = null;
    GameObject atom_info_box_gameobject_;
    AtomInfoBox atom_info_box_;

    public enum VisualizationMethod
    {
        COLOR_CIRCLE,
        ARROWS,
    }
    VisualizationMethod visualization = VisualizationMethod.ARROWS;

    public ISphere center_sphere_;

    /* Holds the transform matrix for the local coordinate system of this selection plane. 
     * For more information look at function CalculateInverseTransform()
     */
    Matrix4x4 ITM_;
    Vector3 Vector_Right_;

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

    void Start() {
        /* Set the scale based on the selection radius used */
        transform.localScale = 2 * new Vector3(Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS);

        /* Make the sphere transparent */
        ISphere s = GetComponent<ISphere>();
        s.SetTransparent(true);
        s.SetRadius(Atoms.SELECTION_MODE_SPHERE_RADIUS);
        s.SetColor(new Color(0.1f, 0.1f, 0.1f, 0));

        if (visualization == VisualizationMethod.COLOR_CIRCLE) {
            colors_ = new Color[3, 3];
            colors_[0, 0] = color_bottom_left;
            colors_[0, 1] = color_bottom;
            colors_[0, 2] = color_bottom_right;
            colors_[1, 0] = color_left;
            colors_[1, 2] = color_right;
            colors_[2, 0] = color_top_left;
            colors_[2, 1] = color_top;
            colors_[2, 2] = color_top_right;
        }
        else {
            //transform.GetChild(0).gameObject.SetActive(false);

            arrows_ = new GameObject[3, 3];
            arrows_[0, 0] = Instantiate(prefab_arrow_bottom_left, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[0, 1] = Instantiate(prefab_arrow_bottom, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[0, 2] = Instantiate(prefab_arrow_bottom_right, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[1, 0] = Instantiate(prefab_arrow_left, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[1, 2] = Instantiate(prefab_arrow_right, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[2, 0] = Instantiate(prefab_arrow_top_left, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[2, 1] = Instantiate(prefab_arrow_top, new Vector3(0, 0, 0), Quaternion.identity);
            arrows_[2, 2] = Instantiate(prefab_arrow_top_right, new Vector3(0, 0, 0), Quaternion.identity);

            arrows_[0, 0].transform.parent = transform;
            arrows_[0, 1].transform.parent = transform;
            arrows_[0, 2].transform.parent = transform;
            arrows_[1, 0].transform.parent = transform;
            arrows_[1, 2].transform.parent = transform;
            arrows_[2, 0].transform.parent = transform;
            arrows_[2, 1].transform.parent = transform;
            arrows_[2, 2].transform.parent = transform;

        }
        
        //atom_info_box_gameobject_ = Instantiate(prefab_atom_info_box_, transform.position + Camera.main.transform.right * (Atoms.SELECTION_MODE_SPHERE_RADIUS + 0.08f), Quaternion.identity);
        //atom_info_box_ = atom_info_box_gameobject_.GetComponent<AtomInfoBox>();

        //SetAtomInfoBoxText();
    }

    //private void SetAtomInfoBoxText() {

    //    atom_info_box_.SetElementText(center_sphere_.atom_.element_);
    //    atom_info_box_.SetAtomNameText(center_sphere_.atom_.name_);
    //    atom_info_box_.SetResidueName(center_sphere_.atom_.res_name_);
    //}

    void Update() {
        CalculateInverseTransform();

        center_sphere_.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);
        //atom_info_box_.SetPosition(center_sphere_, Camera.main.transform.right);

        /* Calculate positions and reset colors and highlighting */
        plane_positions_ = new List<Vector3>(spheres_.Count);
        for (int i = 0; i < spheres_.Count; i++) {
            ISphere s = spheres_[i];
            Vector4 sphere_world_position = new Vector4(s.transform.position.x, s.transform.position.y, s.transform.position.z, 1);
            Vector4 sphere_plane_position = ITM_ * sphere_world_position;
            plane_positions_.Add(sphere_plane_position);
            s.SetHighlighted(0);
            s.SetCPKColor();
        }

        FillArray();

        /* Highlight the spheres that can be navigated to */
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                int s = array_[i, j];
                if (s == -1) continue;
                spheres_[s].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);

                if (visualization == VisualizationMethod.COLOR_CIRCLE) {
                    spheres_[s].SetColor(colors_[j, i]);
                } else {
                    arrows_[j, i].SetActive(true);
                    arrows_[j, i].transform.position = spheres_[s].transform.position +
                        Vector3.Normalize(Camera.main.transform.position - spheres_[s].transform.position) * 2.0f * AtomicRadii.ball_and_stick_radius;

                    arrows_[j, i].transform.rotation = Quaternion.LookRotation(arrows_[j, i].transform.position - Camera.main.transform.position, Camera.main.transform.up);
                }
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
                s.SetColor(Color.white);
            }
        }
    }

    public void ChangeRadius() {
        transform.localScale = 2 * new Vector3(Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS);
        
        /* Make the sphere transparent */
        ISphere s = GetComponent<ISphere>();
        s.SetRadius(Atoms.SELECTION_MODE_SPHERE_RADIUS);
        
        /* Clear spheres */
        spheres_.Clear();

        /* Get the new spheres within radius */
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, Atoms.SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ISphere sphere = c.gameObject.GetComponent<ISphere>();
            if (sphere == null || sphere == center_sphere_) continue;

            AddSphere(sphere);
        }
    }

    private void MoveSelectionToSphere(ISphere s) {
        /* Set new position */
        transform.position = s.transform.position;
        s.SetCPKColor();

        /* Clear spheres */
        ClearHighlightedAndSetCPKColor();
        spheres_.Clear();

        center_sphere_ = s;
        //SetAtomInfoBoxText();

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
    }

    /* Fills the 3x3 array with the closest spheres */
    private void FillArray() {
        array_ = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                array_[i, j] = -1;
                if (visualization == VisualizationMethod.ARROWS && (i != 1 || j != 1)) {
                    arrows_[i, j].SetActive(false);
                }
            }
        }

        /* Calculate the array position based on the angle defined by the local x,y coordinates */
        for (int s = 0; s < spheres_.Count; s++) {
            int i = -1, j = -1;
            float local_xy_angle = Mathf.Atan2(plane_positions_[s].y, plane_positions_[s].x);
            GetArrayIndex(out i, out j, local_xy_angle);

            /* If array slot is empty, or this one is closer, then store its index */
            if (array_[i, j] == -1) array_[i, j] = s;
            else if (plane_positions_[s].magnitude < plane_positions_[array_[i, j]].magnitude) array_[i, j] = s;
        }
    }

    /* Given a angle in radians (-pi, pi] find the array index */
    private void GetArrayIndex(out int i, out int j, float local_xy_angle) {
        if (local_xy_angle >= -0.3926 && local_xy_angle < 0.3926) {
            /* Right */
            i = 2;
            j = 1;
        } else if (local_xy_angle > 0.3926 && local_xy_angle < 1.17809) {
            /* Up right */
            i = 2;
            j = 2;
        } else if (local_xy_angle >= 1.17809 && local_xy_angle < 1.9634) {
            /* Up */
            i = 1;
            j = 2;
        } else if (local_xy_angle >= 1.9634 && local_xy_angle < 2.7488) {
            /* Up left */
            i = 0;
            j = 2;
        } else if (local_xy_angle >= 2.7488 || local_xy_angle < -2.7488) {
            /* Left */
            i = 0;
            j = 1;
        } else if (local_xy_angle >= -2.7488 && local_xy_angle < -1.9634) {
            /* Bottom left */
            i = 0;
            j = 0;
        } else if (local_xy_angle >= -1.9634 && local_xy_angle < -1.17809) {
            /* Bottom */
            i = 1;
            j = 0;
        } else if (local_xy_angle >= -1.17809 && local_xy_angle < -0.3926) {
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
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                if (visualization == VisualizationMethod.ARROWS && (i != 1 || j != 1)) {
                    Destroy(arrows_[i, j]);
                }
            }
        }
        //Destroy(atom_info_box_gameobject_);
    }

    private void ClearHighlightedAndSetCPKColor() {
        foreach (ISphere s in spheres_) {
            s.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
            s.SetCPKColor();
        }
        center_sphere_.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
        center_sphere_.SetCPKColor();
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
        Vector_Right_ = X;

        Matrix4x4 temp = new Matrix4x4();
        temp.SetColumn(0, new Vector4(X.x, X.y, X.z, 0));
        temp.SetColumn(1, new Vector4(Y.x, Y.y, Y.z, 0));
        temp.SetColumn(2, new Vector4(Z.x, Z.y, Z.z, 0));
        temp.SetColumn(3, new Vector4(O.x, O.y, O.z, 1));
        ITM_ = Matrix4x4.Inverse(temp);

        Debug.DrawLine(transform.position, transform.position + X * 0.25f, new Color(1, 0, 0));
        Debug.DrawLine(transform.position, transform.position + Y * 0.25f, new Color(0, 1, 0));
        Debug.DrawLine(transform.position, transform.position + Z * 0.25f, new Color(0, 0, 1));
    }



    /* Not used */
    private float map_to_array(float in_a, float in_b, float in_t, float out_a, float out_b) {
        return out_a + ((out_b - out_a) / (in_b - in_a)) * (in_t - in_a);
    }
}
