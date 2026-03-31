using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform player;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
    }

    private void Update()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.forward = new(transform.forward.x, dir.y, transform.forward.z);
    }
}
