using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    public class SightSlot : AttachmentSlot
    {
        #region Constructors

        public SightSlot ()
        {
            PlayerItem = null;
        }

        public SightSlot ( Sight sight )
        {
            PlayerItem = sight;
        }

        #endregion

        /// <summary>
        /// Inserts <paramref name="sight"/> into this Slot.
        /// </summary>
        /// <param name="sight">The Sight to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem sight )
        {
            if ( !IsValidPlayerItem ( sight ) )
            {
                return new InsertionResult ( sight, InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( sight );
            }
            return new InsertionResult ( sight, InsertionResult.Results.SLOT_FULL );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem is Sight;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Attachment Slot (Sight) - {PlayerItem.Name}";
            }
            else
            {
                return "Attachment Slot (Sight) - Empty";
            }
        }
    }
}
