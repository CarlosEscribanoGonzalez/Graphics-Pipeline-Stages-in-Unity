using UnityEngine;
using System.Collections;
using System;

public class Elevator : MonoBehaviour
{
    [SerializeField] private Transform platform;
    [SerializeField] private Transform destination;
    [SerializeField] private Collider doorCollider;
    [SerializeField] private float speed;
    private PlayerMovement playerMovement;
    private Vector3 startPosition;
    private ElevatorPosition state = ElevatorPosition.Up;
    public event Action OnDestinationReached;

    void Awake()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        startPosition = platform.position;
        doorCollider.enabled = false;
    }

    public void TogglePos()
    {
        state = state == ElevatorPosition.Up ? ElevatorPosition.Down : ElevatorPosition.Up;
        Vector3 targetPos = state == ElevatorPosition.Up ? startPosition : destination.position;
        StopAllCoroutines();
        StartCoroutine(MovePlatformCoroutine(targetPos));
    }

    IEnumerator MovePlatformCoroutine(Vector3 targetPos)
    {
        doorCollider.enabled = true;
        playerMovement.enabled = false;
        playerMovement.transform.SetParent(platform);
        while (platform.position != targetPos)
        {
            platform.position = 
                Vector3.MoveTowards(platform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        doorCollider.enabled = false;
        playerMovement.enabled = true;
        playerMovement.transform.SetParent(null);
        OnDestinationReached?.Invoke();
    }
}

public enum ElevatorPosition
{
    Up, 
    Down
}
