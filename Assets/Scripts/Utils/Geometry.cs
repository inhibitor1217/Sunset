using UnityEngine;
using System.Collections.Generic;

public static class Geometry
{
    
    public static Mesh CreateCircleMesh(int resolution)
    {
        Mesh mesh = new Mesh();

        float angle = 2 * Mathf.PI / (float)resolution;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        vertices.Add(Vector3.zero);
        normals.Add(Vector3.back);
        for (int i = 0; i < resolution; i++)
        {
            vertices.Add(new Vector3(Mathf.Cos(angle * i), Mathf.Sin(angle * i), 0));
            normals.Add(Vector3.back);
            triangles.Add(0);
            triangles.Add((i + 1) % resolution + 1);
            triangles.Add(i % resolution + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }

}
