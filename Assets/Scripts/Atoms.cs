using System;
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
    public static float SELECTION_MODE_SPHERE_RADIUS;

    private VisualizationMethod visualization_method_ = VisualizationMethod.BALL_AND_STICK;
    [SerializeField] GameObject prefab_atom = null;
    [SerializeField] GameObject prefab_bond = null;

    /* Holds a list of all the spheres in the scene */
    List<ISphere> ispheres_ = new List<ISphere>();
    /* A dictionary that holds the type of the atom, and all the ISphere objects in the scene */
    private Dictionary<string, List<ISphere>> atoms_dictionary = new Dictionary<string, List<ISphere>>();
    /* A dictionary that holds the resiude id, and all the ISphere objects that form it */
    private Dictionary<int, List<ISphere>> residue_dictionary = new Dictionary<int, List<ISphere>>();
    /* A list that holds the currently highlighted spehres */
    private List<ISphere> highlighted_spheres_ = new List<ISphere>();

    public enum STATE
    {
        EXPLORING,
        SELECTED_ATOM,
        TORSION_ANGLE,
    }
    private STATE state = STATE.EXPLORING;

    /* Exploring mode paramters */
    private ISphere previously_highlighted_atom_ = null;
    private ICylinder previously_highlighted_bond_ = null;
    private ISphere selected_atom_ = null;
    /* Arc parameters */
    [SerializeField] GameObject prefab_arc_ = null;
    ICylinder[] bonds_selected_ = new ICylinder[2];
    int bonds_selected_id_ = 0;
    GameObject arc_previous_ = null;
    
    /* Selected atom paramters */
    [SerializeField] GameObject prefab_selection_plane_ = null;
    GameObject selection_plane_previous_ = null;

    /* Torsion angle paramters */
    ISphere[] atoms_selected_ = new ISphere[4];
    int atom_selected_id_ = 0;
    [SerializeField] GameObject prefab_torsion_angle = null;

    AtomInfoBox info_ui_;

    public float speed_object_move = 1;

    void Start()
    {
        List<Atom> atoms;
        List<List<int>> connections;
        PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1tes.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/4f0h.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1s5l.pdb", out atoms, out connections);

        Bounds atoms_bounding_box = new Bounds();

        foreach (Atom atom in atoms)
        {
            Vector3 atom_position = new Vector3(atom.x_, atom.y_, atom.z_);
            atoms_bounding_box.Encapsulate(atom_position);

            /* Instantiate the object */
            GameObject temp = Instantiate(prefab_atom, atom_position, Quaternion.identity);
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

        Transform bonds_transform = transform.GetChild(0);
        int bonds = 0;
        foreach (KeyValuePair<int, List<ISphere>> value in residue_dictionary)
        {
            List<ISphere> resiude_atoms = value.Value;
            for(int ia = 0; ia<resiude_atoms.Count; ia++)
            {
                ISphere a = resiude_atoms[ia];
                Vector3 a_position = a.transform.position;
                float a_covalent_radius = AtomicRadii.GetCovalentRadius(a.atom_.element_);
                for(int ib = 0; ib < resiude_atoms.Count; ib++)
                {
                    if (!(ia > ib)) continue;
                    ISphere b = resiude_atoms[ib];

                    Vector3 b_position = b.transform.position;
                    float b_covalent_radius = AtomicRadii.radii_covalent[b.atom_.element_];

                    float distance = Vector3.Distance(a_position, b_position);
                    if (distance <= a_covalent_radius + b_covalent_radius + 0.016)
                    {
                        bonds++;
                        GameObject temp = Instantiate(prefab_bond, a_position, Quaternion.identity);
                        temp.transform.parent = bonds_transform;

                        Vector3 direction = b_position - a_position;
                        Quaternion toRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
                        temp.transform.rotation = toRotation;

                        ICylinder icylinder = temp.GetComponent<ICylinder>();
                        icylinder.radius_ = AtomicRadii.ball_and_stick_bond_radius;
                        icylinder.height_ = distance;
                    }
                }
            }
        }

        SetCameraAndPanelBoxPosition(atoms_bounding_box);

        bonds_selected_[0] = null;
        bonds_selected_[1] = null;

        SELECTION_MODE_SPHERE_RADIUS = AtomicRadii.ball_and_stick_radius * 4.5f;

        info_ui_ = Camera.main.transform.Find("AtomInfoBox").GetComponent<AtomInfoBox>();

        Debug.Log("Spawned: " + bonds + " bonds");
    }
    
    private void SetCameraAndPanelBoxPosition(Bounds atoms_bounds) {
        Camera.main.transform.position = atoms_bounds.center + (atoms_bounds.extents.z + 0.7f) * new Vector3(0, 0, 1);
        Camera.main.transform.LookAt(atoms_bounds.center);

        Transform panel_box = transform.Find("Panel");
        panel_box.transform.position = atoms_bounds.center + (atoms_bounds.extents.x + 2) * Camera.main.transform.right;
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

    public void VisualizationMethodButtonClick() {
        if (visualization_method_ == VisualizationMethod.BALL_AND_STICK) SetVisualizationMethod(VisualizationMethod.SPACE_FILLING);
        else SetVisualizationMethod(VisualizationMethod.BALL_AND_STICK);
    }

    /**
        Change visualization method 
    */
    public void SetVisualizationMethod(VisualizationMethod method) {
        visualization_method_ = method;

        if (visualization_method_ == VisualizationMethod.BALL_AND_STICK) {
            SELECTION_MODE_SPHERE_RADIUS = AtomicRadii.ball_and_stick_radius * 4.5f;
            transform.GetChild(0).gameObject.SetActive(true);
            foreach(ISphere s in ispheres_) {
                s.SetRadius(AtomicRadii.ball_and_stick_radius);
            }
        } else {
            SELECTION_MODE_SPHERE_RADIUS = 0.04f * 6.0f;
            transform.GetChild(0).gameObject.SetActive(false);
            foreach (ISphere s in ispheres_) {
                s.SetAtomicRadius();
            }
        }
        if (selection_plane_previous_ != null) {
            SelectionPlane plane = selection_plane_previous_.GetComponent<SelectionPlane>();
            plane.ChangeRadius();
        }
    }

    //void OnGUI()
    //{
    //GUI.Label(new Rect(0, 0, 100, 100), (1.0f / Time.smoothDeltaTime).ToString());
    //}

    //Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) {
            bool ret = RayCastUIlayer();
            if (ret) return;
        }

        if (state == STATE.EXPLORING) {
            RaycastHit hit;
            /* 
             * Perform ray casting towards the camera direction, move the ray origin slightly forward to avoid intersections with spheres that
             * we are currently inside
             */
            bool ret = Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * AtomicRadii.ball_and_stick_radius, Camera.main.transform.forward, out hit, 100.0f);
            if (ret) {
                ISphere isphere = hit.transform.GetComponent<ISphere>();
                ICylinder icylinder = hit.transform.GetComponent<ICylinder>();
                ButtonEvent button = hit.transform.GetComponent<ButtonEvent>();
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * hit.distance, Color.white);

                if (isphere != null) {
                    if (Input.GetKeyDown(KeyCode.C) == true) {
                        SetColor(isphere);
                    }
                    if (Input.GetMouseButtonDown(0) == true) {
                        selected_atom_ = isphere;
                        state = STATE.SELECTED_ATOM;

                        SpawnSelectionPlane();

                        return;
                    }

                    if (isphere != previously_highlighted_atom_) {
                        info_ui_.SetAtom(isphere);
                        HighLight(isphere);
                    }
                } else if (icylinder != null) {
                    ClearHighlighted();

                    if (icylinder != bonds_selected_[0] && icylinder != bonds_selected_[1]) {
                        icylinder.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);
                        previously_highlighted_bond_ = icylinder;
                    }

                    if (Input.GetMouseButtonDown(0) == true) {
                        previously_highlighted_bond_ = null;

                        ICylinder previous = bonds_selected_[bonds_selected_id_ % 2];
                        if (previous != null) {
                            previous.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                        }

                        icylinder.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.BLUE);
                        bonds_selected_[bonds_selected_id_ % 2] = icylinder;

                        bonds_selected_id_++;

                        if (bonds_selected_[0] != null && bonds_selected_[1] != null &&  bonds_selected_[0] != bonds_selected_[1]) 
                        {
                            if (arc_previous_ != null) {
                                Destroy(arc_previous_);
                            }

                            SpawnArc(bonds_selected_[0], bonds_selected_[1]);
                            
                        }
                    }

                } else if (button != null) {
                    ProcessRayCastUIHit(hit);
                    ClearHighlighted();
                }



            } else {
                ClearHighlighted();
                //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 1000, Color.white);
            }

        } else if (state == STATE.SELECTED_ATOM) {
            bool ret = RayCastUIlayer();
            if (ret) return;

            if (Input.GetKeyDown(KeyCode.Escape) == true) {
                ClearHighlighted();
                selected_atom_ = null;
                state = STATE.EXPLORING;
                Destroy(selection_plane_previous_);
                return;
            }

            if (Input.GetKey(KeyCode.T)) {
                MoveTowardsSelectedAtom(speed_object_move);
            }

            if (Input.GetKey(KeyCode.Y)) {
                MoveTowardsSelectedAtom(-speed_object_move);
            }

        } else if (state == STATE.TORSION_ANGLE) {
            RaycastHit hit;
            /* 
             * Perform ray casting towards the camera direction, move the ray origin slightly forward to avoid intersections with spheres that
             * we are currently inside
             */
            bool ret = Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * AtomicRadii.ball_and_stick_radius, Camera.main.transform.forward, out hit, 100.0f);
            if (ret) {
                ISphere isphere = hit.transform.GetComponent<ISphere>();
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * hit.distance, Color.white);

                if (isphere != null) {
                    if (Input.GetMouseButtonDown(0) == true) {
                        int last_atom_id = atom_selected_id_ % 4;
                        if (atoms_selected_[last_atom_id] != null) atoms_selected_[last_atom_id].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);

                        atoms_selected_[last_atom_id] = isphere;
                        atom_selected_id_++;

                        isphere.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);

                        if (atoms_selected_[0] != null && atoms_selected_[1] != null && atoms_selected_[2] != null && atoms_selected_[3] != null) {

                            GameObject temp = Instantiate(prefab_torsion_angle, isphere.transform.position, Quaternion.identity);

                            TorsionAngle tangle = temp.GetComponent<TorsionAngle>();
                            tangle.pos4_ = atoms_selected_[atom_selected_id_ % 4].transform.position;
                            tangle.pos3_ = atoms_selected_[(atom_selected_id_ + 1)% 4].transform.position;
                            tangle.pos2_ = atoms_selected_[(atom_selected_id_ + 2)% 4].transform.position;
                            tangle.pos1_ = atoms_selected_[(atom_selected_id_ + 3)% 4].transform.position;
                        }
                    }

                }

            }



        }

        
    }

    private bool RayCastUIlayer() {
        RaycastHit hit;
        int layerMask = 1 << 5;
        bool ret = Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * AtomicRadii.ball_and_stick_radius, Camera.main.transform.forward, out hit, 100.0f, layerMask);
        if (ret) {
            ProcessRayCastUIHit(hit);
            return true;
        }
        return false;
    }

    private void ProcessRayCastUIHit(RaycastHit hit) {
        ButtonEvent button = hit.transform.GetComponent<ButtonEvent>();
        if (button == null) return;

        button.RayCastHover();
        if (Input.GetMouseButtonDown(0) == true) {
            button.RayCastHit();
        }
    }

    private void SpawnSelectionPlane() {
        if (selection_plane_previous_ != null) Destroy(selection_plane_previous_);

        selection_plane_previous_ = Instantiate(prefab_selection_plane_, selected_atom_.transform.position, Quaternion.identity);
        selection_plane_previous_.transform.parent = transform;
        SelectionPlane plane = selection_plane_previous_.GetComponent<SelectionPlane>();
        plane.center_sphere_ = selected_atom_;

        ClearHighlighted();
        Collider[] hitColliders = Physics.OverlapSphere(selected_atom_.transform.position, SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ISphere s = c.gameObject.GetComponent<ISphere>();
            if (s == null || s == selected_atom_) continue;

            plane.AddSphere(s);
        }
    }

    private void MoveTowardsSelectedAtom(float speed) {
        /* Calculate desired position of the selected sphere, just in front of the camera */
        Vector3 desired_position = Camera.main.transform.position + 2 * Camera.main.transform.forward * AtomicRadii.GetCovalentRadius(selected_atom_.atom_.element_);
        /* Calculate the movement speed */
        speed = speed * Vector3.Distance(selected_atom_.transform.position, desired_position);

        SelectionPlane plane = selection_plane_previous_.GetComponent<SelectionPlane>();
        
        /* Change position */
        Vector3 movement_direction = Vector3.Normalize(plane.center_sphere_.transform.position - desired_position);
        transform.position = transform.position - speed * Time.deltaTime * movement_direction;
    }

    private void SpawnArc(ICylinder a, ICylinder b) {
        /* Spawn an arc between two cylinders 
         * Calculate the origin and the direction by taking into consideration 
         * all possible cylinder directions
         */

        Vector3 position1 = a.transform.position;
        Vector3 position2 = b.transform.position;
        Vector3 position3 = position1 + a.transform.up * a.height_; 
        Vector3 position4 = position2 + b.transform.up * b.height_;

        Vector3 arc_origin;
        Vector3 arc_dir1;
        Vector3 arc_dir2;
        if (position1 == position2){
            arc_origin = position1;
            arc_dir1 = a.transform.up;
            arc_dir2 = b.transform.up;
        } else if (position1 == position4) {
            arc_origin = position1;
            arc_dir1 = a.transform.up;
            arc_dir2 = -b.transform.up;
        } else if (position2 == position3) {
            arc_origin = position2;
            arc_dir1 = -a.transform.up;
            arc_dir2 = b.transform.up;
        } else if (position4 == position3) {
            arc_origin = position3;
            arc_dir1 = -a.transform.up;
            arc_dir2 = -b.transform.up;
        } else {
            /* The two vectors don't form an angle */
            return;
        }

        arc_previous_ = Instantiate(prefab_arc_, arc_origin, Quaternion.identity);
        arc_previous_.transform.parent = transform;

        ArcRenderer arc = arc_previous_.GetComponent<ArcRenderer>();
        arc.X_ = arc_dir1;
        arc.W_ = arc_dir2;
        arc.Radius_ = (a.height_ + b.height_) / 4;
    }

    //public void OnDrawGizmos() {
    //    if (state == STATE.SELECTED_ATOM) {
    //        Gizmos.DrawWireSphere(selected_atom_.transform.position, AtomicRadii.ball_and_stick_bond_radius * 10);
    //    }
    //}

    private void SetColor(ISphere isphere)
    {
        Color rcolor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        foreach (ISphere s in atoms_dictionary[isphere.atom_.element_])
        {
            s.SetColor(rcolor);
        }
    }

    private void HighLight(ISphere isphere)
    {
        if (previously_highlighted_atom_ != null && previously_highlighted_atom_.atom_.res_seq_ != isphere.atom_.res_seq_)
        {
            ClearHighlighted();
        }

        isphere.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);
        highlighted_spheres_.Add(isphere);
        foreach (ISphere s in residue_dictionary[isphere.atom_.res_seq_])
        {
            s.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);
            highlighted_spheres_.Add(s);
        }
        previously_highlighted_atom_ = isphere;
    }

    private void ClearHighlighted()
    {
        foreach (ISphere s in highlighted_spheres_)
        {
            s.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
        }
        highlighted_spheres_.Clear();
        previously_highlighted_atom_ = null;

        if (previously_highlighted_bond_ != null) previously_highlighted_bond_.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
        previously_highlighted_bond_ = null;
    }

}
