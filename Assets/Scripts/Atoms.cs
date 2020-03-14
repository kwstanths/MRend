using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atoms : MonoBehaviour
{
    [SerializeField] GameObject prefab_atom = null;
    
    /* A dictionary that holds the type of the atom, and all the ISphere objects in the scene */
    private Dictionary<string, List<ISphere>> atoms_dictionary = new Dictionary<string, List<ISphere>>();
    /* A dictionary that holds the resiude id, and all the ISphere objects that form it */
    private Dictionary<int, List<ISphere>> residue_dictionary = new Dictionary<int, List<ISphere>>();

    private ISphere previously_highlighted_atom = null;

    // Start is called before the first frame update
    void Start()
    {
        List<Atom> atoms;
        List<List<int>> connections;
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1tes.pdb", out atoms, out connections);
        PDBParser.ParseAtomsAndConnections(@"Assets/MModels/4f0h.pdb", out atoms, out connections);

        List<ISphere> ispheres = new List<ISphere>();
        foreach (Atom atom in atoms)
        {
            /* Instantiate the object */
            GameObject temp = Instantiate(prefab_atom, new Vector3(atom.x_, atom.y_, atom.z_), Quaternion.identity);
            temp.transform.parent = transform;

            /* Find ISphere component, and set the atom */
            ISphere isphere = temp.GetComponent<ISphere>();
            isphere.atom_ = atom;
            ispheres.Add(isphere);

            /* Insert to dictionary */
            InsertToAtomsDictionary(isphere);
            InsertToResiudesDictionary(isphere);
        }

        foreach (List<int> c in connections)
        {
            int atom_id = c[0];
            for (int i = 1; i < c.Count; i++)
            {
                ISphere connection_isphere = ispheres[c[i]];
                ispheres[atom_id].connections_.Add(connection_isphere);
            }
        }
    }

    private void InsertToAtomsDictionary(ISphere sphere) {
        string atom_name = sphere.atom_.name_;
        if (!atoms_dictionary.ContainsKey(atom_name)) {
            atoms_dictionary.Add(atom_name, new List<ISphere>());
        }
        atoms_dictionary[atom_name].Add(sphere);
    }

    private void InsertToResiudesDictionary(ISphere sphere)
    {
        int resiude = sphere.atom_.res_seq_;
        if (!residue_dictionary.ContainsKey(resiude))
        {
            residue_dictionary.Add(resiude, new List<ISphere>());
        }
        residue_dictionary[resiude].Add(sphere);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 100), (1.0f / Time.smoothDeltaTime).ToString());
    }

    // Update is called once per frame
    void Update() {
       
        RaycastHit hit;
        bool ret = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f);
        if (ret)
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * hit.distance, Color.white);

            ISphere isphere = hit.transform.GetComponent<ISphere>();

            if (Input.GetKeyDown(KeyCode.C) == true)
            {
                SetColor(isphere);
            }

            if (isphere == previously_highlighted_atom) return;
            HighLight(isphere);
        }
        else {
            if (previously_highlighted_atom != null)
            {
                RemoveHighlightFromPrevious();
            }
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 1000, Color.white);
        }
    }

    private void SetColor(ISphere isphere)
    {
        Color rcolor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        foreach (ISphere s in atoms_dictionary[isphere.atom_.name_])
        {
            s.SetColor(rcolor);
        }
    }

    private void HighLight(ISphere isphere)
    {
        if (previously_highlighted_atom != null && previously_highlighted_atom.atom_.res_seq_ != isphere.atom_.res_seq_)
        {
            RemoveHighlightFromPrevious();
        }

        isphere.SetHighlighted(true);
        foreach (ISphere s in residue_dictionary[isphere.atom_.res_seq_])
        {
            s.SetHighlighted(true);
        }
        previously_highlighted_atom = isphere;
    }

    private void RemoveHighlightFromPrevious()
    {
        previously_highlighted_atom.SetHighlighted(false);
        foreach (ISphere s in residue_dictionary[previously_highlighted_atom.atom_.res_seq_])
        {
            s.SetHighlighted(false);
        }
        previously_highlighted_atom = null;
    }

    private Material mat_hydrogen_;
    private Material mat_carbon_;
    private Material mat_nitrogen_;
    private Material mat_oxygen_;
    private Material mat_fclrine_;
    private Material mat_bromine_;
    private Material mat_iodine_;
    private Material mat_noble_gases_;
    private Material mat_phosphorus_;
    private Material mat_sulfur_;
    private Material mat_boron_;
    private Material mat_alkali_metals_;
    private Material mat_titanium_;
    private Material mat_iron_;
    private Material mat_alkaline_earth_metals_;

    private void ReadMaterials() {
        mat_hydrogen_ = Resources.Load("Materials/Hydrogen", typeof(Material)) as Material;
        mat_carbon_ = Resources.Load("Materials/Carbon", typeof(Material)) as Material;
        mat_nitrogen_ = Resources.Load("Materials/Nitrogen", typeof(Material)) as Material;
        mat_oxygen_ = Resources.Load("Materials/Oxygen", typeof(Material)) as Material;
        mat_fclrine_ = Resources.Load("Materials/FCLrine", typeof(Material)) as Material;
        mat_bromine_ = Resources.Load("Materials/Bromine", typeof(Material)) as Material;
        mat_iodine_ = Resources.Load("Materials/Iodine", typeof(Material)) as Material;
        mat_noble_gases_ = Resources.Load("Materials/NobleGases", typeof(Material)) as Material;
        mat_phosphorus_ = Resources.Load("Materials/Phosphorus", typeof(Material)) as Material;
        mat_sulfur_ = Resources.Load("Materials/Sulfur", typeof(Material)) as Material;
        mat_boron_ = Resources.Load("Materials/Boron", typeof(Material)) as Material;
        mat_alkali_metals_ = Resources.Load("Materials/AlkaliMetals", typeof(Material)) as Material;
        mat_alkaline_earth_metals_ = Resources.Load("Materials/AlkalineEarthMetals", typeof(Material)) as Material;
        mat_titanium_ = Resources.Load("Materials/Titanium", typeof(Material)) as Material;
        mat_iron_ = Resources.Load("Materials/Iron", typeof(Material)) as Material;
    }

    private void SetMaterial(GameObject go, Atom atom) {

        //if (atom.name_.Contains("C")) go.GetComponent<Renderer>().material = mat_carbon_;
        //if (atom.name_.Contains("O")) go.GetComponent<Renderer>().material = mat_oxygen_;
        //if (atom.name_.Contains("H")) go.GetComponent<Renderer>().material = mat_hydrogen_;


        //if (atom.name_.Equals("H")) {
        //    go.GetComponent<Renderer>().material = mat_hydrogen_;
        //} else if (atom.name_.Equals("C")) {
        //    go.GetComponent<Renderer>().material = mat_carbon_;
        //} else if (atom.name_.Equals("N")) {
        //    go.GetComponent<Renderer>().material = mat_nitrogen_;
        //} else if (atom.name_.Equals("O")) {
        //    go.GetComponent<Renderer>().material = mat_oxygen_;
        //} else if (atom.name_.Equals("F") || atom.name_.Equals("CL")) {
        //    go.GetComponent<Renderer>().material = mat_fclrine_;
        //} else if (atom.name_.Equals("BR")) {
        //    go.GetComponent<Renderer>().material = mat_bromine_;
        //} else if (atom.name_.Equals("I")) {
        //    go.GetComponent<Renderer>().material = mat_iodine_;
        //} else if (atom.name_.Equals("HE") || atom.name_.Equals("NE") || atom.name_.Equals("AR") || atom.name_.Equals("XE") || atom.name_.Equals("KR")) {
        //    go.GetComponent<Renderer>().material = mat_noble_gases_;
        //} else if (atom.name_.Equals("P")) {
        //    go.GetComponent<Renderer>().material = mat_phosphorus_;
        //} else if (atom.name_.Equals("S")) {
        //    go.GetComponent<Renderer>().material = mat_sulfur_;
        //} else if (atom.name_.Equals("B")) {
        //    go.GetComponent<Renderer>().material = mat_boron_;
        //} else if (atom.name_.Equals("LI") || atom.name_.Equals("NA") || atom.name_.Equals("K") || atom.name_.Equals("RB") || atom.name_.Equals("CS") || atom.name_.Equals("FR")) {
        //    go.GetComponent<Renderer>().material = mat_alkali_metals_;
        //} else if (atom.name_.Equals("BE") || atom.name_.Equals("MG") || atom.name_.Equals("CA") || atom.name_.Equals("SR") || atom.name_.Equals("BA") || atom.name_.Equals("RA")) {
        //    go.GetComponent<Renderer>().material = mat_alkaline_earth_metals_;
        //} else if (atom.name_.Equals("TI")) {
        //    go.GetComponent<Renderer>().material = mat_titanium_;
        //} else if (atom.name_.Equals("FE")) {
        //    go.GetComponent<Renderer>().material = mat_iron_;
        //}
    }

}
