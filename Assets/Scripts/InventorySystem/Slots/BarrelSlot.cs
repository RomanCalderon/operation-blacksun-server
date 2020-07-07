using System;
using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    [Serializable]
    public class BarrelSlot : AttachmentSlot
    {
        #region Constructors

        public BarrelSlot ( string id )
        {
            Id = id;
            PlayerItem = null;
        }

        public BarrelSlot ( string id, Barrel barrel )
        {
            Id = id;
            PlayerItem = barrel;
        }

        #endregion

        /// <summary>
        /// Inserts <paramref name="barrel"/> into this Slot.
        /// </summary>
        /// <param name="barrel">The Barrel to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem barrel )
        {
            if ( !IsValidPlayerItem ( barrel ) )
            {
                return new InsertionResult ( barrel, InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( barrel );
            }
            return new InsertionResult ( barrel, InsertionResult.Results.SLOT_FULL );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem is Barrel;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Attachment Slot (Barrel) - {PlayerItem.Name}";
            }
            else
            {
                return "Attachment Slot (Barrel) - Empty";
            }
        }
    }
}
