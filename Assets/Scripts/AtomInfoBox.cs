using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AtomInfoBox : MonoBehaviour
{
    Canvas canvas_;

    Text text_element_;
    Text text_residue_;
    Text text_atom_name_;
    Text text_chain_;
    Text text_occupancy_;
    Text text_temp_factor_;
    Text text_torsion_1_;
    Text text_torsion_2_;
    Text text_torsion_3_;
    Text text_torsion_4_;

    // Start is called before the first frame update
    void Start()
    {
        canvas_ = GetComponentInChildren<Canvas>();
        text_element_ = canvas_.transform.Find("Element").GetComponent<Text>();
        text_residue_ = canvas_.transform.Find("Residue").GetComponent<Text>();
        text_atom_name_ = canvas_.transform.Find("AtomName").GetComponent<Text>();
        text_occupancy_ = canvas_.transform.Find("Occupancy").GetComponent<Text>();
        text_temp_factor_ = canvas_.transform.Find("TempFactor").GetComponent<Text>();
        text_chain_ = canvas_.transform.Find("Chain").GetComponent<Text>();

        text_torsion_1_ = canvas_.transform.Find("torsion_atom_1").GetComponent<Text>();
        text_torsion_2_ = canvas_.transform.Find("torsion_atom_2").GetComponent<Text>();
        text_torsion_3_ = canvas_.transform.Find("torsion_atom_3").GetComponent<Text>();
        text_torsion_4_ = canvas_.transform.Find("torsion_atom_4").GetComponent<Text>();

        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAtom(ISphere s) {
        SetElement(s.atom_.element_);
        SetResiude(s.atom_.res_name_);
        SetAtomName(s.atom_.name_);
        SetChain(s.atom_.chain_id_.ToString());
        SetOccupancy(s.atom_.occupancy_.ToString("F2"));
        SetTempFactor(s.atom_.temp_factor_.ToString("F1"));
    }

    public void SetElement(string text) {
        string name = AtomNames.GetFullName(text);
        text_element_.text = name;
    }

    public void SetResiude(string text) {
        text_residue_.text = text;
    }

    public void SetAtomName(string text) {
        text_atom_name_.text = text;
    }

    public void SetChain(string text) {
        text_chain_.text = "Chain: " + text;
    }

    public void SetOccupancy(string text) {
        text_occupancy_.text = "Occupancy: " + text;
    }

    public void SetTempFactor(string text) {
        text_temp_factor_.text = "Temp. Factor: " + text;
    }

    public void ResetInfo() {
        text_element_.text = "";
        text_residue_.text = "";
        text_atom_name_.text = "";
        text_chain_.text = "Chain: ";
        text_occupancy_.text = "Occupancy: ";
        text_temp_factor_.text = "Temp. Factor: ";
        text_torsion_1_.text = "1: ";
        text_torsion_2_.text = "2: ";
        text_torsion_3_.text = "3: ";
        text_torsion_4_.text = "4: ";
    }

    public void SetTorsionAtom(int atom, string text) {
        switch (atom) {
            case 0:
                text_torsion_1_.text = "1: " + text;

                break;
            case 1:
                text_torsion_2_.text = "2: " + text;

                break;
            case 2:
                text_torsion_3_.text = "3: " + text;

                break;
            case 3:
                text_torsion_4_.text = "4: " + text;

                break;
            default:
                break;
        }
    }

    public void ClearTorsionAtoms() {
        text_torsion_1_.text = "";
        text_torsion_2_.text = "";
        text_torsion_3_.text = "";
        text_torsion_4_.text = "";
    }
}
