using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    /// <summary>
    /// The base class for constructing a PlayerItem ScriptableObject.
    /// </summary>
    public class PlayerItem : ScriptableObject
    {
        [Header ( "General" )]
        [Tooltip ( "A unique identifier for this PlayerItem." )]
        public int Id;
        [Tooltip ( "The name of this PlayerItem." )]
        public string Name = string.Empty;
        [Tooltip ( "The maximum stacking capacity." ), Range ( 1, 256 )]
        public int StackSize = 1;
    }
}
