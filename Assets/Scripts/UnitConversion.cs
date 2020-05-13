using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitConversion
{
    /* This class is not complete, some numbers dont take into account this scale */

    public enum UNITS {
        Angstrom,
        Nanometers,
    }

    public static UNITS unit_used = UNITS.Angstrom;

    public static Vector3 TransformFromAngstrom(Vector3 value) {
        if (unit_used == UNITS.Angstrom) return value;
        else if (unit_used == UNITS.Nanometers) return value * 10.0f;

        return new Vector3(0, 0, 0);
    }

    public static float TransformFromAngstrom(float value) {
        if (unit_used == UNITS.Angstrom) return value;
        else if (unit_used == UNITS.Nanometers) return value * 10.0f;

        return 0;
    }
}
