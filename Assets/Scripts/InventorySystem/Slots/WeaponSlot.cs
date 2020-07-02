using InventorySystem.PlayerItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InventorySystem.Slots
{
    public class WeaponSlot : Slot
    {
        public override PlayerItem PlayerItem
        {
            get;
            protected set;
        }

        protected override bool IsStackable
        {
            get;
            set;
        }

        #region Constructors

        public WeaponSlot ()
        {
            PlayerItem = null;
            IsStackable = false;
            StackSize = 0;
        }

        public WeaponSlot ( Weapon weapon )
        {
            PlayerItem = weapon;
            IsStackable = false;
            StackSize = 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts <paramref name="weapon"/> into this Slot.
        /// </summary>
        /// <param name="weapon">The Weapon to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem weapon )
        {
            if ( weapon.GetType () != typeof ( Weapon ) )
            {
                return new InsertionResult ( InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( weapon );
            }
            return new InsertionResult ( InsertionResult.Results.SLOT_FULL );
        }

        #region Overrides

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

        #endregion
        #endregion
    }
}
