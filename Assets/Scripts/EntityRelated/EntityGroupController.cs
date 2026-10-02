using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EntityGenerator))]
public class EntityGroupController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float animSpeedMult = 5f;
    [SerializeField] private float maxAnimSpeed = 5f;
    [SerializeField] private float flipThreshold = 0.05f;
    [Header("Own movement")]
    [SerializeField] private Vector2 speedRange = new(1, 10);
    [SerializeField] private Vector2 restTimeRange = new(0.5f, 3); //Rest time once destination is reached
    private Mesh mesh;
    private EntityGenerator generator;
    private Vector3[] positions;
    private GraphicsBuffer entityDataBuffer;
    private EntityData[] data;
    private Material material;
    private Transform cam;
    private int N => mesh.vertices.Length;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        generator = GetComponent<EntityGenerator>();
        material = GetComponent<MeshRenderer>().material;
        cam = Camera.main.transform;
        positions = new Vector3[N];
        data = new EntityData[N];
        InitSizes();
        int stride = sizeof(int) + sizeof(float) * 4;
        entityDataBuffer = new(GraphicsBuffer.Target.Structured, N, stride);
        material.SetBuffer("entityData", entityDataBuffer);
        //This script only controls movement if there is no AComputeMovement script attached
        if (!TryGetComponent(out AComputeMovement _))
        {
            for (int i = 0; i < N; i++)
                StartCoroutine(UpdateEntityCoroutine(i));
        }
    }

    private void OnDestroy() => entityDataBuffer?.Dispose();

    private void LateUpdate()
    {
        mesh.vertices = positions;
        entityDataBuffer.SetData(data);
    }

    //Used by AComputeMovement to update the info
    public void UpdateInfo(Vector3[] positions, Vector3[] velocities)
    {
        this.positions = positions;
        for(int i = 0; i < data.Length; i++)
        {
            Vector3 dir = velocities[i].normalized;
            float dot = Vector3.Dot(dir, cam.right);
            bool flip = Mathf.Abs(dot) > flipThreshold ? dot > 0 : data[i].flip == 1;
            data[i].flip = flip ? 1 : 0;
            data[i].rotation = (flip ? -1 : 1) * Mathf.Rad2Deg * Mathf.Atan(dir.y * 2);
            data[i].speed = Mathf.Min(velocities[i].magnitude, maxAnimSpeed);
        }
    }

    private void InitSizes()
    {
        (float[] sizesX, float[] sizesY) = generator.GetSizes();
        for(int i = 0; i < N; i++)
        {
            data[i].sizeX = sizesX[i];
            data[i].sizeY = sizesY[i];
        }
    }

    //Basic movement coroutine (in CPU)
    IEnumerator UpdateEntityCoroutine(int fishIdx)
    {
        positions[fishIdx] = mesh.vertices[fishIdx];
        while (true)
        {
            data[fishIdx].speed = 0;
            yield return new WaitForSeconds(Utils.RandomInRange(restTimeRange));
            Vector3 destination = generator.GetPointInDomain();
            float speed = Utils.RandomInRange(speedRange);
            data[fishIdx].speed = Mathf.Min(speed * animSpeedMult, maxAnimSpeed);
            while (positions[fishIdx] != destination)
            {
                Vector3 dir = (destination - positions[fishIdx]).normalized;
                float dot = Vector3.Dot(dir, cam.right);
                bool flip = Mathf.Abs(dot) > flipThreshold ? dot > 0 : data[fishIdx].flip == 1;
                data[fishIdx].flip = flip ? 1 : 0;
                data[fishIdx].rotation = (flip ? -1 : 1) * Mathf.Rad2Deg * Mathf.Atan(dir.y * 2);
                positions[fishIdx] = Vector3.MoveTowards(positions[fishIdx], destination, speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    struct EntityData
    {
        public int flip;
        public float rotation;
        public float speed;
        public float sizeX;
        public float sizeY;
    }
}