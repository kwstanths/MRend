using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomicRadii 
{
    /* In angstrom */
    public static float ball_and_stick_radius = 0.04f;

    /* In angstrom */
    public static float ball_and_stick_bond_radius = 0.014f;

    /* A map that holds the covalent atomic radius for every element */
    public static Dictionary<string, float> radii_covalent = new Dictionary<string, float>(){
        {"H",  0.025f },
        {"LI", 0.145f },
        {"BE", 0.105f },
        {"B",  0.105f },
        {"C",  0.076f },
        {"N",  0.065f },
        {"O",  0.066f },
        {"F",  0.05f },

        {"MG", 0.150f },

        {"P",  0.1f },
        {"S",  0.1f },

        {"CA", 0.180f},

        {"MN", 0.140f },
        {"FE", 0.140f },

        /* Deuterium is not present in the atomic table, but some atoms in PDB files are of this type */
        {"D",  0.025f },
    };

    /* A map that holds the van der walls atomic radius for every element */
    public static Dictionary<string, float> radii_vanderwaals = new Dictionary<string, float>(){
        {"H",  0.110f },
        {"LI", 0.182f },
        {"BE", 0.153f },
        {"B",  0.192f },
        {"C",  0.170f },
        {"N",  0.155f },
        {"O",  0.152f },

        {"FE", 0.240f },
    };

    /* Get the covalent radius of an element */
    public static float GetCovalentRadius(string element)
    {
        try
        {
            return UnitConversion.TransformFromAngstrom(radii_covalent[element]);
        }
        catch (System.Exception)
        {
            Debug.Log("Can't find covalent radius for element: " + element);
            throw;
        }
    }
}
