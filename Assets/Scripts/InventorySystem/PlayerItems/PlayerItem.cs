using System;
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
        [Tooltip ( "The maximum stacking capacity. 1 = no stacking." ), Range ( 1, 256 )]
        public int StackLimit = 1;

        public override bool Equals ( object other )
        {
            return Id == ( ( PlayerItem ) other ).Id;
        }

        public override int GetHashCode ()
        {
            int hashCode = -1488479220;
            hashCode = hashCode * -1521134295 + base.GetHashCode ();
            hashCode = hashCode * -1521134295 + Id.GetHashCode ();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode ( Name );
            return hashCode;
        }

        public override string ToString ()
        {
            return Name;
        }
    }
}
