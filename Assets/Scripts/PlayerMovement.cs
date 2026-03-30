using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private float speed = 5;
    private Vector3 movement = Vector3.zero;
    private CharacterController controller;
    public event Action<Vector3> OnPositionUpdated;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        Vector2 input = (moveActionReference.action.ReadValue<Vector2>()).normalized * speed;
        float gravityPull = controller.isGrounded ? 0 : movement.y + Physics.gravity.y;
        movement = transform.TransformDirection(new(input.x, gravityPull, input.y)) * Time.deltaTime;
        controller.Move(movement);
        OnPositionUpdated?.Invoke(transform.position);
    }
}
