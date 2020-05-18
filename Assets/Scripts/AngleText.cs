using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleText : MonoBehaviour
{
    public string angle_degrees_;

    // Start is called before the first frame update
    void Start()
    {
        /* Set the text */
        TextMesh temp = GetComponent<TextMesh>();
        temp.text = angle_degrees_;
    }

    // Update is called once per frame
    void Update()
    {
        /* Make the text always face the camera */
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);
    }
}
