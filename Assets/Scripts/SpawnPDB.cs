using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPDB : MonoBehaviour
{
    [SerializeField] GameObject prefab_atom = null;
    [SerializeField] GameObject prefab_bond = null;

    private Dictionary<int, List<Tuple<GameObject, Atom>>> residue_dictionary = new Dictionary<int, List<Tuple<GameObject, Atom>>>();

    // Start is called before the first frame update
    void Start()
    {
        List<Atom> atoms;
        List<List<int>> connections;
        PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1tes.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/4f0h.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1s5l.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1ea4.pdb", out atoms, out connections);

        Bounds atoms_bounding_box = new Bounds();

        foreach (Atom atom in atoms) {
            /* Units in Nano meters */
            Vector3 atom_position = new Vector3(atom.x_, atom.y_, atom.z_);
            atoms_bounding_box.Encapsulate(atom_position);

            /* Instantiate the object */
            GameObject temp = Instantiate(prefab_atom, atom_position, Quaternion.identity);
            temp.transform.parent = transform;
            temp.isStatic = this.gameObject.isStatic;

            InsertToResiudesDictionary(atom, temp);
        }

        Transform bonds_transform = transform.GetChild(0);
        int bonds = 0;
        foreach (KeyValuePair<int, List<Tuple<GameObject, Atom>>> value in residue_dictionary) {
            List<Tuple<GameObject,Atom>> resiude_atoms = value.Value;
            for (int ia = 0; ia < resiude_atoms.Count; ia++) {
                GameObject a = resiude_atoms[ia].Item1;
                Vector3 a_position = a.transform.position;
                float a_covalent_radius = AtomicRadii.GetCovalentRadius(resiude_atoms[ia].Item2.element_);
                for (int ib = 0; ib < resiude_atoms.Count; ib++) {
                    if (!(ia > ib)) continue;
                    GameObject b = resiude_atoms[ib].Item1;

                    Vector3 b_position = b.transform.position;
                    float b_covalent_radius = AtomicRadii.radii_covalent[resiude_atoms[ib].Item2.element_];

                    float distance = Vector3.Distance(a_position, b_position);
                    if (distance <= a_covalent_radius + b_covalent_radius + 0.015) {
                        bonds++;
                        GameObject temp = Instantiate(prefab_bond, a_position, Quaternion.identity);
                        temp.transform.parent = bonds_transform;
                        temp.isStatic = this.gameObject.isStatic;

                        Vector3 direction = b_position - a_position;
                        Quaternion toRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
                        temp.transform.rotation = toRotation;

                        ICylinderBench icylinder = temp.GetComponent<ICylinderBench>();
                        if (icylinder != null) {
                            icylinder.radius_ = AtomicRadii.ball_and_stick_bond_radius;
                            icylinder.height_ = distance;
                        } else {
                            temp.transform.localScale = new Vector3(0.5f, 0.5f * distance, 0.5f);
                        }
                    }
                }
            }
        }
        Debug.Log("Spawned: " + bonds + " bonds");
    }

    private void InsertToResiudesDictionary(Atom atom, GameObject temp) {
        int residue_key = CalculateUniqueResidueIdentifier(atom);
        if (!residue_dictionary.ContainsKey(residue_key)) {
            residue_dictionary.Add(residue_key, new List<Tuple<GameObject, Atom>>());
        }
        residue_dictionary[residue_key].Add(Tuple.Create(temp, atom));
    }

    private int CalculateUniqueResidueIdentifier(Atom atom) {
        int resiude = atom.res_seq_;
        char chain_id = atom.chain_id_;

        return chain_id * 10000 + resiude;
    }
}
