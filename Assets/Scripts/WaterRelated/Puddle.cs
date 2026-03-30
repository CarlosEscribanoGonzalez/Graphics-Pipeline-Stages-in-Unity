using System.Collections.Generic;
using UnityEngine;

public class FluidSurface : MonoBehaviour
{
    [SerializeField] private int numCells = 60; 
    [SerializeField] private float timeStep = 0.01f;
    [SerializeField] private float propagationSpeed = 0.04f; 
    [SerializeField] private float damping = 0.001f; 
    [SerializeField] private float initHeight = 50;
    [SerializeField] private float sigma = 0.01f;
    private float width; 
    private float height; 
    private MeshFilter filter;

    struct Node
    {
        public float y;
        public float newY; 
        public float vel;
        public float a;
    };
    private Node[] vertices;

    void Awake()
    {
        filter = GetComponent<MeshFilter>();
        Vector3 worldSize = filter.mesh.bounds.size; //Permite poner un plano para previsualizar el área
        width = worldSize.x;
        height = worldSize.z;
        filter.mesh = GenerateMesh();
        initHeight /= ((transform.lossyScale.x + transform.lossyScale.z) / 2);
        GetComponent<MeshCollider>().sharedMesh = filter.mesh;
        SetupInitialSimulationState();
    }

    void Update()
    {
        Mesh mesh = filter.mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            vertex.y = this.vertices[i].y;
            vertices[i] = vertex;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void FixedUpdate()
    {
        float DELTA_X = width / numCells;
        float DELTA_X2 = DELTA_X * DELTA_X;
        float DELTA_Z = height / numCells;
        float DELTA_Z2 = DELTA_Z * DELTA_Z;
        float C2 = propagationSpeed * propagationSpeed;
        Node[] v = vertices;
        for (int z = 1; z < numCells; ++z)
        {
            for (int x = 1; x < numCells; ++x)
            {
                int i = Idx(x, z);
                int iPrevX = Idx(x - 1, z);
                int iNextX = Idx(x + 1, z);
                int iPrevZ = Idx(x, z - 1);
                int iNextZ = Idx(x, z + 1);
                float d2x = (v[iNextX].y - 2 * v[i].y + v[iPrevX].y) / DELTA_X2;
                float d2z = (v[iNextZ].y - 2 * v[i].y + v[iPrevZ].y) / DELTA_Z2;
                v[i].a = C2 * (d2x + d2z);
                v[i].a += -damping * v[i].vel;
                v[i].vel += timeStep * v[i].a;
                v[i].newY = v[i].y + timeStep * v[i].vel;
            }
        }
        for (int z = 1; z < numCells; ++z)
        {
            for (int x = 1; x < numCells; ++x)
            {
                int i = Idx(x, z);
                v[i].y = v[i].newY;
            }
        }
        vertices = v;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out FootstepManager footstepManager))
            footstepManager.OnStepPerformed += HandleStep;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out FootstepManager footstepManager))
            footstepManager.OnStepPerformed -= HandleStep;
    }

    private void HandleStep(Vector3 pos) => AddWave(pos);

    private Mesh GenerateMesh()
    {
        Mesh mesh = new();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        for (int x = 0; x < numCells + 1; ++x)
        {
            for (int z = 0; z < numCells + 1; ++z)
            {
                vertices.Add(new Vector3(-width * 0.5f + width * (x / ((float)numCells)), 0, -height * 0.5f + height * (z / ((float)numCells))));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)numCells, z / (float)numCells));
            }
        }
        var triangles = new List<int>();
        var vertCount = numCells + 1;
        for (int i = 0; i < vertCount * vertCount - vertCount; ++i)
        {
            if ((i + 1) % vertCount == 0)
            {
                continue;
            }
            triangles.AddRange(new List<int>()
            {
                i + 1 + vertCount, i + vertCount, i,
                i, i + 1, i + vertCount + 1
            });
        }
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        return mesh;
    }

    private void SetupInitialSimulationState()
    {
        Vector3[] vertices = filter.mesh.vertices;
        this.vertices = new Node[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            this.vertices[i].y = 0;
            this.vertices[i].vel = 0;
            this.vertices[i].a = 0;
        }
    }

    void AddWave(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Vector2 offset = new(localPos.x, localPos.z);
        Mesh mesh = filter.mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            if (vertex.x <= -width / 2.0001f || vertex.x >= width / 2.0001f || vertex.z <= -height / 2.0001f || vertex.z >= height / 2.0001f)
            {
                this.vertices[i].y = 0;
                continue;
            }
            vertex.x -= offset.x;
            vertex.z -= offset.y;
            this.vertices[i].y += initHeight * Mathf.Exp(-sigma * vertex.x * vertex.x) * Mathf.Exp(-sigma * vertex.z * vertex.z);
        }
    }

    int Idx(int x, int z) { return x + (numCells + 1) * z; }
}