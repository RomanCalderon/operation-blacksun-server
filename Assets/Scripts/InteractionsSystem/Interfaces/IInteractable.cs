internal interface IInteractable
{
    bool IsInteractable { get; set; }

    bool IsInteracting { get; }

    int ClientId { get; }

    string AccessKey { get; set; }

    void Initialize ( bool isInteractable = true, string accessKey = null );

    void StartHover ();

    void StartInteract ( int clientId, string accessKey = null );

    void StartInteract ( int clientId, string [] accessKeys );

    void StopInteract ();

    void StopHover ();
}
