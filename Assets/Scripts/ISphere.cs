using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISphere : MonoBehaviour
{
    public Atom atom_;
    public List<ISphere> connections_ = new List<ISphere>();

    private MaterialBlock material_block_;

    private void Awake()
    {
    }

    private void Start()
    {
        material_block_ = GetComponent<MaterialBlock>();
        SetCPKColor();
        SetAtomicRadius();
    }

    public void SetColor(Color color) {
        material_block_.SetColor(color);
    }

    public void SetHighlighted(bool is_highlighted)
    {
        material_block_.SetHighlighted(is_highlighted);
    }

    public void SetRadius(float radius)
    {
        /* Set sphere collider radius for ray casting */
        float scaling = transform.localScale.x;
        GetComponent<SphereCollider>().radius = radius / scaling;

        /* Set material radius */
        material_block_.SetRadius(radius);
    }

    public void SetAtomicRadius()
    {
        try
        {
            float radius = AtomicRadii.radii_covalent[atom_.element_];
            SetRadius(radius);
        }
        catch
        {
            print("Atom: " + atom_.serial_ + ", Element: " + atom_.element_ + " does not have atomic radius set");
        }
    }
    
    public void SetCPKColor()
    {
        if (atom_ == null) return;

        if (atom_.element_.Equals("H")) {
            SetColor(CPKColors.color_hydrogen_);
        } else if (atom_.element_.Equals("C")) {
            SetColor(CPKColors.color_carbon_);
        } else if (atom_.element_.Equals("N")) {
            SetColor(CPKColors.color_nitrogen_);
        } else if (atom_.element_.Equals("O")) {
            SetColor(CPKColors.color_oxygen_);
        } else if (atom_.element_.Equals("F") || atom_.element_.Equals("CL")) {
            SetColor(CPKColors.color_fclrine_);
        } else if (atom_.element_.Equals("BR")) {
            SetColor(CPKColors.color_bromine_);
        } else if (atom_.element_.Equals("I")) {
            SetColor(CPKColors.color_iodine_);
        } else if (atom_.element_.Equals("HE") || atom_.element_.Equals("NE") || atom_.element_.Equals("AR") || atom_.element_.Equals("XE") || atom_.element_.Equals("KR")) {
            SetColor(CPKColors.color_noble_gases_);
        } else if (atom_.element_.Equals("P")) {
            SetColor(CPKColors.color_phosphorus_);
        } else if (atom_.element_.Equals("S")) {
            SetColor(CPKColors.color_sulfur_);
        } else if (atom_.element_.Equals("B")) {
            SetColor(CPKColors.color_boron_);
        } else if (atom_.element_.Equals("LI") || atom_.element_.Equals("NA") || atom_.element_.Equals("K") || atom_.element_.Equals("RB") || atom_.element_.Equals("CS") || atom_.element_.Equals("FR")) {
            SetColor(CPKColors.color_alkali_metals_);
        } else if (atom_.element_.Equals("BE") || atom_.element_.Equals("MG") || atom_.element_.Equals("CA") || atom_.element_.Equals("SR") || atom_.element_.Equals("BA") || atom_.element_.Equals("RA")) {
            SetColor(CPKColors.color_alkaline_earth_metals_);
        } else if (atom_.element_.Equals("TI")) {
            SetColor(CPKColors.color_titanium_);
        } else if (atom_.element_.Equals("FE")) {
            SetColor(CPKColors.color_iron_);
        } else {
            SetColor(CPKColors.color_other);
        }
    }

}
