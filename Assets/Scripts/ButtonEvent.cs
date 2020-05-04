using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonEvent : MonoBehaviour
{
    public UnityEvent button_hit_event_;

    public Sprite image_1_;
    public Sprite image_1_selected_;
    public Sprite image_2_;
    public Sprite image_2_selected_;

    int image_mode_;
    private Image renderer_;
    float last_time_hovered_;

    public void Awake() {
        renderer_ = GetComponent<Image>();
        image_mode_ = 1;
        renderer_.sprite = image_1_;

        if (button_hit_event_ == null) {
            button_hit_event_ = new UnityEvent();
        }
    }

    public void RayCastHoverOff() {
        if (!(Time.time > last_time_hovered_ + 0.06)) return;

        if (image_mode_ == 1) {
            renderer_.sprite = image_1_;
        } else {
            renderer_.sprite = image_2_;
        }
    }

    public void RayCastHover() {
        if (image_mode_ == 1) {
            renderer_.sprite = image_1_selected_;
        } else {
            renderer_.sprite = image_2_selected_;
        }

        last_time_hovered_ = Time.time;
    }

    public void RayCastHit() {
        button_hit_event_.Invoke();
        if (image_mode_ == 1) {
            image_mode_ = 2;
            renderer_.sprite = image_2_;
        } else {
            image_mode_ = 1;
            renderer_.sprite = image_1_;
        }
    }
}
