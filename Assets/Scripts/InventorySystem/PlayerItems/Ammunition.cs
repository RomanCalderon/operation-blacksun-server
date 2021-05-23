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
            /// <summary>Rifle</summary>
            NATO_556,
            /// <summary>Rifle</summary>
            AAC,
            /// <summary>SMG, Pistol</summary>
            ACP_Ultra,
            /// <summary>SMG, Pistol</summary>
            Wilson_9MM,
            /// <summary>Shotgun</summary>
            GAUGE_12,
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