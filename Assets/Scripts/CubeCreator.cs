using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
    A class the creates a cube mesh that sits on top of the xz plane, aimed to be used by Cylinder impostors
    In order to support instancing, all cylinders must use the same Mesh object, and thus, this is a Singleton class
*/
public class CubeCreator
{
    /* Singleton instance */
    private static CubeCreator instance_;
    /* Single mesh object */
    private Mesh mesh_;

    private CubeCreator()
    {
        SpawnMesh();
    }

    public static CubeCreator Instance
    {
        get {
            if (instance_ == null)
            {
                instance_ = new CubeCreator();
            }
            return instance_;
        }
    }

    public Mesh GetCubeMesh()
    {
        return mesh_;
    }

    public void SpawnMesh()
    {
        /* Spawn the mesh */
        mesh_ = new Mesh();

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
        mesh_.vertices = vertices;

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
        mesh_.triangles = tris;
    }
}
