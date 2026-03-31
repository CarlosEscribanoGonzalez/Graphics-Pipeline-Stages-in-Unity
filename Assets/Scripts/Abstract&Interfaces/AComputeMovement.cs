using UnityEngine;

public abstract class AComputeMovement : MonoBehaviour
{
    protected abstract void InitShaderParams();
    protected abstract void InitBuffers();
    protected abstract void ReleaseBuffers();
}
