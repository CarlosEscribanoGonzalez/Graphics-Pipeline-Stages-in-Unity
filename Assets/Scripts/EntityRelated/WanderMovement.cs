using UnityEngine;

[RequireComponent(typeof(EntityGenerator))]
[RequireComponent(typeof(EntityGroupController))]
public class WanderMovement : AComputeMovement
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private Vector2 speedRange = new(1, 10);
    [SerializeField] private float distThreshold = 0.1f;
    [SerializeField] private Vector2 restRange = new(0.5f, 3);
    private int kernel;
    private int groups;
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
        shader = Instantiate(shader);
        generator = GetComponent<EntityGenerator>();
        controller = GetComponent<EntityGroupController>();
        kernel = shader.FindKernel("CSMain");
        groups = Mathf.CeilToInt(N / 256f);
        positions = generator.GetPositions();
        velocities = new Vector3[N];
        InitShaderParams();
        InitBuffers();
    }

    private void OnDestroy() => ReleaseBuffers();

    private void Update()
    {
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloat("time", Time.time);
        shader.Dispatch(kernel, groups, 1, 1);
        posBuffer.GetData(positions);
        velBuffer.GetData(velocities);
        controller.UpdateInfo(positions, velocities);
    }

    protected override void InitShaderParams()
    {
        shader.SetInt("entityCount", N);
        shader.SetVector("speedRange", speedRange);
        shader.SetFloat("distThreshold", distThreshold);
        shader.SetVector("boundsX", generator.Bounds_X);
        shader.SetVector("boundsY", generator.Bounds_Y);
        shader.SetVector("boundsZ", generator.Bounds_Z);
        shader.SetVector("restRange", restRange);
    }

    protected override void InitBuffers()
    {
        posBuffer = new(N, sizeof(float) * 3);
        targetPosBuffer = new(N, sizeof(float) * 3);
        velBuffer = new(N, sizeof(float) * 3);
        restBuffer = new(N, sizeof(float));
        shader.SetBuffer(kernel, "positions", posBuffer);
        shader.SetBuffer(kernel, "targetPositions", targetPosBuffer);
        shader.SetBuffer(kernel, "velocities", velBuffer);
        shader.SetBuffer(kernel, "restTimers", restBuffer);
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