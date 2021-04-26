internal interface IInteractable
{
    bool IsInteractable { get; set; }

    bool IsInteracting { get; }

    float MinRange { get; }

    string AccessKey { get; set; }

    void StartHover ();

    void StartInteract ( string accessKey );

    void StartInteract ( string [] accessKeys = null );

    void StopInteract ();

    void EndHover ();
}
