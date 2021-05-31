using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    /// <summary>
    /// A PlayerItem which defines the properties of a weapon.
    /// </summary>
    [CreateAssetMenu ( fileName = "New Weapon", menuName = "PlayerItems/Weapon", order = 1 )]
    public class Weapon : PlayerItem
    {
        public enum WeaponTypes
        {
            /// <summary>
            /// Only compatible in primary weapon slots.
            /// </summary>
            Primary,
            /// <summary>
            /// Only compatible in secondary weapon slots.
            /// </summary>
            Secondary,
            /// <summary>
            /// Compatible in any weapon slot.
            /// </summary>
            Any
        }

        public enum WeaponClasses
        {
            Rifle,
            SMG,
            Shotgun,
            Pistol,
            Sniper
        }

        public enum FireModes
        {
            SemiAuto,
            FullAuto
        }

        [Header ( "Weapon" )]
        [Tooltip ( "Defines which type of weapon slot this weapon is compatible with." )]
        public WeaponTypes WeaponType;
        [Tooltip ( "The class this weapon falls under." )]
        public WeaponClasses WeaponClass;
        [Tooltip ( "The type of ammo this weapon is compatible with." )]
        public Ammunition.Calibers Caliber;
        [Tooltip ( "The base amount of damage this weapon inflicts per shot." ), Min ( 0 )]
        public int BaseDamage = 20;
        [Tooltip ( "The weapon's rate of fire in seconds." ), Min ( 0 )]
        public float FireRate = 0.2f;
        [Tooltip ( "The weapon's firing mode." )]
        public FireModes FireMode = FireModes.SemiAuto;

        [Header ( "Compatible Attachments" )]
        [Tooltip ( "List of every compatible Barrel for this Weapon." )]
        public Barrel [] m_compatibleBarrels = null;
        [Tooltip ( "List of every compatible Sight for this Weapon." )]
        public Sight [] m_compatibleSights = null;
        [Tooltip ( "List of every compatible Stock for this Weapon." )]
        public Stock [] m_compatibleStocks = null;

        public bool IsCompatibleAttachment ( Attachment attachment )
        {
            return attachment switch
            {
                Barrel barrel => m_compatibleBarrels.Any ( b => b.Id == barrel.Id ),
                Sight sight => m_compatibleSights.Any ( s => s.Id == sight.Id ),
                Magazine magazine => magazine.CompatibleAmmoCaliber == Caliber,
                Stock stock => m_compatibleStocks.Any ( s => s.Id == stock.Id ),
                _ => false,
            };
        }
    }
}