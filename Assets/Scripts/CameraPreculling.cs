using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * This class is used to change the camera's field of view when performing frustum culling so that
 * quad that are not oriented in world space are not culled by camera's side frustum planes
 */
public class CameraPreculling : MonoBehaviour
{
    /* Angle to add to the camera's field of view for culling */
    [SerializeField] float fov_addition = 5.0f;

    /* Holds an instance of the camera */
    private Camera cam;
    /* Holds camera's currently set field of view */
    float cam_old_fov;

    // Start is called before the first frame update
    void Start()
    {
        /* Grab components */
        cam = GetComponent<Camera>();
        cam_old_fov = cam.fieldOfView;
    }

    private void OnPreCull()
    {
        /* Change the field of view used for culling */
        cam.fieldOfView = cam_old_fov + fov_addition;
    }

    private void OnPreRender()
    {
        /* Set back old field of view for rendering */
        cam.fieldOfView = cam_old_fov;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
