using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference lookActionReference;
    [SerializeField] private float sensitivity = 5;
    [Range(0, 90)][SerializeField] private float maxVerticalRotation = 85f;
    private Transform playerTransform;
    private float vertRotation = 0;

    private void Awake()
    {
        playerTransform = GetComponentInParent<PlayerMovement>().transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 rotation = lookActionReference.action.ReadValue<Vector2>() * sensitivity;
        playerTransform.Rotate(Vector3.up * rotation.x, Space.Self);
        vertRotation -= rotation.y;
        vertRotation = Mathf.Clamp(vertRotation, -maxVerticalRotation, maxVerticalRotation);
        transform.localRotation = Quaternion.Euler(vertRotation, 0, 0);
    }
}
