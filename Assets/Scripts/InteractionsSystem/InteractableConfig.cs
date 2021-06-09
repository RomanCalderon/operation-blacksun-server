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
        public virtual int InteractionType { get; protected set; }

        public bool IsInteractable = true;

        public Color ContextColor = Color.white;

        [field: SerializeField]
        public virtual float InteractTime { get; protected set; } = 0;

        public string AccessKey = null;

        [field: SerializeField]
        public virtual PlayerItem PlayerItem { get; protected set; }

        [field: SerializeField]
        public virtual int Quantity { get; protected set; }
    }
}
