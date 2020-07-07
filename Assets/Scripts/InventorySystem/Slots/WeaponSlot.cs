using System;
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
                return base.Insert ( weapon );
            }
            return new InsertionResult ( weapon, InsertionResult.Results.SLOT_FULL );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem is Weapon;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Weapon Slot - {PlayerItem.Name}";
            }
            else
            {
                return "Weapon Slot - Empty";
            }
        }

        public override void OnValidate ()
        {
            Name = ToString ();
        }
    }
}
