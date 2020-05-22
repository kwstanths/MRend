using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A script attached to an impsotor sphere that will implement a selection plane of cylinders around this cylinder */
public class SelectionPlaneCylinders : MonoBehaviour
{
    [SerializeField] GameObject prefab_arrow_top;
    [SerializeField] GameObject prefab_arrow_top_right;
    [SerializeField] GameObject prefab_arrow_right;
    [SerializeField] GameObject prefab_arrow_bottom_right;
    [SerializeField] GameObject prefab_arrow_bottom;
    [SerializeField] GameObject prefab_arrow_bottom_left;
    [SerializeField] GameObject prefab_arrow_left;
    [SerializeField] GameObject prefab_arrow_top_left;

    public SelectionVisualizationMethod visualization = SelectionVisualizationMethod.ARROWS;

    /* The currently selected cylinder */
    public ICylinder center_cylinder_;

    /* Holds the transform matrix for the local coordinate system of this selection plane. 
     * For more information look at function CalculateInverseTransform()
     */
    Matrix4x4 ITM_;
    Vector3 Vector_Right_;

    /* Holds the cylinders contained within this selection plane */
    List<ICylinder> cylinders_ = new List<ICylinder>();
    /* Holds the positions of the above cylinders into the local coordinate system defined by the above ITM */
    List<Vector3> plane_positions_;

    /* A 3x3 array corresponding to the eight 2D directions, that will store cylinder indices that point to the 
     * cylinders_ array, that will be used to store the closest selection, and the correponsing color 
     * and arrow object
     */
    int[,] array_;
    Color[,] colors_;
    GameObject[,] arrows_;

    /* The color of the color circle */
    Color color_top = new Color(0.4f, 0.69f, 1);
    Color color_top_right = new Color(1, 0.6f, 1);
    Color color_right = new Color(0.4f, 0.4f, 0);
    Color color_bottom_right = new Color(0.37f, 0.37f, 0.37f);
    Color color_bottom = new Color(0.4f, 0, 0.2f);
    Color color_bottom_left = new Color(0, 0.6f, 0.6f);
    Color color_left = new Color(0.4f, 0.2f, 0);
    Color color_top_left = new Color(0.2f, 0.4f, 0);

    /* The info box */
    AtomInfoBox info_ui_;

    /* The parent Atoms object */
    Atoms atoms_object_;

