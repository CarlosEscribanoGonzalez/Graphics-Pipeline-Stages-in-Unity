using System;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [SerializeField] private float stepDist = 1.0f;
    private Vector3 lastRegisteredPos;
    public event Action<Vector3> OnStepPerformed;

    private void Awake()
    {
        GetComponent<PlayerMovement>().OnPositionUpdated += HandlePositionUpdate;
        lastRegisteredPos = transform.position;
    }

    private void OnDestroy()
    {
        GetComponent<PlayerMovement>().OnPositionUpdated -= HandlePositionUpdate;
    }

    private void HandlePositionUpdate(Vector3 newPos)
    {
        if (Vector3.Distance(lastRegisteredPos, newPos) >= stepDist)
        {
            lastRegisteredPos = newPos;
            OnStepPerformed?.Invoke(newPos);
        }
    }
}
