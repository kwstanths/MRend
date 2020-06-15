using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class Atoms : MonoBehaviour
{
    public enum VisualizationMethod
    {
        SPACE_FILLING,
        BALL_AND_STICK,
    }

    public enum ExploringMethod
    {
        RESIDUES,
        CHAINS,
    }

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
    /* A dictionary that holds the chain id, and all the ISphere objects that form it */
    private Dictionary<char, List<ISphere>> chains_dictionary = new Dictionary<char, List<ISphere>>();
    /* A set that holds the currently highlighted spehres */
    private HashSet<ISphere> highlighted_spheres_ = new HashSet<ISphere>();
    /* A set that holds the currently colored spheres */
    private HashSet<ISphere> colored_spheres_ = new HashSet<ISphere>();
    /* */
    BoundingBox atoms_bounding_box_ = new BoundingBox();

    /* Interaction state */
    public enum STATE
    {
        EXPLORING_ATOMS,
        ATOM_DISTANCES,
        BOND_ANGLES,
        TORSION_ANGLE,
    }
    private STATE state = STATE.EXPLORING_ATOMS;

    /* Exploring mode paramters */
    ExploringMethod exploring_method_ = ExploringMethod.RESIDUES;

    /* Atom distance parameters */
    [SerializeField] GameObject prefab_atom_distance_ = null;
    GameObject atom_distance_previous_;

    /* Bond angle parameters */
    [SerializeField] GameObject prefab_arc_ = null;
    ICylinder[] bonds_selected_ = new ICylinder[2];
    private ICylinder previously_highlighted_bond_ = null;
    int bonds_selected_id_ = 0;
    GameObject arc_previous_ = null;

    /* Marked atom parameters */
    [SerializeField] GameObject prefab_marked_atom_ = null;
    GameObject marked_atom_object_ = null;
    GameObject marked_sphere_ = null;

    /* Selected atom and bond paramters */
    private ISphere selected_atom_ = null;
    private ICylinder selected_bond_ = null;
    [SerializeField] GameObject prefab_selection_plane_spheres_ = null;
    [SerializeField] GameObject prefab_selection_plane_cylinders_ = null;
    GameObject selection_plane_previous_ = null;
    SelectionVisualizationMethod selection_visualization_ = SelectionVisualizationMethod.ARROWS;

    /* Torsion angle paramters */
    ISphere[] atoms_selected_ = new ISphere[4];
    int atom_selected_id_ = 0;
    [SerializeField] GameObject prefab_torsion_angle = null;
    GameObject torsion_angle_previous_;
    bool torsion_plane_spawned_ = false;

    /* A reference to the information panel */
    AtomInfoBox info_ui_;

    /* Model movement paramters */
    public float speed_object_towards_move = 1;
    public float speed_object_vertical_move = 0.07f;

    void Start()
    {
        List<Atom> atoms;
        List<List<int>> connections;
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1tes.pdb", out atoms, out connections);
        PDBParser.ParseAtomsAndConnections(@"Assets/MModels/4f0h.pdb", out atoms, out connections);
        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1s5l.pdb", out atoms, out connections);

        //PDBParser.ParseAtomsAndConnections(@"Assets/MModels/1ea4.pdb", out atoms, out connections);

        /*  Spawn the objects */
        foreach (Atom atom in atoms)
        {
            /* Units in Nano meters */
            Vector3 atom_position = new Vector3(atom.x_, atom.y_, atom.z_);
            atoms_bounding_box_.AddPoint(atom_position);

            /* Instantiate the atom */
            GameObject temp = Instantiate(prefab_atom, atom_position, Quaternion.identity);
            temp.transform.parent = transform;
            temp.isStatic = this.gameObject.isStatic;

            /* Find ISphere component, and set the atom information */
            ISphere isphere = temp.GetComponent<ISphere>();
            isphere.atom_ = atom;
            ispheres_.Add(isphere);

            /* Insert to the dictionaries used */
            InsertToAtomsDictionary(isphere);
            InsertToResiudesDictionary(isphere);
            InsertToChainsDictionary(isphere);
        }
        
        /* Parse connections, currently the application does not do something with these connections */
        foreach (List<int> c in connections)
        {
            int atom_id = c[0];
            for (int i = 1; i < c.Count; i++)
            {
                ISphere connection_isphere = ispheres_[c[i]];
                ispheres_[atom_id].connections_.Add(connection_isphere);
            }
        }

        /* Spawn bonds */
        Transform bonds_transform = transform.GetChild(0);
        int bonds = 0;
        /* For all resisudes */
        foreach (KeyValuePair<int, List<ISphere>> value in residue_dictionary)
        {
            /* Get combinations of two atoms */
            List<ISphere> resiude_atoms = value.Value;
            for(int ia = 0; ia < resiude_atoms.Count; ia++)
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

                    /* If their distance is smaller then the sume of radius + plus a bias, then spawn a bond */
                    float distance = Vector3.Distance(a_position, b_position);
                    if (distance <= a_covalent_radius + b_covalent_radius + 0.015)
                    {
                        bonds++;
                        GameObject temp = Instantiate(prefab_bond, a_position, Quaternion.identity);
                        temp.transform.parent = bonds_transform;
                        temp.isStatic = this.gameObject.isStatic;

                        /* Rotate it accordingly */
                        Vector3 direction = b_position - a_position;
                        Quaternion toRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
                        temp.transform.rotation = toRotation;

                        /* Set size and radius */
                        ICylinder icylinder = temp.GetComponent<ICylinder>();
                        icylinder.radius_ = AtomicRadii.ball_and_stick_bond_radius;
                        icylinder.height_ = distance;
                    }
                }
            }
        }
        Debug.Log("Spawned: " + bonds + " bonds");

        /* Position the model and the camera in the world */
        SetCameraAndPanelBoxPosition(atoms_bounding_box_);

        bonds_selected_[0] = null;
        bonds_selected_[1] = null;

        /* Set some default info on the world panel */
        SELECTION_MODE_SPHERE_RADIUS = AtomicRadii.ball_and_stick_radius * 5.0f;
        transform.GetChild(1).GetComponent<ModePanel>().SetRadius(SELECTION_MODE_SPHERE_RADIUS);

        info_ui_ = Camera.main.transform.Find("AtomInfoBox").GetComponent<AtomInfoBox>();
    }

    private void SetCameraAndPanelBoxPosition(BoundingBox atoms_bounds) {
        /* We are not going to move the atoms object, instead, move everything else accordingly */
        /* Place camera */
        Camera.main.transform.position = atoms_bounds.center;
        Camera.main.transform.position += (atoms_bounds.extents.z + 0.9f) * new Vector3(0, 0, 1);
        Camera.main.transform.position += atoms_bounds.extents.y/6 * new Vector3(0, 1, 0);

        Camera.main.transform.LookAt(atoms_bounds.center);
        
        /* Place world panel */
        Transform panel_box = transform.Find("Panel");
        panel_box.transform.position = atoms_bounds.center + (atoms_bounds.extents.x + 2) * Camera.main.transform.right;

        /* Place virtual floor and walls */
        GameObject walls = GameObject.Find("Walls");
        walls.transform.position = Camera.main.transform.position;
        walls.transform.position = new Vector3(atoms_bounds.center.x, atoms_bounds.min.y - 0.08f, atoms_bounds.center.z);
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
        int residue_key = CalculateUniqueResidueIdentifier(sphere);
        if (!residue_dictionary.ContainsKey(residue_key))
        {
            residue_dictionary.Add(residue_key, new List<ISphere>());
        }
        residue_dictionary[residue_key].Add(sphere);
    }

    private void InsertToChainsDictionary(ISphere sphere) {
        char chain = sphere.atom_.chain_id_;
        if (!chains_dictionary.ContainsKey(chain)) {
            chains_dictionary.Add(chain, new List<ISphere>());
        }
        chains_dictionary[chain].Add(sphere);
    }

    /* Calculate a unique identifier for a residues, since different chains use the same ids */
    private int CalculateUniqueResidueIdentifier(ISphere sphere) {
        int resiude = sphere.atom_.res_seq_;
        char chain_id = sphere.atom_.chain_id_;

        return chain_id * 10000 + resiude;
    }

    public void SetVisualizationMethod(VisualizationMethod method) {
        visualization_method_ = method;

        if (visualization_method_ == VisualizationMethod.BALL_AND_STICK) {
            /* Activate all the bonds */
            transform.GetChild(0).gameObject.SetActive(true);
            /* Set the ball and stick radius */
            foreach (ISphere s in ispheres_) {
                s.SetRadius(AtomicRadii.ball_and_stick_radius);
            }
        } else {
            /* Deactivaate all the bonds */
            transform.GetChild(0).gameObject.SetActive(false);
            /* Set the covalent radius */
            foreach (ISphere s in ispheres_) {
                s.SetAtomicRadius();
            }
        }
    }

    /* The following change function are called by string reference, from the world UI buttons */
    public void ChangeVisualizationMethod() {
        if (visualization_method_ == VisualizationMethod.BALL_AND_STICK) {
            SetVisualizationMethod(VisualizationMethod.SPACE_FILLING);

            /* 
             * If we are to change visualization method to space filling, change the marked atom object
             * since its position depends on the radius, and if we are bond angles mode, destroy reset state
             * since there are no bonds anymore
             */
            if (marked_atom_object_ != null) {
                ISphere sphere = marked_sphere_.GetComponent<ISphere>();
                marked_atom_object_.transform.position = sphere.transform.position + 1.4f * Vector3.up * AtomicRadii.GetCovalentRadius(sphere.atom_.element_);
            }
            if (state == STATE.BOND_ANGLES) {
                ResetState(true);
            }
        } else {
            SetVisualizationMethod(VisualizationMethod.BALL_AND_STICK);

            if (marked_atom_object_ != null) {
                ISphere sphere = marked_sphere_.GetComponent<ISphere>();
                marked_atom_object_.transform.position = sphere.transform.position + 1.4f * Vector3.up * AtomicRadii.ball_and_stick_radius;
            }
        }
    }
    public void ChangeSelectionPlaneVisualizationMethod() {
        /* Change the visualization method of the navigation, i.e. arrows and color circle */
        if (selection_visualization_ == SelectionVisualizationMethod.ARROWS) selection_visualization_ = SelectionVisualizationMethod.COLOR_CIRCLE;
        else selection_visualization_ = SelectionVisualizationMethod.ARROWS;

        if (selection_plane_previous_ != null) {
            SelectionPlaneSpheres planes = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();
            SelectionPlaneCylinders planec = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>();
            if (planes != null) planes.ChangeVisualization();
            if (planec != null) planec.ChangeVisualization();
        }
    }
    public void ChangeExploringMethod() {
        if (exploring_method_ == ExploringMethod.RESIDUES) {
            ClearHighlighted();
            exploring_method_ = ExploringMethod.CHAINS;
        } else {
            ClearColored();
            exploring_method_ = ExploringMethod.RESIDUES;
        }
    }
    public void ChangeModeToExploringAtoms() {
        /* 
         * If the previous state is bond angles, and a bond was selected, then find the closest atom
         * and move the selection there
         */
        if (state == STATE.BOND_ANGLES) {
            ICylinder currently_selected_cylinder = null;
            if (selection_plane_previous_ != null) {
                currently_selected_cylinder = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>().center_cylinder_;
            }
            ResetState(true);

            if (currently_selected_cylinder != null) {
                /* If there was a selected cylinder, move selection to an atom */
                ISphere nearest_atom = GetNearestAtom(currently_selected_cylinder.transform.position, 2 * AtomicRadii.ball_and_stick_radius);
                if (nearest_atom != null) {
                    selected_atom_ = nearest_atom;
                    SpawnSelectionPlaneSpheres();
                }
            }

        }
        else ResetState(false);

        state = STATE.EXPLORING_ATOMS;
        transform.GetChild(1).GetComponent<ModePanel>().SetState(state);
    }
    public void ChangeModeToAtomDistances() {
        /* 
         * If the previous state is bond angles, and a bond was selected, then find the closest atom
         * and move the selection there
         */
        if (state == STATE.BOND_ANGLES) {
            ICylinder currently_selected_cylinder = null;
            if (selection_plane_previous_ != null) {
                currently_selected_cylinder = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>().center_cylinder_;
            }
            ResetState(true);

            if (currently_selected_cylinder != null) {

                ISphere nearest_atom = GetNearestAtom(currently_selected_cylinder.transform.position, 2 * AtomicRadii.ball_and_stick_radius);
                if (nearest_atom != null) {
                    selected_atom_ = nearest_atom;
                    SpawnSelectionPlaneSpheres();
                }
            }

        } else ResetState(false);

        state = STATE.ATOM_DISTANCES;
        transform.GetChild(1).GetComponent<ModePanel>().SetState(state);
    }
    public void ChangeModeToBondAngles() {
        /* 
         * If the previous state is not bond angles, and an atom was selected, then find the closest bond
         * and move the selection there
         */
        if (state != STATE.BOND_ANGLES) {
            ISphere currently_selected_sphere = null;
            if (selection_plane_previous_ != null) {
                currently_selected_sphere = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>().center_sphere_;
            }
            ResetState(true);

            if (currently_selected_sphere != null) {

                ICylinder nearest_bond = GetNearestBond(currently_selected_sphere.transform.position, 2 * AtomicRadii.GetCovalentRadius(currently_selected_sphere.atom_.element_));
                if (nearest_bond != null) {
                    selected_bond_ = nearest_bond;
                    SpawnSelectionPlaneCylinders();
                }
            }
        }
        else ResetState(false);

        state = STATE.BOND_ANGLES;
        transform.GetChild(1).GetComponent<ModePanel>().SetState(state);
        info_ui_.ResetInfo();
    }
    public void ChangeModeToTorsionAngles() {
        /* 
         * If the previous state is bond angles, and a bond was selected, then find the closest atom
         * and move the selection there
         */
        if (state == STATE.BOND_ANGLES) {
            ICylinder currently_selected_cylinder = null;
            if (selection_plane_previous_ != null) {
                currently_selected_cylinder = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>().center_cylinder_;
            }
            ResetState(true);

            if (currently_selected_cylinder != null) {

                ISphere nearest_atom = GetNearestAtom(currently_selected_cylinder.transform.position, 2 * AtomicRadii.ball_and_stick_radius);
                if (nearest_atom != null) {
                    selected_atom_ = nearest_atom;
                    SpawnSelectionPlaneSpheres();
                }
            }

        }
        else ResetState(false);

        state = STATE.TORSION_ANGLE;
        transform.GetChild(1).GetComponent<ModePanel>().SetState(state);
    }

    /* Functions to increase and decrease the selection radius of the 2D navigation sphere */
    public void IncreaseSelectionSphereRadius() {
        Atoms.SELECTION_MODE_SPHERE_RADIUS += 0.02f;
        if (selection_plane_previous_ != null) {
            SelectionPlaneSpheres planes = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();
            SelectionPlaneCylinders planec = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>();
            if (planes != null) planes.ChangeRadius();
            if (planec != null) planec.ChangeRadius();
        }
        transform.GetChild(1).GetComponent<ModePanel>().SetRadius(SELECTION_MODE_SPHERE_RADIUS);
    }
    public void DecreaseSelectionSphereRadius() {
        Atoms.SELECTION_MODE_SPHERE_RADIUS -= 0.02f;
        if (selection_plane_previous_ != null) {
            SelectionPlaneSpheres planes = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();
            SelectionPlaneCylinders planec = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>();
            if (planes != null) planes.ChangeRadius();
            if (planec != null) planec.ChangeRadius();
        }
        transform.GetChild(1).GetComponent<ModePanel>().SetRadius(SELECTION_MODE_SPHERE_RADIUS);
    }

    /* Reset the current state
     * @param destroy_selection If true, destroy the 2D selection object
     */
    private void ResetState(bool destroy_selection) {
        switch (state) {
            case STATE.EXPLORING_ATOMS:
                if (selection_plane_previous_ != null) {
                    if (destroy_selection) {
                        Destroy(selection_plane_previous_);
                        selected_atom_ = null;
                        info_ui_.ResetInfo();
                    }
                }

                break;
            case STATE.ATOM_DISTANCES:
                if (selection_plane_previous_ != null) {
                    if (destroy_selection) {
                        Destroy(selection_plane_previous_);
                        selected_atom_ = null;
                        info_ui_.ResetInfo();
                    }
                }
                if (atom_distance_previous_ != null) Destroy(atom_distance_previous_);
                if (marked_atom_object_ != null) Destroy(marked_atom_object_);
                marked_sphere_ = null;
                atoms_selected_[0] = null;
                atoms_selected_[1] = null;
                atoms_selected_[2] = null;
                atoms_selected_[3] = null;
                atom_selected_id_ = 0;

                break;
            case STATE.BOND_ANGLES:
                if (selection_plane_previous_ != null) {
                    if (destroy_selection) {
                        Destroy(selection_plane_previous_);
                        selected_bond_ = null;
                    }
                }
                if (arc_previous_ != null) Destroy(arc_previous_);
                if (bonds_selected_[0] != null) bonds_selected_[0].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                if (bonds_selected_[1] != null) bonds_selected_[1].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                bonds_selected_[0] = null;
                bonds_selected_[1] = null;
                bonds_selected_id_ = 0;

                break;
            case STATE.TORSION_ANGLE:
                if (selection_plane_previous_ != null) {
                    if (destroy_selection) {
                        Destroy(selection_plane_previous_);
                        selected_atom_ = null;
                        info_ui_.ResetInfo();
                    }
                }
                if (torsion_angle_previous_ != null) Destroy(torsion_angle_previous_);
                if (atoms_selected_[0] != null) atoms_selected_[0].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                if (atoms_selected_[1] != null) atoms_selected_[1].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                if (atoms_selected_[2] != null) atoms_selected_[2].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                if (atoms_selected_[3] != null) atoms_selected_[3].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
                atoms_selected_[0] = null;
                atoms_selected_[1] = null;
                atoms_selected_[2] = null;
                atoms_selected_[3] = null;
                atom_selected_id_ = 0;
                torsion_plane_spawned_ = false;
                info_ui_.ClearTorsionAtoms();

                break;
            default:
                break;
        }

        ClearColored();
        ClearHighlighted();
    }

    /* Get nearest atom given a position and a radius */
    private ISphere GetNearestAtom(Vector3 position, float radius) {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius);

        ISphere current_nearest = null;
        float nearest_distance = radius;
        foreach (Collider c in hitColliders) {
            ISphere s = c.gameObject.GetComponent<ISphere>();
            if (s == null) continue;

            float distance = Vector3.Distance(position, s.transform.position);
            if (distance < nearest_distance) {
                nearest_distance = distance;
                current_nearest = s;
            }
        }

        return current_nearest;
    }

    /* Get nearest bond given a position and a radius */
    private ICylinder GetNearestBond(Vector3 position, float radius) {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius);

        ICylinder current_nearest = null;
        float nearest_distance = radius;
        foreach (Collider c in hitColliders) {
            ICylinder s = c.gameObject.GetComponent<ICylinder>();
            if (s == null) continue;

            float distance = Vector3.Distance(position, s.transform.position);
            if (distance < nearest_distance) {
                nearest_distance = distance;
                current_nearest = s;
            }
        }

        return current_nearest;
    }

    /* FPS counter */
    //void OnGUI() {
    //    GUI.Label(new Rect(0, 0, 100, 100), (1.0f / Time.smoothDeltaTime).ToString());
    //}

    void Update() {

        /* If the bring forward the panel button is hit, the ray cast only in UI layer */
        if (Input.GetKey(KeyCode.Space)) {
            bool ui_hit = RayCastUIlayer();
            if (ui_hit) return;
        }

        RaycastHit hit;
        /* 
         * Perform ray casting towards the camera direction, move the ray origin slightly forward to avoid intersections with spheres that
         * we are currently inside
         */
        bool ray_cast_hit = Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * AtomicRadii.ball_and_stick_radius, Camera.main.transform.forward, out hit, 100.0f);

        /* Process model movement input */
        if (Input.GetKey(KeyCode.Keypad1)) {
            MoveTowardsSelectedObject(speed_object_towards_move);
        }

        if (Input.GetKey(KeyCode.Keypad7)) {
            MoveTowardsSelectedObject(-speed_object_towards_move);
        }

        if (Input.GetKey(KeyCode.Keypad4)) {
            transform.position = transform.position - Camera.main.transform.right * speed_object_vertical_move;
        }

        if (Input.GetKey(KeyCode.Keypad6)) {
            transform.position = transform.position + Camera.main.transform.right * speed_object_vertical_move;
        }

        if (Input.GetKey(KeyCode.Keypad8)) {
            transform.position = transform.position + Camera.main.transform.up * speed_object_vertical_move;
        }

        if (Input.GetKey(KeyCode.Keypad5)) {
            transform.position = transform.position - Camera.main.transform.up * speed_object_vertical_move;
        }

        /* Check ray cast against UI */
        if (ray_cast_hit) {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * hit.distance, Color.white);

            ButtonEvent button = hit.transform.GetComponent<ButtonEvent>();
            if (button != null) {
                ProcessRayCastUIHit(hit);
            }
        }

        /* Process state machine */
        if (state == STATE.EXPLORING_ATOMS) {

            /* If there is a selected atom, and the discard button is pressed, then destory selection */
            if (selected_atom_ != null) {
                if (Input.GetKeyDown(KeyCode.Escape) == true) {
                    ClearHighlighted();
                    selected_atom_ = null;
                    Destroy(selection_plane_previous_);
                    return;
                }

                return;
            }

            /* Else process ray cast hit */
            if (ray_cast_hit) {

                /* Hit a sphere? */
                ISphere isphere = hit.transform.GetComponent<ISphere>();
                if (isphere != null) {
                    /* Check if selection of an atom is triggered */
                    if (Input.GetMouseButtonDown(0) == true) {
                        selected_atom_ = isphere;
                        SpawnSelectionPlaneSpheres();
                        return;
                    }

                    /* If not, set highlighting based on the visulization method */
                    info_ui_.SetAtom(isphere);
                    if (exploring_method_ == ExploringMethod.RESIDUES && !highlighted_spheres_.Contains(isphere)) {
                        ClearHighlighted();
                        HighLightResidue(isphere);
                    }
                    else if (exploring_method_ == ExploringMethod.CHAINS) {
                        /* Highlight residue */
                        if (!highlighted_spheres_.Contains(isphere)) {
                            ClearHighlighted();
                            HighLightResidue(isphere);
                        }

                        /* Color chain */
                        if (!colored_spheres_.Contains(isphere)) {
                            ClearColored();
                            ColorChain(isphere);
                        }
                    }

                }

            } else {
                /* If ray cast failed, clear highlighting */
                ClearHighlighted();
                if (exploring_method_ != ExploringMethod.CHAINS) ClearColored();

                //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 1000, Color.white);
            }

        }
        else if (state == STATE.ATOM_DISTANCES) {

            /* If discard button, then reset state */
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ResetState(true);
                return;
            }

            /* if we have selected an atom, process distances */
            if (selected_atom_ != null) {
                SelectionPlaneSpheres plane = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();

                /* 
                 * Get the current selected atom, and if it's different than the previous, and the marked button is pressed
                 * mark atom, and calculate distance
                 */
                int current_index = atom_selected_id_ % 4;
                ISphere previously_added = ((current_index == 0) ? atoms_selected_[3] : atoms_selected_[current_index - 1]);
                if (Input.GetKeyDown(KeyCode.E) && previously_added != plane.center_sphere_) {
                    atoms_selected_[current_index] = plane.center_sphere_;

                    atom_selected_id_++;

                    /* If marked atom object is null, spawn it */
                    if (marked_atom_object_ == null) {
                        marked_atom_object_ = Instantiate(prefab_marked_atom_, plane.center_sphere_.transform.position, Quaternion.identity);
                        marked_atom_object_.transform.parent = this.transform;
                    }
                    /* Calculate radius of the atom based on the viusalization method, and move marked atom object */
                    float selected_atom_radius;
                    if (GetVisualizationMethod() == VisualizationMethod.BALL_AND_STICK) {
                        selected_atom_radius = AtomicRadii.ball_and_stick_radius;
                    }
                    else {
                        selected_atom_radius = AtomicRadii.GetCovalentRadius(plane.center_sphere_.atom_.element_);
                    }
                    marked_atom_object_.transform.position = plane.center_sphere_.transform.position + 1.4f * Vector3.up * selected_atom_radius;
                    marked_sphere_ = plane.center_sphere_.gameObject;

                    /* If the previously marked atom is not null, then calculate distance */
                    if (previously_added != null) {
                        Vector3 middle = (previously_added.transform.position + plane.center_sphere_.transform.position) / 2;

                        /* Destroy the previous atom distance object from the world */
                        if (atom_distance_previous_ != null) Destroy(atom_distance_previous_);
                        /* and spawn the new in the middle of the distance */
                        atom_distance_previous_ = Instantiate(prefab_atom_distance_, middle, Quaternion.identity);
                        atom_distance_previous_.transform.parent = transform;
                        AtomDistance temp = atom_distance_previous_.GetComponent<AtomDistance>();
                        temp.atom1_ = previously_added;
                        temp.atom2_ = plane.center_sphere_;
                    }
                }
                return;
            }

            /* If there is not selected atom, process ray casting as usual */
            if (ray_cast_hit) {
                ISphere isphere = hit.transform.GetComponent<ISphere>();

                if (isphere != null) {
                    if (Input.GetMouseButtonDown(0) == true) {
                        selected_atom_ = isphere;
                        SpawnSelectionPlaneSpheres();
                        return;
                    }

                    info_ui_.SetAtom(isphere);
                    if (exploring_method_ == ExploringMethod.RESIDUES && !highlighted_spheres_.Contains(isphere)) {
                        ClearHighlighted();
                        HighLightResidue(isphere);
                    }
                    else if (exploring_method_ == ExploringMethod.CHAINS) {
                        if (!highlighted_spheres_.Contains(isphere)) {
                            ClearHighlighted();
                            HighLightResidue(isphere);
                        }

                        if (!colored_spheres_.Contains(isphere)) {
                            ClearColored();
                            ColorChain(isphere);
                        }
                    }
                }

            } else {
                ClearHighlighted();
                if (exploring_method_ != ExploringMethod.CHAINS) ClearColored();
            }


        }
        else if (state == STATE.BOND_ANGLES) {

            /* If discard button is hit, reset state */
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ResetState(true);
                return;
            }

            /* If selection is spawned, process selected bonds */
            if (selected_bond_ != null) {
                SelectionPlaneCylinders plane = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>();

                int current_index = bonds_selected_id_ % 2;
                ICylinder previously_added = ((current_index == 0) ? bonds_selected_[1] : bonds_selected_[current_index - 1]);
                /* if the precious is different than the currently selected, select bond, and the arc */
                if (previously_added != plane.center_cylinder_) {
                    bonds_selected_[current_index] = plane.center_cylinder_;

                    bonds_selected_id_++;

                    if (previously_added != null) {
                        if (arc_previous_ != null) Destroy(arc_previous_);
                        SpawnArc(bonds_selected_[0], bonds_selected_[1]);
                    }
                }
                return;
            }

            /* Else, process ray casting */
            if (ray_cast_hit) {
                ClearHighlighted();

                ICylinder icylinder = hit.transform.GetComponent<ICylinder>();

                if (icylinder != null) {
                    icylinder.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);
                    previously_highlighted_bond_ = icylinder;

                    if (Input.GetMouseButtonDown(0) == true) {
                        selected_bond_ = icylinder;
                        SpawnSelectionPlaneCylinders();
                    }

                }

            }
            else {
                ClearHighlighted();
            }

        }
        else if (state == STATE.TORSION_ANGLE) {

            /* if the torsion planed has been spawned, highlight the atoms that participate in it */
            if (torsion_plane_spawned_) {
                atoms_selected_[0].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);
                atoms_selected_[1].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);
                atoms_selected_[2].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);
                atoms_selected_[3].SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.GREEN);
            }

            /* If discard is pushed, and the torsion planed is spawned, go back to atom selection
             * else reset and destroy selection
             */
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (torsion_plane_spawned_) {
                    ISphere last_added_sphere = ((atom_selected_id_ == 0) ? atoms_selected_[3] : atoms_selected_[atom_selected_id_ - 1]);
                    ResetState(false);
                    selected_atom_ = last_added_sphere;
                    SpawnSelectionPlaneSpheres();
                    info_ui_.SetAtom(last_added_sphere);
                }
                else {
                    ResetState(true);
                }

                return;
            }

            /* If selection is spawned, then process the currently selected atom */
            if (selected_atom_ != null) {
                SelectionPlaneSpheres plane = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();

                ISphere previously_added = ((atom_selected_id_ == 0) ? atoms_selected_[3] : atoms_selected_[atom_selected_id_ - 1]);
                if (Input.GetKeyDown(KeyCode.E) && atom_selected_id_ < 4 && previously_added != plane.center_sphere_) {
                    atoms_selected_[atom_selected_id_ % 4] = plane.center_sphere_;
                    info_ui_.SetTorsionAtom(atom_selected_id_, plane.center_sphere_.atom_.name_);

                    atom_selected_id_++;

                    /* If reached 4 selected atoms, spawn the torsion plane object */
                    if (atom_selected_id_ == 4) {
                        SpawnTorsionAngle();
                        selected_atom_ = null;
                        Destroy(selection_plane_previous_);
                        torsion_plane_spawned_ = true;
                    }
                }
                return;
            }

            /* Else ray cast as usual */
            if (ray_cast_hit && !torsion_plane_spawned_) {
                ISphere isphere = hit.transform.GetComponent<ISphere>();

                if (isphere != null) {
                    if (Input.GetMouseButtonDown(0) == true) {
                        selected_atom_ = isphere;
                        SpawnSelectionPlaneSpheres();
                        return;
                    }

                    info_ui_.SetAtom(isphere);
                    if (exploring_method_ == ExploringMethod.RESIDUES && !highlighted_spheres_.Contains(isphere)) {
                        ClearHighlighted();
                        HighLightResidue(isphere);
                    }
                    else if (exploring_method_ == ExploringMethod.CHAINS) {
                        if (!highlighted_spheres_.Contains(isphere)) {
                            ClearHighlighted();
                            HighLightResidue(isphere);
                        }

                        if (!colored_spheres_.Contains(isphere)) {
                            ClearColored();
                            ColorChain(isphere);
                        }
                    }
                }

            }
            else {
                ClearHighlighted();
                if (exploring_method_ != ExploringMethod.CHAINS) ClearColored();
            }


        } /* End of torsion angle state */


    }

    private void RayCastAtoms() {

    }

    private void SpawnTorsionAngle() {
        /* Use the selected atoms to spawn the torsion angle
         * Spawn in the origin, since TorsionAngle will calcualte the atom planes in world space coordinates
         */
        torsion_angle_previous_ = Instantiate(prefab_torsion_angle, new Vector3(0, 0, 0), Quaternion.identity);
        torsion_angle_previous_.transform.parent = transform;

        TorsionAngle tangle = torsion_angle_previous_.GetComponent<TorsionAngle>();
        tangle.pos1_ = atoms_selected_[atom_selected_id_ % 4].transform.position;
        tangle.pos2_ = atoms_selected_[(atom_selected_id_ + 1) % 4].transform.position;
        tangle.pos3_ = atoms_selected_[(atom_selected_id_ + 2) % 4].transform.position;
        tangle.pos4_ = atoms_selected_[(atom_selected_id_ + 3) % 4].transform.position;
    }

    /* Ray cast in the UI layer only */
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

    /* Process the ray cast hit, set the states in the virtual button */
    private void ProcessRayCastUIHit(RaycastHit hit) {
        ButtonEvent button = hit.transform.GetComponent<ButtonEvent>();
        if (button == null) return;

        button.RayCastHover();
        if (Input.GetMouseButtonDown(0) == true) {
            button.RayCastHit();
        }
    }

    /* Spawn a selection sphere for 2D navigation for the currently selected atom */
    private void SpawnSelectionPlaneSpheres() {
        if (selection_plane_previous_ != null) Destroy(selection_plane_previous_);

        selection_plane_previous_ = Instantiate(prefab_selection_plane_spheres_, selected_atom_.transform.position, Quaternion.identity);
        selection_plane_previous_.transform.parent = transform;
        SelectionPlaneSpheres plane = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();
        plane.visualization = selection_visualization_;
        plane.center_sphere_ = selected_atom_;

        /* Get and set the spheres within the selection radius */
        ClearHighlighted();
        Collider[] hitColliders = Physics.OverlapSphere(selected_atom_.transform.position, SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ISphere s = c.gameObject.GetComponent<ISphere>();
            if (s == null || s == selected_atom_) continue;

            plane.AddSphere(s);
        }
    }

    /* Spawn a selection sphere for 2D navigation for the currently selected bond */
    private void SpawnSelectionPlaneCylinders() {
        if (selection_plane_previous_ != null) Destroy(selection_plane_previous_);

        selection_plane_previous_ = Instantiate(prefab_selection_plane_cylinders_, SelectionPlaneCylinders.CalculateCylinderMiddle(selected_bond_), Quaternion.identity);
        selection_plane_previous_.transform.parent = transform;
        SelectionPlaneCylinders plane = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>();
        plane.visualization = selection_visualization_;
        plane.center_cylinder_ = selected_bond_;

        /* Get and set the bonds within the selection radius */
        ClearHighlighted();
        Collider[] hitColliders = Physics.OverlapSphere(selected_bond_.transform.position, SELECTION_MODE_SPHERE_RADIUS);
        foreach (Collider c in hitColliders) {
            ICylinder s = c.gameObject.GetComponent<ICylinder>();
            if (s == null || s == selected_bond_) continue;

            plane.AddCylinder(s);
        }
    }

    /* Gradually move the model towards or away from the user, given a selected object, atom or bond */
    private void MoveTowardsSelectedObject(float speed) {
        if (selection_plane_previous_ == null) return;
        /* Calculate desired position of the selected object, just in front of the camera */
        SelectionPlaneSpheres plane1 = selection_plane_previous_.GetComponent<SelectionPlaneSpheres>();
        SelectionPlaneCylinders plane2 = selection_plane_previous_.GetComponent<SelectionPlaneCylinders>();

        /* Calculate the desired distance that we want the selected object to be in front of the user */
        float distance = 0;
        Vector3 target;
        if (plane1 != null) {
            target = plane1.center_sphere_.transform.position;
            if (GetVisualizationMethod() == VisualizationMethod.BALL_AND_STICK) {
                distance = 6 * AtomicRadii.ball_and_stick_radius;
            } else {
                distance= 4 * AtomicRadii.GetCovalentRadius(selected_atom_.atom_.element_);
            }
        } else if (plane2 != null) {
            target = plane2.center_cylinder_.transform.position + plane2.center_cylinder_.transform.up * plane2.center_cylinder_.height_ / 2;
            distance = plane2.center_cylinder_.height_;
        } else {
            return;
        }
        Vector3 movement_direction = Vector3.Normalize(target - Camera.main.transform.position);

        Vector3 desired_position = target - movement_direction * distance;
        /* Calculate the movement speed, scale it based on the distance to the object */
        speed = speed * Vector3.Distance(desired_position, Camera.main.transform.position);

        /* Change position */
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

    /* Set a random color to a sphere, currently not used */
    private void SetRandomColor(ISphere isphere)
    {
        Color rcolor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        foreach (ISphere s in atoms_dictionary[isphere.atom_.element_])
        {
            s.SetColor(rcolor);
        }
    }

    /* Highlight a residue */
    private void HighLightResidue(ISphere isphere)
    {
        ClearHighlighted();

        int residue_key = CalculateUniqueResidueIdentifier(isphere);
        foreach (ISphere s in residue_dictionary[residue_key])
        {
            s.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.WHITE);
            highlighted_spheres_.Add(s);
        }
    }

    /* Color a chain */
    private void ColorChain(ISphere isphere) {
        foreach (ISphere s in chains_dictionary[isphere.atom_.chain_id_]) {
            s.FixColor(new Color(0, 0.8f, 0));
            colored_spheres_.Add(s);
        }
    }

    /* Clear the highlighted objects */
    private void ClearHighlighted()
    {
        foreach (ISphere s in highlighted_spheres_)
        {
            s.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
        }
        highlighted_spheres_.Clear();

        if (previously_highlighted_bond_ != null) previously_highlighted_bond_.SetHighlighted(HighlightColors.HIGHLIGHT_COLOR.NO_HIGHLIGHT);
        previously_highlighted_bond_ = null;
    }

    /* Clear the colored objects */
    private void ClearColored() {
        foreach (ISphere s in colored_spheres_) {
            s.UnfixColor();
            s.SetCPKColor();
        }
        colored_spheres_.Clear();
    }

    /* Change the ambient occlusion intensity factor, currently not used */
    private void ChangeAmbientOcclusionFactor() {
        PostProcessVolume volume = Camera.main.transform.GetComponent<PostProcessVolume>();
        AmbientOcclusion ao_effect = null;
        volume.profile.TryGetSettings(out ao_effect);

        if (visualization_method_ == VisualizationMethod.BALL_AND_STICK) {
            ao_effect.intensity.value = 1;
        }
        else {
            ao_effect.intensity.value = 0.5f;
        }
    }

}
