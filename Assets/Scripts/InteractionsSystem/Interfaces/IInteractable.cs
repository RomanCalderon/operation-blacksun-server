using UnityEngine;

public interface IInteractable
{
    int InteractionType { get; }

    bool IsInteractable { get; }

    string InteractionContext { get; }

    Color InteractionColor { get; }

    bool IsInteracting { get; }

    float InteractTime { get; }

    string AccessKey { get; }

    int ClientId { get; }

    void Initialize ( int interactionType, bool isInteractable, string interactionContext, Color interactionLabelColor, float interactTime = 0f, string accessKey = null );

    void StartHover ();

    void StartInteract ( int clientId, string accessKey = null );

    void StartInteract ( int clientId, string [] accessKeys );

    void StopInteract ();

    void StopHover ();
}
