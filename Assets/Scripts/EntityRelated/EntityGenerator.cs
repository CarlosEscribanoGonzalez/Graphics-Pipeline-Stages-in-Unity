using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    [SerializeField] private int numEntities;
    [SerializeField] private Vector2 bounds_x = new(-10, 10);
    [SerializeField] private Vector2 bounds_y = new(-10, 10);
    [SerializeField] private Vector2 bounds_z = new(-10, 10);
    [SerializeField] private bool initRandomPositions = true;
    private MeshFilter meshFilter;
    public int NumEntities => numEntities;
    public Vector2 Bounds_X => bounds_x;
    public Vector2 Bounds_Y => bounds_y;
    public Vector2 Bounds_Z => bounds_z;

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
        meshFilter.mesh = pointMesh;
    }

    public Vector3 GetPointInDomain()
    {
        float pos_x = Utils.RandomInRange(bounds_x);
        float pos_y = Utils.RandomInRange(bounds_y);
        float pos_z = Utils.RandomInRange(bounds_z);
        return new(pos_x, pos_y, pos_z);
    }

    public Vector3[] GetPositions()
    {
        return meshFilter.mesh.vertices;
    }

    private Vector3[] GenerateVertices()
    {
        List<Vector3> entityPositions = new();
        for (int i = 0; i < numEntities; i++)
        {
            if (initRandomPositions) entityPositions.Add(GetPointInDomain());
            else entityPositions.Add(Vector3.zero);
        }
        return entityPositions.ToArray();
    }

    private int[] GenerateTriangles() //Si no son triángulos no se pintan
    {
        List<int> triangles = new();
        for (int i = 0; i < numEntities; i++)
            for (int j = 0; j < 3; j++) triangles.Add(i);
        return triangles.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new(
            (bounds_x.x + bounds_x.y) / 2f,
            (bounds_y.x + bounds_y.y) / 2f,
            (bounds_z.x + bounds_z.y) / 2f
        );
        Vector3 size = new(
            bounds_x.y - bounds_x.x,
            bounds_y.y - bounds_y.x,
            bounds_z.y - bounds_z.x
        );
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(center, size);
    }
}
