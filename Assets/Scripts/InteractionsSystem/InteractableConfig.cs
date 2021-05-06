using UnityEngine;

public class InteractableConfig : ScriptableObject
{
    public enum InteractionTypes
    {
        STANDARD,
        REVIVE
    }

    [Header("Interactable Config")]
    public InteractionTypes InteractionType = 0;
    public bool IsInteractable = true;
    public Color ContextColor = Color.white;
    public float InteractTime = 0;
    public string AccessKey = null;
}
