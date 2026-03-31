using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class InteractionController : MonoBehaviour
{
    [SerializeField] private InputActionReference interactActionReference;
    [SerializeField] private float maxDistance = 2;
    private IInteractable hoveredInteractable;

    private void Awake() => interactActionReference.action.started += Interact;

    private void OnDestroy() => interactActionReference.action.started -= Interact;

    private void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable) && 
                !interactable.InteractionBlocked)
            {
                if (hoveredInteractable == interactable) return;
                hoveredInteractable?.ToggleHover(false);
                hoveredInteractable = interactable;
                hoveredInteractable.ToggleHover(true);
                return;
            }
        }
        hoveredInteractable?.ToggleHover(false);
        hoveredInteractable = null;
    }

    private void Interact(InputAction.CallbackContext _) => hoveredInteractable?.Interact();
}
