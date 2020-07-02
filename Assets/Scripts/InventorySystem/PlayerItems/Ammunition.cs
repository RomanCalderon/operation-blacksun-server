using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    /// <summary>
    /// A PlayerItem which defines the properties of an ammunition type.
    /// </summary>
    [CreateAssetMenu ( fileName = "New Ammunition", menuName = "PlayerItems/Ammunition", order = 3 )]
    public class Ammunition : PlayerItem
    {
        public enum Calibers
        {
            /// <summary>SMG, Pistol</summary>
            Wilson_9MM,
            /// <summary>SMG, Pistol</summary>
            ACP_Ultra,
            /// <summary>Rifle</summary>
            NATO_556,
            /// <summary>Rifle</summary>
            AAC,
            /// <summary>Shotgun</summary>
            G12,
            /// <summary>Shotgun</summary>
            Boar_75,
            /// <summary>Sniper</summary>
            C3,
            /// <summary>Sniper</summary>
            Vanquisher
        }

        [Header ( "Ammo" )]
        [Tooltip ( "This ammunition's caliber." )]
        public Calibers Caliber;
    }
}