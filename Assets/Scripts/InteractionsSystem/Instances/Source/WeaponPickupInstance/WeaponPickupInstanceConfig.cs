using UnityEngine;
using InventorySystem.PlayerItems;

namespace InteractableConfiguration
{
    [CreateAssetMenu ( fileName = "New Weapon Pickup Instance Config", menuName = "Interactable/Weapon Pickup Instance Config" )]
    public class WeaponPickupInstanceConfig : PickupInstanceConfig
    {
        public override float InteractTime { get => 0.2f; }
        public override PlayerItem PlayerItem { get => base.PlayerItem as Weapon; protected set => base.PlayerItem = value; }
        public override int Quantity { get => 1; }
    }
}
