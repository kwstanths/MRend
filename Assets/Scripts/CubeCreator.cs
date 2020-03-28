using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCreator : MonoBehaviour
{

    public void Start()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(-0.500000f, 0.000000f, 0.500000f),
            new Vector3(-0.500000f, 1.000000f, 0.500000f),
            new Vector3(-0.500000f, 0.000000f, -0.500000f),
            new Vector3(-0.500000f, 1.000000f, -0.500000f),
            new Vector3(0.500000f, 0.000000f, 0.500000f),
            new Vector3(0.500000f, 1.000000f, 0.500000f),
            new Vector3(0.500000f, 0.000000f, -0.500000f),
            new Vector3(0.500000f, 1.000000f, -0.500000f),
        };
        mesh.vertices = vertices;

        int[] tris = new int[]
        {
            1,2,0,
            3,6,2,
            7,4,6,
            5,0,4,
            6,0,2,
            3,5,7,
            1,3,2,
            3,7,6,
            7,5,4,
            5,1,0,
            6,4,0,
            3,1,5
        };
        mesh.triangles = tris;

        meshFilter.mesh = mesh;
    }
}
