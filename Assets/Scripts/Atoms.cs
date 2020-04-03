using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VisualizationMethod
{
    SPACE_FILLING,
    BALL_AND_STICK,
}

public class Atoms : MonoBehaviour
{
    [SerializeField] GameObject prefab_atom = null;
    [SerializeField] GameObject prefab_bond = null;

    /* */
    List<ISphere> ispheres_ = new List<ISphere>();
    /* A dictionary that holds the type of the atom, and all the ISphere objects in the scene */
    private Dictionary<string, List<ISphere>> atoms_dictionary = new Dictionary<string, List<ISphere>>();
    /* A dictionary that holds the resiude id, and all the ISphere objects that form it */
    private Dictionary<int, List<ISphere>> residue_dictionary = new Dictionary<int, List<ISphere>>();

    private ISphere previously_highlighted_atom = null;

    private VisualizationMethod visualization_method_ = VisualizationMethod.BALL_AND_STICK;

    // Start is called before the first frame update
    void Start()
    {
        List<Atom> atoms;
        List<List<int>> connections;
        PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1tes.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/4f0h.pdb", out atoms, out connections);

        foreach (Atom atom in atoms)
        {
            /* Instantiate the object */
            GameObject temp = Instantiate(prefab_atom, new Vector3(atom.x_, atom.y_, atom.z_), Quaternion.identity);
            temp.transform.parent = transform;

            /* Find ISphere component, and set the atom */
            ISphere isphere = temp.GetComponent<ISphere>();
            isphere.atom_ = atom;
            ispheres_.Add(isphere);

            /* Insert to dictionary */
            InsertToAtomsDictionary(isphere);
            InsertToResiudesDictionary(isphere);
        }

        foreach (List<int> c in connections)
        {
            int atom_id = c[0];
            for (int i = 1; i < c.Count; i++)
            {
                ISphere connection_isphere = ispheres_[c[i]];
                ispheres_[atom_id].connections_.Add(connection_isphere);
            }
        }

        if (visualization_method_ == VisualizationMethod.BALL_AND_STICK)
        {
            foreach (KeyValuePair<int, List<ISphere>> value in residue_dictionary)
            {
                List<ISphere> resiude_atoms = value.Value;
                foreach (ISphere a in resiude_atoms)
                {
                    Vector3 a_position = a.transform.position;
                    float a_covalent_radius = AtomicRadii.GetCovalentRadius(a.atom_.element_);
                    foreach (ISphere b in resiude_atoms)
                    {
                        if (a == b) continue;

                        Vector3 b_position = b.transform.position;
                        float b_covalent_radius = AtomicRadii.radii_covalent[b.atom_.element_];

                        float distance = Vector3.Distance(a_position, b_position);
                        if (distance <= a_covalent_radius + b_covalent_radius + 0.015)
                        {
                            GameObject temp = Instantiate(prefab_bond, a_position, Quaternion.identity);
                            temp.transform.parent = transform;

                            Vector3 direction = b_position - a_position;
                            Quaternion toRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
                            temp.transform.rotation = toRotation;

                            ICylinder icylinder = temp.GetComponent<ICylinder>();
                            icylinder.radius_ = 0.015f;
                            icylinder.height_ = distance;
                        }
                    }
                }
            }
        }
    }

    public VisualizationMethod GetVisualizationMethod()
    {
        return visualization_method_;
    }

    private void InsertToAtomsDictionary(ISphere sphere) {
        string atom_name = sphere.atom_.element_;
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
            //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 1000, Color.white);
        }
    }

    private void SetColor(ISphere isphere)
    {
        Color rcolor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        foreach (ISphere s in atoms_dictionary[isphere.atom_.element_])
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

}
