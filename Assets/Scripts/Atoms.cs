using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atoms : MonoBehaviour
{
    [SerializeField] GameObject prefab_atom;
    
    private List<Atom> atoms_;

    private Material mat_hydrogen_;
    private Material mat_carbon_;
    private Material mat_nitrogen_;
    private Material mat_oxygen_;
    private Material mat_fclrine_;

    private Material mat_alkaline_earth_metals_;

    // Start is called before the first frame update
    void Start()
    {
        mat_hydrogen_ = Resources.Load("Materials/Hydrogen", typeof(Material)) as Material;
        mat_carbon_ = Resources.Load("Materials/Carbon", typeof(Material)) as Material;
        mat_nitrogen_ = Resources.Load("Materials/Nitrogen", typeof(Material)) as Material;
        mat_oxygen_ = Resources.Load("Materials/Oxygen", typeof(Material)) as Material;
        mat_fclrine_ = Resources.Load("Materials/FCLrine", typeof(Material)) as Material;
        mat_alkaline_earth_metals_ = Resources.Load("Materials/AlkalineEarthMetals", typeof(Material)) as Material;

        atoms_ = PDBParser.ParseAtoms(@"Assets/MModels/1tes.pdb");
        foreach(Atom atom in atoms_) {
            GameObject temp = Instantiate(prefab_atom, new Vector3(atom.x_, atom.y_, atom.z_), Quaternion.identity);
            temp.transform.parent = this.transform;

            SetMaterial(temp, atom);
        }
    }

    private void SetMaterial(GameObject go, Atom atom) {

        if (atom.name_.Equals("H")) {
            go.GetComponent<Renderer>().material = mat_hydrogen_;
        } else if (atom.name_.Equals("C")) {
            go.GetComponent<Renderer>().material = mat_carbon_;
        } else if (atom.name_.Equals("N")) {
            go.GetComponent<Renderer>().material = mat_nitrogen_;
        } else if (atom.name_.Equals("O")) {
            go.GetComponent<Renderer>().material = mat_oxygen_;
        } else if (atom.name_.Equals("F") || atom.name_.Equals("CL")) {
            go.GetComponent<Renderer>().material = mat_fclrine_;
        } else if (atom.name_.Equals("BE") || atom.name_.Equals("MG") || atom.name_.Equals("CA") || atom.name_.Equals("SR") || atom.name_.Equals("BA") || atom.name_.Equals("RA")) {
            go.GetComponent<Renderer>().material = mat_alkaline_earth_metals_;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
