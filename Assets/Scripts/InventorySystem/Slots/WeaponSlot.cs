using System;
using System.Collections.Generic;
using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    [Serializable]
    public class WeaponSlot : Slot
    {
        #region Constructors

        public WeaponSlot ( string id )
        {
            Id = id;
            PlayerItem = null;
        }

        public WeaponSlot ( string id, Weapon weapon )
        {
            Id = id;
            PlayerItem = weapon;
            StackSize = 1;
        }

        #endregion

        /// <summary>
        /// Inserts <paramref name="weapon"/> into this Slot.
        /// </summary>
        /// <param name="weapon">The Weapon to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem weapon )
        {
            if ( !IsValidPlayerItem ( weapon ) )
            {
                return new InsertionResult ( weapon, InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                PlayerItem = weapon;
                StackSize = 1;
                return new InsertionResult ( weapon, InsertionResult.Results.SUCCESS );
            }
            return new InsertionResult ( weapon, InsertionResult.Results.SLOT_FULL );
        }

        public override InsertionResult Insert ( PlayerItem playerItem, int quantity )
        {
            return Insert ( playerItem );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            return playerItem != null && playerItem is Weapon;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Weapon Slot ({Id}) - {PlayerItem.Name}";
            }
            else
            {
                return $"Weapon Slot ({Id}) - Empty";
            }
        }

        public override void OnValidate ()
        {
            Name = ToString ();
        }
        
        public override bool Equals ( object obj )
        {
            return Id == ( obj as WeaponSlot ).Id && PlayerItem == ( obj as WeaponSlot ).PlayerItem;
        }

        public override int GetHashCode ()
        {
            int hashCode = 1797897742;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode ( Name );
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode ( Id );
            hashCode = hashCode * -1521134295 + EqualityComparer<PlayerItem>.Default.GetHashCode ( PlayerItem );
            hashCode = hashCode * -1521134295 + IsStackable.GetHashCode ();
            hashCode = hashCode * -1521134295 + StackSize.GetHashCode ();
            return hashCode;
        }
    }
}
