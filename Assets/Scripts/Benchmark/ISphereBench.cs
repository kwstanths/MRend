using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISphereBench : MonoBehaviour
{
    private void Start() {
        MaterialBlockSphere material_block_ = GetComponent<MaterialBlockSphere>();
        material_block_.SetRadius(AtomicRadii.ball_and_stick_radius);
    }

}
