using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/* 
 * A script attached on an impostor sphere that holds the atom information, radius, color, etc.
 */
public class ISphere : MonoBehaviour
{
    /* Holds the atom information associated with this impostor sphere */
    public Atom atom_;
    /* A list of spheres mentioned as connections in the PDB file */
    public List<ISphere> connections_ = new List<ISphere>();

    /* A reference to the material property block of that impostor sphere */
    private MaterialBlockSphere material_block_;

    private bool fixed_color_ = false;
    private Color fixed_color_value_;

    private void Start()
    {
        /* Init material property block, color and radius */
        material_block_ = GetComponent<MaterialBlockSphere>();
        SetCPKColor();

        if (GetComponentInParent<Atoms>().GetVisualizationMethod() == Atoms.VisualizationMethod.SPACE_FILLING) {
            SetAtomicRadius();
        } else {
            SetRadius(AtomicRadii.ball_and_stick_radius);
        }
    }

    public void SetColor(Color color) {
        material_block_.SetColor(color);
    }

    public void FixColor(Color color) {
        fixed_color_ = true;
        fixed_color_value_ = color;
        material_block_.SetColor(color);
    }

    public void UnfixColor() {
        fixed_color_ = false;
    }

    public void ResetColor() {
        if (fixed_color_) material_block_.SetColor(fixed_color_value_);
        else SetCPKColor();

    }

    public bool IsColorFixed() {
        return fixed_color_;
    }

    public void SetHighlighted(HighlightColors.HIGHLIGHT_COLOR color)
    {
        material_block_.SetHighlighted(HighlightColors.GetColorValue(color));
    }

    public void SetRadius(float radius)
    {
        /* Set sphere collider radius for ray casting */
        float scaling = transform.localScale.x;
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider != null) collider.radius = radius / scaling;

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
            return;
        }
    }
    
    public void SetCPKColor()
    {
        if (atom_ == null) return;

        SetColor(CPKColors.GetCPKColor(atom_.element_));
    }

    public void SetTransparent(bool transparent) {
        /* Set the rendering queue to either transparent or opaque geometry */
        RenderQueue queue;
        if (transparent) queue = RenderQueue.Transparent;
        else queue = RenderQueue.Geometry;

        /* This is going to break instancing for that sphere */
        Material m = GetComponent<MeshRenderer>().material;
        m.renderQueue = (int)queue + 1;
    }

}
