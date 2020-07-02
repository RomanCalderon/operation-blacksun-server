using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    [CreateAssetMenu ( fileName = "New Sight", menuName = "PlayerItems/Attachments/Sight", order = 3 )]
    public class Sight : Attachment
    {
        [Header ( "Sight" )]
        [Tooltip ( "The amount of zoom/magnifaction this sight provides." ), Range ( 1, 8 )]
        public float SightZoomStrength = 1;
        [Tooltip ( "Increases the player's ADS zoom by this much." ), Range ( 1, 2 )]
        public float PlayerZoomModifier = 1;
    }
}
