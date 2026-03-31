using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityGenerator))]
[RequireComponent(typeof(EntityGroupController))]
public class FlockMovement : AComputeMovement
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private Vector2 targetChangeInterval = new(2, 5);
    [Header("Flock params:")]
    [SerializeField] private float neighborRadius = 3f;
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private float maxVel = 10f;
    [SerializeField] private float maxForce = 5f;
    [SerializeField] private float weightSeparation = 1f;
    [SerializeField] private float weightAlignment = 1f;
    [SerializeField] private float weightCohesion = 1f;
    [SerializeField] private float weightTarget = 1f;
    private int kernel;
    private int groups;
    private Vector3[] positions;
    private Vector3[] velocities;
    private EntityGenerator generator;
    private EntityGroupController controller;
    //Buffers:
    private ComputeBuffer posBuffer;
    private ComputeBuffer newPosBuffer;
    private ComputeBuffer velBuffer;
    private ComputeBuffer newVelBuffer;
    private int N => generator.NumEntities;

    private void Start()
    {
        shader = Instantiate(shader);
        generator = GetComponent<EntityGenerator>();
        controller = GetComponent<EntityGroupController>();
        kernel = shader.FindKernel("CSMain");
        groups = Mathf.CeilToInt(N / 256f);
        InitShaderParams();
        InitBuffers();
        positions = generator.GetPositions();
        velocities = new Vector3[N];
        for (int i = 0; i < N; i++) velocities[i] = Random.insideUnitSphere / 10;
        StartCoroutine(ChangeTargetCoroutine());
    }

    private void OnDestroy() => ReleaseBuffers();

    private void Update()
    {
        shader.SetFloat("deltaTime", Time.deltaTime);
        posBuffer.SetData(positions);
        velBuffer.SetData(velocities);
        shader.Dispatch(kernel, groups, 1, 1);
        newPosBuffer.GetData(positions);
        newVelBuffer.GetData(velocities);
        controller.UpdateInfo(positions, velocities);
    }

    protected override void InitShaderParams()
    {
        shader.SetInt("entityCount", N);
        shader.SetFloat("neighborRadius", neighborRadius);
        shader.SetFloat("separationRadius", separationRadius);
        shader.SetFloat("maxVel", maxVel);
        shader.SetFloat("maxForce", maxForce);
        shader.SetFloat("weightSeparation", weightSeparation);
        shader.SetFloat("weightAlignment", weightAlignment);
        shader.SetFloat("weightCohesion", weightCohesion);
        shader.SetFloat("weightTarget", weightTarget);
    }

    protected override void InitBuffers()
    {
        posBuffer = new(N, sizeof(float) * 3);
        newPosBuffer = new(N, sizeof(float) * 3);
        velBuffer = new(N, sizeof(float) * 3);
        newVelBuffer = new(N, sizeof(float) * 3);
        shader.SetBuffer(kernel, "positions", posBuffer);
        shader.SetBuffer(kernel, "newPositions", newPosBuffer);
        shader.SetBuffer(kernel, "velocities", velBuffer);
        shader.SetBuffer(kernel, "newVelocities", newVelBuffer);
    }

    protected override void ReleaseBuffers()
    {
        posBuffer.Release();
        newPosBuffer.Release();
        velBuffer.Release();
        newVelBuffer.Release();
    }

    IEnumerator ChangeTargetCoroutine()
    {
        while (true)
        {
            Vector3 targetPos = generator.GetPointInDomain();
            shader.SetVector("targetPos", targetPos);
            yield return new WaitForSeconds(Utils.RandomInRange(targetChangeInterval));
        }
    }
}