using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomicRadii 
{
    /* A map that holds the covalent atomic radius for every element */
    public static Dictionary<string, float> radii_covalent = new Dictionary<string, float>(){
        {"H",  0.025f },
        {"LI", 0.145f },
        {"BE", 0.105f },
        {"B",  0.105f },
        {"C",  0.07f },
        {"N",  0.065f },
        {"O",  0.06f },

        {"FE", 0.140f },
    };

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
}
