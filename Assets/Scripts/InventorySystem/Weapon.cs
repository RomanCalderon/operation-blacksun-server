using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/PlayerItems/Weapon", order = 1)]
public class Weapon : PlayerItem
{
    public enum WeaponTypes
    {
        Primary,
        Secondary
    }

    public enum WeaponClasses
    {
        Rifle,
        SMG,
        Shotgun,
        Pistol,
        Sniper
    }

    public WeaponTypes WeaponType;
    public WeaponClasses WeaponClass;
}
