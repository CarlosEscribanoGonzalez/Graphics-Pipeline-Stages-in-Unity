using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    [SerializeField] private int numEntities;
    [SerializeField] private Vector2 bounds_x = new(-10, 10);
    [SerializeField] private Vector2 bounds_y = new(-10, 10);
    [SerializeField] private Vector2 bounds_z = new(-10, 10);
    private MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        Mesh pointMesh = new()
        {
            vertices = GenerateVertices(),
            triangles = GenerateTriangles(),
            bounds = new(Vector3.zero, new(bounds_x.y - bounds_x.x, bounds_y.y - bounds_y.x,
                bounds_z.y - bounds_z.x))
        };
        meshFilter.sharedMesh = pointMesh;
    }

    public Vector3 GetPointInDomain()
    {
        float pos_x = Utils.RandomInRange(bounds_x);
        float pos_y = Utils.RandomInRange(bounds_y);
        float pos_z = Utils.RandomInRange(bounds_z);
        return new(pos_x, pos_y, pos_z);
    }

    private Vector3[] GenerateVertices()
    {
        List<Vector3> fishPositions = new();
        for (int i = 0; i < numEntities; i++) 
            fishPositions.Add(GetPointInDomain());
        return fishPositions.ToArray();
    }

    private int[] GenerateTriangles() //Si no son triángulos no se pintan
    {
        List<int> triangles = new();
        for (int i = 0; i < numEntities; i++)
            for (int j = 0; j < 3; j++) triangles.Add(i);
        return triangles.ToArray();
    }
}
