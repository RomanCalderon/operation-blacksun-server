using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    [CreateAssetMenu ( fileName = "New Magazine", menuName = "PlayerItems/Attachments/Magazine", order = 2 )]
    public class Magazine : Attachment
    {
        [Header ( "Magazine" )]
        [Tooltip ( "The maximum amount of rounds this magazine can contain." ), Min ( 0 )]
        public int AmmoCapacity = 18;
        [Tooltip ( "The ammo caliber that this magazine is compatible with." )]
        public Ammunition.Calibers CompatibleAmmoCaliber;
    }
}
