using UnityEngine;
using InventorySystem.PlayerItems;

namespace InteractableConfiguration
{
    #region Models

    public enum InteractionTypes
    {
        STANDARD,
        REVIVE
    }

    #endregion

    public class InteractableConfig : ScriptableObject
    {
        public virtual int InteractionType { get;  protected set; }
        public bool IsInteractable = true;
        public Color ContextColor = Color.white;
        public float InteractTime = 0;
        public string AccessKey = null;
        public virtual PlayerItem PlayerItem { get; protected set; }
        public virtual int Quantity { get; protected set; }
    }
}
