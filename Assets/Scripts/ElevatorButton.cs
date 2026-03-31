using UnityEngine;

public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject hoverInfo;
    private Elevator elevator;
    public bool InteractionBlocked { get; set; }

    private void Start()
    {
        hoverInfo.SetActive(false);
        elevator = GetComponentInParent<Elevator>();
        elevator.OnDestinationReached += RestoreInteraction;
    }

    private void OnDestroy() => elevator.OnDestinationReached -= RestoreInteraction;

    public void ToggleHover(bool isHovering)
    {
        hoverInfo.SetActive(isHovering);
    }

    public void Interact()
    {
        InteractionBlocked = true;
        elevator.TogglePos();
    }

    private void RestoreInteraction() => InteractionBlocked = false;
}
