using UnityEngine;

[RequireComponent(typeof(EntityGenerator))]
[RequireComponent(typeof(EntityGroupController))]
public class WanderMovement : AComputeMovement
{
    [SerializeField] private ComputeShader cshader;
    [SerializeField] private Vector2 speedRange = new(1, 10);
    [SerializeField] private float distThreshold = 0.1f;
    [SerializeField] private Vector2 restRange = new(0.5f, 3);
    private int kernel;
    private int numBlocks;
    private Vector3[] positions;
    private Vector3[] velocities;
    private EntityGenerator generator;
    private EntityGroupController controller;
    //Buffers:
    private ComputeBuffer posBuffer;
    private ComputeBuffer targetPosBuffer;
    private ComputeBuffer velBuffer;
    private ComputeBuffer restBuffer;
    private int N => generator.NumEntities;

    private void Start()
    {
        cshader = Instantiate(cshader);
        generator = GetComponent<EntityGenerator>();
        controller = GetComponent<EntityGroupController>();
        kernel = cshader.FindKernel("CSMain");
        numBlocks = Mathf.CeilToInt(N / 256f);
        positions = generator.GetPositions();
        velocities = new Vector3[N];
        InitShaderParams();
        InitBuffers();
    }

    private void OnDestroy() => ReleaseBuffers();

    private void Update()
    {
        cshader.SetFloat("deltaTime", Time.deltaTime);
        cshader.SetFloat("time", Time.time);
        cshader.Dispatch(kernel, numBlocks, 1, 1);
        posBuffer.GetData(positions);
        velBuffer.GetData(velocities);
        controller.UpdateInfo(positions, velocities);
    }

    protected override void InitShaderParams()
    {
        cshader.SetInt("entityCount", N);
        cshader.SetVector("speedRange", speedRange);
        cshader.SetFloat("distThreshold", distThreshold);
        cshader.SetVector("boundsX", generator.Bounds_X);
        cshader.SetVector("boundsY", generator.Bounds_Y);
        cshader.SetVector("boundsZ", generator.Bounds_Z);
        cshader.SetVector("restRange", restRange);
    }

    protected override void InitBuffers()
    {
        posBuffer = new(N, sizeof(float) * 3);
        targetPosBuffer = new(N, sizeof(float) * 3);
        velBuffer = new(N, sizeof(float) * 3);
        restBuffer = new(N, sizeof(float));
        cshader.SetBuffer(kernel, "positions", posBuffer);
        cshader.SetBuffer(kernel, "targetPositions", targetPosBuffer);
        cshader.SetBuffer(kernel, "velocities", velBuffer);
        cshader.SetBuffer(kernel, "restTimers", restBuffer);
        posBuffer.SetData(positions);
        targetPosBuffer.SetData(positions);
        velBuffer.SetData(velocities);
        restBuffer.SetData(new float[N]);
    }

    protected override void ReleaseBuffers()
    {
        posBuffer.Release();
        targetPosBuffer.Release();
        velBuffer.Release();
        restBuffer.Release();
    }
}