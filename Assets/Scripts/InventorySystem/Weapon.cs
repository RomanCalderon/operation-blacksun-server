using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A PlayerItem which defines the properties of a weapon.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/PlayerItems/Weapon", order = 1)]
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

    /// <summary>
    /// Defines which type of weapon slot this weapon is compatible with.
    /// </summary>
    public WeaponTypes WeaponType;
    /// <summary>
    /// The class this weapon falls under.
    /// </summary>
    public WeaponClasses WeaponClass;
}
