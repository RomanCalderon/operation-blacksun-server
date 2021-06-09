using UnityEngine;
using InventorySystem.PlayerItems;

namespace InteractableConfiguration
{
    [CreateAssetMenu ( fileName = "New Pickup Instance Config", menuName = "Interactable/Attachment Pickup Instance Config" )]
    public class AttachmentPickupInstanceConfig : PickupInstanceConfig
    {
        public override int Quantity { get => 1; }
    }
}
