using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonEventSimple : ButtonEvent
{
    public UnityEvent button_hit_event_;

    public Sprite image_;
    public Sprite image_selected_;

    private Image renderer_;
    float last_time_hovered_;

    public void Awake() {
        renderer_ = GetComponent<Image>();

        renderer_.sprite = image_;

        if (button_hit_event_ == null) {
            button_hit_event_ = new UnityEvent();
        }
    }

    public override void RayCastHoverOff() {
        if (!(Time.time > last_time_hovered_ + 0.06)) return;

        renderer_.sprite = image_;
    }

    public override void RayCastHover() {
        renderer_.sprite = image_selected_;
        last_time_hovered_ = Time.time;
    }

    public override void RayCastHit() {
        button_hit_event_.Invoke();
    }

};