    void Start() {
        /* Set the scale based on the selection radius used */
        transform.localScale = 2 * new Vector3(Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS);

        /* Make the selection sphere transparent */
        ISphere s = GetComponent<ISphere>();
        s.SetTransparent(true);
        s.SetRadius(Atoms.SELECTION_MODE_SPHERE_RADIUS);
        s.SetColor(new Color(0.1f, 0.1f, 0.1f, 0));

        /* Set color circle parameters */
        colors_ = new Color[3, 3];
        colors_[0, 0] = color_bottom_left;
        colors_[0, 1] = color_bottom;
        colors_[0, 2] = color_bottom_right;
        colors_[1, 0] = color_left;
        colors_[1, 2] = color_right;
        colors_[2, 0] = color_top_left;
        colors_[2, 1] = color_top;
        colors_[2, 2] = color_top_right;
        
        /* Set arrow parameters */
        arrows_ = new GameObject[3, 3];
        arrows_[0, 0] = Instantiate(prefab_arrow_bottom_left, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[0, 1] = Instantiate(prefab_arrow_bottom, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[0, 2] = Instantiate(prefab_arrow_bottom_right, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[1, 0] = Instantiate(prefab_arrow_left, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[1, 2] = Instantiate(prefab_arrow_right, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[2, 0] = Instantiate(prefab_arrow_top_left, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[2, 1] = Instantiate(prefab_arrow_top, new Vector3(0, 0, 0), Quaternion.identity);
        arrows_[2, 2] = Instantiate(prefab_arrow_top_right, new Vector3(0, 0, 0), Quaternion.identity);
        //arrows_[0, 0].transform.parent = transform;
        //arrows_[0, 1].transform.parent = transform;
        //arrows_[0, 2].transform.parent = transform;
        //arrows_[1, 0].transform.parent = transform;
        //arrows_[1, 2].transform.parent = transform;
        //arrows_[2, 0].transform.parent = transform;
        //arrows_[2, 1].transform.parent = transform;
        //arrows_[2, 2].transform.parent = transform;
        arrows_[0, 0].SetActive(false);
        arrows_[0, 1].SetActive(false);
        arrows_[0, 2].SetActive(false);
        arrows_[1, 0].SetActive(false);
        arrows_[1, 2].SetActive(false);
        arrows_[2, 0].SetActive(false);
        arrows_[2, 1].SetActive(false);
        arrows_[2, 2].SetActive(false);

        if (visualization == SelectionVisualizationMethod.ARROWS) {
            transform.GetChild(0).gameObject.SetActive(false);
        } else {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        /* Get info box object */
        info_ui_ = Camera.main.transform.Find("AtomInfoBox").GetComponent<AtomInfoBox>();

        /* Get parent atoms object */
        atoms_object_ = transform.parent.GetComponent<Atoms>();
    }

    public static Vector3 CalculateCylinderMiddle(ICylinder s) {
        return s.transform.position + s.transform.up * s.height_ / 2;
    }

    void Update() {
        /* Calculate the coordiinate system transformation matrix */
        CalculateInverseTransform();

        /* highlight and color center cylinder */
        center_cylinder_.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);
        center_cylinder_.ResetColor();

        /* Calculate positions and reset colors and highlighting for cylinders within the radius */
        plane_positions_ = new List<Vector3>(cylinders_.Count);
        for (int i = 0; i < cylinders_.Count; i++) {
            ICylinder s = cylinders_[i];
            Vector3 cylinder_middle = CalculateCylinderMiddle(s);
            Vector4 cylinder_world_position = new Vector4(cylinder_middle.x, cylinder_middle.y, cylinder_middle.z, 1);
            Vector4 cylinder_plane_position = ITM_ * cylinder_world_position;
            plane_positions_.Add(cylinder_plane_position);
            s.SetHighlighted(0);
            s.ResetColor();
        }

        /* Calculate cylinders available for selection and their directions */
        FillArray();

        /* Highlight the cylinders that can be navigated to, set the arrow directions based on the visualization used, or set the colors */
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                int s = array_[i, j];
                /* If no cylinder mapped in this direction, skip */
                if (s == -1) continue;
                cylinders_[s].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);

                if (visualization == SelectionVisualizationMethod.COLOR_CIRCLE) {
                    cylinders_[s].SetColor(colors_[j, i]);
                } else {
                    /* If arrows navigation used, calculate atomic radius used based on the atoms viusalization method */
                    float atom_radius = AtomicRadii.ball_and_stick_radius;
                    
                    /* and set the arrow objects in front of the atom */
                    arrows_[j, i].SetActive(true);
                    Vector3 cylinder_middle = CalculateCylinderMiddle(cylinders_[s]);
                    arrows_[j, i].transform.position = cylinder_middle +
                        Vector3.Normalize(Camera.main.transform.position - cylinder_middle) * 1.2f * atom_radius;

                    /* make arrows face the camera */
                    arrows_[j, i].transform.rotation = Quaternion.LookRotation(arrows_[j, i].transform.position - Camera.main.transform.position, Camera.main.transform.up);
                }
            }
        }

        /* Get the index for the selected cylinder based on the control input */
        int cylinder_index = GetDirectionInput();
        if (cylinder_index != -1) {
            ICylinder s = cylinders_[cylinder_index];

            /* If clicked as well, move the selection */
            if (Input.GetMouseButtonDown(0)) {
                MoveSelectionToCylinder(s);
            } else {
                /* Make input selected cylinder white */
                s.SetColor(Color.white);
            }
        }
    }

    public void ChangeVisualization() {
        if (visualization == SelectionVisualizationMethod.ARROWS) {
            /* If it was arrows, change to color circle */
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (i != 1 || j != 1) {
                        arrows_[i, j].SetActive(false);
                    }
                }
            }
            visualization = SelectionVisualizationMethod.COLOR_CIRCLE;
            /* Make color circle object active */
            transform.GetChild(0).gameObject.SetActive(true);
        } else {
            visualization = SelectionVisualizationMethod.ARROWS;
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    /* Change selection radius */
    public void ChangeRadius() {
        transform.localScale = 2 * new Vector3(Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS, Atoms.SELECTION_MODE_SPHERE_RADIUS);

        /* Set radius of the transparent sphere */
        ISphere s = GetComponent<ISphere>();
        s.SetRadius(Atoms.SELECTION_MODE_SPHERE_RADIUS);

        /* Reset cylinders within the previous radius */
        for (int i = 0; i < cylinders_.Count; i++) {
            ICylinder cylinder = cylinders_[i];
            cylinder.SetHighlighted(0);
            cylinder.ResetColor();
        }
        cylinders_.Clear();

        /* Get the new cylinders within radius */
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, Atoms.SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ICylinder cylinder = c.gameObject.GetComponent<ICylinder>();
            if (cylinder == null || cylinder == center_cylinder_) continue;

            AddCylinder(cylinder);
        }
    }

    private void MoveSelectionToCylinder(ICylinder s) {
        /* Set new position */
        transform.position = CalculateCylinderMiddle(s);
        s.ResetColor();

        /* Clear cylinders object */
        ClearHighlightedAndResetColor();
        cylinders_.Clear();

        /* Set info displayed */
        center_cylinder_ = s;

        /* Get the new cylinders within radius */
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, Atoms.SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ICylinder cylinder = c.gameObject.GetComponent<ICylinder>();
            if (cylinder == null || cylinder == s) continue;

            AddCylinder(cylinder);
        }
    }

    /* Get the 2D direction input */
    private int GetDirectionInput() {
        int array_index = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) array_index = array_[2, 1];
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) array_index = array_[0, 1];
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) array_index = array_[1, 0];
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) array_index = array_[1, 2];
        if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))) array_index = array_[2, 2];
        if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))) array_index = array_[0, 0];
        if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))) array_index = array_[2, 0];
        if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) && (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))) array_index = array_[0, 2];
        return array_index;
    }

    /* Fills the 3x3 array with the closest cylinders */
    private void FillArray() {
        /* Reset the array and deactivate all arrow objects */
        array_ = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                array_[i, j] = -1;
                if (visualization == SelectionVisualizationMethod.ARROWS && (i != 1 || j != 1)) {
                    arrows_[i, j].SetActive(false);
                }
            }
        }

        /* Calculate the array position based on the angle defined by the local coordiante system x,y coordinates */
        for (int s = 0; s < cylinders_.Count; s++) {
            int i = -1, j = -1;
            float local_xy_angle = Mathf.Atan2(plane_positions_[s].y, plane_positions_[s].x);
            GetArrayIndex(out i, out j, local_xy_angle);

            /* If the array slot is empty, or this one is closer, then store its index */
            if (array_[i, j] == -1) array_[i, j] = s;
            else if (plane_positions_[s].magnitude < plane_positions_[array_[i, j]].magnitude) array_[i, j] = s;
        }
    }

    /* Given an angle in radians (-pi, pi] find the array index that it corresponds to in the 3x3 array */
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
        ClearHighlightedAndResetColor();
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                if (i != 1 || j != 1) {
                    Destroy(arrows_[i, j]);
                }
            }
        }
    }

    private void ClearHighlightedAndResetColor() {
        foreach (ICylinder s in cylinders_) {
            s.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
            s.ResetColor();
        }
        center_cylinder_.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
        center_cylinder_.ResetColor();
    }

    public void AddCylinder(ICylinder s) {
        cylinders_.Add(s);
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
    }



    /* Not used */
    private float map_to_array(float in_a, float in_b, float in_t, float out_a, float out_b) {
        return out_a + ((out_b - out_a) / (in_b - in_a)) * (in_t - in_a);
    }
}
