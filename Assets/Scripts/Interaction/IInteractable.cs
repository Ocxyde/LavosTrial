namespace Code.Lavos
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract(PlayerController player);
        void OnInteract(PlayerController player);
        void OnHighlightEnter(PlayerController player);
        void OnHighlightExit(PlayerController player);
    }
}
