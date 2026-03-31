public interface IInteractable
{
    public bool InteractionBlocked { get; set; }
    public void Interact();
    public void ToggleHover(bool isHovering);
}
