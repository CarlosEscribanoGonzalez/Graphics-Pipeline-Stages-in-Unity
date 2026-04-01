using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityGenerator))]
[RequireComponent(typeof(EntityGroupController))]
public class FlockMovement : AComputeMovement
{
    [SerializeField] private ComputeShader cshader;
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
    private int numBlocks;
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
        cshader = Instantiate(cshader);
        generator = GetComponent<EntityGenerator>();
        controller = GetComponent<EntityGroupController>();
        kernel = cshader.FindKernel("CSMain");
        numBlocks = Mathf.CeilToInt(N / 256f);
        InitShaderParams();
        InitBuffers();
        positions = generator.GetPositions();
        velocities = new Vector3[N];
        for (int i = 0; i < N; i++) velocities[i] = Random.insideUnitSphere / 100; //Velocidades aleatorias
        StartCoroutine(ChangeTargetCoroutine());
    }

    private void OnDestroy() => ReleaseBuffers();

    private void Update()
    {
        cshader.SetFloat("deltaTime", Time.deltaTime);
        posBuffer.SetData(positions);
        velBuffer.SetData(velocities);
        cshader.Dispatch(kernel, numBlocks, 1, 1);
        newPosBuffer.GetData(positions);
        newVelBuffer.GetData(velocities);
        controller.UpdateInfo(positions, velocities);
    }

    protected override void InitShaderParams()
    {
        cshader.SetInt("entityCount", N);
        cshader.SetFloat("neighborRadius", neighborRadius);
        cshader.SetFloat("separationRadius", separationRadius);
        cshader.SetFloat("maxVel", maxVel);
        cshader.SetFloat("maxForce", maxForce);
        cshader.SetFloat("weightSeparation", weightSeparation);
        cshader.SetFloat("weightAlignment", weightAlignment);
        cshader.SetFloat("weightCohesion", weightCohesion);
        cshader.SetFloat("weightTarget", weightTarget);
    }

    protected override void InitBuffers()
    {
        posBuffer = new(N, sizeof(float) * 3);
        newPosBuffer = new(N, sizeof(float) * 3);
        velBuffer = new(N, sizeof(float) * 3);
        newVelBuffer = new(N, sizeof(float) * 3);
        cshader.SetBuffer(kernel, "positions", posBuffer);
        cshader.SetBuffer(kernel, "newPositions", newPosBuffer);
        cshader.SetBuffer(kernel, "velocities", velBuffer);
        cshader.SetBuffer(kernel, "newVelocities", newVelBuffer);
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
            cshader.SetVector("targetPos", targetPos);
            yield return new WaitForSeconds(Utils.RandomInRange(targetChangeInterval));
        }
    }
}