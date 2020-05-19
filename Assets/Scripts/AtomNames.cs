using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomNames : MonoBehaviour
{

        /* A map that holds the full name for every element */
        public static Dictionary<string, string> names = new Dictionary<string, string>() {
        {"H",   "Hydrogen" },
        {"HE",  "Helium" },
        {"LI",  "Lithium" },
        {"BE",  "Beryllium" },
        {"B",   "Boron" },
        {"C",   "Carbon" },
        {"N",   "Nitrogen" },
        {"O",   "Oxygen" },
        {"F",   "Fluorine" },
        {"NE",  "Neon" },
        {"NA",  "Sodium" },
        {"MG",  "Magnesium" },
        {"AL",  "Aluminium" },
        {"SI",  "Silicon" },
        {"P",   "Phosphorus" },
        {"S",   "Sulfur" },
        {"CL",  "Chlorine" },
        {"AR",  "Argon" },
        {"K",   "Potassium" },
        {"CA",  "Calcium" },
        {"SC",  "Scandium" },
        {"TI",  "Titanium" },
        {"V",   "Vanadium" },
        {"CR",  "Chromium" },
        {"MN",  "Manganese" },
        {"FE",  "Iron" },
        {"CO",  "Cobalt" },
        {"NI",  "Nickel" },
        {"CU",  "Copper" },
        {"ZN",  "Zinc" },
        {"GA",  "Gallium" },
        {"GE",  "Germanium" },
        {"AS",  "Arsenic" },
        {"SE",  "Selenium" },
        {"BR",  "Bromine" },
        {"KR",  "Krypton" },
        {"RB",  "Rubidium" },
        {"SR",  "Strontium" },
        {"Y",   "Yttrium" },
        {"ZR",  "Zirconium" },
        {"NB",  "Niobium" },
        {"MO",  "Molybdenum" },
        {"TC",  "Technetium" },
        {"RU",  "Ruthenium" },
        {"RH",  "Rhodium" },
        {"PD",  "Palladium" },
        {"AG",  "Silver" },
        {"CD",  "Cadmium" },
        {"IN",  "Indium" },
        {"SN",  "Tin" },
        {"SB",  "Antimony" },
        {"TE",  "Tellurium" },
        {"I",   "Iodine" },
        {"XE",  "Xenon" },
        {"CS",  "Caesium" },
        {"BA",  "Barium" },
        {"LA",  "Lanthanum" },
        {"CE",  "Cerium" },
        {"PR",  "Praseodymium" },
        {"ND",  "Neodymium" },
        {"PM",  "Promethium" },
        {"SM",  "Samarium" },
        {"EU",  "Europium" },
        {"GD",  "Gadolinium" },
        {"TB",  "Terbium" },
        {"DY",  "Dysprosium" },
        {"HO",  "Holmium" },
        {"ER",  "Erbium" },
        {"TM",  "Thulium" },
        {"YB",  "Ytterbium" },
        {"LU",  "Lutetium" },
        {"HF",  "Hafnium" },
        {"TA",  "Tantalum" },
        {"W",   "Tungsten" },
        {"RE",  "Rhenium" },
        {"OS",  "Osmium" },
        {"IR",  "Iridium" },
        {"PT",  "Platinum" },
        {"AU",  "Gold" },
        {"HG",  "Mercury" },
        {"TL",  "Thallium" },
        {"PB",  "Lead" },
        {"BI",  "Bismuth" },
        {"PO",  "Polonium" },
        {"AT",  "Astatine" },
        {"RN",  "Radon" },
        {"FR",  "Francium" },
        {"RA",  "Radium" },
        {"AC",  "Actinium" },
        {"TH",  "Thorium" },
        {"PA",  "Protactinium" },
        {"U",   "Uranium" },
        {"NP",  "Neptunium" },
        {"PU",  "Plutonium" },
        {"AM",  "Americium" },
        {"CM",  "Curium" },

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
