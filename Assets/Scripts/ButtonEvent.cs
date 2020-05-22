using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class ButtonEvent : MonoBehaviour
{
    public abstract void RayCastHoverOff();

    public abstract void RayCastHover();

    public abstract void RayCastHit();
}


//public class ButtonOnOff : ButtonEvent
//{

//};
