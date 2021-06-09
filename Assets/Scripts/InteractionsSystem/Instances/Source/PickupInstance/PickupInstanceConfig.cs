using UnityEngine;
using InventorySystem.PlayerItems;

namespace InteractableConfiguration
{
    [CreateAssetMenu ( fileName = "New Pickup Instance Config", menuName = "Interactable/Pickup Instance Config" )]
    public class PickupInstanceConfig : InteractableConfig
    {
        public override float InteractTime { get => base.InteractTime; protected set => base.InteractTime = value; }
        public override int InteractionType { get => ( int ) InteractionTypes.STANDARD; }
        public override PlayerItem PlayerItem { get => base.PlayerItem; protected set => base.PlayerItem = value; }
        public override int Quantity { get => base.Quantity; protected set => base.Quantity = value; }

        public void Init ( PlayerItem playerItem, int quantity = 1 )
        {
            PlayerItem = playerItem;
            Quantity = quantity;
        }
    }
}