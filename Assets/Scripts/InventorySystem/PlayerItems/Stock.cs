using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    [CreateAssetMenu ( fileName = "New Stock", menuName = "PlayerItems/Attachments/Stock", order = 4 )]
    public class Stock : Attachment
    {
        [Header ( "Stock" )]
        [Tooltip ( "Reduces the recoil by this amount. 0 = no reduction. 1 = full reduction." ), Range ( 0, 1 )]
        public float RecoilReductionModifier = 0.0f;
    }
}
