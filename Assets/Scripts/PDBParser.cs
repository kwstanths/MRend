using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PDBParser
{
    public static List<Atom> ParseAtoms(string file_name) {
        List<Atom> temp = new List<Atom>();

        if (!File.Exists(file_name)) {
            return temp;
        }

        string[] readText = File.ReadAllLines(file_name);
        foreach (string s in readText) {
            string type = s.Substring(0, 6);
            if (type.Equals("ATOM  ")) {
                int serial = System.Int32.Parse(s.Substring(6, 5));
                string atom_name = s.Substring(12, 4).Replace(" ", "");
                char alt_loc = s[16];
                string res_name = s.Substring(17, 3);
                char chain_id = s[21];
                int res_seq = System.Int32.Parse(s.Substring(22, 4));
                char i_code = s[26];
                float x = AngstromsToNanoMeters(float.Parse(s.Substring(30, 8)));
                float y = AngstromsToNanoMeters(float.Parse(s.Substring(38, 8)));
                float z = AngstromsToNanoMeters(float.Parse(s.Substring(46, 8)));
                float occupancy = float.Parse(s.Substring(54, 6));
                float temp_factor = float.Parse(s.Substring(60, 6));
                string element = s.Substring(76, 2);
                string charge = s.Substring(78, 2);

                temp.Add(new Atom(serial, atom_name, alt_loc, res_name, chain_id, res_seq, i_code, x, y, z, occupancy, temp_factor, element, charge));
            }
        }

        return temp;
    }

    static public float AngstromsToNanoMeters(float angstroms) {
        return angstroms * 0.1f;
    }
}
