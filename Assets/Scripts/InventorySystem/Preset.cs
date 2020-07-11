using System;
using UnityEngine;
using InventorySystem.PlayerItems;

namespace InventorySystem.Presets
{
    [CreateAssetMenu ( fileName = "New Inventory Preset", menuName = "Inventory/Preset" )]
    public class Preset : ScriptableObject
    {
        [Header ( "Rig" )]
        public PlayerItemPreset [] RigItems = null;

        [Header ( "Backpack" )]
        public PlayerItemPreset [] BackpackItems = null;

        [Header ( "Primary Weapon" )]
        public WeaponPreset PrimaryWeapon;

        [Header ( "Secondary Weapon" )]
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
