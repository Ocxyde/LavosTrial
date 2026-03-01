// IInteractable.cs
// Interface for all interactable objects in the plug-in-and-out system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Implement this interface to make objects interactable by PlayerController

namespace Code.Lavos.Core
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
