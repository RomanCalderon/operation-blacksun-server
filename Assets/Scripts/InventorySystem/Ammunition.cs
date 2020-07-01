using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammunition", menuName = "ScriptableObjects/PlayerItems/Ammunition", order = 3)]
class Ammunition : PlayerItem
{
    public enum AmmunitionTypes
    {
        /// <summary>SMG, Pistol</summary>
        B_9MM,
        /// <summary>SMG, Pistol</summary>
        C_ACP,
        /// <summary>Rifle</summary>
        A_556,
        /// <summary>Rifle</summary>
        E_AAC,
        /// <summary>Shotgun</summary>
        D_Boar75,
        /// <summary>Sniper</summary>
        F_Carbon3,
        /// <summary>Sniper</summary>
        G_Vanquisher 
    }

    public AmmunitionTypes AmmunitionType;
}
