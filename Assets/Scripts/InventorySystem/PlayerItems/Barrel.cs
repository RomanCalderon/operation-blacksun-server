using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    [CreateAssetMenu ( fileName = "New Barrel", menuName = "PlayerItems/Attachments/Barrel", order = 1 )]
    public class Barrel : Attachment
    {
        public enum BarrelTypes
        {
            /// <summary>
            /// Reduces muzzle rise.
            /// </summary>
            Compensator,
            /// <summary>
            /// Greatly reduces gunshot audio level.
            /// </summary>
            Silencer
        }
        
        [Header ( "Barrel" )]
        public BarrelTypes BarrelType;
    }
}
