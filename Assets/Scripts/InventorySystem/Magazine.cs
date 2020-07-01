using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.InventorySystem
{
    [CreateAssetMenu ( fileName = "New Magazine", menuName = "PlayerItems/Attachments/Magazine", order = 1 )]
    public class Magazine : Attachment
    {
        [Header ( "Magazine" ), Min ( 0 )]
        [Tooltip ( "The maximum amount of rounds this magazine can contain." )]
        public int AmmoCapacity = 5;
        [Tooltip ( "The ammo caliber that this magazine is compatible with." )]
        public Ammunition.Calibers CompatibleAmmoCaliber;
    }
}
