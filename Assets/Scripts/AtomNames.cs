using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomNames : MonoBehaviour
{

        /* A map that holds the full name for every element */
        public static Dictionary<string, string> names = new Dictionary<string, string>() {
        {"H",  "Hydrogen" },
        {"LI", "Lithium" },
        {"BE", "Beryllium" },
        {"B",  "Boron" },
        {"C",  "Carbon" },
        {"N",  "Nitrogen" },
        {"O",  "Oxygen" },
        {"F",  "Fluorine" },

        {"MG", "Magnesium" },

        {"P",  "Phosphorus" },
        {"S",  "Sulfur" },

        {"CA", "Calcium"},

        {"MN", "Manganese" },
        {"FE", "Iron" },

        /* Deuterium is not present in the atomic table, but some atoms in PDB files are of this type */
        {"D",  "Deuteritium" },
    };


     public static string GetFullName(string element) {
        try {
            return names[element];
        }
        catch (System.Exception) {
            Debug.Log("Can't find full name for element: " + element);
            throw;
        }
    }
 }
