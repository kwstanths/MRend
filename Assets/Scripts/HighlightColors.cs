using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightColors
{
    public enum HIGHLIGHT_COLOR
    {
        NO_HIGHLIGHT = 0,
        WHITE = 1,
        RED = 2,
        GREEN = 3,
    }
    static float[] highlight_values_ = { 0, 1, 0.6f, 0.3f };

    static public float GetColorValue(HIGHLIGHT_COLOR color) {
        return highlight_values_[(int)color];
    }

}
