using System;
using UnityEngine;
using InventorySystem.PlayerItems;

namespace InventorySystem.Presets
{
    [CreateAssetMenu ( fileName = "New Inventory Preset", menuName = "Inventory/Preset" )]
    public class Preset : ScriptableObject
    {
        // Rig
        public PlayerItem [] RigItems = null;

        // Backpack
        public PlayerItemPreset [] BackpackItems = null;

        // Primary weapon
        public WeaponPreset PrimaryWeapon;

        // Secondary weapon
        public WeaponPreset SecondaryWeapon;
    }

    [Serializable]
    public struct WeaponPreset
    {
        // Primary weapon
        public Weapon Weapon;
        // Attachments
        public Barrel Barrel;
        public Sight Sight;
        public Magazine Magazine;
        public Stock Stock;
    }

    [Serializable]
    public struct PlayerItemPreset
    {
        public PlayerItem PlayerItem;
        [Range ( 1, Constants.SLOT_MAX_STACK_SIZE )]
        public int Quantity;
    }
}
