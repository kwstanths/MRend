using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ISphere : MonoBehaviour
{
    public Atom atom_;
    public List<ISphere> connections_ = new List<ISphere>();

    private MaterialBlockSphere material_block_;

    private void Awake()
    {
    }

    private void Start()
    {
        //RenderQueue queue = RenderQueue.Geometry;

        //Material m = GetComponent<MeshRenderer>().material;
        //m.renderQueue = (int)queue;
        //m.SetOverrideTag("RenderType", "Opaque");

        material_block_ = GetComponent<MaterialBlockSphere>();
        SetCPKColor();

        if (GetComponentInParent<Atoms>().GetVisualizationMethod() == VisualizationMethod.SPACE_FILLING)
        {
            SetAtomicRadius();
        }
        else
        {
            SetRadius(AtomicRadii.ball_and_stick_radius);
        }
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

    public void SetAmbient(float ambient) {
        /* Set material radius */
        material_block_.SetAmbientComponent(ambient);
    }

    public void SetAtomicRadius()
    {
        try
        {
            float radius = AtomicRadii.GetCovalentRadius(atom_.element_);
            SetRadius(radius);
        }
        catch
        {
            
        }
    }
    
    public void SetCPKColor()
    {
        if (atom_ == null) return;

        SetColor(CPKColors.GetCPKColor(atom_.element_));
    }

}
