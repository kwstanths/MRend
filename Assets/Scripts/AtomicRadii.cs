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
        {"H",   0.031f },
        {"HE",  0.028f },
        {"LI",  0.128f },
        {"BE",  0.096f },
        {"B",   0.084f },
        {"C",   0.076f },
        {"N",   0.071f },
        {"O",   0.066f },
        {"F",   0.057f },
        {"NE",  0.058f },
        {"NA",  0.166f },
        {"MG",  0.141f },
        {"AL",  0.121f },
        {"SI",  0.111f },
        {"P",   0.107f },
        {"S",   0.105f },
        {"CL",  0.102f },
        {"AR",  0.106f },
        {"K",   0.203f },
        {"CA",  0.176f },
        {"SC",  0.170f },
        {"TI",  0.160f },
        {"V",   0.153f },
        {"CR",  0.139f },
        {"MN",  0.140f },
        {"FE",  0.132f },
        {"CO",  0.126f },
        {"NI",  0.124f },
        {"CU",  0.132f },
        {"ZN",  0.122f },
        {"GA",  0.122f },
        {"GE",  0.120f },
        {"AS",  0.119f },
        {"SE",  0.120f },
        {"BR",  0.120f },
        {"KR",  0.116f },
        {"RB",  0.220f },
        {"SR",  0.195f },
        {"Y",   0.190f },
        {"ZR",  0.175f },
        {"NB",  0.164f },
        {"MO",  0.154f },
        {"TC",  0.147f },
        {"RU",  0.146f },
        {"RH",  0.142f },
        {"PD",  0.139f },
        {"AG",  0.145f },
        {"CD",  0.144f },
        {"IN",  0.142f },
        {"SN",  0.139f },
        {"SB",  0.139f },
        {"TE",  0.138f },
        {"I",   0.139f },
        {"XE",  0.140f },
        {"CS",  0.244f },
        {"BA",  0.215f },
        {"LA",  0.207f },
        {"CE",  0.204f },
        {"PR",  0.203f },
        {"ND",  0.201f },
        {"PM",  0.199f },
        {"SM",  0.198f },
        {"EU",  0.198f },
        {"GD",  0.196f },
        {"TB",  0.194f },
        {"DY",  0.192f },
        {"HO",  0.192f },
        {"ER",  0.189f },
        {"TM",  0.190f },
        {"YB",  0.187f },
        {"LU",  0.175f },
        {"HF",  0.187f },
        {"TA",  0.170f },
        {"W",   0.162f },
        {"RE",  0.151f },
        {"OS",  0.144f },
        {"IR",  0.141f },
        {"PT",  0.136f },
        {"AU",  0.136f },
        {"HG",  0.132f },
        {"TL",  0.145f },
        {"PB",  0.146f },
        {"BI",  0.148f },
        {"PO",  0.140f },
        {"AT",  0.150f },
        {"RN",  0.150f },
        {"FR",  0.260f },
        {"RA",  0.221f },
        {"AC",  0.215f },
        {"TH",  0.206f },
        {"PA",  0.200f },
        {"U",   0.196f },
        {"NP",  0.190f },
        {"PU",  0.187f },
        {"AM",  0.180f },
        {"CM",  0.169f },

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
