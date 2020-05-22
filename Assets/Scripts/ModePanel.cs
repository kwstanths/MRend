using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModePanel : MonoBehaviour
{
    Canvas canvas_ = null;
    RectTransform canvas_transform_ = null;

    ButtonEventToggle button_visualization_;
    ButtonEventToggle button_selecton_plane_visualization_;
    ButtonEventToggle button_exploring_method_;

    Text text_mode_;
    ButtonEventOnOff button_exploring_mode_;
    ButtonEventOnOff button_atom_distances_mode_;
    ButtonEventOnOff button_bond_angles_mode_;
    ButtonEventOnOff button_torsion_angles_mode_;

    Text text_radius_;
    ButtonEventSimple button_radius_up_;
    ButtonEventSimple button_radius_down_;

    private void Awake() {
        canvas_ = GetComponentInChildren<Canvas>();
        canvas_transform_ = canvas_.GetComponent<RectTransform>();

        button_visualization_ = canvas_.transform.Find("ButtonVisualization").GetComponent<ButtonEventToggle>();
        button_selecton_plane_visualization_ = canvas_.transform.Find("ButtonSelectionMode").GetComponent<ButtonEventToggle>();
        button_exploring_method_ = canvas_.transform.Find("ButtonExploringMethod").GetComponent<ButtonEventToggle>();

        text_mode_ = canvas_.transform.Find("text_select_mode").GetComponent<Text>();
        button_exploring_mode_ = canvas_.transform.Find("ButtonExploreAtoms").GetComponent<ButtonEventOnOff>();
        button_atom_distances_mode_ = canvas_.transform.Find("ButtonAtomDistances").GetComponent<ButtonEventOnOff>();
        button_bond_angles_mode_ = canvas_.transform.Find("ButtonBondAngles").GetComponent<ButtonEventOnOff>();
        button_torsion_angles_mode_ = canvas_.transform.Find("ButtonTorsionAngles").GetComponent<ButtonEventOnOff>();

        button_radius_up_ = canvas_.transform.Find("button_radius_up").GetComponent<ButtonEventSimple>();
        button_radius_down_ = canvas_.transform.Find("button_radius_down").GetComponent<ButtonEventSimple>();
        text_radius_ = canvas_.transform.Find("navigation_radius").GetComponent<Text>();

        transform.localScale = UnitConversion.TransformFromAngstrom(transform.localScale);
    }

    void Update() {
        Vector3 look_direction = transform.position - Camera.main.transform.position;
        look_direction.y = 0;
        transform.rotation = Quaternion.LookRotation(look_direction);

        //float distance_to_camera = Vector3.Distance(transform.position, Camera.main.transform.position);
        //transform.localScale = new Vector3(0.2f, 0.2f, 0.2f) + new Vector3(distance_to_camera, distance_to_camera, distance_to_camera) * 0.35f;

        /* If space is pushed, then render everything on top */
        bool ignore_z_test = Input.GetKey(KeyCode.Space);
        IgnoreZText[] texts = GetComponentsInChildren<IgnoreZText>();
        foreach (IgnoreZText t in texts) {
            t.DoZTest(!ignore_z_test);
        }
        IgnoreZImage[] images = GetComponentsInChildren<IgnoreZImage>();
        foreach (IgnoreZImage t in images) {
            t.DoZTest(!ignore_z_test);

        }

        button_visualization_.RayCastHoverOff();
        button_selecton_plane_visualization_.RayCastHoverOff();
        button_exploring_method_.RayCastHoverOff();
        button_exploring_mode_.RayCastHoverOff();
        button_atom_distances_mode_.RayCastHoverOff();
        button_bond_angles_mode_.RayCastHoverOff();
        button_torsion_angles_mode_.RayCastHoverOff();
        button_radius_up_.RayCastHoverOff();
        button_radius_down_.RayCastHoverOff();
    }

    public void SetState(Atoms.STATE state) {
        switch (state) {
            case Atoms.STATE.EXPLORING_ATOMS:
                button_atom_distances_mode_.Unselect();
                button_bond_angles_mode_.Unselect();
                button_torsion_angles_mode_.Unselect();
                text_mode_.text = "Select mode: Atom exploration";
                break;
            case Atoms.STATE.ATOM_DISTANCES:
                button_exploring_mode_.Unselect();
                button_bond_angles_mode_.Unselect();
                button_torsion_angles_mode_.Unselect();
                text_mode_.text = "Select mode: Atom distances";
                break;
            case Atoms.STATE.BOND_ANGLES:
                button_exploring_mode_.Unselect();
                button_atom_distances_mode_.Unselect();
                button_torsion_angles_mode_.Unselect();
                text_mode_.text = "Select mode: Bond angles";
                break;
            case Atoms.STATE.TORSION_ANGLE:
                button_exploring_mode_.Unselect();
                button_atom_distances_mode_.Unselect();
                button_bond_angles_mode_.Unselect();
                text_mode_.text = "Select mode: Torsion angles";
                break;
            default:
                break;
        }
    }

    public void SetRadius(float radius) {
        text_radius_.text = radius.ToString("F2");
    }

    public float GetHalfSizeX() {
        if (canvas_ == null) {
            print("AtomInfoBox: Canvas is null");
            return 0.0f;
        }

        return transform.localScale.x * canvas_.transform.localScale.x * canvas_transform_.sizeDelta.x / 2;
    }

    public void SetPosition(ISphere s, Vector3 Right) {
        transform.position = s.transform.position + Right * (Atoms.SELECTION_MODE_SPHERE_RADIUS + 0.06f + GetHalfSizeX());
    }

}
