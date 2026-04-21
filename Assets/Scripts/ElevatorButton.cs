using UnityEngine;

public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject hoverInfo;
    private Elevator elevator;
    public bool InteractionBlocked { get; set; }
    private AudioManager audioManager;
    [SerializeField] private AudioClip buttonClip;

    private void Start()
    {
        hoverInfo.SetActive(false);
        elevator = GetComponentInParent<Elevator>();
        elevator.OnDestinationReached += RestoreInteraction;
        audioManager = FindFirstObjectByType<AudioManager>();
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
        if (buttonClip != null)
        {
            audioManager.PlaySFX(buttonClip);
        }
    }
        
    private void RestoreInteraction() => InteractionBlocked = false;
}
