using UnityEngine;
using InventorySystem.PlayerItems;

namespace InteractableConfiguration
{
    [CreateAssetMenu ( fileName = "New Pickup Instance Config", menuName = "Interactable/Weapon Pickup Instance Config" )]
    public class WeaponPickupInstanceConfig : PickupInstanceConfig
    {
        public override int Quantity { get => 1; }
    }
}
