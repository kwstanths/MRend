using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPKColors 
{
    /* A map that holds the covalent atomic radius for every element */
    public static Dictionary<string, Color> colors = new Dictionary<string, Color>(){
        {"H",    Color.white },
        {"C",    new Color(0.08f, 0.08f, 0.08f) },
        {"N",    Color.blue },
        {"O",    Color.red },
        {"F",    Color.green },
        {"CL",   Color.green },
        {"BR",   new Color(0.54f, 0, 0) },
        {"I",    new Color(0.58f, 0, 0.82f) },
        { "HE",  Color.cyan },
        { "NE",  Color.cyan },
        { "AR",  Color.cyan },
        { "XE",  Color.cyan },
        { "KR",  Color.cyan },
        { "P",   new Color(1, 0.53f, 0) },
        { "S",   Color.yellow },
        { "B",   new Color(1, 0.85f, 0.6f) },
        { "LI",  new Color(0.54f, 0.16f, 0.88f) },
        { "NA",  new Color(0.54f, 0.16f, 0.88f) },
        { "K",   new Color(0.54f, 0.16f, 0.88f) },
        { "RB",  new Color(0.54f, 0.16f, 0.88f) },
        { "CS",  new Color(0.54f, 0.16f, 0.88f) },
        { "FR",  new Color(0.54f, 0.16f, 0.88f) },
        { "BE",  new Color(0, 0.39f, 0) },
        { "MG",  new Color(0, 0.39f, 0) },
        { "CA",  new Color(0, 0.39f, 0) },
        { "SR",  new Color(0, 0.39f, 0) },
        { "BA",  new Color(0, 0.39f, 0) },
        { "RA",  new Color(0, 0.39f, 0) },
        { "TI",  new Color(0.56f, 0.56f, 0.56f) },
        { "FE",  new Color(1, 0.54f, 0) },
    };
    public static Color color_other = new Color(1, 0, 0.8f);

    /**
        Get the CPK color the element 
    */
    public static Color GetCPKColor(string element) {
        try {
            return colors[element];
        }
        catch (System.Exception e) {
            return color_other;
        }
    }
}
