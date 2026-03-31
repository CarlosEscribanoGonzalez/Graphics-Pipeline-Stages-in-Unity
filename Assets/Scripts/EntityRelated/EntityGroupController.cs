using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EntityGenerator))]
public class EntityGroupController : MonoBehaviour
{
    [SerializeField] private float animSpeedMult = 5f; //Multiplicador velocidad animación coleteo
    [SerializeField] private float flipThreshold = 0.05f;
    [Header("Own movement")]
    [SerializeField] private Vector2 speedRange = new(1, 10); //Rango de velocidades
    [SerializeField] private Vector2 restTimeRange = new(0.5f, 3); //Tiempo de descanso al llegar al destino
    private Mesh mesh;
    private EntityGenerator generator;
    private Vector3[] positions;
    private GraphicsBuffer entityDataBuffer;
    private EntityData[] data;
    private Material material;
    private Transform cam;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        generator = GetComponent<EntityGenerator>();
        material = GetComponent<MeshRenderer>().material;
        positions = new Vector3[mesh.vertices.Length];
        data = new EntityData[mesh.vertices.Length];
        cam = Camera.main.transform;
        int stride = sizeof(int) + sizeof(float) * 2;
        entityDataBuffer = new(GraphicsBuffer.Target.Structured, mesh.vertices.Length, stride);
        material.SetBuffer("entityData", entityDataBuffer);
        //Si hay Compute Movement el movimiento lo controla ese script, de lo contrario lo hace este
        if (!TryGetComponent(out AComputeMovement _))
        {
            for (int i = 0; i < mesh.vertices.Length; i++)
                StartCoroutine(UpdateEntityCoroutine(i));
        }
    }

    private void OnDestroy() => entityDataBuffer?.Dispose();

    private void LateUpdate()
    {
        mesh.vertices = positions;
        entityDataBuffer.SetData(data);
    }

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
            data[i].speed = velocities[i].magnitude * animSpeedMult;
        }
    }

    IEnumerator UpdateEntityCoroutine(int fishIdx)
    {
        positions[fishIdx] = mesh.vertices[fishIdx];
        while (true)
        {
            data[fishIdx].speed = 0;
            yield return new WaitForSeconds(Utils.RandomInRange(restTimeRange));
            Vector3 destination = generator.GetPointInDomain();
            float speed = Utils.RandomInRange(speedRange);
            data[fishIdx].speed = speed * animSpeedMult;
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
}

public struct EntityData
{
    public int flip;
    public float rotation;
    public float speed;
}