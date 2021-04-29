internal interface IInteractable
{
    bool IsInteractable { get; set; }

    bool IsInteracting { get; }

    float InteractTime { get; set; }

    string AccessKey { get; set; }

    int ClientId { get; }

    void Initialize ( bool isInteractable = true, float interactTime = 0f, string accessKey = null );

    void StartHover ();

    void StartInteract ( int clientId, string accessKey = null );

    void StartInteract ( int clientId, string [] accessKeys );

    void StopInteract ();

    void StopHover ();
}
