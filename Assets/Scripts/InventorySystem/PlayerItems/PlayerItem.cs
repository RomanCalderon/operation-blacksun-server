using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.PlayerItems
{
    public enum Rarity
    {
        STANDARD,
        ADVANCED,
        EPIC,
        COSMIC
    }

    /// <summary>
    /// The base class for constructing a PlayerItem ScriptableObject.
    /// </summary>
    public class PlayerItem : ScriptableObject
    {
        [Header ( "General" )]
        [Tooltip ( "A unique identifier for this PlayerItem." )]
        public string Id;
        [Tooltip ( "The name of this PlayerItem." )]
        public string Name = string.Empty;
        [Tooltip ( "Item rarity level." )]
        public Rarity Rarity = Rarity.STANDARD;
        [Tooltip ( "The maximum stacking capacity. 1 = no stacking." ), Range ( 1, 256 )]
        public int StackLimit = 1;

        #region Overrides

        public override bool Equals ( object obj )
        {
            return obj is PlayerItem instance && Id == instance.Id;
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }

        public override string ToString ()
        {
            return Name;
        }

        #endregion
    }
}
