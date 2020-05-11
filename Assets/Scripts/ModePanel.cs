using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModePanel : MonoBehaviour
{
    Canvas canvas_ = null;
    RectTransform canvas_transform_ = null;

    ButtonEvent button_visualization_;
    ButtonEvent button_selecton_plane_visualization_;

    private void Awake() {
        canvas_ = GetComponentInChildren<Canvas>();
        canvas_transform_ = canvas_.GetComponent<RectTransform>();
        button_visualization_ = canvas_.transform.Find("ButtonVisualization").GetComponent<ButtonEvent>();
        button_selecton_plane_visualization_ = canvas_.transform.Find("ButtonSelectionMode").GetComponent<ButtonEvent>();
    }

    void Update() {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        float distance_to_camera = Vector3.Distance(transform.position, Camera.main.transform.position);
        //transform.localScale = new Vector3(0.2f, 0.2f, 0.2f) + new Vector3(distance_to_camera, distance_to_camera, distance_to_camera) * 0.35f;

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
