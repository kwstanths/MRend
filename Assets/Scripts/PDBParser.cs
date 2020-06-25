using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PDBParser
{
    public static void ParseAtomsAndConnections(string file_name, out List<Atom> atoms, out List<List<int>> connections) {
        atoms = new List<Atom>();
        connections = new List<List<int>>();

        if (!File.Exists(file_name)) {
            Debug.Log("File: " + file_name + " not found");
            throw new IOException("Can't find model file: " + file_name);
        }

        string[] readText = File.ReadAllLines(file_name);
        foreach (string s in readText) {
            string type = s.Substring(0, 6);
            if (type.Equals("ATOM  ") || type.Equals("HETATM")) {
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
                string element = s.Substring(76, 2).Replace(" ", "");
                string charge = s.Substring(78, 2);

                atoms.Add(new Atom(serial, atom_name, alt_loc, res_name, chain_id, res_seq, i_code, x, y, z, occupancy, temp_factor, element, charge));
            }
            //else if (type.Equals("CONECT"))
            //{
            //    int atom_id = TryTransformConnection(s.Substring(6, 5));
            //    int connected_1 = TryTransformConnection(s.Substring(11, 5));
            //    int connected_2 = TryTransformConnection(s.Substring(16, 5));
            //    int connected_3 = TryTransformConnection(s.Substring(21, 5));
            //    int connected_4 = TryTransformConnection(s.Substring(26, 5));
            //    connections.Add(new List<int>());
            //    connections[connections.Count - 1].Add(atom_id);
            //    connections[connections.Count - 1].Add(connected_1);
            //    if (connected_2 != -1) connections[connections.Count - 1].Add(connected_2);
            //    if (connected_3 != -1) connections[connections.Count - 1].Add(connected_3);
            //    if (connected_4 != -1) connections[connections.Count - 1].Add(connected_4);
            //}

        }

        Debug.Log("Loaded: " + atoms.Count + " atoms");
        return;
    }

    static public float AngstromsToNanoMeters(float angstroms) {
        return angstroms * 0.1f;
    }

    static int TryTransformConnection(string number)
    {
        try
        {
            return System.Int32.Parse(number);
        }
        catch (System.FormatException)
        {

            return -1;
        }
    }
}
